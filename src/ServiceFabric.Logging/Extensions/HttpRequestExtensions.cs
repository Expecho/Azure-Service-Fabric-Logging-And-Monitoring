using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ServiceFabric.Logging.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string ReadRequestBodyAsString(this HttpRequest request)
        {
            string requestBodyAsText = "";

            if (!HttpMethods.IsGet(request.Method)
                && !HttpMethods.IsHead(request.Method)
                && !HttpMethods.IsDelete(request.Method)
                && !HttpMethods.IsTrace(request.Method)
                && request.ContentLength > 0)
            {
                using (var reader = new StreamReader(request.Body))
                {
                    requestBodyAsText = reader.ReadToEnd();
                }
            }
            return requestBodyAsText;
        }

        public static string ReadHeadersAsString(this HttpRequest request)
        {
            var headers = new StringBuilder();
            foreach (string key in request.Headers.Keys)
            {
                string value = request.Headers[key];
                if (!string.IsNullOrEmpty(value))
                {
                    headers.AppendLine($"{key}={value}");
                }
            }
            return headers.ToString();
        }
    }
}
