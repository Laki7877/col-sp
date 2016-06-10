using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class ErrorRequest
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ErrorRequest()
        {
            ErrorCode = string.Empty;
            ErrorMessage = string.Empty;
        }
    }
}
