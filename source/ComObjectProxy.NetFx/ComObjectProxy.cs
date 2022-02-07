using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using ComObjectProxy.Core;

namespace ComObjectProxy.NetFx
{
    public class NetFxComProxyFactory : IComProxyFactory
    {
        public T Create<T>(T comObject)
        {
            return (T)new ComObjectProxy<T>(comObject).GetTransparentProxy();
        }

        public TCollection CreateCollection<TCollection, T>(TCollection comObject) where TCollection : IEnumerable
        {
            return (TCollection)new ComCollectionProxy<TCollection, T>(comObject).GetTransparentProxy();
        }

        public object Create(object comObject, Type targetType)
        {
            return this.GetType()
                .GetMethods()
                .Where(m => m.Name == nameof(Create) && m.IsGenericMethod)
                .FirstOrDefault()
                .MakeGenericMethod(targetType)
                .Invoke(this, new[] { comObject });
        }

        public object CreateCollection(object comObject, Type collectionType, Type elementType)
        {
            return this.GetType()
                .GetMethods()
                .Where(m => m.Name == nameof(CreateCollection) && m.IsGenericMethod)
                .FirstOrDefault()
                .MakeGenericMethod(new[] { collectionType, elementType })
                .Invoke(this, new object[] { comObject });
        }
    }

    internal class ComCollectionProxy<TCollection, T> : ComObjectProxy<TCollection> where TCollection : IEnumerable
    {
        public ComCollectionProxy(TCollection classToProxy) : base(classToProxy)
        {
        }

        protected override object InvokeMember(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name == nameof(IEnumerable.GetEnumerator) && typeof(TCollection).IsInterface)
            {
                return proxyImpl.GetProxyEnumerable<T>(base.instance as IEnumerable).GetEnumerator();
            }
            return base.InvokeMember(targetMethod, args);
        }
    }
    internal class ComObjectProxy<T> : RealProxy
    {
        protected T instance;
        public ComObjectProxy(T classToProxy) : base(typeof(T))
        {
            this.instance = classToProxy;
        }

        protected ProxyImpl proxyImpl = new ProxyImpl();
        public override IMessage Invoke(IMessage msg)
        {
            if (msg is IMethodCallMessage callMessage)
            {
                var result = InvokeMember(callMessage.MethodBase as MethodInfo, callMessage.Args);

                return new ReturnMessage(result, null, 0, callMessage.LogicalCallContext, callMessage);
            }

            return new ReturnMessage(new NotSupportedException(), null);
        }

        protected virtual object InvokeMember(MethodInfo targetMethod, object[] args)
        {
            var result = proxyImpl.InvokeMember(instance, targetMethod, args);

            if (
                result != null &&
                result.GetType().IsCOMObject &&
                targetMethod.ReturnType.IsInterface)
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