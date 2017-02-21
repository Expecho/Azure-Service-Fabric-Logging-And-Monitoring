using Microsoft.Diagnostics.EventFlow;
using Microsoft.Extensions.Configuration;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public class StatefulServiceHealthOutputFactory : IPipelineItemFactory<StatefulServiceHealthOutput>
    {
        public StatefulServiceHealthOutput CreateItem(IConfiguration configuration, IHealthReporter healthReporter)
        {
            return new StatefulServiceHealthOutput(healthReporter);
        }
    }
}