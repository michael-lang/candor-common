using Candor.Web.Mvc.Filters;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(CandorMvcApplication.AppStart_RegisterErrorRouteAndFilter), "PreStartup")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(CandorMvcApplication.AppStart_RegisterErrorRouteAndFilter), "PostStartup")]

namespace CandorMvcApplication
{
    public static class AppStart_RegisterErrorRouteAndFilter
    {
        public static void PreStartup() 
        {
            //may edit/add customErrors section here in a later version?s

            RegisterFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
        public static void PostStartup()
        {
        }

        public static void RegisterFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleAndLogErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "NotFound",
                url: "NotFound/{action}/{id}",
                defaults: new
                {
                    controller = "Error",
                    action = "NotFound",
                    id = UrlParameter.Optional
                });
        }
    }
}