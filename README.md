# Overview

This repository contains an Azure Service Fabric application that provides some guidance and examples on how to setup and configure logging and demonstrates some approaches. It is probably not production ready but it gives a working overview on how things are working together. Please see it as a good starting point for your own integration.

The application consist of two stateless services, one hosting an ASP.Net Core application that acts as an Web Api and a stateless service that is being called from the Web Api using [Service Remoting](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reliable-services-communication-remoting).

![Application Overview](blobs/asf-application.PNG?raw=true )

It uses a structured logging framework called [Serilog](https://serilog.net/) to log events. The logged events are then written to [Application Insights](https://azure.microsoft.com/en-us/services/application-insights/) using [this](https://github.com/serilog/serilog-sinks-applicationinsights) Serilog sink. By using Serilog to capture the events instead of logging directly to Application Insights using the SDK it is easy to add different outputs for the logged events.

# How it works

.Net Core provides [a logging abstraction](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) that provides support for structured logging. Since Serilog is supported by the .Net Core logging abstraction we can seperate the calls to the logger from the actual implementation. (For more background about this, read [this excellent blog post](https://msdn.microsoft.com/en-us/magazine/mt694089.aspx))

By using [custom Middleware](/src/ServiceFabric.Logging/Middleware/RequestTrackingMiddleware.cs) in the ASP.Net Core pipeline we can capture the trace id of the request and use that in the logging to correlate log events. Calls from the Web Api to the other stateless service are [intercepted and logged](/src/ServiceFabric.Logging/Remoting/ServiceRemoting.cs) based on the answers in [this](http://stackoverflow.com/questions/34166193/how-to-add-message-header-to-the-request-when-using-default-client-of-azure-serv) StackOverflow question.

Most other logging frameworks provide Application Insights sinks that are only capable of writing Trace entities to Application Insights. Using structured logging gives the advantage to transform the log events to specific Application Insights entities like Requests, Dependencies, Metrics and Exceptions. This is done by the [TelemetryBuilder](/src/ServiceFabric.Logging/ApplicationInsights/TelemetryBuilder.cs) class.

# Features
 - Full Application Insights Integration
 - Structured Logging
 - Easy to add other sinks
 - Integration with .Net Core logging framework
 
# Getting started

This source is build and tested using Azure Service Fabric 5.5 and SDK 2.5, with Visual Studio 2017 v15.1

Before running the sample [create a new Application Insights resource](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-create-new-resource) and copy the Instrumentation Key to the application parameter in the [Service Fabric Application manifest file](/src/AzureServiceFabric.Demo.Diagnostics/ApplicationPackageRoot/ApplicationManifest.xml#L6):

```
<ApplicationManifest xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" ApplicationTypeName="AzureServiceFabric.Demo.DiagnosticsType" ApplicationTypeVersion="1.0.0" xmlns="http://schemas.microsoft.com/2011/01/fabric">
  <Parameters>
    <Parameter Name="WebApi_InstanceCount" DefaultValue="-1" />
    <Parameter Name="MyStateless_InstanceCount" DefaultValue="-1" />
    <Parameter Name="ApplicationInsightsKey" DefaultValue="[YOUR_KEY-HERE]" />
  </Parameters>
  ```

Deploy the application, wait for all services to be up and running and point the browser to the web api at http://localhost:8700/api/values?a=1&b=2

Observe the events written to application insights:

![Application Insights](/blobs/app-insights.PNG )


![Application Insights Live Metrics Stream](/blobs/live-metrics.PNG )

# Additional Resources

This repository is focused on using structured logging using Serilog in order to provide rich en detailed logging output. If you are not familiar with structured logging these links provide some insights about the what and why:

- [The concept of structured logging](https://nblumhardt.com/2016/06/structured-logging-concepts-in-net-series-1/)
- [Benefits of structured logging](http://softwareengineering.stackexchange.com/questions/312197/benefits-of-structured-logging-vs-basic-logging)
