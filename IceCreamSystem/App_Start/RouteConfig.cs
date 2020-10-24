using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace IceCreamSystem
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            _ = routes.MapRoute(
                name: "Default",
                url: "{action}",
                defaults: new { controller = "Employees", action = "Login" }
            );

            routes.MapMvcAttributeRoutes();

        }
    }
}
