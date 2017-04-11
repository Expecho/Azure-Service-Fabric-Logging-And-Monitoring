using System.Fabric;
using System.Runtime.Remoting.Messaging;
using Microsoft.ApplicationInsights.Extensibility;
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
            var configuration = new TelemetryConfiguration()
            {
                InstrumentationKey = aiKey
            };

            configuration.TelemetryInitializers.Add(new OperationContextTelemetryInitializer(() =>
                CallContext.LogicalGetData(HeaderIdentifiers.TraceId)?.ToString()));

            new LiveStreamProvider(configuration).Enable();
            
            var loggerFactory = new LoggerFactory();
            var logger = new LoggerConfiguration()
                .WriteTo
                .ApplicationInsights(configuration, (logEvent, formatter) => new TelemetryBuilder(context, logEvent, formatter)
                .LogEventToTelemetryConverter())
                .CreateLogger();

            loggerFactory.AddSerilog(logger, true);

            return loggerFactory;
        }
    }
}


