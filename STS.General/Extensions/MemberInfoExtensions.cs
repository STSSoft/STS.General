using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Extensions
{
    public static class MemberInfoExtensions
    {
        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberType must be from FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }

        public static bool CanRead(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo pi = (PropertyInfo)member;
                        return pi.CanRead;
                    }

                case MemberTypes.Field:
                    {
                        return true;
                    }

                default:
                    throw new NotSupportedException(member.MemberType.ToString());
            }
        }

        public static bool CanWrite(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo pi = (PropertyInfo)member;

                        return pi.CanWrite;
                    }

                case MemberTypes.Field:
                    {
                        FieldInfo fi = (FieldInfo)member;

                        return ((fi.Attributes & FieldAttributes.InitOnly) != (FieldAttributes.InitOnly)) && ((fi.Attributes & FieldAttributes.Literal) != (FieldAttributes.Literal));
                    }

                default:
                    throw new NotSupportedException(member.MemberType.ToString());
            }
        }

        public static bool IsStatic(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Property:
                    {
                        PropertyInfo pi = (PropertyInfo)member;

                        var getMethod = pi.GetGetMethod();
                        if (getMethod != null)
                            return getMethod.IsStatic;
                        else
                            return pi.GetSetMethod().IsStatic;
                    }

                case MemberTypes.Field:
                    {
                        FieldInfo fi = (FieldInfo)member;

                        return (fi.Attributes & FieldAttributes.Static) == FieldAttributes.Static;
                    }

                default:
                    throw new NotSupportedException(member.MemberType.ToString());
            }
        }
    }
}
