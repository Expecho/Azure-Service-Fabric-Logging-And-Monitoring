using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace ServiceInterfaces
{
    public interface IMyService : IService
    {
        Task<int> CalculateSum(int a, int b);
    }
}
