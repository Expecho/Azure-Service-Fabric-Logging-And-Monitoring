using Microsoft.Extensions.Logging;

namespace ServiceFabric.Logging
{
    public interface ILoggerFactoryBuilder
    {
        ILoggerFactory CreateLoggerFactory(string aiKey);
    }
}