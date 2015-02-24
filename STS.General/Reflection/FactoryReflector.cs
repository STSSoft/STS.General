using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace STS.General.Reflection
{
    public class FactoryReflector<T>
    {
        public static readonly Type[] EmptyTypes = Type.EmptyTypes;

        private Func<T> createInstanceDelegate;

        public FactoryReflector()
        {
            Type type = typeof(T);

            Expression body;
            if (type.GetConstructor(EmptyTypes) != null)
                body = Expression.New(type);
            else
            {
                if (type.IsValueType)
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

            if (type.GetConstructor(Type.EmptyTypes) == null)
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
