using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceInterfaces;
using System;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;
using ServiceFabric.Logging.Remoting;

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
        public async Task<int> Get(int a, int b)
        {
            var uri = new Uri($"{FabricRuntime.GetActivationContext().ApplicationName}/MyStateless");
            var sum = await serviceRemoting.CallAsync<IMyService, int>(HttpContext.TraceIdentifier, uri, service => service.CalculateSum(a, b));

            await new HttpClient().GetAsync("http://www.google.nl");

            return sum;
        }
    }
}
