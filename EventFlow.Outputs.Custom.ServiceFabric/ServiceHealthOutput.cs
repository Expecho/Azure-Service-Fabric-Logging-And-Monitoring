using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Health;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Outputs.Custom.ServiceFabric.Common;
using Microsoft.Diagnostics.EventFlow;

namespace EventFlow.Outputs.Custom.ServiceFabric
{
    public class ServiceHealthOutput<T> where T : IHealthReportFactory
    {
        private readonly IHealthReporter healthReporter;
        private readonly T healtReportFactory;
        private readonly FabricClient.HealthClient healthClient;

        public ServiceHealthOutput(IHealthReporter healthReporter, T healtReportFactory)
        {
            this.healthReporter = healthReporter;
            this.healtReportFactory = healtReportFactory;
            healthClient = new FabricClient
            {
                Settings =
                {
                    HealthReportSendInterval = TimeSpan.FromSeconds(5)
                }
            }.HealthManager;
        }

        public Task SendEventsAsync(IReadOnlyCollection<EventData> events, long transmissionSequenceNumber, CancellationToken cancellationToken)
        {
            foreach (var eventData in events)
            {
                try
                {
                    object eventName;
                    if (!eventData.TryGetPropertyValue("EventName", out eventName))
                        eventName = "Undefined";

                    var information = new HealthInformation(
                        eventData.ProviderName,
                        eventName.ToString(),
                        Helper.CreateHealthStateFromLevel(eventData.Level))
                    {
                        Description = string.Join(Environment.NewLine, eventData.Payload.Select(v => $"{v.Key}: {v.Value}"))
                    };

                    long replicaOrInstanceId;
                    Guid partitionId;
                    if (EventDataValidator.TryGetServiceContext(eventData, out replicaOrInstanceId, out partitionId))
                    {
                        healthReporter.ReportWarning($"{nameof(ServiceHealthOutput<T>)} skipped an event due to missing metadata.",
                            EventFlowContextIdentifiers.Configuration);
                        continue;
                    }

                    var report = healtReportFactory.CreateHealthReport(
                        partitionId,
                        replicaOrInstanceId,
                        information);

                    healthClient.ReportHealth(report);
                }
                catch (Exception ex)
                {
                    healthReporter.ReportWarning($"{nameof(ServiceHealthOutput<T>)} dropped an event due to error: {ex.Message}.",
                        EventFlowContextIdentifiers.Configuration);
                    throw;
                }
            }

            return Task.CompletedTask;
        }
    }
}