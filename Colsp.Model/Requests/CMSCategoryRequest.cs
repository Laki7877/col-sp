using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model
{
    public class CMSCategoryRequest : PaginatedRequest
    {
        public CMSCategoryRequest()
        {
            this.CategoryProductList = new List<CMSCategoryProductMapRequest>();
        }

        public int CMSCategoryId { get; set; }
        public string CMSCategoryNameEN { get; set; }
        public string CMSCategoryNameTH { get; set; }
        public bool Visibility { get; set; }
        public string Status { get; set; }
        public int? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public List<CMSCategoryProductMapRequest> CategoryProductList { get; set; }

        public string SearchText { get; set; }

        public override void DefaultOnNull()
        {
            SearchText = GetValueOrDefault(SearchText, string.Empty);
            _order = GetValueOrDefault(_order, "ShopId");
            base.DefaultOnNull();
        }

    }

    public class CMSCategoryProductMapRequest
    {
        public int CMSCategoryProductMapId { get; set; }
        public int? CMSCategoryId { get; set; }
        public string Pid { get; set; }
        public string ProductBoxBadge { get; set; }
        public int? Sequence { get; set; }
        public int? ShopId { get; set; }
        public bool? IsActive { get; set; }
        public int? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreateIP { get; set; }
        public string UpdateIP { get; set; }
        public string FeatureImgUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public DateTime? ExpireDate { get; set; }
    }
}
