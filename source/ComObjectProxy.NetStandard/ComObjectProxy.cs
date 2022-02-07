using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ComObjectProxy.Core
{
    internal class ComCollectionProxy<TCollection, T> : ComObjectProxy<TCollection> where TCollection : IEnumerable
    {
        private ProxyImpl proxyImpl = new ProxyImpl();
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod.Name == nameof(IEnumerable.GetEnumerator) && typeof(T).IsInterface)
            {
                Console.WriteLine($"{typeof(T).Name} call {targetMethod.Name} Begin");
                var result = proxyImpl.GetProxyEnumerable<T>(base.instance).GetEnumerator();
                Console.WriteLine($"{typeof(T).Name} call {targetMethod.Name} End: {targetMethod.ReturnType.FullName}");
                return result;
            }

            return base.Invoke(targetMethod, args);
        }
    }

    internal class ComObjectProxy<T> : DispatchProxy
    {
        protected T instance;
        private readonly ProxyImpl proxyImpl = new ProxyImpl();

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
            Console.WriteLine($"{typeof(T).Name} call {targetMethod.Name} Begin");
            object result = proxyImpl.InvokeMember(instance, targetMethod, args);
            if (
                result is { } &&
                result.GetType() is { IsCOMObject: true } &&
                targetMethod.ReturnType is { IsInterface: true })
            {
                if (ComProxyFactory.ConverterMap.ContainsKey(targetMethod.ReturnType))
                {
                    var element = ComProxyFactory.ConverterMap[targetMethod.ReturnType];
                    var temp = ComProxyFactory.CreateCollection(result, targetMethod.ReturnType, element);
                    Console.WriteLine(
                        $"{typeof(T).Name} call {targetMethod.Name} End: {temp}/{targetMethod.GetType().Name}");
                    return temp;
                }
                var tt = ComProxyFactory.Create(result, targetMethod.ReturnType);
                Console.WriteLine($"{typeof(T).Name} call {targetMethod.Name} End: {tt}/{targetMethod.GetType().Name}");
                return tt;
            }
            
            Console.WriteLine($"{typeof(T).Name} call {targetMethod.Name} End: {result}/{targetMethod.GetType().Name}");
            return result;
        }
    }
}