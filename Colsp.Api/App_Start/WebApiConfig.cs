﻿using Colsp.Api.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Colsp.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			// Json self reference handling
			config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
				= Newtonsoft.Json.ReferenceLoopHandling.Ignore;

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
        }
    }
}
