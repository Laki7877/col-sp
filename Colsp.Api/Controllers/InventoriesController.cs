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
using System.Collections.Generic;
using Colsp.Api.Helpers;
using Colsp.Api.Constants;

namespace Colsp.Api.Controllers
{
    public class InventoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Inventories/Search")]
        [HttpPost]
        public HttpResponseMessage GetInventories(ProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = User.ShopRequest().ShopId;


                var products = (
                           from inv in db.Inventories
                           join stage in db.ProductStages on new { inv.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId }
                           where !Constant.STATUS_REMOVE.Equals(stage.Status) && 
                                 (
                                    stage.IsVariant == true 
                                    || (stage.IsVariant == false && stage.ImageCount == 0)
                                 )
                           select new
                           {
                               ProductId = stage.ProductId,
                               Sku = stage.Sku,
                               Upc = stage.Upc,
                               Pid = stage.Pid,
                               ProductNameEn = stage.ProductNameEn,
                               ProductNameTh = stage.ProductNameTh,
                               inv.Quantity,
                               inv.Defect,
                               inv.OnHold,
                               inv.Reserve,
                               inv.SafetyStockSeller,
                               UpdatedDt = inv.UpdateOn,
                               Brand = stage.ProductStageGroup.Brand != null ? new { stage.ProductStageGroup.Brand.BrandId, stage.ProductStageGroup.Brand.BrandNameEn } : null,
                               GlobalCategory = stage.ProductStageGroup.GlobalCategory != null ? new { stage.ProductStageGroup.GlobalCategory.Lft, stage.ProductStageGroup.GlobalCategory.Rgt, stage.ProductStageGroup.GlobalCategory.NameEn } : null,
                               LocalCategory = stage.ProductStageGroup.LocalCategory != null ? new { stage.ProductStageGroup.LocalCategory.Lft, stage.ProductStageGroup.LocalCategory.Rgt, stage.ProductStageGroup.LocalCategory.NameEn } : null,
                               Tag = stage.ProductStageGroup.ProductStageTags != null ? string.Join(",",stage.ProductStageGroup.ProductStageTags) :null,
                               SalePrice = stage.SalePrice,
                               CreateOn = stage.CreateOn,
                               Status = stage.Status,
                           });
                request.DefaultOnNull();
                if (request.ProductNames != null && request.ProductNames.Count > 0)
                {
                    products = products.Where(w => request.ProductNames.Any(a => w.ProductNameEn.Contains(a))
                    || request.ProductNames.Any(a => w.ProductNameTh.Contains(a)));
                }
                if (request.Pids != null && request.Pids.Count > 0)
                {
                    products = products.Where(w => request.Pids.Any(a => w.Pid.Contains(a)));
                }
                if (request.Skus != null && request.Skus.Count > 0)
                {
                    products = products.Where(w => request.Skus.Any(a => w.Sku.Contains(a)));
                }
                if (request.Brands != null && request.Brands.Count > 0)
                {
                    List<int> brandIds = request.Brands.Where(w => w.BrandId != 0).Select(s => s.BrandId).ToList();
                    if (brandIds != null && brandIds.Count > 0)
                    {
                        products = products.Where(w => brandIds.Contains(w.Brand.BrandId));
                    }
                    List<string> brandNames = request.Brands.Where(w => w.BrandNameEn != null).Select(s => s.BrandNameEn).ToList();
                    if (brandNames != null && brandNames.Count > 0)
                    {
                        products = products.Where(w => brandNames.Any(a => w.Brand.BrandNameEn.Contains(a)));
                    }
                }
                if (request.GlobalCategories != null && request.GlobalCategories.Count > 0)
                {
                    var lft = request.GlobalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
                    var rgt = request.GlobalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();
                    if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
                    {
                        products = products.Where(w => lft.Any(a => a <= w.GlobalCategory.Lft) && rgt.Any(a => a >= w.GlobalCategory.Rgt));
                    }
                    List<string> catNames = request.GlobalCategories.Where(w => w.NameEn != null).Select(s => s.NameEn).ToList();
                    if (catNames != null && catNames.Count > 0)
                    {
                        products = products.Where(w => catNames.Any(a => w.GlobalCategory.NameEn.Contains(a)));
                    }
                }
                if (request.LocalCategories != null && request.LocalCategories.Count > 0)
                {
                    var lft = request.LocalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
                    var rgt = request.LocalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();

                    if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
                    {
                        products = products.Where(w => lft.Any(a => a <= w.LocalCategory.Lft) && rgt.Any(a => a >= w.LocalCategory.Rgt));
                    }
                    List<string> catNames = request.LocalCategories.Where(w => w.NameEn != null).Select(s => s.NameEn).ToList();
                    if (catNames != null && catNames.Count > 0)
                    {
                        products = products.Where(w => catNames.Any(a => w.LocalCategory.NameEn.Contains(a)));
                    }
                }
                if (request.Tags != null && request.Tags.Count > 0)
                {
                    products = products.Where(w => request.Tags.Any(a => w.Tag.Contains(a)));
                }
                if (request.PriceFrom != 0)
                {
                    products = products.Where(w => w.SalePrice >= request.PriceFrom);
                }
                if (request.PriceTo != 0)
                {
                    products = products.Where(w => w.SalePrice <= request.PriceTo);
                }
                if (!string.IsNullOrEmpty(request.CreatedDtFrom))
                {
                    DateTime from = Convert.ToDateTime(request.CreatedDtFrom);
                    products = products.Where(w => w.CreateOn >= from);
                }
                if (!string.IsNullOrEmpty(request.CreatedDtTo))
                {
                    DateTime to = Convert.ToDateTime(request.CreatedDtTo);
                    products = products.Where(w => w.CreateOn <= to);
                }

                if (!string.IsNullOrEmpty(request.ModifyDtFrom))
                {
                    DateTime from = Convert.ToDateTime(request.ModifyDtFrom);
                    products = products.Where(w => w.UpdatedDt >= from);
                }
                if (!string.IsNullOrEmpty(request.ModifyDtTo))
                {
                    DateTime to = Convert.ToDateTime(request.ModifyDtTo);
                    products = products.Where(w => w.UpdatedDt <= to);
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText)
                    || p.Pid.Contains(request.SearchText)
                    || p.Upc.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("NormalStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) > w.SafetyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("OutOfStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) <= w.SafetyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("LowStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) <= w.SafetyStockSeller
                        && (w.Quantity - w.Defect - w.OnHold - w.Reserve) > 0);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                }

