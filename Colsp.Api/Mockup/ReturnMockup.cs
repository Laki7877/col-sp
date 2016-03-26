using Colsp.Api.Constants;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;

namespace Colsp.Api.Mockup
{
    public static class ReturnMockup
    {
        private static ReturnRequest R1 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE001", Order = OrderMockup.O1, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_APPROVE, ReasonForReturn = "This shirt makes me look ugly. It is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum." };
        private static ReturnRequest R2 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE002", Order = OrderMockup.O2, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, ReasonForReturn = "Reason For Return 2" };
        private static ReturnRequest R3 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE003", Order = OrderMockup.O3, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };
        private static ReturnRequest R4 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE004", Order = OrderMockup.O4, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };
        private static ReturnRequest R5 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE005", Order = OrderMockup.O5, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_APPROVE, };
        private static ReturnRequest R6 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE006", Order = OrderMockup.O6, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };
        private static ReturnRequest R7 = new ReturnRequest() { ReturnDate = DateTime.Now, ReturnId = "RE007", Order = OrderMockup.O7, ShopId = 3, CnNumber = string.Empty, Status = Constant.RETURN_STATUS_WAIT_FOR_APPROVAL, };


        public static List<ReturnRequest> ReturnList = new List<ReturnRequest>() { R1, R2, R3, R4 };
    }
}