using System.Collections.Generic;
using System.Configuration;
using System.Globalization;

namespace Colsp.Api.Constants
{
    public static class Constant
    {

		public static readonly string SE_Asia_Standard_Time = "SE Asia Standard Time";

		public static readonly List<int> IGNORE_INVENTORY_SHIPPING = new List<int>() { 3, 10 };
        public static readonly List<int> IGNORE_PRICE_SHIPPING = new List<int>() { 3 };

		public static readonly List<string> IMPORT_REQUIRE_FIELD = new List<string>() {
			"AAA",
			"AAB",
			"AAC",
			"AAD",
		};

        public static readonly int DEFAULT_GLOBAL_CATEGORY = 1;

        public static readonly string PRODUCT_IMAGE_URL = string.Concat(AppSettingKey.IMAGE_STATIC_URL, "productimages/large/");

        public static readonly int DEFAULT_THE_ONE_CARD = 1;

        public static readonly int DEFAULT_ADD_YEAR = 10;

        public static readonly int BULK_APPROVE_LIMIT = 100;

        public static readonly int DEFAULT_SHIPPING_ID = 1;


        public static readonly int ADMIN_SHOP_ID = 0;

        public enum ImageRatio  { IMAGE_RATIO_16_9 };

        public static readonly int HISTORY_REVISION = 10;
        public static readonly int TOP_SELLING = 10;

        public static readonly decimal IMAGE_RATIO_16_9 = 16 / 9;

        public static readonly CultureInfo DATETIME_FORMAT =  new CultureInfo("es-ES");

        public static readonly string AUTHEN_SCHEMA = "Bearer";




        public static readonly int SHIPPING_DROP_SHIP_3PL = 1;
        public static readonly int SHIPPING_FULFILLMENT = 2;

        public static readonly int DEFAULT_BOOSTWEIGHT = 5000;
		public static readonly int DEFAULT_GLOBAL_BOOSTWEIGHT = 5000;

		public static List<char> IGNORE_PID = new List<char>() {
                    'D',
                    'E',
                    'F',
                    'G',
                    'H',
                    'I',
                    'M',
                    'N',
                    'O',
                    'S'
        };
        public static readonly string START_PID = "1111110";

        public static readonly int SHOP_OWNER_GROUP_ID = 2;
        public static readonly Dictionary<string, int> STOCK_TYPE = new Dictionary<string, int>() { { "Stock", 1 }, { "Pre-Order", 2 } };
        public static readonly string DEFAULT_STOCK_TYPE = "Stock";

        public static readonly int MAX_LOCAL_CATEGORY = 50;

        public static readonly string INVENTORY_STATUS_ADD = "AD";
        public static readonly string INVENTORY_STATUS_UPDATE = "UD";
        public static readonly string INVENTORY_STATUS_DELETE = "DE";

        //SQL exception
        public static readonly int MAX_RETRY_DEADLOCK = 3;
        public static readonly string UNIQUE_CONSTRAIN_PREFIX = "CK";
        public static readonly string UNIQUE_CONSTRAIN_DELIMETER = "_";
        public static readonly string UNIQUE_CONSTRAIN_SURFFIX = "has already been used.";


        public static readonly string STATUS_ACTIVE = "AT";
        public static readonly string STATUS_NOT_ACTIVE = "NA";
        
        public static readonly string STATUS_VISIBLE = "VI";
        public static readonly string STATUS_NOT_VISIBLE = "NV";
        public static readonly string STATUS_YES = "Y";
        public static readonly string STATUS_NO = "N";

        public static readonly string DATA_TYPE_STRING = "ST";
        public static readonly string DATA_TYPE_LIST = "LT";
        public static readonly string DATA_TYPE_CHECKBOX = "CB";
        public static readonly string DATA_TYPE_HTML = "HB";

        public static readonly string PRODUCT_STATUS_DRAFT              = "DF";
        public static readonly string PRODUCT_STATUS_WAIT_FOR_APPROVAL  = "WA";
        public static readonly string PRODUCT_STATUS_JUNK               = "JU";
        public static readonly string PRODUCT_STATUS_APPROVE            = "AP";
        public static readonly string PRODUCT_STATUS_NOT_APPROVE        = "RJ";
        public static readonly string STATUS_REMOVE                     = "RM";


