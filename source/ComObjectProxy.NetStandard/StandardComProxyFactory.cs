using System.Collections;

namespace ComObjectProxy.Core;

public class StandardComProxyFactory : IComProxyFactory
{
    public TCollection CreateCollection2<TCollection, T>(TCollection comObject) where TCollection : IEnumerable<T>
    {
        var proxy = ComCollectionProxy<TCollection, T>.Create<TCollection, ComCollectionProxy<TCollection, T>>();
        (proxy as ComCollectionProxy<TCollection, T>).SetTarget(comObject);
        
        return proxy;
    }
    public TCollection CreateCollection<TCollection, T>(TCollection comObject) where TCollection : IEnumerable
    {
        var proxy = ComCollectionProxy<TCollection, T>.Create<TCollection, ComCollectionProxy<TCollection, T>>();
        (proxy as ComCollectionProxy<TCollection, T>).SetTarget(comObject);
        
        return proxy;
    }
    public object CreateCollection(object comObject, Type collectionType, Type elementType)
    {
        var create = typeof(StandardComProxyFactory)
            .GetMethods()
            .Where(m => m.IsGenericMethod &&  m.Name == "CreateCollection")
            .FirstOrDefault();

        return create.MakeGenericMethod(collectionType, elementType)
            .Invoke(this, new[] { comObject });
    }
        
    public object Create(object comObject, Type targetType)
    {
        var create = typeof(StandardComProxyFactory)
            .GetMethods()
            .Where(m => m.IsGenericMethod &&  m.Name == "Create")
            .FirstOrDefault();
        return create.MakeGenericMethod(targetType)
            .Invoke(this, new[] { comObject });
    }
        
    public T Create<T>(T comObject)
    {
        var foo = ComObjectProxy<T>.Create<T, ComObjectProxy<T>>();
        (foo as ComObjectProxy<T>).SetTarget(comObject);

        return foo;
    }
}