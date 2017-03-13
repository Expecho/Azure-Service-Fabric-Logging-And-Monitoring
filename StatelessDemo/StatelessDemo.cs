using Interface;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.Extensions.Logging;
using Interface.Logging;

namespace StatelessDemo
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class StatelessDemo : StatelessService, IStatelessDemo
    {
        private readonly ILogger logger;
        private int requestCount;

        public StatelessDemo(StatelessServiceContext context, ILogger logger)
            : base(context)
        {
            this.logger = logger;
            logger.LogInformation(LoggingEvents.SYSTEM_EVENT, nameof(StatelessDemo));
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            logger.LogInformation(LoggingEvents.SYSTEM_EVENT, nameof(CreateServiceInstanceListeners));
            return new[] { new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context)) };
        }

        public Task<string> HelloWorldAsync()
        {
            using (logger.BeginScope(nameof(HelloWorldAsync)))
            {
                logger.LogInformation(LoggingEvents.REQUEST, "Times requested: {requestCount}", ++requestCount);

                var metrics = new List<LoadMetric>
                {
                    new LoadMetric(nameof(requestCount), requestCount)
                };

                Partition.ReportLoad(metrics);
                return Task.FromResult("Hello World");
            }
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(LoggingEvents.SYSTEM_EVENT, nameof(OnOpenAsync));
            return base.OnOpenAsync(cancellationToken);
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation(LoggingEvents.SYSTEM_EVENT, nameof(OnCloseAsync));
            return base.OnCloseAsync(cancellationToken);
        }

        protected override void OnAbort()
        {
            logger.LogInformation(LoggingEvents.SYSTEM_EVENT, nameof(OnAbort));
            base.OnAbort();
        }
    }
}
