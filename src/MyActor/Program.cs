using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors.Runtime;
using ServiceFabric.Logging;
using ServiceFabric.Logging.Extensions;
using System.Fabric;
using ServiceFabric.Remoting.CustomHeaders.Actors;

namespace MyActor
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
                // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see https://aka.ms/servicefabricactorsplatform

                ActorRuntime.RegisterActorAsync<MyActor>((context, actorType) =>
                    {
                        var applicationInsightsKey = FabricRuntime.GetActivationContext()
                            .GetConfigurationPackageObject("Config")
                            .Settings
                            .Sections["ConfigurationSection"]
                            .Parameters["ApplicationInsightsKey"]
                            .Value;

                        var loggerFactory = new LoggerFactoryBuilder(context).CreateLoggerFactory(applicationInsightsKey);
                        logger = loggerFactory.CreateLogger<MyActor>();

                        return new ExtendedActorService(context, actorType, (service, id) => new MyActor(service, id, logger));
                    }).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                logger?.LogActorHostInitalizationFailed<MyActor>(e);
                throw;
            }
        }
    }
}
