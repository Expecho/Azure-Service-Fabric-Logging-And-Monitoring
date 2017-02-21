using Microsoft.Diagnostics.EventFlow;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public class StatelessServiceHealthOutput : ServiceHealthOutput<StatelessServiceInstanceHealthReportFactory>, IOutput
    {
        public StatelessServiceHealthOutput(IHealthReporter healthReporter) : base(healthReporter, new StatelessServiceInstanceHealthReportFactory())
        {
            
        }
    }
}
