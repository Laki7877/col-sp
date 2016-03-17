using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class AdminApproveRequest
    {
        public string Information           { get; set;}
        public string Image                 { get; set;}
        public string Category              { get; set;}
        public string Variant               { get; set;}
        public string MoreOption            { get; set;}
        public string RejectReason          { get; set;}

        public AdminApproveRequest()
        {
            Information = string.Empty;
            Image = string.Empty;
            Variant = string.Empty;
            Category = string.Empty;
            MoreOption = string.Empty;
            RejectReason = string.Empty;
        }
    }                               
}
