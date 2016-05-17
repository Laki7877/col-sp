using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Logic
{
    public class ProductCondition : BaseCondition
    {
        public SearchOption SearchBy { get; set; }
        public string Tag { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public List<int> CMSCategoryIds { get; set; }
        public List<string> ProductIds { get; set; }

        public List<string> Tags
        {
            get
            {
                if (this.Tag == string.Empty)
                {
                    return null;
                }
                else
                {
                    string tags = FormatInputTag(this.Tag);
                    string[] temp = tags.Split(',');
                    return temp.ToList();
                }
            }
        }

        private string FormatInputTag(string tags)
        {
            string result = string.Empty;

            if (tags.LastIndexOf(',') > 0)
                result = tags.Substring(0, tags.Length - 1);

            return result;
        }
    }

    public enum SearchOption
    {
        [StringValue("PID")]
        PID,

        [StringValue("SKU")]
        SKU,

        [StringValue("ProductName")]
        ProductName
    }
}
