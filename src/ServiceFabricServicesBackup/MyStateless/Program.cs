using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Logging;
using System;
using System.Fabric;
using System.Threading;
using ServiceFabric.Logging.Extensions;

namespace MyStateless
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            ILogger logger = null;

            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync("MyStatelessType",
                    context =>
                    {
                        var applicationInsightsKey = FabricRuntime.GetActivationContext()
                            .GetConfigurationPackageObject("Config")
                            .Settings
                            .Sections["ConfigurationSection"]
                            .Parameters["ApplicationInsightsKey"]
                            .Value;

                        var loggerFactory = new LoggerFactoryBuilder(context).CreateLoggerFactory(applicationInsightsKey);
                        logger = loggerFactory.CreateLogger<MyStateless>();

                        return new MyStateless(context, logger);
                    }).GetAwaiter().GetResult();

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                logger?.LogStatelessServiceInitalizationFailed<MyStateless>(e);
                throw;
            }
        }
    }
}
