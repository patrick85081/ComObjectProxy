using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ComObjectProxy.Core
{
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