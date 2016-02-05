using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class CMSBrandInShopRequest
    {
        public string CMSNameEN { get; set; }
        public string CMSNameTH { get; set; }
        public string URLKey { get; set; }
        public Nullable<int> CMSTypeId { get; set; }
        public Nullable<int> CMSSortId { get; set; }
        public Nullable<System.DateTime> EffectiveDate { get; set; }
        public Nullable<System.TimeSpan> EffectiveTime { get; set; }
        public Nullable<System.DateTime> ExpiryDate { get; set; }
        public Nullable<System.TimeSpan> ExpiryTime { get; set; }
        public Nullable<int> ShopId { get; set; }
        public Nullable<int> CMSCount { get; set; }
        public string ShortDescriptionTH { get; set; }
        public string LongDescriptionTH { get; set; }
        public string ShortDescriptionEN { get; set; }
        public string LongDescriptionEN { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<bool> Visibility { get; set; }
        public Nullable<int> By { get; set; }
        public string IP { get; set; }
        public Nullable<int> BrandId { get; set; }
        public string BrandNameEN { get; set; }
        public string BrandNameTH { get; set; }
        public string Path { get; set; }
        public string PicUrl { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKey { get; set; }
        public string UrlKey { get; set; }
        public string BrandShortDescriptionEn { get; set; }
        public string BrandLongDescriptionEn { get; set; }
        public string BrandShortDescriptionTh { get; set; }
        public string BrandLongDescriptionTh { get; set; }
        public int? CMSStatusId { get; set; }
        public int? Sequence { get; set; }
    }
}
