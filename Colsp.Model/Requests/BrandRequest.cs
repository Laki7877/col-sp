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
        public string SearchText { get; set; }
        public string BrandNameTh { get; set; }
        public string ThumbnailEn { get; set; }
        public string ThumbnailTh { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionTh { get; set; }
        public ImageRequest BrandImage { get; set; }
        public SEORequest SEO { get; set; }
        public override void DefaultOnNull()
        {
            BrandId = GetValueOrDefault(BrandId, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "BrandId");
            base.DefaultOnNull();
        }
    }
}
