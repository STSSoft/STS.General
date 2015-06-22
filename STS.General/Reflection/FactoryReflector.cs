using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using STS.General.Extensions;
using System.Reflection;

namespace STS.General.Reflection
{
    public class FactoryReflector<T>
    {
#if !NETFX_CORE
        public static readonly Type[] EmptyTypes = Type.EmptyTypes;
#endif

        private Func<T> createInstanceDelegate;

        public FactoryReflector()
        {
            Type type = typeof(T);

            ConstructorInfo cst = null;

#if NETFX_CORE
            cst = type.GetConstructor();
#else
            cst = type.GetConstructor(EmptyTypes);
#endif

            Expression body;
            if (cst != null)
                body = Expression.New(type);
            else
            {
                bool isValueType;

#if NETFX_CORE
                isValueType = type.IsByRef;
#else
                isValueType = type.IsValueType;
#endif

                if (isValueType)
                    body = Expression.Constant(default(T));
                else
                    body = Expression.Convert(Expression.Constant(null), type);
            }

            var lambda = Expression.Lambda<Func<T>>(body);
            createInstanceDelegate = (Func<T>)lambda.Compile();
        }

        public T CreateInstance()
        {
            return createInstanceDelegate();
        }
    }

    public class FactoryReflector
    {
        private Func<object> createInstanceDelegate;

        public FactoryReflector(Type type)
        {
            if (null == type)
                throw new ArgumentNullException("type");

            ConstructorInfo cst = null;

#if NETFX_CORE
            cst = type.GetConstructor();
#else
            cst = type.GetConstructor(Type.EmptyTypes);
#endif

            if (cst == null)
            {
                var lambda = Expression.Lambda<Func<object>>(Expression.Constant(null));

                createInstanceDelegate = (Func<object>)lambda.Compile();
            }
            else
            {
                var obj = Expression.New(type);
                var lambda = Expression.Lambda<Func<object>>(obj);

                createInstanceDelegate = lambda.Compile();
            }
        }

        public object CreateInstance()
        {
            return createInstanceDelegate();
        }
    }
}
