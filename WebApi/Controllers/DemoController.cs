using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Interface;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.Extensions.Logging;
using Interface.Logging;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class DemoController : Controller
    {
        private readonly ILogger logger;

        public DemoController(ILogger logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [Route("HelloWorld")]
        public async Task<string> HelloWorldAsync()
        {
            var activationContext = FabricRuntime.GetActivationContext();
           
            try
            {
                var uri = new Uri($"{activationContext.ApplicationName}/StatelessDemo");
                var service = ServiceProxy.Create<IStatelessDemo>(uri);

                logger.LogInformation(LoggingEvents.REQUEST, "Calling {uri}", uri);

                return await service.HelloWorldAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(LoggingEvents.EXCEPTION, ex, ex.Message);
                ServiceEventSource.Current.ServiceCriticalError(ex);
            }

            return string.Empty;
        }

        [HttpGet("CreateCriticalException")]
        public void CreateCriticalException()
        {
            var ex = new InvalidOperationException("demo exception", new ArgumentNullException("demo innerexception"));

            logger.LogCritical(LoggingEvents.EXCEPTION, ex, ex.Message);

            ServiceEventSource.Current.ServiceCriticalError(ex);
        }

        [HttpGet("CreateUnhandledException")]
        public void CreateUnhandledException()
        {
            throw new Exception("Breaking things");
        }
    }
}
