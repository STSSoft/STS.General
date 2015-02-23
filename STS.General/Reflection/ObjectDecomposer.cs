using STS.General.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace STS.General.Reflection
{
    public class ObjectDecomposer
    {
        public readonly Func<object, List<string>> toStringList;
        public readonly Func<object, List<object>> toObjectList;

        public readonly Type Type;
        public readonly Func<Type, MemberInfo, int> MembersOrder;

        public HashSet<string> ExcludedMembers { get; private set; }

        public List<Type> CurrentMembersType { get; private set; }
        public List<string> AllMemberNames { get; private set; }
        public List<string> CurrentMemberNames { get; private set; }

        public ObjectDecomposer(Type type, HashSet<string> excludedMembers = null, Func<Type, MemberInfo, int> membersOrder = null)
        {
            Type = type;
            MembersOrder = membersOrder;
            ExcludedMembers = excludedMembers == null ? new HashSet<string>() : excludedMembers;

            AllMemberNames = new List<string>();
            CurrentMemberNames = new List<string>();
            CurrentMembersType = new List<Type>();

            toObjectList = CreateToObjectListMethod().Compile();
            toStringList = CreateToStringArrayMethod().Compile();
        }

        public List<string> ToStringList(object value)
        {
            return toStringList(value);
        }

        public List<object> ToObjectList(object value)
        {
            return toObjectList(value);
        }

        public Expression<Func<object, List<string>>> CreateToStringArrayMethod()
        {
            var list = new List<Expression>();

            var item = Expression.Parameter(typeof(object));
            var value = Expression.Variable(Type);

            var stringList = Expression.Variable(typeof(List<string>));
            list.Add(Expression.Assign(stringList, Expression.New(typeof(List<string>).GetConstructor(Type.EmptyTypes))));

            list.Add(Expression.Assign(value, Expression.Convert(item, Type)));
            list.Add(CreateStrBodyFor(value, stringList, null));

            list.Add(Expression.Label(Expression.Label(typeof(List<string>)), stringList));

            var block = Expression.Block(new ParameterExpression[] { stringList, value }, list);

            return Expression.Lambda<Func<object, List<string>>>(block, item);
        }

        public Expression<Func<object, List<object>>> CreateToObjectListMethod()
        {
            var list = new List<Expression>();

            var item = Expression.Parameter(typeof(object));
            var value = Expression.Variable(Type);

            var objList = Expression.Variable(typeof(List<object>));
            list.Add(Expression.Assign(objList, Expression.New(typeof(List<object>).GetConstructor(Type.EmptyTypes))));

            list.Add(Expression.Assign(value, Expression.Convert(item, Type)));
            list.Add(CreateObjBodyFor(value, objList, null));

            list.Add(Expression.Label(Expression.Label(typeof(List<object>)), objList));

            var block = Expression.Block(new ParameterExpression[] { objList, value }, list);

            return Expression.Lambda<Func<object, List<object>>>(block, item);
        }

        private Expression CreateObjBodyFor(Expression item, Expression objList, string parentName)
        {
            var list = new List<Expression>();

            var type = item.Type;
            var value = Expression.Variable(type);
            list.Add(Expression.Assign(value, Expression.Convert(item, type)));

            var addMethod = typeof(List<object>).GetMethod("Add", new Type[] { typeof(object) });

            if (type == typeof(object) || type.IsEnum || type.IsPrimitive || type == typeof(string))
                list.Add(Expression.Call(objList, addMethod, Expression.Convert(value, typeof(object))));
            else
            {
                foreach (var member in GetMembers(type))
                {
                    string memberName = parentName == null ? member.Name : string.Join(".", parentName, member.Name);

                    var memberType = member.GetUnderlyingType();
                    var attributes = memberType.GetCustomAttributes();
                    bool goesIn = attributes.Where(x => x.GetType() == typeof(BrowsableAttribute)).Count() > 0;

                    if (ExcludedMembers.Contains(memberName))
                        continue;

                    if (!goesIn)
                        list.Add(Expression.Call(objList, addMethod, Expression.Convert(Expression.PropertyOrField(value, member.Name), typeof(object))));
                    else
                    {
                        var field = Expression.PropertyOrField(value, member.Name);
                        list.Add(CreateObjBodyFor(field, objList, memberName));
                    }
                }
            }

            return Expression.Block(new ParameterExpression[] { value }, list);
        }

        private Expression CreateStrBodyFor(Expression item, Expression stringList, string parentName)
        {
            var list = new List<Expression>();

            var type = item.Type;
            var value = Expression.Variable(type);
            list.Add(Expression.Assign(value, Expression.Convert(item, type)));

            var addMethod = typeof(List<string>).GetMethod("Add", new Type[] { typeof(string) });

            if (type == typeof(object) || type.IsEnum || type.IsPrimitive || type == typeof(string))
            {
                CurrentMemberNames.Add(type.Name);
                CurrentMembersType.Add(type);
                AllMemberNames.Add(type.Name);
                list.Add(Expression.Call(stringList, addMethod, CallToString(value)));
            }
            else
            {
                foreach (var member in GetMembers(type))
                {
                    string memberName = parentName == null ? member.Name : string.Join(".", parentName, member.Name);

                    var memberType = member.GetUnderlyingType();
                    var attributes = memberType.GetCustomAttributes();
                    bool hasRecursiveAttr = attributes.Where(x => x.GetType() == typeof(BrowsableAttribute)).Count() > 0;

                    if (!hasRecursiveAttr)
                        AllMemberNames.Add(memberName);

                    if (ExcludedMembers.Contains(memberName))
                        continue;

                    if (hasRecursiveAttr)
                    {
                        var field = Expression.PropertyOrField(value, member.Name);
                        list.Add(CreateStrBodyFor(field, stringList, memberName));
                    }
                    else
                    {
                        CurrentMembersType.Add(memberType);
                        CurrentMemberNames.Add(memberName);
                        if (!memberType.IsClass)
                            list.Add(Expression.Call(stringList, addMethod, CallToString(Expression.PropertyOrField(value, member.Name))));
                        else
                        {
                            var cond = Expression.Condition(Expression.NotEqual(value, Expression.Constant(null)),
                                Expression.Call(stringList, addMethod, CallToString(Expression.PropertyOrField(value, member.Name))),
                                Expression.Call(stringList, addMethod, Expression.Constant("null", typeof(string)))
                                );

                            list.Add(cond);
                        }
                    }
                }
            }

            return Expression.Block(new ParameterExpression[] { value }, list);
        }

        private Expression CallToString(Expression member)
        {
            var toStringMethod = member.Type.GetMethod("ToString", new Type[] { });

            if (member.Type.IsClass)
            {
                return Expression.Block(
                    Expression.Condition(Expression.NotEqual(member, Expression.Constant(null)),
                        Expression.Label(Expression.Label(typeof(string)), Expression.Call(member, toStringMethod)),
                        Expression.Label(Expression.Label(typeof(string)), Expression.Constant("null", typeof(string)))
                    ));
            }

            return Expression.Call(member, toStringMethod);
        }

        private IEnumerable<MemberInfo> GetMembers(Type type, HashSet<string> excludedMembers = null)
        {
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (excludedMembers != null && excludedMembers.Contains(member.Name))
                    continue;

                //Members
                if (member.MemberType == MemberTypes.Field)
                    yield return (FieldInfo)member;

                //Properties
                if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    if (property.GetIndexParameters().Length > 0)
                        continue;

                    if (property.GetMethod != null)
                        yield return member;
                }
            }
        }
    }
}