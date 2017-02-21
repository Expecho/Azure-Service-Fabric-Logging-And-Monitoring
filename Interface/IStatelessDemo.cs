using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Interface
{
    public interface IStatelessDemo : IService
    {
        Task<string> HelloWorldAsync();
    }
}
