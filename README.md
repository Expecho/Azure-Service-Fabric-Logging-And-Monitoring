# Overview

This repository contains an Azure Service Fabric application that provides some guidance and examples on how to setup and configure logging and demonstrates some approaches. 

The application consist of two stateless services, one hosting an ASP.Net Core application that acts as an Web Api and a stateless service that is being called from the Web Api using [Service Remoting](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-communication-remoting).

![Application Overview](/blobs/asf-application.PNG )

It uses a structured logging framework called [Serilog](https://serilog.net/) to log events. The logged events are then written to [Application Insights](https://azure.microsoft.com/en-us/services/application-insights/) using [this](https://github.com/serilog/serilog-sinks-applicationinsights) Serilog sink. By using Serilog to capture the events instead of logging directly to Application Insights using the SDK it is easy to add different outputs for the logged events.

# Getting started

Before running the sample [create a new Application Insights resource](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-create-new-resource) and copy the Instrumentation Key to the application parameter in the [Service Fabric Application manifest file](/src/AzureServiceFabric.Demo.Diagnostics/ApplicationPackageRoot/ApplicationManifest.xml#L6):

```
<ApplicationManifest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ApplicationTypeName="AzureServiceFabric.Demo.DiagnosticsType" ApplicationTypeVersion="1.0.0" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Parameters>
    <Parameter Name="WebApi_InstanceCount" DefaultValue="-1" />
    <Parameter Name="MyStateless_InstanceCount" DefaultValue="-1" />
    <Parameter Name="ApplicationInsightsKey" DefaultValue="[YOUR_KEY-HERE]" />
  </Parameters>
  ```

Deploy the application and point the browser to the web api at http://localhost:8700/api/values?a=1&b=2

# Additional Resources

This repository is focused on using structured logging using Serilog in order to provide rich en detailed logging output. If you are not familiar with structured logging these links provide some insights about the what and why:

- [The concept of structured logging](https://nblumhardt.com/2016/06/structured-logging-concepts-in-net-series-1/)
- [Benefits of structured logging](http://softwareengineering.stackexchange.com/questions/312197/benefits-of-structured-logging-vs-basic-logging)
