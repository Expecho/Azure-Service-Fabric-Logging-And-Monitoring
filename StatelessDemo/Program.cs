using System;
using System.Diagnostics;
using System.Threading;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Runtime;

namespace StatelessDemo
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.
                var loggerFactory = new LoggerFactory();

                var logger = new LoggerConfiguration()
                    .WriteTo.Trace()
                    .WriteTo.Observers((events) => events.Subscribe(e =>
                    {
                        Debugger.Break();   
                    }))
                    .CreateLogger();

                loggerFactory.AddSerilog(logger, true);

                ServiceRuntime.RegisterServiceAsync("StatelessDemoType",
                    context => new StatelessDemo(context, loggerFactory.CreateLogger<StatelessDemo>())).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(StatelessDemo).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);

                logger.Dispose();
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
