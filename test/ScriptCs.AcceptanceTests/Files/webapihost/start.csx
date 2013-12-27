#load event.csx
#load "webapiconfig.csx"
#load "testcontroller.csx"

using System.Reflection;
using System.IO;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Dispatcher;
using System.Threading;

var server = new HttpSelfHostServer(conf);
server.OpenAsync().Wait();
Console.WriteLine("Listening...");
EventHelper.FinishEvent.Wait();
Thread.Sleep(4000);