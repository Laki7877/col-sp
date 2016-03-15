using System.Collections.Generic;

namespace Colsp.Api.Constants
{
    public static class Constant
    {
        public static List<char> IGNORE_PID = new List<char>() {
                    '0',
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
        public static string START_PID = "1111110";

        public static int SHOP_OWNER_GROUP_ID = 2;
        public static readonly Dictionary<string, int> STOCK_TYPE = new Dictionary<string, int>() { { "Stock", 1 }, { "Pre-Order", 2 } };

        public static readonly int MAX_LOCAL_CATEGORY = 8;

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
        public static readonly string STATUS_REMOVE = "RM";
        public static readonly string STATUS_VISIBLE = "VI";
        public static readonly string STATUS_NOT_VISIBLE = "NV";
        public static readonly string STATUS_YES = "Y";
        public static readonly string STATUS_NO = "N";

        public static readonly string DATA_TYPE_STRING = "ST";
        public static readonly string DATA_TYPE_LIST = "LT";
        public static readonly string DATA_TYPE_CHECKBOX = "CB";

        public static readonly string PRODUCT_STATUS_DRAFT = "DF";
        public static readonly string PRODUCT_STATUS_WAIT_FOR_APPROVAL = "WA";
        public static readonly string PRODUCT_STATUS_JUNK = "JU";
        public static readonly string PRODUCT_STATUS_APPROVE = "AP";
        public static readonly string PRODUCT_STATUS_NOT_APPROVE = "RJ";

        


        public static readonly string USER_TYPE_ADMIN = "A";
        public static readonly string USER_TYPE_SELLER = "S";
        public static readonly string SHOP_TYPE = "H";

        public static string STATUS_PROMOTION_INACTIVE = "IA";
        public static string STATUS_PROMOTION_ACTIVE = "AT";

        public static readonly string COUPON_FILTER_INCLUDE = "I";
        public static readonly string COUPON_FILTER_EXCLUDE = "E";
                

        #region CMS
        public static readonly int CMS_SHOP_GOBAL = 0;

        public static readonly int CMS_STATUS_DRAFT = 1;
        public static readonly int CMS_STATUS_WAIT_FOR_APPROVAL = 4;
        public static readonly int CMS_STATUS_JUNK = 5;
        public static readonly int CMS_STATUS_APPROVE = 2;
        public static readonly int CMS_STATUS_NOT_APPROVE = 3;

        public static readonly int CMS_TYPE_STATIC_PAGE = 1;
        public static readonly int CMS_TYPE_COLLECTION_PAGE = 2;
        #endregion

        public static readonly string SHOP_GROUP_BU = "BU";
        public static readonly string SHOP_GROUP_INDY = "IN";
        public static readonly string SHOP_GROUP_MERCHANT = "ME";

        

        public static readonly string LANG_EN = "EN";
        public static readonly string LANG_TH = "TH";

        public static readonly string DIMENSTION_MM = "MM";
        public static readonly string DIMENSTION_CM = "CM";
        public static readonly string DIMENSTION_M = "M";

        public static readonly string WEIGHT_MEASURE_G = "G";
        public static readonly string WEIGHT_MEASURE_KG = "KG";

        public static readonly string ATTRIBUTE_VALUE_MAP_PREFIX = "((";
        public static readonly string ATTRIBUTE_VALUE_MAP_SURFIX = "))";

        public static readonly string VARIANT_DISPLAY_GROUP = "GROUP";
        public static readonly string VARIANT_DISPLAY_INDIVIDUAL = "INDIVIDUAL";
    }
}