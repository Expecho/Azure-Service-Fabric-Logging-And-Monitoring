using Microsoft.Diagnostics.EventFlow;
using Microsoft.Extensions.Configuration;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public class StatelessServiceHealthOutputFactory : IPipelineItemFactory<StatelessServiceHealthOutput>
    {
        public StatelessServiceHealthOutput CreateItem(IConfiguration configuration, IHealthReporter healthReporter)
        {
            return new StatelessServiceHealthOutput(healthReporter);
        }
    }
}