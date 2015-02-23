using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using STS.General.Extensions;

namespace STS.General.Reflection
{
    public static class TypeAnalyzer
    {
        public static int GetSize(this Type type, BindingFlags bindingAtr, int unknownTypeSize = 16)
        {
            int size = 0;
            type.GetInternalSize(bindingAtr, unknownTypeSize, ref size);

            return size;
        }

        private static void GetInternalSize(this Type type, BindingFlags bindingAtr, int unknownTypeSize, ref int size)
        {
            if (type.IsPrimitive)
            {
                size += Marshal.SizeOf(type);
                return;
            }

            if (type == typeof(DateTime) || type == typeof(TimeSpan))
            {
                size += sizeof(long);
                return;
            }

            if (type.IsEnum)
            {
                Enum.GetUnderlyingType(type).GetInternalSize(BindingFlags.Public | BindingFlags.Instance, unknownTypeSize, ref size);
                return;
            }

            if (type == typeof(string) || type.IsArray || type.Name == typeof(List<>).Name || type.Name == typeof(Dictionary<,>).Name || type.Name == typeof(Nullable<>).Name)
            {
                size += unknownTypeSize;
                return;
            }

            if (type.Name == typeof(KeyValuePair<,>).Name)
            {
                var kvTypes = type.GetGenericArguments();
                kvTypes[0].GetInternalSize(bindingAtr, unknownTypeSize, ref size);
                kvTypes[1].GetInternalSize(bindingAtr, unknownTypeSize, ref size);

                return;
            }

            if (type.IsValueType) //is struct
            {
                foreach (MemberInfo member in type.GetMembers(bindingAtr).Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field))
                    member.GetUnderlyingType().GetInternalSize(bindingAtr, unknownTypeSize, ref size);

                return;
            }

            if (type.IsClass)
            {
                size += 1;
                foreach (MemberInfo member in type.GetMembers(bindingAtr).Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field))
                    member.GetUnderlyingType().GetInternalSize(bindingAtr, unknownTypeSize, ref size);

                return;
            }
        }

        public static bool HasFixedLenght(this Type type, BindingFlags bindingAtr)
        {
            if (type.IsPrimitive || type.IsEnum)
                return true;

            if (type == typeof(DateTime) || type == typeof(TimeSpan))
                return true;

            if (type == typeof(string) || type.IsArray || type.Name == typeof(List<>).Name || type.Name == typeof(Dictionary<,>).Name || type.Name == typeof(Nullable<>).Name)
                return false;

            if (type.Name == typeof(KeyValuePair<,>).Name)
            {
                var kvTypes = type.GetGenericArguments();
                if (!kvTypes[0].HasFixedLenght(bindingAtr))
                    return false;
                if (!kvTypes[1].HasFixedLenght(bindingAtr))
                    return false;
            }

            if (type.IsValueType) //is struct
            {
                foreach (MemberInfo member in type.GetMembers(bindingAtr).Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field))
                {
                    if (!member.GetUnderlyingType().HasFixedLenght(bindingAtr))
                        return false;
                }
            }

            if (type.IsClass)
            {
                foreach (MemberInfo member in type.GetMembers(bindingAtr).Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field))
                {
                    if (!member.GetUnderlyingType().HasFixedLenght(bindingAtr))
                        return false;
                }
            }

            return true;
        }

        private static IEnumerable<KeyValuePair<Type, string>> GetNestedProperties(Type type, Func<Type, bool> isSupported, StringBuilder path, HashSet<Type> set)
        {
            foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetAccessors(false).Length == 2))
            {
                if (set.Contains(p.PropertyType))
                    continue;

                yield return new KeyValuePair<Type, string>(p.PropertyType, path.ToString() + "." + p.Name);

                if (isSupported(p.PropertyType))
                    continue;

                path.Append("." + p.Name);
                set.Add(p.PropertyType);

                foreach (var kv in GetNestedProperties(p.PropertyType, isSupported, path, set))
                    yield return kv;

                set.Remove(p.PropertyType);
                path.Remove(path.Length - p.Name.Length - 1, p.Name.Length + 1);
            }
        }

        public static IEnumerable<KeyValuePair<Type, string>> GetNestedProperties(Type type, Func<Type, bool> isSupported)
        {
            return GetNestedProperties(type, isSupported, new StringBuilder(type.Name), new HashSet<Type>(new Type[] { type }));
        }
    }
}
