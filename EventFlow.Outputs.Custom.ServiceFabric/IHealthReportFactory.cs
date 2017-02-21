using System;
using System.Fabric.Health;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public interface IHealthReportFactory
    {
        HealthReport CreateHealthReport(Guid partitionId, long replicaOrInstanceId, HealthInformation healthInformation);
    }
}