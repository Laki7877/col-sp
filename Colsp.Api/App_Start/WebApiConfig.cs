using Colsp.Api.Constants;
using Colsp.Api.Filters;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Routing;
using System.Web.Routing;

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
				= Newtonsoft.Json.PreserveReferencesHandling.None;
			//config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling
			//	= Newtonsoft.Json.NullValueHandling.Ignore;

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

            //enable static image
            config.Routes.IgnoreRoute("ImageRoute", "Images/{file}");


            //crearte folder for image
            // ~/Images
            string imgageRootPath = AppSettingKey.IMAGE_ROOT_PATH;
            // Product Image Tmp folder
            string imageTmpFolder = AppSettingKey.TMP_FOLDER;
            string rootImagePath = Path.Combine(imgageRootPath, imageTmpFolder);
            if (!Directory.Exists(rootImagePath))
            {
                Directory.CreateDirectory(rootImagePath);
            }

            //Product folder
            string rootProductPath = Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.TMP_FOLDER);
            if (!Directory.Exists(rootProductPath))
            {
                Directory.CreateDirectory(rootProductPath);
            }

            //Brand folder
            string rootBrandPath = Path.Combine(imgageRootPath, AppSettingKey.BRAND_FOLDER, AppSettingKey.TMP_FOLDER);
            if (!Directory.Exists(rootBrandPath))
            {
                Directory.CreateDirectory(rootBrandPath);
            }

            //Shop folder
            string rootShopPath = Path.Combine(imgageRootPath, AppSettingKey.SHOP_FOLDER, AppSettingKey.TMP_FOLDER);
            if (!Directory.Exists(rootShopPath))
            {
                Directory.CreateDirectory(rootShopPath);
            }

            //Attribute Value Folder
            string rootAttributeValPath = Path.Combine(imgageRootPath, AppSettingKey.ATTRIBUTE_VALUE_FOLDER, AppSettingKey.TMP_FOLDER);
            if (!Directory.Exists(rootAttributeValPath))
            {
                Directory.CreateDirectory(rootAttributeValPath);
            }

            //Global Category Folder
            string rootGlobalPath = Path.Combine(imgageRootPath, AppSettingKey.GLOBAL_CAT_FOLDER, AppSettingKey.TMP_FOLDER);
            if (!Directory.Exists(rootGlobalPath))
            {
                Directory.CreateDirectory(rootGlobalPath);
            }

            //Local Category Folder
            string rootLocalPath = Path.Combine(imgageRootPath, AppSettingKey.LOCAL_CAT_FOLDER, AppSettingKey.TMP_FOLDER);
            if (!Directory.Exists(rootLocalPath))
            {
                Directory.CreateDirectory(rootLocalPath);
            }

            //CSV Import folder
            string rootExcelPath = AppSettingKey.IMPORT_ROOT_PATH;
            if (!Directory.Exists(rootExcelPath))
            {
                Directory.CreateDirectory(rootExcelPath);
            }
        }
    }
}
