using System.Collections.Generic;

namespace Colsp.Model.Requests
{
    public class FilterByRequest
    {
        public string Type { get; set; }
        public List<BrandRequest> Brands { get; set; }
        public List<string> Emails { get; set; }
        public List<CategoryRequest> GlobalCategories { get; set; }
        public List<CategoryRequest> LocalCategories { get; set; }
        public List<ShopRequest> Shops { get; set; }

        public FilterByRequest()
        {
            Type = string.Empty;
            Brands = new List<BrandRequest>();
            Emails = new List<string>();
            GlobalCategories = new List<CategoryRequest>();
            LocalCategories = new List<CategoryRequest>();

        }
    }
}
