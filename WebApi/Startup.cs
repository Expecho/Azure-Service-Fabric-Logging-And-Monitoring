using System.Web.Http;
using Owin;

namespace WebApi
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            config.MapHttpAttributeRoutes();

            appBuilder.UseWebApi(config);
            appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            config.EnsureInitialized();
        }
    }
}
