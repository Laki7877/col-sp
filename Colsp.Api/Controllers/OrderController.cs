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
    public class OrderController : ApiController
    {

        [Route("api/Order/{orderId}")]
        [HttpGet]
        public HttpResponseMessage GetOrder([FromUri] string orderId)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId;
                var order = (from or in OrderMockup.OrderList
                             where or.ShopId == shopId && or.OrderId.Equals(orderId)
                             select new
                             {
                                 or.CustomerName,
                                 or.OrderId,
                                 or.OrderDate,
                                 or.ShippingType,
                                 or.TotalAmt,
                                 or.Status,
                                 or.ShipAddress,
                                 or.BillAddress,
                                 or.Products,
                                 or.OrdDiscAmt,
                             }).SingleOrDefault();
                if(order == null)
                {
                    throw new Exception("Cannot find this order");
                }

                return Request.CreateResponse(HttpStatusCode.OK, order);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Order")]
        [HttpGet]
        public HttpResponseMessage GetOrder([FromUri] PurchaseOrderReuest request)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId;
                var list = (from or in OrderMockup.OrderList
                            where or.ShopId==shopId
                            select new
                            {
                                or.CustomerName,
                                or.OrderId,
                                or.OrderDate,
                                or.ShippingType,
                                or.TotalAmt,
                                or.Status
                            }).AsQueryable();
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,list);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("PaymentPending", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.ORDER_PAYMENT_PENDING));
                    }
                    else if (string.Equals("PaymentConfirmed", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.ORDER_PAYMENT_CONFIRM));
                    }
                    else if (string.Equals("Preparing", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.ORDER_PREPARING));
                    }
                    else if (string.Equals("ReadyToShip", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.ORDER_READY_TO_SHIP));
                    }
                    else if (string.Equals("Shipping", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                            list = list.Where(p => p.Status.Equals(Constant.ORDER_SHIPPING));
                    }
                    else if (string.Equals("Delivered", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.ORDER_DELIVERED));
                    }
                    else if (string.Equals("Cancelled", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.ORDER_CANCELLED));
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
    }
}
