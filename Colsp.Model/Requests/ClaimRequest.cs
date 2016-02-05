using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ClaimRequest
    {
        public object Shop { get; set; }
        public object Permission { get; set; }
        public object User { get; set; }

    }
}
