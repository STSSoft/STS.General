using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using STS.General.Data;

namespace STS.General.Extensions
{
#if !NETFX_CORE
    using TypeCode = STS.General.Data.TypeCode;
#endif

    public static class TypeExtensions
    {
#if NETFX_CORE
        /// <summary>
        /// Searches for a public instance constructor whose parameters match the types in the specified array of Type objects.
        /// </summary>                  
        /// <param name="types">An array of Type objects representing the number, order, and type of the parameters you want the constructor to get.</param>
        /// <returns>A ConstructorInfo object that represents the public instance constructor whose parameters match the types in the specified array, if such a match is found; otherwise, a null reference.</returns>
        public static ConstructorInfo GetConstructor(this Type type, params Type[] types)
        {
            ConstructorInfo constructor = null;
            foreach (var ctr in type.GetTypeInfo().DeclaredConstructors)
            {
                Type[] parameterTypes = ctr.GetParameters().Select(x => x.ParameterType).ToArray();

                if (!CheckTypesEqual(parameterTypes, types))
                    continue;

                constructor = ctr;
                break;
            }

            return constructor;
        }

        /// <summary>
        ///  Searches for the specified public method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="name">The string containing the name of the public method to get.</param>
        /// <returns>An object representing the public method whose parameters match the specified argument types, if found; otherwise, null.</returns>
        public static MethodInfo GetMethod(this Type type, string name)
        {
            MethodInfo methodInfo = null;
            foreach (var method in type.GetRuntimeMethods())
            {
                Type[] parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();

                if (method.Name != name)
                    continue;
                
                methodInfo = method;
                break;
            }

            return methodInfo;
        }

        /// <summary>
        ///  Searches for the specified public method whose parameters match the specified argument types.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name">The string containing the name of the public method to get.</param>
        /// <param name="types"> An array of System.Type objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of System.Type objects to get a method that takes no parameters.</param>
        /// <returns>An object representing the public method whose parameters match the specified argument types, if found; otherwise, null.</returns>
        public static MethodInfo GetMethod(this Type type, string name, params Type[] types)
        {
            MethodInfo methodInfo = null;
            foreach (var method in type.GetRuntimeMethods())
            {
                Type[] parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
                
                if (!CheckTypesEqual(parameterTypes, types))
                    continue;

                methodInfo = method;
                break;
            }

            return methodInfo;
        }

        /// <summary>
        /// Gets all the interfaces implemented or inherited by the current System.Type.
        /// </summary>
        /// <param name="type">The type that contains the interfaces.</param>
        /// <returns> An array of System.Type objects representing all the interfaces implemented
        //     or inherited by the current System.Type.-or- An empty array of type System.Type,
        //     if no interfaces are implemented or inherited by the current System.Type.</returns>
        public static Type[] GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.ToArray();
        }

        /// <summary>
        /// Retrieves an object that represents a specified property.
        /// </summary>
        /// <param name="type">The type that contains the property.</param>
        /// <param name="name">The string containing the name of the public property to get.</param>
        /// <returns> An object representing the public property with the specified name, if found otherwise, null.</returns>
        public static PropertyInfo GetProperty(this Type type, string name)
        {
            return type.GetRuntimeProperty(name);
        }

        /// <summary>
        /// Searches for the specified nested type.
        /// </summary>
        /// <param name="type">The type that contains the nested type.</param>
        /// <param name="name">The string containing the name of the nested type to get.</param>
        /// <returns>An object representing the nested type that matches the specified requirements, if found; otherwise, null.</returns>
        public static Type GetNestedType(this Type type, string name)
        {
            return type.GetTypeInfo().GetDeclaredNestedType(name).AsType();
        }

        /// <summary>
        ///  Searches for the members defined for the current System.Type.
        /// </summary>
        /// <param name="type">The type that contains the member.</param>
        /// <returns> An array of System.Reflection.MemberInfo objects representing all the public
        /// members of the current System.Type.-or- An empty array of type System.Reflection.MemberInfo,
        /// if the current System.Type does not have public members.
        /// </returns>
        public static MemberInfo[] GetMembers(this Type type)
        {
            List<MemberInfo> members = new List<MemberInfo>();

            foreach (var field in type.GetRuntimeFields())
                members.Add(field);

            foreach (var property in type.GetRuntimeProperties())
                members.Add(property);

            return members.ToArray();
        }