        public static readonly string USER_TYPE_ADMIN = "A";
        public static readonly string USER_TYPE_SELLER = "S";
        public static readonly string SHOP_TYPE = "H";

        public static readonly string COUPON_FILTER_INCLUDE = "I";
        public static readonly string COUPON_FILTER_EXCLUDE = "E";

        public static readonly string NEWSLETTER_FILTER_INCLUDE = "I";
        public static readonly string NEWSLETTER_FILTER_EXCLUDE = "E";

        public static readonly string NEWSLETTER_VISIBLE_TO_ALL = "AL";
        public static readonly string NEWSLETTER_VISIBLE_TO_BU = "BU";
        public static readonly string NEWSLETTER_VISIBLE_TO_INDY = "IN";
        public static readonly string NEWSLETTER_VISIBLE_TO_MERCHANT = "ME";


        public static readonly string SHOP_GROUP_BU = "BU";
        public static readonly string SHOP_GROUP_INDY = "IN";
        public static readonly string SHOP_GROUP_MERCHANT = "ME";

        public static readonly string ATTRIBUTE_VISIBLE_ADMIN = "AD";
        public static readonly string ATTRIBUTE_VISIBLE_ALL_USER = "ME";


        public static readonly string LANG_EN = "EN";
        public static readonly string LANG_TH = "TH";

        public static readonly string SMALL = "S";
        public static readonly string MEDIUM = "M";
        public static readonly string LARGE = "L";

        public static readonly string DIMENSTION_MM = "MM";
        public static readonly string DIMENSTION_CM = "CM";
        public static readonly string DIMENSTION_M = "M";

        public static readonly string WEIGHT_MEASURE_G = "G";
        public static readonly string WEIGHT_MEASURE_KG = "KG";

        public static readonly string ATTRIBUTE_VALUE_MAP_PREFIX = "((";
        public static readonly string ATTRIBUTE_VALUE_MAP_SURFIX = "))";

        public static readonly string VARIANT_DISPLAY_GROUP = "GROUP";
        public static readonly string VARIANT_DISPLAY_INDIVIDUAL = "INDIVIDUAL";


        public static readonly string ORDER_PAYMENT_PENDING = "PP";
        public static readonly string ORDER_PAYMENT_CONFIRM = "PC";
        public static readonly string ORDER_PREPARING = "PE";
        public static readonly string ORDER_READY_TO_SHIP = "RS";
        public static readonly string ORDER_SHIPPING = "SH";
        public static readonly string ORDER_DELIVERED = "DE";
        public static readonly string ORDER_CANCELED = "CA";

        public static readonly string RETURN_STATUS_WAIT_FOR_APPROVAL = "WA";
        public static readonly string RETURN_STATUS_APPROVE = "AP";

        public static readonly string NOT_AVAILABLE = "N/A";

        public static readonly double CACHE_TIMEOUT = 60;

        public static string STATUS_PROMOTION_INACTIVE = "IA";
        public static string STATUS_PROMOTION_ACTIVE = "AT";


		public static string COUPON_ACTION_AMOUNT = "AMOUNT";
		public static string COUPON_ACTION_PERCENT = "PERCENT";


		#region CMS
		public static readonly int CMS_SHOP_GOBAL = 0;

        public static readonly string CMS_STATUS_DRAFT              = "DF";
        public static readonly string CMS_STATUS_WAIT_FOR_APPROVAL  = "WA";
        public static readonly string CMS_STATUS_JUNK               = "JU";
        public static readonly string CMS_STATUS_APPROVE            = "AP";
        public static readonly string CMS_STATUS_NOT_APPROVE        = "RJ";
        public static readonly string CMS_STATUS_REMOVE             = "RM";



        public static readonly string CMS_MASTER_TYPE_STATIC        = "ST";
        public static readonly string CMS_MASTER_TYPE_COLLECTION    = "CL";

        public static readonly int CMS_TYPE_STATIC_PAGE = 1;
        public static readonly int CMS_TYPE_COLLECTION_PAGE = 2;
        #endregion
    }
}