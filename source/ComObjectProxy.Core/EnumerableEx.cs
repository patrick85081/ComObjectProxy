namespace ComObjectProxy.Core;

internal static class EnumerableEx
{
    public static IEnumerable<T> SelectWrapper<T>(this IEnumerable<T> u)
    {
        return u.OfType<T>().Select(GetWrapper<T>);
    }

    private static T GetWrapper<T>(T u)
    {
        if (u.GetType().IsCOMObject)
        {
            return ComProxyFactory.Create(u);
        }

        return u;
    }
}