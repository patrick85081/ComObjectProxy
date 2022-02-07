using System.Collections;

namespace ComObjectProxy.Core;

public class ComProxyFactory
{
    public static Dictionary<Type, Type> ConverterMap = new();
    private static IComProxyFactory factory = null;

    public static void SetComProxyFactory(IComProxyFactory factory)
    {
        ComProxyFactory.factory = factory;
    }
    public static void RegisterCollectionConverter<TCollection, TElement>() where TCollection : IEnumerable
    {
        ConverterMap[typeof(TCollection)] = typeof(TElement);
    }

    public static TCollection CreateCollection2<TCollection, T>(TCollection comObject)
        where TCollection : IEnumerable<T>
    {
        return (TCollection)CreateCollection(comObject, typeof(TCollection), typeof(T));
    }

    public static TCollection CreateCollection<TCollection, T>(TCollection comObject) where TCollection : IEnumerable
    {
        return (TCollection)CreateCollection(comObject, typeof(TCollection), typeof(T));
    }
    // public static TCollection CreateCollection2<TCollection, T>(TCollection comObject) where TCollection : IEnumerable<T>
    // {
    //     var proxy = ComCollectionProxy<TCollection, T>.Create<TCollection, ComCollectionProxy<TCollection, T>>();
    //     (proxy as ComCollectionProxy<TCollection, T>).SetTarget(comObject);
    //     
    //     return proxy;
    // }
    // public static TCollection CreateCollection<TCollection, T>(TCollection comObject) where TCollection : IEnumerable
    // {
    //     var proxy = ComCollectionProxy<TCollection, T>.Create<TCollection, ComCollectionProxy<TCollection, T>>();
    //     (proxy as ComCollectionProxy<TCollection, T>).SetTarget(comObject);
    //     
    //     return proxy;
    // }

    public static object CreateCollection(object comObject, Type collectionType, Type elementType)
    {
        return factory.CreateCollection(comObject, collectionType, elementType);
    }
    public static object Create(object comObject, Type targetType)
    {
        return factory.Create(comObject, targetType);
    }

    public static T Create<T>(T comObject)
    {
        return (T)Create(comObject, typeof(T));
    }
}