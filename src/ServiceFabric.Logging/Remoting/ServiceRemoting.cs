using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using ServiceFabric.Logging.Extensions;
using ServiceFabric.Remoting.CustomHeaders;

namespace ServiceFabric.Logging.Remoting
{
    public class ServiceRemoting : IServiceRemoting
    {
        private readonly ILogger _logger;

        public ServiceRemoting(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TResult> CallAsync<TService, TResult>(
            CustomHeaders customHeaders,
            Uri uri,
            Expression<Func<TService, Task<TResult>>> callMethod) where TService : IService
        {
            var proxyFactory = new ServiceProxyFactory(c => new ExtendedServiceRemotingClientFactory(new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: c), customHeaders));
            var service = proxyFactory.CreateServiceProxy<TService>(uri);
            var success = true;

            var stopwatch = Stopwatch.StartNew();
            var started = DateTime.Now;

            try
            {
                return await callMethod.Compile().Invoke(service);
            }
            catch (Exception exception)
            {
                success = false;
                _logger.LogError(ServiceFabricEvent.Exception, exception, exception.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogDependency(callMethod, started, stopwatch.Elapsed, success);
            }
        }

        public async Task CallAsync<TService>(
            CustomHeaders customHeaders,
            Uri uri,
            Expression<Func<TService, Task>> callMethod) where TService : IService
        {
            var proxyFactory = new ServiceProxyFactory(c => new ExtendedServiceRemotingClientFactory(new FabricTransportServiceRemotingClientFactory(remotingCallbackMessageHandler: c), customHeaders));
            var service = proxyFactory.CreateServiceProxy<TService>(uri);
            var success = true;

            var stopwatch = Stopwatch.StartNew();
            var started = DateTime.Now;

            try
            {
                await callMethod.Compile().Invoke(service);
            }
            catch (Exception exception)
            {
                success = false;
                _logger.LogError(ServiceFabricEvent.Exception, exception, exception.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogDependency(callMethod, started, stopwatch.Elapsed, success);
            }
        }
    }
}


