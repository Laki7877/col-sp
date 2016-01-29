using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Constants
{
    public static class Constant
    {
        public static readonly string STATUS_ACTIVE = "AT";
        public static readonly string STATUS_NOT_ACTIVE = "NA";
        public static readonly string STATUS_REMOVE = "RM";
        public static readonly string STATUS_VISIBLE = "VI";
        public static readonly string STATUS_NOT_VISIBLE = "NV";

        public static readonly string DATA_TYPE_STRING = "ST";
        public static readonly string DATA_TYPE_LIST = "LT";

        public static Dictionary<string, int?> STOCK_TYPE = new Dictionary<string, int?>() { {"Stock", 1 }, { "Pre-Order" , 2} };


        public static readonly string PRODUCT_STATUS_DRAFT = "DF";
        public static readonly string PRODUCT_STATUS_WAIT_FOR_APPROVAL = "WA";
        public static readonly string PRODUCT_STATUS_JUNK = "JU";
        public static readonly string PRODUCT_STATUS_APPROVE = "AP";
        public static readonly string PRODUCT_STATUS_NOT_APPROVE = "NP";


        public static readonly string USER_TYPE_ADMIN = "A";
        public static readonly string USER_TYPE_SELLER = "S";
    }
}