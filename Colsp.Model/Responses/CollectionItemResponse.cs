using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class CollectionItemResponse
    {
        public int CollectionId { get; set; }
        public int CollectionItemId { get; set; }
        public Nullable<int> PId { get; set; }
        public string ProductBoxBadge { get; set; }
        public int Sequence { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
    }
}
