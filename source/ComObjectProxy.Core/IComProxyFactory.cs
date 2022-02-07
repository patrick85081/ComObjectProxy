namespace ComObjectProxy.Core;

public interface IComProxyFactory
{
    object Create(object comObject, Type targetType);
    object CreateCollection(object comObject, Type collectionType, Type elementType);
}