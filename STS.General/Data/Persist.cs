using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using STS.General.Persist;
using STS.General.Compression;
using STS.General.Extensions;
using STS.General.Comparers;

namespace STS.General.Data
{
    public class Persist<T> : IPersist<T>
    {
        public readonly Action<BinaryWriter, T> write;
        public readonly Func<BinaryReader, T> read;

        public readonly Type Type;
        public readonly Func<Type, MemberInfo, int> MembersOrder;
        public readonly AllowNull AllowNull;

        public Persist(Func<Type, MemberInfo, int> membersOrder = null, AllowNull allowNull = AllowNull.None)
        {
            Type = typeof(T);
            MembersOrder = membersOrder;
            AllowNull = allowNull;

            write = CreateWriteMethod().Compile();
            read = CreateReadMethod().Compile();
        }

        public Expression<Action<BinaryWriter, T>> CreateWriteMethod()
        {
            var writer = Expression.Parameter(typeof(BinaryWriter));
            var item = Expression.Parameter(Type);

            return Expression.Lambda<Action<BinaryWriter, T>>(PersistHelper.CreateWriteBody(item, writer, MembersOrder, AllowNull), writer, item);
        }

        public Expression<Func<BinaryReader, T>> CreateReadMethod()
        {
            var reader = Expression.Parameter(typeof(BinaryReader), "reader");

            return Expression.Lambda<Func<BinaryReader, T>>(PersistHelper.CreateReadBody(reader, Type, MembersOrder, AllowNull), reader);
        }

        public void Write(BinaryWriter writer, T item)
        {
            write(writer, item);
        }

        public T Read(BinaryReader reader)
        {
            return read(reader);
        }
    }

    public class Persist : IPersist<object>
    {
        public readonly Action<BinaryWriter, object> write;
        public readonly Func<BinaryReader, object> read;

        public readonly Type Type;
        public readonly Func<Type, MemberInfo, int> MembersOrder;
        public readonly AllowNull AllowNull;

        public Persist(Type type, Func<Type, MemberInfo, int> membersOrder = null, AllowNull allowNull = AllowNull.None)
        {
            Type = type;
            MembersOrder = membersOrder;
            AllowNull = allowNull;

            write = CreateWriteMethod().Compile();
            read = CreateReadMethod().Compile();
        }

        public Expression<Action<BinaryWriter, object>> CreateWriteMethod()
        {
            var writer = Expression.Parameter(typeof(BinaryWriter));
            var item = Expression.Parameter(typeof(object));

            return Expression.Lambda<Action<BinaryWriter, object>>(PersistHelper.CreateWriteBody(Expression.Convert(item, Type), writer, MembersOrder, AllowNull), writer, item);
        }

        public Expression<Func<BinaryReader, object>> CreateReadMethod()
        {
            var reader = Expression.Parameter(typeof(BinaryReader), "reader");
            var body = PersistHelper.CreateReadBody(reader, Type, MembersOrder, AllowNull);

            return Expression.Lambda<Func<BinaryReader, object>>(Expression.Convert(body, typeof(object)), reader);
        }

        #region IPersist<object> Members

        public void Write(BinaryWriter writer, object item)
        {
            write(writer, item);
        }

        public object Read(BinaryReader reader)
        {
            return read(reader);
        }

        #endregion
    }

    public enum AllowNull : byte
    {
        /// <summary>
        /// Instance and all instance members and their members can be null.
        /// </summary>
        All,

        /// <summary>
        /// Instance can not be null, but instance members and their members can be null.
        /// </summary>
        OnlyMembers,

        /// <summary>
        /// Instance and instance members and their members cannot be null (the default and most space efficient variant).
        /// </summary>
        None
    }

    public static class PersistHelper
    {
        public static Expression CreateWriteBody(Expression item, Expression writer, Func<Type, MemberInfo, int> membersOrder, AllowNull allowNull)
        {
            return BuildWrite(item, writer, membersOrder, allowNull, 0);
        }

