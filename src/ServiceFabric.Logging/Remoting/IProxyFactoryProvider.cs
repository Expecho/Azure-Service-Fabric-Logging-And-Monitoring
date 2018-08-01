using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ServiceFabric.Logging.Remoting
{
    /// <summary>
    /// Defines methods to create proxy factories
    /// </summary>
    public interface IProxyFactoryProvider
    {
        /// <summary>
        /// Create a <see cref="IServiceProxyFactory"/>
        /// </summary>
        /// <returns>An instance of a type that implements <see cref="IServiceProxyFactory"/></returns>
        IServiceProxyFactory CreateServiceProxyFactory();

        /// <summary>
        /// Create a <see cref="IActorProxyFactory"/>
        /// </summary>
        /// <returns>An instance of a type that implements <see cref="IActorProxyFactory"/></returns>
        IActorProxyFactory CreateActorProxyFactory();
    }
}