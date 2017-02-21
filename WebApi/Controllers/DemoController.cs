using System;
using System.Fabric;
using System.Web.Http;
using Interface;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebApi.Controllers
{
    [ServiceRequestActionFilter]
    [RoutePrefix("api/demo")]
    public class DemoController : ApiController
    {
        [HttpGet]
        [Route("HelloWorld")]
        public string HelloWorld()
        {
            var activationContext = FabricRuntime.GetActivationContext();
           
            try
            {
                var uri = new Uri($"fabric:/{activationContext.ApplicationName}/StatelessDemo");
                var service = ServiceProxy.Create<IStatelessDemo>(uri, new ServicePartitionKey(1));

                return service.HelloWorld();
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Message(ex.Message);
            }

            return string.Empty;
        }
    }
}
