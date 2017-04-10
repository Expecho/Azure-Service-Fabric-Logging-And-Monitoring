using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace ServiceFabric.Logging.ApplicationInsights
{
    public class LiveStreamProvider
    {
        private readonly TelemetryConfiguration configuration;

        public LiveStreamProvider(string instrumentationKey)
        {
            configuration = new TelemetryConfiguration()
            {
                InstrumentationKey = instrumentationKey
            };
        }

        public void Enable()
        {
            QuickPulseTelemetryProcessor processor = null;

            configuration.TelemetryProcessorChainBuilder
                .Use((next) =>
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
