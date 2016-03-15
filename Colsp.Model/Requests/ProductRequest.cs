using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class ProductRequest : PaginatedRequest
	{
		public int SellerId { get; set; }
		public string SearchText { get; set; }
        public int CategoryId { get; set; }
        public int GlobalCatId { get; set; }
        public List<string> ProductNames { get; set; }
        public List<string> Pids { get; set; }
        public List<string> Skus { get; set; }
        public List<CategoryRequest> GlobalCategories { get; set; }
        public List<CategoryRequest> LocalCategories { get; set; }
        public List<BrandRequest> Brands { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }
        public decimal PriceFrom { get; set; }
        public decimal PriceTo { get; set; }
      
        public List<string> Tags { get; set; }
        public int LocalCatId { get; set; }
        public int AttributeSetId { get; set; }
        public int AttributeId { get; set; }
        public string Pid { get; set; }
        public string _missingfilter { get; set; }
        public int BrandId { get; set; }
        public int ShopId { get; set; }
        public List<string> SearchTag { get; set; }
        public long ProductId { get; set; }
        public string ProductNameEn { get; set; }


        public string CreatedDtFrom { get; set; }
        public string CreatedDtTo { get; set; }
        public string ModifyDtFrom { get; set; }
        public string ModifyDtTo { get; set; }
        


        public ProductRequest()
        {
            SellerId = 0;
            SearchText = string.Empty;
            CategoryId = 0;
            GlobalCatId = 0;
            PriceFrom = 0;
            PriceTo = 0;
            LocalCatId = 0;
            AttributeSetId = 0;
            AttributeId = 0;
            Pid = string.Empty;
            BrandId = 0;
            ShopId = 0;
            ProductId = 0;
            ProductNameEn = string.Empty;

            _missingfilter = string.Empty;


            SearchTag = new List<string>();
            GlobalCategories = new List<CategoryRequest>();
            LocalCategories = new List<CategoryRequest>();
            Brands = new List<BrandRequest>();
            AttributeSets = new List<AttributeSetRequest>();
            Tags = new List<string>();
            ProductNames = new List<string>();
            Pids = new List<string>();
            Skus = new List<string>();
        }


        public override void DefaultOnNull()
		{
			SellerId = GetValueOrDefault(SellerId, 0);
            CategoryId = GetValueOrDefault(CategoryId, 0);
            GlobalCatId = GetValueOrDefault(GlobalCatId, 0);
            LocalCatId = GetValueOrDefault(LocalCatId, 0);
            AttributeSetId = GetValueOrDefault(AttributeSetId, 0);
            AttributeId = GetValueOrDefault(AttributeId, 0);
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            Pid = GetValueOrDefault(Pid, null);
			_order = GetValueOrDefault(_order, "ProductId");
			base.DefaultOnNull();
		}

        
    }
}
