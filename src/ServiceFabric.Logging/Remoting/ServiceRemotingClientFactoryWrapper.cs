using System;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace ServiceFabric.Logging.Remoting
{
    internal class ServiceRemotingClientFactoryWrapper : IServiceRemotingClientFactory
    {
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        private readonly string traceId;
        private readonly IServiceRemotingClientFactory serviceRemotingClientFactory;

        public ServiceRemotingClientFactoryWrapper(string traceId, IServiceRemotingClientFactory serviceRemotingClientFactory)
        {
            this.traceId = traceId;
            this.serviceRemotingClientFactory = serviceRemotingClientFactory;
        }

        public async Task<IServiceRemotingClient> GetClientAsync(ResolvedServicePartition previousRsp,
            TargetReplicaSelector targetReplicaSelector,
            string listenerName,
            OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            var client = await serviceRemotingClientFactory.GetClientAsync(previousRsp, targetReplicaSelector, listenerName, retrySettings, cancellationToken);
            return new ServiceRemotingClientWrapper(client, traceId);
        }

        public async Task<IServiceRemotingClient> GetClientAsync(Uri serviceUri,
            ServicePartitionKey partitionKey,
            TargetReplicaSelector targetReplicaSelector,
            string listenerName,
            OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            var client = await serviceRemotingClientFactory.GetClientAsync(serviceUri, partitionKey, targetReplicaSelector, listenerName, retrySettings, cancellationToken);
            return new ServiceRemotingClientWrapper(client, traceId);
        }

        public Task<OperationRetryControl> ReportOperationExceptionAsync(IServiceRemotingClient client,
            ExceptionInformation exceptionInformation,
            OperationRetrySettings retrySettings,
            CancellationToken cancellationToken)
        {
            return serviceRemotingClientFactory.ReportOperationExceptionAsync(((ServiceRemotingClientWrapper)client).Client, exceptionInformation, retrySettings, cancellationToken);
        }

        private class ServiceRemotingClientWrapper : IServiceRemotingClient
        {
            private readonly string traceId;

            public ServiceRemotingClientWrapper(IServiceRemotingClient client, string traceId)
            {
                this.Client = client;
                this.traceId = traceId;
            }

            internal IServiceRemotingClient Client { get; }

            public ResolvedServiceEndpoint Endpoint
            {
                get
                {
                    return Client.Endpoint;
                }

                set
                {
                    Client.Endpoint = value;
                }
            }

            public string ListenerName
            {
                get
                {
                    return Client.ListenerName;
                }

                set
                {
                    Client.ListenerName = value;
                }
            }

            public ResolvedServicePartition ResolvedServicePartition
            {
                get
                {
                    return Client.ResolvedServicePartition;
                }

                set
                {
                    Client.ResolvedServicePartition = value;
                }
            }

            public Task<byte[]> RequestResponseAsync(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
            {
                messageHeaders.AddHeader(HeaderIdentifiers.TraceId, Encoding.ASCII.GetBytes(traceId));

                return Client.RequestResponseAsync(messageHeaders, requestBody);
            }

            public void SendOneWay(ServiceRemotingMessageHeaders messageHeaders, byte[] requestBody)
            {
                messageHeaders.AddHeader(HeaderIdentifiers.TraceId, Encoding.ASCII.GetBytes(traceId));

                Client.SendOneWay(messageHeaders, requestBody);
            }
        }
    }
}
