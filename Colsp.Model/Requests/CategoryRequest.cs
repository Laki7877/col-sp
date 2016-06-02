using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class CategoryRequest
    {
        public int CategoryId { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string UrlKey { get; set; }
        public string Status { get; set; }
        public int Lft { get; set; }
        public int Rgt { get; set; }
        public int ShopId { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }
        public decimal Commission { get; set; }
        public bool Visibility { get; set; }
        public List<ImageRequest> CategoryBannerEn { get; set; }
        public List<ImageRequest> CategoryBannerTh { get; set; }
        public List<ImageRequest> CategorySmallBannerEn { get; set; }
        public List<ImageRequest> CategorySmallBannerTh { get; set; }
        public string FeatureTitle { get; set; }
        public bool TitleShowcase { get; set; }
        public List<ProductRequest> FeatureProducts { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionShortTh { get; set; }
        public string DescriptionMobileEn { get; set; }
        public string DescriptionMobileTh { get; set; }
        public bool BannerSmallStatusEn { get; set; }
        public bool BannerStatusEn { get; set; }
        public bool BannerSmallStatusTh { get; set; }
        public bool BannerStatusTh { get; set; }
        public bool FeatureProductStatus { get; set; }
        public SortByRequest SortBy { get; set; }

        // Exclude in Local categories for Coupon
        public List<ProductRequest> Exclude { get; set; }
        public List<string> Include { get; set; }
        public List<string> Exclude { get; set; }
        public bool IsLandingPage { get; set; }

        public CategoryRequest()
        {
            CategoryId = 0;
            NameEn = string.Empty;
            NameTh = string.Empty;
            UrlKey = string.Empty;
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
            CategorySmallBannerEn = new List<ImageRequest>();
            CategorySmallBannerTh = new List<ImageRequest>();
            DescriptionMobileEn = string.Empty;
            DescriptionMobileTh = string.Empty;
            SortBy = new SortByRequest();
            Exclude = new List<ProductRequest>();
            Include = new List<string>();
            Exclude = new List<string>();
            IsLandingPage = false;
        }
    }
}
