using System.Fabric;
using System.Globalization;
using System.Runtime.Remoting.Messaging;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using ServiceFabric.Logging.ApplicationInsights;
using ServiceFabric.Logging.PropertyMap;

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
            var configuration = new TelemetryConfiguration
            {
                InstrumentationKey = aiKey
            };

            configuration.TelemetryInitializers.Add(new OperationContextTelemetryInitializer(() =>
                CallContext.LogicalGetData(HeaderIdentifiers.TraceId)?.ToString()));

            new LiveStreamProvider(configuration).Enable();
            
            var loggerFactory = new LoggerFactory();
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo
                .ApplicationInsights(configuration, (logEvent, formatter) => new TelemetryBuilder(context, logEvent)
                .LogEventToTelemetryConverter())
                .CreateLogger();

            InitContextProperties();

            loggerFactory.AddSerilog(logger, true);

            return loggerFactory;
        }

        private void InitContextProperties()
        {
            LogContext.PushProperty(ServiceContextProperties.ServiceTypeName, context.ServiceTypeName);
            LogContext.PushProperty(ServiceContextProperties.ServiceName, context.ServiceName);
            LogContext.PushProperty(ServiceContextProperties.PartitionId, context.PartitionId);
            LogContext.PushProperty(ServiceContextProperties.NodeName, context.NodeContext.NodeName);
            LogContext.PushProperty(ServiceContextProperties.ApplicationName, context.CodePackageActivationContext.ApplicationName);
            LogContext.PushProperty(ServiceContextProperties.ApplicationTypeName, context.CodePackageActivationContext.ApplicationTypeName);
            LogContext.PushProperty(ServiceContextProperties.ServicePackageVersion, context.CodePackageActivationContext.CodePackageVersion);

            if (context is StatelessServiceContext)
            {
                LogContext.PushProperty(ServiceContextProperties.InstanceId, context.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture));
            }
            else if (context is StatefulServiceContext)
            {
                LogContext.PushProperty(ServiceContextProperties.ReplicaId, context.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}


