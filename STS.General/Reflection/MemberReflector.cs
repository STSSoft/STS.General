using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using STS.General.Extensions;

namespace STS.General.Reflection
{
    /// <summary>
    /// Provides fast generic nested access - get and set, to private static/non-static properties, fields and constants.
    /// </summary>
    public class MemberReflector<TObject, TMember>
    {
        private List<MemberInfo> members = new List<MemberInfo>();

        public Type OwnerType { get; private set; }
        public string MemberName { get; private set; }
        public readonly bool CanRead;
        public readonly bool CanWrite;

        public readonly Func<TObject, TMember> Get;
        public readonly Action<TObject, TMember> Set;

        public MemberReflector(string pathOrMemberName)
        {
            OwnerType = typeof(TObject);
            MemberName = pathOrMemberName;

            Type type = OwnerType;
            string[] tokens = pathOrMemberName.Split('.');

            for (int i = 0; i < tokens.Length; i++)
            {
                var info = FindMember(type, tokens[i]);
                members.Add(info);

                type = info.GetUnderlyingType();
            }

            bool canAccess = members.Take(members.Count - 1).All(x => x.CanRead());

            CanRead = canAccess && members[members.Count - 1].CanRead();
            CanWrite = canAccess && members[members.Count - 1].CanWrite();

            if (CanRead)
                Get = CreateGetMethod().Compile();
            if (CanWrite)
                Set = CreateSetMethod().Compile();
        }

        private static MemberInfo FindMember(Type type, string memberName)
        {
            MemberInfo[] members;

#if NETFX_CORE
            members = type.GetMembers();
#else
            members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
#endif

            var memberInfo = members.Where(x => x.Name == memberName).FirstOrDefault();
            if (memberInfo == null)
                throw new ArgumentException(String.Format("Type {0} does not have member '{1}'.", type, memberName));

            return memberInfo;
        }

        public Expression<Func<TObject, TMember>> CreateGetMethod()
        {
            var obj = Expression.Parameter(typeof(TObject), "obj");

            var member = Expression.MakeMemberAccess(members[0].IsStatic() ? null : obj, members[0]);
            for (int i = 1; i < members.Count; i++)
                member = Expression.MakeMemberAccess(members[i].IsStatic() ? null : member, members[i]);

            var lambda = Expression.Lambda<Func<TObject, TMember>>(member, obj);

            return lambda;
        }

        public Expression<Action<TObject, TMember>> CreateSetMethod()
        {
            var obj = Expression.Parameter(typeof(TObject), "obj");
            var value = Expression.Parameter(typeof(TMember), "value");

            var member = Expression.MakeMemberAccess(members[0].IsStatic() ? null : obj, members[0]);
            for (int i = 1; i < members.Count; i++)
                member = Expression.MakeMemberAccess(members[i].IsStatic() ? null : member, members[i]);

            var lambda = Expression.Lambda<Action<TObject, TMember>>(Expression.Assign(member, value), obj, value);

            return lambda;
        }

        public MemberInfo MemberInfo
        {
            get { return members[members.Count - 1]; }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}", typeof(TObject).Name, MemberName);
        }

        public static Func<TObject, TMember> MemberGet(string memberName)
        {
            MemberReflector<TObject, TMember> helper = new MemberReflector<TObject, TMember>(memberName);

            return helper.Get;
        }

        public static Action<TObject, TMember> MemberSet(string memberName)
        {
            MemberReflector<TObject, TMember> helper = new MemberReflector<TObject, TMember>(memberName);

            return helper.Set;
        }
    }

    /// <summary>
    /// Provides fast non-generic nested access - get and set, to private static/non-static properties, fields and constants.
    /// </summary>
    public class MemberReflector
    {
        private List<MemberInfo> members = new List<MemberInfo>();

        public Type OwnerType { get; private set; }
        public string MemberName { get; private set; }
        public readonly bool CanRead;
        public readonly bool CanWrite;

        public readonly Func<object, object> Get;
        public readonly Action<object, object> Set;

        public MemberReflector(Type ownerType, string pathOrMemberName)
        {
            OwnerType = ownerType;
            MemberName = pathOrMemberName;

            Type type = OwnerType;
            string[] tokens = pathOrMemberName.Split('.');

            for (int i = 0; i < tokens.Length; i++)
            {
                var info = FindMember(type, tokens[i]);
                members.Add(info);

                type = info.GetUnderlyingType();
            }

            bool canAccess = members.Take(members.Count - 1).All(x => x.CanRead());

            CanRead = canAccess && members[members.Count - 1].CanRead();
            CanWrite = canAccess && members[members.Count - 1].CanWrite();

            if (CanRead)
                Get = CreateGetMethod().Compile();
            if (CanWrite)
                Set = CreateSetMethod().Compile();
        }

        private static MemberInfo FindMember(Type type, string memberName)
        {
            MemberInfo[] members;

#if NETFX_CORE
            members = type.GetMembers();
#else
            members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
#endif

            var memberInfo = members.Where(x => x.Name == memberName).FirstOrDefault();
            if (memberInfo == null)
                throw new ArgumentException(String.Format("Type {0} does not have member '{1}'.", type, memberName));

            return memberInfo;
        }

        public Expression<Func<object, object>> CreateGetMethod()
        {
            var obj = Expression.Parameter(typeof(object), "obj");

            var member = Expression.MakeMemberAccess(members[0].IsStatic() ? null : Expression.Convert(obj, OwnerType), members[0]);
            for (int i = 1; i < members.Count; i++)
                member = Expression.MakeMemberAccess(members[i].IsStatic() ? null : member, members[i]);

            var lambda = Expression.Lambda<Func<object, object>>(Expression.Convert(member, typeof(object)), obj);

            return lambda;
        }

        public Expression<Action<object, object>> CreateSetMethod()
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            var value = Expression.Parameter(typeof(object), "value");

            var member = Expression.MakeMemberAccess(members[0].IsStatic() ? null : Expression.Convert(obj, OwnerType), members[0]);
            for (int i = 1; i < members.Count; i++)
                member = Expression.MakeMemberAccess(members[i].IsStatic() ? null : member, members[i]);

            var lambda = Expression.Lambda<Action<object, object>>(Expression.Assign(member, Expression.Convert(value, members[members.Count - 1].GetUnderlyingType())), obj, value);

            return lambda;
        }

        public MemberInfo MemberInfo
        {
            get { return members[members.Count - 1]; }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}", OwnerType.Name, MemberName);
        }

        public static Func<object, object> MemberGet(Type ownerType, string memberName)
        {
            MemberReflector helper = new MemberReflector(ownerType, memberName);

            return helper.Get;
        }

        public static Action<object, object> MemberSet(Type ownerType, string memberName)
        {
            MemberReflector helper = new MemberReflector(ownerType, memberName);

            return helper.Set;
        }
    }
}
