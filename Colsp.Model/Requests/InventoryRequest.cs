using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class InventoryRequest : PaginatedRequest
    {
        public string Pid { get; set; }
        public string SearchText { get; set; }
        public int? Quantity { get; set; }

        public override void DefaultOnNull()
        {
            Pid = GetValueOrDefault(Pid, null);
            SearchText = GetValueOrDefault(SearchText, null);
            _order = GetValueOrDefault(_order, "Quantity-Defect-OnHold-Reserve");
            base.DefaultOnNull();
        }
    }
}
