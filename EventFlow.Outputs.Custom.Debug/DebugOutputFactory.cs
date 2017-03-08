using Microsoft.Diagnostics.EventFlow;
using Microsoft.Extensions.Configuration;

namespace EventFlow.Outputs.Custom.Debug
{
    public class DebugOutputFactory : IPipelineItemFactory<DebugOutput>
    {
        public DebugOutput CreateItem(IConfiguration configuration, IHealthReporter healthReporter)
        {
            return new DebugOutput(healthReporter);
        }
    }
}
