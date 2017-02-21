# Azure Service Fabric Logging

This repository contains an Azure Service Fabric application that demonstrates different ways to set up logging.

Service Fabric uses Event Tracing for Windows (EWT) to emit events. Using the EventFlow library suite (https://github.com/Azure/diagnostics-eventflow) these events can be send to different sinks like OMS, Application Insights or others using a configurable pipeline.

## Custom health reporting

Azure Service Fabric introduces a health model that provides rich, flexible, and extensible health evaluation and reporting. The model allows near-real-time monitoring of the state of the cluster and the services running in it. Developers can create their own health reports. This repository contains a custom EventFlow sink that reports events as health reports. See [the EventFlow.Outputs.Custom.ServiceFabric project](EventFlow.Outputs.Custom.ServiceFabric). Custom outputs are configured using the EventFlow configuration file, [See this example configuration](WebApi/PackageRoot/Config/eventFlowConfig.json)

