using System;
using Microsoft.Diagnostics.EventFlow;

namespace EventFlow.Outputs.Custom.ServiceFabric.Common
{
    internal class EventDataValidator
    {
        internal static bool TryGetServiceContext(EventData eventData, out long replicaOrInstanceId, out Guid partitionId)
        {
            object replicaOrInstanceIdValue;
            replicaOrInstanceId = 0;
            object partitionIdValue;
            partitionId = Guid.Empty;

            if (!eventData.TryGetPropertyValue("replicaOrInstanceId", out replicaOrInstanceIdValue)) return true;
            if (!eventData.TryGetPropertyValue("partitionId", out partitionIdValue)) return true;
            if (!Guid.TryParse(partitionIdValue.ToString(), out partitionId)) return true;
            if (!long.TryParse(replicaOrInstanceIdValue.ToString(), out replicaOrInstanceId)) return true;

            return false;
        }
    }
}