using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace ServiceFabric.Logging.ApplicationInsights
{
    public class LiveStreamProvider
    {
        private readonly TelemetryConfiguration configuration;

        public LiveStreamProvider(TelemetryConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Enable()
        {
            var module = new DependencyTrackingTelemetryModule();
            module.Initialize(configuration);

            QuickPulseTelemetryProcessor processor = null;

            configuration.TelemetryProcessorChainBuilder
                .Use(next =>
                {
                    processor = new QuickPulseTelemetryProcessor(next);
                    return processor;
                })
                .Build();

            var quickPulse = new QuickPulseTelemetryModule();
            quickPulse.Initialize(configuration);
            quickPulse.RegisterTelemetryProcessor(processor);
        }
    }
}
