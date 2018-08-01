using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Logging.PropertyMap;

namespace ServiceFabric.Logging.Extensions
{
    public static class ILoggerExtensions
    {
        public static void LogMetric(this ILogger logger, string name, double value)
        {
            logger.LogInformation(ServiceFabricEvent.Metric,
                $"Metric {{{MetricProperties.Name}}}, value: {{{MetricProperties.Value}}}",
                name,
                value);
        }

        public static void LogMetric(this ILogger logger, string name, double value, double? max = null,
            double? min = null)
        {
            logger.LogInformation(ServiceFabricEvent.Metric,
                $"Metric {{{MetricProperties.Name}}}, value: {{{MetricProperties.Value}}}, min: {{{MetricProperties.MinValue}}}, max: {{{MetricProperties.MaxValue}}}",
                name,
                value,
                min,
                max);
        }

        public static void LogDependency<TService, TResult>(this ILogger logger,
            Expression<Func<TService, Task<TResult>>> callMethod, DateTime started, TimeSpan duration, bool success)
            where TService : IService
        {
            logger.LogInformation(ServiceFabricEvent.ServiceRequest,
                $"The call to {{{DependencyProperties.Type}}} dependency {{{DependencyProperties.DependencyTypeName}}} named {{{DependencyProperties.Name}}} finished in {{{DependencyProperties.DurationInMs}}} ms (success: {{{DependencyProperties.Success}}}) ({{{DependencyProperties.StartTime}}})",
                "ServiceFabric",
                typeof(TService).FullName,
                ((MethodCallExpression)callMethod.Body).Method.ToString(),
                duration.TotalMilliseconds,
                success,
                started);
        }

        public static void LogDependency<TService>(this ILogger logger, Expression<Func<TService, Task>> callMethod,
            DateTime started, TimeSpan duration, bool success) where TService : IService
        {
            logger.LogInformation(ServiceFabricEvent.ServiceRequest,
                $"The call to {{{DependencyProperties.Type}}} dependency {{{DependencyProperties.DependencyTypeName}}} named {{{DependencyProperties.Name}}} finished in {{{DependencyProperties.DurationInMs}}} ms (success: {{{DependencyProperties.Success}}}) ({{{DependencyProperties.StartTime}}})",
                "ServiceFabric",
                typeof(TService).FullName,
                ((MethodCallExpression)callMethod.Body).Method.ToString(),
                duration.TotalMilliseconds,
                success,
                started);
        }

        public static void LogDependency(this ILogger logger, string service, string method,
            DateTime started, TimeSpan duration, bool success)
        {
            logger.LogInformation(ServiceFabricEvent.ServiceRequest,
                $"The call to {{{DependencyProperties.Type}}} dependency {{{DependencyProperties.DependencyTypeName}}} named {{{DependencyProperties.Name}}} finished in {{{DependencyProperties.DurationInMs}}} ms (success: {{{DependencyProperties.Success}}}) ({{{DependencyProperties.StartTime}}})",
                "ServiceFabric",
                service,
                method,
                duration.TotalMilliseconds,
                success,
                started);
        }

        public static void LogRequest(this ILogger logger, HttpContext context, DateTime started, TimeSpan duration, bool success)
        {
            var request = context.Request;
            logger.LogInformation(ServiceFabricEvent.ApiRequest,
                $"The {{{ApiRequestProperties.Method}}} action to {{{ApiRequestProperties.Scheme}}}{{{ApiRequestProperties.Host}}}{{{ApiRequestProperties.Path}}} finished in {{{ApiRequestProperties.DurationInMs}}} ms with status code {{{ApiRequestProperties.StatusCode}}}({{{ApiRequestProperties.Success}}}) Headers: {{{ApiRequestProperties.Headers}}} Body: {{{ApiRequestProperties.Body}}} ({{{ApiRequestProperties.Response}}}) ({{{ApiRequestProperties.StartTime}}})",
                request.Method,
                request.Scheme,
                request.Host.Value,
                request.Path.Value,
                duration.TotalMilliseconds,
                context.Response.StatusCode,
                success,
                request.ReadHeadersAsString(),
                request.ReadRequestBodyAsString(),
                context.Response,
                started);
        }

        public static void LogActorMethod<TActor>(this ILogger logger, ActorMethodContext context, DateTime started, TimeSpan duration) where TActor : Actor
        {
            logger.LogInformation(ServiceFabricEvent.ActorMethod,
                "The {CallType} call to actor {Actor}  method {MethodName} finished in {Duration} ms (started at ({Started}))",
                context.CallType,
                typeof(TActor).FullName,
                context.MethodName,
                duration.TotalMilliseconds,
                started);
        }

        public static void LogStatelessServiceStartedListening<T>(this ILogger logger, string endpoint) where T : StatelessService
        {
            logger.LogInformation(ServiceFabricEvent.ServiceListening,
                "The stateless service {ServiceType} started listening (endpoint {Endpoint})", typeof(T).FullName, endpoint);
        }

        public static void LogStatefulServiceStartedListening<T>(this ILogger logger, string endpoint) where T : StatefulService
        {
            logger.LogInformation(ServiceFabricEvent.ServiceListening,
               "The stateful service {ServiceType} started listening (endpoint {Endpoint})", typeof(T).FullName, endpoint);
        }

        public static void LogStatelessServiceInitalizationFailed<T>(this ILogger logger, Exception exception) where T : StatelessService
        {
            logger.LogCritical(ServiceFabricEvent.ServiceInitializationFailed, exception,
                "The stateless service {ServiceType} failed to initialize.", typeof(T).FullName);
        }

        public static void LogStatefulServiceInitalizationFailed<T>(this ILogger logger, Exception exception) where T : StatefulService
        {
            logger.LogCritical(ServiceFabricEvent.ServiceInitializationFailed, exception,
                "The stateful service {ServiceType} failed to initialize.", typeof(T).FullName);
        }

        public static void LogActorHostInitalizationFailed<T>(this ILogger logger, Exception exception) where T : Actor
        {
            logger.LogCritical(ServiceFabricEvent.ActorHostInitializationFailed, exception,
                "The stateful service {ServiceType} failed to initialize.", typeof(T).FullName);
        }
    }
} 