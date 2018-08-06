using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using ServiceFabric.Logging.Extensions;
using ServiceFabric.Remoting.CustomHeaders.Actors;

namespace ServiceFabric.Logging.Actors
{
    /// <summary>
    /// Factory class for creating instances of an <see cref="ExtendedActorService"/>
    /// </summary>
    public class ActorServiceFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="ExtendedActorService"/>
        /// </summary>
        /// <param name="context">The <see cref="StatefulServiceContext"/> of the <see cref="ExtendedActorService"/></param>
        /// <param name="actorType">The <see cref="ActorTypeInformation"/> of the Actor</param>
        /// <param name="logger">An implementation instance of <see cref="ILogger"/> used for logging</param>
        /// <param name="actorFactory"></param>
        /// <returns></returns>
        public static ExtendedActorService CreateActorService(StatefulServiceContext context, ActorTypeInformation actorType, ILogger logger, Func<ActorService, ActorId, ActorBase> actorFactory)
        {
            return new ExtendedActorService(context, actorType, actorFactory)
            {
                BeforeHandleRequestResponseAsync = info =>
                {
                    var stopwatch = Stopwatch.StartNew();
                    return Task.FromResult<object>(stopwatch);
                },
                AfterHandleRequestResponseAsync = responseInfo =>
                {
                    var stopwatch = (Stopwatch)responseInfo.State;

                    if (responseInfo.Exception != null)
                        logger.LogError(ServiceFabricEvent.Exception, responseInfo.Exception, responseInfo.Exception.Message);

                    var elapsed = stopwatch.ElapsedMilliseconds;
                    logger.LogDependency(
                        responseInfo.ActorService.ToString(),
                        responseInfo.Method,
                        DateTime.Now.AddMilliseconds(elapsed * -1),
                        TimeSpan.FromMilliseconds(elapsed),
                        responseInfo.Exception == null,
                        responseInfo.ActorId);

                    return Task.CompletedTask;
                }
            };
        }
    }
}