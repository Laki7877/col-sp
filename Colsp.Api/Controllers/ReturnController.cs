using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Api.Mockup;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Colsp.Api.Controllers
{
    public class ReturnController : ApiController
    {
        [Route("api/Returns/{returnId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeReturn([FromUri] string returnId, ReturnRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = this.User.ShopRequest().ShopId;
                var retur = (from or in ReturnMockup.ReturnList
                             where or.ShopId == shopId && or.ReturnId.Equals(returnId)
                             select or).SingleOrDefault();
                if (retur == null)
                {
                    throw new Exception("Cannot find return");
                }
                retur.CnNumber = request.CnNumber;
                retur.Status = request.Status;
                return GetReturn(retur.ReturnId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Returns/{returnId}")]
        [HttpGet]
        public HttpResponseMessage GetReturn([FromUri] string returnId)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId;
                var retur = (from re in ReturnMockup.ReturnList
                             where re.ShopId == shopId && re.ReturnId.Equals(returnId)
                             select re).SingleOrDefault();
                if (retur == null)
                {
                    throw new Exception("Cannot find this return");
                }

                return Request.CreateResponse(HttpStatusCode.OK, retur);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Returns")]
        [HttpGet]
        public HttpResponseMessage GetReturn([FromUri] ReturnRequest request)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId;
                var list = (from re in ReturnMockup.ReturnList
                            where re.ShopId == shopId
                            select new
                            {
                                re.ReturnDate,
                                re.ReturnId,
                                Order = new
                                {
                                    re.Order.OrderId,
                                    re.Order.CustomerName,
                                    re.Order.TotalAmt,
                                    re.Order.GrandTotalAmt,
                                    re.Order.ShippingType,
                                    re.Order.Status
                                },
                                re.CnNumber,
                                re.Status,
                            }).AsQueryable();
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, list);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    list = list.Where(w => w.Order.OrderId.Contains(request.SearchText)
                    || w.ReturnId.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("Accepted", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.RETURN_STATUS_APPROVE));
                    }
                    else if (string.Equals("Waiting", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                }
                var total = list.Count();
                var pagedProducts = list.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Returns/Rate")]
        [HttpGet]
        public HttpResponseMessage GetReturnRate()
        {
            try
            {
                var shopId = User.ShopRequest().ShopId;
                var returnCount = ReturnMockup.ReturnList.Where(w => w.ShopId == shopId).ToList().Count;
                var orderCount = OrderMockup.OrderList.Where(w => w.ShopId == shopId).ToList().Count;
                if(orderCount == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Constant.NOT_AVAILABLE);
                }
                return Request.CreateResponse(HttpStatusCode.OK, decimal.Multiply(decimal.Divide(returnCount,orderCount),100));
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }
    }
}
