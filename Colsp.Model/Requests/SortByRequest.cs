using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class SortByRequest
    {
        public int      SortById          { get; set; }
        public string   SortByName        { get; set; }
        public string   NameEn            { get; set; }
        public string   NameTh            { get; set; }

        public SortByRequest()
        {
            SortById                = 0;
            SortByName              = string.Empty;
            NameEn                  = string.Empty;
            NameTh                  = string.Empty;
        }
    }
}
