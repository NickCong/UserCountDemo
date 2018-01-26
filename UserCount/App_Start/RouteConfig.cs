using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace UserCount
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "CustomRoute1",
                 "Demo/{id}/{action}",
                 new { controller = "Demo", id = ConfigurationManager.AppSettings["DomainSourceID"] }
            );
        }
    }
}
