using System;
using System.Diagnostics;
using System.Fabric;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Owin;

namespace WebApi
{
    internal class OwinCommunicationListener : ICommunicationListener
    {
        private readonly ServiceEventSource eventSource;
        private readonly Action<IAppBuilder> startup;
        private readonly ServiceContext serviceContext;
        private readonly string endpointName;
        private readonly string appRoot;

        private IDisposable webApp;
        private string publishAddress;
        private string listeningAddress;

        public OwinCommunicationListener(Action<IAppBuilder> startup, ServiceContext serviceContext, ServiceEventSource eventSource, string endpointName)
            : this(startup, serviceContext, eventSource, endpointName, null)
        {
        }

        public OwinCommunicationListener(Action<IAppBuilder> startup, ServiceContext serviceContext, ServiceEventSource eventSource, string endpointName, string appRoot)
        {
            if (startup == null)
            {
                throw new ArgumentNullException(nameof(startup));
            }

            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            if (eventSource == null)
            {
                throw new ArgumentNullException(nameof(eventSource));
            }

            this.startup = startup;
            this.serviceContext = serviceContext;
            this.endpointName = endpointName;
            this.eventSource = eventSource;
            this.appRoot = appRoot;
        }

        public bool ListenOnSecondary { get; set; }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serviceEndpoint = serviceContext.CodePackageActivationContext.GetEndpoint(endpointName);
            var port = serviceEndpoint.Port;

            if (serviceContext is StatefulServiceContext)
            {
                var statefulServiceContext = serviceContext as StatefulServiceContext;

                listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}{2}/{3}/{4}",
                    port,
                    string.IsNullOrWhiteSpace(appRoot)
                        ? string.Empty
                        : appRoot.TrimEnd('/') + '/',
                    statefulServiceContext.PartitionId,
                    statefulServiceContext.ReplicaId,
                    Guid.NewGuid());
            }
            else if (serviceContext is StatelessServiceContext)
            {
                listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}",
                    port,
                    string.IsNullOrWhiteSpace(appRoot)
                        ? string.Empty
                        : appRoot.TrimEnd('/') + '/');
            }
            else
            {
                throw new InvalidOperationException();
            }

            publishAddress = listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            try
            {
                eventSource.ServiceMessage(serviceContext, "Starting web server on " + listeningAddress);

                webApp = WebApp.Start(listeningAddress, appBuilder => startup.Invoke(appBuilder));

                eventSource.ServiceMessage(serviceContext, "Listening on " + publishAddress);

                return Task.FromResult(publishAddress);
            }
            catch (Exception ex)
            {
                eventSource.ServiceMessage(serviceContext, "Web server failed to open. " + ex.ToString());

                StopWebServer();

                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            eventSource.ServiceMessage(serviceContext, "Closing web server");

            StopWebServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            eventSource.ServiceMessage(serviceContext, "Aborting web server");

            StopWebServer();
        }

        private void StopWebServer()
        {
            if (webApp != null)
            {
                try
                {
                    webApp.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}
