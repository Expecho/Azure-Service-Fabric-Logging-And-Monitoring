using Microsoft.Diagnostics.EventFlow;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public class StatefulServiceHealthOutput : ServiceHealthOutput<StatefulServiceReplicaHealthReportFactory>, IOutput
    {
        public StatefulServiceHealthOutput(IHealthReporter healthReporter) : base(healthReporter, new StatefulServiceReplicaHealthReportFactory())
        {

        }
    }
}