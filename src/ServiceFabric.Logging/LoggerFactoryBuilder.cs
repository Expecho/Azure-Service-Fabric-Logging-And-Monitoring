using System.Fabric;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using ServiceFabric.Logging.ApplicationInsights;

namespace ServiceFabric.Logging
{
    public class LoggerFactoryBuilder : ILoggerFactoryBuilder
    {
        private readonly ServiceContext context;

        public LoggerFactoryBuilder(ServiceContext context)
        {
            this.context = context;
        }

        public ILoggerFactory CreateLoggerFactory(string aiKey)
        {
            var loggerFactory = new LoggerFactory();
            var logger = new LoggerConfiguration()
                .WriteTo
                .ApplicationInsights(aiKey, (logEvent, formatter) => new TelemetryBuilder(context, logEvent, formatter)
                .LogEventToTelemetryConverter())
                .CreateLogger();

            LogContext.PushProperty(nameof(context.ServiceTypeName), context.ServiceTypeName);
            LogContext.PushProperty(nameof(context.ServiceName), context.ServiceName);
            LogContext.PushProperty(nameof(context.PartitionId), context.PartitionId);
            LogContext.PushProperty(nameof(context.ReplicaOrInstanceId), context.ReplicaOrInstanceId);
            LogContext.PushProperty(nameof(context.NodeContext.NodeName), context.NodeContext.NodeName);
            LogContext.PushProperty(nameof(context.CodePackageActivationContext.ApplicationName), context.CodePackageActivationContext.ApplicationName);
            LogContext.PushProperty(nameof(context.CodePackageActivationContext.ApplicationTypeName), context.CodePackageActivationContext.ApplicationTypeName);
            LogContext.PushProperty(nameof(context.CodePackageActivationContext.CodePackageVersion), context.CodePackageActivationContext.CodePackageVersion);

            loggerFactory.AddSerilog(logger, true);

            return loggerFactory;
        }
    }
}


