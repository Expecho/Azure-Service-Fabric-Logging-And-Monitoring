using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceFabric.Logging;
using ServiceInterfaces;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IServiceRemoting serviceRemoting;

        public ValuesController(ILogger logger, IServiceRemoting serviceRemoting)
        {
            this.serviceRemoting = serviceRemoting;
        }

        // GET api/values?a=1&b=2
        [HttpGet]
        public Task<int> Get(int a, int b)
        {
            var uri = new Uri($"{FabricRuntime.GetActivationContext().ApplicationName}/Stateless");
            return serviceRemoting.CallAsync<IMyService, int>(HttpContext.TraceIdentifier, uri, service => service.CalculateSum(a, b));
        }
    }
}
