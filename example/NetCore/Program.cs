// See https://aka.ms/new-console-template for more information

using WUApiLib;
using ComObjectProxy.Core;
// using NetCore;

Console.WriteLine("Hello, World!");

ComProxyFactory.SetComProxyFactory(new StandardComProxyFactory());
ComProxyFactory.RegisterCollectionConverter<UpdateCollection, IUpdate>();
ComProxyFactory.RegisterCollectionConverter<ICategoryCollection, ICategory>();
ComProxyFactory.RegisterCollectionConverter<IUpdateExceptionCollection, IUpdateException>();

var search = ComProxyFactory.Create(new UpdateSession().CreateUpdateSearcher());
var result = search.Search("");
var updates = result.Updates.Cast<IUpdate>().ToArray();

Console.WriteLine();