using Microsoft.AspNetCore.Builder;

namespace ServiceFabric.Logging.Middleware
{
    public static class RequestTrackingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestTracking(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTrackingMiddleware>();
        }
    }
}