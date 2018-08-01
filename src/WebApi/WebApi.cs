using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Logging;
using ServiceFabric.Logging.Extensions;
using ServiceFabric.Logging.Remoting;
using ServiceFabric.Remoting.CustomHeaders;

namespace WebApi
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class WebApi : StatelessService
    {
        private readonly ILogger _logger;

        public WebApi(StatelessServiceContext context, ILogger logger)
            : base(context)
        {
            _logger = logger;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new WebListenerCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        _logger.LogStatelessServiceStartedListening<WebApi>(url);

                        var webHost = new WebHostBuilder()
                            .UseKestrel()
                            .ConfigureServices(
                                services => services
                                    .AddHttpContextAccessor()
                                    .AddSingleton(serviceContext)
                                    .AddSingleton(_logger)
                                    .AddTransient<IProxyFactoryProvider, ProxyFactoryProvider>())
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                            .UseStartup<Startup>()
                            .UseUrls(url)
                            .Build();

                        return webHost;
                    }))
            };
        }
    }
}