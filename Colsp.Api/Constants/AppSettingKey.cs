using System.Configuration;
using System.Web;

namespace Colsp.Api.Constants
{
    public static class AppSettingKey
    {

        //Image Root & Folder 
        public static readonly string IMAGE_ROOT_FOLDER = ConfigurationManager.AppSettings["ImageRootPath"];
        public static readonly string IMAGE_ROOT_PATH = HttpContext.Current.Server.MapPath(string.Concat("~/" , IMAGE_ROOT_FOLDER));
        public static readonly string ORIGINAL_FOLDER = ConfigurationManager.AppSettings["OriginalFolder"];
        public static readonly string ZOOM_FOLDER = ConfigurationManager.AppSettings["ZoomFolder"];
        public static readonly string LARGE_FOLDER = ConfigurationManager.AppSettings["LargeFolder"];
        public static readonly string NORMAL_FOLDER = ConfigurationManager.AppSettings["NormalFolder"];
        public static readonly string THUMBNAIL_FOLDER = ConfigurationManager.AppSettings["ThumbnailFolder"];
        public static readonly string PRODUCT_FOLDER = ConfigurationManager.AppSettings["ImageProductFolder"];
        public static readonly string BRAND_FOLDER = ConfigurationManager.AppSettings["ImageBrandFolder"];
        public static readonly string CMS_FOLDER = ConfigurationManager.AppSettings["ImageCMSFolder"];
        public static readonly string SHOP_FOLDER = ConfigurationManager.AppSettings["ImageShopFolder"];
        public static readonly string ATTRIBUTE_VALUE_FOLDER = ConfigurationManager.AppSettings["AttributeValueFolder"];
        public static readonly string GLOBAL_CAT_FOLDER = ConfigurationManager.AppSettings["GlobalCatFolder"];
        public static readonly string LOCAL_CAT_FOLDER = ConfigurationManager.AppSettings["LocalCatFolder"];
        public static readonly string NEWSLETTER_FOLDER = ConfigurationManager.AppSettings["NewsletterFolder"];
        public static readonly string THEME_FOLDER = ConfigurationManager.AppSettings["ThemeFolder"];
        public static readonly string IMAGE_TMP_FOLDER = ConfigurationManager.AppSettings["ImageTmpFolder"];


        //CSV Root
        public static readonly string IMPORT_ROOT_PATH = HttpContext.Current.Server.MapPath(string.Concat("~/", ConfigurationManager.AppSettings["ImportTmpFolder"]));
        public static readonly string EXPORT_ROOT_PATH = HttpContext.Current.Server.MapPath(string.Concat("~/", ConfigurationManager.AppSettings["ExportTmpFolder"]));


        //Pid Number Only
        public static readonly bool PID_NUMBER_ONLY = bool.Parse(ConfigurationManager.AppSettings["PidNumberOnly"]);


		public static readonly string IMAGE_STATIC_URL = ConfigurationManager.AppSettings["ImageStaticUrl"];
	}
}