using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceInterfaces;
using System.Collections.Generic;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using ServiceFabric.Logging;
using ServiceFabric.Remoting.CustomHeaders;
using ServiceFabric.Remoting.CustomHeaders.ReliableServices;

namespace MyStateless
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class MyStateless : StatelessService, IMyService
    {
        private readonly ILogger _logger;
        
        public MyStateless(StatelessServiceContext context, ILogger logger)
            : base(context)
        {
            _logger = logger;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(context =>
                    new FabricTransportServiceRemotingListener(context,
                        new ExtendedServiceRemotingMessageDispatcher(context, this)))
            };
        }

        public async Task<int> CalculateSumAsync(int a, int b)
        {
            var traceId = RemotingContext.GetData(HeaderIdentifiers.TraceId);

            _logger.LogTrace($"Hello from inside {nameof(MyStateless)} (traceId {traceId})");

            await new HttpClient().GetAsync("http://www.nu.nl");

            return a + b;
        }
    }
}
