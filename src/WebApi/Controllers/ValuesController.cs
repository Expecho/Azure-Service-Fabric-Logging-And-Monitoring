using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceInterfaces;
using System;
using System.Fabric;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using MyActor.Interfaces;
using ServiceFabric.Logging;
using ServiceFabric.Logging.Remoting;
using ServiceFabric.Remoting.CustomHeaders;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IServiceRemoting _serviceRemoting;

        public ValuesController(ILogger logger, IServiceRemoting serviceRemoting)
        {
            this._serviceRemoting = serviceRemoting;
        }

        // GET api/values?a=1&b=2
        [HttpGet]
        public async Task<int> Get(int a, int b)
        {
            var uri = new Uri($"{FabricRuntime.GetActivationContext().ApplicationName}/MyStateless");

            Func<CustomHeaders> customHeadersProvider = () => new CustomHeaders
            {
                { HeaderIdentifiers.TraceId, HttpContext.TraceIdentifier }
            };

            var sum = await _serviceRemoting.CallAsync<IMyService, int>(customHeadersProvider, uri, service => service.CalculateSumAsync(a, b));

            await new HttpClient().GetAsync("http://www.google.nl");

            var actor = ActorProxy.Create<IMyActor>(ActorId.CreateRandom());
            await actor.GetCountAsync(CancellationToken.None);

            return sum;
        }

        [HttpPost]
        public IActionResult Post([FromBody] string data)
        {
            return new StatusCodeResult(201);
        }
    }
}
