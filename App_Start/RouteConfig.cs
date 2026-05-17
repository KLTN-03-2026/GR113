using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace demomvc
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // ✅ FIX CHO XemThoiKhoaBieu
            routes.MapRoute(
                name: "XemTKB",
                url: "XemThoiKhoaBieu",
                defaults: new
                {
                    controller = "XemThoiKhoaBieu",
                    action = "Index"
                }
            );

            // ✅ DEFAULT
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new
                {
                    controller = "DangNhap",
                    action = "Index",
                    id = UrlParameter.Optional
                }
            );
        }
    }
}
