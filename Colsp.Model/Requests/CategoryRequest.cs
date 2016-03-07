using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CategoryRequest
    {
        public int? CategoryId { get; set; }
        public string NameEn { get; set; }
        public string NameTh { get; set; }
        public string UrlKeyEn { get; set; }
        public string Status { get; set; }
        public int? Lft { get; set; }
        public int? Rgt { get; set; }
        public int? ShopId { get; set; }
        public string CategoryAbbreviation { get; set; }
        public List<AttributeSetRequest> AttributeSets { get; set; }
        public decimal? Commission { get; set; }
        public bool? Visibility { get; set; }
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
            AttributeSets = new List<AttributeSetRequest>();
            CategoryBannerEn = new List<ImageRequest>();
            CategoryBannerTh = new List<ImageRequest>();
            FeatureProducts = new List<ProductRequest>();
        }
    }

    public class CategoryShiftRequest
    {
        public int? Parent { get; set; }
        public int? Sibling { get; set; }
        public int? Child { get; set; }
    }
}
