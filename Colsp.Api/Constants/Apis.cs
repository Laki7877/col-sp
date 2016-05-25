using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Constants
{
    public class Apis
    {
        public const string CmosRootUrl = "http://devcmos-api.cenergy.co.th/";


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



    }
}