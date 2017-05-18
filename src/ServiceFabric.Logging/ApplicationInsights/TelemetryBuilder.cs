using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Globalization;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Serilog.Events;
using ServiceFabric.Logging.PropertyMap;

namespace ServiceFabric.Logging.ApplicationInsights
{
    internal class TelemetryBuilder
    {
        private readonly ServiceContext context;
        private readonly LogEvent logEvent;
        private readonly IFormatProvider formatProvider;

        public TelemetryBuilder(ServiceContext context, LogEvent logEvent, IFormatProvider formatProvider)
        {
            this.context = context;
            this.logEvent = logEvent;
            this.formatProvider = formatProvider;
        }

        public ITelemetry LogEventToTelemetryConverter()
        {
            int serviceFabricEvent = ServiceFabricEvent.Undefined;
            if (logEvent.Properties.TryGetValue(SharedProperties.EventId, out LogEventPropertyValue eventId))
            {
                int.TryParse(((StructureValue)eventId).Properties[0].Value.ToString(), out serviceFabricEvent);
            }

            ITelemetry telemetry;
            switch (serviceFabricEvent)
            {
                case ServiceFabricEvent.Exception:
                    telemetry = CreateExceptionTelemetry();
                    break;
                case ServiceFabricEvent.ApiRequest:
                    telemetry = CreateRequestTelemetry();
                    break;
                case ServiceFabricEvent.Metric:
                    telemetry = CreateMetricTelemetry();
                    break;
                case ServiceFabricEvent.ServiceRequest:
                case ServiceFabricEvent.Dependency:
                    telemetry = CreateDependencyTelemetry();
                    break;
                default:
                    telemetry = CreateTraceTelemetry();
                    break;
            }

            SetContextProperties(telemetry);

            return telemetry;
        }

        private ITelemetry CreateRequestTelemetry()
        {
            var requestTelemetry = new RequestTelemetry
            {
                ResponseCode = TryGetStringValue(ApiRequestProperties.StatusCode),
                Url = new Uri($"{TryGetStringValue(ApiRequestProperties.Scheme)}://{TryGetStringValue(ApiRequestProperties.Host)}{TryGetStringValue(ApiRequestProperties.Path)}"),
                Name = $"{TryGetStringValue(ApiRequestProperties.Method)} {TryGetStringValue(ApiRequestProperties.Path)}",
                Timestamp = DateTime.Parse(TryGetStringValue(ApiRequestProperties.StartTime)),
                Duration = TimeSpan.FromMilliseconds(double.Parse(TryGetStringValue(ApiRequestProperties.DurationInMs))),
                Success = bool.Parse(TryGetStringValue(ApiRequestProperties.Success)),
                Properties = { { ApiRequestProperties.Headers, TryGetStringValue(ApiRequestProperties.Headers) } }
            };

            requestTelemetry.Context.Operation.Name = requestTelemetry.Name;
            requestTelemetry.Id = TryGetStringValue(SharedProperties.TraceId);

            AddLogEventProperties(requestTelemetry, typeof(ApiRequestProperties).GetFields().Select(f => f.GetRawConstantValue().ToString()));

            return requestTelemetry;
        }

        private ITelemetry CreateDependencyTelemetry()
        {
            var dependencyTelemetry = new DependencyTelemetry()
            {
                Name = TryGetStringValue(DependencyProperties.DependencyTypeName),
                Duration = TimeSpan.FromMilliseconds(double.Parse(TryGetStringValue(DependencyProperties.DurationInMs))),
                Data = TryGetStringValue(DependencyProperties.Name),
                Success = bool.Parse(TryGetStringValue(DependencyProperties.Success)),
                Type = TryGetStringValue(DependencyProperties.Type),
                Timestamp = DateTime.Parse(TryGetStringValue(DependencyProperties.StartTime)),
            };

            dependencyTelemetry.Id = dependencyTelemetry.Data;
            dependencyTelemetry.Context.Operation.Name = dependencyTelemetry.Name;

            AddLogEventProperties(dependencyTelemetry, typeof(DependencyProperties).GetFields().Select(f => f.GetRawConstantValue().ToString()));

            return dependencyTelemetry;
        }

        private ITelemetry CreateMetricTelemetry()
        {
            var metricTelemetry = new MetricTelemetry()
            {
                Name = TryGetStringValue(MetricProperties.Name),
                Sum = double.Parse(TryGetStringValue(MetricProperties.Value)),
                Timestamp = logEvent.Timestamp
            };

            if (logEvent.Properties.TryGetValue(MetricProperties.MinValue, out LogEventPropertyValue min))
                metricTelemetry.Min = double.Parse(min.ToString());

            if (logEvent.Properties.TryGetValue(MetricProperties.MaxValue, out LogEventPropertyValue max))
                metricTelemetry.Max = double.Parse(max.ToString());

            AddLogEventProperties(metricTelemetry, typeof(MetricProperties).GetFields().Select(f => f.GetRawConstantValue().ToString()));

            return metricTelemetry;
        }

        private ITelemetry CreateExceptionTelemetry()
        {
            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
            {
                SeverityLevel = logEvent.Level.ToSeverityLevel(),
                Timestamp = logEvent.Timestamp
            };

            AddLogEventProperties(exceptionTelemetry);

            return exceptionTelemetry;
        }

        private ITelemetry CreateTraceTelemetry()
        {
            var traceTelemetry = new TraceTelemetry(logEvent.RenderMessage())
            {
                SeverityLevel = logEvent.Level.ToSeverityLevel(),
                Timestamp = logEvent.Timestamp
            };

            AddLogEventProperties(traceTelemetry);

            return traceTelemetry;
        }

        private void SetContextProperties(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = context.NodeContext.NodeName;
            telemetry.Context.Cloud.RoleInstance = context.NodeContext.NodeInstanceId.ToString(CultureInfo.InvariantCulture);
            telemetry.Context.Component.Version = context.CodePackageActivationContext.CodePackageVersion;

#if Debug
            telemetry.Context.Operation.SyntheticSource = "DebugSession";
#else
            if (Debugger.IsAttached)
            {
                telemetry.Context.Operation.SyntheticSource = "DebuggerAttached";
            }
#endif

            if (logEvent.Properties.TryGetValue(SharedProperties.TraceId, out LogEventPropertyValue value))
            {
                var id = ((ScalarValue) value).Value.ToString();
                telemetry.Context.Operation.ParentId = id;
                telemetry.Context.Operation.Id = id;
            }
        }

        private void AddLogEventProperties(ISupportProperties telemetry, IEnumerable<string> excludePropertyKeys = null)
        {
            var excludedPropertyKeys = new List<string>
            {
                nameof(context.NodeContext.NodeName),
                nameof(context.CodePackageActivationContext.CodePackageVersion)
            };

            if(excludePropertyKeys != null)
                excludedPropertyKeys.AddRange(excludePropertyKeys);

            foreach (var property in logEvent
                .Properties
                .Where(property => property.Value != null && !excludedPropertyKeys.Contains(property.Key) && !telemetry.Properties.ContainsKey(property.Key)))
            {
                ApplicationInsightsPropertyFormatter.WriteValue(property.Key, property.Value, telemetry.Properties);
            }
        }

        private string TryGetStringValue(string propertyName)
        {
            if (!logEvent.Properties.TryGetValue(propertyName, out LogEventPropertyValue value))
                throw new ArgumentException($"LogEvent does not contain required property {propertyName} for EventId {logEvent.Properties[SharedProperties.EventId]}", propertyName);

            return ((ScalarValue)value).Value.ToString();
        }
    }
}
