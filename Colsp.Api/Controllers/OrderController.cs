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
        [Route("api/Orders")] 
        [HttpPut]
        public HttpResponseMessage SaveChangeOrder(List<PurchaseOrderReuest> request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = this.User.ShopRequest().ShopId;
                var orderIds = request.Select(s => s.OrderId).ToList();
                var orderList = OrderMockup.OrderList.Where(w => w.ShopId == shopId && orderIds.Contains(w.OrderId)).ToList();
                foreach (var orderRq in request)
                {
                    var order = orderList.Where(w => w.OrderId.Equals(orderRq.OrderId)).SingleOrDefault();
                    if (order == null)
                    {
                        throw new Exception("Cannot find order");
                    }
                    if (!string.IsNullOrEmpty(orderRq.Status))
                    {
                        order.Status = orderRq.Status;
                    }
                    foreach (var product in orderRq.Products)
                    {
                        var current = order.Products.Where(w => w.Pid.Equals(product.Pid)).SingleOrDefault();
                        if (current == null)
                        {
                            throw new Exception("Cannot find product " + product.Pid);
                        }
                        current.Quantity = product.Quantity;
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Orders/{orderId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeOrder([FromUri] string orderId,PurchaseOrderReuest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = this.User.ShopRequest().ShopId;
                var order = OrderMockup.OrderList.Where(w => w.ShopId == shopId && w.OrderId.Equals(orderId)).SingleOrDefault();
                if (order == null)
                {
                    throw new Exception("Cannot find order");
                }
                if (!string.IsNullOrEmpty(request.Status))
                {
                    order.Status = request.Status;
                }
                
                order.InvoiceNumber = request.InvoiceNumber;
                foreach (var product in request.Products)
                {
                    var current = order.Products.Where(w => w.Pid.Equals(product.Pid)).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find product " + product.Pid);
                    }
                    current.Quantity = product.Quantity;
                }
                return GetOrder(order.OrderId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Orders/{orderId}")]
        [HttpGet]
        public HttpResponseMessage GetOrder([FromUri] string orderId)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId;
                var order = (from or in OrderMockup.OrderList
                             where or.ShopId == shopId && or.OrderId.Equals(orderId)
                             select or).SingleOrDefault();
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

        [Route("api/Orders")]
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
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    list = list.Where(w => w.OrderId.Contains(request.SearchText) 
                    || w.CustomerName.Contains(request.SearchText));
                }
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
