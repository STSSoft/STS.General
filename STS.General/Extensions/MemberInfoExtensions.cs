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
#if NETFX_CORE
            if (member is FieldInfo)
                return ((FieldInfo)member).FieldType;

            if (member is PropertyInfo)
                return ((PropertyInfo)member).PropertyType;
            
            if (member is EventInfo)
                return ((EventInfo)member).EventHandlerType;

            throw new ArgumentException("MemberType must be from FieldInfo, PropertyInfo or EventInfo", "member");
#else
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
#endif
        }

        public static bool CanRead(this MemberInfo member)
        {
#if NETFX_CORE
            if (member is PropertyInfo)
            {
                PropertyInfo property = member as PropertyInfo;

                return property.CanRead;
            }

            if (member is FieldInfo)
                return true;

            throw new NotSupportedException(member.ToString());
#else
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
#endif
        }

        public static bool CanWrite(this MemberInfo member)
        {
#if NETFX_CORE
            if (member is PropertyInfo)
                return ((PropertyInfo)member).CanWrite;

            if (member is FieldInfo)
            {
                FieldInfo field = member as FieldInfo;

                return ((field.Attributes & FieldAttributes.InitOnly) != (FieldAttributes.InitOnly)) && ((field.Attributes & FieldAttributes.Literal) != (FieldAttributes.Literal));
            }

            throw new NotSupportedException(member.ToString());
#else
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
#endif
        }

        public static bool IsStatic(this MemberInfo member)
        {
#if NETFX_CORE
            if (member is PropertyInfo)
            {
                PropertyInfo property = member as PropertyInfo;

                MethodInfo getMethod = property.GetMethod;
                if (getMethod != null)
                    return getMethod.IsStatic;
                else
                    return property.SetMethod.IsStatic;
            }

            if (member is FieldInfo)
            {
                FieldInfo field = member as FieldInfo;

                return (field.Attributes & FieldAttributes.Static) == FieldAttributes.Static;
            }

            throw new NotSupportedException(member.ToString());
#else
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
#endif
        }
    }
}
