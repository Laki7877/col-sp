using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class CategoryRequest
    {
        public int CategoryId { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string UrlKeyEn { get; set; }
        public string UrlKeyTh { get; set; }
        public string Status { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public int ShopId { get; set; }
        //public string CategoryAbbreviation { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }
        public decimal Commission { get; set; }
        public bool Visibility { get; set; }
        public List<ImageRequest> CategoryBannerEn { get; set; }
        public List<ImageRequest> CategoryBannerTh { get; set; }
        public string FeatureTitle { get; set; }
        public bool TitleShowcase { get; set; }
        public List<ProductRequest> FeatureProducts { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionShortTh { get; set; }

        public CategoryRequest()
        {
            CategoryId = 0;
            NameEn = string.Empty;
            NameTh = string.Empty;
            UrlKeyEn = string.Empty;
            UrlKeyTh = string.Empty;
            Status = string.Empty;
            Lft = 0;
            Rgt = 0;
            ShopId = 0;
            Commission = 0;
            Visibility = false;
            FeatureTitle = string.Empty;
            TitleShowcase = false;
            DescriptionFullEn = string.Empty;
            DescriptionFullTh  = string.Empty;
            DescriptionShortEn = string.Empty;
            DescriptionShortTh = string.Empty;
            AttributeSets = new List<AttributeSetRequest>();
            CategoryBannerEn = new List<ImageRequest>();
            CategoryBannerTh = new List<ImageRequest>();
            FeatureProducts = new List<ProductRequest>();
        }
    }
}
