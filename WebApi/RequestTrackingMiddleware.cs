using Interface.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi
{
    public class RequestTrackingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public RequestTrackingMiddleware(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            using (logger.BeginScope(new Dictionary<string, object>
            {
                ["TraceId"] = context.Request.HttpContext.TraceIdentifier,
                ["Method"] = context.Request.Method,
                ["Host"] = context.Request.Host,
                ["Path"] = context.Request.Path
            }))
            {
                var startTime = DateTime.Now;

                await next(context);

                logger.LogInformation(LoggingEvents.REQUEST, "The request took {timeInMs} ms", (DateTime.Now - startTime).TotalMilliseconds);
            }
        }
    }
}
