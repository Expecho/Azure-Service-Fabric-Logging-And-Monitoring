using System;
using System.Fabric.Health;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public class StatelessServiceInstanceHealthReportFactory : IHealthReportFactory
    {
        public HealthReport CreateHealthReport(Guid partitionId, long replicaOrInstanceId, HealthInformation healthInformation)
        {
            return new StatelessServiceInstanceHealthReport(partitionId, replicaOrInstanceId, healthInformation);
        }
    }
}