        private static Expression BuildWrite(Expression item, Expression writer, Func<Type, MemberInfo, int> membersOrder, AllowNull allowNull, int depth)
        {
            var type = item.Type;
            bool canBeNull = CanBeNull(type, allowNull, depth);

            if (type == typeof(Guid))
                return GetWriteCommand(writer, Expression.Call(item, type.GetMethod("ToByteArray")), canBeNull);

            if (type.IsEnum)
                return GetWriteCommand(writer, Expression.Convert(item, item.Type.GetEnumUnderlyingType()), canBeNull);

            if (DataType.IsPrimitiveType(type))
                return GetWriteCommand(writer, item, canBeNull);

            if (type.IsKeyValuePair())
            {
                return Expression.Block(
                    BuildWriteAssignedOrCurrent(Expression.PropertyOrField(item, "Key"), writer, membersOrder, allowNull, depth + 1),
                    BuildWriteAssignedOrCurrent(Expression.PropertyOrField(item, "Value"), writer, membersOrder, allowNull, depth + 1)
                 );
            }

            if (type.IsArray || type.IsList())
            {
                if (!canBeNull)
                    return Expression.Block(Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.Convert(type.IsArray ? (Expression)Expression.ArrayLength(item) : Expression.Property(item, "Count"), typeof(ulong))),
                        item.For(i =>
                           BuildWriteAssignedOrCurrent(type.IsArray ? Expression.ArrayAccess(item, i) : item.This(i), writer, membersOrder, allowNull, depth + 1),
                           Expression.Label()));

                return Expression.IfThenElse(Expression.NotEqual(item, Expression.Constant(null)),
                    Expression.Block(
                        Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(true)),
                        Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.Convert(type.IsArray ? (Expression)Expression.ArrayLength(item) : Expression.Property(item, "Count"), typeof(ulong))),
                        item.For(i =>
                        BuildWriteAssignedOrCurrent(type.IsArray ? Expression.ArrayAccess(item, i) : item.This(i), writer, membersOrder, allowNull, depth + 1),
                        Expression.Label())),
                    Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(false))
                    );
            }

            if (type.IsDictionary())
            {
                var keyType = type.GetGenericArguments()[0];

                if (!IsSupportDictionaryKeyType(keyType))
                    throw new NotSupportedException(String.Format("Dictionarty<{0}, TValue>", keyType.ToString()));

                if (!canBeNull)
                    return Expression.Block(
                            Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.Convert(Expression.Property(item, "Count"), typeof(ulong))),
                            item.ForEach(current =>
                            {
                                var kv = Expression.Variable(current.Type);

                                return Expression.Block(new ParameterExpression[] { kv },
                                    Expression.Assign(kv, current),
                                    BuildWriteAssignedOrCurrent(Expression.PropertyOrField(kv, "Key"), writer, membersOrder, allowNull, depth + 1),
                                    BuildWriteAssignedOrCurrent(Expression.PropertyOrField(kv, "Value"), writer, membersOrder, allowNull, depth + 1)
                                );
                            }, Expression.Label())
                           );

                return Expression.IfThenElse(Expression.NotEqual(item, Expression.Constant(null)),
                    Expression.Block(
                        Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(true)),
                        Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.Convert(Expression.Property(item, "Count"), typeof(ulong))),
                        item.ForEach(current =>
                        {
                            var kv = Expression.Variable(current.Type);

                            return Expression.Block(new ParameterExpression[] { kv },
                                Expression.Assign(kv, current),
                                BuildWriteAssignedOrCurrent(Expression.PropertyOrField(kv, "Key"), writer, membersOrder, allowNull, depth + 1),
                                BuildWriteAssignedOrCurrent(Expression.PropertyOrField(kv, "Value"), writer, membersOrder, allowNull, depth + 1)
                            );
                        }, Expression.Label())),
                      Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(false))
                    );
            }

            if (type.IsNullable())
            {
                if (!canBeNull)
                    return BuildWrite(Expression.PropertyOrField(item, "Value"), writer, membersOrder, allowNull, depth + 1);

                return Expression.Block(
                        Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.PropertyOrField(item, "HasValue")),
                        Expression.IfThen(Expression.PropertyOrField(item, "HasValue"), 
                            BuildWrite(Expression.PropertyOrField(item, "Value"), writer, membersOrder, allowNull, depth + 1)));
            }

            if (type.IsClass || type.IsStruct())
            {
                List<ParameterExpression> variables = new List<ParameterExpression>();
                List<Expression> list = new List<Expression>();

                if (canBeNull)
                    list.Add(Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(true)));

                var members = DataTypeUtils.GetPublicMembers(type, membersOrder).ToList();

                for (int i = 0; i < members.Count; i++)
                {
                    var member = members[i];
                    var memberType = member.GetUnderlyingType();

                    if (DataType.IsPrimitiveType(memberType) || memberType.IsKeyValuePair())
                        list.Add(BuildWrite(Expression.PropertyOrField(item, member.Name), writer, membersOrder, allowNull, depth + 1));
                    else
                    {
                        var @var = Expression.Variable(member.GetUnderlyingType());
                        variables.Add(var);
                        list.Add(Expression.Assign(var, Expression.PropertyOrField(item, member.Name)));
                        list.Add(BuildWrite(var, writer, membersOrder, allowNull, depth + 1));
                    }
                }

                if (!canBeNull)
                    return Expression.Block(variables, list);

                return Expression.IfThenElse(Expression.NotEqual(item, Expression.Constant(null)),
                        Expression.Block(variables, list),
                        Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(false))
                    );
            }

            throw new NotSupportedException(item.Type.ToString());
        }

        private static Expression BuildWriteAssignedOrCurrent(Expression item, Expression writer, Func<Type, MemberInfo, int> membersOrder, AllowNull allowNull, int depth)
        {
            var type = item.Type;

            if (type == typeof(Guid) || type.IsEnum || DataType.IsPrimitiveType(type))
                return BuildWrite(item, writer, membersOrder, allowNull, depth);

            ParameterExpression @var = Expression.Variable(type);

            return Expression.Block(new ParameterExpression[] { @var },
                Expression.Assign(@var, item),
                BuildWrite(@var, writer, membersOrder, allowNull, depth));
        }

        private static Expression GetWriteCommand(Expression writer, Expression item, bool canBeNull)
        {
            Debug.Assert(DataType.IsPrimitiveType(item.Type));

            Type type = item.Type;

            if (type == typeof(Boolean) ||
                type == typeof(Char) ||
                type == typeof(SByte) ||
                type == typeof(Byte) ||
                type == typeof(Int16) ||
                type == typeof(Int32) ||
                type == typeof(UInt32) ||
                type == typeof(UInt16) ||
                type == typeof(Int64) ||
                type == typeof(UInt64) ||
                type == typeof(Single) ||
                type == typeof(Double) ||
                type == typeof(Decimal))
            {
                MethodInfo writeAny = typeof(BinaryWriter).GetMethod("Write", new Type[] { type });
                return Expression.Call(writer, writeAny, item);

                //writer.Write(item);
            }
            else if (type == typeof(DateTime) || type == typeof(TimeSpan))
            {
                MethodInfo writeLong = typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(long) });
                return Expression.Call(writer, writeLong, Expression.PropertyOrField(item, "Ticks"));

                //writer.Write(item.Ticks);
            }
            else if (type == typeof(String))
            {
                var writeBool = typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) });
                var writeString = typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(string) });

                if (!canBeNull)
                    return Expression.Call(writer, writeString, item);

                return Expression.IfThenElse(Expression.NotEqual(item, Expression.Constant(null)),
                    Expression.Block(Expression.Call(writer, writeBool, Expression.Constant(true)), Expression.Call(writer, writeString, item)),
                    Expression.Call(writer, writeBool, Expression.Constant(false))
                    );

                //if (item != null)
                //{
                //    writer.Write(true);
                //    writer.Write(item);
                //}
                //else
                //    writer.Write(false);
            }
            else if (type == typeof(byte[]))
            {
                var writeByteArray = typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(byte[]) });

                if (!canBeNull)
                    return Expression.Block(
                            Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.ConvertChecked(Expression.Property(item, "Length"), typeof(ulong))),
                            Expression.Call(writer, writeByteArray, item)
                            );

                return Expression.IfThenElse(Expression.NotEqual(item, Expression.Constant(null)),
                        Expression.Block(
                            Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(true)),
                            Expression.Call(typeof(CountCompression).GetMethod("Serialize"), writer, Expression.ConvertChecked(Expression.Property(item, "Length"), typeof(ulong))),
                            Expression.Call(writer, writeByteArray, item)
                            ),
                        Expression.Call(writer, typeof(BinaryWriter).GetMethod("Write", new Type[] { typeof(bool) }), Expression.Constant(false))
                        );

                //if (buffer != null)
                //{
                //    writer.Write(true);
                //    CountCompression.Serialize(writer, checked((long)buffer.Length));
                //    writer.Write(buffer);
                //}
                //else
                //    writer.Write(false);
            }
            else
                throw new NotSupportedException(type.ToString());
        }

        public static Expression CreateReadBody(Expression reader, Type itemType, Func<Type, MemberInfo, int> membersOrder, AllowNull allowNull)
        {
            return BuildRead(reader, itemType, membersOrder, allowNull, 0);
        }

        private static Expression BuildRead(Expression reader, Type itemType, Func<Type, MemberInfo, int> membersOrder, AllowNull allowNull, int depth)
        {
            bool canBeNull = CanBeNull(itemType, allowNull, depth);

            if (itemType == typeof(Guid))
                return Expression.New(itemType.GetConstructor(new Type[] { typeof(byte[]) }), GetReadCommand(reader, typeof(byte[]), canBeNull));

            if (itemType.IsEnum)
                return Expression.Convert(GetReadCommand(reader, itemType.GetEnumUnderlyingType(), canBeNull), itemType);

            if (DataType.IsPrimitiveType(itemType))
                return GetReadCommand(reader, itemType, canBeNull);

            if (itemType.IsKeyValuePair())
            {
                return Expression.New(
                        itemType.GetConstructor(new Type[] { itemType.GetGenericArguments()[0], itemType.GetGenericArguments()[1] }),
                        BuildRead(reader, itemType.GetGenericArguments()[0], membersOrder, allowNull, depth + 1), BuildRead(reader, itemType.GetGenericArguments()[1], membersOrder, allowNull, depth + 1)
                    );
            }

            if (itemType.IsArray || itemType.IsList() || itemType.IsDictionary())
            {
                var field = Expression.Variable(itemType);
                var lenght = Expression.Variable(typeof(int));

                var block = Expression.Block(
                    Expression.Assign(lenght, Expression.Convert(Expression.Call(typeof(CountCompression).GetMethod("Deserialize"), reader), typeof(int))),
                    itemType.IsDictionary() && itemType.GetGenericArguments()[0] == typeof(byte[]) ?
                        Expression.Assign(field, Expression.New(field.Type.GetConstructor(new Type[] { typeof(int), typeof(IEqualityComparer<byte[]>) }), lenght, Expression.Field(null, typeof(BigEndianByteArrayEqualityComparer), "Instance"))) :
                        Expression.Assign(field, Expression.New(field.Type.GetConstructor(new Type[] { typeof(int) }), lenght)),
                    field.For(i =>
                        {
                            if (itemType.IsArray)
                                return Expression.Assign(Expression.ArrayAccess(field, i), BuildRead(reader, itemType.GetElementType(), membersOrder, allowNull, depth + 1));
                            else if (itemType.IsList())
                                return Expression.Call(field, field.Type.GetMethod("Add"), BuildRead(reader, itemType.GetGenericArguments()[0], membersOrder, allowNull, depth + 1));
                            else //if (dataType.IsDictionary)                   
                            {
                                var keyType = itemType.GetGenericArguments()[0];

                                if (!IsSupportDictionaryKeyType(keyType))
                                    throw new NotSupportedException(String.Format("Dictionarty<{0}, TValue>", keyType.ToString()));

                                return Expression.Call(field, field.Type.GetMethod("Add"),
                                    BuildRead(reader, itemType.GetGenericArguments()[0], membersOrder, allowNull, depth + 1),
                                    BuildRead(reader, itemType.GetGenericArguments()[1], membersOrder, allowNull, depth + 1)
                                    );
                            }
                        },
                        Expression.Label(), lenght)
                    );

                if (canBeNull)
                    return Expression.Block(field.Type, new ParameterExpression[] { field, lenght },
                        Expression.IfThenElse(Expression.Call(reader, typeof(BinaryReader).GetMethod("ReadBoolean")),
                            block,
                            Expression.Assign(field, Expression.Constant(null, field.Type))),
                        Expression.Label(Expression.Label(field.Type), field));

                return Expression.Block(field.Type, new ParameterExpression[] { field, lenght },
                        block,
                        Expression.Label(Expression.Label(field.Type), field));
            }

            if (itemType.IsNullable())
            {
                if (!canBeNull)
                    return Expression.New(itemType.GetConstructor(new Type[] { itemType.GetGenericArguments()[0] }), BuildRead(reader, itemType.GetGenericArguments()[0], membersOrder, allowNull, depth + 1));

                return Expression.Condition(Expression.Call(reader, typeof(BinaryReader).GetMethod("ReadBoolean")),
                        Expression.New(itemType.GetConstructor(new Type[] { itemType.GetGenericArguments()[0] }), BuildRead(reader, itemType.GetGenericArguments()[0], membersOrder, allowNull, depth + 1)),
                        Expression.Constant(null, itemType));
            }

            if (itemType.IsClass || itemType.IsStruct())
            {
                var item = Expression.Variable(itemType);

                List<Expression> list = new List<Expression>();
                list.Add(Expression.Assign(item, Expression.New(item.Type)));

                foreach (var member in DataTypeUtils.GetPublicMembers(itemType, membersOrder))
                    list.Add(Expression.Assign(Expression.PropertyOrField(item, member.Name), BuildRead(reader, member.GetUnderlyingType(), membersOrder, allowNull, depth + 1)));

                if (!canBeNull)
                {
                    list.Add(Expression.Label(Expression.Label(item.Type), item));
                    return Expression.Block(item.Type, new ParameterExpression[] { item }, list);
                }

                return Expression.Block(itemType, new ParameterExpression[] { item },
                    Expression.IfThenElse(Expression.Call(reader, typeof(BinaryReader).GetMethod("ReadBoolean")),
                        Expression.Block(list),
                        Expression.Assign(item, Expression.Constant(null, itemType))),
                    Expression.Label(Expression.Label(item.Type), item));
            }

            throw new ArgumentException(itemType.ToString());
        }

        private static Expression GetReadCommand(Expression reader, Type itemType, bool canBeNull)
        {
            Debug.Assert(DataType.IsPrimitiveType(itemType));

            if (itemType == typeof(Boolean) || itemType == typeof(Char) || itemType == typeof(SByte) || itemType == typeof(Byte) ||
                    itemType == typeof(Int16) || itemType == typeof(UInt16) || itemType == typeof(Int32) || itemType == typeof(UInt32) || itemType == typeof(Int64) || itemType == typeof(UInt64) ||
                    itemType == typeof(Single) || itemType == typeof(Double) || itemType == typeof(Decimal))
            {
                MethodInfo readAny = typeof(BinaryReader).GetMethod("Read" + itemType.Name);

                //return reader.ReadInt32();

                return Expression.Call(reader, readAny);
            }

            if (itemType == typeof(DateTime))
            {
                MethodInfo readLong = typeof(BinaryReader).GetMethod("Read" + typeof(long).Name);
                return Expression.New(typeof(DateTime).GetConstructor(new Type[] { typeof(long) }), Expression.Call(reader, readLong));

                //return new DateTime(reader.ReadInt64());
            }

            if (itemType == typeof(TimeSpan))
            {
                MethodInfo readLong = typeof(BinaryReader).GetMethod("Read" + typeof(long).Name);
                return Expression.New(typeof(TimeSpan).GetConstructor(new Type[] { typeof(long) }), Expression.Call(reader, readLong));

                //return new DateTime(reader.ReadInt64());
            }

            if (itemType == typeof(string))
            {
                var readBool = typeof(BinaryReader).GetMethod("Read" + typeof(bool).Name);
                var readString = typeof(BinaryReader).GetMethod("Read" + typeof(string).Name);

                if (!canBeNull)
                    return Expression.Call(reader, readString); //return reader.ReadString();

                //return reader.ReadBoolean() ? reader.ReadString() : null;

                return Expression.Condition(Expression.Call(reader, readBool), Expression.Call(reader, readString), Expression.Constant(null, typeof(string)));
            }

            if (itemType == typeof(byte[]))
            {
                var readBool = typeof(BinaryReader).GetMethod("Read" + typeof(bool).Name);
                var readBytes = typeof(BinaryReader).GetMethod("ReadBytes");

                var call = Expression.Call(typeof(CountCompression).GetMethod("Deserialize"), reader);

                if (!canBeNull)
                    return Expression.Call(reader, readBytes, Expression.Convert(call, typeof(int))); //return reader.ReadBytes((int)CountCompression.Deserialize(reader));

                //return reader.ReadBoolean() ? reader.ReadBytes((int)CountCompression.Deserialize(reader)) : null;

                return Expression.Condition(Expression.Call(reader, readBool), Expression.Call(reader, readBytes, Expression.Convert(call, typeof(int))), Expression.Constant(null, typeof(byte[])));
            }

            throw new NotSupportedException(itemType.ToString());
        }

        private static bool CanBeNull(Type type, AllowNull allowNull, int depth)
        {
            //if (type == typeof(Guid))
            //    return false;

            if (type.IsEnum)
                return false;

            if (type.IsStruct() && !type.IsNullable())
                return false;

            if (allowNull == AllowNull.OnlyMembers)
                return depth > 0;

            return allowNull == AllowNull.All;
        }

        private static bool IsSupportDictionaryKeyType(Type type)
        {
            if (type == typeof(Guid))
                return true;

            if(type.IsEnum)
                return true;

            if (DataType.IsPrimitiveType(type))
                return true;

            return false;
        }
    }
}
