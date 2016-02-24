using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CouponRequest : PaginatedRequest
    {
        public int? CouponId { get; set; }
        public string CouponCode { get; set; }
        public string CouponName { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
        public string Status { get; set; }
        public int? Remaining { get; set; }
        public string SearchText { get; set; }
        public ActionRequest Action { get; set; }
        public ConditionRequest Conditions { get; set; }
        public int? UsagePerCustomer { get; set; }
        public int? MaximumUser { get; set; }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "CouponName");
            base.DefaultOnNull();
        }

        public CouponRequest()
        {
            Action = new ActionRequest();
            Conditions = new ConditionRequest();
        }
    }

    public class ActionRequest
    {
        public string Type { get; set;}
        public decimal? DiscountAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        
    }

    public class ConditionRequest
    {
        public List<OrderRequest> Order { get; set; }
        public FilterByRequest FilterBy { get; set; }
        public List<String> Include { get; set; }
        public List<String> Exclude { get; set; }


        public ConditionRequest()
        {
            Order = new List<OrderRequest>();
            FilterBy = new FilterByRequest();
            Include = new List<string>();
            Exclude = new List<string>();
        }

    }

    public class OrderRequest
    {
        public string Type { get; set; }
        public decimal? Value { get; set; }

    }
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
            Brands = new List<BrandRequest>();
            Emails = new List<string>();
            GlobalCategories = new List<CategoryRequest>();
            LocalCategories = new List<CategoryRequest>();
           
        }
    }

}
