using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetCore
{
    public class ComProxyFactory
    {
        internal static Dictionary<Type, Type> ConverterMap = new();
        public static void RegisterCollectionConverter<TCollection, TElement>() where TCollection : IEnumerable
        {
            ConverterMap[typeof(TCollection)] = typeof(TElement);
        }
        public static TCollection CreateCollection2<TCollection, T>(TCollection comObject) where TCollection : IEnumerable<T>
        {
            var proxy = ComCollectionProxy<TCollection, T>.Create<TCollection, ComCollectionProxy<TCollection, T>>();
            (proxy as ComCollectionProxy<TCollection, T>).SetTarget(comObject);
        
            return proxy;
        }
        public static TCollection CreateCollection<TCollection, T>(TCollection comObject) where TCollection : IEnumerable
        {
            var proxy = ComCollectionProxy<TCollection, T>.Create<TCollection, ComCollectionProxy<TCollection, T>>();
            (proxy as ComCollectionProxy<TCollection, T>).SetTarget(comObject);
        
            return proxy;
        }

        public static object CreateCollection(object comObject, Type collectionType, Type elementType)
        {
            var create = typeof(ComProxyFactory)
                .GetMethods()
                .Where(m => m.IsGenericMethod && m.IsStatic && m.Name == "CreateCollection")
                .FirstOrDefault();

            return create.MakeGenericMethod(collectionType, elementType)
                .Invoke(null, new[] { comObject });
        }
        public static object Create(object comObject, Type targetType)
        {
            var create = typeof(ComProxyFactory)
                .GetMethods()
                .Where(m => m.IsGenericMethod && m.IsStatic && m.Name == "Create")
                .FirstOrDefault();
            return create.MakeGenericMethod(targetType)
                .Invoke(null, new[] { comObject });
        }

        public static T Create<T>(T comObject)
        {
            var foo = ComObjectProxy<T>.Create<T, ComObjectProxy<T>>();
            (foo as ComObjectProxy<T>).SetTarget(comObject);

            return foo;
        }
    }

    public class ComCollectionProxy<TCollection, T> : ComObjectProxy<TCollection> where TCollection : IEnumerable
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod.Name == nameof(IEnumerable.GetEnumerator) && typeof(T).IsInterface)
            {
                return GetEnumerator().GetEnumerator();
            }

            return base.Invoke(targetMethod, args);
        }

        IEnumerable<T> GetEnumerator()
        {
            foreach (var arg in base.instance)
            {
                yield return (T)ComProxyFactory.Create(arg, typeof(T));
            }
        }
    }
    public class ComObjectProxy<T> : DispatchProxy
    {
        protected T instance;

        public static T Create(T comObject)
        {
            var foo = Create<T, ComObjectProxy<T>>();
            (foo as ComObjectProxy<T>).instance = comObject;

            return foo;
        }

        internal void SetTarget(T instance)
        {
            this.instance = instance;
        }

        public T GetTarget() => instance;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == instance.GetType().GetMethod("GetType"))
            {
                return typeof(T);
            }
            var (name, flags) = targetMethod switch
            {
                { Name.Length: > 0 } when targetMethod.Name.StartsWith("get_") =>
                    (targetMethod.Name.Substring(4), BindingFlags.GetProperty),
                { Name.Length: > 0 } when targetMethod.Name.StartsWith("set_") =>
                    (targetMethod.Name.Substring(4), BindingFlags.SetProperty),
                { Name.Length: > 0 } when targetMethod.Name.StartsWith("add_") =>
                    (targetMethod.Name.Substring(4), BindingFlags.InvokeMethod),
                { Name.Length: > 0 } when targetMethod.Name.StartsWith("remove_") =>
                    (targetMethod.Name.Substring(7), BindingFlags.InvokeMethod),
                _ =>
                    (targetMethod.Name, BindingFlags.InvokeMethod),
                // _ => throw new NotSupportedException()
            };
            var result = instance.GetType().InvokeMember(name, flags, null, instance, args);
            if (
                result is { } &&
                result.GetType() is { IsCOMObject: true } &&
                targetMethod.ReturnType is { IsInterface: true })
            {
                if (ComProxyFactory.ConverterMap.ContainsKey(targetMethod.ReturnType))
                {
                    var element = ComProxyFactory.ConverterMap[targetMethod.ReturnType];
                    return ComProxyFactory.CreateCollection(result, targetMethod.ReturnType, element);
                }
                return ComProxyFactory.Create(result, targetMethod.ReturnType);
            }

            return result;
        }
    }
}