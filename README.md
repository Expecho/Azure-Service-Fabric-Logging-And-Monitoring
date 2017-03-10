This repository contains an Azure Service Fabric application that provides some guidance and examples on how to setup and configure logging and demonstrates some approaches.          

# Azure Service Fabric Logging

Service Fabric by default uses Event Tracing for Windows (EWT) to emit events. Using [the EventFlow library suite] (https://github.com/Azure/diagnostics-eventflow) these events can be send to different sinks like OMS, Application Insights or others using a configurable pipeline.

## Structured Logging

Using ETW gives the advantages of being able to use structured logging. There are other structured logging enabled frameworks like [Serilog](https://serilog.net/). This repository is focused on using structured logging in order to provide rich en detailed logging output. If you are not familiar with structured logging these links provide some insights about the what and why:

- [The concept of structured logging](https://nblumhardt.com/2016/06/structured-logging-concepts-in-net-series-1/)
- [Benefits of structured logging](http://softwareengineering.stackexchange.com/questions/312197/benefits-of-structured-logging-vs-basic-logging)

## Custom health reporting using the EventFlow library suite

Azure Service Fabric introduces a health model that provides rich, flexible, and extensible health evaluation and reporting. The model allows near-real-time monitoring of the state of the cluster and the services running in it. Developers can create their own health reports. This repository contains a custom EventFlow sink that reports events as health reports. See [the EventFlow.Outputs.Custom.ServiceFabric project](EventFlow.Outputs.Custom.ServiceFabric). Custom outputs are configured using the EventFlow configuration file, [See this example configuration](WebApi/PackageRoot/Config/eventFlowConfig.json)

## Centralized Event Collecting

There are multiple ways to collect the events emitted by the EventSource classes of the different services. The most efficient way is using [the EventSource input](https://github.com/Azure/diagnostics-eventflow#eventsource) of the EventFlow library.

Another way is to create a seperate service that collects the events of the different EventSource classes using [the ETW input provider](https://github.com/Azure/diagnostics-eventflow#etw-event-tracing-for-windows). This way you can centralize the setup of the EventFlow pipeline.  See [the EventSourceEventCollector project](EventSourceEventCollector).

# Useful links

- [Service Fabric Diagnostics Overview](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-diagnostics-overview)
- [Service Fabric Health Monitoring](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-health-introduction)
