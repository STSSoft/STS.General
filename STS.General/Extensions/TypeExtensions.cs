using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace STS.General.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsStruct(this Type type)
        {
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
        }

        public static bool IsInheritInterface(this Type type, Type @interface)
        {
            if (!@interface.IsInterface)
                throw new ArgumentException(String.Format("The type '{0}' has to be an interface.", @interface.Name));

            return type.GetInterfaces().FirstOrDefault(x => x == @interface) != null;
        }

        public static IEnumerable<MemberInfo> GetPublicReadWritePropertiesAndFields(this Type type)
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (member.MemberType == MemberTypes.Field)
                {
                    FieldInfo field = (FieldInfo)member;
                    if (field.IsInitOnly)
                        continue;

                    yield return member;
                }

                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    if (property.GetAccessors(false).Length != 2)
                        continue;

                    yield return member;
                }
            }
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(new Type[] { }) != null;
        }

        public static bool IsDictionary(this Type type)
        {
            return type.Name == typeof(Dictionary<,>).Name;
        }

        public static bool IsList(this Type type)
        {
            return type.Name == typeof(List<>).Name;
        }

        public static bool IsKeyValuePair(this Type type)
        {
            return type.Name == typeof(KeyValuePair<,>).Name;
        }

        public static bool IsNullable(this Type type)
        {
            return type.Name == typeof(Nullable<>).Name;
        }

        public static bool IsNumeric(this Type type)
        {
            if (type.IsEnum)
                return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
