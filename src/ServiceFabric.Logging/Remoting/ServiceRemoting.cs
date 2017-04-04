using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Wcf.Client;
using ServiceFabric.Logging.Extensions;

namespace ServiceFabric.Logging.Remoting
{
    public class ServiceRemoting : IServiceRemoting
    {
        private readonly ILogger logger;

        public ServiceRemoting(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<TResult> CallAsync<TService, TResult>(string traceId, Uri uri, Expression<Func<TService, Task<TResult>>> callMethod) where TService : IService
        {
            var proxyFactory = new ServiceProxyFactory(c => new ServiceRemotingClientFactoryWrapper(traceId, 
                            new WcfServiceRemotingClientFactory(callbackClient: c)));

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
                logger.LogError((int)ServiceFabricEvent.Exception, exception, exception.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                logger.LogDependency(callMethod, started, stopwatch.Elapsed, success);
            }
        }

        public async Task CallAsync<TService>(string traceId, Uri uri, Expression<Func<TService, Task>> callMethod) where TService : IService
        {
            var proxyFactory = new ServiceProxyFactory(c => new ServiceRemotingClientFactoryWrapper(traceId,
                            new WcfServiceRemotingClientFactory(callbackClient: c)));

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
                logger.LogError((int)ServiceFabricEvent.Exception, exception, exception.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                logger.LogDependency(callMethod, started, stopwatch.Elapsed, success);
            }
        }
    }
}


