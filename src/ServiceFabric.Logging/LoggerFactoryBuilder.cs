using System.Fabric;
using System.Globalization;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using ServiceFabric.Logging.ApplicationInsights;
using ServiceFabric.Logging.PropertyMap;
using ServiceFabric.Remoting.CustomHeaders;

namespace ServiceFabric.Logging
{
    public class LoggerFactoryBuilder : ILoggerFactoryBuilder
    {
        private readonly ServiceContext _context;

        public LoggerFactoryBuilder(ServiceContext context)
        {
            _context = context;
        }

        public ILoggerFactory CreateLoggerFactory(string aiKey)
        {
            var configuration = new TelemetryConfiguration
            {
                InstrumentationKey = aiKey
            };

            configuration.TelemetryInitializers.Add(new OperationContextTelemetryInitializer(() =>
                RemotingContext.GetData(HeaderIdentifiers.TraceId)?.ToString()));

            new LiveStreamProvider(configuration).Enable();
            
            var loggerFactory = new LoggerFactory();
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo
                .ApplicationInsights(
                    configuration,
                    (logEvent, formatter) => new TelemetryBuilder(_context, logEvent).LogEventToTelemetryConverter())
                .CreateLogger();

            InitContextProperties();

            loggerFactory.AddSerilog(logger, true);

            return loggerFactory;
        }

        private void InitContextProperties()
        {
            LogContext.PushProperty(ServiceContextProperties.ServiceTypeName, _context.ServiceTypeName);
            LogContext.PushProperty(ServiceContextProperties.ServiceName, _context.ServiceName);
            LogContext.PushProperty(ServiceContextProperties.PartitionId, _context.PartitionId);
            LogContext.PushProperty(ServiceContextProperties.NodeName, _context.NodeContext.NodeName);
            LogContext.PushProperty(ServiceContextProperties.ApplicationName, _context.CodePackageActivationContext.ApplicationName);
            LogContext.PushProperty(ServiceContextProperties.ApplicationTypeName, _context.CodePackageActivationContext.ApplicationTypeName);
            LogContext.PushProperty(ServiceContextProperties.ServicePackageVersion, _context.CodePackageActivationContext.CodePackageVersion);

            if (_context is StatelessServiceContext)
            {
                LogContext.PushProperty(ServiceContextProperties.InstanceId, _context.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture));
            }
            else if (_context is StatefulServiceContext)
            {
                LogContext.PushProperty(ServiceContextProperties.ReplicaId, _context.ReplicaOrInstanceId.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}