                var total = products.Count();
                var pagedProducts = products.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Inventories")]
        [HttpGet]
        public HttpResponseMessage GetInventories([FromUri] InventoryRequest request)
        {
            try
            {
                if(User.ShopRequest() == null)
                {
                    throw new Exception("Your not assigned any shop");
                }
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }

                var shopId = User.ShopRequest().ShopId;

                var invenentory = (from inv in db.Inventories
                                join stage in db.ProductStages on new { inv.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId }
                                where
                                !stage.Status.Equals(Constant.STATUS_REMOVE)
                                && (
                                    stage.IsVariant == true 
                                    || (
                                            stage.IsVariant == false 
                                            && stage.VariantCount == 0
                                        )
                                )
                                select new
                                {
                                    ImageUrl = stage.FeatureImgUrl,
                                    ProductId = stage.ProductId,
                                    Sku = stage.Sku,
                                    Upc = stage.Upc,
                                    Pid = stage.Pid,
                                    ProductNameEn = stage.ProductNameEn,
                                    ProductNameTh = stage.ProductNameTh,
                                    inv.Quantity,
                                    inv.Defect,
                                    inv.OnHold,
                                    inv.Reserve,
                                    inv.SafetyStockSeller,
                                    UpdatedDt = inv.UpdateOn,
                                    stage.IsVariant,
                                    VariantAttribute = stage.ProductStageAttributes.Select(s => new
                                    {
                                        s.Attribute.AttributeNameEn,
                                        Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
                                              :s.ValueEn,
                                    }),
                                    Brand = stage.ProductStageGroup.Brand == null ? null : new
                                    {
                                        stage.ProductStageGroup.Brand.BrandId,
                                        stage.ProductStageGroup.Brand.BrandNameEn
                                    }
                                });
                request.DefaultOnNull();
                if (User.BrandRequest() != null)
                {
                    var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
                    if (brands != null && brands.Count > 0)
                    {
                        invenentory = invenentory.Where(w => brands.Contains(w.Brand.BrandId));
                    }
                }
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    invenentory = invenentory.Where(w => w.Pid.Contains(request.SearchText)
                    || w.Sku.Contains(request.SearchText)
                    || w.ProductNameEn.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("NormalStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        invenentory = invenentory.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) > w.SafetyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("OutOfStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        invenentory = invenentory.Where(w => w.Quantity == 0);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("LowStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        invenentory = invenentory.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) <= w.SafetyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                }
                var total = invenentory.Count();
                var pagedAttribute = invenentory.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedAttribute, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Inventories/{pid}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeInventories(string pid, InventoryRequest request)
        {
            try
            {
                var inv = db.Inventories.Where(w=> w.Pid.Equals(pid) 
                    && !Constant.STATUS_REMOVE.Equals(w.ProductStage.Status))
                    .Select(s => new
                    {
                        s.Pid,
                        s.Quantity,
                        s.ProductStage.ProductStageGroup.ShippingId
                    })
                    .SingleOrDefault();
                if(inv == null)
                {
                    throw new Exception("Cannot find inventory");
                }
                if(inv.ShippingId == 3 || inv.ShippingId == 4)
                {
                    throw new Exception("Cannot update inventory with COL Fulfillment or 3PL Fulfillment");
                }
                Inventory inventory = new Inventory()
                {
                    Pid = inv.Pid
                };
                db.Inventories.Attach(inventory);
                db.Entry(inventory).Property(p => p.Quantity).IsModified = true;
                inventory.Quantity = inv.Quantity + request.UpdateQuantity;
                db.Configuration.ValidateOnSaveEnabled = false;
                Util.DeadlockRetry(db.SaveChanges, "Inventory");
                return Request.CreateResponse(HttpStatusCode.OK, inv.Quantity);
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