using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
    public class OnBoardRequest
    {
        public bool ChangePassword { get; set; }
        public bool SetUpShop { get; set; }
        public bool AddProduct { get; set; }
        public bool ProductApprove { get; set; }
        public bool DecorateStore { get; set; }
    }
}
