using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Colsp.Api.Constants
{
    public static class AppSettingKey
    {

        //Image Root & Folder 
        public static readonly string IMAGE_ROOT_FOLDER = ConfigurationManager.AppSettings["ImageRootPath"];
        public static readonly string IMAGE_ROOT_PATH = HttpContext.Current.Server.MapPath(string.Concat("~/" , IMAGE_ROOT_FOLDER));
        public static readonly string TMP_FOLDER = ConfigurationManager.AppSettings["ImageTmpFolder"];
        public static readonly string PRODUCT_FOLDER = ConfigurationManager.AppSettings["ImageProductFolder"];
        public static readonly string BRAND_FOLDER = ConfigurationManager.AppSettings["ImageBrandFolder"];
        public static readonly string SHOP_FOLDER = ConfigurationManager.AppSettings["ImageShopFolder"];
        public static readonly string ATTRIBUTE_VALUE_FOLDER = ConfigurationManager.AppSettings["AttributeValueFolder"];
        public static readonly string GLOBAL_CAT_FOLDER = ConfigurationManager.AppSettings["GlobalCatFolder"];
        public static readonly string LOCAL_CAT_FOLDER = ConfigurationManager.AppSettings["LocalCatFolder"];


        //CSV Root
        public static readonly string IMPORT_ROOT_FOLDER = ConfigurationManager.AppSettings["ImportTmpFolder"];
        public static readonly string IMPORT_ROOT_PATH = HttpContext.Current.Server.MapPath(string.Concat("~/", IMPORT_ROOT_FOLDER));


    }
}