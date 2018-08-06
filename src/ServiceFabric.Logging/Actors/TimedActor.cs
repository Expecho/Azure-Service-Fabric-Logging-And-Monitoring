using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using ServiceFabric.Logging.Extensions;

namespace ServiceFabric.Logging.Actors
{
    /// <summary>
    /// Base class for measuring actor call duration
    /// </summary>
    /// <typeparam name="T">The Actor type to measure</typeparam>
    [Obsolete("Use ProxyFactoryProvider to create an Actor proxy")]
    public class TimedActor<T> : Actor where T : Actor
    {
        private Stopwatch stopwatch;

        /// <summary>
        /// Gets an instance of <see cref="ILogger"/>
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="actorService">The <see cref="ActorService"/></param>
        /// <param name="actorId">The <see cref="ActorId"/> of the actor</param>
        /// <param name="logger">The <see cref="ILogger"/> implementation used for logging</param>
        public TimedActor(ActorService actorService, ActorId actorId, ILogger logger) : base(actorService, actorId)
        {
            Logger = logger;
        }

        /// <summary>
        /// Initiate logging of method duration
        /// </summary>
        /// <param name="actorMethodContext">The method context</param>
        /// <returns></returns>
        protected override Task OnPreActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            stopwatch = Stopwatch.StartNew();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Log method duration
        /// </summary>
        /// <param name="actorMethodContext">The method context</param>
        /// <returns></returns>
        protected override Task OnPostActorMethodAsync(ActorMethodContext actorMethodContext)
        {
            stopwatch.Stop();

            Logger.LogActorMethod<T>(actorMethodContext, DateTime.Now.AddMilliseconds(stopwatch.ElapsedMilliseconds), stopwatch.Elapsed);

            return Task.CompletedTask;
        }
    }
}
