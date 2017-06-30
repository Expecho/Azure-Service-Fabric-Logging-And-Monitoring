using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using ServiceFabric.Logging.Extensions;

namespace ServiceFabric.Logging.Actors
{
    public class TimedActor<T> : Actor where T : Actor
    {
        private readonly ILogger logger;
        private Stopwatch stopwatch;

        public TimedActor(ActorService actorService, ActorId actorId, ILogger logger) : base(actorService, actorId)
        {
            this.logger = logger;
        }

        protected override Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            stopwatch = Stopwatch.StartNew();

            return Task.CompletedTask;
        }

        protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            stopwatch.Stop();

            logger.LogActorMethod<T>(actorMethodContext, DateTime.Now.AddMilliseconds(stopwatch.ElapsedMilliseconds), stopwatch.Elapsed);

            return Task.CompletedTask;
        }
    }
}
