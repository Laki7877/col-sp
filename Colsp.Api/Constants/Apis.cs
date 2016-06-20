namespace Colsp.Api.Constants
{
	public static class Apis
    {
        //private const string CmosRootUrl = "https://devmkp-cmosapi.cenergy.co.th/";
        //private const string ElasticRootUrl = "http://128.199.77.82/elasticsearch/api/v0.2.0/";
		//private const string EVoucherRootUrl = "https://devmkp-api.cenergy.co.th/";
		//private const string OrderRootUrl = "https://devmkp-api.cenergy.co.th/";

		public static readonly string CmosKeyAppIdKey         = "X-Cmos-AppId";
        public static readonly string CmosKeyAppIdValue       = "22778";
        public static readonly string CmosKeyAppSecretKey     = "X-Cmos-AppSecret";
        public static readonly string CmosKeyAppSecretValue   = "Ntr5GsyPe6RD73r5YE2PgG4bQfAAUfbP";

        public static readonly string CmosCreateProduct       = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/cmos/product/create");
        public static readonly string CmosUpdateProduct       = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/cmos/product/update");
        public static readonly string CmosUpdateStatus        = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/cmos/product/updatestatus");
        public static readonly string CmosUpdateStock         = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/cmos/product/updatestock");
        public static readonly string SellerUpdateProduct     = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/seller/product/update");
        public static readonly string SellerUpdateStock       = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/seller/product/updatestock");
        public static readonly string SellerUpdateStatus      = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/seller/product/updatestatus");
        public static readonly string SellerCreateShop        = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/seller/shop/info");
		public static readonly string ShopCreate			  = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/cmos/shop/create");
		public static readonly string ShopUpdate			  = string.Concat(AppSettingKey.CMOS_ROOT_URL , "api/cmos/shop/update");

		public static readonly string ElasticCreateProduct    = string.Concat(AppSettingKey.ELASTIC_ROOT_URL , "reindex/single/insert");
		public static readonly string ElasticUpdateProduct	  = string.Concat(AppSettingKey.ELASTIC_ROOT_URL , "reindex/single/update");

		public static readonly string EVoucherKeyAppIdKey = "X-Central-AppId";
		public static readonly string EVoucherKeyAppIdValue = "1441071018";
		public static readonly string EVoucherKeyAppSecretKey = "X-Central-SecretKey";
		public static readonly string EVoucherKeyAppSecretValue = "8e87cd231a9c2498dd56841cc66f7c78";
		public static readonly string EVoucherKeyVersionKey = "X-Central-Version";
		public static readonly string EVoucherKeyVersionValue = "1";

		public static readonly string EVoucherCreate = string.Concat(AppSettingKey.EVOUCHER_ROOT_URL , "evoucher");
		public static readonly string EVoucherUpdate = string.Concat(AppSettingKey.EVOUCHER_ROOT_URL , "evoucher?id=");

		public static readonly string OrderKeyAppIdKey = "X-Central-AppId";
		public static readonly string OrderKeyAppIdValue = "1441071018";
		public static readonly string OrderKeyAppSecretKey = "X-Central-SecretKey";
		public static readonly string OrderKeyAppSecretValue = "8e87cd231a9c2498dd56841cc66f7c78";
		public static readonly string OrderKeyVersionKey = "X-Central-Version";
		public static readonly string OrderKeyVersionValue = "1";

		public static readonly string SubOrderGetAll = string.Concat(AppSettingKey.ORDER_ROOT_URL , "suborder/list?ShopId=");
		public static readonly string SubOrderGetOne = string.Concat(AppSettingKey.ORDER_ROOT_URL , "suborder/detail?subOrderId=");



	}
}