using Microsoft.ServiceFabric.Services.Remoting;

namespace Interface
{
    public interface IStatelessDemo : IService
    {
        string HelloWorld();
    }
}
