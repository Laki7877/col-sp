using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Api.Helpers;
using Colsp.Api.Mockup;
using Colsp.Entity.Models;
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
        private ColspEntities db = new ColspEntities();

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
                var shopId = User.ShopRequest().ShopId;
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
                var shopId = User.ShopRequest().ShopId;
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
                    current.ShipQuantity = product.ShipQuantity;
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
                var shopId = User.ShopRequest().ShopId;
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
                var shopId = User.ShopRequest().ShopId;
                var list = (from or in OrderMockup.OrderList
                            where or.ShopId==shopId
                            select new
                            {
                                or.CustomerName,
                                or.OrderId,
                                or.OrderDate,
                                or.ShippingType,
                                or.TotalAmt,
                                or.Status,
                                or.Payment,
                                or.Carrier
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
                    else if (string.Equals("Canceled", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        list = list.Where(p => p.Status.Equals(Constant.ORDER_CANCELED));
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

        [Route("api/Orders/Revenue")]
        [HttpGet]
        public HttpResponseMessage GetRevenue([FromUri] PurchaseOrderReuest request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = User.ShopRequest().ShopId;
                IQueryable<PurchaseOrderReuest> order = OrderMockup.OrderList.Where(w=>w.ShopId==shopId).AsQueryable();
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("Today", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        order = order.Where(p => p.OrderDate >= DateTime.Today && p.OrderDate < DateTime.Today.AddDays(1));
                        var todayOrder = (from or in order
                                  group or by or.OrderDate.Hour/2 into hourGroup
                                  select new
                                  {
                                      Key = hourGroup.Key,
                                      Value = hourGroup.Sum(s => s.GrandTotalAmt)
                                  }).ToList();

                        return Request.CreateResponse(HttpStatusCode.OK, todayOrder);
                    }
                    else if (string.Equals("ThisWeek", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        DateTime monday =  Util.StartOfWeek(DateTime.Now, DayOfWeek.Monday);
                        DateTime sunday = Util.StartOfWeek(DateTime.Now.AddDays(7), DayOfWeek.Sunday);
                        order = order.Where(p => p.OrderDate >= monday && p.OrderDate <= sunday);
                        var weekOrder = from or in order
                                         group or by or.OrderDate.DayOfWeek into weekGroup
                                         select new
                                         {
                                             Key = weekGroup.Key,
                                             Value = weekGroup.Sum(s => s.GrandTotalAmt)
                                         };
                        return Request.CreateResponse(HttpStatusCode.OK, weekOrder);
                    }
                    else if (string.Equals("ThisMonth", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        var firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                        order = order.Where(p => p.OrderDate >= firstDayOfMonth && p.OrderDate <= lastDayOfMonth);
                        var monthOrder = from or in order
                                         group or by or.OrderDate.Day into dayGroup
                                         select new
                                         {
                                             Key = dayGroup.Key,
                                             Value = dayGroup.Sum(s => s.GrandTotalAmt)
                                         };
                        return Request.CreateResponse(HttpStatusCode.OK, monthOrder);
                    }
                    else if (string.Equals("ThisYear", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        var firstDayOfYear = new DateTime(DateTime.Today.Year, 1, 1);
                        var lastDayOfYear = new DateTime(DateTime.Today.Year, 12, 31);
                        order = order.Where(p => p.OrderDate >= firstDayOfYear && p.OrderDate <= lastDayOfYear);
                        var yearOrder = from or in order
                                        group or by or.OrderDate.Month into monthGroup
                                        select new
                                        {
                                            Key = monthGroup.Key,
                                            Value = monthGroup.Sum(s => s.GrandTotalAmt)
                                        };
                        return Request.CreateResponse(HttpStatusCode.OK, yearOrder);
                    }
                    else
                    {
                        throw new Exception("Invalid _filter");
                    }
                }
                else
                {
                    throw new Exception("_filter is required");
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Orders/TopOrder")]
        [HttpGet]
        public HttpResponseMessage GetTopOrder()
        {
            try
            {
                var shopId = User.ShopRequest().ShopId;
                var orderDetailList = OrderMockup.OrderList.Where(w => w.ShopId == shopId).SelectMany(s => s.Products).ToList();

                var pids = (from detail in orderDetailList
                          group detail by detail.Pid into detailGroup
                          orderby detailGroup.Sum(s=>s.Quantity) descending
                          select detailGroup.Key
                          ).Take(Constant.TOP_SELLING).ToList();
                if(pids == null || pids.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, Constant.NOT_AVAILABLE);
                }

                var productList = db.ProductStages.Where(w => w.ShopId == shopId && pids.Contains(w.Pid)).Select(s=>new
                {
                    s.ProductNameEn,
                    s.FeatureImgUrl,
                    s.ProductId
                }).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, productList);

            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
