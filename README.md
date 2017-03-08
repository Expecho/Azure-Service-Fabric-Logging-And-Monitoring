# Azure Service Fabric Logging

This repository contains an Azure Service Fabric application that provides some guidance and examples on how to setup logging.           

Service Fabric uses Event Tracing for Windows (EWT) to emit events. Using [the EventFlow library suite] (https://github.com/Azure/diagnostics-eventflow) these events can be send to different sinks like OMS, Application Insights or others using a configurable pipeline.

## Custom health reporting using the EventFlow library suite

Azure Service Fabric introduces a health model that provides rich, flexible, and extensible health evaluation and reporting. The model allows near-real-time monitoring of the state of the cluster and the services running in it. Developers can create their own health reports. This repository contains a custom EventFlow sink that reports events as health reports. See [the EventFlow.Outputs.Custom.ServiceFabric project](EventFlow.Outputs.Custom.ServiceFabric). Custom outputs are configured using the EventFlow configuration file, [See this example configuration](WebApi/PackageRoot/Config/eventFlowConfig.json)

## Centralized Event Collecting

There are multiple ways to collect the events emitted by the EventSource classes of the different services. The most efficient way is using [the EventSource input](https://github.com/Azure/diagnostics-eventflow#eventsource) of the EventFlow library.

Another way is to create a seperate service that collects the events of the different EventSource classes using [the ETW input provider](https://github.com/Azure/diagnostics-eventflow#etw-event-tracing-for-windows). This way you can centralize the setup of the EventFlow pipeline.  See [the EventSourceEventCollector project](EventSourceEventCollector).

