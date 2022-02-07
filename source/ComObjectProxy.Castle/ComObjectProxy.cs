using System.Collections;
using Castle.DynamicProxy;
using ComObjectProxy.Core;

namespace ComObjectProxy.Castle;

public class ComCollectionProxy<TElement> : ComObjectProxy
{
    public override void Intercept(IInvocation invocation)
    {
        if (invocation.Method.Name == nameof(IEnumerable.GetEnumerator))
        {
            var proxyEnumerable = proxyImpl.GetProxyEnumerable<TElement>((IEnumerable)invocation.InvocationTarget);
            invocation.ReturnValue = proxyEnumerable.GetEnumerator();
            return;
        }
        
        base.Intercept(invocation);
    }
}
public class ComObjectProxy : IInterceptor
{
    protected ProxyImpl proxyImpl = new ProxyImpl();

    public virtual void Intercept(IInvocation invocation)
    {
        var result = proxyImpl.InvokeMember(invocation.InvocationTarget, invocation.Method, invocation.Arguments);

        var targetMethod = invocation.Method;
        if (
            result is { } &&
            result.GetType() is { IsCOMObject: true } &&
            targetMethod.ReturnType is { IsInterface: true })
        {
            if (ComProxyFactory.ConverterMap.ContainsKey(targetMethod.ReturnType))
            {
                var element = ComProxyFactory.ConverterMap[targetMethod.ReturnType];
                result = ComProxyFactory.CreateCollection(result, targetMethod.ReturnType, element);
            }

            result = ComProxyFactory.Create(result, targetMethod.ReturnType);
        }

        invocation.ReturnValue = result;
    }
}