using System.Fabric;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace ServiceFabric.Logging.Remoting
{
    public class TracingEnabledRemotingServiceDispatcher : ServiceRemotingDispatcher
    {
        public TracingEnabledRemotingServiceDispatcher(ServiceContext serviceContext, IService service)
            : base(serviceContext, service)
        {

        }

        public override void HandleOneWay(IServiceRemotingRequestContext requestContext,
            ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            messageHeaders.TryGetHeaderValue(HeaderIdentifiers.TraceId, out byte[] col);
            if (col != null && col.Any())
            {
                CallContext.LogicalSetData(HeaderIdentifiers.TraceId, Encoding.ASCII.GetString(col));
            }
            base.HandleOneWay(requestContext, messageHeaders, requestBody);
        }

        public override Task<byte[]> RequestResponseAsync(IServiceRemotingRequestContext requestContext,
            ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
        {
            messageHeaders.TryGetHeaderValue(HeaderIdentifiers.TraceId, out byte[] col);
            if (col != null && col.Any())
            {
                CallContext.LogicalSetData(HeaderIdentifiers.TraceId, Encoding.ASCII.GetString(col));
            }
            return base.RequestResponseAsync(requestContext, messageHeaders, requestBody);
        }
    }
}
