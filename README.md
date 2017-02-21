# Azure Service Fabric Logging

This repository contains an Azure Service Fabric application that demonstrates different ways to set up logging.

Service Fabric uses Event Tracing for Windows (EWT) to emit events. Using the EventFlow library suite (https://github.com/Azure/diagnostics-eventflow) these events can be send to different sinks like OMS, Application Insights or others using a configurable pipeline.
