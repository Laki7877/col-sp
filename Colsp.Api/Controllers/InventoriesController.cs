using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using System;

namespace Colsp.Api.Controllers
{
    public class InventoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Inventories")]
        [HttpGet]
        public HttpResponseMessage GetInventories([FromUri] InventoryRequest request)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId.Value;

                var inven = (from inv in db.Inventories
                              join stage in db.ProductStages on new { inv.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId } into mastJoin
                              from mast in mastJoin.DefaultIfEmpty()
                              join variant in db.ProductStageVariants on new { inv.Pid, ShopId = shopId } equals new { variant.Pid, variant.ShopId } into varJoin
                              from vari in varJoin.DefaultIfEmpty()
                              where vari != null || mast != null
                             select new
                              {
                                  ProductId = mast != null ? mast.ProductId : vari.ProductId,
                                  Sku = vari != null ? vari.Sku : mast.Sku,
                                  Upc = vari != null ? vari.Upc : mast.Upc,
                                  Pid = vari != null ? vari.Pid : mast.Pid,
                                  ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
                                  ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
                                  inv.Quantity,
                                  inv.Defect,
                                  inv.OnHold,
                                  inv.Reserve,
                                  inv.SaftyStockSeller,
                                  inv.UpdatedDt
                              });
                if(request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, inven);
                }

                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("NormalStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                }

                var total = inven.Count();
                var pagedAttribute = inven.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
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

        private bool InventoryExists(string id)
        {
            return db.Inventories.Count(e => e.Pid == id) > 0;
        }
    }
}