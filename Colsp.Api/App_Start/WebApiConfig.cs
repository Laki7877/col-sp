using Colsp.Api.Constants;
using Colsp.Api.Filters;
using System;
using System.IO;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Colsp.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            #region Json Formater
            // Json self reference handling
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
				= Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			config.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling
				= Newtonsoft.Json.PreserveReferencesHandling.None;
            //config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling
            //	= Newtonsoft.Json.NullValueHandling.Ignore;
            #endregion

            // Enable CORs
            config.EnableCors(new EnableCorsAttribute("*", "*","*"));
            #region Filter
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
            #endregion
            #region Create Folder
            //crearte folder for image
            // ~/Images
            string imgageRootPath = AppSettingKey.IMAGE_ROOT_PATH;

            //Product Zoom folder
            string rootProductPath = Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.ZOOM_FOLDER);
            if (!Directory.Exists(rootProductPath))
            {
                Directory.CreateDirectory(rootProductPath);
            }
            //Product Large folder
            rootProductPath = Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.LARGE_FOLDER);
            if (!Directory.Exists(rootProductPath))
            {
                Directory.CreateDirectory(rootProductPath);
            }
            //Product Normal folder
            rootProductPath = Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.NORMAL_FOLDER);
            if (!Directory.Exists(rootProductPath))
            {
                Directory.CreateDirectory(rootProductPath);
            }
            //Product Thumbnail folder
            rootProductPath = Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.THUMBNAIL_FOLDER);
            if (!Directory.Exists(rootProductPath))
            {
                Directory.CreateDirectory(rootProductPath);
            }
            rootProductPath = Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.ORIGINAL_FOLDER);
            if (!Directory.Exists(rootProductPath))
            {
                Directory.CreateDirectory(rootProductPath);
            }
            rootProductPath = Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.IMAGE_TMP_FOLDER);
            if (!Directory.Exists(rootProductPath))
            {
                Directory.CreateDirectory(rootProductPath);
            }

            foreach (var file in Directory.GetFiles(Path.Combine(imgageRootPath, AppSettingKey.PRODUCT_FOLDER, AppSettingKey.IMAGE_TMP_FOLDER)))
            {
                if (Path.GetFileName(file).StartsWith("BodyPart", true, null))
                {
                    if(File.GetCreationTime(file).CompareTo(DateTime.Now.AddDays(-1)) > 0)
                    {
                        File.Delete(file);
                    }
                }
            }

            //Brand folder
            string rootBrandPath = Path.Combine(imgageRootPath, AppSettingKey.BRAND_FOLDER);
            if (!Directory.Exists(rootBrandPath))
            {
                Directory.CreateDirectory(rootBrandPath);
            }

            //Shop folder
            string rootShopPath = Path.Combine(imgageRootPath, AppSettingKey.SHOP_FOLDER);
            if (!Directory.Exists(rootShopPath))
            {
                Directory.CreateDirectory(rootShopPath);
            }

            //Attribute Value Folder
            string rootAttributeValPath = Path.Combine(imgageRootPath, AppSettingKey.ATTRIBUTE_VALUE_FOLDER);
            if (!Directory.Exists(rootAttributeValPath))
            {
                Directory.CreateDirectory(rootAttributeValPath);
            }

            //Global Category Folder
            string rootGlobalPath = Path.Combine(imgageRootPath, AppSettingKey.GLOBAL_CAT_FOLDER);
            if (!Directory.Exists(rootGlobalPath))
            {
                Directory.CreateDirectory(rootGlobalPath);
            }

            //Local Category Folder
            string rootLocalPath = Path.Combine(imgageRootPath, AppSettingKey.LOCAL_CAT_FOLDER);
            if (!Directory.Exists(rootLocalPath))
            {
                Directory.CreateDirectory(rootLocalPath);
            }
            //Local Category Folder
            string rootNewsletterPath = Path.Combine(imgageRootPath, AppSettingKey.NEWSLETTER_FOLDER);
            if (!Directory.Exists(rootNewsletterPath))
            {
                Directory.CreateDirectory(rootNewsletterPath);
            }

            //CSV Import folder
            string rootExcelPath = AppSettingKey.IMPORT_ROOT_PATH;
            if (!Directory.Exists(rootExcelPath))
            {
                Directory.CreateDirectory(rootExcelPath);
            }

            //Theme Folder
            string rootThemePath = Path.Combine(imgageRootPath, AppSettingKey.THEME_FOLDER);
            if (!Directory.Exists(rootThemePath))
            {
                Directory.CreateDirectory(rootThemePath);
            }


            //Export Folder
            string rootExportPath = AppSettingKey.EXPORT_ROOT_PATH;
            if (!Directory.Exists(rootExportPath))
            {
                Directory.CreateDirectory(rootExportPath);
            }


            #endregion

        }
    }
}
