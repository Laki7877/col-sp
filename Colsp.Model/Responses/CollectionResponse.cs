using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Responses
{
    public class CollectionResponse
    {
        public string CollectionName { get; set; }
        public string URLKeyTH { get; set; }
        public string URLKeyEN { get; set; }
        public string CollectionSort { get; set; }
        public List<CollectionItemResponse> CollectionItemList { get; set; }
        public string CollectionDescription { get; set; }
    }
}
