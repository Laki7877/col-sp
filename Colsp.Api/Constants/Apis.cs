using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Constants
{
    public class Apis
    {
        private const string CmosRootUrl = "https://devmkp-cmosapi.cenergy.co.th/";
        private const string ElasticRootUrl = "http://128.199.77.82/elasticsearch/api/v0.2.0/";
		private const string EVoucherRootUrl = "https://devmkp-api.cenergy.co.th/";
		private const string OrderRootUrl = "https://devmkp-api.cenergy.co.th/";


		public const string CmosKeyAppIdKey         = "X-Cmos-AppId";
        public const string CmosKeyAppIdValue       = "22778";
        public const string CmosKeyAppSecretKey     = "X-Cmos-AppSecret";
        public const string CmosKeyAppSecretValue   = "Ntr5GsyPe6RD73r5YE2PgG4bQfAAUfbP";

        public const string CmosCreateProduct       = CmosRootUrl + "api/cmos/product/create";
        public const string CmosUpdateProduct       = CmosRootUrl + "api/cmos/product/update";
        public const string CmosUpdateStatus        = CmosRootUrl + "api/cmos/product/updatestatus";
        public const string CmosUpdateStock         = CmosRootUrl + "api/cmos/product/updatestock";
        public const string SellerUpdateProduct     = CmosRootUrl + "api/seller/product/update";
        public const string SellerUpdateStock       = CmosRootUrl + "api/seller/product/updatestock";
        public const string SellerUpdateStatus      = CmosRootUrl + "api/seller/product/updatestatus";
        public const string SellerCreateShop        = CmosRootUrl + "api/seller/shop/info";

        public const string ElasticCreateProduct    = ElasticRootUrl + "reindex/single/insert";


		public const string EVoucherKeyAppIdKey = "X-Central-AppId";
		public const string EVoucherKeyAppIdValue = "1441071018";
		public const string EVoucherKeyAppSecretKey = "X-Central-SecretKey";
		public const string EVoucherKeyAppSecretValue = "8e87cd231a9c2498dd56841cc66f7c78";
		public const string EVoucherKeyVersionKey = "X-Central-Version";
		public const string EVoucherKeyVersionValue = "1";

		public const string EVoucherCreate = EVoucherRootUrl + "evoucher";
		public const string EVoucherUpdate = EVoucherRootUrl + "evoucher?id=";



		public const string OrderKeyAppIdKey = "X-Central-AppId";
		public const string OrderKeyAppIdValue = "1441071018";
		public const string OrderKeyAppSecretKey = "X-Central-SecretKey";
		public const string OrderKeyAppSecretValue = "8e87cd231a9c2498dd56841cc66f7c78";
		public const string OrderKeyVersionKey = "X-Central-Version";
		public const string OrderKeyVersionValue = "1";

		public const string SubOrderGetAll = OrderRootUrl + "suborder/list?ShopId=";
		public const string SubOrderGetOne = OrderRootUrl + "suborder/detail?subOrderId=";


	}
}