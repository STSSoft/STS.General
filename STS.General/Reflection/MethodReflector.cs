using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace STS.General.Reflection
{
    public class MethodReflector
    {
        private object[] callDelegates = new object[4];

        public Type OwnerType { get; private set; }
        public string MethodName { get; private set; }

        public MethodReflector(Type ownerType, string methodName, params Type[] arguments)
        {
            List<ParameterExpression> args = new List<ParameterExpression>(arguments.Length);
            List<Expression> cast_args = new List<Expression>(arguments.Length);

            MethodInfo methodInfo = ownerType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, null, arguments, null);
            var instance = Expression.Parameter(typeof(object), "instance");
            var cast_instance = Expression.Convert(instance, ownerType);
            args.Add(instance);
            for (int i = 0; i < arguments.Length; i++)
            {
                var arg = Expression.Parameter(typeof(object), "arg" + i);
                var cast_arg = Expression.Convert(arg, arguments[i]);
                args.Add(arg);
                cast_args.Add(cast_arg);
            }
            var call = Expression.Call(cast_instance, methodInfo, cast_args);
            if (methodInfo.ReturnType == typeof(void))
            {
                var lambda = Expression.Lambda(call, args.ToArray());
                callDelegates[arguments.Length] = lambda.Compile();
            }
            else
            {
                var converted = Expression.Convert(call, typeof(object));
                var lambda = Expression.Lambda(converted, args.ToArray());
                callDelegates[arguments.Length] = lambda.Compile();
            }
        }

        public void Call(object instance)
        {
            ((Action<object>)callDelegates[0])(instance);
        }

        public void Call(object instance, object argument0)
        {
            ((Action<object, object>)callDelegates[1])(instance, argument0);
        }

        public void Call(object instance, object argument0, object argument1)
        {
            ((Action<object, object, object>)callDelegates[2])(instance, argument0, argument1);
        }

        public void Call(object instance, object argument0, object argument1, object argument2)
        {
            ((Action<object, object, object, object>)callDelegates[3])(instance, argument0, argument1, argument2);
        }

        public object CallFunc(object instance)
        {
            return ((Func<object, object>)callDelegates[0])(instance);
        }

        public object CallFunc(object instance, object argument0)
        {
            return ((Func<object, object, object>)callDelegates[1])(instance, argument0);
        }

        public object CallFunc(object instance, object argument0, object argument1)
        {
            return ((Func<object, object, object, object>)callDelegates[2])(instance, argument0, argument1);
        }

        public object CallFunc(object instance, object argument0, object argument1, object argument2)
        {
            return ((Func<object, object, object, object, object>)callDelegates[3])(instance, argument0, argument1, argument2);
        }
    }
}
