using System;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.EventFlow.ServiceFabric;
using Microsoft.ServiceFabric.Services.Runtime;
using Serilog.Context;
using Serilog;
using Microsoft.Extensions.Logging;
using Interface.Logging;
using Serilog.Formatting.Json;

namespace WebApi
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
                using (var pipeline = ServiceFabricDiagnosticPipelineFactory.CreatePipeline(
                            "WebApi-DiagnosticsPipeline"))
                {
                    var loggerFactory = new LoggerFactory();

                    var loggerImp = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Trace(new JsonFormatter())
                        .CreateLogger();

                    loggerFactory.AddSerilog(loggerImp, true);
                    
                    var logger = loggerFactory.CreateLogger<WebApi>();

                    ServiceRuntime.RegisterServiceAsync("WebApiType",
                    context =>
                    {
                        LogContext.PushProperty(nameof(context.ServiceTypeName), context.ServiceTypeName);
                        LogContext.PushProperty(nameof(context.ServiceName), context.ServiceName);
                        LogContext.PushProperty(nameof(context.PartitionId), context.PartitionId);
                        LogContext.PushProperty(nameof(context.ReplicaOrInstanceId), context.ReplicaOrInstanceId);
                        LogContext.PushProperty(nameof(context.NodeContext.NodeName), context.NodeContext.NodeName);
                        LogContext.PushProperty(nameof(context.CodePackageActivationContext.ApplicationName), context.CodePackageActivationContext.ApplicationName);
                        LogContext.PushProperty(nameof(context.CodePackageActivationContext.ApplicationTypeName), context.CodePackageActivationContext.ApplicationTypeName);

                        return new WebApi(context, logger);
                    }).GetAwaiter().GetResult();

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id,
                        typeof (WebApi).Name);

                    logger.LogTrace(LoggingEvents.SYSTEM_EVENT, "ServiceType {ServiceName} is registred in process with id {ProcessId}", typeof(WebApi).Name, Process.GetCurrentProcess().Id);

                    // Prevents this host process from terminating so services keeps running. 
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
