using System;
using System.Fabric.Health;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public class StatefulServiceReplicaHealthReportFactory : IHealthReportFactory
    {
        public HealthReport CreateHealthReport(Guid partitionId, long replicaOrInstanceId, HealthInformation healthInformation)
        {
            return new StatefulServiceReplicaHealthReport(partitionId, replicaOrInstanceId, healthInformation);
        }
    }
}