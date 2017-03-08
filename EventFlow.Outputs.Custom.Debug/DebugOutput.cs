using Microsoft.Diagnostics.EventFlow;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System;

namespace EventFlow.Outputs.Custom.Debug
{
    public class DebugOutput : IOutput
    {
        public DebugOutput(IHealthReporter healthReporter)
        {

        }

        public Task SendEventsAsync(IReadOnlyCollection<EventData> events, long transmissionSequenceNumber, CancellationToken cancellationToken)
        {
            var eventDescriptions = events
                .Where(e => e.ProviderName != "System.Threading.Tasks.TplEventSource")
                .Select(e => $"DebugOutput: {e.ProviderName} - {string.Join(", ", e.Payload.Select(v => $"{v.Key}: {v.Value}"))}");

            foreach (var eventDescription in eventDescriptions)
            {
                System.Diagnostics.Debug.WriteLine(eventDescription);
            }
            
            return Task.FromResult(true);
        }
    }
}
