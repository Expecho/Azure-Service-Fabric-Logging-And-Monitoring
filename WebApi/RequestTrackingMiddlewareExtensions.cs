using Microsoft.AspNetCore.Builder;

namespace WebApi
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
