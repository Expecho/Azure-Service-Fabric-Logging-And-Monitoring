using System;
using System.Fabric;
using System.Threading.Tasks;
using System.Web.Http;
using Interface;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebApi.Controllers
{
    [ServiceRequestActionFilter]
    [RoutePrefix("api/demo")]
    public class DemoController : ApiController
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
                ServiceEventSource.Current.Message(ex.Message);
            }

            return string.Empty;
        }
    }
}
