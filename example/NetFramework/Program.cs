using System;
using System.Linq;
using ComObjectProxy.Castle;
using ComObjectProxy.Core;
using ComObjectProxy.NetFx;
using WUApiLib;

namespace NetFramework
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            // ComProxyFactory.SetComProxyFactory(new NetFxComProxyFactory());
            ComProxyFactory.SetComProxyFactory(new CastleComProxyFactory());
            ComProxyFactory.RegisterCollectionConverter<UpdateCollection, IUpdate>();
            ComProxyFactory.RegisterCollectionConverter<ICategoryCollection, ICategory>();
            ComProxyFactory.RegisterCollectionConverter<IUpdateExceptionCollection, IUpdateException>();

            var search = ComProxyFactory.Create(new UpdateSession().CreateUpdateSearcher());
            // var search = new UpdateSession().CreateUpdateSearcher();
            var result = search.Search("");
            var updates = result.Updates.Cast<IUpdate>().ToArray();

            Console.WriteLine();
        }
    }
}