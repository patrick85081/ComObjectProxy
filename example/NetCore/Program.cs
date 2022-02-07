// See https://aka.ms/new-console-template for more information

using System.Collections;
using ComObjectProxy.Castle;
using WUApiLib;
using ComObjectProxy.Core;

// using NetCore;

Console.WriteLine("Hello, World!");

// ComProxyFactory.SetComProxyFactory(new StandardComProxyFactory());
ComProxyFactory.SetComProxyFactory(new CastleComProxyFactory());
ComProxyFactory.RegisterCollectionConverter<UpdateCollection, IUpdate>();
ComProxyFactory.RegisterCollectionConverter<ICategoryCollection, ICategory>();
ComProxyFactory.RegisterCollectionConverter<IUpdateExceptionCollection, IUpdateException>();

var search = ComProxyFactory.Create(new UpdateSession().CreateUpdateSearcher());
// var search = new UpdateSession().CreateUpdateSearcher();
var type = search.GetType();
var result = search.Search("");
var updates = result.Updates;

Console.ReadLine();