using Colsp.Api.Constants;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Mockup
{
    public static class ReturnMockup
    {
        public static ReturnRequest R1 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE001", Order = OrderMockup.O1, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_APPROVE, };
        public static ReturnRequest R2 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE002", Order = OrderMockup.O2, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };
        public static ReturnRequest R3 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE003", Order = OrderMockup.O3, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };
        public static ReturnRequest R4 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE004", Order = OrderMockup.O4, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };
        public static ReturnRequest R5 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE005", Order = OrderMockup.O5, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_APPROVE, };
        public static ReturnRequest R6 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE006", Order = OrderMockup.O6, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };
        public static ReturnRequest R7 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE007", Order = OrderMockup.O7, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };


        public static List<ReturnRequest> ReturnList = new List<ReturnRequest>() { R1, R2, R3, R4, R5, R4, R7 };
    }
}