        /// <summary>
        /// Searches for the public members with the specified name.
        /// </summary>
        /// <param name="type">The type that contains the member.</param>
        /// <param name="name">The string containing the name of the public members to get.</param>
        /// <returns> An array of System.Reflection.MemberInfo objects representing the public members with the specified name, if found; otherwise, an empty array.</returns>
        public static MemberInfo[] GetMember(this Type type, string name)
        {
            return type.GetMembers().Where(member => member.Name == name).ToArray();
        }

        private static bool CheckTypesEqual(Type[] x, Type[] y)
        {
            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }
#endif
        public static bool IsStruct(this Type type)
        {
#if NETFX_CORE
            TypeInfo typeInfo = type.GetTypeInfo();

            return typeInfo.IsValueType && !typeInfo.IsPrimitive && !typeInfo.IsEnum;
#else
            return type.IsValueType && !type.IsPrimitive && !type.IsEnum;
#endif
        }

        /// <summary>
        /// Gets a value indicating whether the System.Type is one of the primitive types. Use this method if you want the code to be multiplatform.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns>true if the System.Type is one of the primitive types; otherwise, false.</returns>
        public static bool IsPrimitive(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsPrimitive;
#else
            return type.IsPrimitive;
#endif
        }

        /// <summary>
        /// Gets a value indicating whether the System.Type is a class; that is, not a value type or interface.
        /// Use this method if you want the code to be multiplatform
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns> true if the System.Type is a class; otherwise, false.</returns>
        public static bool IsClass(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        /// <summary>
        /// Gets a value indicating whether the current System.Type represents an enumeration.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns>true if the current System.Type represents an enumeration; otherwise, false.</returns>
        public static bool IsEnum(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().IsEnum;
#else
            return type.IsEnum;
#endif
        }

        /// <summary>
        /// Returns the underlying type of the current enumeration type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The underlying type of the current enumeration.</returns>
        public static Type GetEnumBaseType(this Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().BaseType;
#else
            return type.GetEnumUnderlyingType();
#endif
        }

        public static Type GetGenericArgument(this Type type, int index)
        {
#if NETFX_CORE
            return type.GenericTypeArguments[index];
#else
            return type.GetGenericArguments()[index];
#endif
        }

        public static bool IsInheritInterface(this Type type, Type @interface)
        {
#if NETFX_CORE
            if (!@interface.GetTypeInfo().IsInterface)
                throw new ArgumentException(String.Format("The type '{0}' has to be an interface.", @interface.Name));

            return @interface.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(x => x == @interface) != null;
#else
            if (!@interface.IsInterface)
                throw new ArgumentException(String.Format("The type '{0}' has to be an interface.", @interface.Name));

            return type.GetInterfaces().FirstOrDefault(x => x == @interface) != null;
#endif
        }

        /// <summary>
        ///  Retrieves a collection that represents all the public fields and properties defined on a specified type.
        /// </summary>
        /// <param name="type">The type that contains the fields and properties.</param>
        /// <returns>A collection of fields and properties for the specified type.</returns>
        public static IEnumerable<MemberInfo> GetPublicReadWritePropertiesAndFields(this Type type)
        {
#if NETFX_CORE
            foreach (var field in type.GetRuntimeFields())
            {
                if (field.IsInitOnly)
                    continue;

                if (field.IsPublic)
                    yield return field;
            }

            foreach (var property in type.GetRuntimeProperties())
            {
                if (!property.CanRead)
                    continue;
                if (!property.CanWrite)
                    continue;

                yield return property;
            }
#else
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
#endif
        }

#if !NETFX_CORE

        public static bool IsNumeric(this Type type)
        {
            if (type.IsEnum())
                return false;

            switch ((int)Type.GetTypeCode(type))
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
#endif

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

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructor(new Type[] { }) != null;
        }
    }
}
