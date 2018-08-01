using Microsoft.AspNetCore.Mvc;
using ServiceInterfaces;
using System;
using System.Fabric;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using MyActor.Interfaces;
using ServiceFabric.Logging.Remoting;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IProxyFactoryProvider _proxyFactoryProvider;

        public ValuesController(IProxyFactoryProvider proxyFactoryProvider)
        {
            _proxyFactoryProvider = proxyFactoryProvider;
        }

        // GET api/values?a=1&b=2
        [HttpGet]
        public async Task<int> Get(int a, int b)
        {
            var uri = new Uri($"{FabricRuntime.GetActivationContext().ApplicationName}/MyStateless");
            
            var proxyFactory = _proxyFactoryProvider.CreateServiceProxyFactory();
            var sum = await proxyFactory.CreateServiceProxy<IMyService>(uri).CalculateSumAsync(a, b);
            
            await new HttpClient().GetAsync("http://www.google.nl");

            var actorProxyFactory = _proxyFactoryProvider.CreateActorProxyFactory();
            var actor = actorProxyFactory.CreateActorProxy<IMyActor>(ActorId.CreateRandom());
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
