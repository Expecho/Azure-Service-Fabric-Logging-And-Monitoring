using System;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Interface;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class DemoController : Controller
    {
        [HttpGet]
        [Route("HelloWorld")]
        public async Task<string> HelloWorldAsync()
        {
            var activationContext = FabricRuntime.GetActivationContext();
           
            try
            {
                var uri = new Uri($"{activationContext.ApplicationName}/StatelessDemo");
                var service = ServiceProxy.Create<IStatelessDemo>(uri);

                return await service.HelloWorldAsync();
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceCriticalError(ex);
            }

            return string.Empty;
        }

        [HttpGet("CreateCriticalException")]
        public void CreateCriticalException()
        {
            ServiceEventSource.Current.ServiceCriticalError(new InvalidOperationException("demo exception", new ArgumentNullException("demo innerexception")));
        }

        [HttpGet("CreateUnhandledException")]
        public void CreateUnhandledException()
        {
            throw new Exception("Breaking things");
        }
    }
}
