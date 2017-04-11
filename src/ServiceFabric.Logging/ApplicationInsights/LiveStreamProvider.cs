using System;
using System.Fabric;
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
            var profilerAssembliesPath = FabricRuntime.GetActivationContext().GetCodePackageObject("Code").Path;

            Environment.SetEnvironmentVariable("COR_ENABLE_PROFILING", "1", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("COR_PROFILER", "{324F817A-7420-4E6D-B3C1-143FBED6D855}", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("COR_PROFILER_PATH", $@"{profilerAssembliesPath}\MicrosoftInstrumentationEngine_x86.dll", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("MicrosoftInstrumentationEngine_Host", "{CA487940-57D2-10BF-11B2-A3AD5A13CBC0}", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("MicrosoftInstrumentationEngine_HostPATH", $@"{profilerAssembliesPath}\Microsoft.ApplicationInsights.ExtensionsHost_x86.dll", EnvironmentVariableTarget.Process);

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
