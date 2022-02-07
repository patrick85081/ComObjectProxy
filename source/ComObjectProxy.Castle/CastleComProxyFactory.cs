using Castle.DynamicProxy;
using ComObjectProxy.Core;

namespace ComObjectProxy.Castle;

public class CastleComProxyFactory : IComProxyFactory
{
    public object Create(object comObject, Type targetType)
    {
        // throw new NotImplementedException();
        return new ProxyGenerator().CreateInterfaceProxyWithTargetInterface(targetType, comObject, new ComObjectProxy());
    }

    public object CreateCollection(object comObject, Type collectionType, Type elementType)
    {
        var interceptor = typeof(ComCollectionProxy<>)
            .MakeGenericType(elementType)
            .GetConstructor(Type.EmptyTypes)
            .Invoke(new object[0]) as IInterceptor;
        return new ProxyGenerator().CreateInterfaceProxyWithTargetInterface(collectionType, comObject, interceptor);
    }
}