using System.Fabric.Health;
using Microsoft.Diagnostics.EventFlow;

namespace EventFlow.Outputs.Custom.ServiceFabric.Common
{
    internal class Helper
    {
        internal static HealthState CreateHealthStateFromLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    return HealthState.Error;
                case LogLevel.Warning:
                    return HealthState.Warning;
                case LogLevel.Informational:
                case LogLevel.Verbose:
                    return HealthState.Ok;
                default:
                    return HealthState.Unknown;
            }
        }
    }
}
