using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using ServiceFabric.Remoting.CustomHeaders;

namespace ServiceFabric.Logging.Remoting
{
    /// <summary>
    /// Helper class for remoting calls
    /// </summary>
    [Obsolete("Use the IProxyFactoryProvider interface")]
    public interface IServiceRemoting
    {
        /// <summary>
        /// Call a service using remoting and log the call
        /// </summary>
        /// <typeparam name="TService">The type of the service to call</typeparam>
        /// <typeparam name="TResult">The type of the result of the service method</typeparam>
        /// <param name="customHeaders">The user defined headers included in the call</param>
        /// <param name="uri">The uri of the service to call</param>
        /// <param name="callMethod">The expression that provides the name of the method to call for logging purposes</param>
        /// <returns>The result the service method</returns>
        Task<TResult> CallAsync<TService, TResult>(CustomHeaders customHeaders, Uri uri, Expression<Func<TService, Task<TResult>>> callMethod) where TService : IService;

        /// <summary>
        /// Call a service using remoting and log the call
        /// </summary>
        /// <typeparam name="TService">The type of the service to call</typeparam>
        /// <param name="customHeaders">The user defined headers included in the call</param>
        /// <param name="uri">The uri of the service to call</param>
        /// <param name="callMethod">The expression that provides the name of the method to call for logging purposes</param>
        /// <returns>The task to await the call</returns>
        Task CallAsync<TService>(CustomHeaders customHeaders, Uri uri, Expression<Func<TService, Task>> callMethod) where TService : IService;
    }
}