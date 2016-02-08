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
                var shopId = this.User.ShopRequest().ShopId;
                var inven = (from stage in db.ProductStages
                             let inv = db.Inventories.Where(w => w.Pid.Equals(stage.Pid)).FirstOrDefault()
                             join variant in db.ProductStageVariants on stage.ProductId equals variant.ProductId into varJoin
                             from varJ in varJoin.DefaultIfEmpty()
                             let varInv = db.Inventories.Where(w => w.Pid.Equals(varJ.Pid)).FirstOrDefault()
                             where stage.ShopId == shopId
                             select new
                             {
                                 stage.ProductId,
                                 Sku = varJ != null ? varJ.Sku : stage.Sku,
                                 Upc = varJ != null ? varJ.Upc : stage.Upc,
                                 Pid = varJ != null ? varJ.Pid : stage.Pid,
                                 ProductNameEn = varJ != null ? varJ.ProductNameEn : stage.ProductNameEn,
                                 ProductNameTh = varJ != null ? varJ.ProductNameTh : stage.ProductNameTh,
                                 Status = varJ != null ? varJ.Status : stage.Status,
                                 Inventory = inv != null ? inv : varInv
                             });
                if(request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, inven);
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