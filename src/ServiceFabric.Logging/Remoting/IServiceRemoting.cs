using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using ServiceFabric.Remoting.CustomHeaders;

namespace ServiceFabric.Logging.Remoting
{
    public interface IServiceRemoting
    {
        Task<TResult> CallAsync<TService, TResult>(CustomHeaders customHeaders, Uri uri, Expression<Func<TService, Task<TResult>>> callMethod) where TService : IService;
        Task CallAsync<TService>(CustomHeaders customHeaders, Uri uri, Expression<Func<TService, Task>> callMethod) where TService : IService;
    }
}