using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public class ElasticRequest
	{
		public string Pid { get; set; }
		public long ProductId { get; set; }
		public string ParentPId { get; set; }
		public string Sku { get; set; }
		public string MasterPid { get; set; }
		public int ShopId { get; set; }
		public string ShopNameEn { get; set; }
		public string ShopNameTh { get; set; }
		public string ShopUrlKey { get; set; }
		public string Bu { get; set; }
		public int GlobalCatId { get; set; }
		public string GlobalCatNameEn { get; set; }
		public string GlobalCatNameTh { get; set; }
		public int GlobalRootCatId { get; set; }
		public string GlobalRootCatNameEn { get; set; }
		public string GlobalRootCatNameTh { get; set; }
		public int? LocalCatId { get; set; }
		public string LocalCatNameEn { get; set; }
		public string LocalCatNameTh { get; set; }
		public int? LocalRootCatId { get; set; }
		public string LocalRootCatNameEn { get; set; }
		public string LocalRootCatNameTh { get; set; }
		public int? BrandId { get; set; }
		public string BrandNameEn { get; set; }
		public string BrandNameTh { get; set; }
		public string ProductNameEn { get; set; }
		public string ProductNameTh { get; set; }
		public string Upc { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal SalePrice { get; set; }
		public decimal PromotionPrice { get; set; }
		public decimal PriceRangeMin { get; set; }
		public decimal PriceRangeMax { get; set; }
		public decimal Discount { get; set; }
		public string ProductRating { get; set; }
		public string DescriptionFullEn { get; set; }
		public string DescriptionFullTh { get; set; }
		public int ImageCount { get; set; }
		public string Installment { get; set; }
		public string EffectiveDate { get; set; }
		public string ExpireDate { get; set; }
		public string EffectiveDatePromotion { get; set; }
		public string ExpireDatePromotion { get; set; }
		public string ExpressDelivery { get; set; }
		public bool IsNew { get; set; }
		public bool IsClearance { get; set; }
		public bool IsBestSeller { get; set; }
		public bool IsOnlineExclusive { get; set; }
		public bool IsOnlyAt { get; set; }
		public string MetaKeyEn { get; set; }
		public string MetaKeyTh { get; set; }
		public string SeoEn { get; set; }
		public string SeoTh { get; set; }
		public string UrlKey { get; set; }
		public int BoostWeight { get; set; }
		public int GlobalBoostWeight { get; set; }
		public bool IsMaster { get; set; }
		public bool IsVariant { get; set; }
		public bool IsSell { get; set; }
		public bool Visibility { get; set; }
		public string Status { get; set; }
		public string Display { get; set; }
		public string CreateDate { get; set; }
		public string UpdateDate { get; set; }
		public string NewArrivalDate { get; set; }
		public bool IsLocalSearch { get; set; }
		public bool IsGlobalSearch { get; set; }
		public bool IsSearch { get; set; }
		public string StockAvailable { get; set; }
		public List<string> tag { get; set; }
		public DefaultProductRequest DefaultProduct { get; set; }
		public List<ElasticAttributeRequest> Attributes { get; set; }

		public ElasticRequest()
		{
			DefaultProduct = new DefaultProductRequest();
			Attributes = new List<ElasticAttributeRequest>();
		}
	}
	public class DefaultProductRequest
	{
		public string Pid { get; set; }
		public string ShopUrlKey { get; set; }
	}

	public class ElasticAttributeRequest
	{
		public int AttributeId { get; set; }
		public string AttributeNameEn { get; set; }
		public string AttributeNameTh { get; set; }
		public bool ShowLocalSearchFlag { get; set; }
		public bool ShowGlobalSearchFlag { get; set; }
		public bool IsFilterable { get; set; }
		public bool IsVariant { get; set; }
		public string FreeTextValueEn { get; set; }
		public string FreeTextValueTh { get; set; }
		public bool IsAttributeValue { get; set; }
		public List<ElasticAttributeValueRequest> AttributeValues { get; set; }

		public ElasticAttributeRequest()
		{
			AttributeValues = new List<ElasticAttributeValueRequest>();
		}

	}

	public class ElasticAttributeValueRequest
	{
		public int AttributeValueId { get; set; }
		public string AttributeValueNameEn { get; set; }
		public string AttributeValueNameTh { get; set; }
		public string ImageUrl { get; set; }
	}

}
