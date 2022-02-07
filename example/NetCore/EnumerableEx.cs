using System.Collections.Generic;
using System.Linq;

namespace NetCore
{
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
            // var type = u.GetType();
            // var propertyNames = typeof(T).GetProperties().Select(x => x.Name);
            // return propertyNames.ToDictionary(
            //     p => p,
            //     p => type.InvokeMember(p, BindingFlags.GetProperty, null, u, null));
        }
    }
}