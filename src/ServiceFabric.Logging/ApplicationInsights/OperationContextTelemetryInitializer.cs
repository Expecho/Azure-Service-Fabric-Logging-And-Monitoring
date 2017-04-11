using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace ServiceFabric.Logging.ApplicationInsights
{
    public class OperationContextTelemetryInitializer : ITelemetryInitializer
    {
        private readonly Func<string> operationIdProvider;

        public OperationContextTelemetryInitializer(Func<string> operationIdProvider)
        {
            this.operationIdProvider = operationIdProvider;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Operation.Id = operationIdProvider.Invoke();
            telemetry.Context.Operation.ParentId = operationIdProvider.Invoke();

            if(telemetry.Context.Operation.Name == null)
                telemetry.Context.Operation.Name = Guid.NewGuid().ToString();
        }
    }
}