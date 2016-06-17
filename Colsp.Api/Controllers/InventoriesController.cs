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
                           from s in db.Inventories
                           join stage in db.ProductStages on new { s.Pid, ShopId = shopId } equals new { stage.Pid, stage.ShopId }
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
                               s.Quantity,
                               s.Defect,
                               s.OnHold,
                               s.Reserve,
                               s.SafetyStockSeller,
                               UpdatedDt = s.UpdateOn,
							   Brand = stage.ProductStageGroup.Brand != null ? new
							   {
								   stage.ProductStageGroup.Brand.BrandId,
								   stage.ProductStageGroup.Brand.BrandNameEn
							   } : null,
							   GlobalCategory = stage.ProductStageGroup.GlobalCategory != null ? new
							   {
								   stage.ProductStageGroup.GlobalCategory.Lft,
								   stage.ProductStageGroup.GlobalCategory.Rgt,
								   stage.ProductStageGroup.GlobalCategory.NameEn
							   } : null,
                               LocalCategory = stage.ProductStageGroup.LocalCategory != null ? new
							   {
								   stage.ProductStageGroup.LocalCategory.Lft,
								   stage.ProductStageGroup.LocalCategory.Rgt,
								   stage.ProductStageGroup.LocalCategory.NameEn
							   } : null,
                               //Tag = stage.ProductStageGroup.ProductStageTags != null ? string.Join(",",stage.ProductStageGroup.ProductStageTags) :null,
                               SalePrice = stage.SalePrice,
                               CreateOn = stage.CreateOn,
                               Status = stage.Status,
							   stage.OriginalPrice,
							   stage.ProductStageGroup.ImageFlag,
							   stage.ProductStageGroup.InfoFlag,
							   stage.ProductStageGroup.OnlineFlag,
							   stage.Visibility,
							   stage.VariantCount,
							   //stage.IsMaster,
							   ImageUrl = string.Empty.Equals(stage.FeatureImgUrl) ? string.Empty : string.Concat(Constant.PRODUCT_IMAGE_URL, stage.FeatureImgUrl),
							   stage.ProductStageGroup.GlobalCatId,
							   stage.ProductStageGroup.LocalCatId,
							   stage.ProductStageGroup.AttributeSetId,
							   stage.ProductStageAttributes,
							   CreatedDt = stage.ProductStageGroup.CreateOn,
							   stage.ShopId,
							   Tags = stage.ProductStageGroup.ProductStageTags.Select(t => t.Tag),
							   Shop = new { stage.Shop.ShopId, stage.Shop.ShopNameEn },
							   stage.IsVariant,
							   PidSku = db.ProductStages.Where(w => w.ProductId == stage.ProductId).Select(p => new
							   {
								   p.Pid,
								   p.Sku,
							   }),
							   VariantAttribute = stage.ProductStageAttributes.Select(s => new
							   {
								   s.Attribute.AttributeNameEn,
								   Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
											  : s.ValueEn,
							   }),
						   });
				bool isSeller = false;
				if (User.ShopRequest() != null)
				{
					//add shopid criteria for seller request
					if (User.BrandRequest() != null)
					{
						var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
						if (brands != null && brands.Count > 0)
						{
							products = products.Where(w => brands.Contains(w.Brand.BrandId));
						}
					}
					isSeller = true;
				}
				//set request default value
				request.DefaultOnNull();
				//add ProductName criteria
				if (request.ProductNames != null && request.ProductNames.Count > 0)
				{
					products = products.Where(w => request.ProductNames.Any(a => w.ProductNameEn.Contains(a))
					|| request.ProductNames.Any(a => w.ProductNameTh.Contains(a)));
				}
				//add Pid criteria
				if (request.Pids != null && request.Pids.Count > 0)
				{
					products = products.Where(w => w.PidSku.Any(a => request.Pids.Contains(a.Pid)));
				}
				//add Sku criteria
				if (request.Skus != null && request.Skus.Count > 0)
				{
					products = products.Where(w => w.PidSku.Any(a => request.Skus.Contains(a.Sku)));
				}
				//add Brand criteria
				if (request.Brands != null && request.Brands.Count > 0)
				{
					//if request send brand id, add brand id criteria
					List<int> brandIds = request.Brands.Where(w => w.BrandId != 0).Select(s => s.BrandId).ToList();
					if (brandIds != null && brandIds.Count > 0)
					{
						products = products.Where(w => brandIds.Contains(w.Brand.BrandId));
					}
					//if request send brand name, add brand name criteria
					List<string> brandNames = request.Brands.Where(w => w.BrandNameEn != null && !string.IsNullOrWhiteSpace(w.BrandNameEn)).Select(s => s.BrandNameEn).ToList();
					if (brandNames != null && brandNames.Count > 0)
					{
						products = products.Where(w => brandNames.Any(a => w.Brand.BrandNameEn.Contains(a)));
					}
				}
				if (request.Shops != null && request.Shops.Count > 0)
				{
					List<int> shopIds = request.Shops.Where(w => w.ShopId != 0).Select(s => s.ShopId).ToList();
					if (shopIds != null && shopIds.Count > 0)
					{
						products = products.Where(w => shopIds.Contains(w.Shop.ShopId));
					}
				}
				//add Global category criteria
				if (request.GlobalCategories != null && request.GlobalCategories.Count > 0)
				{
					//if request send category parent left and right, add category parent left and right criteria
					var lft = request.GlobalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
					var rgt = request.GlobalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();
					if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
					{
						products = products.Where(w => lft.Any(a => a <= w.GlobalCategory.Lft) && rgt.Any(a => a >= w.GlobalCategory.Rgt));
					}
					//if request send category name, add category category name criteria
					List<string> catNames = request.GlobalCategories.Where(w => w.NameEn != null && !string.IsNullOrWhiteSpace(w.NameEn)).Select(s => s.NameEn).ToList();
					if (catNames != null && catNames.Count > 0)
					{
						products = products.Where(w => catNames.Any(a => w.GlobalCategory.NameEn.Contains(a)));
					}
				}
				//add Local category criteria
				if (request.LocalCategories != null && request.LocalCategories.Count > 0)
				{
					//if request send category parent left and right, add category parent left and right criteria
					var lft = request.LocalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
					var rgt = request.LocalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();

					if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
					{
						products = products.Where(w => lft.Any(a => a <= w.LocalCategory.Lft) && rgt.Any(a => a >= w.LocalCategory.Rgt));
					}
					//if request send category name, add category category name criteria
					List<string> catNames = request.LocalCategories.Where(w => w.NameEn != null && !string.IsNullOrWhiteSpace(w.NameEn)).Select(s => s.NameEn).ToList();
					if (catNames != null && catNames.Count > 0)
					{
						products = products.Where(w => catNames.Any(a => w.LocalCategory.NameEn.Contains(a)));
					}
				}
				//add Tag criteria
				if (request.Tags != null && request.Tags.Count > 0)
				{
					products = products.Where(w => request.Tags.Any(a => w.Tags.Contains(a)));
				}
				//add sale price(from) criteria
				if (request.PriceFrom != 0)
				{
					products = products.Where(w => w.SalePrice >= request.PriceFrom);
				}
				//add sale price(to) criteria
				if (request.PriceTo != 0)
				{
					products = products.Where(w => w.SalePrice <= request.PriceTo);
				}
				//add create date(from) criteria
				if (request.CreatedDtFrom != null)
				{
					DateTime from = Convert.ToDateTime(request.CreatedDtFrom);
					products = products.Where(w => w.CreatedDt >= from);
				}
				//add create date(to) criteria
				if (request.CreatedDtTo != null)
				{
					DateTime to = Convert.ToDateTime(request.CreatedDtTo);
					products = products.Where(w => w.CreatedDt <= to);
				}
				//add modify date(from) criteria
				if (request.ModifyDtFrom != null)
				{
					DateTime from = Convert.ToDateTime(request.ModifyDtFrom);
					products = products.Where(w => w.UpdatedDt >= from);
				}
				//add modify date(to) criteria
				if (request.ModifyDtTo != null)
				{
					DateTime to = Convert.ToDateTime(request.ModifyDtTo);
					products = products.Where(w => w.UpdatedDt <= to);
				}
				if (!string.IsNullOrEmpty(request.SearchText))
				{
					if (isSeller)
					{
						products = products.Where(p => p.Sku.Contains(request.SearchText)
							|| p.ProductNameEn.Contains(request.SearchText)
							|| p.ProductNameTh.Contains(request.SearchText)
							|| p.Pid.Contains(request.SearchText)
							|| p.Upc.Contains(request.SearchText)
							|| p.Tags.Any(a => a.Contains(request.SearchText)));
					}
					else
					{
						products = products.Where(p => p.Sku.Contains(request.SearchText)
							|| p.ProductNameEn.Contains(request.SearchText)
							|| p.ProductNameTh.Contains(request.SearchText)
							|| p.Pid.Contains(request.SearchText)
							|| p.Upc.Contains(request.SearchText));
					}
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
                                    ImageUrl = string.Empty.Equals(stage.FeatureImgUrl) ? string.Empty : string.Concat(Constant.PRODUCT_IMAGE_URL, stage.FeatureImgUrl),
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
                        invenentory = invenentory.Where(w => w.Quantity <= 0);
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
                #region Query
                var inv = db.Inventories.Where(w=> w.Pid.Equals(pid) 
                    && !Constant.STATUS_REMOVE.Equals(w.ProductStage.Status))
                    .Select(s => new
                    {
                        s.Pid,
                        s.Quantity,
                        s.ProductStage.ProductStageGroup.ShippingId
                    })
                    .SingleOrDefault();
                #endregion
                #region Validation
                if (inv == null)
                {
                    throw new Exception("Cannot find inventory");
                }
                if(Constant.IGNORE_INVENTORY_SHIPPING.Contains(inv.ShippingId))
                {
                    throw new Exception("Cannot update inventory. Check you shipping type.");
                }
                #endregion
                #region Setup
                Inventory inventory = new Inventory()
                {
                    Pid = inv.Pid
                };
				var email = User.UserRequest().Email;
				var currentDt = SystemHelper.GetCurrentDateTime();
				db.Inventories.Attach(inventory);
                db.Entry(inventory).Property(p => p.Quantity).IsModified = true;
				db.Entry(inventory).Property(p => p.UpdateBy).IsModified = true;
				db.Entry(inventory).Property(p => p.UpdateOn).IsModified = true;
				inventory.Quantity = inv.Quantity + request.UpdateQuantity;
				inventory.UpdateOn = currentDt;
				inventory.UpdateBy = email;

                db.Configuration.ValidateOnSaveEnabled = false;
                Util.DeadlockRetry(db.SaveChanges, "Inventory");
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, inv.Quantity);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Inventories/Cmos/{pid}")]
        [HttpPut]
        [OverrideAuthentication, OverrideAuthorization]
        public HttpResponseMessage SaveChangeInventoriesCmos(string pid, InventoryRequest request)
        {
            try
            {
                #region Query
                var inv = db.Inventories.Where(w => w.Pid.Equals(pid))
                    .Select(s => new
                    {
                        s.Pid,
                        s.Quantity,
                        s.Defect,
                        s.Reserve,
                        s.OnHold,
                    })
                    .SingleOrDefault();
                #endregion
                #region Validation
                if (inv == null)
                {
                    throw new Exception("Cannot find inventory");
                }
                #endregion
                #region Setup
                Inventory inventory = new Inventory()
                {
                    Pid = inv.Pid
                };
                db.Inventories.Attach(inventory);
                db.Entry(inventory).Property(p => p.Quantity).IsModified = true;
                db.Entry(inventory).Property(p => p.Defect).IsModified = true;
                db.Entry(inventory).Property(p => p.Reserve).IsModified = true;
                db.Entry(inventory).Property(p => p.OnHold).IsModified = true;
                inventory.Quantity = inv.Quantity + request.StockAdjustQty;
                inventory.Defect = inv.Defect + request.DefectAdjustQty;
                inventory.Reserve = inv.Reserve + request.ReserveAdjustQty;
                inventory.OnHold = inv.OnHold + request.HoldAdjustQty;
                db.Configuration.ValidateOnSaveEnabled = false;
                Util.DeadlockRetry(db.SaveChanges, "Inventory");
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK,"Successful");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException());
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