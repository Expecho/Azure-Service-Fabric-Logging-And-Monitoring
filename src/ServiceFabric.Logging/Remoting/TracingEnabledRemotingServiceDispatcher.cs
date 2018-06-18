using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Logging.Remoting
{
    public class TracingEnabledRemotingServiceDispatcher : ServiceRemotingMessageDispatcher
    {
        public TracingEnabledRemotingServiceDispatcher(ServiceContext serviceContext, IService service)
            : base(serviceContext, service)
        {
        }

        public TracingEnabledRemotingServiceDispatcher(ServiceContext serviceContext, IService service, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(serviceContext, service, serviceRemotingMessageBodyFactory)
        {
        }

        public TracingEnabledRemotingServiceDispatcher(IEnumerable<Type> remotingTypes, ServiceContext serviceContext, object serviceImplementation, IServiceRemotingMessageBodyFactory serviceRemotingMessageBodyFactory = null)
            : base(remotingTypes, serviceContext, serviceImplementation, serviceRemotingMessageBodyFactory)
        {
        }

        public override void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
        {
            SetTraceId(requestMessage);
            base.HandleOneWayMessage(requestMessage);
        }

        public override Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext,
            IServiceRemotingRequestMessage requestMessage)
        {
            SetTraceId(requestMessage);
            return base.HandleRequestResponseAsync(requestContext, requestMessage);
        }

        private void SetTraceId(IServiceRemotingRequestMessage requestMessage)
        {
            if (requestMessage.GetHeader().TryGetHeaderValue(HeaderIdentifiers.TraceId, out byte[] headerValue))
            {
                if (headerValue != null && headerValue.Any())
                {
                    CallContext.SetData(HeaderIdentifiers.TraceId, Encoding.ASCII.GetString(headerValue));
                }
            }
        }
    }
}
