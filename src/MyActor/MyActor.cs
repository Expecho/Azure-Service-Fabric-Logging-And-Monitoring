using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using MyActor.Interfaces;
using ServiceFabric.Logging;
using ServiceFabric.Remoting.CustomHeaders;

namespace MyActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class MyActor : Actor, IMyActor
    {
        private readonly ILogger _logger;

        public MyActor(ActorService actorService, ActorId actorId, ILogger logger) : base(actorService, actorId)
        {
            _logger = logger;
        }

        protected override Task OnActivateAsync()
        {
            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return StateManager.TryAddStateAsync("count", 0);
        }

        Task<int> IMyActor.GetCountAsync(CancellationToken cancellationToken)
        {
            var traceId = RemotingContext.GetData(HeaderIdentifiers.TraceId);
            _logger.LogInformation($"Hello from inside {nameof(MyActor)} (traceId {traceId})");

            return StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        Task IMyActor.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }
    }
}
