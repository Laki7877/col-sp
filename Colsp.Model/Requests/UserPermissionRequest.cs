using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class PermissionRequest : PaginatedRequest
    {
        public int? PermissionId { get; set; }
        public string PermissionName { get; set; }
    }
}
