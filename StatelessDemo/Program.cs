using System;
using System.Diagnostics;
using System.Threading;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog.Context;
using Serilog.Formatting.Json;

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
                    .Enrich.FromLogContext()
                    .WriteTo.Trace(new JsonFormatter())
                    .CreateLogger();

                loggerFactory.AddSerilog(logger, true);

                ServiceRuntime.RegisterServiceAsync("StatelessDemoType",
                    context => 
                    {
                        LogContext.PushProperty(nameof(context.ServiceTypeName), context.ServiceTypeName);
                        LogContext.PushProperty(nameof(context.ServiceName), context.ServiceName);
                        LogContext.PushProperty(nameof(context.PartitionId), context.PartitionId);
                        LogContext.PushProperty(nameof(context.ReplicaOrInstanceId), context.ReplicaOrInstanceId);
                        LogContext.PushProperty(nameof(context.NodeContext.NodeName), context.NodeContext.NodeName);
                        LogContext.PushProperty(nameof(context.CodePackageActivationContext.ApplicationName), context.CodePackageActivationContext.ApplicationName);
                        LogContext.PushProperty(nameof(context.CodePackageActivationContext.ApplicationTypeName), context.CodePackageActivationContext.ApplicationTypeName);
                        
                        return new StatelessDemo(context, loggerFactory.CreateLogger<StatelessDemo>());
                    }).GetAwaiter().GetResult();

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
