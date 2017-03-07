using System;
using System.Diagnostics;
using Raven.Client;
using Raven.Client.Embedded;
using Raven.Client.Indexes;

Console.WriteLine("Starting RavenDB Server, Please Be Patient...");

var documentStore = new EmbeddableDocumentStore
{
    DataDirectory = "c:/scriptcs/ravendb",
    UseEmbeddedHttpServer = true,
}.Initialize() as EmbeddableDocumentStore;

var url = string.Format("http://localhost:{0}", documentStore.Configuration.Port);
Console.WriteLine("RavenDB Started, Listening On {0}", url);
Process.Start(url);

Console.ReadKey();