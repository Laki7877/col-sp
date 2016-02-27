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
using Colsp.Api.Constants;
using Colsp.Api.Helpers;

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
                var shopId = this.User.ShopRequest().ShopId.Value;
                var products = (from inv in db.Inventories
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
                                 inv.UpdatedDt,
                                 Brand = vari != null ? vari.ProductStage.Brand != null ? new { vari.ProductStage.Brand.BrandId, vari.ProductStage.Brand.BrandNameEn } : null
                                        : mast.Brand != null ? new { mast.Brand.BrandId, mast.Brand.BrandNameEn } : null,
                                 GlobalCategory = vari != null ? vari.ProductStage.GlobalCategory != null ? new { vari.ProductStage.GlobalCategory.Lft, vari.ProductStage.GlobalCategory.Rgt , vari.ProductStage.GlobalCategory.NameEn } : null
                                        : mast.GlobalCategory != null ? new { mast.GlobalCategory.Lft, mast.GlobalCategory.Rgt, mast.GlobalCategory.NameEn }:null,
                                 LocalCategory = vari != null ? vari.ProductStage.LocalCategory != null ? new { vari.ProductStage.LocalCategory.Lft, vari.ProductStage.LocalCategory.Rgt, vari.ProductStage.LocalCategory.NameEn } : null
                                        : mast.LocalCategory != null ? new { mast.LocalCategory.Lft, mast.LocalCategory.Rgt, mast.LocalCategory.NameEn } : null ,
                                 Tag = vari != null ? vari.ProductStage.Tag : mast.Tag,
                                 SalePrice = vari != null ? vari.SalePrice : mast.SalePrice,
                                 CreatedDt = vari != null ? vari.CreatedDt : mast.CreatedDt,
                                 Status = vari != null ? vari.Status : mast.Status,
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
                    List<int> brandIds = request.Brands.Where(w => w.BrandId != null).Select(s => s.BrandId.Value).ToList();
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
                    var lft = request.GlobalCategories.Where(w => w.Lft != null).Select(s => s.Lft).ToList();
                    var rgt = request.GlobalCategories.Where(w => w.Rgt != null).Select(s => s.Rgt).ToList();
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
                    var lft = request.LocalCategories.Where(w => w.Lft != null).Select(s => s.Lft).ToList();
                    var rgt = request.LocalCategories.Where(w => w.Rgt != null).Select(s => s.Rgt).ToList();

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
                if (request.PriceFrom != null)
                {
                    products = products.Where(w => w.SalePrice >= request.PriceFrom);
                }
                if (request.PriceTo != null)
                {
                    products = products.Where(w => w.SalePrice <= request.PriceTo);
                }
                if (request.CreatedDtFrom != null)
                {
                    DateTime from = Convert.ToDateTime(request.CreatedDtFrom);
                    products = products.Where(w => w.CreatedDt >= from);
                }
                if (request.CreatedDtTo != null)
                {
                    DateTime to = Convert.ToDateTime(request.CreatedDtTo);
                    products = products.Where(w => w.CreatedDt <= to);
                }

                if (request.ModifyDtFrom != null)
                {
                    DateTime from = Convert.ToDateTime(request.ModifyDtFrom);
                    products = products.Where(w => w.UpdatedDt >= from);
                }
                if (request.ModifyDtTo != null)
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
                        products = products.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) > w.SaftyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("OutOfStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) <= w.SaftyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("LowStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) <= w.SaftyStockSeller
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
                if(this.User.ShopRequest() == null)
                {
                    throw new Exception("Your not assigned any shop");
                }
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = this.User.ShopRequest().ShopId.Value;
                var inven = (from inv in db.Inventories
                              join stage in db.ProductStages on new { inv.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId } into mastJoin
                              from mast in mastJoin.DefaultIfEmpty()
                              join variant in db.ProductStageVariants on new { inv.Pid, ShopId = shopId } equals new { variant.Pid, variant.ShopId } into varJoin
                              from vari in varJoin.DefaultIfEmpty()
                              where vari != null || (mast != null && mast.ProductStageVariants.Count == 0)
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
                                  inv.UpdatedDt,
                                  IsVariant = vari != null ? true : false,
                                  VariantAttribute = vari.ProductStageVariantArrtibuteMaps.Select(s => new
                                  {
                                         s.Attribute.AttributeNameEn,
                                         Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.Value) select tt.AttributeValueEn).FirstOrDefault()
                                            : s.Value,
                                   })
                             });
                request.DefaultOnNull();
                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    inven = inven.Where(w => w.Pid.Contains(request.SearchText)
                    || w.Sku.Contains(request.SearchText)
                    || w.ProductNameEn.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("NormalStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        inven = inven.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) > w.SaftyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("OutOfStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        inven = inven.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) <= w.SaftyStockSeller);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("LowStock", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        inven = inven.Where(w => (w.Quantity - w.Defect - w.OnHold - w.Reserve) <= w.SaftyStockSeller 
                        && (w.Quantity - w.Defect - w.OnHold - w.Reserve) > 0);
                        //products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
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

        [Route("api/Inventories/{pid}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeInventories(string pid, InventoryRequest request)
        {
            try
            {
                var inv = db.Inventories.Find(pid);
                if(inv == null)
                {
                    throw new Exception("Cannot find inventory");
                }
                inv.Quantity = Validation.ValidationInteger(request.Quantity,"Quantity",true, Int32.MaxValue,0).Value;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
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