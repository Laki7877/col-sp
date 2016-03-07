using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class BrandRequest : PaginatedRequest
    {
        public int? BrandId { get; set; }
        public string BrandNameEn { get; set; }
        public string BrandNameTh { get; set; }
        public string DisplayNameEn { get; set; }
        public string DisplayNameTh { get; set; }
        public string ThumbnailEn { get; set; }
        public string ThumbnailTh { get; set; }
        public string DescriptionFullEn { get; set; }
        public string DescriptionFullTh { get; set; }
        public string DescriptionShortEn { get; set; }
        public string DescriptionShortTh { get; set; }
        public ImageRequest BrandImage { get; set; }
        public List<ImageRequest> BrandBannerEn { get; set; }
        public List<ImageRequest> BrandBannerTh { get; set; }
        public SEORequest SEO { get; set; }
        public string SearchText { get; set; }
        public string FeatureTitle { get; set; }
        public  bool TitleShowcase { get; set; }
        public List<ProductRequest> FeatureProducts { get; set; }

        public override void DefaultOnNull()
        {
            
            BrandId = GetValueOrDefault(BrandId, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "BrandId");
            base.DefaultOnNull();
        }
        public BrandRequest()
        {
            BrandImage = new ImageRequest();
            BrandBannerEn = new List<ImageRequest>();
            BrandBannerTh = new List<ImageRequest>();
            FeatureProducts = new List<ProductRequest>();
            SEO = new SEORequest();
        }
    }
}
