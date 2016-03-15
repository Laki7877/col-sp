using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class BrandRequest : PaginatedRequest
    {
        public int  BrandId                         { get; set; }
        public string BrandNameEn                   { get; set; }
        public string BrandNameTh                   { get; set; }
        public string DisplayNameEn                 { get; set; }
        public string DisplayNameTh                 { get; set; }
        public string ThumbnailEn                   { get; set; }
        public string ThumbnailTh                   { get; set; }
        public string DescriptionFullEn             { get; set; }
        public string DescriptionFullTh             { get; set; }
        public string DescriptionShortEn            { get; set; }
        public string DescriptionShortTh            { get; set; }
        public ImageRequest BrandImage              { get; set; }
        public List<ImageRequest> BrandBannerEn     { get; set; }
        public List<ImageRequest> BrandBannerTh     { get; set; }
        public SEORequest SEO                       { get; set; }
        public string SearchText                    { get; set; }
        public string FeatureTitle                  { get; set; }
        public bool TitleShowcase                   { get; set; }
        public List<ProductRequest> FeatureProducts { get; set; }

        public BrandRequest()
        {
            BrandId = 0;
            BrandNameEn         = string.Empty;
            BrandNameTh         = string.Empty;
            DisplayNameEn       = string.Empty;
            DisplayNameTh       = string.Empty;
            ThumbnailEn         = string.Empty;
            ThumbnailTh         = string.Empty;
            DescriptionFullEn   = string.Empty;
            DescriptionFullTh   = string.Empty;
            DescriptionShortEn  = string.Empty;
            DescriptionShortTh  = string.Empty;
            BrandImage = new ImageRequest();
            BrandBannerEn = new List<ImageRequest>();
            BrandBannerTh = new List<ImageRequest>();
            SEO = new SEORequest();
            SearchText = string.Empty;
            FeatureTitle = string.Empty;
            TitleShowcase = false;
            FeatureProducts = new List<ProductRequest>();
        }
        public override void DefaultOnNull()
        {
            BrandId = GetValueOrDefault(BrandId, 0);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "BrandId");
            base.DefaultOnNull();
        }

    }
}
