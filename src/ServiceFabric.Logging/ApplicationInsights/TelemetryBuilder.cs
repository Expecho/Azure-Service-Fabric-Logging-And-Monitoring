using System;
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
            ITelemetry telemetry = null;

            if (logEvent.Exception != null)
                telemetry = CreateExceptionTelemetry();
            else if (logEvent.Properties.TryGetValue(SharedProperties.EventId, out LogEventPropertyValue value) && Enum.TryParse(((StructureValue)value).Properties[0].Value.ToString(), out ServiceFabricEvent serviceFabricEvent))
            {
                switch (serviceFabricEvent)
                {
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
            }

            if (telemetry == null)
            {
                telemetry = CreateTraceTelemetry();
            }

            SetContextProperties(telemetry);

            return telemetry;
        }

        private ITelemetry CreateRequestTelemetry()
        {
            var requestTelemetry = new RequestTelemetry
            {
                HttpMethod = TryGetStringValue(ApiRequestProperties.Method),
                ResponseCode = TryGetStringValue(ApiRequestProperties.StatusCode),
                Url = new Uri($"{TryGetStringValue(ApiRequestProperties.Scheme)}://{TryGetStringValue(ApiRequestProperties.Host)}{TryGetStringValue(ApiRequestProperties.Path)}"),
                Name = $"{TryGetStringValue(ApiRequestProperties.Method)} {TryGetStringValue(ApiRequestProperties.Path)}",
                StartTime = DateTime.Parse(TryGetStringValue(ApiRequestProperties.StartTime)),
                Duration = TimeSpan.FromMilliseconds(double.Parse(TryGetStringValue(ApiRequestProperties.DurationInMs))),
                Success = bool.Parse(TryGetStringValue(ApiRequestProperties.Success))
            };

            requestTelemetry.Context.Operation.Name = requestTelemetry.Name;
            requestTelemetry.Id = TryGetStringValue(SharedProperties.TraceId);

            return requestTelemetry;
        }

        private ITelemetry CreateDependencyTelemetry()
        {
            var dependencyTelemetry = new DependencyTelemetry()
            {
                Name = TryGetStringValue(DependencyProperties.DependencyTypeName),
                Duration = TimeSpan.FromMilliseconds(double.Parse(TryGetStringValue(DependencyProperties.DurationInMs))),
                CommandName = TryGetStringValue(DependencyProperties.Name),
                Success = bool.Parse(TryGetStringValue(DependencyProperties.Success)),
                StartTime = DateTime.Parse(TryGetStringValue(DependencyProperties.StartTime)),
                DependencyKind = TryGetStringValue(DependencyProperties.Type)
            };

            dependencyTelemetry.Id = dependencyTelemetry.CommandName;
            dependencyTelemetry.Context.Operation.Name = dependencyTelemetry.Name;

            return dependencyTelemetry;
        }

        private ITelemetry CreateMetricTelemetry()
        {
            var metricTelemetry = new MetricTelemetry()
            {
                Name = TryGetStringValue(MetricProperties.Name),
                Value = double.Parse(TryGetStringValue(MetricProperties.Value))
            };

            if (logEvent.Properties.TryGetValue(MetricProperties.MinValue, out LogEventPropertyValue min))
                metricTelemetry.Min = double.Parse(min.ToString());

            if (logEvent.Properties.TryGetValue(MetricProperties.MaxValue, out LogEventPropertyValue max))
                metricTelemetry.Max = double.Parse(max.ToString());

            return metricTelemetry;
        }

        private ITelemetry CreateExceptionTelemetry()
        {
            var exceptionTelemetry = new ExceptionTelemetry(logEvent.Exception)
            {
                SeverityLevel = logEvent.Level.ToSeverityLevel()
            };

            AddLogEventProperties(exceptionTelemetry);

            return exceptionTelemetry;
        }

        private ITelemetry CreateTraceTelemetry()
        {
            var traceTelemetry = new TraceTelemetry(logEvent.RenderMessage())
            {
                SeverityLevel = logEvent.Level.ToSeverityLevel()
            };

            AddLogEventProperties(traceTelemetry);

            return traceTelemetry;
        }

        private void SetContextProperties(ITelemetry telemetry)
        {
            telemetry.Timestamp = logEvent.Timestamp;
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

        private void AddLogEventProperties(ISupportProperties telemetry)
        {
            foreach (var property in logEvent
                .Properties
                .Where(property => property.Value != null && !telemetry.Properties.ContainsKey(property.Key)))
            {
                ApplicationInsightsPropertyFormatter.WriteValue(property.Key, property.Value, telemetry.Properties);
            }
        }

        private LogEventPropertyValue TryGetPropertyValue(string propertyName)
        {
            if (!logEvent.Properties.TryGetValue(propertyName, out LogEventPropertyValue value))
                throw new ArgumentException($"LogEvent does not contain required property {propertyName} for EventId {logEvent.Properties[SharedProperties.EventId]}", propertyName);

            return value;
        }

        private string TryGetStringValue(string propertyName)
        {
            if (!logEvent.Properties.TryGetValue(propertyName, out LogEventPropertyValue value))
                throw new ArgumentException($"LogEvent does not contain required property {propertyName} for EventId {logEvent.Properties[SharedProperties.EventId]}", propertyName);

            return ((ScalarValue)value).Value.ToString();
        }
    }
}
