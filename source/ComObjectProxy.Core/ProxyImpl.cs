using System.Collections;
using System.Reflection;

namespace ComObjectProxy.Core;

public class ProxyImpl
{
    public object InvokeMember(object target, MethodBase? targetMethod, object?[] args)
    {
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
        var result = target.GetType().InvokeMember(name, flags, null, target, args);
        return result;
    }

    public IEnumerable<T> GetProxyEnumerable<T>(IEnumerable instance) 
    {
        foreach (var arg in instance)
        {
            yield return (T)ComProxyFactory.Create(arg, typeof(T));
        }
    }
}