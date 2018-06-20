using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ServiceInterfaces
{
    public interface IMyService : IService
    {
        Task<int> CalculateSumAsync(int a, int b);
    }
}
