using Colsp.Api.Constants;
using Colsp.Api.Filters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Colsp.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			// Json self reference handling
			config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
				= Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			config.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling
				= Newtonsoft.Json.PreserveReferencesHandling.Objects;
			config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling
				= Newtonsoft.Json.NullValueHandling.Ignore;

			// Enable CORs
			config.EnableCors(new EnableCorsAttribute("*","*","*"));

			// Setup authorization and authentication filters
			config.Filters.Add(new BasicAuthenticateAttribute());
			config.Filters.Add(new AuthorizeAttribute());

			// Create web api routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //crearte folder for image
            string imgageRootPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[AppSettingKey.IMAGE_ROOT_PATH]);
            string imageTmpFolder = ConfigurationManager.AppSettings[AppSettingKey.IMAGE_TMP_FOLDER];
            string rootImagePath = Path.Combine(imgageRootPath, imageTmpFolder);
            if (!Directory.Exists(rootImagePath))
            {
                Directory.CreateDirectory(rootImagePath);
            }
        }
    }
}
