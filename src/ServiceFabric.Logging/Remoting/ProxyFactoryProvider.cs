using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using ServiceFabric.Logging.Extensions;
using ServiceFabric.Remoting.CustomHeaders;

namespace ServiceFabric.Logging.Remoting
{
    /// <summary>
    /// Provides create method to create proxy factories with built-in teacing and logging
    /// </summary>
    public class ProxyFactoryProvider : IProxyFactoryProvider
    {
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Func<CustomHeaders> _customHeadersProvider;

        /// <summary>
        /// Create an instance
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> implementation to use for logging</param>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/> implementation to get the call trace id from</param>
        public ProxyFactoryProvider(ILogger logger, IHttpContextAccessor httpContextAccessor)
        {
            this._logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _customHeadersProvider = () => new CustomHeaders
            {
                {HeaderIdentifiers.TraceId, _httpContextAccessor.HttpContext.TraceIdentifier}
            };
        }

        /// <summary>
        /// Create an instance of <see cref="ServiceProxyFactory"/> with built-in tracing and logging
        /// </summary>
        /// <returns>An instance of <see cref="ServiceProxyFactory"/> with built-in tracing and logging</returns>
        public IServiceProxyFactory CreateServiceProxyFactory()
        {
            return new ServiceProxyFactory(handler =>
                new ExtendedServiceRemotingClientFactory(
                    new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: handler), _customHeadersProvider)
                {
                    BeforeSendRequestResponseAsync = requestInfo =>
                    {
                        var stopwatch = Stopwatch.StartNew();
                        return Task.FromResult<object>(stopwatch);
                    },
                    AfterSendRequestResponseAsync = responseInfo =>
                    {
                        var stopwatch = (Stopwatch)responseInfo.State;

                        if(responseInfo.Exception != null)
                            _logger.LogError(ServiceFabricEvent.Exception, responseInfo.Exception, responseInfo.Exception.Message);

                        var elapsed = stopwatch.ElapsedMilliseconds;
                        _logger.LogDependency(
                            responseInfo.Service.ToString(), 
                            responseInfo.Method, 
                            DateTime.Now.AddMilliseconds(elapsed * -1), 
                            TimeSpan.FromMilliseconds(elapsed), 
                            responseInfo.Exception == null);

                        return Task.CompletedTask;
                    }
                });
        }

        /// <summary>
        /// Create an instance of <see cref="ActorProxyFactory"/> with built-in tracing and logging
        /// </summary>
        /// <returns>An instance of <see cref="ActorProxyFactory"/> with built-in tracing and logging</returns>
        public IActorProxyFactory CreateActorProxyFactory()
        {
            return new ActorProxyFactory(handler =>
                new ExtendedServiceRemotingClientFactory(
                    new FabricTransportActorRemotingClientFactory(handler), _customHeadersProvider)
                {
                    BeforeSendRequestResponseAsync = requestInfo =>
                    {
                        var stopwatch = Stopwatch.StartNew();
                        return Task.FromResult<object>(stopwatch);
                    },
                    AfterSendRequestResponseAsync = responseInfo =>
                    {
                        var stopwatch = (Stopwatch)responseInfo.State;

                        if (responseInfo.Exception != null)
                            _logger.LogError(ServiceFabricEvent.Exception, responseInfo.Exception, responseInfo.Exception.Message);

                        var elapsed = stopwatch.ElapsedMilliseconds;
                        _logger.LogDependency(
                            responseInfo.Service.ToString(),
                            responseInfo.Method,
                            DateTime.Now.AddMilliseconds(elapsed * -1),
                            TimeSpan.FromMilliseconds(elapsed),
                            responseInfo.Exception == null);

                        return Task.CompletedTask;
                    }
                });
        }
    }
}
