using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;
using Colsp.Api.Helpers;
using System.Web;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using System.Web.Http.Cors;

namespace Colsp.Api.Controllers
{
    public class ProductStagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/ProductStages/UnGroup")]
        [HttpGet]
        public HttpResponseMessage GetUnGroupProduct([FromUri] UnGroupProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid Request");
                }
                var productTemp = db.ProductStages.Where(w => w.IsSell == true 
                && w.IsVariant == false && w.ProductStageGroup.GlobalCatId == request.CategoryId && (w.ProductStageGroup.AttributeSetId == null
                || w.ProductStageGroup.AttributeSetId == request.AttributeSetId)).Select(s => new
                {
                    s.ShopId,
                    s.Pid,
                    s.ProductNameEn,
                    s.Sku,
                    s.ProductId
                });
                //check if its seller permission
                if (User.ShopRequest() != null)
                {
                    int shopId = User.ShopRequest().ShopId;
                    productTemp = productTemp.Where(w => w.ShopId == shopId);
                }
                else
                {
                    productTemp = productTemp.Where(w => w.ShopId == request.ShopId);
                }
                request.DefaultOnNull();

                if (!string.IsNullOrWhiteSpace(request.SearchText))
                {
                    productTemp = productTemp.Where(w => w.ProductNameEn.Contains(request.SearchText)
                    || w.Pid.Contains(request.SearchText)
                    || w.Sku.Contains(request.SearchText));
                }

                //count number of products
                var total = productTemp.Count();
                //make paginate query from database
                var pagedProducts = productTemp.Paginate(request);
                //create response
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                //return response
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }



        [Route("api/Products")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSet([FromUri] ProductRequest request)
        {
            try
            {
                var products = db.Products.Where(w => w.IsSell == true && w.IsMaster == false && !Constant.STATUS_REMOVE.Equals(w.Status))
                    .Select(s=>new
                {
                    s.ProductNameEn,
                    s.Pid,
                    s.ProductId,
                    s.Sku
                });
                if(request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, products);
                }

                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    products = products.Where(w => w.Pid.Contains(request.SearchText)
                    || w.ProductNameEn.Contains(request.SearchText)
                    || w.Sku.Contains(request.SearchText));
                }

                //count number of products
                var total = products.Count();
                //make paginate query from database
                var pagedProducts = products.Paginate(request);
                //create response
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                //return response
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/AttributeSet")]
        [HttpPost]
        public HttpResponseMessage GetAttributeSet(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var productIds = request.Select(s => s.ProductId).ToList();
                var attrSet = db.AttributeSets.Where(w => w.ProductStageGroups.Any(a => productIds.Contains(a.ProductId) && !Constant.STATUS_REMOVE.Equals(a.Status)) )
                    .Select(s => new
                    {
                        s.AttributeSetId,
                        s.AttributeSetNameEn,
                        ProductCount = s.ProductStageGroups.Where(w => productIds.Contains(w.ProductId))
                    });
                return Request.CreateResponse(HttpStatusCode.OK, attrSet);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

        }

        [Route("api/ProductStages/Guidance/Export")]
        [HttpGet]
        public HttpResponseMessage GetAllGuidance()
        {
            try
            {
                var tmpGuidance = db.ImportHeaders.Where(w => true);
                if (User.ShopRequest() != null)
                {
                    var groupName = User.ShopRequest().ShopGroup;
                    tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
                }
                var guidance = tmpGuidance.Where(w => 
                        !w.MapName.Equals("ADI") 
                        && !w.MapName.Equals("ADJ")
                        && !w.MapName.Equals("ADK"))
                        .Select(s => new
                        {
                            s.GroupName,
                            s.HeaderName,
                            s.MapName,
                            s.ImportHeaderId,
                            s.AcceptedValue,
                            s.Position,
                        }).OrderBy(o => o.Position);
                return Request.CreateResponse(HttpStatusCode.OK, guidance);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityProductStage(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }

                IQueryable<ProductStage> productList = null;
                var ids = request.Select(s => s.ProductId).ToList();
                #region Permission
                if (User.ShopRequest() != null)
                {
                    int shopId = User.ShopRequest().ShopId;
                    productList = db.ProductStages.Where(w => w.ShopId == shopId && w.IsVariant == false);
                }
                else
                {
                    productList = db.ProductStages.Where(w => w.IsVariant == false);
                }
                #endregion

                productList = productList.Where(w => ids.Contains(w.ProductId));
                var pids = productList.Select(s => s.Pid);
                var realProduct = db.Products.Where(w => pids.Contains(w.Pid)).Select(s=>new
                {
                    s.Pid
                });
                foreach (ProductStageRequest proRq in request)
                {
                    
                    var current = productList.Where(w => w.ProductId.Equals(proRq.ProductId)).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find product " + proRq.ProductId + " in shop " + proRq.ShopId);
                    }
                    current.Visibility = proRq.Visibility;
                    current.UpdateBy = User.UserRequest().Email;
                    current.UpdateOn = DateTime.Now;
                    ProductStageGroup group = new ProductStageGroup()
                    {
                        ProductId = current.ProductId
                    };
                    
                    var currentReal = realProduct.Where(w => w.Pid.Equals(current.Pid)).SingleOrDefault();
                    if(currentReal != null)
                    {
                        Product product = new Product()
                        {
                            Pid = currentReal.Pid
                        };
                        db.Products.Attach(product);
                        db.Entry(product).Property(p => p.Visibility).IsModified = true;
                        db.Entry(product).Property(p => p.UpdateBy).IsModified = true;
                        db.Entry(product).Property(p => p.UpdateOn).IsModified = true;
                        product.Visibility = current.Visibility;
                        product.UpdateBy = current.UpdateBy;
                        product.UpdateOn = current.UpdateOn;
                        db.ProductStageGroups.Attach(group);
                        db.Entry(group).Property(p => p.OnlineFlag).IsModified = true;
                        db.Entry(group).Property(p => p.UpdateBy).IsModified = true;
                        db.Entry(group).Property(p => p.UpdateOn).IsModified = true;
                        group.UpdateBy = current.UpdateBy;
                        group.UpdateOn = current.UpdateOn;
                        if (current.Visibility == false)
                        {
                            group.OnlineFlag = false;
                        }
                        else
                        {
                            group.OnlineFlag = true;
                        }

                    }
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/PendingProduct")]
        [HttpPost]
        public HttpResponseMessage GroupPendingProduct(PendingProduct request)
        {
            try
            {
                #region Validation
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                if(request.AttributeSet == null || request.AttributeSet.AttributeSetId == 0)
                {
                    throw new Exception("Invalid attribute set");
                }
                var pids = request.Variants.Select(s => s.Pid).ToList();
                if (pids == null || pids.Count == 0)
                {
                    throw new Exception("No pid selected");
                }
                #endregion
                #region Shop and User
                int shopId = -1;
                if(User.ShopRequest() != null)
                {
                    shopId = User.ShopRequest().ShopId;
                }
                else
                {
                    shopId = request.Shop.ShopId;
                }
                string email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                #endregion
                #region Default Variant
                var defaultVariantRq = request.Variants.Where(w => w.DefaultVariant == true).FirstOrDefault();
                if (defaultVariantRq == null)
                {
                    throw new Exception("Cannot find default variant");
                }
                var productStage = db.ProductStages.Where(w => pids.Contains(w.Pid));
                var defaultVariant = productStage.Where(w => w.Pid.Equals(defaultVariantRq.Pid))
                    .Include(i=>i.ProductStageAttributes)
                    .Include(i=>i.ProductStageGroup)
                    .Include(i=>i.Inventory)
                    .FirstOrDefault();
                if(defaultVariant == null)
                {
                    throw new Exception("Cannot find default variant");
                }
                #endregion
                defaultVariant.ProductStageGroup.AttributeSetId = request.AttributeSet.AttributeSetId;
                var parentVariant = new ProductStage()
                {
                    ProductId = defaultVariant.ProductId,
                    ShopId = defaultVariant.ShopId,
                    ProductNameEn = defaultVariant.ProductNameEn,
                    ProductNameTh = defaultVariant.ProductNameTh,
                    ProdTDNameTh = defaultVariant.ProdTDNameTh,
                    ProdTDNameEn = defaultVariant.ProdTDNameEn,
                    JDADept = defaultVariant.JDADept,
                    JDASubDept = defaultVariant.JDASubDept,
                    SaleUnitTh = defaultVariant.SaleUnitTh,
                    SaleUnitEn = defaultVariant.SaleUnitEn,
                    Sku = defaultVariant.Sku,
                    Upc = defaultVariant.Upc,
                    OriginalPrice = defaultVariant.OriginalPrice,
                    SalePrice = defaultVariant.SalePrice,
                    DescriptionFullEn = defaultVariant.DescriptionFullEn,
                    DescriptionShortEn = defaultVariant.DescriptionShortEn,
                    DescriptionFullTh = defaultVariant.DescriptionFullTh,
                    DescriptionShortTh = defaultVariant.DescriptionShortTh,
                    MobileDescriptionEn = defaultVariant.MobileDescriptionEn,
                    MobileDescriptionTh = defaultVariant.MobileDescriptionTh,
                    ImageCount = defaultVariant.ImageCount,
                    FeatureImgUrl = defaultVariant.FeatureImgUrl,
                    PrepareDay = defaultVariant.PrepareDay,
                    LimitIndividualDay = defaultVariant.LimitIndividualDay,
                    PrepareMon = defaultVariant.PrepareMon,
                    PrepareTue = defaultVariant.PrepareTue,
                    PrepareWed = defaultVariant.PrepareWed,
                    PrepareThu = defaultVariant.PrepareThu,
                    PrepareFri = defaultVariant.PrepareFri,
                    PrepareSat = defaultVariant.PrepareSat,
                    PrepareSun = defaultVariant.PrepareSun,
                    KillerPoint1En = defaultVariant.KillerPoint1En,
                    KillerPoint2En = defaultVariant.KillerPoint2En,
                    KillerPoint3En = defaultVariant.KillerPoint3En,
                    KillerPoint1Th = defaultVariant.KillerPoint1Th,
                    KillerPoint2Th = defaultVariant.KillerPoint2Th,
                    KillerPoint3Th = defaultVariant.KillerPoint3Th,
                    Installment = defaultVariant.Installment,
                    Length = defaultVariant.Length,
                    Height = defaultVariant.Height,
                    Width = defaultVariant.Width,
                    DimensionUnit = defaultVariant.DimensionUnit,
                    Weight = defaultVariant.Weight,
                    WeightUnit = defaultVariant.WeightUnit,
                    MetaTitleEn = defaultVariant.MetaTitleEn,
                    MetaTitleTh = defaultVariant.MetaTitleTh,
                    MetaDescriptionEn = defaultVariant.MetaDescriptionEn,
                    MetaDescriptionTh = defaultVariant.MetaDescriptionTh,
                    MetaKeyEn = defaultVariant.MetaKeyEn,
                    MetaKeyTh = defaultVariant.MetaKeyTh,
                    SeoEn = defaultVariant.SeoEn,
                    SeoTh = defaultVariant.SeoTh,
                    IsHasExpiryDate = defaultVariant.IsHasExpiryDate,
                    IsVat = defaultVariant.IsVat,
                    BoostWeight = defaultVariant.BoostWeight,
                    GlobalBoostWeight = defaultVariant.GlobalBoostWeight,
                    ExpressDelivery = defaultVariant.ExpressDelivery,
                    DeliveryFee = defaultVariant.DeliveryFee,
                    PromotionPrice = defaultVariant.PromotionPrice,
                    EffectiveDatePromotion = defaultVariant.EffectiveDatePromotion,
                    ExpireDatePromotion = defaultVariant.ExpireDatePromotion,
                    NewArrivalDate = defaultVariant.NewArrivalDate,
                    DefaultVariant = false,
                    Display = defaultVariant.Display,
                    MiniQtyAllowed = defaultVariant.MiniQtyAllowed,
                    MaxiQtyAllowed = defaultVariant.MaxiQtyAllowed,
                    UnitPrice = defaultVariant.UnitPrice,
                    PurchasePrice = defaultVariant.PurchasePrice,
                    IsSell = false,
                    IsVariant = false,
                    IsMaster = false,
                    VariantCount = 0,
                    Visibility = defaultVariant.Visibility,
                    OldPid = defaultVariant.OldPid,
                    Bu = defaultVariant.Bu,
                    Status = Constant.PRODUCT_STATUS_DRAFT,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                    Inventory = new Inventory()
                    {
                        CreateBy = email,
                        CreateOn = currentDt,
                        Defect = defaultVariant.Inventory.Defect,
                        MaxQtyAllowInCart = defaultVariant.Inventory.MaxQtyAllowInCart,
                        MaxQtyPreOrder = defaultVariant.Inventory.MaxQtyPreOrder,
                        MinQtyAllowInCart = defaultVariant.Inventory.MinQtyAllowInCart,
                        OnHold = defaultVariant.Inventory.OnHold,
                        Quantity = defaultVariant.Inventory.Quantity,
                        Reserve = defaultVariant.Inventory.Reserve,
                        SafetyStockAdmin = defaultVariant.Inventory.SafetyStockAdmin,
                        SafetyStockSeller = defaultVariant.Inventory.SafetyStockSeller,
                        StockType = defaultVariant.Inventory.StockType,
                        UpdateBy = email,
                        UpdateOn = currentDt,
                        UseDecimal = defaultVariant.Inventory.UseDecimal
                    }
                };


                foreach (var attribute in defaultVariant.ProductStageAttributes)
                {
                    parentVariant.ProductStageAttributes.Add(new ProductStageAttribute()
                    {
                        AttributeId = attribute.AttributeId,
                        AttributeValueId = attribute.AttributeValueId,
                        CheckboxValue = attribute.CheckboxValue,
                        CreateBy = email,
                        CreateOn = currentDt,
                        IsAttributeValue = attribute.IsAttributeValue,
                        Position = attribute.Position,
                        UpdateBy = email,
                        UpdateOn = currentDt,
                        ValueEn = attribute.ValueEn,
                        ValueTh = attribute.ValueTh,
                    });
                }

                parentVariant.VariantCount = request.Variants.Count;
                List<long> groupIds = new List<long>();
                foreach (var variantRq in request.Variants)
                {
                    var variant = productStage.Where(w => w.Pid.Equals(variantRq.Pid)).FirstOrDefault();
                    if(variant == null)
                    {
                        throw new Exception(string.Concat("Cannot find product ", variantRq.Pid));
                    }
                    if(variant.ProductId != parentVariant.ProductId)
                    {
                        groupIds.Add(variant.ProductId);
                    }
                    variant.ProductId = parentVariant.ProductId;
                    variant.IsVariant = true;
                    variant.IsSell = true;
                    variant.IsMaster = false;
                    variant.Status = Constant.PRODUCT_STATUS_DRAFT;
                    db.ProductStageAttributes.RemoveRange(db.ProductStageAttributes.Where(w => w.Pid.Equals(variant.Pid)));


                    if(variantRq.FirstAttribute != null && variantRq.FirstAttribute.AttributeId != 0)
                    {
                        variant.ProductStageAttributes.Add(new ProductStageAttribute()
                        {
                            AttributeId = variantRq.FirstAttribute.AttributeId,
                            AttributeValueId = variantRq.FirstAttribute.AttributeValues[0].AttributeValueId,
                            CheckboxValue = false,
                            CreateBy = email,
                            CreateOn = currentDt,
                            IsAttributeValue = true,
                            ValueEn = string.Concat("((", variantRq.FirstAttribute.AttributeValues[0].AttributeValueId,"))"),
                            ValueTh = string.Concat("((", variantRq.FirstAttribute.AttributeValues[0].AttributeValueId, "))"),
                            UpdateBy = email,
                            UpdateOn = currentDt,
                            Position = 1,
                        });
                    }
                    if (variantRq.SecondAttribute != null && variantRq.SecondAttribute.AttributeId != 0)
                    {
                        variant.ProductStageAttributes.Add(new ProductStageAttribute()
                        {
                            AttributeId = variantRq.SecondAttribute.AttributeId,
                            AttributeValueId = variantRq.SecondAttribute.AttributeValues[0].AttributeValueId,
                            CheckboxValue = false,
                            CreateBy = email,
                            CreateOn = currentDt,
                            IsAttributeValue = true,
                            ValueEn = string.Concat("((", variantRq.SecondAttribute.AttributeValues[0].AttributeValueId, "))"),
                            ValueTh = string.Concat("((", variantRq.SecondAttribute.AttributeValues[0].AttributeValueId, "))"),
                            UpdateBy = email,
                            UpdateOn = currentDt,
                            Position = 2
                        });
                    }

                }
                db.ProductStageGroups.RemoveRange(db.ProductStageGroups.Where(w => groupIds.Contains(w.ProductId)));
                AutoGenerate.GeneratePid(db, new List<ProductStage>() { parentVariant});
                db.ProductStages.Add(parentVariant);
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }







        [Route("api/Products/Master/{productId}")]
        [HttpPut]
        public HttpResponseMessage SaveMasterProduct([FromUri]long productId, MasterProductRequest request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }

                var masterProduct = db.Products.Where(w => w.ProductId == productId && w.IsMaster == true).SingleOrDefault();
                if (masterProduct == null)
                {
                    throw new Exception("Cannot find master product");
                }

                var dbChild = db.Products.Where(w => w.MasterPid.Equals(masterProduct.Pid)).Select(s=>s.Pid).ToList();
                List<string> newChildPids = new List<string>();
                foreach(var child in request.ChildProducts)
                {
                    bool isNew = false;
                    if(dbChild == null || dbChild.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = dbChild.Where(w => w.Equals(child.Pid)).SingleOrDefault();
                        if(current != null)
                        {
                            dbChild.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        Product product = new Product()
                        {
                            Pid = child.Pid
                        };
                        db.Products.Attach(product);
                        db.Entry(product).Property(p => p.MasterPid).IsModified = true;
                        product.MasterPid = masterProduct.Pid;
                    }
                }

                if (dbChild != null && dbChild.Count > 0)
                {
                    foreach (var id in dbChild)
                    {
                        Product product = new Product()
                        {
                            Pid = id
                        };
                        db.Products.Attach(product);
                        db.Entry(product).Property(p => p.MasterPid).IsModified = true;
                        product.MasterPid = null;
                    }
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                Util.DeadlockRetry(db.SaveChanges, "ProductStageMaster");

                //var masterProduct = db.ProductStageMasters.Where(w => w.).ToList();

                //foreach(var child in request.ChildProducts)
                //{
                //    bool isNew = false;
                //    if(masterProduct == null || masterProduct.Count == 0)
                //    {
                //        isNew = true;
                //    }
                //    if (!isNew)
                //    {
                //        var current = masterProduct.Where(w => w.ChildPid.Equals(child.Pid)).SingleOrDefault();
                //        if(current != null)
                //        {
                //            masterProduct.Remove(current);
                //        }
                //        else
                //        {
                //            isNew = true;
                //        }
                //    }
                //    if (isNew)
                //    {
                //        db.ProductStageMasters.Add(new ProductStageMaster()
                //        {
                //            MasterPid = masterProduct[0].MasterPid,
                //            ChildPid = child.Pid,
                //            CreateBy = User.UserRequest().Email,
                //            CreateOn = DateTime.Now,
                //            UpdateBy = User.UserRequest().Email,
                //            UpdateOn = DateTime.Now
                //        });
                //    }
                //}
                //if(masterProduct != null && masterProduct.Count > 0)
                //{
                //    db.ProductStageMasters.RemoveRange(masterProduct);
                //}
                //Util.DeadlockRetry(db.SaveChanges, "ProductStageMaster");
                return GetMasterProduct(0);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Products/Master")]
        [HttpPost]
        public HttpResponseMessage AddMasterProduct(MasterProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invaide request");
                }

                var parentProduct = db.Products
                    .Where(w => w.Pid.Equals(request.MasterProduct.Pid) && w.IsSell == true)
                    .SingleOrDefault();

                if (parentProduct == null)
                {
                    throw new Exception(string.Concat("Cannot find product ", request.MasterProduct.Pid));
                }

                var email = User.UserRequest().Email;
                var currentDt = DateTime.Now;

                var masterProduct = new Product()
                {
                    ShopId = Constant.ADMIN_SHOP_ID,
                    AttributeSetId = parentProduct.AttributeSetId,
                    CreateBy = email,
                    CreateOn = currentDt,
                    EffectiveDate = parentProduct.EffectiveDate,
                    ExpireDate = parentProduct.ExpireDate,
                    GlobalCatId = parentProduct.GlobalCatId,
                    IsBestSeller = parentProduct.IsBestSeller,
                    IsClearance = parentProduct.IsClearance,
                    IsNew = parentProduct.IsNew,
                    IsOnlineExclusive = parentProduct.IsOnlineExclusive,
                    IsOnlyAt = parentProduct.IsOnlyAt,
                    LocalCatId = parentProduct.LocalCatId,
                    Remark = parentProduct.Remark,
                    Status = parentProduct.Status,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                    TheOneCardEarn = parentProduct.TheOneCardEarn,
                    ShippingId = parentProduct.ShippingId,
                    BrandId = parentProduct.BrandId,
                    GiftWrap = parentProduct.GiftWrap,
                    BoostWeight = parentProduct.BoostWeight,
                    DefaultVariant = parentProduct.DefaultVariant,
                    DeliveryFee = parentProduct.DeliveryFee,
                    DescriptionFullEn = parentProduct.DescriptionFullEn,
                    DescriptionFullTh = parentProduct.DescriptionFullTh,
                    DescriptionShortEn = parentProduct.DescriptionShortEn,
                    DescriptionShortTh = parentProduct.DescriptionShortTh,
                    DimensionUnit = parentProduct.DimensionUnit,
                    Display = parentProduct.Display,
                    EffectiveDatePromotion = parentProduct.EffectiveDatePromotion,
                    ExpireDatePromotion = parentProduct.ExpireDatePromotion,
                    ExpressDelivery = parentProduct.ExpressDelivery,
                    FeatureImgUrl = parentProduct.FeatureImgUrl,
                    GlobalBoostWeight = parentProduct.GlobalBoostWeight,
                    Height = parentProduct.Height,
                    ImageCount = parentProduct.ImageCount,
                    Installment = parentProduct.Installment,
                    IsHasExpiryDate = parentProduct.IsHasExpiryDate,
                    IsMaster = true,
                    IsSell = false,
                    IsVariant = false,
                    IsVat = parentProduct.IsVat,
                    JDADept = parentProduct.JDADept,
                    JDASubDept = parentProduct.JDASubDept,
                    KillerPoint1En = parentProduct.KillerPoint1En,
                    KillerPoint1Th = parentProduct.KillerPoint1Th,
                    KillerPoint2En = parentProduct.KillerPoint2En,
                    KillerPoint2Th = parentProduct.KillerPoint2Th,
                    KillerPoint3En = parentProduct.KillerPoint3En,
                    KillerPoint3Th = parentProduct.KillerPoint3Th,
                    Length = parentProduct.Length,
                    LimitIndividualDay = parentProduct.LimitIndividualDay,
                    MaxiQtyAllowed = parentProduct.MaxiQtyAllowed,
                    MetaDescriptionEn = parentProduct.MetaDescriptionEn,
                    MetaDescriptionTh = parentProduct.MetaDescriptionTh,
                    MetaKeyEn = parentProduct.MetaKeyEn,
                    MetaKeyTh = parentProduct.MetaKeyTh,
                    MetaTitleEn = parentProduct.MetaTitleEn,
                    MetaTitleTh = parentProduct.MetaTitleTh,
                    MiniQtyAllowed = parentProduct.MiniQtyAllowed,
                    MobileDescriptionEn = parentProduct.MobileDescriptionEn,
                    MobileDescriptionTh = parentProduct.MobileDescriptionTh,
                    NewArrivalDate = parentProduct.NewArrivalDate,
                    OriginalPrice = parentProduct.OriginalPrice,
                    PrepareDay = parentProduct.PrepareDay,
                    PrepareFri = parentProduct.PrepareFri,
                    PrepareMon = parentProduct.PrepareMon,
                    PrepareSat = parentProduct.PrepareSat,
                    PrepareSun = parentProduct.PrepareSun,
                    PrepareThu = parentProduct.PrepareThu,
                    PrepareTue = parentProduct.PrepareTue,
                    PrepareWed = parentProduct.PrepareWed,
                    ProdTDNameEn = parentProduct.ProdTDNameEn,
                    ProdTDNameTh = parentProduct.ProdTDNameTh,
                    ProductNameEn = parentProduct.ProductNameEn,
                    ProductNameTh = parentProduct.ProductNameTh,
                    PromotionPrice = parentProduct.PromotionPrice,
                    PurchasePrice = parentProduct.PurchasePrice,
                    SalePrice = parentProduct.SalePrice,
                    SaleUnitEn = parentProduct.SaleUnitEn,
                    SaleUnitTh = parentProduct.SaleUnitTh,
                    UnitPrice = parentProduct.UnitPrice,
                    SeoEn = parentProduct.SeoEn,
                    SeoTh = parentProduct.SeoTh,
                    Sku = parentProduct.Sku,
                    Upc = parentProduct.Upc,
                    Visibility = parentProduct.Visibility,
                    VariantCount = parentProduct.VariantCount,
                    Weight = parentProduct.Weight,
                    WeightUnit = parentProduct.WeightUnit,
                    Width = parentProduct.Width,
                    MasterPid = null,
                    ParentPid = null,
                    Pid = parentProduct.Pid,
                };

                ProductStageGroup stageGroup = new ProductStageGroup()
                {
                    ProductId = masterProduct.ProductId,
                    ShopId = Constant.ADMIN_SHOP_ID,
                    GlobalCatId = masterProduct.GlobalCatId,
                    LocalCatId = masterProduct.LocalCatId,
                    AttributeSetId = masterProduct.AttributeSetId,
                    BrandId = masterProduct.BrandId,
                    ShippingId = masterProduct.ShippingId,
                    EffectiveDate = masterProduct.EffectiveDate,
                    ExpireDate = masterProduct.ExpireDate,
                    NewArrivalDate = masterProduct.NewArrivalDate,
                    TheOneCardEarn = masterProduct.TheOneCardEarn,
                    GiftWrap = masterProduct.GiftWrap,
                    IsNew = masterProduct.IsNew,
                    IsClearance = masterProduct.IsClearance,
                    IsBestSeller = masterProduct.IsBestSeller,
                    IsOnlineExclusive = masterProduct.IsOnlineExclusive,
                    IsOnlyAt = masterProduct.IsOnlyAt,
                    Remark = masterProduct.Remark,
                    InfoFlag = false,
                    ImageFlag = false,
                    OnlineFlag = false,
                    InformationTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                    ImageTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                    CategoryTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                    VariantTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                    MoreOptionTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                    RejectReason = string.Empty,
                    Status = Constant.PRODUCT_STATUS_APPROVE,
                    FirstApproveBy = null,
                    FirstApproveOn = null,
                    ApproveBy = null, 
                    ApproveOn = null,
                    RejecteBy = null,
                    RejectOn =  null,
                    SubmitBy =  null,
                    SubmitOn =  null,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                    ProductStages = new List<ProductStage>()
                    {
                        new ProductStage()
                        {
                            ProductId = masterProduct.ProductId,
                            ShopId = Constant.ADMIN_SHOP_ID,
                            ProductNameEn = masterProduct.ProductNameEn,
                            ProductNameTh = masterProduct.ProductNameTh,
                            ProdTDNameTh = masterProduct.ProdTDNameTh,
                            ProdTDNameEn = masterProduct.ProdTDNameEn,
                            JDADept = masterProduct.JDADept,
                            JDASubDept = masterProduct.JDASubDept,
                            SaleUnitTh = masterProduct.SaleUnitTh,
                            SaleUnitEn = masterProduct.SaleUnitEn,
                            Sku = masterProduct.Sku,
                            Upc = masterProduct.Upc,
                            OriginalPrice = masterProduct.OriginalPrice,
                            SalePrice = masterProduct.SalePrice,
                            DescriptionFullEn = masterProduct.DescriptionFullEn,
                            DescriptionShortEn = masterProduct.DescriptionShortEn,
                            DescriptionFullTh = masterProduct.DescriptionFullTh,
                            DescriptionShortTh = masterProduct.DescriptionShortTh,
                            MobileDescriptionEn = masterProduct.MobileDescriptionEn,
                            MobileDescriptionTh = masterProduct.MobileDescriptionTh,
                            ImageCount = masterProduct.ImageCount,
                            FeatureImgUrl = masterProduct.FeatureImgUrl,
                            PrepareDay = masterProduct.PrepareDay,
                            LimitIndividualDay = masterProduct.LimitIndividualDay,
                            PrepareMon = masterProduct.PrepareMon,
                            PrepareTue = masterProduct.PrepareTue,
                            PrepareWed = masterProduct.PrepareWed,
                            PrepareThu = masterProduct.PrepareThu,
                            PrepareFri = masterProduct.PrepareFri,
                            PrepareSat = masterProduct.PrepareSat,
                            PrepareSun = masterProduct.PrepareSun,
                            KillerPoint1En = masterProduct.KillerPoint1En,
                            KillerPoint2En = masterProduct.KillerPoint2En,
                            KillerPoint3En = masterProduct.KillerPoint3En,
                            KillerPoint1Th = masterProduct.KillerPoint1Th,
                            KillerPoint2Th = masterProduct.KillerPoint2Th,
                            KillerPoint3Th = masterProduct.KillerPoint3Th,
                            Installment = masterProduct.Installment,
                            Length = masterProduct.Length,
                            Height = masterProduct.Height,
                            Width = masterProduct.Width,
                            DimensionUnit = masterProduct.DimensionUnit,
                            Weight = masterProduct.Weight,
                            WeightUnit = masterProduct.WeightUnit,
                            MetaTitleEn = masterProduct.MetaTitleEn,
                            MetaTitleTh = masterProduct.MetaTitleTh,
                            MetaDescriptionEn = masterProduct.MetaDescriptionEn,
                            MetaDescriptionTh = masterProduct.MetaDescriptionTh,
                            MetaKeyEn = masterProduct.MetaKeyEn,
                            MetaKeyTh = masterProduct.MetaKeyTh,
                            SeoEn = masterProduct.SeoEn,
                            SeoTh = masterProduct.SeoTh,
                            IsHasExpiryDate = masterProduct.IsHasExpiryDate,
                            IsVat = masterProduct.IsVat,
                            UrlKey = masterProduct.UrlKey,
                            BoostWeight = masterProduct.BoostWeight,
                            GlobalBoostWeight = masterProduct.GlobalBoostWeight,
                            ExpressDelivery = masterProduct.ExpressDelivery,
                            DeliveryFee = masterProduct.DeliveryFee,
                            PromotionPrice = masterProduct.PromotionPrice,
                            EffectiveDatePromotion = masterProduct.EffectiveDatePromotion,
                            ExpireDatePromotion = masterProduct.ExpireDatePromotion,
                            NewArrivalDate = masterProduct.NewArrivalDate,
                            DefaultVariant = masterProduct.DefaultVariant,
                            Display = masterProduct.Display,
                            MiniQtyAllowed = masterProduct.MiniQtyAllowed,
                            MaxiQtyAllowed = masterProduct.MaxiQtyAllowed,
                            UnitPrice = masterProduct.UnitPrice,
                            PurchasePrice = masterProduct.PurchasePrice,
                            IsSell = false,
                            IsVariant = false,
                            IsMaster = true,
                            VariantCount = 0,
                            Visibility = false,
                            OldPid = null,
                            Bu = null,
                            Status = Constant.PRODUCT_STATUS_APPROVE,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        }
                    }
                };





                AutoGenerate.GeneratePid(db, stageGroup.ProductStages);
                masterProduct.Pid = stageGroup.ProductStages.First().Pid;
                masterProduct.UrlKey = stageGroup.ProductStages.First().UrlKey;
                masterProduct.ShopId = Constant.ADMIN_SHOP_ID;
                stageGroup.ProductId = masterProduct.ProductId = db.GetNextProductStageGroupId().SingleOrDefault().Value;
                db.ProductStageGroups.Add(stageGroup);
                db.Products.Add(masterProduct);
                var childPids = request.ChildProducts.Select(s => s.Pid).ToList();
                childPids.Add(request.MasterProduct.Pid);
                var childList = db.Products.Where(w => childPids.Contains(w.Pid)).ToList();
                childList.ForEach(f => f.MasterPid = masterProduct.Pid);
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return GetMasterProduct(masterProduct.ProductId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Products/Master/{productId}")]
        [HttpGet]
        public HttpResponseMessage GetMasterProduct(long productId)
        {
            try
            {
                var master = db.Products.Where(w => w.ProductId == productId && w.IsMaster == true)
                    .Select(s => new
                {
                    MasterProduct = new
                    {
                        s.ProductId,
                        s.ProductNameEn,
                        s.Pid,
                    },
                    ChildProducts = db.Products.Where(w => w.MasterPid.Equals(s.Pid)).Select(sc => new
                    {
                        sc.ProductId,
                        sc.Pid,
                        sc.ProductNameEn
                    }),
                    UpdatedDt = s.UpdateOn,
                }).FirstOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, master);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Products/Master")]
        [HttpGet]
        public HttpResponseMessage GetMasterProduct([FromUri]ProductRequest request)
        {
            try
            {

                var master = db.Products.Where(w => w.IsMaster == true).Select(s => new
                {
                    s.ProductId,
                    s.ProductNameEn,
                    s.Pid,
                    ChildPids = db.Products.Where(w=>w.MasterPid.Equals(s.Pid)).Select(sc=>new
                    {
                        sc.Pid
                    }),
                    UpdatedDt = s.UpdateOn,
                });

                request.DefaultOnNull();
                var total = master.Count();
                var pagedProducts = master.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }








        [Route("api/ProductStages/Template")]
        [HttpPost]
        public HttpResponseMessage ExportTemplate(CSVTemplateRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var tmpGuidance = db.ImportHeaders.Where(w => true);
                if (User.ShopRequest() != null)
                {
                    var groupName = User.ShopRequest().ShopGroup;
                    tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
                }
                var guidance = tmpGuidance.OrderBy(o => o.Position).ToList();
                //tmpGuidance.Where(w=>!"DAT".Equals(w.MapName)).OrderBy(o => o.ImportHeaderId).ToList();
                List<string> header = new List<string>();
                foreach (var g in guidance)
                {
                    header.Add(g.HeaderName);
                }
                List<string> defaultAttribute = null;
                if(User.ShopRequest() != null)
                {
                    defaultAttribute = db.Attributes
                        .Where(w => w.DefaultAttribute && Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo))
                        .Select(s => s.AttributeNameEn)
                        .ToList();
                }
                else
                {
                    defaultAttribute = db.Attributes
                        .Where(w => w.DefaultAttribute)
                        .Select(s => s.AttributeNameEn)
                        .ToList();
                }
               
                if (defaultAttribute != null && defaultAttribute.Count > 0)
                {
                    header.AddRange(defaultAttribute);
                }
                if (request.GlobalCategories != null)
                {
                    List<int> categoryIds = request.GlobalCategories.Select(s => s.CategoryId).ToList();
                    var categories = db.GlobalCategories.Where(w => categoryIds.Contains(w.CategoryId)).Select(s =>
                    new {
                        s.NameEn,
                        s.CategoryId,
                        AttribuyeSet = s.GlobalCatAttributeSetMaps.Select(se =>
                        new {
                            se.AttributeSet.AttributeSetNameEn,
                            Attribute = se.AttributeSet.AttributeSetMaps.Select(sa => sa.Attribute.AttributeNameEn)
                        })
                    }).ToList();
                    if (categories != null && categories.Count > 0)
                    {
                        HashSet<string> attribute = new HashSet<string>();
                        foreach (var cat in categories)
                        {
                            foreach (var attibutS in cat.AttribuyeSet)
                            {
                                foreach (var attr in attibutS.Attribute)
                                {
                                    attribute.Add(attr);
                                }
                            }
                        }
                        if (attribute != null && attribute.Count > 0)
                        {
                            header.AddRange(attribute.ToList());
                        }
                    }
                }
                
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                var csv = new CsvWriter(writer);
                foreach (string h in header)
                {
                    csv.WriteField(h);
                }
                csv.NextRecord();
                writer.Flush();
                stream.Position = 0;
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "file.csv";
                return result;
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        





        [Route("api/ProductStages/Guidance")]
        [HttpGet]
        public HttpResponseMessage GetGuidance(string SearchText, int _limit)
        {
            try
            {
                if (_limit < 0)
                {
                    throw new Exception("Limit cannot be negative");
                }
                var tmpGuidance = db.ImportHeaders.Where(w => true);
                if(User.ShopRequest() != null)
                {
                    var groupName = User.ShopRequest().ShopGroup;
                    tmpGuidance = tmpGuidance.Where(w=>w.ShopGroups.Any(a=>a.Abbr.Equals(groupName)));
                }
                var guidance = tmpGuidance.Where(w => w.HeaderName.Contains(SearchText))
                    .Select(s => new ImportHeaderRequest()
                {
                    HeaderName = s.HeaderName,
                    Description = s.Description,
                    Example = s.Example,
                    GroupName = s.GroupName,
                    Note = s.Note,
                    AcceptedValue = s.AcceptedValue,
                    IsAttribute = false
                }).Take(_limit).ToList();
                if (guidance != null && guidance.Count < _limit)
                {
                    var tmpAttribute = db.Attributes.Where(w => true);
                    if(User.ShopRequest() != null)
                    {
                        tmpAttribute = tmpAttribute.Where(w => w.DefaultAttribute == false 
                        || (w.DefaultAttribute == true && Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo)));
                    }
                    var attribute = tmpAttribute.Where(w => w.AttributeNameEn.Contains(SearchText))
                        .Select(s => new ImportHeaderRequest()
                    {
                        HeaderName = s.AttributeNameEn,
                        GroupName = s.AttributeNameEn,
                        IsVariant = s.VariantStatus,
                        AttributeType = s.DataType,
                        IsAttribute = true,
                        Description = s.AttributeDescriptionEn,
                        AttributeValue = s.AttributeValueMaps.Select(sv => new { sv.AttributeValue.AttributeValueEn, sv.AttributeValue.AttributeValueTh }).ToList()
                    }).Take(_limit - guidance.Count).ToList();
                    guidance.AddRange(attribute);
                }
                return Request.CreateResponse(HttpStatusCode.OK, guidance);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/All/Image")]
        [HttpPut]
        public HttpResponseMessage SaveChangeAllImage(List<VariantRequest> request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                int shopId = User.ShopRequest().ShopId;
                string email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                var pids = request.Select(s => s.Pid).ToList();
                var products = db.ProductStages.Where(w => w.ShopId == shopId && pids.Contains(w.Pid)).Select(s=>new
                {
                    s.Pid,
                    s.ProductId,
                    s.IsVariant
                });
                db.ProductStageImages.RemoveRange(db.ProductStageImages.Where(w => pids.Contains(w.Pid)));
                var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
                var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;
                foreach (VariantRequest varRq in request)
                {
                    var pro = products.Where(w => w.Pid.Equals(varRq.Pid)).SingleOrDefault();
                    if(pro == null)
                    {
                        throw new Exception(string.Concat("Cannot find product ", varRq.Pid));
                    }

                    if (pro.IsVariant)
                    {
                        if (varRq.VariantImg != null && varRq.VariantImg.Count > 0)
                        {
                            int position = 1;
                            foreach (var img in varRq.VariantImg)
                            {
                                db.ProductStageImages.Add(new ProductStageImage()
                                {
                                    CreateBy = email,
                                    CreateOn = currentDt,
                                    FeatureFlag = position == 1 ? true : false,
                                    ImageName = string.Empty,
                                    //ImageOriginName = string.Empty,
                                    //ImageUrlEn = img.Url,
                                    Pid = pro.Pid,
                                    //Position = position++,
                                    ShopId = shopId,
                                    Status = Constant.STATUS_ACTIVE,
                                    UpdateBy = email,
                                    UpdateOn = currentDt
                                });
                            }
                            ProductStageGroup group = new ProductStageGroup()
                            {
                                ProductId = pro.ProductId,
                            };
                            db.ProductStageGroups.Attach(group);
                            db.Entry(group).Property(p => p.ImageFlag).IsModified = true;
                            db.Entry(group).Property(p => p.Status).IsModified = true;
                            group.Status = Constant.PRODUCT_STATUS_DRAFT;
                            group.ImageFlag = true;
                            ProductStage stage = new ProductStage()
                            {
                                Pid = pro.Pid
                            };
                            db.ProductStages.Attach(stage);
                            db.Entry(stage).Property(p => p.FeatureImgUrl).IsModified = true;
                            db.Entry(stage).Property(p => p.Status).IsModified = true;
                            stage.FeatureImgUrl = varRq.VariantImg[0].Url;
                            stage.Status = Constant.PRODUCT_STATUS_DRAFT;
                        }
                    }
                    else
                    {
                        
                        if(varRq.MasterImg != null && varRq.MasterImg.Count > 0)
                        {
                            int position = 1;
                            foreach (var img in varRq.MasterImg)
                            {
                                db.ProductStageImages.Add(new ProductStageImage()
                                {
                                    CreateBy = email,
                                    CreateOn = currentDt,
                                    FeatureFlag = position == 1 ? true : false,
                                    ImageName = string.Empty,
                                    //ImageOriginName = string.Empty,
                                    //ImageUrlEn = img.Url,
                                    Pid = pro.Pid,
                                    SeqNo = position++,
                                    ShopId = shopId,
                                    Status = Constant.STATUS_ACTIVE,
                                    UpdateBy = email,
                                    UpdateOn = currentDt
                                });
                            }
                            ProductStageGroup group = new ProductStageGroup()
                            {
                                ProductId = pro.ProductId,
                            };
                            db.ProductStageGroups.Attach(group);
                            db.Entry(group).Property(p => p.ImageFlag).IsModified = true;
                            db.Entry(group).Property(p => p.Status).IsModified = true;
                            group.ImageFlag = true;
                            group.Status = Constant.PRODUCT_STATUS_DRAFT;
                            ProductStage stage = new ProductStage()
                            {
                                Pid = pro.Pid
                            };
                            db.ProductStages.Attach(stage);
                            db.Entry(stage).Property(p => p.FeatureImgUrl).IsModified = true;
                            db.Entry(stage).Property(p => p.Status).IsModified = true;
                            stage.FeatureImgUrl = varRq.MasterImg[0].Url;
                            stage.Status = Constant.PRODUCT_STATUS_DRAFT;
                        }
                    }
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/All")]
        [HttpGet]
        public HttpResponseMessage GetProductAllStages([FromUri] ProductRequest request)
        {
            try
            {
                var products = db.ProductStages.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status) && w.Visibility == true)
                    .Select(productStage => new
                {
                    productStage.ProductId,
                    productStage.Sku,
                    productStage.Upc,
                    productStage.ProductNameEn,
                    productStage.ProductNameTh,
                    productStage.Pid,
                    productStage.Status,
                    MasterImg = productStage.ProductStageImages.Select(s => new
                    {
                        ImageId = 0,
                        //Url = s.ImageUrlEn,
                        position = s.SeqNo
                    }).OrderBy(o => o.position),
                    VariantImg = productStage.ProductStageImages.Select(s => new
                    {
                        ImageId = 0,
                        //Url = s.ImageUrlEn,
                        position = s.SeqNo
                    }).OrderBy(o => o.position),
                    productStage.IsVariant,
                    productStage.VariantCount,
                    productStage.ProductStageComments.FirstOrDefault().Comment,
                    VariantAttribute = productStage.ProductStageAttributes.Select(s => new
                    {
                        s.Attribute.AttributeNameEn,
                        Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
                            : s.ValueEn,
                    }),
                    productStage.ShopId,
                    Brand = productStage.ProductStageGroup.Brand != null ? new { productStage.ProductStageGroup.Brand.BrandId, productStage.ProductStageGroup.Brand.BrandNameEn } : null,
                });


                if(User.ShopRequest() != null)
                {
                    int shopId = User.ShopRequest().ShopId;
                    products = products.Where(w => w.ShopId == shopId);
                    if (User.BrandRequest() != null)
                    {
                        var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
                        if (brands != null && brands.Count > 0)
                        {
                            products = products.Where(w => brands.Contains(w.Brand.BrandId));
                        }
                    }
                }
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, products.ToList());
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText)
                    || p.Pid.Contains(request.SearchText)
                    || p.Upc.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request.Pid))
                {
                    products = products.Where(p => p.Pid.Equals(request.Pid));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("ImageMissing", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(w => w.MasterImg.Count() == 0 && w.VariantImg.Count() == 0);
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                    else if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
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

        /// <summary>
        /// This endpoint will take in advance search criteria from request
        /// and return list of product
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of product with advance criteria</returns>
        [Route("api/ProductStages/Search")]
        [HttpPost]
        public HttpResponseMessage GetProductStagesAdvance(ProductRequest request)
        {
            try
            {
                //Request cannot be null
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                //Prepare Query to query table ProductStage
                var products = (from p in db.ProductStageGroups
                                where p.ProductStages.Any(a => a.IsVariant == false)
                                select new
                                {
                                    p.ProductStages.FirstOrDefault().Sku,
                                    p.ProductStages.FirstOrDefault().Pid,
                                    p.ProductStages.FirstOrDefault().Upc,
                                    p.ProductStages.FirstOrDefault().ProductId,
                                    p.ProductStages.FirstOrDefault().ProductNameEn,
                                    p.ProductStages.FirstOrDefault().ProductNameTh,
                                    p.ProductStages.FirstOrDefault().SalePrice,
                                    p.ProductStages.FirstOrDefault().OriginalPrice,
                                    p.ProductStages.FirstOrDefault().IsMaster,
                                    p.Shop.ShopNameEn,
                                    p.Status,
                                    p.ImageFlag,
                                    p.InfoFlag,
                                    p.OnlineFlag,
                                    p.ProductStages.FirstOrDefault().Visibility,
                                    p.ProductStages.FirstOrDefault().VariantCount,
                                    ImageUrl = p.ProductStages.FirstOrDefault().FeatureImgUrl,
                                    GlobalCategory = p.GlobalCategory != null ? new { p.GlobalCategory.CategoryId, p.GlobalCategory.NameEn, p.GlobalCategory.Lft, p.GlobalCategory.Rgt } : null,
                                    LocalCategory = p.LocalCategory != null ? new { p.LocalCategory.CategoryId, p.LocalCategory.NameEn, p.LocalCategory.Lft, p.LocalCategory.Rgt } : null,
                                    Brand = p.Brand != null ? new { p.Brand.BrandId, p.Brand.BrandNameEn } : null,
                                    Tags = p.ProductStageTags.Select(s=>s.Tag),
                                    CreatedDt = p.CreateOn,
                                    UpdatedDt = p.UpdateOn,
                                    //PriceTo = p.SalePrice < p.ProductStageVariants.Max(m => m.SalePrice)
                                    //        ? p.ProductStageVariants.Max(m => m.SalePrice) : p.SalePrice,
                                    //PriceFrom = p.SalePrice < p.ProductStageVariants.Min(m => m.SalePrice)
                                    //        ? p.SalePrice : p.ProductStageVariants.Min(m => m.SalePrice),
                                    Shop = new { p.Shop.ShopId, p.Shop.ShopNameEn },
                                    p.InformationTabStatus,
                                    p.ImageTabStatus,
                                    p.MoreOptionTabStatus,
                                    p.VariantTabStatus,
                                    p.CategoryTabStatus,
                                });
                //check if its seller permission
                if (User.ShopRequest() != null)
                {
                    //add shopid criteria for seller request
                    int shopId = User.ShopRequest().ShopId;
                    products = products.Where(w => w.Shop.ShopId == shopId);
                    if (User.BrandRequest() != null)
                    {
                        var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
                        if (brands != null && brands.Count > 0)
                        {
                            products = products.Where(w => brands.Contains(w.Brand.BrandId));
                        }
                    }
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
                    products = products.Where(w => request.Pids.Any(a => w.Pid.Contains(a)));
                }
                //add Sku criteria
                if (request.Skus != null && request.Skus.Count > 0)
                {
                    products = products.Where(w => request.Skus.Any(a => w.Sku.Contains(a)));
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
                if(request.Shops != null && request.Shops.Count > 0)
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
                    products = products.Where(w => request.Tags.Any(a =>w.Tags.Contains(a)));
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
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText)
                    || p.Pid.Contains(request.SearchText)
                    || p.Upc.Contains(request.SearchText)
                    || p.Tags.Any(a => a.Contains(request.SearchText)));
                }
                //add filter criteria
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                    else if (string.Equals("ProductMaster", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.IsMaster == true);
                    }
                }
                if (!string.IsNullOrEmpty(request._filter2))
                {
                    if (string.Equals("Information", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("Image", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("Category", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.CategoryTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("Variation", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("More", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("ReadyForAction", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p =>
                           p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.CategoryTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                }
                //count number of products
                var total = products.Count();
                //make paginate query from database
                var pagedProducts = products.Paginate(request);
                //create response
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                //return response
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            //if anything wrong happen return error
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages")]
        [HttpGet]
        //[ClaimsAuthorize(Permission = new string[] { "View Product", "View All Products" })]
        public HttpResponseMessage GetProductStages([FromUri] ProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }

                var products =  db.ProductStages.Where(w => w.IsVariant == false && !Constant.STATUS_REMOVE.Equals(w.Status)).Select(s => new
                {
                    s.Sku,
                    s.Pid,
                    s.Upc,
                    s.ProductId,
                    s.ProductNameEn,
                    s.ProductNameTh,
                    s.OriginalPrice,
                    s.SalePrice,
                    s.ProductStageGroup.Status,
                    s.ProductStageGroup.ImageFlag,
                    s.ProductStageGroup.InfoFlag,
                    s.ProductStageGroup.OnlineFlag,
                    s.Visibility,
                    s.VariantCount,
                    s.IsMaster,
                    ImageUrl = string.Concat(Constant.IMAGE_STATIC_URL,s.FeatureImgUrl),
                    s.ProductStageGroup.GlobalCatId,
                    s.ProductStageGroup.LocalCatId,
                    s.ProductStageGroup.AttributeSetId,
                    s.ProductStageAttributes,
                    UpdatedDt = s.ProductStageGroup.UpdateOn,
                    s.ShopId,
                    s.ProductStageGroup.InformationTabStatus,
                    s.ProductStageGroup.ImageTabStatus,
                    s.ProductStageGroup.CategoryTabStatus,
                    s.ProductStageGroup.VariantTabStatus,
                    s.ProductStageGroup.MoreOptionTabStatus,
                    Tags = s.ProductStageGroup.ProductStageTags.Select(t => t.Tag),
                    Shop = new { s.Shop.ShopId, s.Shop.ShopNameEn },
                    Brand = s.ProductStageGroup.Brand != null ? new { s.ProductStageGroup.Brand.BrandId, s.ProductStageGroup.Brand.BrandNameEn } : null,
                });
                if (User.ShopRequest() != null)
                {
                    int shopId = User.ShopRequest().ShopId;
                    products = products.Where(w => w.ShopId == shopId);
                    if(User.BrandRequest() != null)
                    {
                        var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
                        if (brands != null && brands.Count > 0)
                        {
                            products = products.Where(w => brands.Contains(w.Brand.BrandId));
                        }
                    }
                }
                request.DefaultOnNull();
                if (request.GlobalCatId != 0)
                {
                    products = products.Where(p => p.GlobalCatId == request.GlobalCatId);
                }
                if (request.LocalCatId != 0)
                {
                    products = products.Where(p => p.LocalCatId == request.LocalCatId);
                }
                if (request.AttributeSetId != 0)
                {
                    products = products.Where(p => p.LocalCatId == request.LocalCatId);
                }
                if (request.AttributeId != 0)
                {
                    products = products.Where(p => p.ProductStageAttributes.All(a => a.AttributeId == request.AttributeId));
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText)
                    || p.Pid.Contains(request.SearchText)
                    || p.Upc.Contains(request.SearchText)
                    || p.Tags.Any(a=>a.Contains(request.SearchText)));
                }
                if (!string.IsNullOrEmpty(request.Pid))
                {
                    products = products.Where(p => p.Pid.Equals(request.Pid));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                    else if (string.Equals("ProductMaster", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.IsMaster == true);
                    }
                }
                if (!string.IsNullOrEmpty(request._filter2))
                {
                    if (string.Equals("Information", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("Image", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("Variation", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("More", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("ReadyForAction", request._filter2, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p =>
                           p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
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

        //[Route("api/ProductStages/{productId}")]
        //[HttpPut]
        //public HttpResponseMessage SaveChangeProduct([FromUri]long productId, ProductStageRequest request)
        //{
        //    try
        //    {

        //        #region Query
        //        var tmpProduct = db.ProductStageGroups.Where(w => w.ProductId == productId)
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageAttributes.Select(sa => sa.Attribute.AttributeValueMaps.Select(sv => sv.AttributeValue))))
        //            .Include(i => i.ProductStages.Select(s => s.Inventory))
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
        //            .Include(i => i.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory))
        //            .Include(i => i.ProductStageLocalCatMaps.Select(s => s.LocalCategory))
        //            .Include(i=>i.ProductStageTags)
        //            .Include(i=>i.ProductStageRelateds1);
        //        ProductStageGroup group = null;
        //        if(User.ShopRequest() != null)
        //        {
        //            var shopId = User.ShopRequest().ShopId;
        //            group = tmpProduct.Where(w => w.ShopId == shopId).SingleOrDefault();
        //        }
        //        else
        //        {
        //            group = tmpProduct.SingleOrDefault();
        //        }

        //        //var product = db.ProductStageGroups.Where(w => w.ShopId == shopId
        //        //    && w.ProductId == productId)
        //        //    .Include(i => i.ProductStages.Select(s => s.ProductStageAttributes.Select(sa=>sa.Attribute.AttributeValueMaps.Select(sv=>sv.AttributeValue))))
        //        //    .Include(i => i.ProductStages.Select(s => s.Inventory))
        //        //    .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
        //        //    .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
        //        //    .Include(i => i.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory))
        //        //    .Include(i => i.ProductStageLocalCatMaps.Select(s => s.LocalCategory)).SingleOrDefault();
        //        if(group == null)
        //        {
        //            throw new Exception("Cannot find product "+ productId);
        //        }
        //        var attributeList = db.Attributes.Include(i => i.AttributeValueMaps).ToList();
        //        var shippingList = db.Shippings.ToList();
        //        #endregion
        //        string email = User.UserRequest().Email;
        //        DateTime currentDt = DateTime.Now;
        //        bool adminPermission = User.HasPermission("Approve Products");
        //        bool sellerPermission = User.HasPermission("Add Product");
        //        #region Setup Group
        //        SetupGroup(group, request,false,email, currentDt, adminPermission, sellerPermission, db, shippingList);
        //        #endregion
        //        #region Master Variant
        //        var masterVariant = group.ProductStages.Where(w=>w.IsVariant==false).FirstOrDefault();
        //        request.MasterVariant.Status = group.Status;
        //        SetupVariant(masterVariant, request.MasterVariant, false, email, currentDt, adminPermission, sellerPermission, db);
        //        masterVariant.Visibility = request.Visibility;
        //        SetupAttribute(masterVariant, request.MasterAttribute, attributeList, email, db);
        //        #endregion
        //        #region Variants
        //        var tmpVariant = group.ProductStages.Where(w => w.IsVariant == true).ToList();
        //        if (request.Variants != null && request.Variants.Count > 0)
        //        {
        //            foreach (var variantRq in request.Variants)
        //            {
        //                bool isNew = false;
        //                ProductStage variant = null;
        //                if (tmpVariant == null || tmpVariant.Count == 0)
        //                {
        //                    isNew = true;
        //                }
        //                if (!isNew)
        //                {
        //                    var current = tmpVariant.Where(w => w.Pid.Equals(variantRq.Pid)).SingleOrDefault();
        //                    if(current != null)
        //                    {
        //                        variant = current;
        //                        tmpVariant.Remove(current);
        //                    }
        //                    else
        //                    {
        //                        isNew = true;
        //                    }
        //                }
        //                if (isNew)
        //                {
        //                    variant = new ProductStage();
        //                    variant.ShopId = masterVariant.ShopId;
        //                    variant.IsVariant = true;
        //                    variant.CreateBy = email;
        //                    variant.CreateOn = currentDt;
        //                    variant.UpdateBy = email;
        //                    variant.UpdateOn = currentDt;
        //                    group.ProductStages.Add(variant);
        //                }
        //                variantRq.Status = group.Status;
        //                SetupVariant(variant, variantRq, false, email, currentDt, adminPermission, sellerPermission, db);
        //                SetupAttribute(variant, new List<AttributeRequest>() { variantRq.FirstAttribute, variantRq.SecondAttribute }, attributeList, email, db);
        //            }
        //            masterVariant.VariantCount = group.ProductStages.Where(w => w.IsVariant == true && w.Visibility == true).ToList().Count;
        //            masterVariant.IsSell = false;
        //            //if (masterVariant.VariantCount == 0)
        //            //{
        //            //    throw new Exception("Minimum 1 variant is required");
        //            //}
        //        }
        //        else
        //        {
        //            masterVariant.VariantCount = 0;
        //            masterVariant.IsSell = true;
        //        }
        //        if(tmpVariant != null && tmpVariant.Count > 0)
        //        {
        //            db.ProductStages.RemoveRange(tmpVariant);
        //        }
        //        #endregion
        //        #region Check Flag
        //        var defaultAttribute = attributeList.Where(w => w.DefaultAttribute == true && w.Required == true).ToList();
        //        if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
        //            && !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
        //            && group.BrandId != null
        //            && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullEn)
        //            && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullTh))
        //        {
        //            group.InfoFlag = true;
        //        }
        //        else
        //        {
        //            group.InfoFlag = false;
        //        }
        //        if (!string.IsNullOrEmpty(masterVariant.FeatureImgUrl))
        //        {
        //            group.ImageFlag = true;
        //        }
        //        else
        //        {
        //            group.ImageFlag = false;
        //        }
        //        #endregion
        //        AutoGenerate.GeneratePid(db, group.ProductStages);
        //        Util.DeadlockRetry(db.SaveChanges, "ProductStage");
        //        SetupGroupAfterSave(group);
        //        Util.DeadlockRetry(db.SaveChanges, "ProductStageImage");
        //        if (Constant.PRODUCT_STATUS_APPROVE.Equals(group.Status))
        //        {
        //            SetupApprovedProduct(group, db);
        //            group.OnlineFlag = true;
        //            Util.DeadlockRetry(db.SaveChanges, "ProductStage");
        //        }
        //        return GetProductStage(group.ProductId);
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
        //    }
        //}







        [Route("api/ProductStages")]
        [HttpPost]
        public HttpResponseMessage AddProduct(ProductStageRequest request)
        {
            try
            {
                #region request validation
                var shop = User.ShopRequest();
                if (request == null && shop == null)
                {
                    throw new Exception("Invalid request");
                }
                #endregion
                #region attribute
                List<int> attributeids = new List<int>();
                attributeids.AddRange(request.MasterAttribute.Where(w => w.AttributeId != 0).Select(s => s.AttributeId));
                attributeids.AddRange(request.Variants.Where(w => w.FirstAttribute.AttributeId != 0).Select(s => s.FirstAttribute.AttributeId));
                attributeids.AddRange(request.Variants.Where(w => w.SecondAttribute.AttributeId != 0).Select(s => s.SecondAttribute.AttributeId));

                var attributeList = db.Attributes.Where(w => attributeids.Contains(w.AttributeId)).Select(s => new AttributeRequest()
                {
                    AttributeId = s.AttributeId,
                    DataType = s.DataType,
                    AttributeValues = s.AttributeValueMaps.Select(sv => new AttributeValueRequest()
                    {
                        AttributeValueId = sv.AttributeValueId,
                    }).ToList(),
                }).ToList();
                #endregion
                #region inventory
                //List<string> pids = new List<string>();
                //pids.Add(request.MasterVariant.Pid);
                //pids.AddRange(request.Variants.Select(s => s.Pid));
                //var inventoryList = db.Inventories.Where(w => pids.Contains(w.Pid)).ToList();
                var inventoryList = new List<Inventory>();
                #endregion
                ProductStageGroup group = new ProductStageGroup();
                SetupProductStageGroup(group, request, attributeList, inventoryList, shop.ShopId, false, true, User.UserRequest().Email, DateTime.Now,db);
                AutoGenerate.GeneratePid(db, group.ProductStages);
                group.ProductId = db.GetNextProductStageGroupId().Single().Value;
                db.ProductStageGroups.Add(group);
                SetupGroupAfterSave(group, db, true);
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                //Util.DeadlockRetry(db.SaveChanges, "ProductStageImage");
                return GetProductStage(group.ProductId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/{productId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeProduct([FromUri]long productId, ProductStageRequest request)
        {
            try
            {

                var tmpGroup = db.ProductStageGroups.Where(w => true);
                bool isAdmin = true;
                if(User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    tmpGroup = tmpGroup.Where(w => w.ShopId == shopId);
                    isAdmin = false;
                }

                var group = tmpGroup.Where(w => w.ProductId == productId).Include(i => i.ProductStages).SingleOrDefault();
                if(group == null)
                {
                    throw new Exception(string.Concat("Cannot find product with id ",productId));
                }
                db.ProductStageGlobalCatMaps.RemoveRange(db.ProductStageGlobalCatMaps.Where(w => w.ProductId == productId));
                db.ProductStageLocalCatMaps.RemoveRange(db.ProductStageLocalCatMaps.Where(w => w.ProductId == productId));
                db.ProductStageTags.RemoveRange(db.ProductStageTags.Where(w => w.ProductId == productId));
                db.ProductStageRelateds.RemoveRange(db.ProductStageRelateds.Where(w => w.Parent == productId));
                List<string> pids = new List<string>();
                pids.Add(request.MasterVariant.Pid);
                pids.AddRange(request.Variants.Select(s => s.Pid));
                db.ProductStageAttributes.RemoveRange(db.ProductStageAttributes.Where(w => pids.Contains(w.Pid)));
                db.ProductStageImages.RemoveRange(db.ProductStageImages.Where(w => pids.Contains(w.Pid)));
                db.ProductStageVideos.RemoveRange(db.ProductStageVideos.Where(w => pids.Contains(w.Pid)));
                var inventoryList = db.Inventories.Where(w => pids.Contains(w.Pid)).ToList();
                #region attribute
                List<int> attributeids = new List<int>();
                attributeids.AddRange(request.MasterAttribute.Where(w => w.AttributeId != 0).Select(s => s.AttributeId));
                attributeids.AddRange(request.Variants.Where(w => w.FirstAttribute.AttributeId != 0).Select(s => s.FirstAttribute.AttributeId));
                attributeids.AddRange(request.Variants.Where(w => w.SecondAttribute.AttributeId != 0).Select(s => s.SecondAttribute.AttributeId));
                var attributeList = db.Attributes.Where(w => attributeids.Contains(w.AttributeId)).Select(s => new AttributeRequest()
                {
                    AttributeId = s.AttributeId,
                    DataType = s.DataType,
                    AttributeValues = s.AttributeValueMaps.Select(sv => new AttributeValueRequest()
                    {
                        AttributeValueId = sv.AttributeValueId,
                    }).ToList(),
                }).ToList();
                #endregion
                SetupProductStageGroup(group, request, attributeList, inventoryList, group.ShopId, isAdmin, false, User.UserRequest().Email, DateTime.Now, db);
                AutoGenerate.GeneratePid(db, group.ProductStages);
                SetupGroupAfterSave(group, db, false);
                if (Constant.RETURN_STATUS_APPROVE.Equals(group.Status))
                {
                    SetupApprovedProduct(group, db);
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return GetProductStage(group.ProductId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/{productId}")]
        [HttpGet]
        public HttpResponseMessage GetProductStage(long productId)
        {
            try
            {
                ProductStageRequest response = GetProductStageRequestFromId(db, productId);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private void SetupProductStageGroup(ProductStageGroup group, ProductStageRequest request, List<AttributeRequest> attributeList,List<Inventory> inventoryList, int shopId, bool isAdmin, bool isNew, string email, DateTime currentDt, ColspEntities db)
        {
            group.ShopId = shopId;
            #region setup category
            if (request.MainGlobalCategory != null && request.MainGlobalCategory.CategoryId != 0)
            {
                group.GlobalCatId = request.MainGlobalCategory.CategoryId;
            }
            else
            {
                throw new Exception("Invalid global category id");
            }
            group.LocalCatId = null;
            if (request.MainLocalCategory != null && request.MainLocalCategory.CategoryId != 0)
            {
                group.LocalCatId = request.MainLocalCategory.CategoryId;
            }
            if(request.GlobalCategories != null)
            {
                foreach(var category in request.GlobalCategories)
                {
                    group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                    {
                        CategoryId = category.CategoryId,
                        CreateBy = email,
                        CreateOn = currentDt,
                        UpdateBy = email,
                        UpdateOn = currentDt,
                    });
                }
            }
            if (request.LocalCategories != null)
            {
                foreach (var category in request.LocalCategories)
                {
                    group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                    {
                        CategoryId = category.CategoryId,
                        CreateBy = email,
                        CreateOn = currentDt,
                        UpdateBy = email,
                        UpdateOn = currentDt,
                    });
                }
            }
            #endregion
            #region setup other mapping
            group.AttributeSetId = null;
            if (request.AttributeSet != null && request.AttributeSet.AttributeSetId != 0)
            {
                group.AttributeSetId = request.AttributeSet.AttributeSetId;
            }
            group.BrandId = null;
            if (request.Brand != null && request.Brand.BrandId != 0)
            {
                group.BrandId = request.Brand.BrandId;
            }
            if (request.ShippingMethod != 0)
            {
                group.ShippingId = request.ShippingMethod;
            }
            else
            {
                group.ShippingId = Constant.DEFAULT_SHIPPING_ID;
            }
            if(request.Tags != null)
            {
                foreach (var tag in request.Tags)
                {
                    group.ProductStageTags.Add(new ProductStageTag()
                    {
                        CreateBy = email,
                        CreateOn = currentDt,
                        Tag = tag,
                        UpdateBy = email,
                        UpdateOn = currentDt
                    });
                }
            }
            if (request.RelatedProducts != null && request.RelatedProducts.Count > 0)
            {
                foreach (var product in request.RelatedProducts)
                {
                    group.ProductStageRelateds1.Add(new ProductStageRelated()
                    {
                        Child = product.ProductId,
                        ShopId = group.ShopId,
                        CreateBy = email,
                        CreateOn = currentDt,
                        UpdateBy = email,
                        UpdateOn = currentDt
                    });
                }
            }
            #endregion
            #region setup other field
            group.EffectiveDate = request.EffectiveDate.HasValue ? request.EffectiveDate.Value : currentDt;
            group.ExpireDate = request.ExpireDate.HasValue && request.ExpireDate.Value.CompareTo(group.EffectiveDate) >= 0 ? request.ExpireDate.Value : group.EffectiveDate.AddYears(Constant.DEFAULT_ADD_YEAR);
            group.NewArrivalDate = request.NewArrivalDate;
            group.TheOneCardEarn = request.TheOneCardEarn;
            group.GiftWrap = request.GiftWrap;
            group.Status = request.Status;
            group.Remark = request.Remark;
            
            if (request.ControlFlags != null)
            {
                group.IsNew = request.ControlFlags.IsNew;
                group.IsClearance = request.ControlFlags.IsClearance;
                group.IsBestSeller = request.ControlFlags.IsBestSeller;
                group.IsOnlineExclusive = request.ControlFlags.IsOnlineExclusive;
                group.IsOnlyAt = request.ControlFlags.IsOnlyAt;
            }
            if (isNew)
            {
                group.InformationTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                group.ImageTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                group.CategoryTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                group.VariantTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                group.MoreOptionTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                group.RejectReason = string.Empty;
                group.FirstApproveBy = null;
                group.FirstApproveOn = null;
                group.ApproveBy = null;
                group.ApproveOn = null;
                group.RejecteBy = null;
                group.RejectOn = null;
                group.SubmitBy = null;
                group.SubmitOn = null;
                group.CreateBy = email;
                group.CreateOn = currentDt;
            }
            if (isAdmin)
            {
                group.InformationTabStatus = request.AdminApprove.Information;
                group.ImageTabStatus = request.AdminApprove.Image;
                group.CategoryTabStatus = request.AdminApprove.Category;
                group.VariantTabStatus = request.AdminApprove.Variation;
                group.MoreOptionTabStatus = request.AdminApprove.MoreOption;
                group.RejectReason = request.AdminApprove.RejectReason;
            }
            if (Constant.PRODUCT_STATUS_APPROVE.Equals(group.Status))
            {
                group.ApproveBy = email;
                group.ApproveOn = currentDt;
                if (group.FirstApproveBy == null)
                {
                    group.FirstApproveBy = email;
                    group.FirstApproveOn = currentDt;
                }
            }
            else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(group.Status))
            {
                group.SubmitBy = email;
                group.SubmitOn = currentDt;
            }
            else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(group.Status))
            {
                group.RejecteBy = email;
                group.RejectOn = currentDt;
            }
            group.UpdateBy = email;
            group.UpdateOn = currentDt;
            #endregion
            #region setup master
            if (request.MasterVariant == null)
            {
                throw new Exception("Invalid master variant");
            }

            ProductStage masterVariant = group.ProductStages.Where(w => w.Pid.Equals(request.MasterVariant.Pid)).SingleOrDefault();
            if(masterVariant == null)
            {
                masterVariant = new ProductStage()
                {
                    IsVariant = false,
                };
                group.ProductStages.Add(masterVariant);
            }
            request.MasterVariant.Status = group.Status;
            SetupProductStage(masterVariant, request.MasterVariant, attributeList,inventoryList.Where(w=>w.Pid.Equals(masterVariant.Pid)).SingleOrDefault(), shopId,isAdmin, isNew, email, currentDt, db);
            if(request.MasterAttribute != null)
            {
                SetupAttribute(masterVariant, request.MasterAttribute, attributeList, email, currentDt);
            }
            
            #endregion
            #region setup variant
            if (request.Variants != null)
            {

                var varaintList = group.ProductStages.Where(w=>!Constant.STATUS_REMOVE.Equals(group.Status)).ToList();

                foreach (var variantRequest in request.Variants)
                {

                    bool isNewProduct = false;
                    ProductStage variant = null;
                    if (varaintList == null || varaintList.Count == 0)
                    {
                        isNewProduct = true;
                    }
                    if (!isNewProduct)
                    {
                        var currentProduct = varaintList.Where(w => w.Pid != null && w.Pid.Equals(variantRequest.Pid)).SingleOrDefault();
                        if (currentProduct != null)
                        {
                            variant = currentProduct;
                            varaintList.Remove(currentProduct);
                        }
                        else
                        {
                            isNewProduct = true;
                        }
                    }

                    if (isNewProduct)
                    {
                        variant = new ProductStage()
                        {
                            CreateBy = email,
                            CreateOn = currentDt
                        };
                        group.ProductStages.Add(variant);
                    }
                    variantRequest.Status = group.Status;
                    SetupProductStage(variant, variantRequest, attributeList, inventoryList.Where(w => w.Pid.Equals(variant.Pid)).SingleOrDefault(), shopId, isAdmin, isNew, email, currentDt,db);
                    variant.IsVariant = true;
                    variant.IsSell = true;
                }
                masterVariant.VariantCount = request.Variants.Where(w=>w.Visibility == true).Count();
                if (masterVariant.VariantCount == 0)
                {
                    masterVariant.IsSell = true;
                }
            }
            #endregion
            #region Check Flag
            if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
                && !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
                && !string.IsNullOrWhiteSpace(masterVariant.Sku)
                && group.BrandId != null
                && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullEn)
                && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullTh)
                && !string.IsNullOrWhiteSpace(masterVariant.MobileDescriptionEn)
                && !string.IsNullOrWhiteSpace(masterVariant.MobileDescriptionTh))
            {
                group.InfoFlag = true;
            }
            else
            {
                group.InfoFlag = false;
            }
            if (!string.IsNullOrEmpty(masterVariant.FeatureImgUrl))
            {
                group.ImageFlag = true;
            }
            else
            {
                group.ImageFlag = false;
            }
            #endregion
        }

        private void SetupProductStage(ProductStage stage, VariantRequest request, List<AttributeRequest> attributeList,Inventory inventory, int shopId, bool isAdmin, bool isNew, string email, DateTime currentDt,ColspEntities db)
        {
            stage.Pid = request.Pid;
            stage.ShopId = shopId;
            stage.ProductNameEn = request.ProductNameEn;
            stage.ProductNameTh = request.ProductNameTh;
            stage.ProdTDNameTh = request.ProdTDNameTh;
            stage.ProdTDNameEn = request.ProdTDNameEn;
            stage.JDADept = string.Empty;
            stage.JDASubDept = string.Empty;
            stage.SaleUnitTh = request.SaleUnitTh;
            stage.SaleUnitEn = request.SaleUnitEn;
            stage.Sku = request.Sku;
            stage.Upc = request.Upc;
            stage.OriginalPrice = request.OriginalPrice;
            stage.SalePrice = request.SalePrice;
            stage.DescriptionFullEn = request.DescriptionFullEn;
            stage.DescriptionShortEn = request.DescriptionShortEn;
            stage.DescriptionFullTh = request.DescriptionFullTh;
            stage.DescriptionShortTh = request.DescriptionShortTh;
            stage.MobileDescriptionEn = request.MobileDescriptionEn;
            stage.MobileDescriptionTh = request.MobileDescriptionTh;
            stage.ImageCount = 0;
            #region Images
            if (request.Images != null)
            {
                stage.ImageCount = request.Images.Count;
                int position = 1;
                foreach (var img in request.Images)
                {
                    if (string.IsNullOrWhiteSpace(img.Url))
                    {
                        continue;
                    }
                    stage.ProductStageImages.Add(new ProductStageImage()
                    {
                        FeatureFlag = position == 1 ? true : false,
                        Large = true,
                        Normal = true,
                        Thumbnail = true,
                        Zoom = true,
                        CreateBy = email,
                        CreateOn = currentDt,
                        SeqNo = position++,
                        ShopId = shopId,
                        Status = Constant.STATUS_ACTIVE,
                        ImageName = img.Url.Split('/').Last(),
                        UpdateBy = email,
                        UpdateOn = currentDt
                    });
                }
                stage.FeatureImgUrl = stage.ProductStageImages.Where(w => w.FeatureFlag == true).Select(s => s.ImageName).FirstOrDefault();
                if(stage.FeatureImgUrl == null)
                {
                    stage.FeatureImgUrl = string.Empty;
                }
            }
            #endregion
            #region Video
            if (request.VideoLinks != null)
            {
                int position = 1;
                foreach(var video in request.VideoLinks)
                {
                    if(string.IsNullOrWhiteSpace(video.Url))
                    {
                        continue;
                    }
                    stage.ProductStageVideos.Add(new ProductStageVideo()
                    {
                        CreateBy = email,
                        CreateOn = currentDt,
                        Position = position++,
                        ShopId = shopId,
                        Status = Constant.STATUS_ACTIVE,
                        VideoUrlEn = video.Url,
                        UpdateBy = email,
                        UpdateOn = currentDt
                    });
                }
            }
            #endregion
            stage.PrepareDay = request.PrepareDay;
            stage.LimitIndividualDay = request.LimitIndividualDay;
            stage.PrepareMon = request.PrepareMon;
            stage.PrepareTue = request.PrepareTue;
            stage.PrepareWed = request.PrepareWed;
            stage.PrepareThu = request.PrepareThu;
            stage.PrepareFri = request.PrepareFri;
            stage.PrepareSat = request.PrepareSat;
            stage.PrepareSun = request.PrepareSun;
            stage.KillerPoint1En = request.KillerPoint1En;
            stage.KillerPoint2En = request.KillerPoint2En;
            stage.KillerPoint3En = request.KillerPoint3En;
            stage.KillerPoint1Th = request.KillerPoint1Th;
            stage.KillerPoint2Th = request.KillerPoint2Th;
            stage.KillerPoint3Th = request.KillerPoint3Th;
            stage.Installment = request.Installment;
            stage.Length = request.Length;
            stage.Height = request.Height;
            stage.Width = request.Width;
            stage.DimensionUnit = request.DimensionUnit;
            if (Constant.DIMENSTION_CM.Equals(stage.DimensionUnit))
            {
                stage.Length = stage.Length * 10;
                stage.Height = stage.Height * 10;
                stage.Width = stage.Width * 10;
            }
            else if (Constant.DIMENSTION_M.Equals(stage.DimensionUnit))
            {
                stage.Length = stage.Length * 1000;
                stage.Height = stage.Height * 1000;
                stage.Width = stage.Width * 1000;
            }

            stage.Weight = request.Weight;
            stage.WeightUnit = request.WeightUnit;

            if (Constant.WEIGHT_MEASURE_KG.Equals(request.WeightUnit))
            {
                stage.Weight = stage.Weight * 1000;
            }
            if (request.SEO != null)
            {
                stage.MetaTitleEn = request.SEO.MetaTitleEn;
                stage.MetaTitleTh = request.SEO.MetaTitleTh;
                stage.MetaDescriptionEn = request.SEO.MetaDescriptionEn;
                stage.MetaDescriptionTh = request.SEO.MetaDescriptionTh;
                stage.MetaKeyEn = request.SEO.MetaKeywordEn;
                stage.MetaKeyTh = request.SEO.MetaKeywordTh;
                stage.SeoEn = request.SEO.SeoEn;
                stage.SeoTh = request.SEO.SeoTh;
                stage.UrlKey = request.SEO.ProductUrlKeyEn;
                stage.BoostWeight = request.SEO.ProductBoostingWeight;
                stage.GlobalBoostWeight = request.SEO.GlobalProductBoostingWeight;
                if (isAdmin)
                {
                    stage.GlobalBoostWeight = request.SEO.GlobalProductBoostingWeight;
                }
            }
            stage.IsHasExpiryDate = request.IsHasExpiryDate;
            stage.IsVat = request.IsVat;
            stage.ExpressDelivery = request.ExpressDelivery;
            stage.DeliveryFee = request.DeliveryFee;
            stage.PromotionPrice = request.PromotionPrice;
            stage.EffectiveDatePromotion = request.EffectiveDatePromotion;
            stage.ExpireDatePromotion = request.ExpireDatePromotion;
            stage.NewArrivalDate = request.NewArrivalDate;
            stage.DefaultVariant = request.DefaultVariant;
            stage.Display = request.Display;
            stage.MiniQtyAllowed = request.MiniQtyAllowed;
            stage.MaxiQtyAllowed = request.MaxiQtyAllowed;
            stage.UnitPrice = request.UnitPrice;
            stage.PurchasePrice = request.PurchasePrice;
            stage.Status = request.Status;
            stage.IsSell = false;
            stage.IsVariant = false;
            stage.VariantCount = 0;
            stage.Visibility = request.Visibility;
            stage.OldPid = null;
            stage.Bu = null;
            if (isNew)
            {
                stage.CreateBy = email;
                stage.CreateOn = currentDt;
            }
            stage.UpdateBy = email;
            stage.UpdateOn = currentDt;
            if (request.FirstAttribute != null && request.FirstAttribute.AttributeId != 0)
            {
                SetupAttribute(stage, new List<AttributeRequest>() { request.FirstAttribute }, attributeList, email, currentDt);
            }
            if (request.SecondAttribute != null && request.SecondAttribute.AttributeId != 0)
            {
                SetupAttribute(stage, new List<AttributeRequest>() { request.SecondAttribute }, attributeList, email, currentDt);
            }
            int defaultStockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
            if (Constant.STOCK_TYPE.ContainsKey(request.StockType))
            {
                defaultStockType = Constant.STOCK_TYPE[request.StockType];
            }
            if (stage.Inventory == null)
            {
                stage.Inventory = new Inventory()
                {
                    MaxQtyAllowInCart = request.MaxQtyAllowInCart,
                    Defect = 0,
                    MaxQtyPreOrder = request.MaxQtyPreOrder,
                    MinQtyAllowInCart = request.MinQtyAllowInCart,
                    OnHold = 0 + request.UpdateAmount,
                    Quantity = 0 + request.Quantity,
                    Reserve = 0 + request.Reserve,
                    SafetyStockAdmin = request.SafetyStock,
                    StockType = defaultStockType,
                    SafetyStockSeller = request.SafetyStock,
                    UseDecimal = request.UseDecimal,
                };
            }
            else
            {
                inventory.Quantity = inventory.Quantity + request.UpdateAmount;
                inventory.StockType = defaultStockType;
                inventory.SafetyStockSeller = request.SafetyStock;
                inventory.MaxQtyAllowInCart = request.MaxQtyAllowInCart;
                inventory.MaxQtyPreOrder = request.MaxQtyPreOrder;
                inventory.MinQtyAllowInCart = request.MinQtyAllowInCart;
                if (isAdmin)
                {
                    inventory.OnHold = request.OnHold;
                    inventory.Reserve = request.Reserve;
                    inventory.Defect = request.Defect;
                }
                if (request.UpdateAmount != 0)
                {
                    db.InventoryHistories.Add(new InventoryHistory()
                    {
                        CreateBy = email,
                        CreateOn = currentDt,
                        Defect = inventory.Defect,
                        MaxQtyAllowInCart = inventory.MaxQtyAllowInCart,
                        MaxQtyPreOrder = inventory.MaxQtyPreOrder,
                        MinQtyAllowInCart = inventory.MinQtyAllowInCart,
                        OnHold = inventory.OnHold,
                        Pid = inventory.Pid,
                        Quantity = inventory.Quantity,
                        Reserve = inventory.Reserve,
                        SafetyStockAdmin = inventory.SafetyStockAdmin,
                        SafetyStockSeller = inventory.SafetyStockSeller,
                        StockType = inventory.StockType,
                        UpdateBy = email,
                        UpdateOn = currentDt,
                        UseDecimal = inventory.UseDecimal,
                        Status = Constant.INVENTORY_STATUS_UPDATE,
                    });
                }
            }

        }

        private void SetupAttribute(ProductStage variant, List<AttributeRequest> requestList, List<AttributeRequest> attributeList, string email, DateTime currentDt)
        {
            int position = 1;
            foreach (var attributeRequest in requestList)
            {
                #region validation
                if (attributeRequest == null || attributeRequest.AttributeId == 0)
                {
                    continue;
                }
                var attribute = attributeList.Where(w => w.AttributeId == attributeRequest.AttributeId).SingleOrDefault();
                if (attribute == null)
                {
                    throw new Exception(string.Concat("No attribute found ", attributeRequest.AttributeId));
                }
                if (variant.IsVariant && !attribute.VariantStatus)
                {
                    throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " is not variant type"));
                }
                #endregion
                #region Instantiate
                string valueEn = attributeRequest.ValueEn;
                string valueTh = attributeRequest.ValueTh;
                bool isAttributeValue = false;
                bool checkboxValue = false;
                int? attributeValueId = null;
                #endregion
                #region List
                if (Constant.DATA_TYPE_LIST.Equals(attribute.DataType))
                {
                    if (attributeRequest.AttributeValues == null)
                    {
                        throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " should have value"));
                    }
                    if (attribute.AttributeValues.Any(a => attributeRequest.AttributeValues
                         .Select(s => s.AttributeValueId)
                         .Contains(a.AttributeValueId)))
                    {
                        valueEn = string.Concat(
                            Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
                            attributeRequest.AttributeValues.FirstOrDefault().AttributeValueId,
                            Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
                        valueTh = string.Concat(
                            Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
                            attributeRequest.AttributeValues.FirstOrDefault().AttributeValueId,
                            Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
                        isAttributeValue = true;
                        attributeValueId = attributeRequest.AttributeValues.FirstOrDefault().AttributeValueId;
                    }
                    else
                    {
                        throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " id ", attribute.AttributeId, " has no ", "Attribute value ",
                            string.Join(",", attributeRequest.AttributeValues.Select(s => s.AttributeValueEn))));
                    }
                }
                #endregion
                #region Check box
                else if (Constant.DATA_TYPE_CHECKBOX.Equals(attribute.DataType))
                {
                    if (attributeRequest.AttributeValues == null)
                    {
                        throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " should have variant"));
                    }
                    if (attribute.AttributeValues.Any(a => attributeRequest.AttributeValues
                         .Select(s => s.AttributeValueId)
                         .Contains(a.AttributeValueId)))
                    {
                        foreach (var attributeValue in attributeRequest.AttributeValues)
                        {
                            valueEn = string.Concat(
                                Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
                                attributeValue.AttributeValueId,
                                Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
                            valueTh = string.Concat(
                                Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
                                attributeValue.AttributeValueId,
                                Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
                            isAttributeValue = true;
                            attributeValueId = attributeValue.AttributeValueId;
                            checkboxValue = attributeValue.CheckboxValue;

                            variant.ProductStageAttributes.Add(new ProductStageAttribute()
                            {
                                AttributeId = attribute.AttributeId,
                                CheckboxValue = checkboxValue,
                                ValueEn = valueEn,
                                ValueTh = valueTh,
                                Position = position++,
                                IsAttributeValue = isAttributeValue,
                                AttributeValueId = attributeValueId,
                                CreateBy = email,
                                CreateOn = currentDt,
                                UpdateBy = email,
                                UpdateOn = currentDt,
                            });
                        }
                        continue;
                    }
                }
                #endregion
                #region Free text
                else
                {
                    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                    if (rg.IsMatch(valueEn) || rg.IsMatch(valueTh))
                    {
                        throw new Exception(string.Concat("Attribute value cannot contain prefix "
                            , Constant.ATTRIBUTE_VALUE_MAP_PREFIX
                            , " and surfix "
                            , Constant.ATTRIBUTE_VALUE_MAP_SURFIX));
                    }
                }
                #endregion
                #region Add attribute
                variant.ProductStageAttributes.Add(new ProductStageAttribute()
                {
                    AttributeId = attribute.AttributeId,
                    CheckboxValue = checkboxValue,
                    ValueEn = valueEn,
                    ValueTh = valueTh,
                    Position = position++,
                    IsAttributeValue = isAttributeValue,
                    AttributeValueId = attributeValueId,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                });
                #endregion
            }

        }

        private ProductStageRequest GetProductStageRequestFromId(ColspEntities db, long productId)
        {
            #region Query
            var tmpPro = db.ProductStageGroups.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status) && w.ProductId == productId)
                .Select(s => new
                {
                    s.ProductId,
                    Shop = new
                    {
                        s.ShopId,
                        s.Shop.ShopNameEn
                    },
                    MainGlobalCategory = new
                    {
                        s.GlobalCatId,
                        s.GlobalCategory.NameEn,
                    },
                    MainLocaCategory = s.LocalCatId == null ? null :  new
                    {
                        s.LocalCatId,
                        s.LocalCategory.NameEn,
                    },
                    s.AttributeSetId,
                    s.BrandId,
                    s.ShippingId,
                    s.EffectiveDate,
                    s.ExpireDate,
                    s.NewArrivalDate,
                    s.TheOneCardEarn,
                    s.GiftWrap,
                    s.IsNew,
                    s.IsClearance,
                    s.IsBestSeller,
                    s.IsOnlineExclusive,
                    s.IsOnlyAt,
                    s.Remark,
                    s.InfoFlag,
                    s.ImageFlag,
                    s.OnlineFlag,
                    s.InformationTabStatus,
                    s.ImageTabStatus,
                    s.CategoryTabStatus,
                    s.VariantTabStatus,
                    s.MoreOptionTabStatus,
                    s.RejectReason,
                    s.Status,
                    s.FirstApproveBy,
                    s.FirstApproveOn,
                    s.ApproveBy,
                    s.ApproveOn,
                    s.RejecteBy,
                    s.RejectOn,
                    s.SubmitBy,
                    s.SubmitOn,
                    s.CreateBy,
                    s.CreateOn,
                    s.UpdateBy,
                    s.UpdateOn,
                    ProductRelated = s.ProductStageRelateds1.Select(r=>new
                    {
                        Child = r.ProductStageGroup.ProductStages.Where(w=>w.IsVariant==false).Select(sm => new
                        {
                            sm.ProductId,
                            sm.Pid,
                            sm.ProductNameEn
                        })
                    }),
                    AttributeSet = s.AttributeSet == null ? null : new
                    {
                        s.AttributeSet.AttributeSetId,
                        s.AttributeSet.AttributeSetNameEn,
                        AttributeSetMap = s.AttributeSet.AttributeSetMaps.Select(sm => new
                        {
                            sm.Attribute.DataType,
                            sm.AttributeId,
                            AttributeValueMap = sm.Attribute.AttributeValueMaps.Select(sv => new
                            {
                                sv.AttributeValue.AttributeValueId,
                                sv.AttributeValue.AttributeValueEn
                            })
                        })
                    },
                    Brand = s.Brand == null ? null : new
                    {
                        s.Brand.BrandId,
                        s.Brand.BrandNameEn
                    },
                    GlobalCategories = s.ProductStageGlobalCatMaps.Select(sg => new
                    {
                        sg.CategoryId,
                        sg.GlobalCategory.NameEn
                    }),
                    LocalCategories = s.ProductStageLocalCatMaps.Select(sl => new
                    {
                        sl.CategoryId,
                        sl.LocalCategory.NameEn
                    }),
                    Tags = s.ProductStageTags.Select(st => st.Tag),
                    ProductStages = s.ProductStages.Select(st => new
                    {
                        st.Pid,
                        st.ProductId,
                        st.ShopId,
                        st.ProductNameEn,
                        st.ProductNameTh,
                        st.ProdTDNameTh,
                        st.ProdTDNameEn,
                        st.JDADept,
                        st.JDASubDept,
                        st.SaleUnitTh,
                        st.SaleUnitEn,
                        st.Sku,
                        st.Upc,
                        st.OriginalPrice,
                        st.SalePrice,
                        st.DescriptionFullEn,
                        st.DescriptionShortEn,
                        st.DescriptionFullTh,
                        st.DescriptionShortTh,
                        st.MobileDescriptionEn,
                        st.MobileDescriptionTh,
                        st.ImageCount,
                        st.FeatureImgUrl,
                        st.PrepareDay,
                        st.LimitIndividualDay,
                        st.PrepareMon,
                        st.PrepareTue,
                        st.PrepareWed,
                        st.PrepareThu,
                        st.PrepareFri,
                        st.PrepareSat,
                        st.PrepareSun,
                        st.KillerPoint1En,
                        st.KillerPoint2En,
                        st.KillerPoint3En,
                        st.KillerPoint1Th,
                        st.KillerPoint2Th,
                        st.KillerPoint3Th,
                        st.Installment,
                        st.Length,
                        st.Height,
                        st.Width,
                        st.DimensionUnit,
                        st.Weight,
                        st.WeightUnit,
                        st.MetaTitleEn,
                        st.MetaTitleTh,
                        st.MetaDescriptionEn,
                        st.MetaDescriptionTh,
                        st.MetaKeyEn,
                        st.MetaKeyTh,
                        st.SeoEn,
                        st.SeoTh,
                        st.IsHasExpiryDate,
                        st.IsVat,
                        st.UrlKey,
                        st.BoostWeight,
                        st.GlobalBoostWeight,
                        st.ExpressDelivery,
                        st.DeliveryFee,
                        st.PromotionPrice,
                        st.EffectiveDatePromotion,
                        st.ExpireDatePromotion,
                        st.NewArrivalDate,
                        st.DefaultVariant,
                        st.Display,
                        st.MiniQtyAllowed,
                        st.MaxiQtyAllowed,
                        st.UnitPrice,
                        st.PurchasePrice,
                        st.IsSell,
                        st.IsVariant,
                        st.IsMaster,
                        st.VariantCount,
                        st.Visibility,
                        ProductStageAttributes = st.ProductStageAttributes.Select(sa => new
                        {
                            sa.Pid,
                            sa.AttributeId,
                            sa.ValueEn,
                            sa.ValueTh,
                            sa.AttributeValueId,
                            sa.CheckboxValue,
                            sa.Position,
                            sa.IsAttributeValue,
                            Attribute = new
                            {
                                sa.Attribute.AttributeId,
                                sa.Attribute.AttributeNameEn,
                                sa.Attribute.DataType,
                                sa.Attribute.VariantStatus,
                            },
                            AttributeValue = sa.AttributeValue == null ? null : new
                            {
                                sa.AttributeValue.AttributeValueId,
                                sa.AttributeValue.AttributeValueEn,
                                sa.AttributeValue.AttributeValueTh,
                            }
                        }),
                        Images = st.ProductStageImages.Select(si => new
                        {
                            si.ImageName
                        }),
                        Videos = st.ProductStageVideos.OrderBy(o => o.Position).Select(sv => new
                        {
                            sv.VideoUrlEn
                        }),
                        Inventory = st.Inventory == null ? null : new
                        {
                            st.Inventory.OnHold,
                            st.Inventory.Reserve,
                            st.Inventory.Quantity,
                            st.Inventory.SafetyStockSeller,
                            st.Inventory.StockType,
                            st.Inventory.MaxQtyAllowInCart,
                            st.Inventory.MaxQtyPreOrder,
                            st.Inventory.MinQtyAllowInCart,
                            st.Inventory.Defect
                        }
                    }),

                }).SingleOrDefault();
            #endregion
            #region group
            ProductStageRequest response = new ProductStageRequest()
            {
                AdminApprove = new AdminApproveRequest()
                {
                    Category = tmpPro.CategoryTabStatus,
                    Image = tmpPro.ImageTabStatus,
                    Information = tmpPro.InformationTabStatus,
                    MoreOption = tmpPro.MoreOptionTabStatus,
                    RejectReason = tmpPro.RejectReason,
                    Variation = tmpPro.VariantTabStatus
                },
                AttributeSet = tmpPro.AttributeSet == null ? new AttributeSetRequest() : new AttributeSetRequest()
                {
                    AttributeSetId = tmpPro.AttributeSet.AttributeSetId,
                    AttributeSetNameEn = tmpPro.AttributeSet.AttributeSetNameEn,
                    Attributes = tmpPro.AttributeSet.AttributeSetMap.Select(s => new AttributeRequest()
                    {
                        AttributeId = s.AttributeId,
                        DataType = s.DataType,
                        AttributeValues = s.AttributeValueMap.Select(sv => new AttributeValueRequest()
                        {
                            AttributeValueEn = sv.AttributeValueEn,
                            AttributeValueId = sv.AttributeValueId,
                        }).ToList(),
                    }).ToList(),
                },
                Brand = tmpPro.Brand == null ? new BrandRequest() : new BrandRequest()
                {
                    BrandId = tmpPro.Brand.BrandId,
                    BrandNameEn = tmpPro.Brand.BrandNameEn
                },
                ControlFlags = new ControlFlagRequest()
                {
                    IsBestSeller = tmpPro.IsBestSeller,
                    IsClearance = tmpPro.IsClearance,
                    IsNew = tmpPro.IsNew,
                    IsOnlineExclusive = tmpPro.IsOnlineExclusive,
                    IsOnlyAt = tmpPro.IsOnlyAt
                },
                EffectiveDate = tmpPro.EffectiveDate,
                ExpireDate = tmpPro.ExpireDate,
                NewArrivalDate = tmpPro.NewArrivalDate,
                GiftWrap = tmpPro.GiftWrap,
                GlobalCategories = tmpPro.GlobalCategories.Select(s => new CategoryRequest()
                {
                    CategoryId = s.CategoryId,
                    NameEn = s.NameEn,
                }).ToList(),
                LocalCategories = tmpPro.LocalCategories.Select(s => new CategoryRequest()
                {
                    CategoryId = s.CategoryId,
                    NameEn = s.NameEn
                }).ToList(),
                ImageFlag = tmpPro.ImageFlag,
                InfoFlag = tmpPro.InfoFlag,
                ProductId = tmpPro.ProductId,
                MainGlobalCategory = new CategoryRequest()
                {
                    CategoryId = tmpPro.MainGlobalCategory.GlobalCatId,
                    NameEn = tmpPro.MainGlobalCategory.NameEn
                },
                MainLocalCategory = new CategoryRequest()
                {
                    CategoryId = tmpPro.MainLocaCategory != null ? tmpPro.MainLocaCategory.LocalCatId.Value : 0,
                    NameEn = tmpPro.MainLocaCategory != null ? tmpPro.MainLocaCategory.NameEn : string.Empty,
                },
                OnlineFlag = tmpPro.OnlineFlag,
                Remark = tmpPro.Remark,
                ShippingMethod = tmpPro.ShippingId,
                ShopId = tmpPro.Shop.ShopId,
                ShopName = tmpPro.Shop.ShopNameEn,
                Status = tmpPro.Status,
                Tags = tmpPro.Tags.ToList()
            };

            #endregion
            #region related product
            foreach (var related in tmpPro.ProductRelated)
            {
                foreach(var child in related.Child)
                {
                    response.RelatedProducts.Add(new VariantRequest()
                    {
                        ProductNameEn = child.ProductNameEn,
                        ProductId = child.ProductId,
                        Pid = child.Pid
                    });
                }
            }
            #endregion
            #region master
            var masterVariant = tmpPro.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
            response.MasterVariant = new VariantRequest()
            {
                Pid = masterVariant.Pid,
                ProductId = masterVariant.ProductId,
                ShopId = masterVariant.ShopId,
                ProductNameEn = masterVariant.ProductNameEn,
                ProductNameTh = masterVariant.ProductNameTh,
                ProdTDNameTh = masterVariant.ProdTDNameTh,
                ProdTDNameEn = masterVariant.ProdTDNameEn,
                SaleUnitTh = masterVariant.SaleUnitTh,
                SaleUnitEn = masterVariant.SaleUnitEn,
                Sku = masterVariant.Sku,
                Upc = masterVariant.Upc,
                OriginalPrice = masterVariant.OriginalPrice,
                SalePrice = masterVariant.SalePrice,
                DescriptionFullEn = masterVariant.DescriptionFullEn,
                DescriptionShortEn = masterVariant.DescriptionShortEn,
                DescriptionFullTh = masterVariant.DescriptionFullTh,
                DescriptionShortTh = masterVariant.DescriptionShortTh,
                MobileDescriptionEn = masterVariant.MobileDescriptionEn,
                MobileDescriptionTh = masterVariant.MobileDescriptionTh,
                PrepareDay = masterVariant.PrepareDay,
                LimitIndividualDay = masterVariant.LimitIndividualDay,
                PrepareMon = masterVariant.PrepareMon,
                PrepareTue = masterVariant.PrepareTue,
                PrepareWed = masterVariant.PrepareWed,
                PrepareThu = masterVariant.PrepareThu,
                PrepareFri = masterVariant.PrepareFri,
                PrepareSat = masterVariant.PrepareSat,
                PrepareSun = masterVariant.PrepareSun,
                KillerPoint1En = masterVariant.KillerPoint1En,
                KillerPoint2En = masterVariant.KillerPoint2En,
                KillerPoint3En = masterVariant.KillerPoint3En,
                KillerPoint1Th = masterVariant.KillerPoint1Th,
                KillerPoint2Th = masterVariant.KillerPoint2Th,
                KillerPoint3Th = masterVariant.KillerPoint3Th,
                Installment = masterVariant.Installment,
                Length = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length/10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length / 1000) : masterVariant.Length,
                Height = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Height / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Height / 1000) : masterVariant.Height,
                Width = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Width / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Width / 1000) : masterVariant.Width,
                DimensionUnit = masterVariant.DimensionUnit,
                Weight = Constant.WEIGHT_MEASURE_KG.Equals(masterVariant.WeightUnit) ? (masterVariant.Weight / 1000) : masterVariant.Weight,
                WeightUnit = masterVariant.WeightUnit,
                SEO = new SEORequest()
                {
                    GlobalProductBoostingWeight = masterVariant.GlobalBoostWeight,
                    MetaDescriptionEn = masterVariant.MetaDescriptionEn,
                    MetaDescriptionTh = masterVariant.MetaDescriptionTh,
                    MetaKeywordEn = masterVariant.MetaKeyEn,
                    MetaKeywordTh = masterVariant.MetaKeyTh,
                    MetaTitleEn = masterVariant.MetaTitleEn,
                    MetaTitleTh = masterVariant.MetaTitleTh,
                    ProductBoostingWeight = masterVariant.BoostWeight,
                    ProductUrlKeyEn = masterVariant.UrlKey,
                    SeoEn = masterVariant.SeoEn,
                    SeoTh = masterVariant.SeoTh,
                },
                IsHasExpiryDate = masterVariant.IsHasExpiryDate,
                IsVat = masterVariant.IsVat,
                ExpressDelivery = masterVariant.ExpressDelivery,
                DeliveryFee = masterVariant.DeliveryFee,
                PromotionPrice = masterVariant.PromotionPrice,
                EffectiveDatePromotion = masterVariant.EffectiveDatePromotion,
                ExpireDatePromotion = masterVariant.ExpireDatePromotion,
                NewArrivalDate = masterVariant.NewArrivalDate,
                DefaultVariant = masterVariant.DefaultVariant,
                Display = masterVariant.Display,
                MiniQtyAllowed = masterVariant.MiniQtyAllowed,
                MaxiQtyAllowed = masterVariant.MaxiQtyAllowed,
                UnitPrice = masterVariant.UnitPrice,
                PurchasePrice = masterVariant.PurchasePrice,
                IsVariant = masterVariant.IsVariant,
                Visibility = masterVariant.Visibility,
                OnHold = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.OnHold,
                Quantity = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Quantity,
                Defect = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Defect,
                Reserve = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Reserve,
                SafetyStock = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.SafetyStockSeller,
                StockType = masterVariant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : masterVariant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
                MaxQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyAllowInCart,
                MinQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MinQtyAllowInCart,
                MaxQtyPreOrder = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyPreOrder,
            };
            response.NewArrivalDate = masterVariant.NewArrivalDate;
            if (masterVariant.ProductStageAttributes != null)
            {
                foreach (var attribute in masterVariant.ProductStageAttributes)
                {

                    var attr = new AttributeRequest()
                    {
                        AttributeId = attribute.Attribute.AttributeId,
                        DataType = attribute.Attribute.DataType,
                        AttributeNameEn = attribute.Attribute.AttributeNameEn,
                        ValueEn = attribute.ValueEn,
                        ValueTh = attribute.ValueTh,
                    };
                    if (attribute.AttributeValue != null)
                    {
                        attr.ValueEn = string.Empty;
                        attr.ValueTh = string.Empty;
                        attr.AttributeValues.Add(new AttributeValueRequest()
                        {
                            AttributeValueId = attribute.AttributeValueId.HasValue ? attribute.AttributeValueId.Value : 0,
                            AttributeValueEn = attribute.AttributeValue.AttributeValueEn,
                            CheckboxValue = attribute.CheckboxValue,
                            AttributeValueTh = attribute.AttributeValue.AttributeValueTh,
                            Position = attribute.Position,
                        });
                    }
                    response.MasterAttribute.Add(attr);
                }
            }
            if (masterVariant.Images != null)
            {
                foreach (var image in masterVariant.Images)
                {
                    response.MasterVariant.Images.Add(new ImageRequest()
                    {
                        Url = string.Concat(Constant.IMAGE_STATIC_URL,image.ImageName),
                    });
                }
            }
            if (masterVariant.Videos != null)
            {
                foreach (var video in masterVariant.Videos)
                {
                    response.MasterVariant.VideoLinks.Add(new VideoLinkRequest()
                    {
                        Url = video.VideoUrlEn,
                    });
                }
            }
            response.Visibility = masterVariant.Visibility;
            #endregion
            #region variant
            var variants = tmpPro.ProductStages.Where(w => w.IsVariant == true && w.IsMaster == false);
            foreach (var variant in variants)
            {
                var tmpVariant = new VariantRequest()
                {
                    Pid = variant.Pid,
                    ProductId = variant.ProductId,
                    ShopId = variant.ShopId,
                    ProductNameEn = variant.ProductNameEn,
                    ProductNameTh = variant.ProductNameTh,
                    ProdTDNameTh = variant.ProdTDNameTh,
                    ProdTDNameEn = variant.ProdTDNameEn,
                    SaleUnitTh = variant.SaleUnitTh,
                    SaleUnitEn = variant.SaleUnitEn,
                    Sku = variant.Sku,
                    Upc = variant.Upc,
                    OriginalPrice = variant.OriginalPrice,
                    SalePrice = variant.SalePrice,
                    DescriptionFullEn = variant.DescriptionFullEn,
                    DescriptionShortEn = variant.DescriptionShortEn,
                    DescriptionFullTh = variant.DescriptionFullTh,
                    DescriptionShortTh = variant.DescriptionShortTh,
                    MobileDescriptionEn = variant.MobileDescriptionEn,
                    MobileDescriptionTh = variant.MobileDescriptionTh,
                    PrepareDay = variant.PrepareDay,
                    LimitIndividualDay = variant.LimitIndividualDay,
                    PrepareMon = variant.PrepareMon,
                    PrepareTue = variant.PrepareTue,
                    PrepareWed = variant.PrepareWed,
                    PrepareThu = variant.PrepareThu,
                    PrepareFri = variant.PrepareFri,
                    PrepareSat = variant.PrepareSat,
                    PrepareSun = variant.PrepareSun,
                    KillerPoint1En = variant.KillerPoint1En,
                    KillerPoint2En = variant.KillerPoint2En,
                    KillerPoint3En = variant.KillerPoint3En,
                    KillerPoint1Th = variant.KillerPoint1Th,
                    KillerPoint2Th = variant.KillerPoint2Th,
                    KillerPoint3Th = variant.KillerPoint3Th,
                    Installment = variant.Installment,
                    Length = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Length / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Length / 1000) : variant.Length,
                    Height = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Height / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Height / 1000) : variant.Height,
                    Width = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Width / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Width / 1000) : variant.Width,
                    DimensionUnit = variant.DimensionUnit,
                    Weight = Constant.WEIGHT_MEASURE_KG.Equals(variant.WeightUnit) ? (variant.Weight / 1000) : variant.Weight,
                    WeightUnit = variant.WeightUnit,
                    SEO = new SEORequest()
                    {
                        GlobalProductBoostingWeight = variant.GlobalBoostWeight,
                        MetaDescriptionEn = variant.MetaDescriptionEn,
                        MetaDescriptionTh = variant.MetaDescriptionTh,
                        MetaKeywordEn = variant.MetaKeyEn,
                        MetaKeywordTh = variant.MetaKeyTh,
                        MetaTitleEn = variant.MetaTitleEn,
                        MetaTitleTh = variant.MetaTitleTh,
                        ProductBoostingWeight = variant.BoostWeight,
                        ProductUrlKeyEn = variant.UrlKey,
                        SeoEn = variant.SeoEn,
                        SeoTh = variant.SeoTh,
                    },
                    IsHasExpiryDate = variant.IsHasExpiryDate,
                    IsVat = variant.IsVat,
                    ExpressDelivery = variant.ExpressDelivery,
                    DeliveryFee = variant.DeliveryFee,
                    PromotionPrice = variant.PromotionPrice,
                    EffectiveDatePromotion = variant.EffectiveDatePromotion,
                    ExpireDatePromotion = variant.ExpireDatePromotion,
                    NewArrivalDate = variant.NewArrivalDate,
                    DefaultVariant = variant.DefaultVariant,
                    Display = variant.Display,
                    MiniQtyAllowed = variant.MiniQtyAllowed,
                    MaxiQtyAllowed = variant.MaxiQtyAllowed,
                    UnitPrice = variant.UnitPrice,
                    PurchasePrice = variant.PurchasePrice,
                    IsVariant = variant.IsVariant,
                    Visibility = variant.Visibility,
                    OnHold = variant.Inventory == null ? 0 : variant.Inventory.OnHold,
                    Quantity = variant.Inventory == null ? 0 : variant.Inventory.Quantity,
                    Reserve = variant.Inventory == null ? 0 : variant.Inventory.Reserve,
                    Defect = variant.Inventory == null ? 0 : variant.Inventory.Defect,
                    SafetyStock = variant.Inventory == null ? 0 : variant.Inventory.SafetyStockSeller,
                    StockType = variant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : variant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
                    MaxQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyAllowInCart,
                    MinQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MinQtyAllowInCart,
                    MaxQtyPreOrder = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyPreOrder,
                };

                if (variant.ProductStageAttributes != null)
                {
                    foreach (var attribute in variant.ProductStageAttributes)
                    {

                        var attr = new AttributeRequest()
                        {
                            AttributeId = attribute.Attribute.AttributeId,
                            DataType = attribute.Attribute.DataType,
                            AttributeNameEn = attribute.Attribute.AttributeNameEn,
                            ValueEn = attribute.ValueEn,
                            ValueTh = attribute.ValueTh,
                        };
                        if (attribute.AttributeValue != null)
                        {
                            attr.ValueEn = string.Empty;
                            attr.ValueTh = string.Empty;

                            attr.AttributeValues.Add(new AttributeValueRequest()
                            {
                                AttributeValueId = attribute.AttributeValueId.HasValue ? attribute.AttributeValueId.Value : 0,
                                AttributeValueEn = attribute.AttributeValue.AttributeValueEn,
                                CheckboxValue = attribute.CheckboxValue,
                                AttributeValueTh = attribute.AttributeValue.AttributeValueTh,
                                Position = attribute.Position,
                            });
                        }
                        if (tmpVariant.FirstAttribute == null || tmpVariant.FirstAttribute.AttributeId == 0)
                        {
                            tmpVariant.FirstAttribute = attr;
                        }
                        else
                        {
                            tmpVariant.SecondAttribute = attr;
                        }
                    }
                }
                if (variant.Images != null)
                {
                    foreach (var image in variant.Images)
                    {
                        tmpVariant.Images.Add(new ImageRequest()
                        {
                            Url = string.Concat(Constant.IMAGE_STATIC_URL, image.ImageName),
                        });
                    }
                }
                if (variant.Videos != null)
                {
                    foreach (var video in variant.Videos)
                    {
                        tmpVariant.VideoLinks.Add(new VideoLinkRequest()
                        {
                            Url = video.VideoUrlEn,
                        });
                    }
                }
                response.Variants.Add(tmpVariant);
            }
            #endregion
            #region history
            var historyList = db.ProductHistoryGroups
                .Where(w => w.ProductId == response.ProductId)
                .OrderByDescending(o => o.HistoryDt)
                .Select(s => new ProductHistoryRequest()
                {
                    ApproveOn = s.ApproveOn,
                    SubmitBy = s.SubmitBy,
                    HistoryId = s.HistoryId,
                    SubmitOn = s.SubmitOn
                }).Take(Constant.HISTORY_REVISION);
            foreach (var history in historyList)
            {
                response.Revisions.Add(new ProductHistoryRequest()
                {
                    ApproveOn = history.ApproveOn,
                    HistoryId = history.HistoryId,
                    SubmitBy = history.SubmitBy,
                    SubmitOn = history.SubmitOn
                });
            }
            #endregion
            return response;
        }

        private void SetupApprovedProduct(ProductStageGroup group, ColspEntities db)
        {
            if (group == null)
            {
                throw new Exception("Product group cannot be null");
            }
            group.OnlineFlag = true;
            #region History Group
            ProductHistoryGroup historyGroup = new ProductHistoryGroup()
            {
                HistoryDt = DateTime.Now,
                ProductId = group.ProductId,
                ShopId = group.ShopId,
                GlobalCatId = group.GlobalCatId,
                LocalCatId = group.LocalCatId,
                AttributeSetId = group.AttributeSetId,
                BrandId = group.BrandId,
                ShippingId = group.ShippingId,
                EffectiveDate = group.EffectiveDate,
                ExpireDate = group.ExpireDate,
                TheOneCardEarn = group.TheOneCardEarn,
                GiftWrap = group.GiftWrap,
                IsNew = group.IsNew,
                IsClearance = group.IsClearance,
                IsBestSeller = group.IsBestSeller,
                IsOnlineExclusive = group.IsOnlineExclusive,
                IsOnlyAt = group.IsOnlyAt,
                Remark = group.Remark,
                InfoFlag = group.InfoFlag,
                ImageFlag = group.ImageFlag,
                OnlineFlag = group.OnlineFlag,
                InformationTabStatus = group.InformationTabStatus,
                ImageTabStatus = group.ImageTabStatus,
                CategoryTabStatus = group.CategoryTabStatus,
                VariantTabStatus = group.VariantTabStatus,
                MoreOptionTabStatus = group.MoreOptionTabStatus,
                RejectReason = group.RejectReason,
                Status = group.Status,
                FirstApproveBy = group.FirstApproveBy,
                FirstApproveOn = group.FirstApproveOn,
                ApproveBy = group.ApproveBy,
                ApproveOn = group.ApproveOn,
                RejecteBy = group.RejecteBy,
                RejectOn = group.RejectOn,
                SubmitBy = group.SubmitBy,
                SubmitOn = group.SubmitOn,
                CreateBy = group.CreateBy,
                CreateOn = group.CreateOn,
                UpdateBy = group.UpdateBy,
                UpdateOn = group.UpdateOn
            };
            #endregion
            #region History Global Category
            foreach (var category in group.ProductStageGlobalCatMaps)
            {
                historyGroup.ProductHistoryGlobalCatMaps.Add(new ProductHistoryGlobalCatMap()
                {
                    CategoryId = category.CategoryId,
                    CreateBy = category.CreateBy,
                    CreateOn = category.CreateOn,
                    UpdateBy = category.UpdateBy,
                    UpdateOn = category.UpdateOn,
                });
            }
            #endregion
            #region History Local Category
            foreach (var category in group.ProductStageLocalCatMaps)
            {
                historyGroup.ProductHistoryLocalCatMaps.Add(new ProductHistoryLocalCatMap()
                {
                    CategoryId = category.CategoryId,
                    CreateBy = category.CreateBy,
                    CreateOn = category.CreateOn,
                    UpdateBy = category.UpdateBy,
                    UpdateOn = category.UpdateOn
                });
            }
            #endregion
            #region History Tag
            foreach (var tag in group.ProductStageTags)
            {
                historyGroup.ProductHistoryTags.Add(new ProductHistoryTag()
                {
                    Tag = tag.Tag,
                    CreateBy = tag.CreateBy,
                    CreateOn = tag.CreateOn,
                    UpdateBy = tag.UpdateBy,
                    UpdateOn = tag.UpdateOn,
                });
            }
            #endregion
            var parent = group.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault();
            if (parent == null)
            {
                throw new Exception("Cannot get parent product");
            }
            var pids = group.ProductStages.Select(s => s.Pid).ToList();
            var productList = db.Products.Where(w => pids.Contains(w.Pid)).ToList();

            db.ProductGlobalCatMaps.RemoveRange(db.ProductGlobalCatMaps.Where(w => pids.Contains(w.Pid)));
            db.ProductLocalCatMaps.RemoveRange(db.ProductLocalCatMaps.Where(w => pids.Contains(w.Pid)));
            db.ProductTags.RemoveRange(db.ProductTags.Where(w => pids.Contains(w.Pid)));
            db.ProductVideos.RemoveRange(db.ProductVideos.Where(w => pids.Contains(w.Pid)));
            db.ProductAttributes.RemoveRange(db.ProductAttributes.Where(w => pids.Contains(w.Pid)));

            foreach (var stage in group.ProductStages)
            {
                bool isNewProduct = false;
                Product product = null;
                if (productList == null || productList.Count == 0)
                {
                    isNewProduct = true;
                }
                if (!isNewProduct)
                {
                    var currentProduct = productList.Where(w => w.Pid.Equals(stage.Pid)).SingleOrDefault();
                    if (currentProduct != null)
                    {
                        product = currentProduct;
                        productList.Remove(currentProduct);
                    }
                    else
                    {
                        isNewProduct = true;
                    }
                }

                if (isNewProduct)
                {
                    product = new Product();
                }
                #region History
                ProductHistory history = new ProductHistory()
                {
                    ProductId = group.ProductId,
                    Pid = stage.Pid,
                    Sku = stage.Sku,
                    ProductNameEn = stage.ProductNameEn,
                    ProductNameTh = stage.ProductNameTh,
                    DescriptionFullEn = stage.DescriptionFullEn,
                    DescriptionFullTh = stage.DescriptionFullTh,
                    DescriptionShortEn = stage.DescriptionShortEn,
                    DescriptionShortTh = stage.DescriptionShortTh,
                    Length = stage.Length,
                    Height = stage.Height,
                    Weight = stage.Weight,
                    DimensionUnit = stage.DimensionUnit,
                    Width = stage.Width,
                    WeightUnit = stage.WeightUnit,
                    BoostWeight = stage.BoostWeight,
                    DefaultVariant = stage.DefaultVariant,
                    Display = stage.Display,
                    FeatureImgUrl = stage.FeatureImgUrl,
                    GlobalBoostWeight = stage.GlobalBoostWeight,
                    ImageCount = stage.ImageCount,
                    Installment = stage.Installment,
                    IsMaster = stage.IsMaster,
                    IsVariant = stage.IsMaster,
                    KillerPoint1En = stage.KillerPoint1En,
                    KillerPoint1Th = stage.KillerPoint1Th,
                    KillerPoint2En = stage.KillerPoint2En,
                    KillerPoint2Th = stage.KillerPoint2Th,
                    KillerPoint3En = stage.KillerPoint3En,
                    KillerPoint3Th = stage.KillerPoint3Th,
                    LimitIndividualDay = stage.LimitIndividualDay,
                    MaxiQtyAllowed = stage.MaxiQtyAllowed,
                    MetaDescriptionEn = stage.MetaDescriptionEn,
                    MetaDescriptionTh = stage.MetaDescriptionTh,
                    MetaKeyEn = stage.MetaKeyEn,
                    MetaKeyTh = stage.MetaKeyTh,
                    MetaTitleEn = stage.MetaTitleEn,
                    MetaTitleTh = stage.MetaTitleTh,
                    SeoEn = stage.SeoEn,
                    SeoTh = stage.SeoTh,
                    MiniQtyAllowed = stage.MiniQtyAllowed,
                    OriginalPrice = stage.OriginalPrice,
                    PrepareDay = stage.PrepareDay,
                    PrepareFri = stage.PrepareFri,
                    PrepareMon = stage.PrepareMon,
                    PrepareSat = stage.PrepareSat,
                    PrepareSun = stage.PrepareSun,
                    PrepareThu = stage.PrepareThu,
                    PrepareTue = stage.PrepareTue,
                    PrepareWed = stage.PrepareWed,
                    PurchasePrice = stage.PurchasePrice,
                    SalePrice = stage.SalePrice,
                    ShopId = stage.ShopId,
                    UnitPrice = stage.UnitPrice,
                    Upc = stage.Upc,
                    DeliveryFee = stage.DeliveryFee,
                    EffectiveDatePromotion = stage.EffectiveDatePromotion,
                    ExpireDatePromotion = stage.ExpireDatePromotion,
                    ExpressDelivery = stage.ExpressDelivery,
                    IsHasExpiryDate = stage.IsHasExpiryDate,
                    IsSell = stage.IsSell,
                    IsVat = stage.IsVat,
                    JDADept = stage.JDADept,
                    JDASubDept = stage.JDASubDept,
                    MobileDescriptionEn = stage.MobileDescriptionEn,
                    MobileDescriptionTh = stage.MobileDescriptionTh,
                    NewArrivalDate = stage.NewArrivalDate,
                    ProdTDNameEn = stage.ProdTDNameEn,
                    ProdTDNameTh = stage.ProdTDNameTh,
                    PromotionPrice = stage.PromotionPrice,
                    SaleUnitEn = stage.SaleUnitEn,
                    SaleUnitTh = stage.SaleUnitTh,
                    UrlKey = stage.UrlKey,
                    VariantCount = stage.VariantCount,
                    Visibility = stage.Visibility,
                    Status = stage.Status,
                    CreateBy = stage.CreateBy,
                    CreateOn = stage.CreateOn,
                    UpdateBy = stage.UpdateBy,
                    UpdateOn = stage.UpdateOn,
                    Bu = stage.Bu,
                    OldPid = stage.OldPid,
                };
                #endregion
                #region Setup Product
                product.Pid = stage.Pid;
                if (stage.IsVariant)
                {
                    product.ParentPid = parent.Pid;
                }
                else
                {
                    product.ParentPid = null;
                }
                product.ProductId = group.ProductId;
                product.ShippingId = group.ShippingId;
                product.AttributeSetId = group.AttributeSetId;
                product.GlobalCatId = group.GlobalCatId;
                product.LocalCatId = group.LocalCatId;
                product.BrandId = group.BrandId;
                product.Sku = stage.Sku;
                product.ProductNameEn = stage.ProductNameEn;
                product.ProductNameTh = stage.ProductNameTh;
                product.DescriptionFullEn = stage.DescriptionFullEn;
                product.DescriptionFullTh = stage.DescriptionFullTh;
                product.DescriptionShortEn = stage.DescriptionShortEn;
                product.DescriptionShortTh = stage.DescriptionShortTh;
                product.Length = stage.Length;
                product.Height = stage.Height;
                product.Weight = stage.Weight;
                product.DimensionUnit = stage.DimensionUnit;
                product.Width = stage.Width;
                product.WeightUnit = stage.WeightUnit;
                product.BoostWeight = stage.BoostWeight;
                product.DefaultVariant = stage.DefaultVariant;
                product.Display = stage.Display;
                product.FeatureImgUrl = stage.FeatureImgUrl;
                product.GlobalBoostWeight = stage.GlobalBoostWeight;
                product.ImageCount = stage.ImageCount;
                product.Installment = stage.Installment;
                product.IsMaster = stage.IsMaster;
                product.KillerPoint1En = stage.KillerPoint1En;
                product.KillerPoint1Th = stage.KillerPoint1Th;
                product.KillerPoint2En = stage.KillerPoint2En;
                product.KillerPoint2Th = stage.KillerPoint2Th;
                product.KillerPoint3En = stage.KillerPoint3En;
                product.KillerPoint3Th = stage.KillerPoint3Th;
                product.LimitIndividualDay = stage.LimitIndividualDay;
                product.MaxiQtyAllowed = stage.MaxiQtyAllowed;
                product.MetaDescriptionEn = stage.MetaDescriptionEn;
                product.MetaDescriptionTh = stage.MetaDescriptionTh;
                product.MetaKeyEn = stage.MetaKeyEn;
                product.MetaKeyTh = stage.MetaKeyTh;
                product.MetaTitleEn = stage.MetaTitleEn;
                product.MetaTitleTh = stage.MetaTitleTh;
                product.SeoEn = stage.SeoEn;
                product.SeoTh = stage.SeoTh;
                product.MiniQtyAllowed = stage.MiniQtyAllowed;
                product.OriginalPrice = stage.OriginalPrice;
                product.PrepareDay = stage.PrepareDay;
                product.PrepareFri = stage.PrepareFri;
                product.PrepareMon = stage.PrepareMon;
                product.PrepareSat = stage.PrepareSat;
                product.PrepareSun = stage.PrepareSun;
                product.PrepareThu = stage.PrepareThu;
                product.PrepareTue = stage.PrepareTue;
                product.PrepareWed = stage.PrepareWed;
                product.PurchasePrice = stage.PurchasePrice;
                product.SalePrice = stage.SalePrice;
                product.ShopId = stage.ShopId;
                product.UnitPrice = stage.UnitPrice;
                product.Upc = stage.Upc;
                product.UrlKey = stage.UrlKey;
                product.VariantCount = stage.VariantCount;
                product.Visibility = stage.Visibility;
                product.DeliveryFee = stage.DeliveryFee;
                product.EffectiveDatePromotion = stage.EffectiveDatePromotion;
                product.ExpireDatePromotion = stage.ExpireDatePromotion;
                product.ExpressDelivery = stage.ExpressDelivery;
                product.IsHasExpiryDate = stage.IsHasExpiryDate;
                product.IsSell = stage.IsSell;
                product.IsVat = stage.IsVat;
                product.IsVariant = stage.IsVariant;
                product.JDADept = stage.JDADept;
                product.JDASubDept = stage.JDASubDept;
                product.MobileDescriptionEn = stage.MobileDescriptionEn;
                product.MobileDescriptionTh = stage.MobileDescriptionTh;
                product.NewArrivalDate = stage.NewArrivalDate;
                product.ProdTDNameEn = stage.ProdTDNameEn;
                product.ProdTDNameTh = stage.ProdTDNameTh;
                product.PromotionPrice = stage.PromotionPrice;
                product.SaleUnitEn = stage.SaleUnitEn;
                product.SaleUnitTh = stage.SaleUnitTh;
                product.UrlKey = stage.UrlKey;
                product.EffectiveDate = group.EffectiveDate;
                product.ExpireDate = group.ExpireDate;
                product.Remark = group.Remark;
                product.TheOneCardEarn = group.TheOneCardEarn;
                product.GiftWrap = group.GiftWrap;
                product.Status = stage.Status;
                product.CreateBy = stage.CreateBy;
                product.CreateOn = stage.CreateOn;
                product.UpdateBy = stage.UpdateBy;
                product.UpdateOn = stage.UpdateOn;
               
                #endregion
                #region Attribute
                //var attribteList = product.ProductAttributes.ToList();
                foreach (var attribute in stage.ProductStageAttributes)
                {
                    product.ProductAttributes.Add(new ProductAttribute()
                    {
                        AttributeId = attribute.AttributeId,
                        CheckboxValue = attribute.CheckboxValue,
                        IsAttributeValue = attribute.IsAttributeValue,
                        Position = attribute.Position,
                        ValueEn = attribute.ValueEn,
                        ValueTh = attribute.ValueTh,
                        AttributeValueId = attribute.AttributeValueId,
                        CreateBy = attribute.CreateBy,
                        CreateOn = attribute.CreateOn,
                        UpdateBy = attribute.UpdateBy,
                        UpdateOn = attribute.UpdateOn,
                    });
                    history.ProductHistoryAttributes.Add(new ProductHistoryAttribute()
                    {
                        AttributeId = attribute.AttributeId,
                        CheckboxValue = attribute.CheckboxValue,
                        IsAttributeValue = attribute.IsAttributeValue,
                        AttributeValueId = attribute.AttributeValueId,
                        Position = attribute.Position,
                        ValueEn = attribute.ValueEn,
                        ValueTh = attribute.ValueTh,
                        CreateBy = attribute.CreateBy,
                        CreateOn = attribute.CreateOn,
                        UpdateBy = attribute.UpdateBy,
                        UpdateOn = attribute.UpdateOn,
                    });
                    //bool isNewAttribute = false;
                    //if (attribteList == null || attribteList.Count == 0)
                    //{
                    //    isNewAttribute = false;
                    //}
                    //if (!isNewAttribute)
                    //{
                    //    var currentAttribute = attribteList
                    //        .Where(w => w.Pid.Equals(stage.Pid) && w.AttributeId == attribute.AttributeId && w.ValueEn.Equals(attribute.ValueEn)).SingleOrDefault();
                    //    if (currentAttribute != null)
                    //    {
                    //        attribteList.Remove(currentAttribute);
                    //    }
                    //    else
                    //    {
                    //        isNewAttribute = true;
                    //    }
                    //}
                    //if (isNewAttribute)
                    //{
                    //    product.ProductAttributes.Add(new ProductAttribute()
                    //    {
                    //        AttributeId = attribute.AttributeId,
                    //        CheckboxValue = attribute.CheckboxValue,
                    //        IsAttributeValue = attribute.IsAttributeValue,
                    //        Position = attribute.Position,
                    //        ValueEn = attribute.ValueEn,
                    //        CreateBy = attribute.CreateBy,
                    //        CreateOn = attribute.CreateOn,
                    //        UpdateBy = attribute.UpdateBy,
                    //        UpdateOn = attribute.UpdateOn,
                    //    });
                    //}
                    //history.ProductHistoryAttributes.Add(new ProductHistoryAttribute()
                    //{
                    //    AttributeId = attribute.AttributeId,
                    //    CheckboxValue = attribute.CheckboxValue,
                    //    IsAttributeValue = attribute.IsAttributeValue,
                    //    Position = attribute.Position,
                    //    ValueEn = attribute.ValueEn,
                    //    CreateBy = attribute.CreateBy,
                    //    CreateOn = attribute.CreateOn,
                    //    UpdateBy = attribute.UpdateBy,
                    //    UpdateOn = attribute.UpdateOn,
                    //});

                }
                //if (attribteList != null && attribteList.Count > 0)
                //{
                //    db.ProductAttributes.RemoveRange(attribteList);
                //}
                #endregion
                #region Related Global Category
                var globalCatList = product.ProductGlobalCatMaps.ToList();
                foreach (var category in group.ProductStageGlobalCatMaps)
                {
                    product.ProductGlobalCatMaps.Add(new ProductGlobalCatMap()
                    {
                        CategoryId = category.CategoryId,
                        CreateBy = category.CreateBy,
                        CreateOn = category.CreateOn,
                        UpdateBy = category.UpdateBy,
                        UpdateOn = category.UpdateOn
                    });

                    //bool isNewGlobalCat = false;
                    //if (globalCatList == null || globalCatList.Count == 0)
                    //{
                    //    isNewGlobalCat = true;
                    //}
                    //if (!isNewGlobalCat)
                    //{
                    //    var currentGlobalCat = globalCatList.Where(w => w.CategoryId == category.CategoryId).SingleOrDefault();
                    //    if (currentGlobalCat != null)
                    //    {
                    //        globalCatList.Remove(currentGlobalCat);
                    //    }
                    //    else
                    //    {
                    //        isNewGlobalCat = true;
                    //    }
                    //}
                    //if (isNewGlobalCat)
                    //{
                    //    product.ProductGlobalCatMaps.Add(new ProductGlobalCatMap()
                    //    {
                    //        CategoryId = category.CategoryId,
                    //        CreateBy = category.CreateBy,
                    //        CreateOn = category.CreateOn,
                    //        UpdateBy = category.UpdateBy,
                    //        UpdateOn = category.UpdateOn
                    //    });
                    //}
                }
                if (globalCatList != null && globalCatList.Count > 0)
                {
                    db.ProductGlobalCatMaps.RemoveRange(globalCatList);
                }

                #endregion
                #region Related Local Category
                var localCatList = product.ProductLocalCatMaps.ToList();
                foreach (var category in group.ProductStageLocalCatMaps)
                {

                    product.ProductLocalCatMaps.Add(new ProductLocalCatMap()
                    {
                        CategoryId = category.CategoryId,
                        CreateBy = category.CreateBy,
                        CreateOn = category.CreateOn,
                        UpdateBy = category.UpdateBy,
                        UpdateOn = category.UpdateOn
                    });

                    //bool isNewLocalCat = false;
                    //if (localCatList == null || localCatList.Count == 0)
                    //{
                    //    isNewLocalCat = true;
                    //}
                    //if (!isNewLocalCat)
                    //{
                    //    var currentLocalCat = localCatList.Where(w => w.CategoryId == category.CategoryId).SingleOrDefault();
                    //    if (currentLocalCat != null)
                    //    {
                    //        localCatList.Remove(currentLocalCat);
                    //    }
                    //    else
                    //    {
                    //        isNewLocalCat = true;
                    //    }
                    //}
                    //if (isNewLocalCat)
                    //{
                    //    product.ProductLocalCatMaps.Add(new ProductLocalCatMap()
                    //    {
                    //        CategoryId = category.CategoryId,
                    //        CreateBy = category.CreateBy,
                    //        CreateOn = category.CreateOn,
                    //        UpdateBy = category.UpdateBy,
                    //        UpdateOn = category.UpdateOn
                    //    });
                    //}
                }
                if (localCatList != null && localCatList.Count > 0)
                {
                    db.ProductLocalCatMaps.RemoveRange(localCatList);
                }
                #endregion
                #region Video
                var videoList = product.ProductVideos.ToList();
                foreach (var video in stage.ProductStageVideos)
                {

                    product.ProductVideos.Add(new ProductVideo()
                    {
                        VideoId = 0,
                        VideoUrlEn = video.VideoUrlEn,
                        Position = video.Position,
                        Status = video.Status,
                        CreateBy = video.CreateBy,
                        CreateOn = video.CreateOn,
                        UpdateBy = video.UpdateBy,
                        UpdateOn = video.UpdateOn
                    });

                    history.ProductHistoryVideos.Add(new ProductHistoryVideo()
                    {
                        VideoUrlEn = video.VideoUrlEn,
                        Position = video.Position,
                        Status = video.Status,
                        CreateBy = video.CreateBy,
                        CreateOn = video.CreateOn,
                        UpdateBy = video.UpdateBy,
                        UpdateOn = video.UpdateOn
                    });

                    //bool isNewVideo = false;
                    //if (videoList == null || videoList.Count == 0)
                    //{
                    //    isNewVideo = true;
                    //}
                    //if (!isNewVideo)
                    //{
                    //    var currentVideo = videoList.Where(w => w.VideoUrlEn.Equals(video.VideoUrlEn)).SingleOrDefault();
                    //    if (currentVideo != null)
                    //    {
                    //        videoList.Remove(currentVideo);
                    //    }
                    //    else
                    //    {
                    //        isNewVideo = true;
                    //    }
                    //}
                    //if (isNewVideo)
                    //{
                    //    product.ProductVideos.Add(new ProductVideo()
                    //    {
                    //        VideoId = 0,
                    //        VideoUrlEn = video.VideoUrlEn,
                    //        Position = video.Position,
                    //        Status = video.Status,
                    //        CreateBy = video.CreateBy,
                    //        CreateOn = video.CreateOn,
                    //        UpdateBy = video.UpdateBy,
                    //        UpdateOn = video.UpdateOn
                    //    });
                    //}
                    //history.ProductHistoryVideos.Add(new ProductHistoryVideo()
                    //{
                    //    VideoUrlEn = video.VideoUrlEn,
                    //    Position = video.Position,
                    //    Status = video.Status,
                    //    CreateBy = video.CreateBy,
                    //    CreateOn = video.CreateOn,
                    //    UpdateBy = video.UpdateBy,
                    //    UpdateOn = video.UpdateOn
                    //});
                }
                if (videoList != null && videoList.Count > 0)
                {
                    db.ProductVideos.RemoveRange(videoList);
                }
                #endregion
                #region Tag
                var tagList = product.ProductTags.ToList();
                foreach (var tag in group.ProductStageTags)
                {
                    product.ProductTags.Add(new ProductTag()
                    {
                        Tag = tag.Tag,
                        CreateBy = tag.CreateBy,
                        CreateOn = tag.CreateOn,
                        UpdateBy = tag.UpdateBy,
                        UpdateOn = tag.UpdateOn
                    });

                    //bool isNewTag = false;
                    //if (tagList == null || tagList.Count == 0)
                    //{
                    //    isNewTag = true;
                    //}
                    //if (!isNewTag)
                    //{
                    //    var currentTag = tagList.Where(w => w.Tag.Equals(tag.Tag)).SingleOrDefault();
                    //    if (currentTag != null)
                    //    {
                    //        tagList.Remove(currentTag);
                    //    }
                    //    else
                    //    {
                    //        isNewTag = true;
                    //    }
                    //}
                    //if (isNewTag)
                    //{
                    //    product.ProductTags.Add(new ProductTag()
                    //    {
                    //        Tag = tag.Tag,
                    //        CreateBy = tag.CreateBy,
                    //        CreateOn = tag.CreateOn,
                    //        UpdateBy = tag.UpdateBy,
                    //        UpdateOn = tag.UpdateOn
                    //    });
                    //}
                }
                if (tagList != null && tagList.Count > 0)
                {
                    db.ProductTags.RemoveRange(tagList);
                }
                #endregion
                #region Related Product
                //if (!parent.Equals(stage.Pid))
                //{
                //    db.ProductRelateds.Add(new ProductRelated()
                //    {
                //        ParentPid = parent.Pid,
                //        ChildPid =  stage.Pid,
                //        CreateBy = parent.CreateBy,
                //        CreateOn = parent.CreateOn,
                //        UpdateBy = parent.UpdateBy,
                //        UpdateOn = parent.UpdateOn
                //    });
                //}
                #endregion
                #region Image
                foreach (var image in stage.ProductStageImages)
                {
                    history.ProductHistoryImages.Add(new ProductHistoryImage()
                    {
                        FeatureFlag = image.FeatureFlag,
                        ImageName = image.ImageName,
                        Large = image.Large,
                        Normal = image.Normal,
                        Thumbnail = image.Thumbnail,
                        Zoom = image.Zoom,
                        Pid = image.Pid,
                        SeqNo = image.SeqNo,
                        ShopId = image.ShopId,
                        Status = image.Status,
                        CreateBy = image.CreateBy,
                        CreateOn = image.CreateOn,
                        UpdateBy = image.UpdateBy,
                        UpdateOn = image.UpdateOn,
                    });
                }
                #endregion
                historyGroup.ProductHistories.Add(history);
                if (isNewProduct)
                {
                    db.Products.Add(product);
                }
            }
            if (productList != null && productList.Count > 0)
            {
                productList.ForEach(e => e.Status = Constant.STATUS_REMOVE);
            }
            historyGroup.HistoryId = db.GetNextProductHistoryId().SingleOrDefault().Value;
            db.ProductHistoryGroups.Add(historyGroup);
        }

        private void SetupGroupAfterSave(ProductStageGroup groupProduct, ColspEntities db, bool isNew = false)
        {
            //var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
            //var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;

            HashSet<string> tmpImageHash = new HashSet<string>();
            foreach (var stage in groupProduct.ProductStages)
            {
                int index = 0;
                foreach (var image in stage.ProductStageImages)
                {
                    if (tmpImageHash.Contains(image.ImageName))
                    {
                        string lastPart = image.ImageName;
                        string oldFile = Path.Combine(
                            AppSettingKey.IMAGE_ROOT_PATH, 
                            AppSettingKey.PRODUCT_FOLDER, 
                            AppSettingKey.ORIGINAL_FOLDER, 
                            lastPart);
                        if (File.Exists(oldFile))
                        {
                            string newFileName = string.Concat(stage.Pid, "_", index, Path.GetExtension(lastPart));
                            image.ImageName = newFileName;
                            File.Copy(oldFile, Path.Combine(
                                AppSettingKey.IMAGE_ROOT_PATH, 
                                AppSettingKey.PRODUCT_FOLDER, 
                                AppSettingKey.ORIGINAL_FOLDER,
                                newFileName));
                            ++index;
                        }
                    }
                    else
                    {
                        tmpImageHash.Add(image.ImageName);
                    }
                }
            }
            foreach (var stage in groupProduct.ProductStages)
            {
                SetupStageAfterSave(stage, db, isNew);
            }
        }

        private void SetupStageAfterSave(ProductStage stage, ColspEntities db = null, bool isNew = false)
        {
            #region Image
            foreach (var image in stage.ProductStageImages)
            {
                string lastPart = image.ImageName;
                string newFile = Path.Combine(
                    AppSettingKey.IMAGE_ROOT_PATH, 
                    AppSettingKey.PRODUCT_FOLDER,
                    AppSettingKey.ORIGINAL_FOLDER,
                    string.Concat(stage.Pid, "_", image.SeqNo, Path.GetExtension(lastPart)));
                string oldFile = Path.Combine(
                    AppSettingKey.IMAGE_ROOT_PATH, 
                    AppSettingKey.PRODUCT_FOLDER, 
                    AppSettingKey.ORIGINAL_FOLDER,
                    lastPart);
                if (File.Exists(oldFile))
                {
                    if (File.Exists(newFile))
                    {
                        continue;
                    }
                    File.Move(oldFile, newFile);
                    image.ImageName = Path.GetFileName(newFile);
                    image.ImageId = db.GetNextProductStageImageId().SingleOrDefault().Value;
                    if (image.FeatureFlag)
                    {
                        stage.FeatureImgUrl = image.ImageName;
                    }
                }
            }
            #endregion
            #region Inventory History
            if (isNew)
            {
                InventoryHistory history = new InventoryHistory()
                {
                    Pid = stage.Pid,
                    MaxQtyAllowInCart = stage.Inventory.MaxQtyAllowInCart,
                    MaxQtyPreOrder = stage.Inventory.MaxQtyPreOrder,
                    MinQtyAllowInCart = stage.Inventory.MinQtyAllowInCart,
                    StockType = stage.Inventory.StockType,
                    UseDecimal = stage.Inventory.UseDecimal,
                    Defect = stage.Inventory.Defect,
                    OnHold = stage.Inventory.OnHold,
                    Quantity = stage.Inventory.Quantity,
                    Reserve = stage.Inventory.Reserve,
                    SafetyStockAdmin = stage.Inventory.SafetyStockAdmin,
                    SafetyStockSeller = stage.Inventory.SafetyStockSeller,
                    Status = Constant.INVENTORY_STATUS_ADD,
                    CreateBy = User.UserRequest().Email,
                    CreateOn = DateTime.Now,
                    UpdateBy = User.UserRequest().Email,
                    UpdateOn = DateTime.Now,
                };
                db.InventoryHistories.Add(history);
            }
            #endregion
        }






        [Route("api/ProductStages/Approve")]
        [HttpPut]
        public HttpResponseMessage ApproveProduct(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var productIds = request.Select(s=>s.ProductId).ToList();
                if(productIds.Count > Constant.BULK_APPROVE_LIMIT)
                {
                    throw new Exception(string.Concat("Cannot bulk appove more than ", Constant.BULK_APPROVE_LIMIT," products"));
                }
                var groupList = db.ProductStageGroups.Where(w => productIds.Contains(w.ProductId))
                    .Include(i=>i.ProductStages.Select(s=>s.ProductStageAttributes))
                    .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
                    .Include(i=>i.ProductStageTags)
                    .Include(i=>i.ProductStageGlobalCatMaps)
                    .Include(i=>i.ProductStageLocalCatMaps);
                var email = User.UserRequest().Email;
                var currenDt = DateTime.Now;
                foreach (ProductStageRequest proRq in request)
                {
                    var pro = groupList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
                    if (pro == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }
                    pro.Status = Constant.PRODUCT_STATUS_APPROVE;
                    pro.ProductStages.ToList().ForEach(e => e.Status = Constant.PRODUCT_STATUS_APPROVE);
                    pro.OnlineFlag = true;
                    pro.ApproveBy = email;
                    pro.ApproveOn = currenDt;
                    if (string.IsNullOrEmpty(pro.FirstApproveBy))
                    {
                        pro.FirstApproveBy = email;
                        pro.FirstApproveOn = currenDt;
                    }
                    SetupApprovedProduct(pro, db);
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        [Route("api/ProductStages/Reject")]
        [HttpPut]
        public HttpResponseMessage RejectProduct(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var productIds = request.Select(s => s.ProductId).ToList();
                if (productIds.Count > Constant.BULK_APPROVE_LIMIT)
                {
                    throw new Exception(string.Concat("Cannot bulk appove more than ", Constant.BULK_APPROVE_LIMIT, " products"));
                }
                var groupList = db.ProductStageGroups.Where(w => productIds.Contains(w.ProductId))
                    .Include(i => i.ProductStages);
                var email = User.UserRequest().Email;
                var currenDt = DateTime.Now;
                foreach (ProductStageRequest proRq in request)
                {
                    var pro = groupList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
                    if (pro == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }
                    pro.Status = Constant.PRODUCT_STATUS_NOT_APPROVE;
                    pro.RejecteBy = email;
                    pro.RejectOn = currenDt;
                    pro.ProductStages.ToList().ForEach(e => e.Status = Constant.PRODUCT_STATUS_NOT_APPROVE);
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }




        //[Route("api/Test/ProductStages")]
        //[HttpPost]
        //public HttpResponseMessage AddProduct(ProductStageRequest request)
        //{
        //    try
        //    {
        //        #region Validation
        //        if (request == null)
        //        {
        //            throw new Exception("Invalid request");
        //        }
        //        if (User.ShopRequest() == null)
        //        {
        //            throw new Exception("Shop is required");
        //        }
        //        #endregion
        //        var shopId = User.ShopRequest().ShopId;
        //        ProductStageGroup group = SetupProduct(db, request,shopId);
        //        AutoGenerate.GeneratePid(db, group.ProductStages);
        //        group.ProductId = db.GetNextProductStageGroupId().Single().Value;
        //        db.ProductStageGroups.Add(group);
        //        Util.DeadlockRetry(db.SaveChanges, "ProductStage");
        //        SetupGroupAfterSave(group,db,true);
        //        Util.DeadlockRetry(db.SaveChanges, "ProductStageImage");
        //        return GetProductStage(group.ProductId) ;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
        //    }
        //}



        //duplicate


        [Route("api/ProductStages/{productId}")]
        [HttpPost]
        public HttpResponseMessage DuplicateProductStage(long productId)
        {
            try
            {
                if (User.ShopRequest() == null)
                {
                    throw new Exception("Shop is required");
                }
                var response = GetProductStageRequestFromId(db, productId);
                if (response == null)
                {
                    throw new Exception("Cannot find product with id " + productId);
                }
                if (response.MasterVariant != null)
                {
                    response.MasterVariant.SEO.ProductUrlKeyEn = null;
                    response.MasterVariant.Pid = null;
                    response.MasterVariant.ProductNameEn = string.Concat("Copy of ", response.MasterVariant.ProductNameEn);
                    if (response.MasterVariant.Images != null)
                    {
                        response.MasterVariant.Images.Clear();
                    }
                    response.Status = Constant.PRODUCT_STATUS_DRAFT;
                }
                if (response.Variants != null)
                {
                    response.Variants.Where(w => w.SEO != null).ToList().ForEach(f => { f.SEO.ProductUrlKeyEn = null; f.Pid = null; f.Images.Clear(); f.Status = Constant.PRODUCT_STATUS_DRAFT; });
                }
                return AddProduct(response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages")]
        [HttpDelete]
        public HttpResponseMessage DeleteProduct(List<ProductStageRequest> request)
        {
            try
            {
                int shopId = User.ShopRequest().ShopId;
                string email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                var ids = request.Select(s => s.ProductId).ToList();
                var producGrouptList = db.ProductStageGroups.Where(w => w.ShopId == shopId && ids.Contains(w.ProductId)).Select(s => new
                {
                    s.ProductId,
                    ProductStages = s.ProductStages.Select(st => new
                    {
                        st.Pid,
                        st.Inventory
                    }),
                });
                var stagePids = producGrouptList.SelectMany(s => s.ProductStages.Select(st => st.Pid));
                var productProd = db.Products.Where(w => stagePids.Contains(w.Pid)).Select(s=>s.Pid);


                //var producGrouptList = db.ProductStageGroups.Where(w => w.ShopId == shopId && ids.Contains(w.ProductId)).Include(i=>i.ProductStages.Select(s=>s.Inventory));
                foreach (ProductStageRequest proRq in request)
                {
                    var productGroup = producGrouptList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
                    if (productGroup == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }
                    var pids = productGroup.ProductStages.Select(s => s.Pid).ToList();
                    var inventories = productGroup.ProductStages.Select(s => s.Inventory).ToList();
                    foreach(var inventory in inventories)
                    {
                        ProductStage stage = new ProductStage()
                        {
                            Pid = inventory.Pid
                        };
                        db.ProductStages.Attach(stage);
                        db.Entry(stage).Property(p => p.Status).IsModified = true;
                        db.Entry(stage).Property(p => p.UpdateBy).IsModified = true;
                        db.Entry(stage).Property(p => p.UpdateOn).IsModified = true;
                        stage.Status = Constant.STATUS_REMOVE;
                        stage.UpdateBy = email;
                        stage.UpdateOn = currentDt;

                        if (productProd.Contains(inventory.Pid))
                        {
                            Product product = new Product()
                            {
                                Pid = inventory.Pid
                            };
                            db.Products.Attach(product);
                            db.Entry(product).Property(p => p.Status).IsModified = true;
                            db.Entry(product).Property(p => p.UpdateBy).IsModified = true;
                            db.Entry(product).Property(p => p.UpdateOn).IsModified = true;
                            product.Status = Constant.STATUS_REMOVE;
                            product.UpdateBy = email;
                            product.UpdateOn = currentDt;
                        }

                        db.InventoryHistories.Add(new InventoryHistory()
                        {
                            Pid = inventory.Pid,
                            StockType = inventory.StockType,
                            MaxQtyAllowInCart = inventory.MaxQtyAllowInCart,
                            MinQtyAllowInCart = inventory.MinQtyAllowInCart,
                            UseDecimal = inventory.UseDecimal,
                            MaxQtyPreOrder = inventory.MaxQtyPreOrder,
                            Defect = inventory.Defect,
                            OnHold = inventory.OnHold,
                            Quantity = inventory.Quantity,
                            Reserve = inventory.Reserve,
                            SafetyStockAdmin = inventory.SafetyStockAdmin,
                            SafetyStockSeller = inventory.SafetyStockSeller,
                            Status = Constant.INVENTORY_STATUS_DELETE,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                    }
                    ProductStageGroup group = new ProductStageGroup()
                    {
                        ProductId = productGroup.ProductId
                    };
                    db.ProductStageGroups.Attach(group);
                    db.Entry(group).Property(p => p.Status).IsModified = true;
                    db.Entry(group).Property(p => p.UpdateBy).IsModified = true;
                    db.Entry(group).Property(p => p.UpdateOn).IsModified = true;
                    group.Status = Constant.STATUS_REMOVE;
                    group.UpdateBy = email;
                    group.UpdateOn = currentDt;
                }
                db.Configuration.ValidateOnSaveEnabled = false;
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Tags")]
        [HttpPut]
        public HttpResponseMessage AppendTage(TagRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var productIds = request.Products.Select(s => s.ProductId).ToList();
                var productList = db.ProductStageGroups.Where(w => true).Include(i => i.ProductStageTags);
                productList = productList.Where(w => productIds.Any(a => a == w.ProductId));
                //bool isAdmin = true;
                if (User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    productList = productList.Where(w => w.ShopId == shopId);
                    //isAdmin = false;
                }
                foreach (var product in productList)
                {
                    //if (!isAdmin && Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(product.Status))
                    if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(product.Status))
                    {
                        throw new Exception("Cannot add tag to Wait for Approval products");
                    }
                    var tagList = request.Tags;
                    var tmpTagList = product.ProductStageTags.Select(s=>s.Tag).ToList();
                    tagList.AddRange(tmpTagList);
                    tagList = tagList.Distinct().ToList();
                    tagList = tagList.Where(w=> !tmpTagList.Contains(w)).ToList();
                    foreach(var tag in tagList)
                    {
                        product.ProductStageTags.Add(new ProductStageTag()
                        {
                            Tag = tag,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                    }
                    product.Status = Constant.PRODUCT_STATUS_DRAFT;
                    //if (isAdmin)
                    //{
                    //    product.Status = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                    //}
                    //else
                    //{
                    //    product.Status = Constant.PRODUCT_STATUS_DRAFT;
                    //}
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                var fileUpload = await Util.SetupImage(Request
                    , AppSettingKey.IMAGE_ROOT_PATH
                    , Path.Combine(AppSettingKey.PRODUCT_FOLDER, AppSettingKey.ORIGINAL_FOLDER)
                    , 1500, 1500, 2000, 2000, 5, true);
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/IgnoreApprove")]
        [HttpGet]
        public HttpResponseMessage GetIgnoreApprove()
        {
            var ignoreList = new List<string>()
            {
                "MinimumAllowedInCart",
                "MaximumAllowedInCart",
                "SalePrice",
                "Quantity",
                "PromotionPrice",
                "PromotionEffectiveDate",
                "PromotionExpireDate",
                "UnitPrice",
                "PurchasePrice",
                "SaleUnitTh",
                "SaleUnitEn",
                "SafetyStock",

            };
            return Request.CreateResponse(HttpStatusCode.OK, ignoreList);
        }

        private ProductStageGroup SetupProduct(ColspEntities db, ProductStageRequest request, int shopId)
        {
            bool adminPermission = User.HasPermission("Approve product");
            bool sellerPermission = User.HasPermission("Add Product");
            ProductStageGroup group = null;
            var attributeList = db.Attributes.Include(i => i.AttributeValueMaps).ToList();
            var shippingList = db.Shippings.ToList();
            string email = User.UserRequest().Email;
            DateTime currentDt = DateTime.Now;
            #region Setup Group
            group = new ProductStageGroup();
            group.ShopId = shopId;
            SetupGroup(group, request, true, email, currentDt, adminPermission, sellerPermission, db, shippingList);
            #endregion
            #region Master Variant
            var masterVariant = new ProductStage();
            request.MasterVariant.Status = group.Status;
            masterVariant.ShopId = shopId;
            masterVariant.IsVariant = false;
            SetupVariant(masterVariant, request.MasterVariant, true, email, currentDt, adminPermission, sellerPermission, db);
            SetupAttribute(masterVariant, request.MasterAttribute, attributeList, email, db);
            masterVariant.Visibility = true;
            group.ProductStages.Add(masterVariant);
            #endregion
            #region Variants
            if (request.Variants != null && request.Variants.Count > 0)
            {
                foreach (var variantRq in request.Variants)
                {
                    var variant = new ProductStage();
                    variant.ShopId = shopId;
                    variant.IsVariant = true;
                    variant.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL });
                    SetupVariant(variant, variantRq, true, email, currentDt, adminPermission, sellerPermission, db);
                    SetupAttribute(variant, new List<AttributeRequest>() { variantRq.FirstAttribute, variantRq.SecondAttribute }, attributeList, email, db);
                    group.ProductStages.Add(variant);
                }
                masterVariant.VariantCount = group.ProductStages.Where(w=> w.IsVariant==true && w.Visibility == true).ToList().Count;
                masterVariant.IsSell = false;
            }
            else
            {
                masterVariant.VariantCount = 0;
                masterVariant.IsSell = true;
            }
            #endregion
            #region Check Flag
            var defaultAttribute = attributeList.Where(w => w.DefaultAttribute == true && w.Required == true).ToList();
            if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
                && !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
                && group.BrandId != null
                && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullEn)
                && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullTh))
            {
                group.InfoFlag = true;
            }
            else
            {
                group.InfoFlag = false;
            }
            if (!string.IsNullOrEmpty(masterVariant.FeatureImgUrl))
            {
                group.ImageFlag = true;
            }
            else
            {
                group.ImageFlag = false;
            }
            #endregion
            return group;
        }

        private void SetupAttributeResponse(ProductStage variant, List<AttributeRequest> attributeList)
        {
            foreach (var attribute in variant.ProductStageAttributes)
            {
                if(attributeList.Where(w=>w.AttributeId==attribute.AttributeId).SingleOrDefault() != null)
                {
                    continue;
                }
                AttributeRequest tmpAttribute = new AttributeRequest();
                tmpAttribute.AttributeId = attribute.AttributeId;
                tmpAttribute.DataType = attribute.Attribute.DataType;
                if (Constant.DATA_TYPE_LIST.Equals(attribute.Attribute.DataType))
                {
                    var tmpValue = attribute.Attribute
                        .AttributeValueMaps
                        .Select(s => s.AttributeValue)
                        .Where(w => w.MapValue.Equals(attribute.ValueEn))
                        .FirstOrDefault();
                    if (tmpValue != null)
                    {
                        tmpAttribute.AttributeValues.Add(new AttributeValueRequest()
                        {
                            AttributeValueId = tmpValue.AttributeValueId,
                            AttributeValueEn = tmpValue.AttributeValueEn,
                            CheckboxValue = attribute.CheckboxValue,
                        });
                    }
                }else if (Constant.DATA_TYPE_CHECKBOX.Equals(attribute.Attribute.DataType))
                {
                    var tmpAttributeList = variant.ProductStageAttributes.Where(w => w.AttributeId == attribute.AttributeId).ToList();
                    foreach(var attr in tmpAttributeList)
                    {
                        var tmpValue = attribute.Attribute
                       .AttributeValueMaps
                       .Select(s => s.AttributeValue)
                       .Where(w => w.MapValue.Equals(attr.ValueEn)).ToList();
                        if (tmpValue != null && tmpValue.Count > 0)
                        {
                            foreach (var val in tmpValue)
                            {
                                tmpAttribute.AttributeValues.Add(new AttributeValueRequest()
                                {
                                    AttributeValueId = val.AttributeValueId,
                                    AttributeValueEn = val.AttributeValueEn,
                                    CheckboxValue = attr.CheckboxValue,
                                });
                            }
                        }
                    }
                   
                }
                else
                {
                    tmpAttribute.ValueEn = attribute.ValueEn;
                    tmpAttribute.ValueTh = attribute.ValueTh;
                }
                attributeList.Add(tmpAttribute);
            }
        }

        

        //private ProductStageRequest GetProductStageRequestFromId(ColspEntities db, long productId)
        //{
        //    var tmpProduct = db.ProductStageGroups.Where(w =>w.ProductId == productId)
        //            .Include(i=>i.ProductStageTags)
        //            .Include(i=>i.ProductStageRelateds1.Select(s=>s.ProductStageGroup.ProductStages))
        //            .Include(i=>i.Brand)
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageAttributes.Select(sa => sa.Attribute.AttributeValueMaps.Select(sv => sv.AttributeValue))))
        //            .Include(i => i.ProductStages.Select(s => s.Inventory))
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
        //            .Include(i => i.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory))
        //            .Include(i => i.ProductStageLocalCatMaps.Select(s => s.LocalCategory));
        //    ProductStageGroup product = null;
        //    if (User.ShopRequest() != null)
        //    {
        //        var shopId = User.ShopRequest().ShopId;
        //        product = tmpProduct.Where(w => w.ShopId == shopId).SingleOrDefault();
        //    }
        //    else
        //    {
        //        product = tmpProduct.SingleOrDefault();
        //    }
        //    if (product == null)
        //    {
        //        throw new Exception("Cannot find product " + productId);
        //    }

        //    var historyList = db.ProductHistoryGroups
        //            .Where(w => w.ProductId == product.ProductId)
        //            .OrderByDescending(o => o.HistoryDt)
        //            .Select(s => new ProductHistoryRequest()
        //            {
        //                ApproveOn = s.ApproveOn,
        //                SubmitBy = s.SubmitBy,
        //                HistoryId = s.HistoryId,
        //                SubmitOn = s.SubmitOn
        //            }).ToList();

        //    //var historyList = db.ProductHistoryGroups
        //    //    .Where(w => w.ProductId == product.ProductId)
        //    //    .OrderByDescending(o => o.HistoryDt)
        //    //    .Take(Constant.HISTORY_REVISION).ToList();
        //    ProductStageRequest response = new ProductStageRequest();
        //    SetupResponse(product, response, historyList);
        //    return response;
        //}

        private void SetupResponse(ProductStageGroup group, ProductStageRequest response,List<ProductHistoryRequest> historyList)
        {

            SetupGroupResponse(group, response);
            var masterVariant = group.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault();
            SetupVariantResponse(masterVariant, response.MasterVariant);
            SetupAttributeResponse(masterVariant, response.MasterAttribute);
            response.Visibility = masterVariant.Visibility;
            var variants = group.ProductStages.Where(w => w.IsVariant == true).ToList();
            foreach (var variant in variants)
            {
                var tmpVariant = new VariantRequest();
                SetupVariantResponse(variant, tmpVariant);
                List<AttributeRequest> tmpAttributeList = new List<AttributeRequest>();
                SetupAttributeResponse(variant, tmpAttributeList);
                if (tmpAttributeList != null && tmpAttributeList.Count > 0)
                {
                    tmpVariant.FirstAttribute = tmpAttributeList.ElementAt(0);
                    if (tmpAttributeList.Count > 1)
                    {
                        tmpVariant.SecondAttribute = tmpAttributeList.ElementAt(1);
                    }
                }
                response.Variants.Add(tmpVariant);
            }
            if(historyList != null)
            {
                response.Revisions = historyList;
                //foreach (var history in historyList)
                //{
                //    response.Revisions.Add(new ProductHistoryRequest()
                //    {
                //        ApproveOn = history.ApproveOn,
                //        HistoryId = history.HistoryId,
                //        SubmitBy = history.SubmitBy,
                //        SubmitOn = history.SubmitOn
                //    });
                //}
            }
            
        }

        private void SetupGroup(ProductStageGroup group, ProductStageRequest request,bool addNew,string email, DateTime currentDt, bool adminPermission,bool sellerPermission, ColspEntities db, List<Shipping> shippingList)
        {
            #region Status
            if (adminPermission)
            {
                if (Constant.PRODUCT_STATUS_DRAFT.Equals(request.Status))
                {
                    throw new Exception("Admin cannot make draft product. Status of product edited by admin will be Wait for Approval.");
                }
                group.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() { Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            }
            else if (sellerPermission)
            {
                if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(group.Status))
                {
                    throw new Exception("Cannot edit Wait for Approval product");
                }
                group.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL });
            }
            else
            {
                group.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
                //throw new Exception("Has no permission");
            }
            #endregion
            #region Category
            if (request.MainGlobalCategory == null || request.MainGlobalCategory.CategoryId == 0)
            {
                throw new Exception("Global category is required");
            }
            group.GlobalCatId = request.MainGlobalCategory.CategoryId;
            if (request.MainLocalCategory != null && request.MainLocalCategory.CategoryId != 0)
            {
                group.LocalCatId = request.MainLocalCategory.CategoryId;
            }
            #endregion
            #region Brand
            if (request.Brand != null && request.Brand.BrandId != 0)
            {
                group.BrandId = request.Brand.BrandId;
            }
            //only draft product can have null brand
            if (!Constant.PRODUCT_STATUS_DRAFT.Equals(request.Status)
                && (group.BrandId == null || group.BrandId == 0))
            {
                throw new Exception("Brand is required");
            }
            #endregion
            #region Attribute Set
            if (request.AttributeSet != null && request.AttributeSet.AttributeSetId != 0)
            {
                group.AttributeSetId = request.AttributeSet.AttributeSetId;
            }
            #endregion
            #region Product Stage Global Cat Maps
            var tmpGlobalCategories = group.ProductStageGlobalCatMaps.ToList();
            if (request.GlobalCategories != null && request.GlobalCategories.Count > 0)
            {
                foreach (var category in request.GlobalCategories)
                {
                    bool isNew = false;
                    if(tmpGlobalCategories == null || tmpGlobalCategories.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpGlobalCategories.Where(w => w.CategoryId == category.CategoryId).SingleOrDefault();
                        if(current != null)
                        {
                            tmpGlobalCategories.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                        {
                            CategoryId = category.CategoryId,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
            }
            if(tmpGlobalCategories != null && tmpGlobalCategories.Count > 0)
            {
                db.ProductStageGlobalCatMaps.RemoveRange(tmpGlobalCategories);
            }
            #endregion
            #region Product Stage Local Cat Maps
            var tmpLocalCategories = group.ProductStageLocalCatMaps.ToList();
            if (request.LocalCategories != null && request.LocalCategories.Count > 0)
            {
                foreach (var category in request.LocalCategories)
                {
                    bool isNew = false;
                    if (tmpLocalCategories == null || tmpLocalCategories.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpLocalCategories.Where(w => w.CategoryId == category.CategoryId).SingleOrDefault();
                        if (current != null)
                        {
                            tmpLocalCategories.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                        {
                            CategoryId = category.CategoryId,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
            }
            if (tmpLocalCategories != null && tmpLocalCategories.Count > 0)
            {
                db.ProductStageLocalCatMaps.RemoveRange(tmpLocalCategories);
            }
            #endregion
            #region Tag
            var tmpTag = group.ProductStageTags.ToList();
            if (request.Tags != null && request.Tags.Count > 0)
            {
                request.Tags = request.Tags.Distinct().ToList();
                foreach (var tag in request.Tags)
                {
                    bool isNew = false;
                    if(tmpTag == null || tmpTag.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpTag.Where(w => w.Tag.Equals(tag)).SingleOrDefault();
                        if(current != null)
                        {
                            tmpTag.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        group.ProductStageTags.Add(new ProductStageTag()
                        {
                            Tag = tag,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                    
                }
            }
            if (tmpTag != null && tmpTag.Count > 0)
            {
                db.ProductStageTags.RemoveRange(tmpTag);
            }
            #endregion
            #region Related Product
            var tmpRelated = group.ProductStageRelateds1.ToList();
            if (request.RelatedProducts != null && request.RelatedProducts.Count > 0)
            {
                foreach (var product in request.RelatedProducts)
                {
                    bool isNew = false;
                    if(tmpRelated == null || tmpRelated.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpRelated.Where(w => w.Child == product.ProductId).SingleOrDefault();
                        if(current != null)
                        {
                            tmpRelated.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        group.ProductStageRelateds1.Add(new ProductStageRelated()
                        {
                            Child = product.ProductId,
                            ShopId = group.ShopId,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt
                        });
                    }
                }
            }
            if(tmpRelated != null && tmpRelated.Count > 0)
            {
                db.ProductStageRelateds.RemoveRange(tmpRelated);
            }
            #endregion
            #region Shipping
            if (request.ShippingMethod == 0)
            {
                request.ShippingMethod = 1;
            }
            var tmpShipping = shippingList.Where(w => w.ShippingId == request.ShippingMethod).SingleOrDefault();
            if (tmpShipping == null)
            {
                throw new Exception("Invalid Shipping");
            }
            group.ShippingId = request.ShippingMethod;
            #endregion
            #region Other field
            group.IsNew = request.ControlFlags.IsNew;
            group.IsClearance = request.ControlFlags.IsClearance;
            group.IsBestSeller = request.ControlFlags.IsBestSeller;
            group.IsOnlineExclusive = request.ControlFlags.IsOnlineExclusive;
            group.IsOnlyAt = request.ControlFlags.IsOnlyAt;
            group.TheOneCardEarn = request.TheOneCardEarn;
            group.GiftWrap =  Validation.ValidateString(request.GiftWrap, "Gift Wrap", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            group.EffectiveDate = request.EffectiveDate.HasValue ? request.EffectiveDate.Value : currentDt;
            group.ExpireDate = request.ExpireDate.HasValue && request.ExpireDate.Value.CompareTo(group.EffectiveDate) >= 0 ? request.ExpireDate.Value : group.EffectiveDate.AddYears(Constant.DEFAULT_ADD_YEAR);
            group.Remark = Validation.ValidateString(request.Remark, "Remark", true, 500, false, string.Empty);
            group.ImageFlag = false;
            group.InfoFlag = false;
            group.OnlineFlag = false;
            group.InformationTabStatus = Validation.ValidateString(request.AdminApprove.Information, "Information Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() { Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.ImageTabStatus = Validation.ValidateString(request.AdminApprove.Image, "Image Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.CategoryTabStatus = Validation.ValidateString(request.AdminApprove.Category, "Category Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.MoreOptionTabStatus = Validation.ValidateString(request.AdminApprove.MoreOption, "More Option Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.VariantTabStatus = Validation.ValidateString(request.AdminApprove.Variation, "Variant Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.RejectReason = Validation.ValidateString(request.AdminApprove.RejectReason, "Reject Reason", true, 500, true, string.Empty);
            //group.SaleUnitEn = Validation.ValidateString(request.SaleUnitEn, "Sale Unit (English)", true, 30, true, string.Empty);
            //group.SaleUnitTh = Validation.ValidateString(request.SaleUnitEn, "Sale Unit (Thai)", true, 30, true, string.Empty);
            #endregion
            #region Create/Update
            if (addNew)
            {
                group.CreateBy = email;
                group.CreateOn = currentDt;
            }
            group.UpdateBy = email;
            group.UpdateOn = currentDt;
            #endregion
            #region Submitted and Approved
            if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(group.Status))
            {
                group.SubmitBy = email;
                group.SubmitOn = DateTime.Now;
            }
            else if (Constant.PRODUCT_STATUS_APPROVE.Equals(group.Status))
            {
                group.ApproveBy = email;
                group.ApproveOn = DateTime.Now;
            }
            #endregion
        }

        private void SetupAttribute(ProductStage variant, List<AttributeRequest> requestList, List<Entity.Models.Attribute> attributeList, string email, ColspEntities db)
        {
            var tmpAttribute = variant.ProductStageAttributes.ToList();
            int position = 1;
            foreach (var request in requestList)
            {
                if (request == null || request.AttributeId == 0) continue;
                bool isNew = false;
                var attribute = attributeList.Where(w => w.AttributeId == request.AttributeId).SingleOrDefault();
                if (attribute == null)
                {
                    throw new Exception("No attribute found " + request.AttributeId);
                }
                if (variant.IsVariant && !attribute.VariantStatus)
                {
                    throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " is not variant type"));
                }
                string value = request.ValueEn;
                bool isAttributeValue = false;
                bool checkboxValue = false;
                int? attributeValueId = null;
                if (Constant.DATA_TYPE_LIST.Equals(attribute.DataType))
                {
                    if (request.AttributeValues == null)
                    {
                        throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " should have variant"));
                    }
                    if (attribute.AttributeValueMaps.Any(a => request.AttributeValues
                        .Select(s => s.AttributeValueId)
                        .Contains(a.AttributeValueId)))
                    {
                        value = string.Concat(
                            Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
                            request.AttributeValues.FirstOrDefault().AttributeValueId,
                            Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
                        isAttributeValue = true;
                        attributeValueId = request.AttributeValues.FirstOrDefault().AttributeValueId;
                    }
                    else
                    {
                        throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " id ", attribute.AttributeId, " has no ", "Attribute value ",
                            string.Join(",", request.AttributeValues.Select(s => s.AttributeValueEn))));
                    }
                }
                else if (Constant.DATA_TYPE_CHECKBOX.Equals(attribute.DataType))
                {
                    if (request.AttributeValues == null)
                    {
                        throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " should have variant"));
                    }
                    if (attribute.AttributeValueMaps.Any(a => request.AttributeValues
                       .Select(s => s.AttributeValueId)
                       .Contains(a.AttributeValueId)))
                    {
                        foreach (var attributeValue in request.AttributeValues)
                        {
                            value = string.Concat(
                                Constant.ATTRIBUTE_VALUE_MAP_PREFIX,
                                attributeValue.AttributeValueId,
                                Constant.ATTRIBUTE_VALUE_MAP_SURFIX);
                            isAttributeValue = true;
                            attributeValueId = request.AttributeValues.FirstOrDefault().AttributeValueId;
                            checkboxValue = attributeValue.CheckboxValue;
                            #region Add Value
                            isNew = false;
                            if (tmpAttribute == null || tmpAttribute.Count == 0)
                            {
                                isNew = true;
                            }
                            if (!isNew)
                            {
                                var current = tmpAttribute.Where(w => w.AttributeId == request.AttributeId && w.ValueEn.Equals(value)).SingleOrDefault();
                                if (current != null)
                                {
                                    if(current.CheckboxValue != checkboxValue)
                                    {
                                        current.CheckboxValue = checkboxValue;
                                        current.UpdateBy = email;
                                        current.UpdateOn = DateTime.Now;
                                    }
                                    
                                    tmpAttribute.Remove(current);
                                }
                                else
                                {
                                    isNew = true;
                                }
                            }
                            if (isNew)
                            {
                                variant.ProductStageAttributes.Add(new ProductStageAttribute()
                                {
                                    AttributeId = attribute.AttributeId,
                                    CheckboxValue = checkboxValue,
                                    ValueEn = value,
                                    Position = position++,
                                    IsAttributeValue = isAttributeValue,
                                    AttributeValueId = attributeValueId,
                                    CreateBy = email,
                                    CreateOn = DateTime.Now,
                                    UpdateBy = email,
                                    UpdateOn = DateTime.Now,
                                });
                            }
                            #endregion
                        }
                        continue;
                    }
                }
                else
                {
                    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                    if (rg.IsMatch(value))
                    {
                        throw new Exception(string.Concat("Attribute value cannot contain prefix "
                            , Constant.ATTRIBUTE_VALUE_MAP_PREFIX
                            , " and surfix "
                            , Constant.ATTRIBUTE_VALUE_MAP_SURFIX));
                    }
                }

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                if(tmpAttribute == null || tmpAttribute.Count == 0)
                {
                    isNew = true;
                }
                if (!isNew)
                {
                    var current = tmpAttribute.Where(w => w.AttributeId == request.AttributeId && w.ValueEn.Equals(value)).SingleOrDefault();
                    if(current != null)
                    {
                        tmpAttribute.Remove(current);
                    }
                    else
                    {
                        isNew = true;
                    }
                }
                if (isNew)
                {
                    variant.ProductStageAttributes.Add(new ProductStageAttribute()
                    {
                        AttributeId = attribute.AttributeId,
                        CheckboxValue = checkboxValue,
                        ValueEn = value,
                        Position = position++,
                        IsAttributeValue = isAttributeValue,
                        AttributeValueId = attributeValueId,
                        CreateBy = email,
                        CreateOn = DateTime.Now,
                        UpdateBy = email,
                        UpdateOn = DateTime.Now,
                    });
                }
            }
            if(tmpAttribute != null && tmpAttribute.Count > 0)
            {
                db.ProductStageAttributes.RemoveRange(tmpAttribute);
            }
        }

        private void SetupVariant(ProductStage variant,VariantRequest request, bool addNew, string email, DateTime currentDt, bool adminPermission, bool sellerPermission, ColspEntities db)
        {
            #region Status
            if (adminPermission)
            {
                variant.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
                variant.GlobalBoostWeight = request.SEO.GlobalProductBoostingWeight;
            }
            else if (sellerPermission)
            {
                variant.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL });
            }
            else
            {
                variant.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            }
            #endregion
            #region Variant Field
            variant.ProductNameTh = Validation.ValidateString(request.ProductNameTh, "Product Name (Thai)", true, 300, true);
            variant.ProductNameEn = Validation.ValidateString(request.ProductNameEn, "Product Name (English)", true, 300, true);
            variant.ProdTDNameEn = Validation.ValidateString(request.ProdTDNameEn, "Prod TD Name (English)", false, 55, true,string.Empty);
            variant.ProdTDNameTh = Validation.ValidateString(request.ProdTDNameTh, "Prod TD Name (Thai)", false, 55, true, string.Empty);
            variant.Sku = Validation.ValidateString(request.Sku, "SKU", false, 300, true, string.Empty);
            variant.Upc = Validation.ValidateString(request.Upc, "UPC", false, 300, true, string.Empty);
            variant.OriginalPrice = Validation.ValidateDecimal(request.OriginalPrice, "Original Price", false, 10, 3, true, 0).Value;
            variant.SalePrice = Validation.ValidateDecimal(request.SalePrice, "Sale Price", true, 10, 3, true).Value;
            variant.IsHasExpiryDate = Validation.ValidateString(request.IsHasExpiryDate, "Is  Has Expiry Date", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            variant.IsVat = Validation.ValidateString(request.IsVat, "Is Vat", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            variant.UnitPrice = Validation.ValidateDecimal(request.UnitPrice, "Unit Price", false, 10, 3, true, 0).Value;
            variant.PurchasePrice = Validation.ValidateDecimal(request.PurchasePrice, "Purchase Price", true, 10, 3, true).Value;
            variant.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Description (Thai)", false, int.MaxValue, false, string.Empty);
            variant.DescriptionShortTh = Validation.ValidateString(request.DescriptionShortTh, "Short Description (Thai)", false, 500, true, string.Empty);
            variant.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Description (English)", false, int.MaxValue, false, string.Empty);
            variant.DescriptionShortEn = Validation.ValidateString(request.DescriptionShortEn, "Short Description (English)", false, 500, true, string.Empty);
            variant.MobileDescriptionEn = Validation.ValidateString(request.MobileDescriptionEn, "Mobile Description (English)", false, 500, true, string.Empty);
            variant.MobileDescriptionTh = Validation.ValidateString(request.MobileDescriptionTh, "Mobile Description (Thai)", false, 500, true, string.Empty);
            variant.PrepareDay = request.PrepareDay;
            variant.LimitIndividualDay = request.LimitIndividualDay;
            if (variant.LimitIndividualDay)
            {
                variant.PrepareMon = request.PrepareMon;
                variant.PrepareTue = request.PrepareTue;
                variant.PrepareWed = request.PrepareWed;
                variant.PrepareThu = request.PrepareThu;
                variant.PrepareFri = request.PrepareFri;
                variant.PrepareSat = request.PrepareSat;
                variant.PrepareSun = request.PrepareSun;
            }
            else
            {
                variant.PrepareMon = variant.PrepareDay;
                variant.PrepareTue = variant.PrepareDay;
                variant.PrepareWed = variant.PrepareDay;
                variant.PrepareThu = variant.PrepareDay;
                variant.PrepareFri = variant.PrepareDay;
                variant.PrepareSat = variant.PrepareDay;
                variant.PrepareSun = variant.PrepareDay;
            }
            variant.KillerPoint1En = Validation.ValidateString(request.KillerPoint1En, "Killer Point 1 (English)", false, 200, false, string.Empty);
            variant.KillerPoint2En = Validation.ValidateString(request.KillerPoint2En, "Killer Point 2 (English)", false, 200, false, string.Empty);
            variant.KillerPoint3En = Validation.ValidateString(request.KillerPoint3En, "Killer Point 3 (English)", false, 200, false, string.Empty);
            variant.KillerPoint1Th = Validation.ValidateString(request.KillerPoint1Th, "Killer Point 1 (Thai)", false, 200, false, string.Empty);
            variant.KillerPoint2Th = Validation.ValidateString(request.KillerPoint2Th, "Killer Point 2 (Thai)", false, 200, false, string.Empty);
            variant.KillerPoint3Th = Validation.ValidateString(request.KillerPoint3Th, "Killer Point 3 (Thai)", false, 200, false, string.Empty);
            variant.Installment = Validation.ValidateString(request.Installment, "Installment", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            variant.Length = Validation.ValidateDecimal(request.Length, "Length", true, 11, 2, true,0).Value;
            variant.Height = Validation.ValidateDecimal(request.Height, "Height", true, 11, 2, true,0).Value;
            variant.Width = Validation.ValidateDecimal(request.Width, "Width", true, 11, 2, true,0).Value;
            variant.Weight = Validation.ValidateDecimal(request.Weight, "Weight", true, 11, 2, true,0).Value;
            variant.DimensionUnit = Validation.ValidateString(request.DimensionUnit, "Dimension Unit", true, 2, true, Constant.DIMENSTION_MM, new List<string>() { Constant.DIMENSTION_MM, Constant.DIMENSTION_CM, Constant.DIMENSTION_M });
            variant.WeightUnit = Validation.ValidateString(request.WeightUnit, "Weight Unit", true, 2, true, Constant.WEIGHT_MEASURE_G, new List<string>() { Constant.WEIGHT_MEASURE_G, Constant.WEIGHT_MEASURE_KG });
            variant.MetaTitleEn = Validation.ValidateString(request.SEO.MetaTitleEn, "Meta Title (English)", false, 60, false,string.Empty);
            variant.MetaTitleTh = Validation.ValidateString(request.SEO.MetaTitleTh, "Meta Title (Thai)", false, 60, false, string.Empty);
            variant.MetaDescriptionEn = Validation.ValidateString(request.SEO.MetaDescriptionEn, "Meta Description (English)", false, 150, false, string.Empty);
            variant.MetaDescriptionTh = Validation.ValidateString(request.SEO.MetaDescriptionTh, "Meta Description (Thai)", false, 150, false, string.Empty);
            variant.MetaKeyEn = Validation.ValidateString(request.SEO.MetaKeywordEn, "Meta Keyword (English)", false, 150, false, string.Empty);
            variant.MetaKeyTh = Validation.ValidateString(request.SEO.MetaKeywordTh, "Meta Keyword (Thai)", false, 150, false, string.Empty);
            variant.SeoEn = Validation.ValidateString(request.SEO.SeoEn, "SEO (English)", false, 300, false, string.Empty);
            variant.SeoTh = Validation.ValidateString(request.SEO.SeoTh, "SEO (Thai)", false, 300, false, string.Empty);
            variant.UrlKey = Validation.ValidateString(request.SEO.ProductUrlKeyEn, "Product Url Key", false, 300, false, string.Empty);
            variant.BoostWeight = request.SEO.ProductBoostingWeight;
            variant.Visibility = request.Visibility;
            variant.DefaultVariant = request.DefaultVariant;
            variant.Display = Validation.ValidateString(request.Display, "Display", true, 20, true, Constant.VARIANT_DISPLAY_GROUP, new List<string>() { Constant.VARIANT_DISPLAY_GROUP, Constant.VARIANT_DISPLAY_INDIVIDUAL });
            variant.JDADept = string.Empty;
            variant.JDASubDept = string.Empty;
            variant.SaleUnitEn = Validation.ValidateString(request.SaleUnitEn, "Sale Unit (English)", false, 30, false, string.Empty);
            variant.SaleUnitTh = Validation.ValidateString(request.SaleUnitTh, "Sale Unit (Thai)", false, 30, false, string.Empty);
            variant.IsSell = true;
            variant.ExpressDelivery = Validation.ValidateString(request.ExpressDelivery, "Express Delivery", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            variant.EffectiveDatePromotion = request.EffectiveDatePromotion;
            variant.ExpireDatePromotion = request.ExpireDatePromotion;
            variant.PromotionPrice = request.PromotionPrice;
            variant.IsHasExpiryDate = request.IsHasExpiryDate;
            #endregion
            #region Inventory
            int stockType = Constant.STOCK_TYPE[Constant.DEFAULT_STOCK_TYPE];
            if (Constant.STOCK_TYPE.ContainsKey(request.StockType))
            {
                stockType = Constant.STOCK_TYPE[request.StockType];
            }
            if (variant.Inventory != null)
            {
                if (variant.Inventory.Quantity != request.Quantity 
                    || variant.Inventory.SafetyStockSeller != request.SafetyStock
                    || variant.Inventory.StockType != stockType
                    || variant.Inventory.MaxQtyAllowInCart != request.MaxQtyAllowInCart
                    || variant.Inventory.MinQtyAllowInCart != request.MinQtyAllowInCart
                    || variant.Inventory.MaxQtyPreOrder != request.MaxQtyPreOrder
                    || variant.Inventory.UseDecimal != request.UseDecimal
                 )
                {
                    variant.Inventory.Quantity = request.Quantity;
                    variant.Inventory.SafetyStockSeller = request.SafetyStock;
                    variant.Inventory.StockType = stockType;
                    variant.Inventory.MaxQtyAllowInCart = request.MaxQtyAllowInCart;
                    variant.Inventory.MinQtyAllowInCart = request.MinQtyAllowInCart;
                    variant.Inventory.MaxQtyPreOrder = request.MaxQtyPreOrder;
                    variant.Inventory.UseDecimal = request.UseDecimal;
                    variant.Inventory.UpdateBy = email;
                    variant.Inventory.UpdateOn = currentDt;

                    InventoryHistory history = new InventoryHistory()
                    {
                        Pid = variant.Pid,
                        StockType = variant.Inventory.StockType,
                        Defect = variant.Inventory.Defect,
                        MaxQtyAllowInCart = variant.Inventory.MaxQtyAllowInCart,
                        MinQtyAllowInCart = variant.Inventory.MinQtyAllowInCart,
                        MaxQtyPreOrder = variant.Inventory.MaxQtyPreOrder,
                        UseDecimal = variant.Inventory.UseDecimal,
                        OnHold = variant.Inventory.OnHold,
                        Quantity = variant.Inventory.Quantity,
                        Reserve = variant.Inventory.Reserve,
                        SafetyStockAdmin = variant.Inventory.SafetyStockAdmin,
                        SafetyStockSeller = variant.Inventory.SafetyStockSeller,
                        Status = Constant.INVENTORY_STATUS_UPDATE,
                        CreateBy = email,
                        CreateOn = currentDt,
                        UpdateBy = email,
                        UpdateOn = currentDt,
                    };
                    db.InventoryHistories.Add(history);
                }
            }
            else
            {
                variant.Inventory = new Inventory()
                {
                    StockType = stockType,
                    MaxQtyAllowInCart = request.MaxQtyAllowInCart,
                    MinQtyAllowInCart = request.MinQtyAllowInCart,
                    UseDecimal = request.UseDecimal,
                    Defect = 0,
                    MaxQtyPreOrder = 0,
                    OnHold = 0,
                    Quantity = request.Quantity,
                    Reserve = 0,
                    SafetyStockAdmin = request.SafetyStock,
                    SafetyStockSeller = request.SafetyStock,
                    CreateBy = email,
                    CreateOn = currentDt,
                    UpdateBy = email,
                    UpdateOn = currentDt,
                };
            }

            #endregion
            #region Image
            var tmpImage = variant.ProductStageImages.ToList();
            if (request.Images != null && request.Images.Count > 0)
            {
                variant.ImageCount = request.Images.Count;
                int position = 1;
                bool featureImg = true;
                foreach (var image in request.Images)
                {
                    if (image == null && string.IsNullOrWhiteSpace(image.Url)) continue;
                    bool isNew = false;
                    if (tmpImage == null || tmpImage.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpImage.Where(w => w.ImageName.Equals(image.Url)).SingleOrDefault();
                        if (current != null)
                        {
                            if (current.SeqNo != position || current.FeatureFlag != featureImg)
                            {
                                current.SeqNo = position;
                                current.FeatureFlag = featureImg;
                                current.UpdateBy = email;
                                current.UpdateOn = currentDt;
                            }
                            ++position;
                            tmpImage.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        variant.ProductStageImages.Add(new ProductStageImage()
                        {
                            ShopId = variant.ShopId,
                            //ImageUrlEn = image.Url,
                            SeqNo = position++,
                            FeatureFlag = featureImg,
                            ImageName = string.Empty,
                            //ImageOriginName = string.Empty,
                            Status = Constant.STATUS_ACTIVE,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                    if (featureImg)
                    {
                        variant.FeatureImgUrl = image.Url;
                    }
                    featureImg = false;
                }
            }
            else
            {
                variant.ImageCount = 0;
                variant.FeatureImgUrl = string.Empty;
            }
            if (tmpImage != null && tmpImage.Count > 0)
            {
                db.ProductStageImages.RemoveRange(tmpImage);
            }
            #endregion
            #region Video
            var tmpVideo = variant.ProductStageVideos.ToList();
            if (request.VideoLinks != null && request.VideoLinks.Count > 0)
            {
                int position = 1;
                foreach (var video in request.VideoLinks)
                {
                    if (video == null || string.IsNullOrWhiteSpace(video.Url)) continue;
                    bool isNew = false;

                    if (tmpVideo == null || tmpVideo.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpVideo.Where(w => w.VideoUrlEn.Equals(video.Url)).SingleOrDefault();
                        if (current != null)
                        {
                            if(current.Position != position)
                            {
                                current.Position = position;
                                current.UpdateBy = email;
                                current.UpdateOn = currentDt;
                            }
                            ++position;
                            tmpVideo.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        variant.ProductStageVideos.Add(new ProductStageVideo()
                        {
                            ShopId = variant.ShopId,
                            VideoUrlEn = video.Url,
                            Status = Constant.STATUS_ACTIVE,
                            Position = position++,
                            CreateBy = email,
                            CreateOn = currentDt,
                            UpdateBy = email,
                            UpdateOn = currentDt,
                        });
                    }
                }
            }
                        
            if(tmpVideo !=null && tmpVideo.Count > 0)
            {
                db.ProductStageVideos.RemoveRange(tmpVideo);
            }
            #endregion
            #region Create/Update
            if (addNew)
            {
                variant.CreateBy = email;
                variant.CreateOn = currentDt;
            }
            variant.UpdateBy = email;
            variant.UpdateOn = currentDt;
            #endregion
        }

        private void SetupImage(ProductStage variant, List<ImageRequest> imageRq, ColspEntities db)
        {
            var tmpImage = variant.ProductStageImages.ToList();
            if (imageRq != null && imageRq.Count > 0)
            {
                variant.ImageCount = imageRq.Count;
                int position = 1;
                bool featureImg = true;
                foreach (var image in imageRq)
                {
                    if (image == null && string.IsNullOrWhiteSpace(image.Url)) continue;
                    bool isNew = false;
                    if (tmpImage == null || tmpImage.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpImage.Where(w => w.ImageName.Equals(image.Url)).SingleOrDefault();
                        if (current != null)
                        {
                            if (current.SeqNo != position || current.FeatureFlag != featureImg)
                            {
                                string lastPart = current.ImageName.Split('/').Last();
                                string oldFile = Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.PRODUCT_FOLDER, lastPart);
                                if (File.Exists(oldFile))
                                {
                                    string filename = Path.GetFileNameWithoutExtension(oldFile) + "_tmp" + Path.GetExtension(oldFile);
                                    string newFile = Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.PRODUCT_FOLDER, filename);
                                    if (File.Exists(newFile))
                                    {
                                        File.Delete(newFile);
                                    }
                                    File.Move(oldFile, newFile);
                                    //current.ImageUrlEn = filename; 
                                }
                                current.SeqNo = position;
                                current.FeatureFlag = featureImg;
                                current.UpdateBy = User.UserRequest().Email;
                                current.UpdateOn = DateTime.Now;
                            }
                            ++position;
                            tmpImage.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        variant.ProductStageImages.Add(new ProductStageImage()
                        {
                            ShopId = variant.ShopId,
                            //ImageUrlEn = image.Url,
                            SeqNo = position++,
                            FeatureFlag = featureImg,
                            ImageName = string.Empty,
                            //ImageOriginName = string.Empty,
                            Status = Constant.STATUS_ACTIVE,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                    }
                    if (featureImg)
                    {
                        variant.FeatureImgUrl = image.Url;
                    }
                    featureImg = false;
                }
            }
            else
            {
                variant.ImageCount = 0;
                variant.FeatureImgUrl = string.Empty;
            }
            if (tmpImage != null && tmpImage.Count > 0)
            {
                db.ProductStageImages.RemoveRange(tmpImage);
            }
        }

        

        

        private void SetupVariantResponse(ProductStage variant, VariantRequest response)
        {
            response.Pid = variant.Pid;
            response.ShopId = variant.ShopId;
            response.ProductNameTh = variant.ProductNameTh;
            response.ProductNameEn = variant.ProductNameEn;
            response.ProdTDNameEn = variant.ProdTDNameEn;
            response.ProdTDNameTh = variant.ProdTDNameTh;
            response.Sku = variant.Sku;
            response.Upc = variant.Upc;
            response.OriginalPrice = variant.OriginalPrice;
            response.SalePrice = variant.SalePrice;
            response.UnitPrice = variant.UnitPrice;
            response.PurchasePrice = variant.PurchasePrice;
            response.DescriptionFullTh = variant.DescriptionFullTh;
            response.DescriptionShortTh = variant.DescriptionShortTh;
            response.DescriptionFullEn = variant.DescriptionFullEn;
            response.DescriptionShortEn = variant.DescriptionShortEn;
            response.PrepareDay = variant.PrepareDay;
            response.LimitIndividualDay = variant.LimitIndividualDay;
            response.PrepareMon = variant.PrepareMon;
            response.PrepareTue = variant.PrepareTue;
            response.PrepareWed = variant.PrepareWed;
            response.PrepareThu = variant.PrepareThu;
            response.PrepareFri = variant.PrepareFri;
            response.PrepareSat = variant.PrepareSat;
            response.PrepareSun = variant.PrepareSun;
            response.KillerPoint1En = variant.KillerPoint1En;
            response.KillerPoint2En = variant.KillerPoint2En;
            response.KillerPoint3En = variant.KillerPoint3En;
            response.KillerPoint1Th = variant.KillerPoint1Th;
            response.KillerPoint2Th = variant.KillerPoint2Th;
            response.KillerPoint3Th = variant.KillerPoint3Th;
            response.Installment = variant.Installment;
            response.Length = variant.Length;
            response.Height = variant.Height;
            response.Width = variant.Width;
            response.Weight = variant.Weight;
            response.DimensionUnit = variant.DimensionUnit.Trim();
            response.WeightUnit = variant.WeightUnit.Trim();
            response.SEO.MetaTitleEn = variant.MetaTitleEn;
            response.SEO.MetaTitleTh = variant.MetaTitleTh;
            response.SEO.MetaDescriptionEn = variant.MetaDescriptionEn;
            response.SEO.MetaDescriptionTh = variant.MetaDescriptionTh;
            response.SEO.MetaKeywordEn = variant.MetaKeyEn;
            response.SEO.MetaKeywordTh = variant.MetaKeyTh;
            response.SEO.ProductBoostingWeight = variant.BoostWeight;
            response.SEO.SeoEn = variant.SeoEn;
            response.SEO.SeoTh = variant.SeoTh;
            response.Visibility = variant.Visibility;
            response.DefaultVariant = variant.DefaultVariant;
            response.Quantity = variant.Inventory.Quantity;
            response.SafetyStock = variant.Inventory.SafetyStockSeller;
            response.StockType = Constant.STOCK_TYPE.Where(w => w.Value.Equals(variant.Inventory.StockType)).SingleOrDefault().Key;
            response.PromotionPrice = variant.PromotionPrice;
            response.EffectiveDatePromotion = variant.EffectiveDatePromotion;
            response.ExpireDatePromotion = variant.ExpireDatePromotion;
            response.IsHasExpiryDate = variant.IsHasExpiryDate;
            response.Display = variant.Display;
            response.IsHasExpiryDate = variant.IsHasExpiryDate;
            response.IsVat = variant.IsVat;
            response.SaleUnitEn = variant.SaleUnitEn;
            response.SaleUnitTh = variant.SaleUnitTh;
            response.ExpressDelivery = variant.ExpressDelivery;

            if (variant.ProductStageImages != null && variant.ProductStageImages.Count > 0)
            {
                variant.ProductStageImages = variant.ProductStageImages.OrderBy(o => o.SeqNo).ToList();
                foreach (var image in variant.ProductStageImages)
                {
                    response.Images.Add(new ImageRequest()
                    {
                        ImageId = 0,
                        //Url = image.ImageUrlEn,
                        Position = image.SeqNo
                    });
                }
            }
            if(variant.ProductStageVideos != null && variant.ProductStageVideos.Count > 0)
            {
                variant.ProductStageVideos = variant.ProductStageVideos.OrderBy(o => o.Position).ToList();
                foreach (var video in variant.ProductStageVideos)
                {
                    response.VideoLinks.Add(new VideoLinkRequest()
                    {
                        VideoId = 0,
                        Url = video.VideoUrlEn
                    });
                }
            }
        }

        private void SetupGroupResponse(ProductStageGroup group, ProductStageRequest response)
        {
            response.ProductId = group.ProductId;
            response.ShopId = group.ShopId;
            response.MainGlobalCategory = new CategoryRequest() { CategoryId = group.GlobalCatId };
            if (group.LocalCatId != null)
            {
                response.MainLocalCategory = new CategoryRequest() { CategoryId = group.LocalCatId.Value };
            }
            if (group.Brand != null)
            {
                response.Brand = new BrandRequest() { BrandId = group.BrandId.HasValue ? group.BrandId.Value : 0, BrandNameEn = group.Brand.BrandNameEn  };
            }
            if (group.AttributeSetId != null)
            {
                response.AttributeSet = new AttributeSetRequest() { AttributeSetId = group.AttributeSetId.Value };
            }
            if (group.ProductStageGlobalCatMaps != null && group.ProductStageGlobalCatMaps.Count > 0)
            {
                foreach (var category in group.ProductStageGlobalCatMaps)
                {
                    response.GlobalCategories.Add(new CategoryRequest()
                    {
                        CategoryId = category.CategoryId,
                        NameEn = category.GlobalCategory.NameEn,
                        NameTh = category.GlobalCategory.NameTh
                    });
                }
            }
            if (group.ProductStageLocalCatMaps != null && group.ProductStageLocalCatMaps.Count > 0)
            {
                foreach (var category in group.ProductStageLocalCatMaps)
                {
                    response.LocalCategories.Add(new CategoryRequest()
                    {
                        CategoryId = category.CategoryId,
                        NameEn = category.LocalCategory.NameEn,
                        NameTh = category.LocalCategory.NameTh
                    });
                }
            }
            if(group.ProductStageRelateds1 != null && group.ProductStageRelateds1.Count > 0)
            {
                foreach (var pro in group.ProductStageRelateds1)
                {
                    response.RelatedProducts.Add(new VariantRequest()
                    {
                        ProductId = pro.ProductStageGroup1.ProductId,
                        ProductNameEn = pro.ProductStageGroup1.ProductStages.Where(w=>w.IsVariant==false).FirstOrDefault().ProductNameEn,
                    });
                }
            }
            if (group.ProductStageTags != null && group.ProductStageTags.Count > 0)
            {
                response.Tags = group.ProductStageTags.Select(s => s.Tag).ToList();
            }
            response.ShippingMethod = group.ShippingId;
            response.EffectiveDate = group.EffectiveDate;
            response.ExpireDate = group.ExpireDate;
            response.ControlFlags.IsNew = group.IsNew;
            response.ControlFlags.IsBestSeller = group.IsBestSeller;
            response.ControlFlags.IsClearance = group.IsClearance;
            response.ControlFlags.IsOnlineExclusive = group.IsOnlineExclusive;
            response.ControlFlags.IsOnlyAt = group.IsOnlyAt;
            response.TheOneCardEarn = group.TheOneCardEarn;
            response.GiftWrap = group.GiftWrap;
            response.Remark = group.Remark;
            response.AdminApprove.Information = group.InformationTabStatus;
            response.AdminApprove.Image = group.ImageTabStatus;
            response.AdminApprove.Category = group.CategoryTabStatus;
            response.AdminApprove.Variation = group.VariantTabStatus;
            response.AdminApprove.MoreOption = group.MoreOptionTabStatus;
            response.AdminApprove.RejectReason = group.RejectReason;
            response.InfoFlag = group.InfoFlag;
            response.ImageFlag = group.ImageFlag;
            response.OnlineFlag = group.OnlineFlag;
            response.Status = group.Status;
        }

        

        [Route("api/ProductStages/Export")]
        [HttpPost]
        public HttpResponseMessage ExportProductProducts(ExportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                #region validation
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                #endregion
                #region Setup Header
                Dictionary<string, Tuple<string, int>> headDicTmp = new Dictionary<string, Tuple<string, int>>();
                var tmpGuidance = db.ImportHeaders.Where(w => true);
                if (User.ShopRequest() != null)
                {
                    var groupName = User.ShopRequest().ShopGroup;
                    tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
                }
                var guidance = tmpGuidance.Where(w=>!w.MapName.Equals("ADM")).OrderBy(o => o.Position);
                int i = 0;
                foreach (var current in guidance)
                {
                    var op = request.Options.Where(w => w.Equals(current.MapName)).SingleOrDefault();
                    if (op == null)
                    {
                        continue;
                    }
                    if (!headDicTmp.ContainsKey(current.HeaderName))
                    {
                        headDicTmp.Add(current.MapName, new Tuple<string, int>(current.HeaderName, i++));
                    }
                }
                //default attribute
                if (request.Options.Contains("ADM"))
                {
                    List<string> defAttri = null;
                    if (User.ShopRequest() != null)
                    {
                        defAttri = db.Attributes.Where(w => w.DefaultAttribute && Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo)).Select(s => s.AttributeNameEn).ToList();
                    }
                    else
                    {
                        defAttri = db.Attributes.Where(w => w.DefaultAttribute && Constant.ATTRIBUTE_VISIBLE_ADMIN.Equals(w.VisibleTo)).Select(s => s.AttributeNameEn).ToList();
                    }
                    if (defAttri != null && defAttri.Count > 0)
                    {
                        foreach (var attr in defAttri)
                        {
                            headDicTmp.Add(attr, new Tuple<string, int>(attr, i++));
                        }
                    }
                }
                #endregion
                #region Query
                var query = db.ProductStages.Where(w=>!Constant.STATUS_REMOVE.Equals(w.Status))
                    .Select(s => new
                {
                    ProductStageGroup = s.ProductStageGroup == null ? null : new
                    {
                        Brand = s.ProductStageGroup.BrandId == null ? null : new
                        {
                            s.ProductStageGroup.Brand.BrandNameEn,
                            s.ProductStageGroup.Brand.BrandId,
                        },
                        Tags = s.ProductStageGroup.ProductStageTags.Select(st => st.Tag),
                        s.ProductStageGroup.GlobalCatId,
                        s.ProductStageGroup.LocalCatId,
                        ProductStageGlobalCatMaps = s.ProductStageGroup.ProductStageGlobalCatMaps.Select(sc => sc.CategoryId),
                        ProductStageLocalCatMaps = s.ProductStageGroup.ProductStageLocalCatMaps.Select(sc => sc.CategoryId),
                        ProductStageRelateds1 = s.ProductStageGroup.ProductStageRelateds1.Select(sp => sp.ProductStageGroup1.ProductStages.Where(w => w.IsVariant == false).Select(sv => sv.Pid)),
                        s.ProductStageGroup.EffectiveDate,
                        s.ProductStageGroup.ExpireDate,
                        s.ProductStageGroup.Shipping.ShippingMethodEn,
                        s.ExpressDelivery,
                        s.DeliveryFee,
                        s.NewArrivalDate,
                        s.ProductStageGroup.IsBestSeller,
                        s.ProductStageGroup.IsClearance,
                        s.ProductStageGroup.IsNew,
                        s.ProductStageGroup.IsOnlineExclusive,
                        s.ProductStageGroup.IsOnlyAt,
                        s.ProductStageGroup.Remark,
                        s.ProductStageGroup.GiftWrap,
                        AttributeSet = s.ProductStageGroup.AttributeSet == null ? null : new
                        {
                            s.ProductStageGroup.AttributeSet.AttributeSetId,
                            s.ProductStageGroup.AttributeSet.AttributeSetNameEn,
                            AttributeSetMaps = s.ProductStageGroup.AttributeSet.AttributeSetMaps.Select(sm => new
                            {
                                Attribute = sm.Attribute == null ? null : new
                                {
                                    sm.Attribute.AttributeNameEn
                                },
                            }),
                        },
                    },
                    s.Status,
                    s.ProductId,
                    s.DefaultVariant,
                    s.Pid,
                    s.ProductNameEn,
                    s.ProductNameTh,
                    s.ProdTDNameEn,
                    s.ProdTDNameTh,
                    s.Sku,
                    s.Upc,
                    s.OriginalPrice,
                    s.PromotionPrice,
                    s.EffectiveDatePromotion,
                    s.ExpireDatePromotion,
                    s.SalePrice,
                    s.UnitPrice,
                    s.PurchasePrice,
                    s.SaleUnitEn,
                    s.SaleUnitTh,
                    s.IsVat,
                    s.Installment,
                    s.DescriptionFullEn,
                    s.DescriptionFullTh,
                    s.MobileDescriptionEn,
                    s.MobileDescriptionTh,
                    s.DescriptionShortEn,
                    s.DescriptionShortTh,
                    s.KillerPoint1En,
                    s.KillerPoint1Th,
                    s.KillerPoint2En,
                    s.KillerPoint2Th,
                    s.KillerPoint3En,
                    s.KillerPoint3Th,
                    s.IsHasExpiryDate,
                    Inventory = s.Inventory == null ? null : new
                    {
                        s.Inventory.Quantity,
                        s.Inventory.SafetyStockSeller,
                        s.Inventory.StockType,
                        s.Inventory.MinQtyAllowInCart,
                        s.Inventory.MaxQtyAllowInCart,
                        s.Inventory.MaxQtyPreOrder
                    },
                    s.PrepareDay,
                    s.PrepareMon,
                    s.PrepareTue,
                    s.PrepareWed,
                    s.PrepareThu,
                    s.PrepareFri,
                    s.PrepareSat,
                    s.PrepareSun,
                    s.Length,
                    s.Width,
                    s.Height,
                    s.Weight,
                    s.MetaDescriptionEn,
                    s.MetaDescriptionTh,
                    s.MetaKeyEn,
                    s.MetaKeyTh,
                    s.MetaTitleEn,
                    s.MetaTitleTh,
                    s.SeoEn,
                    s.SeoTh,
                    s.UrlKey,
                    s.BoostWeight,
                    s.GlobalBoostWeight,
                   
                    ProductStageAttributes = s.ProductStageAttributes.Select(ss => new
                    {
                        ss.IsAttributeValue,
                        ss.CheckboxValue,
                        ss.ValueEn,
                        ss.ValueTh,
                        Attribute = ss.Attribute == null ? null : new
                        {
                            ss.Attribute.AttributeId,
                            ss.Attribute.AttributeNameEn,
                            DataType = ss.Attribute.DataType,
                            DefaultAttribute = ss.Attribute.DefaultAttribute,
                            AttributeValueMaps =
                            ss.Attribute.AttributeValueMaps.Select(sv => new
                            {
                                AttributeValue = sv.AttributeValue == null ? null : new
                                {
                                    sv.AttributeValue.AttributeValueEn,
                                    sv.AttributeValue.AttributeValueId,
                                    sv.AttributeValue.MapValue,
                                }
                            })
                        }
                    }),
                    s.ShopId,
                    s.IsVariant,
                    s.VariantCount,
                    s.Visibility,
                });
                var productIds = request.ProductList.Select(s => s.ProductId).ToList();
                if (productIds != null && productIds.Count > 0)
                {
                    if (productIds.Count > 2000)
                    {
                        throw new Exception("Too many product selected");
                    }
                    query = query.Where(w => productIds.Contains(w.ProductId));
                }
                if (User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    query = query.Where(w => w.ShopId == shopId);

                    if (User.BrandRequest() != null)
                    {
                        var brands = User.BrandRequest().Select(s => s.BrandId).ToList();
                        if (brands != null && brands.Count > 0)
                        {
                            query = query.Where(w => w.ProductStageGroup.Brand != null && brands.Contains(w.ProductStageGroup.Brand.BrandId));
                        }
                    }
                }
                var productList = query.OrderBy(o=>o.ProductId).ThenBy(o=>o.IsVariant);
                #endregion
                #region initiate
                List<List<string>> rs = new List<List<string>>();
                List<string> bodyList = null;
                if (request.AttributeSets != null && request.AttributeSets.Count > 0)
                {
                    headDicTmp.Add("ADI", new Tuple<string, int>("Attribute Set", i++));
                    headDicTmp.Add("ADJ", new Tuple<string, int>("Variation Option 1", i++));
                    headDicTmp.Add("ADK", new Tuple<string, int>("Variation Option 2", i++));
                }
                List<ProductStageAttribute> masterAttribute = null;
                List<ProductStageAttribute> defaultAttribute = null;
                #endregion
                foreach (var p in productList)
                {
                    #region Setup Attribute
                    if (p.IsVariant == false)
                    {
                        masterAttribute = new List<ProductStageAttribute>();
                        foreach(var stageAttr in p.ProductStageAttributes.Where(w => !w.Attribute.DefaultAttribute))
                        {
                            var tmpAttribute = new ProductStageAttribute();
                            tmpAttribute.ValueEn = stageAttr.ValueEn;
                            tmpAttribute.ValueTh = stageAttr.ValueTh;
                            tmpAttribute.CheckboxValue = stageAttr.CheckboxValue;
                            if (stageAttr.Attribute != null)
                            {
                                tmpAttribute.Attribute = new Entity.Models.Attribute()
                                {
                                    AttributeId = stageAttr.Attribute.AttributeId,
                                    AttributeNameEn = stageAttr.Attribute.AttributeNameEn,
                                    DataType = stageAttr.Attribute.DataType,
                                    DefaultAttribute = stageAttr.Attribute.DefaultAttribute,
                                };
                                if (stageAttr.Attribute.AttributeValueMaps != null)
                                {
                                    foreach (var stageVal in stageAttr.Attribute.AttributeValueMaps)
                                    {
                                        tmpAttribute.Attribute.AttributeValueMaps.Add(new AttributeValueMap()
                                        {
                                            AttributeId = tmpAttribute.AttributeId,
                                            AttributeValueId = stageVal.AttributeValue.AttributeValueId,
                                            AttributeValue = new AttributeValue()
                                            {
                                                AttributeValueId = stageVal.AttributeValue.AttributeValueId,
                                                AttributeValueEn = stageVal.AttributeValue.AttributeValueEn,
                                                MapValue = stageVal.AttributeValue.MapValue,
                                            }
                                        });
                                    }
                                }
                            }
                            masterAttribute.Add(tmpAttribute);
                        }

                        defaultAttribute = new List<ProductStageAttribute>();
                        foreach (var stageAttr in p.ProductStageAttributes.Where(w => w.Attribute.DefaultAttribute))
                        {
                            var tmpAttribute = new ProductStageAttribute();
                            tmpAttribute.ValueEn = stageAttr.ValueEn;
                            tmpAttribute.ValueTh = stageAttr.ValueTh;
                            tmpAttribute.CheckboxValue = stageAttr.CheckboxValue;
                            if (stageAttr.Attribute != null)
                            {
                                tmpAttribute.Attribute = new Entity.Models.Attribute()
                                {
                                    AttributeId = stageAttr.Attribute.AttributeId,
                                    AttributeNameEn = stageAttr.Attribute.AttributeNameEn,
                                    DataType = stageAttr.Attribute.DataType,
                                    DefaultAttribute = stageAttr.Attribute.DefaultAttribute,
                                };
                                if (stageAttr.Attribute.AttributeValueMaps != null)
                                {
                                    foreach (var stageVal in stageAttr.Attribute.AttributeValueMaps)
                                    {
                                        tmpAttribute.Attribute.AttributeValueMaps.Add(new AttributeValueMap()
                                        {
                                            AttributeId = tmpAttribute.AttributeId,
                                            AttributeValueId = stageVal.AttributeValue.AttributeValueId,
                                            AttributeValue = new AttributeValue()
                                            {
                                                AttributeValueId = stageVal.AttributeValue.AttributeValueId,
                                                AttributeValueEn = stageVal.AttributeValue.AttributeValueEn,
                                                MapValue = stageVal.AttributeValue.MapValue,
                                            }
                                        });
                                    }
                                }
                            }
                            defaultAttribute.Add(tmpAttribute);
                        }
                        if (p.VariantCount > 0)
                        {
                            continue;
                        }
                        //masterAttribute = p.ProductStageAttributes.Where(w=>!w.Attribute.DefaultAttribute).ToList();
                        //defaultAttribute = p.ProductStageAttributes.Where(w => w.Attribute.DefaultAttribute).ToList();
                    }
                    #endregion
                    bodyList = new List<string>(new string[headDicTmp.Count]);
                    #region System Information
                    if (headDicTmp.ContainsKey("AAA"))
                    {
                        if (Constant.PRODUCT_STATUS_DRAFT.Equals(p.Status))
                        {
                            bodyList[headDicTmp["AAA"].Item2] = "Draft";
                        }
                        else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(p.Status))
                        {
                            bodyList[headDicTmp["AAA"].Item2] = "Wait for Approval";
                        }
                        else if (Constant.PRODUCT_STATUS_APPROVE.Equals(p.Status))
                        {
                            bodyList[headDicTmp["AAA"].Item2] = "Approve";
                        }
                        else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(p.Status))
                        {
                            bodyList[headDicTmp["AAA"].Item2] = "Not Approve";
                        }
                    }
                    if (headDicTmp.ContainsKey("AAB"))
                    {
                        bodyList[headDicTmp["AAB"].Item2] = string.Concat(p.ProductId);
                    }
                    if (headDicTmp.ContainsKey("AAC"))
                    {
                        bodyList[headDicTmp["AAC"].Item2] = p.DefaultVariant == true ? "Yes" : "No";
                    }
                    if (headDicTmp.ContainsKey("AAD"))
                    {
                        bodyList[headDicTmp["AAD"].Item2] = p.Pid;
                    }
                    #endregion
                    #region Vital Infromation
                    if (headDicTmp.ContainsKey("ADL"))
                    {
                        bodyList[headDicTmp["ADL"].Item2] = p.Visibility == true ? "Yes" : "No";
                    }
                    if (headDicTmp.ContainsKey("AAE"))
                    {
                        bodyList[headDicTmp["AAE"].Item2] = p.ProductNameEn;
                    }
                    if (headDicTmp.ContainsKey("AAF"))
                    {
                        bodyList[headDicTmp["AAF"].Item2] = p.ProductNameTh;
                    }

                    if (headDicTmp.ContainsKey("AAG"))
                    {
                        bodyList[headDicTmp["AAG"].Item2] = p.ProdTDNameEn;
                    }
                    if (headDicTmp.ContainsKey("AAH"))
                    {
                        bodyList[headDicTmp["AAH"].Item2] = p.ProdTDNameTh;
                    }
                    if (headDicTmp.ContainsKey("AAI"))
                    {
                        bodyList[headDicTmp["AAI"].Item2] = p.Sku;
                    }

                    if (headDicTmp.ContainsKey("AAJ"))
                    {
                        bodyList[headDicTmp["AAJ"].Item2] = p.Upc;
                    }
                    if (headDicTmp.ContainsKey("AAK"))
                    {
                        if (p.ProductStageGroup.Brand != null && !string.IsNullOrEmpty(p.ProductStageGroup.Brand.BrandNameEn))
                        {
                            bodyList[headDicTmp["AAK"].Item2] = p.ProductStageGroup.Brand.BrandNameEn;
                        }
                    }
                    #endregion
                    #region Price
                    if (headDicTmp.ContainsKey("AAL"))
                    {
                        bodyList[headDicTmp["AAL"].Item2] = string.Concat(p.SalePrice);
                    }
                    if (headDicTmp.ContainsKey("AAM"))
                    {
                        bodyList[headDicTmp["AAM"].Item2] = string.Concat(p.OriginalPrice);
                    }
                    if (headDicTmp.ContainsKey("AAN"))
                    {
                        bodyList[headDicTmp["AAN"].Item2] = Constant.STATUS_YES.Equals(p.Installment) ? "Yes" : "No";
                    }
                    if (headDicTmp.ContainsKey("AAO"))
                    {
                        bodyList[headDicTmp["AAO"].Item2] = string.Concat(p.PromotionPrice);
                    }
                    if (headDicTmp.ContainsKey("AAP"))
                    {
                        if (p.EffectiveDatePromotion != null)
                        {
                            bodyList[headDicTmp["AAP"].Item2] = p.EffectiveDatePromotion.Value.ToString(Constant.DATETIME_FORMAT);
                        }
                    }
                    if (headDicTmp.ContainsKey("AAQ"))
                    {
                        if (p.ExpireDatePromotion != null)
                        {
                            bodyList[headDicTmp["AAQ"].Item2] = p.ExpireDatePromotion.Value.ToString(Constant.DATETIME_FORMAT);
                        }
                    }
                    if (headDicTmp.ContainsKey("AAR"))
                    {
                        bodyList[headDicTmp["AAR"].Item2] = string.Concat(p.UnitPrice);
                    }
                    if (headDicTmp.ContainsKey("AAS"))
                    {
                        bodyList[headDicTmp["AAS"].Item2] = string.Concat(p.PurchasePrice);
                    }
                    if (headDicTmp.ContainsKey("AAT"))
                    {
                        bodyList[headDicTmp["AAT"].Item2] = p.SaleUnitEn;
                    }
                    if (headDicTmp.ContainsKey("AAU"))
                    {
                        bodyList[headDicTmp["AAU"].Item2] = p.SaleUnitTh;
                    }
                    if (headDicTmp.ContainsKey("AAV"))
                    {
                        bodyList[headDicTmp["AAV"].Item2] = Constant.STATUS_YES.Equals(p.IsVat) ? "Yes" : "No";
                    }

                    #endregion
                    #region Description
                    if (headDicTmp.ContainsKey("AAW"))
                    {
                        bodyList[headDicTmp["AAW"].Item2] = p.DescriptionFullEn;
                    }
                    if (headDicTmp.ContainsKey("AAX"))
                    {
                        bodyList[headDicTmp["AAX"].Item2] = p.DescriptionFullTh;
                    }
                    if (headDicTmp.ContainsKey("AAY"))
                    {
                        bodyList[headDicTmp["AAY"].Item2] = p.MobileDescriptionEn;
                    }
                    if (headDicTmp.ContainsKey("AAZ"))
                    {
                        bodyList[headDicTmp["AAZ"].Item2] = p.MobileDescriptionTh;
                    }
                    if (headDicTmp.ContainsKey("ABA"))
                    {
                        bodyList[headDicTmp["ABA"].Item2] = p.DescriptionShortEn;
                    }
                    if (headDicTmp.ContainsKey("ABB"))
                    {
                        bodyList[headDicTmp["ABB"].Item2] = p.DescriptionShortTh;
                    }
                    if (headDicTmp.ContainsKey("ABC"))
                    {
                        bodyList[headDicTmp["ABC"].Item2] = p.KillerPoint1En;
                    }
                    if (headDicTmp.ContainsKey("ABD"))
                    {
                        bodyList[headDicTmp["ABD"].Item2] = p.KillerPoint1Th;
                    }
                    if (headDicTmp.ContainsKey("ABE"))
                    {
                        bodyList[headDicTmp["ABE"].Item2] = p.KillerPoint2En;
                    }
                    if (headDicTmp.ContainsKey("ABF"))
                    {
                        bodyList[headDicTmp["ABF"].Item2] = p.KillerPoint2Th;
                    }
                    if (headDicTmp.ContainsKey("ABG"))
                    {
                        bodyList[headDicTmp["ABG"].Item2] = p.KillerPoint3En;
                    }
                    if (headDicTmp.ContainsKey("ABH"))
                    {
                        bodyList[headDicTmp["ABH"].Item2] = p.KillerPoint3Th;
                    }
                    #endregion
                    #region Search Tags
                    if (headDicTmp.ContainsKey("ABI"))
                    {
                        if(p.ProductStageGroup.Tags != null && p.ProductStageGroup.Tags.ToList().Count > 0)
                        {
                            bodyList[headDicTmp["ABI"].Item2] = string.Join(",", p.ProductStageGroup.Tags);
                        }
                    }
                    #endregion
                    #region Inventory
                    if (headDicTmp.ContainsKey("ABJ"))
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["ABJ"].Item2] = string.Concat(p.Inventory.Quantity);
                        }
                    }
                    if (headDicTmp.ContainsKey("ABL"))
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["ABL"].Item2] = string.Concat(p.Inventory.SafetyStockSeller);
                        }
                    }
                    if (headDicTmp.ContainsKey("ABM"))
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["ABM"].Item2] = string.Concat(p.Inventory.MinQtyAllowInCart);
                        }
                    }
                    if (headDicTmp.ContainsKey("ABN"))
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["ABN"].Item2] = string.Concat(p.Inventory.MaxQtyAllowInCart);
                        }
                    }
                    if (headDicTmp.ContainsKey("ABO"))
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["ABO"].Item2] = Constant.STOCK_TYPE.Where(w => w.Value.Equals(p.Inventory.StockType)).SingleOrDefault().Key;
                        }
                    }
                    if (headDicTmp.ContainsKey("ABP"))
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["ABP"].Item2] = string.Concat(p.Inventory.MaxQtyPreOrder);
                        }
                    }
                    if (headDicTmp.ContainsKey("ABQ"))
                    {
                        bodyList[headDicTmp["ABQ"].Item2] = Constant.STATUS_YES.Equals(p.IsHasExpiryDate) ? "Yes" : "No";
                    }
                    #endregion
                    #region Shipping Detail
                    if (headDicTmp.ContainsKey("ABR"))
                    {
                        bodyList[headDicTmp["ABR"].Item2] = p.ProductStageGroup.ShippingMethodEn;
                    }
                    if (headDicTmp.ContainsKey("ABS"))
                    {
                        bodyList[headDicTmp["ABS"].Item2] = Constant.STATUS_YES.Equals(p.ProductStageGroup.ExpressDelivery) ? "Yes" : "No";
                    }
                    if (headDicTmp.ContainsKey("ABT"))
                    {
                        bodyList[headDicTmp["ABT"].Item2] = string.Concat(p.ProductStageGroup.DeliveryFee);
                    }

                    if (headDicTmp.ContainsKey("ABU"))
                    {
                        bodyList[headDicTmp["ABU"].Item2] = string.Concat(p.PrepareDay);
                    }
                    if (headDicTmp.ContainsKey("ABV"))
                    {
                        bodyList[headDicTmp["ABV"].Item2] = string.Concat(p.PrepareMon);
                    }
                    if (headDicTmp.ContainsKey("ABW"))
                    {
                        bodyList[headDicTmp["ABW"].Item2] = string.Concat(p.PrepareTue);
                    }
                    if (headDicTmp.ContainsKey("ABX"))
                    {
                        bodyList[headDicTmp["ABX"].Item2] = string.Concat(p.PrepareWed);
                    }
                    if (headDicTmp.ContainsKey("ABY"))
                    {
                        bodyList[headDicTmp["ABY"].Item2] = string.Concat(p.PrepareThu);
                    }
                    if (headDicTmp.ContainsKey("ABZ"))
                    {
                        bodyList[headDicTmp["ABZ"].Item2] = string.Concat(p.PrepareFri);
                    }
                    if (headDicTmp.ContainsKey("ACA"))
                    {
                        bodyList[headDicTmp["ACA"].Item2] = string.Concat(p.PrepareSat);
                    }
                    if (headDicTmp.ContainsKey("ACB"))
                    {
                        bodyList[headDicTmp["ACB"].Item2] = string.Concat(p.PrepareSun);
                    }
                    if (headDicTmp.ContainsKey("ACC"))
                    {
                        bodyList[headDicTmp["ACC"].Item2] = string.Concat(p.Length);
                    }
                    if (headDicTmp.ContainsKey("ACD"))
                    {
                        bodyList[headDicTmp["ACD"].Item2] = string.Concat(p.Height);
                    }
                    if (headDicTmp.ContainsKey("ACE"))
                    {
                        bodyList[headDicTmp["ACE"].Item2] = string.Concat(p.Width);
                    }
                    if (headDicTmp.ContainsKey("ACF"))
                    {
                        bodyList[headDicTmp["ACF"].Item2] = string.Concat(p.Weight);
                    }
                    #endregion
                    #region Category
                    if (headDicTmp.ContainsKey("ACG"))
                    {
                        bodyList[headDicTmp["ACG"].Item2] = string.Concat(p.ProductStageGroup.GlobalCatId);
                    }
                    if (headDicTmp.ContainsKey("ACH"))
                    {
                        if (p.ProductStageGroup.ProductStageGlobalCatMaps != null && p.ProductStageGroup.ProductStageGlobalCatMaps.ToList().Count > 0)
                        {
                            bodyList[headDicTmp["ACH"].Item2] = string.Concat(p.ProductStageGroup.ProductStageGlobalCatMaps.ToList()[0]);
                        }
                    }
                    if (headDicTmp.ContainsKey("ACI"))
                    {
                        if (p.ProductStageGroup.ProductStageGlobalCatMaps != null && p.ProductStageGroup.ProductStageGlobalCatMaps.ToList().Count > 1)
                        {
                            bodyList[headDicTmp["ACI"].Item2] = string.Concat(p.ProductStageGroup.ProductStageGlobalCatMaps.ToList()[1]);
                        }
                    }
                    if (headDicTmp.ContainsKey("ACJ"))
                    {
                        bodyList[headDicTmp["ACJ"].Item2] = string.Concat(p.ProductStageGroup.LocalCatId);
                    }
                    if (headDicTmp.ContainsKey("ACK"))
                    {
                        if (p.ProductStageGroup.ProductStageLocalCatMaps != null && p.ProductStageGroup.ProductStageLocalCatMaps.ToList().Count > 0)
                        {
                            bodyList[headDicTmp["ACK"].Item2] = string.Concat(p.ProductStageGroup.ProductStageLocalCatMaps.ToList()[0]);
                        }
                    }
                    if (headDicTmp.ContainsKey("ACL"))
                    {
                        if (p.ProductStageGroup.ProductStageLocalCatMaps != null && p.ProductStageGroup.ProductStageLocalCatMaps.ToList().Count > 1)
                        {
                            bodyList[headDicTmp["ACL"].Item2] = string.Concat(p.ProductStageGroup.ProductStageLocalCatMaps.ToList()[1]);
                        }
                    }
                    #endregion
                    #region Relationship
                    if (headDicTmp.ContainsKey("ACM"))
                    {
                        if (p.ProductStageGroup.ProductStageRelateds1 != null && p.ProductStageGroup.ProductStageRelateds1.ToList().Count > 0)
                        {
                            var pids = p.ProductStageGroup.ProductStageRelateds1.SelectMany(s=>s);
                            bodyList[headDicTmp["ACM"].Item2] = string.Join(",", pids);
                        }
                    }
                    #endregion
                    #region SEO
                    if (headDicTmp.ContainsKey("ACN"))
                    {
                        bodyList[headDicTmp["ACN"].Item2] = p.SeoEn;
                    }
                    if (headDicTmp.ContainsKey("ACO"))
                    {
                        bodyList[headDicTmp["ACO"].Item2] = p.SeoTh;
                    }

                    if (headDicTmp.ContainsKey("ACP"))
                    {
                        bodyList[headDicTmp["ACP"].Item2] = p.MetaTitleEn;
                    }
                    if (headDicTmp.ContainsKey("ACQ"))
                    {
                        bodyList[headDicTmp["ACQ"].Item2] = p.MetaTitleTh;
                    }
                    if (headDicTmp.ContainsKey("ACR"))
                    {
                        bodyList[headDicTmp["ACR"].Item2] = p.MetaDescriptionEn;
                    }
                    if (headDicTmp.ContainsKey("ACS"))
                    {
                        bodyList[headDicTmp["ACS"].Item2] = p.MetaDescriptionTh;
                    }
                    if (headDicTmp.ContainsKey("ACT"))
                    {
                        bodyList[headDicTmp["ACT"].Item2] = p.MetaKeyEn;
                    }
                    if (headDicTmp.ContainsKey("ACU"))
                    {
                        bodyList[headDicTmp["ACU"].Item2] = p.MetaKeyTh;
                    }
                    if (headDicTmp.ContainsKey("ACV"))
                    {
                        bodyList[headDicTmp["ACV"].Item2] = p.UrlKey;
                    }
                    if (headDicTmp.ContainsKey("ACW"))
                    {
                        bodyList[headDicTmp["ACW"].Item2] = string.Concat(p.BoostWeight);
                    }
                    if (headDicTmp.ContainsKey("ACX") && User.ShopRequest() == null)
                    {
                        bodyList[headDicTmp["ACX"].Item2] = string.Concat(p.GlobalBoostWeight);
                    }
                    #endregion
                    #region More Detail
                    if (headDicTmp.ContainsKey("ACY"))
                    {
                        if (p.ProductStageGroup.EffectiveDate != null)
                        {
                            bodyList[headDicTmp["ACY"].Item2] = p.ProductStageGroup.EffectiveDate.ToString(Constant.DATETIME_FORMAT);
                        }
                    }
                    if (headDicTmp.ContainsKey("ACZ"))
                    {
                        if (p.ProductStageGroup.ExpireDate != null)
                        {
                            bodyList[headDicTmp["ACZ"].Item2] = p.ProductStageGroup.ExpireDate.ToString(Constant.DATETIME_FORMAT);
                        }
                    }
                    if (headDicTmp.ContainsKey("ADA"))
                    {
                        bodyList[headDicTmp["ADA"].Item2] = Constant.STATUS_YES.Equals(p.ProductStageGroup.GiftWrap) ? "Yes" : "No" ;
                    }
                    if (headDicTmp.ContainsKey("ADB"))
                    {
                        if (p.ProductStageGroup.NewArrivalDate != null)
                        {
                            bodyList[headDicTmp["ADB"].Item2] = p.ProductStageGroup.NewArrivalDate.Value.ToString(Constant.DATETIME_FORMAT);
                        }
                    }
                    if (headDicTmp.ContainsKey("ADC"))
                    {
                        bodyList[headDicTmp["ADC"].Item2] = p.ProductStageGroup.IsNew == true ? "Yes" : "No";
                    }
                    
                    if (headDicTmp.ContainsKey("ADD"))
                    {
                        bodyList[headDicTmp["ADD"].Item2] = p.ProductStageGroup.IsClearance == true ? "Yes" : "No";
                    }
                    if (headDicTmp.ContainsKey("ADE"))
                    {
                        bodyList[headDicTmp["ADE"].Item2] = p.ProductStageGroup.IsBestSeller == true ? "Yes" : "No";
                    }

                    if (headDicTmp.ContainsKey("ADF"))
                    {
                        bodyList[headDicTmp["ADF"].Item2] = p.ProductStageGroup.IsOnlineExclusive == true ? "Yes" : "No";
                    }
                    if (headDicTmp.ContainsKey("ADG"))
                    {
                        bodyList[headDicTmp["ADG"].Item2] = p.ProductStageGroup.IsOnlyAt == true ? "Yes" : "No";
                    }
                    if (headDicTmp.ContainsKey("ADH"))
                    {
                        bodyList[headDicTmp["ADH"].Item2] = p.ProductStageGroup.Remark;
                    }
                    #endregion
                    #region Attibute Section
                    if (request.AttributeSets != null && request.AttributeSets.Count > 0)
                    {
                        if (p.ProductStageGroup.AttributeSet != null)
                        {
                            var set = request.AttributeSets.Where(w => w.AttributeSetId == p.ProductStageGroup.AttributeSet.AttributeSetId).SingleOrDefault();
                            if (set != null)
                            {
                                //make header for attribute
                                foreach (var attr in p.ProductStageGroup.AttributeSet.AttributeSetMaps.Select(s=>s.Attribute))
                                {
                                    if (!headDicTmp.ContainsKey(attr.AttributeNameEn))
                                    {
                                        headDicTmp.Add(attr.AttributeNameEn, new Tuple<string, int>(attr.AttributeNameEn, i++));
                                        bodyList.Add(string.Empty);
                                    }
                                }

                                bodyList[headDicTmp["ADI"].Item2] = p.ProductStageGroup.AttributeSet.AttributeSetNameEn;
                                //make vaiant option 1 value
                                if (p.IsVariant && p.ProductStageAttributes != null && p.ProductStageAttributes.ToList().Count > 0)
                                {
                                    bodyList[headDicTmp["ADJ"].Item2] = p.ProductStageAttributes.ToList()[0].Attribute.AttributeNameEn;
                                }
                                //make vaiant option 2 value
                                if (p.IsVariant && p.ProductStageAttributes != null && p.ProductStageAttributes.ToList().Count > 1)
                                {
                                    bodyList[headDicTmp["ADK"].Item2] = p.ProductStageAttributes.ToList()[1].Attribute.AttributeNameEn;
                                }
                                //make master attribute value
                                if (!p.IsVariant && masterAttribute != null && masterAttribute.ToList().Count > 0)
                                {
                                    foreach (var masterValue in masterAttribute)
                                    {
                                        
                                        if (headDicTmp.ContainsKey(masterValue.Attribute.AttributeNameEn))
                                        {
                                            int desColumn = headDicTmp[masterValue.Attribute.AttributeNameEn].Item2;
                                            for (int j = bodyList.Count; j <= desColumn; j++)
                                            {
                                                bodyList.Add(string.Empty);
                                            }
                                            
                                            if (Constant.DATA_TYPE_LIST.Equals(masterValue.Attribute.DataType))
                                            {
                                                var tmpValue = masterValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == masterValue.ValueEn).Select(s=>s.AttributeValue.AttributeValueEn).SingleOrDefault();
                                                bodyList[desColumn] = tmpValue;
                                            }
                                            else if (Constant.DATA_TYPE_CHECKBOX.Equals(masterValue.Attribute.DataType))
                                            {
                                                var tmpValue = masterValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == masterValue.ValueEn).Select(s=>s.AttributeValue.AttributeValueEn).SingleOrDefault();
                                                if (masterValue.CheckboxValue)
                                                {
                                                    if (string.IsNullOrWhiteSpace(bodyList[desColumn]))
                                                    {
                                                        bodyList[desColumn] = tmpValue;
                                                    }
                                                    else
                                                    {
                                                        bodyList[desColumn] = string.Concat(bodyList[desColumn], ",", tmpValue);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bodyList[desColumn] = string.Concat(masterValue.ValueEn,";", masterValue.ValueTh);
                                            }

                                        }
                                    }
                                }
                                if (p.IsVariant && p.ProductStageAttributes != null && p.ProductStageAttributes.ToList().Count > 0)
                                {
                                    foreach (var variantValue in p.ProductStageAttributes)
                                    {
                                        if (headDicTmp.ContainsKey(variantValue.Attribute.AttributeNameEn))
                                        {
                                            int desColumn = headDicTmp[variantValue.Attribute.AttributeNameEn].Item2;
                                            for (int j = bodyList.Count; j <= desColumn; j++)
                                            {
                                                bodyList.Add(string.Empty);
                                            }
                                            
                                            if (Constant.DATA_TYPE_LIST.Equals(variantValue.Attribute.DataType))
                                            {
                                                var tmpValue = variantValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == variantValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
                                                bodyList[desColumn] = tmpValue;
                                            }
                                            else if (Constant.DATA_TYPE_CHECKBOX.Equals(variantValue.Attribute.DataType))
                                            {
                                                var tmpValue = variantValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == variantValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
                                                if (variantValue.CheckboxValue)
                                                {
                                                    if (string.IsNullOrWhiteSpace(bodyList[desColumn]))
                                                    {
                                                        bodyList[desColumn] = tmpValue;
                                                    }
                                                    else
                                                    {
                                                        bodyList[desColumn] = string.Concat(bodyList[desColumn], ",", tmpValue);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bodyList[desColumn] = string.Concat(variantValue.ValueEn,";", variantValue.ValueTh);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region Default Attribute
                    //make default attribute value
                    if (defaultAttribute != null && defaultAttribute.ToList().Count > 0)
                    {
                        foreach (var defaultValue in defaultAttribute)
                        {
                            //avoid admin attribute
                            if (!headDicTmp.ContainsKey(defaultValue.Attribute.AttributeNameEn))
                            {
                                continue;
                            }
                            int desColumn = headDicTmp[defaultValue.Attribute.AttributeNameEn].Item2;
                            for (int j = bodyList.Count; j <= desColumn; j++)
                            {
                                bodyList.Add(string.Empty);
                            }
                            if (Constant.DATA_TYPE_LIST.Equals(defaultValue.Attribute.DataType))
                            {
                                var tmpValue = defaultValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == defaultValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
                                bodyList[desColumn] = tmpValue;
                            }
                            else if (Constant.DATA_TYPE_CHECKBOX.Equals(defaultValue.Attribute.DataType))
                            {
                                var tmpValue = defaultValue.Attribute.AttributeValueMaps.Where(w => w.AttributeValue.MapValue == defaultValue.ValueEn).Select(s => s.AttributeValue.AttributeValueEn).SingleOrDefault();
                                if (defaultValue.CheckboxValue)
                                {
                                    if (string.IsNullOrWhiteSpace(bodyList[desColumn]))
                                    {
                                        bodyList[desColumn] = tmpValue;
                                    }
                                    else
                                    {
                                        bodyList[desColumn] = string.Concat(bodyList[desColumn], ",", tmpValue);
                                    }
                                }
                            }
                            else
                            {
                                bodyList[desColumn] = string.Concat(defaultValue.ValueEn ,";",defaultValue.ValueTh);
                            }
                        }
                    }
                    #endregion
                    rs.Add(bodyList);
                }
                #region Write header
                stream = new MemoryStream();
                writer = new StreamWriter(stream, Encoding.UTF8);
                var csv = new CsvWriter(writer);
                string headers = string.Empty;
                foreach (KeyValuePair<string, Tuple<string, int>> entry in headDicTmp)
                {
                    csv.WriteField(entry.Value.Item1);
                }
                csv.NextRecord();
                #endregion
                #region Write body
                foreach (List<string> r in rs)
                {
                    foreach (string field in r)
                    {
                        csv.WriteField(field);
                    }
                    csv.NextRecord();
                }
                #endregion
                #region Create Response
                writer.Flush();
                stream.Position = 0;

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "file.csv";
                #endregion
                return result;

            }
            catch (Exception e)
            {
                #region close writer
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Import")]
        [HttpPost]
        public async Task<HttpResponseMessage> ImportProduct()
        {
            string fileName = string.Empty;
            HashSet<string> errorMessage = new HashSet<string>();
            int row = 2;
            try
            {
                #region Validate Request
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Content Multimedia");
                }
                string root = HttpContext.Current.Server.MapPath("~/Import");
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
                {
                    throw new Exception("No file uploaded");
                }
                fileName = streamProvider.FileData[0].LocalFileName;
                #endregion
                Dictionary<string, ProductStageGroup> groupList = SetupImport(fileName, errorMessage, row, db);
                #region Validate Error Message
                if (errorMessage.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
                }
                #endregion
                #region Setup Product for database
                foreach (var product in groupList)
                {
                    var masterVariant = product.Value.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
                    if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
                       && !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
                       && product.Value.BrandId != null
                       && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullEn)
                       && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullTh))
                    {
                        product.Value.InfoFlag = true;
                    }
                    else
                    {
                        product.Value.InfoFlag = false;
                    }

                    masterVariant.VariantCount = product.Value.ProductStages.Where(w => w.IsVariant == true).Count();
                    if(masterVariant.VariantCount == 0)
                    {
                        masterVariant.IsSell = true;
                    }
                    AutoGenerate.GeneratePid(db, product.Value.ProductStages);
                    product.Value.ProductId = db.GetNextProductStageGroupId().SingleOrDefault().Value;
                    db.ProductStageGroups.Add(product.Value);
                }
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK, "Total " + groupList.Count + " products updated");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        [Route("api/ProductStages/Publish")]
        [HttpPost]
        public HttpResponseMessage PublishProduct(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var productList = db.ProductStageGroups.Where(w => true);

                if (User.ShopRequest() != null)
                {
                    int shopId = User.ShopRequest().ShopId;
                    productList = productList.Where(w => w.ShopId == shopId);
                }
                var ids = request.Where(w => w.ProductId != 0).Select(s => s.ProductId);
                productList = productList.Where(w => ids.Any(a=>a==w.ProductId)).Include(i=>i.ProductStages);
                string email = User.UserRequest().Email;
                var currentDt = DateTime.Now;
                foreach (ProductStageRequest rq in request)
                {
                    var current = productList.Where(w => w.ProductId.Equals(rq.ProductId)).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception(string.Concat("Cannot find product " , rq.ProductId));
                    }
                    if (!current.Status.Equals(Constant.PRODUCT_STATUS_DRAFT))
                    {
                        throw new Exception(string.Concat("Only product with draft status and complete Info & Image tab can be published"));
                    }

                    if (current.ProductStages.Any(a=> string.IsNullOrWhiteSpace(a.ProductNameEn))  
                        || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.ProductNameTh))
                        || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.Sku))
                        || current.BrandId == null
                        || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.DescriptionFullEn))
                        || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.DescriptionFullTh))
                        || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.MobileDescriptionEn))
                        || current.ProductStages.Any(a => string.IsNullOrWhiteSpace(a.MobileDescriptionTh)))
                    {
                        throw new Exception(string.Concat("ProudctId ", rq.ProductId, " is not ready for publishing"));
                    }

                    current.Status = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                    current.UpdateBy = email;
                    current.UpdateOn = currentDt;
                    current.ProductStages.ToList().ForEach(e =>
                    {
                        e.Status = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                        e.UpdateBy = email;
                        e.UpdateOn = currentDt;
                    });
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK, "Published success");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private List<List<string>> ReadExcel(CsvReader csvResult, string[] header, List<string> firstRow)
        {
            List<List<string>> listRow = new List<List<string>>() { firstRow };
            List<string> listColumn = null;
            while (csvResult.Read())
            {
                listColumn = new List<string>();
                foreach (string h in header)
                {
                    listColumn.Add(csvResult.GetField<string>(h));
                }
                listRow.Add(listColumn);
            }
            return listRow;
        }

        private Dictionary<string, ProductStageGroup> SetupImport(string fileName, HashSet<string> errorMessage, int row, ColspEntities db, bool isUpdate = false, HashSet<string> updateHeader = null)
        {
            using (var fileReader = File.OpenText(fileName))
            {
                Dictionary<string, ProductStageGroup> groupList = null;
                using (var csvResult = new CsvReader(fileReader))
                {
                    if (!csvResult.Read())
                    {
                        throw new Exception("File is not in a proper format");
                    }
                    #region Header
                    var tmpGuidance = db.ImportHeaders.Where(w => true);
                    if (User.ShopRequest() != null && User.UserRequest().IsAdmin == false)
                    {
                        var groupName = User.ShopRequest().ShopGroup;
                        tmpGuidance = tmpGuidance.Where(w => w.ShopGroups.Any(a => a.Abbr.Equals(groupName)));
                    }
                    var guidance = tmpGuidance.Select(s => new ImportHeaderRequest()
                    {
                        HeaderName = s.HeaderName,
                        MapName = s.MapName,
                    }).ToList();
                    Dictionary<string, int> headDic = new Dictionary<string, int>();
                    IEnumerable<IEnumerable<string>> csvRows = null;
                    int i = 0;
                    string[] headers = csvResult.FieldHeaders;
                    List<string> firstRow = new List<string>();
                    foreach (string head in headers)
                    {
                        if (headDic.ContainsKey(head))
                        {
                            throw new Exception(head + " is duplicate header");
                        }
                        var headerGuidance = guidance.Where(w => w.HeaderName.Equals(head)).Select(s=>s.MapName).FirstOrDefault();
                        if (string.IsNullOrEmpty(headerGuidance))
                        {
                            headDic.Add(head, i++);
                        }
                        else
                        {
                            headDic.Add(headerGuidance, i++);
                        }
                        
                        firstRow.Add(csvResult.GetField<string>(head));
                        if (isUpdate)
                        {
                            updateHeader.Add(headerGuidance);
                        }
                    }
                    #endregion
                    csvRows = ReadExcel(csvResult, headers, firstRow);
                    List<ProductStage> products = new List<ProductStage>();
                    #region Default Query
                    int shopId = User.ShopRequest().ShopId;
                    var brands = db.Brands.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE)).Select(s => new { BrandNameEn = s.BrandNameEn, BrandId = s.BrandId }).ToList();
                    var globalCatId = db.GlobalCategories.Where(w => w.Rgt - w.Lft == 1).Select(s => new { CategoryId = s.CategoryId }).ToList();
                    var localCatId = db.LocalCategories.Where(w => w.Rgt - w.Lft == 1 && w.ShopId == shopId).Select(s => new { CategoryId = s.CategoryId }).ToList();
                    var attributeSet = db.AttributeSets
                        .Where(w => w.Status.Equals(Constant.STATUS_ACTIVE))
                        .Select(s => new
                        {
                            s.AttributeSetId,
                            s.AttributeSetNameEn,
                            Attribute = s.AttributeSetMaps.Select(se => new
                            {
                                se.Attribute.AttributeId,
                                se.Attribute.AttributeNameEn,
                                se.Attribute.VariantStatus,
                                se.Attribute.DataType,
                                AttributeValue = se.Attribute.AttributeValueMaps.Select(sv => new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn })
                            })
                        }).ToList();

                    var tmpDefaultAttribute = db.Attributes.Where(w => w.DefaultAttribute == true);
                    if (User.ShopRequest() != null)
                    {
                        tmpDefaultAttribute = tmpDefaultAttribute.Where(w => Constant.ATTRIBUTE_VISIBLE_ALL_USER.Equals(w.VisibleTo));
                    }
                    var defaultAttribute = tmpDefaultAttribute
                        .Select(se => new
                        {
                            se.AttributeId,
                            se.AttributeNameEn,
                            se.VariantStatus,
                            se.DataType,
                            AttributeValue = se.AttributeValueMaps.Select(sv => new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn })
                        }).ToList();
                    var relatedProduct = db.ProductStages.Where(w => w.ShopId == shopId && w.IsVariant == false).Select(s => new { s.Pid, s.ProductId }).ToList();
                    var shipping = db.Shippings.ToList();
                    #endregion
                    #region Initialize
                    Dictionary<Tuple<string, int>, Inventory> inventoryList = new Dictionary<Tuple<string, int>, Inventory>();
                    groupList = new Dictionary<string, ProductStageGroup>();
                    int tmpGroupId = 0;
                    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                    List<string> body = null;
                    string groupId = null;
                    bool isNew = true;
                    ProductStageGroup group = null;
                    ProductStage variant = null;
                    //ProductStageVariant variant = null;
                    #endregion
                    foreach (var b in csvRows)
                    {
                        body = b.ToList();
                        if (body.All(a => string.IsNullOrWhiteSpace(a)))
                        {
                            continue;
                        }
                        #region Group
                        isNew = true;
                        groupId = string.Empty;
                        group = null;
                        if (headDic.ContainsKey("AAB"))
                        {
                            //Get column 'Group Id'.
                            groupId = body[headDic["AAB"]];
                            if (rg.IsMatch(groupId))
                            {
                                errorMessage.Add("Invalid Group ID at row" + row);
                                continue;
                            }
                            if (groupList.ContainsKey(groupId))
                            {
                                group = groupList[groupId];
                                isNew = false;
                            }
                        }
                        if (group == null)
                        {
                            long productId = 0;
                            if (string.IsNullOrEmpty(groupId))
                            {
                                groupId = string.Concat("((", tmpGroupId++, "))");
                            }
                            else if (isUpdate)
                            {
                                try
                                {
                                    productId = Convert.ToInt32(groupId);
                                }
                                catch (Exception)
                                {
                                    productId = 0;
                                }
                            }
                            group = new ProductStageGroup()
                            {
                                ProductId = productId,
                                ShopId = shopId,
                                Status = Constant.PRODUCT_STATUS_DRAFT,
                                CategoryTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                                ImageTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                                InformationTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                                MoreOptionTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                                VariantTabStatus = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL,
                                ImageFlag = false,
                                InfoFlag = false,
                                OnlineFlag = false,
                                RejectReason = string.Empty,
                                TheOneCardEarn = Constant.DEFAULT_THE_ONE_CARD,
                                CreateBy = User.UserRequest().Email,
                                CreateOn = DateTime.Now,
                                UpdateBy = User.UserRequest().Email,
                                UpdateOn = DateTime.Now
                            };
                        }
                        #endregion
                        #region Variant Detail
                        //Initialise product stage variant
                        variant = new ProductStage()
                        {
                            ShopId = shopId,
                            Status = Constant.PRODUCT_STATUS_DRAFT,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                            Visibility = headDic.ContainsKey("ADL") && string.Equals(body[headDic["ADL"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false,
                            IsVariant = true,
                            IsMaster = false,
                            FeatureImgUrl = string.Empty,
                            ImageCount = 0,
                            MaxiQtyAllowed = 0,
                            MiniQtyAllowed = 0,
                            UnitPrice = 0,
                            DimensionUnit = Constant.DIMENSTION_MM,
                            WeightUnit = Constant.WEIGHT_MEASURE_G,
                            GlobalBoostWeight = 0,
                            Display = Constant.VARIANT_DISPLAY_GROUP,
                            VariantCount = 0,
                            PurchasePrice = 0,
                            SalePrice = 0,
                            OriginalPrice = 0,
                            DefaultVariant = false,
                            Bu = null,
                            DeliveryFee = 0,
                            EffectiveDatePromotion = Validation.ValidateCSVDatetimeColumn(headDic, body, "AAP", guidance, errorMessage, row),
                            ExpireDatePromotion = Validation.ValidateCSVDatetimeColumn(headDic, body, "AAQ", guidance, errorMessage, row),
                            IsSell = false,
                            JDADept = string.Empty,
                            JDASubDept = string.Empty,
                            NewArrivalDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "ADB", guidance, errorMessage, row),
                            PromotionPrice = 0,
                            OldPid = null,
                            ProductNameEn = Validation.ValidateCSVStringColumn(headDic, body, "AAE", guidance, true, 300, errorMessage, row),
                            ProductNameTh = Validation.ValidateCSVStringColumn(headDic, body, "AAF", guidance, true, 300, errorMessage, row),
                            ProdTDNameEn = Validation.ValidateCSVStringColumn(headDic, body, "AAG", guidance, false, 55, errorMessage, row, string.Empty),
                            ProdTDNameTh = Validation.ValidateCSVStringColumn(headDic, body, "AAH", guidance, false, 55, errorMessage, row, string.Empty),
                            Sku = Validation.ValidateCSVStringColumn(headDic, body, "AAI", guidance, false, 300, errorMessage, row, string.Empty),
                            Upc = Validation.ValidateCSVStringColumn(headDic, body, "AAJ", guidance, false, 300, errorMessage, row, string.Empty),
                            SaleUnitEn = Validation.ValidateCSVStringColumn(headDic, body, "AAT", guidance, false, 100, errorMessage, row, string.Empty),
                            SaleUnitTh = Validation.ValidateCSVStringColumn(headDic, body, "AAU", guidance, false, 100, errorMessage, row, string.Empty),
                            IsVat = headDic.ContainsKey("AAV") && string.Equals(body[headDic["AAV"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
                            DescriptionFullEn = Validation.ValidateCSVStringColumn(headDic, body, "AAW", guidance, false, int.MaxValue, errorMessage, row, string.Empty),
                            DescriptionFullTh = Validation.ValidateCSVStringColumn(headDic, body, "AAX", guidance, false, int.MaxValue, errorMessage, row, string.Empty),
                            MobileDescriptionEn = Validation.ValidateCSVStringColumn(headDic, body, "AAY", guidance, false, int.MaxValue, errorMessage, row, string.Empty),
                            MobileDescriptionTh = Validation.ValidateCSVStringColumn(headDic, body, "AAZ", guidance, false, int.MaxValue, errorMessage, row, string.Empty),
                            DescriptionShortEn = Validation.ValidateCSVStringColumn(headDic, body, "ABA", guidance, false, 500, errorMessage, row, string.Empty),
                            DescriptionShortTh = Validation.ValidateCSVStringColumn(headDic, body, "ABB", guidance, false, 500, errorMessage, row, string.Empty),
                            KillerPoint1En = Validation.ValidateCSVStringColumn(headDic, body, "ABC", guidance, false, 200, errorMessage, row, string.Empty),
                            KillerPoint1Th = Validation.ValidateCSVStringColumn(headDic, body, "ABD", guidance, false, 200, errorMessage, row, string.Empty),
                            KillerPoint2En = Validation.ValidateCSVStringColumn(headDic, body, "ABE", guidance, false, 200, errorMessage, row, string.Empty),
                            KillerPoint2Th = Validation.ValidateCSVStringColumn(headDic, body, "ABF", guidance, false, 200, errorMessage, row, string.Empty),
                            KillerPoint3En = Validation.ValidateCSVStringColumn(headDic, body, "ABG", guidance, false, 200, errorMessage, row, string.Empty),
                            KillerPoint3Th = Validation.ValidateCSVStringColumn(headDic, body, "ABH", guidance, false, 200, errorMessage, row, string.Empty),
                            IsHasExpiryDate = headDic.ContainsKey("ABQ") && string.Equals(body[headDic["ABQ"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
                            ExpressDelivery = headDic.ContainsKey("ABS") && string.Equals(body[headDic["ABS"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
                            SeoEn = Validation.ValidateCSVStringColumn(headDic, body, "ACN", guidance, false, 300, errorMessage, row, string.Empty),
                            SeoTh = Validation.ValidateCSVStringColumn(headDic, body, "ACO", guidance, false, 300, errorMessage, row, string.Empty),
                            MetaTitleEn = Validation.ValidateCSVStringColumn(headDic, body, "ACP", guidance, false, 300, errorMessage, row, string.Empty),
                            MetaTitleTh = Validation.ValidateCSVStringColumn(headDic, body, "ACQ", guidance, false, 300, errorMessage, row, string.Empty),
                            MetaDescriptionEn = Validation.ValidateCSVStringColumn(headDic, body, "ACR", guidance, false, 500, errorMessage, row, string.Empty),
                            MetaDescriptionTh = Validation.ValidateCSVStringColumn(headDic, body, "ACS", guidance, false, 500, errorMessage, row, string.Empty),
                            MetaKeyEn = Validation.ValidateCSVStringColumn(headDic, body, "ACT", guidance, false, 300, errorMessage, row, string.Empty),
                            MetaKeyTh = Validation.ValidateCSVStringColumn(headDic, body, "ACU", guidance, false, 300, errorMessage, row, string.Empty),
                            UrlKey = Validation.ValidateCSVStringColumn(headDic, body, "ACV", guidance, false, 300, errorMessage, row),
                            //GiftWrap = string.Equals(body[headDic["Gift Wrap"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
                            Installment = headDic.ContainsKey("AAN") && string.Equals(body[headDic["AAN"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO,
                            PrepareDay = Validation.ValidateCSVIntegerColumn(headDic, body, "ABU", guidance, false, int.MaxValue, errorMessage, row, 0),
                            PrepareMon = Validation.ValidateCSVIntegerColumn(headDic, body, "ABV", guidance, false, int.MaxValue, errorMessage, row, 0),
                            PrepareTue = Validation.ValidateCSVIntegerColumn(headDic, body, "ABW", guidance, false, int.MaxValue, errorMessage, row, 0),
                            PrepareWed = Validation.ValidateCSVIntegerColumn(headDic, body, "ABX", guidance, false, int.MaxValue, errorMessage, row, 0),
                            PrepareThu = Validation.ValidateCSVIntegerColumn(headDic, body, "ABY", guidance, false, int.MaxValue, errorMessage, row, 0),
                            PrepareFri = Validation.ValidateCSVIntegerColumn(headDic, body, "ABZ", guidance, false, int.MaxValue, errorMessage, row, 0),
                            PrepareSat = Validation.ValidateCSVIntegerColumn(headDic, body, "ACA", guidance, false, int.MaxValue, errorMessage, row, 0),
                            PrepareSun = Validation.ValidateCSVIntegerColumn(headDic, body, "ACB", guidance, false, int.MaxValue, errorMessage, row, 0),
                            LimitIndividualDay = true,
                            Length = Validation.ValidateCSVIntegerColumn(headDic, body, "ACC", guidance, false, int.MaxValue, errorMessage, row, 0),
                            Height = Validation.ValidateCSVIntegerColumn(headDic, body, "ACD", guidance, false, int.MaxValue, errorMessage, row, 0),
                            Width = Validation.ValidateCSVIntegerColumn(headDic, body, "ACE", guidance, false, int.MaxValue, errorMessage, row, 0),
                            Weight = Validation.ValidateCSVIntegerColumn(headDic, body, "ACF", guidance, false, int.MaxValue, errorMessage, row, 0),
                            BoostWeight = Validation.ValidateCSVIntegerColumn(headDic, body, "ACW", guidance, false, int.MaxValue, errorMessage, row, 0),
                        };
                        if (headDic.ContainsKey("AAC"))
                        {
                            string defaultVar = body[headDic["AAC"]];
                            variant.DefaultVariant = "Yes".Equals(defaultVar);
                        }
                        if (headDic.ContainsKey("AAO"))
                        {
                            try
                            {
                                var priceSt = body[headDic["AAO"]];
                                if (!string.IsNullOrWhiteSpace(priceSt))
                                {
                                    decimal price = decimal.Parse(priceSt);
                                    variant.PromotionPrice = price;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Promotion Price at row " + row);
                            }
                        }
                        if (headDic.ContainsKey("AAR"))
                        {
                            try
                            {
                                var unitPriceSt = body[headDic["AAR"]];
                                if (!string.IsNullOrWhiteSpace(unitPriceSt))
                                {
                                    decimal unitPrice = decimal.Parse(unitPriceSt);
                                    variant.UnitPrice = unitPrice;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Unit Price at row " + row);
                            }
                        }
                        if (headDic.ContainsKey("AAS"))
                        {
                            try
                            {
                                var purchasePriceSt = body[headDic["AAS"]];
                                if (!string.IsNullOrWhiteSpace(purchasePriceSt))
                                {
                                    decimal purchasePrice = decimal.Parse(purchasePriceSt);
                                    variant.PurchasePrice = purchasePrice;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Purchase Price at row " + row);
                            }
                        }
                        if (isUpdate)
                        {
                            if (headDic.ContainsKey("AAD"))
                            {
                                variant.Pid = Validation.ValidateCSVStringColumn(headDic, body, "AAD", guidance, false, 7, errorMessage, row,string.Empty);
                            }
                            else
                            {
                               throw new Exception("No PID column found");
                            }
                        }
                        #endregion
                        #region Delivery Fee
                        if (headDic.ContainsKey("ABT"))
                        {
                            try
                            {
                                var feeSt = body[headDic["ABT"]];
                                if (!string.IsNullOrWhiteSpace(feeSt))
                                {
                                    decimal fee = decimal.Parse(feeSt);
                                    variant.DeliveryFee = fee;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Delivery Fee at row " + row);
                            }
                        }
                        #endregion
                        #region Original Price
                        if (headDic.ContainsKey("AAM"))
                        {
                            try
                            {
                                var originalPriceSt = body[headDic["AAM"]];
                                if (!string.IsNullOrWhiteSpace(originalPriceSt))
                                {
                                    decimal originalPrice = decimal.Parse(originalPriceSt);
                                    variant.OriginalPrice = originalPrice;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Original Price at row " + row);
                            }
                        }
                        #endregion
                        #region Sale Price
                        if (headDic.ContainsKey("AAL"))
                        {
                            try
                            {
                                var salePriceSt = body[headDic["AAL"]];
                                if (!string.IsNullOrWhiteSpace(salePriceSt))
                                {
                                    decimal salePrice = decimal.Parse(salePriceSt);
                                    variant.SalePrice = salePrice;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Sale Price at row " + row);
                            }
                        }
                        #endregion
                        #region Inventory Amount
                        if (headDic.ContainsKey("ABJ"))
                        {
                            try
                            {
                                string val = body[headDic["ABJ"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }

                                    }
                                    variant.Inventory.Quantity = variant.Inventory.Quantity + int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Inventory Amount at row " + row);
                            }
                        }
                        #endregion
                        #region Safety Stock Amount
                        if (headDic.ContainsKey("ABL"))
                        {
                            try
                            {
                                string val = body[headDic["ABL"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }
                                    }
                                    variant.Inventory.SafetyStockSeller = int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Safety Stock Amount at row " + row);
                            }
                        }
                        #endregion
                        #region Minimum Quantity Allowed in Cart
                        if (headDic.ContainsKey("ABM"))
                        {
                            try
                            {
                                string val = body[headDic["ABM"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }

                                    }
                                    variant.Inventory.MinQtyAllowInCart = int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Minimum Quantity Allowed in Cart at row " + row);
                            }
                        }
                        #endregion
                        #region Maximum Quantity Allowed in Cart
                        if (headDic.ContainsKey("ABN"))
                        {
                            try
                            {
                                string val = body[headDic["ABN"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }

                                    }
                                    variant.Inventory.MaxQtyAllowInCart = int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Maximum Quantity Allowed in Cart at row " + row);
                            }
                        }
                        #endregion
                        #region Maximum Quantity Allow for Preorder
                        if (headDic.ContainsKey("ABP"))
                        {
                            try
                            {
                                string val = body[headDic["ABP"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }

                                    }
                                    variant.Inventory.MaxQtyPreOrder = int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
                            }
                        }
                        #endregion
                        #region  On Hold
                        if (headDic.ContainsKey("ADN"))
                        {
                            try
                            {
                                string val = body[headDic["ADN"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }

                                    }
                                    variant.Inventory.OnHold = int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
                            }
                        }
                        #endregion
                        #region  Reserve
                        if (headDic.ContainsKey("ADO"))
                        {
                            try
                            {
                                string val = body[headDic["ADO"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }

                                    }
                                    variant.Inventory.Reserve = int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
                            }
                        }
                        #endregion
                        #region  Defect
                        if (headDic.ContainsKey("ADP"))
                        {
                            try
                            {
                                string val = body[headDic["ADP"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (variant.Inventory == null)
                                    {
                                        variant.Inventory = new Inventory()
                                        {
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["ABO"]]))
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic["ABO"]]];
                                        }
                                        else
                                        {
                                            variant.Inventory.StockType = Constant.STOCK_TYPE[body[headDic[Constant.DEFAULT_STOCK_TYPE]]];
                                        }

                                    }
                                    variant.Inventory.Defect = int.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Maximum Quantity Allow for Preorder at row " + row);
                            }
                        }
                        #endregion


                        if (variant.DefaultVariant || isNew)
                        {
                            variant.DefaultVariant = true;
                            #region Relate Product
                            if (headDic.ContainsKey("ACM"))
                            {
                                var tmpRelateProduct = body[headDic["ACM"]].Split(',');
                                foreach (var pro in tmpRelateProduct)
                                {
                                    if (string.IsNullOrWhiteSpace(pro))
                                    {
                                        continue;
                                    }
                                    var child = relatedProduct.Where(w => w.Pid.Equals(pro.Trim())).Select(s => s.ProductId).SingleOrDefault();
                                    if(child != 0)
                                    {
                                        if (!group.ProductStageRelateds1.Any(a => a.Child == child))
                                        {
                                            group.ProductStageRelateds1.Add(new ProductStageRelated()
                                            {
                                                Child = child,
                                                CreateBy = User.UserRequest().Email,
                                                CreateOn = DateTime.Now,
                                                UpdateBy = User.UserRequest().Email,
                                                UpdateOn = DateTime.Now,
                                            });
                                        }
                                    }
                                    else
                                    {
                                        errorMessage.Add("Cannot find related product " + pro + " at row " + row);
                                    }
                                }
                            }
                            #endregion
                            #region Brand 
                            if (headDic.ContainsKey("AAK"))
                            {
                                if (!string.IsNullOrWhiteSpace(body[headDic["AAK"]]))
                                {
                                    var brandId = brands.Where(w => string.Equals(w.BrandNameEn, body[headDic["AAK"]] , StringComparison.OrdinalIgnoreCase)).Select(s => s.BrandId).FirstOrDefault();
                                    if (brandId != 0)
                                    {
                                        group.BrandId = brandId;
                                    }
                                    else
                                    {
                                        errorMessage.Add("Invalid Brand Name at row " + row);
                                    }
                                }
                            }
                            #endregion
                            #region Global category
                            if (headDic.ContainsKey("ACG"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["ACG"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = int.Parse(catIdSt);
                                        var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            group.GlobalCatId = cat;
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }else if (!isUpdate)
                                    {
                                        errorMessage.Add("Global Category ID is required at row " + row);
                                    }
                                }
                                catch (Exception)
                                {
                                    errorMessage.Add("Invalid Global Category ID at row " + row);
                                }
                            }
                            else if(!isUpdate)
                            {
                                throw new Exception("Global Category field is required");
                            }
                            if (headDic.ContainsKey("ACH"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["ACH"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = int.Parse(catIdSt);
                                        var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            if (!group.ProductStageGlobalCatMaps.Any(a => a.CategoryId == cat))
                                            {
                                                group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                                                {
                                                    CategoryId = cat,
                                                    CreateBy = User.UserRequest().Email,
                                                    CreateOn = DateTime.Now,
                                                    UpdateBy = User.UserRequest().Email,
                                                    UpdateOn = DateTime.Now,
                                                });
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    errorMessage.Add("Invalid 1st Alternative Global Category at row " + row);
                                }
                            }
                            if (headDic.ContainsKey("ACI"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["ACI"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = int.Parse(catIdSt);
                                        var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            if (!group.ProductStageGlobalCatMaps.Any(a => a.CategoryId == cat))
                                            {
                                                group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                                                {
                                                    CategoryId = cat,
                                                    CreateBy = User.UserRequest().Email,
                                                    CreateOn = DateTime.Now,
                                                    UpdateBy = User.UserRequest().Email,
                                                    UpdateOn = DateTime.Now,
                                                });
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    errorMessage.Add("Invalid 2nd Alternative Global Category at row " + row);
                                }
                            }

                            #endregion
                            #region Local Category
                            if (headDic.ContainsKey("ACJ"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["ACJ"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = int.Parse(catIdSt);
                                        var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            group.LocalCatId = cat;
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch
                                {
                                    errorMessage.Add("Invalid Local Category ID at row " + row);
                                }
                            }
                            if (headDic.ContainsKey("ACK"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["ACK"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = int.Parse(catIdSt);
                                        var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            if (!group.ProductStageLocalCatMaps.Any(a => a.CategoryId == cat))
                                            {
                                                group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                                                {
                                                    CategoryId = cat,
                                                    CreateBy = User.UserRequest().Email,
                                                    CreateOn = DateTime.Now,
                                                    UpdateBy = User.UserRequest().Email,
                                                    UpdateOn = DateTime.Now,
                                                });
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    errorMessage.Add("Invalid 1st Alternative Local Category at row " + row);
                                }
                            }
                            if (headDic.ContainsKey("ACL"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["ACL"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = int.Parse(catIdSt);
                                        var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            if (!group.ProductStageLocalCatMaps.Any(a => a.CategoryId == cat))
                                            {
                                                group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                                                {
                                                    CategoryId = cat,
                                                    CreateBy = User.UserRequest().Email,
                                                    CreateOn = DateTime.Now,
                                                    UpdateBy = User.UserRequest().Email,
                                                    UpdateOn = DateTime.Now,
                                                });
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    errorMessage.Add("Invalid 2nd Alternative Local Category at row " + row);
                                }
                            }
                            #endregion
                            #region Master Variant

                            ProductStage masterVariant = group.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
                            if (masterVariant == null)
                            {
                                masterVariant = new ProductStage();
                                group.ProductStages.Add(masterVariant);
                            }

                            masterVariant.ShopId = variant.ShopId;
                            masterVariant.Status = variant.Status;
                            masterVariant.CreateBy = variant.CreateBy;
                            masterVariant.CreateOn = variant.CreateOn;
                            masterVariant.UpdateBy = variant.UpdateBy;
                            masterVariant.UpdateOn = variant.UpdateOn;
                            masterVariant.Visibility = variant.Visibility;
                            masterVariant.IsVariant = false;
                            masterVariant.FeatureImgUrl = variant.FeatureImgUrl;
                            masterVariant.ImageCount = variant.ImageCount;
                            masterVariant.MaxiQtyAllowed = variant.MaxiQtyAllowed;
                            masterVariant.MiniQtyAllowed = variant.MiniQtyAllowed;
                            masterVariant.UnitPrice = variant.UnitPrice;
                            masterVariant.DimensionUnit = variant.DimensionUnit;
                            masterVariant.WeightUnit = variant.WeightUnit;
                            masterVariant.GlobalBoostWeight = variant.GlobalBoostWeight;
                            masterVariant.Display = variant.Display;
                            masterVariant.VariantCount = variant.VariantCount;
                            masterVariant.PurchasePrice = variant.PurchasePrice;
                            masterVariant.SalePrice = variant.SalePrice;
                            masterVariant.OriginalPrice = variant.OriginalPrice;
                            masterVariant.DefaultVariant = variant.DefaultVariant;
                            masterVariant.Bu = variant.Bu;
                            masterVariant.DeliveryFee = variant.DeliveryFee;
                            masterVariant.EffectiveDatePromotion = variant.EffectiveDatePromotion;
                            masterVariant.ExpireDatePromotion = variant.ExpireDatePromotion;
                            masterVariant.IsSell = variant.IsSell;
                            masterVariant.JDADept = variant.JDADept;
                            masterVariant.JDASubDept = variant.JDASubDept;
                            masterVariant.NewArrivalDate = variant.NewArrivalDate;
                            masterVariant.PromotionPrice = variant.PromotionPrice;
                            masterVariant.OldPid = variant.OldPid;
                            masterVariant.ProductNameEn = variant.ProductNameEn;
                            masterVariant.ProductNameTh = variant.ProductNameTh;
                            masterVariant.ProdTDNameEn = variant.ProdTDNameEn;
                            masterVariant.ProdTDNameTh = variant.ProdTDNameTh;
                            masterVariant.Sku = variant.Sku;
                            masterVariant.Upc = variant.Upc;
                            masterVariant.SaleUnitEn = variant.SaleUnitEn;
                            masterVariant.SaleUnitTh = variant.SaleUnitTh;
                            masterVariant.IsVat = variant.IsVat;
                            masterVariant.DescriptionFullEn = variant.DescriptionFullEn;
                            masterVariant.DescriptionFullTh = variant.DescriptionFullTh;
                            masterVariant.MobileDescriptionEn = variant.MobileDescriptionEn;
                            masterVariant.MobileDescriptionTh = variant.MobileDescriptionTh;
                            masterVariant.DescriptionShortEn = variant.DescriptionShortEn;
                            masterVariant.DescriptionShortTh = variant.DescriptionShortTh;
                            masterVariant.KillerPoint1En = variant.KillerPoint1En;
                            masterVariant.KillerPoint1Th = variant.KillerPoint1Th;
                            masterVariant.KillerPoint2En = variant.KillerPoint2En;
                            masterVariant.KillerPoint2Th = variant.KillerPoint2Th;
                            masterVariant.KillerPoint3En = variant.KillerPoint3En;
                            masterVariant.KillerPoint3Th = variant.KillerPoint3Th;
                            masterVariant.IsHasExpiryDate = variant.IsHasExpiryDate;
                            masterVariant.ExpressDelivery = variant.ExpressDelivery;
                            masterVariant.SeoEn = variant.SeoEn;
                            masterVariant.SeoTh = variant.SeoTh;
                            masterVariant.MetaTitleEn = variant.MetaTitleEn;
                            masterVariant.MetaTitleTh = variant.MetaTitleTh;
                            masterVariant.MetaDescriptionEn = variant.MetaDescriptionEn;
                            masterVariant.MetaDescriptionTh = variant.MetaDescriptionTh;
                            masterVariant.MetaKeyEn = variant.MetaKeyEn;
                            masterVariant.MetaKeyTh = variant.MetaKeyTh;
                            masterVariant.UrlKey = string.Empty;
                            masterVariant.Installment = variant.Installment;
                            masterVariant.PrepareDay = variant.PrepareDay;
                            masterVariant.PrepareMon = variant.PrepareMon;
                            masterVariant.PrepareTue = variant.PrepareTue;
                            masterVariant.PrepareWed = variant.PrepareWed;
                            masterVariant.PrepareThu = variant.PrepareThu;
                            masterVariant.PrepareFri = variant.PrepareFri;
                            masterVariant.PrepareSat = variant.PrepareSat;
                            masterVariant.PrepareSun = variant.PrepareSun;
                            masterVariant.LimitIndividualDay = variant.LimitIndividualDay;
                            masterVariant.Length = variant.Length;
                            masterVariant.Height = variant.Height;
                            masterVariant.Width = variant.Width;
                            masterVariant.Weight = variant.Weight;
                            masterVariant.BoostWeight = variant.BoostWeight;

                            if(masterVariant.Inventory == null)
                            {
                                masterVariant.Inventory = new Inventory()
                                {
                                    CreateBy = variant.Inventory.CreateBy,
                                    CreateOn = variant.Inventory.CreateOn,
                                    Defect = variant.Inventory.Defect,
                                    Quantity = variant.Inventory.Quantity,
                                    Reserve = variant.Inventory.Reserve,
                                    OnHold = variant.Inventory.OnHold,
                                    SafetyStockAdmin = variant.Inventory.SafetyStockAdmin,
                                    SafetyStockSeller = variant.Inventory.SafetyStockSeller,
                                    UseDecimal = variant.Inventory.UseDecimal,
                                    UpdateBy = variant.Inventory.UpdateBy,
                                    UpdateOn = variant.Inventory.UpdateOn,
                                    MaxQtyAllowInCart = variant.Inventory.MaxQtyAllowInCart,
                                    MaxQtyPreOrder = variant.Inventory.MaxQtyPreOrder,
                                    MinQtyAllowInCart = variant.Inventory.MinQtyAllowInCart,
                                    StockType = variant.Inventory.StockType
                                };
                            }
                            else
                            {
                                masterVariant.Inventory.CreateBy = variant.Inventory.CreateBy;
                                masterVariant.Inventory.CreateOn = variant.Inventory.CreateOn;
                                masterVariant.Inventory.Defect = variant.Inventory.Defect;
                                masterVariant.Inventory.Quantity = variant.Inventory.Quantity;
                                masterVariant.Inventory.Reserve = variant.Inventory.Reserve;
                                masterVariant.Inventory.OnHold = variant.Inventory.OnHold;
                                masterVariant.Inventory.SafetyStockAdmin = variant.Inventory.SafetyStockAdmin;
                                masterVariant.Inventory.SafetyStockSeller = variant.Inventory.SafetyStockSeller;
                                masterVariant.Inventory.UseDecimal = variant.Inventory.UseDecimal;
                                masterVariant.Inventory.UpdateBy = variant.Inventory.UpdateBy;
                                masterVariant.Inventory.UpdateOn = variant.Inventory.UpdateOn;
                                masterVariant.Inventory.MaxQtyAllowInCart = variant.Inventory.MaxQtyAllowInCart;
                                masterVariant.Inventory.MaxQtyPreOrder = variant.Inventory.MaxQtyPreOrder;
                                masterVariant.Inventory.MinQtyAllowInCart = variant.Inventory.MinQtyAllowInCart;
                                masterVariant.Inventory.StockType = variant.Inventory.StockType;
                            }

                            #endregion
                            #region More Detail
                            if (headDic.ContainsKey("ABI"))
                            {
                                var tmpTag = body[headDic["ABI"]].Split(',');
                                foreach (var tag in tmpTag)
                                {
                                    if (string.IsNullOrWhiteSpace(tag))
                                    {
                                        continue;
                                    }
                                    if (!group.ProductStageTags.Any(a => a.Tag.Equals(tag)))
                                    {
                                        group.ProductStageTags.Add(new ProductStageTag()
                                        {
                                            Tag = tag,
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        });
                                    }
                                }
                            }
                            //group.EffectiveDate = request.EffectiveDate.HasValue ? request.EffectiveDate.Value : currentDt;
                            //group.ExpireDate = request.ExpireDate.HasValue && request.ExpireDate.Value.CompareTo(group.EffectiveDate) >= 0 ? request.ExpireDate.Value : group.EffectiveDate.AddYears(Constant.DEFAULT_ADD_YEAR);
                            var tmpDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "ACY", guidance, errorMessage, row);
                            group.EffectiveDate = tmpDate.HasValue ? tmpDate.Value : DateTime.Now;
                            tmpDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "ACZ", guidance, errorMessage, row);
                            group.ExpireDate = tmpDate.HasValue? tmpDate.Value : group.EffectiveDate.AddYears(Constant.DEFAULT_ADD_YEAR);
                            group.Remark = Validation.ValidateCSVStringColumn(headDic, body, "ADH", guidance, false, 500, errorMessage, row, string.Empty);
                            if (headDic.ContainsKey("ADC"))
                            {
                                group.IsNew = string.Equals(body[headDic["ADC"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                            }
                            if (headDic.ContainsKey("ADD"))
                            {
                                group.IsClearance = string.Equals(body[headDic["ADD"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                            }
                            if (headDic.ContainsKey("ADE"))
                            {
                                group.IsBestSeller = string.Equals(body[headDic["ADE"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                            }
                            if (headDic.ContainsKey("ADF"))
                            {
                                group.IsOnlineExclusive = string.Equals(body[headDic["ADF"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                            }
                            if (headDic.ContainsKey("ADG"))
                            {
                                group.IsOnlyAt = string.Equals(body[headDic["ADG"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                            }
                            if (headDic.ContainsKey("ADA"))
                            {
                                group.GiftWrap = string.Equals(body[headDic["AAV"]], "yes", StringComparison.OrdinalIgnoreCase) ? Constant.STATUS_YES : Constant.STATUS_NO;
                            }

                            #endregion
                            #region Default Attribute
                            foreach (var attr in defaultAttribute)
                            {
                                if (headDic.ContainsKey(attr.AttributeNameEn))
                                {
                                    var valueEn = Validation.ValidateCSVStringColumn(headDic, body, attr.AttributeNameEn, guidance, false, 300, errorMessage, row);
                                    if (string.IsNullOrWhiteSpace(valueEn))
                                    {
                                        continue;
                                    }
                                    var spit = valueEn.Split(';');
                                    var valueTh = valueEn;
                                    if (spit.Count() > 0)
                                    {
                                        valueEn = spit[0].Trim();
                                    }
                                    if(spit.Count() > 1)
                                    {
                                        valueTh = spit[1].Trim();
                                    }
                                    int? valueId = null;
                                    bool isValue = false;
                                    if (Constant.DATA_TYPE_LIST.Equals(attr.DataType))
                                    {
                                        valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(valueEn)).Select(s => s.AttributeValueId).FirstOrDefault();
                                        if (valueId == 0)
                                        {
                                            throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
                                        }
                                        valueEn = string.Concat("((", valueId, "))");
                                        valueTh = valueEn;
                                        isValue = true;
                                    }
                                    if (Constant.DATA_TYPE_CHECKBOX.Equals(attr.DataType))
                                    {
                                        var tmpValue = valueEn.Split(',');
                                        tmpValue = tmpValue.Distinct().ToArray();
                                        foreach (var v in tmpValue)
                                        {
                                            var tmpDefValue = v.Trim();
                                            valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(tmpDefValue)).Select(s => s.AttributeValueId).FirstOrDefault();
                                            if (valueId == 0)
                                            {
                                                throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
                                            }
                                            var checkValue = string.Concat("((", valueId, "))");
                                            var tmpVariant = group.ProductStages
                                                    .Where(w => w.IsVariant == false
                                                     && !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(checkValue))
                                                     )
                                                    .SingleOrDefault();
                                            if (tmpVariant != null)
                                            {
                                                tmpVariant.ProductStageAttributes.Add(new ProductStageAttribute()
                                                {
                                                    AttributeId = attr.AttributeId,
                                                    ValueEn = checkValue,
                                                    ValueTh = checkValue,
                                                    CheckboxValue = true,
                                                    AttributeValueId = valueId,
                                                    IsAttributeValue = true,
                                                    CreateBy = User.UserRequest().Email,
                                                    CreateOn = DateTime.Now,
                                                    UpdateBy = User.UserRequest().Email,
                                                    UpdateOn = DateTime.Now,
                                                    
                                                });
                                            }
                                        }
                                        continue;
                                    }
                                    var tmpMasterVariant = group.ProductStages
                                                    .Where(w => w.IsVariant == false
                                                        && !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(valueEn)))
                                                    .SingleOrDefault();
                                    if (tmpMasterVariant != null)
                                    {
                                        tmpMasterVariant.ProductStageAttributes.Add(new ProductStageAttribute()
                                        {
                                            AttributeId = attr.AttributeId,
                                            ValueEn = valueEn,
                                            ValueTh = valueTh,
                                            CheckboxValue = false,
                                            IsAttributeValue = isValue,
                                            AttributeValueId = valueId,
                                            CreateBy = User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        });
                                    }
                                }
                            }
                            #endregion
                            #region Shipping 
                            if (headDic.ContainsKey("ABR"))
                            {
                                var shippingId = shipping.Where(w => w.ShippingMethodEn.Equals(body[headDic["ABR"]])).Select(s => s.ShippingId).FirstOrDefault();
                                if (shippingId != 0)
                                {
                                    group.ShippingId = shippingId;
                                }
                                else
                                {
                                    group.ShippingId = Constant.DEFAULT_SHIPPING_ID;
                                }
                            }
                            #endregion
                        }
                        #region Attribute Set
                        if (headDic.ContainsKey("ADI"))
                        {
                            try
                            {
                                string val = body[headDic["ADI"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    
                                    var attrSet = attributeSet.Where(w => string.Equals(w.AttributeSetNameEn, val, StringComparison.OrdinalIgnoreCase)).SingleOrDefault();
                                    if (attrSet == null)
                                    {
                                        throw new Exception("Attribute set " + val + " not found in database at row " + row);
                                    }
                                    group.AttributeSetId = attrSet.AttributeSetId;
                                    var variant1 = Validation.ValidateCSVStringColumn(headDic, body, "ADJ", guidance, false, 300, errorMessage, row);
                                    var variant2 = Validation.ValidateCSVStringColumn(headDic, body, "ADK", guidance, false, 300, errorMessage, row);
                                    bool isInvalid = false;
                                    if (!string.IsNullOrWhiteSpace(variant1) && !attrSet.Attribute.Any(a => a.AttributeNameEn.Equals(variant1)))
                                    {
                                        errorMessage.Add("Cannot find attribute " + variant1 + " in attribute set " + attrSet.AttributeSetNameEn + " at row " + row);
                                        isInvalid = true;
                                    }
                                    if (!string.IsNullOrWhiteSpace(variant2) && !attrSet.Attribute.Any(a => a.AttributeNameEn.Equals(variant2)))
                                    {
                                        errorMessage.Add("Cannot find attribute " + variant2 + " in attribute set " + attrSet.AttributeSetNameEn + " at row " + row);
                                        isInvalid = true;
                                    }
                                    if (!string.IsNullOrEmpty(variant1) && variant1.Equals(variant2))
                                    {
                                        errorMessage.Add("Attribute variant 1 " + variant1 + " cannot be same with attribute variant 2 " + variant2 + " at row " + row);
                                        isInvalid = true;
                                    }
                                    if (isInvalid)
                                    {
                                        continue;
                                    }
                                    foreach (var attr in attrSet.Attribute)
                                    {
                                        if (headDic.ContainsKey(attr.AttributeNameEn))
                                        {
                                            var valueEn = Validation.ValidateCSVStringColumn(headDic, body, attr.AttributeNameEn, guidance, false, 300, errorMessage, row);
                                            if (string.IsNullOrWhiteSpace(valueEn))
                                            {
                                                continue;
                                            }
                                            var spit = valueEn.Split(';');
                                            var valueTh = valueEn;
                                            if (spit.Count() > 0)
                                            {
                                                valueEn = spit[0].Trim();
                                            }
                                            if (spit.Count() > 1)
                                            {
                                                valueTh = spit[1].Trim();
                                            }
                                            int? valueId = null;
                                            bool isValue = false;
                                            if (Constant.DATA_TYPE_LIST.Equals(attr.DataType))
                                            {
                                                valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(valueEn)).Select(s => s.AttributeValueId).FirstOrDefault();
                                                if (valueId == 0)
                                                {
                                                    throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
                                                }
                                                valueEn = string.Concat("((", valueId, "))");
                                                valueTh = string.Concat("((", valueId, "))");
                                                isValue = true;
                                            }
                                            if (Constant.DATA_TYPE_CHECKBOX.Equals(attr.DataType))
                                            {
                                                var tmpValue = valueEn.Split(',');
                                                tmpValue = tmpValue.Distinct().ToArray();
                                                foreach (var v in tmpValue)
                                                {
                                                    var tmpDefValue = v.Trim();
                                                    valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(tmpDefValue)).Select(s => s.AttributeValueId).FirstOrDefault();
                                                    if (valueId == 0)
                                                    {
                                                        throw new Exception("Invalid attribute value " + valueEn + " in attribute " + attr.AttributeNameEn);
                                                    }
                                                    if (string.Equals(attr.AttributeNameEn, variant1, StringComparison.OrdinalIgnoreCase)
                                                        || string.Equals(attr.AttributeNameEn, variant2, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        errorMessage.Add("Checkbox cannot be variant");
                                                        continue;
                                                    }
                                                    else
                                                    {

                                                        var checkValue = string.Concat("((", valueId, "))");
                                                        var tmpMasterVariant = group.ProductStages
                                                                .Where(w => w.IsVariant == false
                                                                    && !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(checkValue))
                                                                 )
                                                                .SingleOrDefault();
                                                        if (tmpMasterVariant != null)
                                                        {
                                                            tmpMasterVariant.ProductStageAttributes.Add(new ProductStageAttribute()
                                                            {
                                                                AttributeId = attr.AttributeId,
                                                                ValueEn = checkValue,
                                                                ValueTh = checkValue,
                                                                CheckboxValue = true,
                                                                IsAttributeValue = true,
                                                                AttributeValueId = valueId,
                                                                CreateBy = User.UserRequest().Email,
                                                                CreateOn = DateTime.Now,
                                                                UpdateBy = User.UserRequest().Email,
                                                                UpdateOn = DateTime.Now,
                                                            });
                                                        }
                                                    }
                                                }
                                                continue;
                                            }
                                            
                                            if (string.Equals(attr.AttributeNameEn, variant1, StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (!attr.VariantStatus && Constant.DATA_TYPE_STRING.Equals(attr.AttributeNameEn))
                                                {
                                                    errorMessage.Add("Attribute " + attr.AttributeNameEn + " is not variant type at row " + row);
                                                    continue;
                                                }
                                                if (!variant.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId))
                                                {
                                                    variant.ProductStageAttributes.Add(new ProductStageAttribute()
                                                    {
                                                        AttributeId = attr.AttributeId,
                                                        ValueEn = valueEn,
                                                        ValueTh = valueTh,
                                                        CheckboxValue = false,
                                                        AttributeValueId = valueId,
                                                        IsAttributeValue = isValue,
                                                        CreateBy = User.UserRequest().Email,
                                                        CreateOn = DateTime.Now,
                                                        UpdateBy = User.UserRequest().Email,
                                                        UpdateOn = DateTime.Now,
                                                    });
                                                }
                                            }
                                            
                                            else if (string.Equals(attr.AttributeNameEn, variant2, StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (!attr.VariantStatus && Constant.DATA_TYPE_STRING.Equals(attr.AttributeNameEn))
                                                {
                                                    errorMessage.Add("Attribute " + attr.AttributeNameEn + " is not variant type at row " + row);
                                                    continue;
                                                }
                                                if (!variant.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId))
                                                {
                                                    variant.ProductStageAttributes.Add(new ProductStageAttribute()
                                                    {
                                                        AttributeId = attr.AttributeId,
                                                        ValueEn = valueEn,
                                                        ValueTh = valueTh,
                                                        CheckboxValue = false,
                                                        AttributeValueId = valueId,
                                                        IsAttributeValue = isValue,
                                                        CreateBy = User.UserRequest().Email,
                                                        CreateOn = DateTime.Now,
                                                        UpdateBy = User.UserRequest().Email,
                                                        UpdateOn = DateTime.Now,
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                var tmpMasterVariant = group.ProductStages
                                                            .Where(w => w.IsVariant == false
                                                            && !w.ProductStageAttributes.Any(a => a.AttributeId == attr.AttributeId && a.ValueEn.Equals(valueEn)))
                                                            .SingleOrDefault();
                                                if (tmpMasterVariant != null)
                                                {
                                                    tmpMasterVariant.ProductStageAttributes.Add(new ProductStageAttribute()
                                                    {
                                                        AttributeId = attr.AttributeId,
                                                        ValueEn = valueEn,
                                                        ValueTh = valueTh,
                                                        AttributeValueId = valueId,
                                                        CheckboxValue = false,
                                                        IsAttributeValue = isValue,
                                                        CreateBy = User.UserRequest().Email,
                                                        CreateOn = DateTime.Now,
                                                        UpdateBy = User.UserRequest().Email,
                                                        UpdateOn = DateTime.Now,
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                errorMessage.Add(e.Message + " at row " + row);
                            }
                        }
                        #endregion
                        #region Validate Attribute
                        if (variant.ProductStageAttributes != null && variant.ProductStageAttributes.Count > 0)
                        {

                            var productAttribute = group.ProductStages.Select(s => s.ProductStageAttributes);

                            foreach(var v in productAttribute)
                            {
                                if(variant.ProductStageAttributes
                                    .All(a=>v.Any(aa=>a.AttributeId==aa.AttributeId && a.ValueEn.Equals(aa.ValueEn) && a.ValueTh.Equals(aa.ValueTh))))
                                {
                                    errorMessage.Add(string.Concat("Duplicate variant value at row " , row));
                                }
                            }
                            group.ProductStages.Add(variant);
                        }
                        if (!groupList.ContainsKey(groupId))
                        {
                            groupList.Add(groupId, group);
                        }
                        #endregion
                        row++;
                    }
                    //if (csvResult != null)
                    //{
                    //    csvResult.Dispose();
                    //}
                }
                //if (fileReader != null)
                //{
                //    fileReader.Close();
                //    fileReader.Dispose();
                //}
                return groupList;
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

        [Route("api/ProductStages/Import")]
        [HttpPut]
        public async Task<HttpResponseMessage> ImportSaveProduct()
        {
            string fileName = string.Empty;
            HashSet<string> errorMessage = new HashSet<string>();
            int row = 2;
            try
            {
                if(User.ShopRequest() == null)
                {
                    throw new Exception("Invalid request");
                }
                
                #region Validate Request
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Content Multimedia");
                }
                string root = HttpContext.Current.Server.MapPath("~/Import");
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
                {
                    throw new Exception("No file uploaded");
                }
                fileName = streamProvider.FileData[0].LocalFileName;
                #endregion
                HashSet<string> header = new HashSet<string>();
                Dictionary<string, ProductStageGroup> groupList = SetupImport(fileName, errorMessage, row, db,true,header);
                #region Validate Error Message
                if (errorMessage.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
                }
                #endregion
                #region Query
                var shopId = User.ShopRequest().ShopId;
                var productIds = groupList.Values.Select(s=>s.ProductId).ToList();
                var gropEnList = db.ProductStageGroups
                    .Where(w => w.ShopId == shopId && productIds.Contains(w.ProductId))
                    .Include(i=>i.ProductStages.Select(s=>s.ProductStageAttributes))
                    .Include(i=>i.ProductStageTags)
                    .Include(i=>i.ProductStageGlobalCatMaps)
                    .Include(i=>i.ProductStageLocalCatMaps)
                    .Include(i=>i.ProductStages.Select(s=>s.Inventory))
                    .Include(i=>i.ProductStageRelateds).ToList();
                #endregion
                foreach (var g in groupList.Values)
                {
                    #region Validation
                    var groupEn = gropEnList.Where(w => w.ProductId == g.ProductId).SingleOrDefault();
                    if(groupEn == null)
                    {
                        errorMessage.Add("Cannot find 'empty' group id in seller portal. If you are trying to add new products, please use 'Import - Add New Products' feature.");
                        continue;
                    }
                    if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(groupEn.Status))
                    {
                        errorMessage.Add("Cannot edit product " + groupEn.ProductId + " with status Wait for Approval" );
                        continue;
                    }
                    #endregion
                    groupEn.Status = Constant.PRODUCT_STATUS_DRAFT;
                    #region Brand
                    if (header.Contains("AAK"))
                    {
                        groupEn.BrandId = g.BrandId;
                    }
                    #endregion
                    #region Category
                    if (header.Contains("ACG"))
                    {
                        groupEn.GlobalCatId = g.GlobalCatId;
                    }
                    if(header.Contains("ACJ"))
                    {
                        groupEn.LocalCatId = g.LocalCatId;
                    }
                    if (header.Contains("ACH") 
                        || header.Contains("ACI"))
                    {
                        var globalCat = groupEn.ProductStageGlobalCatMaps.ToList();
                        if(g.ProductStageGlobalCatMaps != null)
                        {
                            foreach(var cat in g.ProductStageGlobalCatMaps)
                            {
                                bool isNewCat = false;
                                if(globalCat == null || globalCat.Count == 0)
                                {
                                    isNewCat = true;
                                }
                                if (!isNewCat)
                                {
                                    var currentCat = globalCat.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
                                    if(currentCat != null)
                                    {
                                        globalCat.Remove(currentCat);
                                    }
                                    else
                                    {
                                        isNewCat = true;
                                    }
                                }
                                if (isNewCat)
                                {
                                    groupEn.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                                    {
                                        CategoryId = cat.CategoryId,
                                        CreateBy = User.UserRequest().Email,
                                        CreateOn = DateTime.Now,
                                        UpdateBy = User.UserRequest().Email,
                                        UpdateOn = DateTime.Now,
                                    });
                                }
                            }
                        }
                        if(globalCat != null && globalCat.Count > 0)
                        {
                            db.ProductStageGlobalCatMaps.RemoveRange(globalCat);
                        }
                    }
                    if (header.Contains("ACK")
                        || header.Contains("ACL"))
                    {
                        var localCat = groupEn.ProductStageLocalCatMaps.ToList();
                        if (g.ProductStageLocalCatMaps != null)
                        {
                            foreach (var cat in g.ProductStageLocalCatMaps)
                            {
                                bool isNewCat = false;
                                if (localCat == null || localCat.Count == 0)
                                {
                                    isNewCat = true;
                                }
                                if (!isNewCat)
                                {
                                    var currentCat = localCat.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
                                    if (currentCat != null)
                                    {
                                        localCat.Remove(currentCat);
                                    }
                                    else
                                    {
                                        isNewCat = true;
                                    }
                                }
                                if (isNewCat)
                                {
                                    groupEn.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                                    {
                                        CategoryId = cat.CategoryId,
                                        CreateBy = User.UserRequest().Email,
                                        CreateOn = DateTime.Now,
                                        UpdateBy = User.UserRequest().Email,
                                        UpdateOn = DateTime.Now,
                                    });
                                }
                            }
                        }
                        if (localCat != null && localCat.Count > 0)
                        {
                            db.ProductStageLocalCatMaps.RemoveRange(localCat);
                        }
                    }
                    #endregion
                    #region Tag
                    if (header.Contains("ABI"))
                    {
                        var tag = groupEn.ProductStageTags.ToList();
                        if(g.ProductStageTags != null && g.ProductStageTags.ToList().Count > 0)
                        {
                            foreach(var t in g.ProductStageTags)
                            {
                                bool isNew = false;
                                if(tag == null || tag.Count == 0)
                                {
                                    isNew = true;
                                }
                                if (!isNew)
                                {
                                    var current = tag.Where(w => w.Tag.Equals(t.Tag)).SingleOrDefault();
                                    if(current != null)
                                    {
                                        tag.Remove(current);
                                    }
                                    else
                                    {
                                        isNew = true;
                                    }
                                }
                                if (isNew)
                                {
                                    groupEn.ProductStageTags.Add(new ProductStageTag()
                                    {
                                        Tag = t.Tag,
                                        CreateBy = User.UserRequest().Email,
                                        CreateOn = DateTime.Now,
                                        UpdateBy = User.UserRequest().Email,
                                        UpdateOn = DateTime.Now,
                                    });
                                }
                            }
                        }
                        if(tag != null && tag.Count > 0)
                        {
                            db.ProductStageTags.RemoveRange(tag);
                        }
                    }
                    #endregion
                    #region Setup Group
                    if (header.Contains("ACY"))
                    {
                        groupEn.EffectiveDate = g.EffectiveDate;
                    }
                    if (header.Contains("ACZ"))
                    {
                        groupEn.ExpireDate = g.ExpireDate;
                    }
                    if (header.Contains("ADA"))
                    {
                        groupEn.GiftWrap = g.GiftWrap;
                    }

                    if (header.Contains("ADB"))
                    {
                        groupEn.NewArrivalDate = g.NewArrivalDate;
                    }
                    if (header.Contains("ADC"))
                    {
                        groupEn.IsNew = g.IsNew;
                    }
                    if (header.Contains("ADD"))
                    {
                        groupEn.IsClearance = g.IsClearance;
                    }
                    if (header.Contains("ADE"))
                    {
                        groupEn.IsBestSeller = g.IsBestSeller;
                    }
                    if (header.Contains("ADF"))
                    {
                        groupEn.IsOnlineExclusive = g.IsOnlineExclusive;
                    }
                    if (header.Contains("ADG"))
                    {
                        groupEn.IsOnlyAt = g.IsOnlyAt;
                    }

                    if (header.Contains("Remark"))
                    {
                        groupEn.Remark = g.Remark;
                    }
                    
                    if (header.Contains("ADI"))
                    {
                        groupEn.AttributeSetId = g.AttributeSetId;
                    }
                    if (header.Contains("ABR"))
                    {
                        groupEn.ShippingId = g.ShippingId;
                    }
                    #endregion
                    #region Master Variant
                    var masterVariantEn = groupEn.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
                    var importVariantEn = g.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
                    if (importVariantEn != null && masterVariantEn != null)
                    {
                        #region Setup Variant
                        if (header.Contains("AAC"))
                        {
                            masterVariantEn.DefaultVariant = importVariantEn.DefaultVariant;
                        }
                        if (header.Contains("AAE"))
                        {
                            masterVariantEn.ProductNameEn = importVariantEn.ProductNameEn;
                        }
                        if (header.Contains("AAF"))
                        {
                            masterVariantEn.ProductNameTh = importVariantEn.ProductNameTh;
                        }
                        if (header.Contains("AAG"))
                        {
                            masterVariantEn.ProdTDNameEn = importVariantEn.ProdTDNameEn;
                        }
                        if (header.Contains("AAH"))
                        {
                            masterVariantEn.ProdTDNameTh = importVariantEn.ProdTDNameTh;
                        }
                        if (header.Contains("AAI"))
                        {
                            masterVariantEn.Sku = importVariantEn.Sku;
                        }
                        if (header.Contains("AAJ"))
                        {
                            masterVariantEn.Upc = importVariantEn.Upc;
                        }
                        if (header.Contains("AAM"))
                        {
                            masterVariantEn.OriginalPrice = importVariantEn.OriginalPrice;
                        }
                        if (header.Contains("AAL"))
                        {
                            masterVariantEn.SalePrice = importVariantEn.SalePrice;
                        }
                        if (header.Contains("AAN"))
                        {
                            masterVariantEn.Installment = importVariantEn.Installment;
                        }
                        if (header.Contains("AAO"))
                        {
                            masterVariantEn.PromotionPrice = importVariantEn.PromotionPrice;
                        }
                        if (header.Contains("AAP"))
                        {
                            masterVariantEn.EffectiveDatePromotion = importVariantEn.EffectiveDatePromotion;
                        }
                        if (header.Contains("AAQ"))
                        {
                            masterVariantEn.ExpireDatePromotion = importVariantEn.ExpireDatePromotion;
                        }
                        if (header.Contains("AAR"))
                        {
                            masterVariantEn.UnitPrice = importVariantEn.UnitPrice;
                        }
                        if (header.Contains("AAS"))
                        {
                            masterVariantEn.PurchasePrice = importVariantEn.PurchasePrice;
                        }
                        if (header.Contains("AAT"))
                        {
                            masterVariantEn.SaleUnitEn = importVariantEn.SaleUnitEn;
                        }
                        if (header.Contains("AAU"))
                        {
                            masterVariantEn.SaleUnitTh = importVariantEn.SaleUnitTh;
                        }
                        if (header.Contains("AAV"))
                        {
                            masterVariantEn.IsVat = importVariantEn.IsVat;
                        }

                        if (header.Contains("AAW"))
                        {
                            masterVariantEn.DescriptionFullEn = importVariantEn.DescriptionFullEn;
                        }
                        if (header.Contains("AAX"))
                        {
                            masterVariantEn.DescriptionFullTh = importVariantEn.DescriptionFullTh;
                        }
                        if (header.Contains("AAY"))
                        {
                            masterVariantEn.MobileDescriptionEn = importVariantEn.MobileDescriptionEn;
                        }
                        if (header.Contains("AAZ"))
                        {
                            masterVariantEn.MobileDescriptionTh = importVariantEn.MobileDescriptionTh;
                        }

                        if (header.Contains("ABA"))
                        {
                            masterVariantEn.DescriptionShortEn = importVariantEn.DescriptionShortEn;
                        }
                        if (header.Contains("ABB"))
                        {
                            masterVariantEn.DescriptionShortTh = importVariantEn.DescriptionShortTh;
                        }
                        if (header.Contains("ABC"))
                        {
                            masterVariantEn.KillerPoint1En = importVariantEn.KillerPoint1En;
                        }
                        if (header.Contains("ABD"))
                        {
                            masterVariantEn.KillerPoint1Th = importVariantEn.KillerPoint1Th;
                        }
                        if (header.Contains("ABE"))
                        {
                            masterVariantEn.KillerPoint2En = importVariantEn.KillerPoint2En;
                        }
                        if (header.Contains("ABF"))
                        {
                            masterVariantEn.KillerPoint2Th = importVariantEn.KillerPoint2Th;
                        }
                        if (header.Contains("ABG"))
                        {
                            masterVariantEn.KillerPoint3En = importVariantEn.KillerPoint3En;
                        }
                        if (header.Contains("ABH"))
                        {
                            masterVariantEn.KillerPoint3Th = importVariantEn.KillerPoint3Th;
                        }
                        if (header.Contains("ABJ"))
                        {
                            if (masterVariantEn.Inventory != null)
                            {
                                masterVariantEn.Inventory.Quantity = importVariantEn.Inventory.Quantity;
                            }
                            else
                            {
                                masterVariantEn.Inventory = importVariantEn.Inventory;
                            }
                        }
                        if (header.Contains("ABL"))
                        {
                            if (masterVariantEn.Inventory != null)
                            {
                                masterVariantEn.Inventory.SafetyStockSeller = importVariantEn.Inventory.SafetyStockSeller;
                            }
                            else
                            {
                                masterVariantEn.Inventory = importVariantEn.Inventory;
                            }
                        }
                        if (header.Contains("ABM"))
                        {
                            if (masterVariantEn.Inventory != null)
                            {
                                masterVariantEn.Inventory.MinQtyAllowInCart = importVariantEn.Inventory.MinQtyAllowInCart;
                            }
                            else
                            {
                                masterVariantEn.Inventory = importVariantEn.Inventory;
                            }
                        }
                        if (header.Contains("ABN"))
                        {
                            if (masterVariantEn.Inventory != null)
                            {
                                masterVariantEn.Inventory.MaxQtyAllowInCart = importVariantEn.Inventory.MaxQtyAllowInCart;
                            }
                            else
                            {
                                masterVariantEn.Inventory = importVariantEn.Inventory;
                            }
                        }
                        if (header.Contains("ABP"))
                        {
                            if (masterVariantEn.Inventory != null)
                            {
                                masterVariantEn.Inventory.MaxQtyPreOrder = importVariantEn.Inventory.MaxQtyPreOrder;
                            }
                            else
                            {
                                masterVariantEn.Inventory = importVariantEn.Inventory;
                            }
                        }
                        if (header.Contains("ABO"))
                        {
                            if (masterVariantEn.Inventory != null)
                            {
                                masterVariantEn.Inventory.StockType = importVariantEn.Inventory.StockType;
                            }
                            else
                            {
                                masterVariantEn.Inventory = importVariantEn.Inventory;
                            }
                        }
                        if (header.Contains("ABQ"))
                        {
                            masterVariantEn.IsHasExpiryDate = importVariantEn.IsHasExpiryDate;
                        }
                        if (header.Contains("ABS"))
                        {
                            masterVariantEn.ExpressDelivery = importVariantEn.ExpressDelivery;
                        }
                        if (header.Contains("ABT"))
                        {
                            masterVariantEn.DeliveryFee = importVariantEn.DeliveryFee;
                        }

                        if (header.Contains("ABU"))
                        {
                            masterVariantEn.PrepareDay = importVariantEn.PrepareDay;
                        }
                        if (header.Contains("ABV"))
                        {
                            masterVariantEn.PrepareMon = importVariantEn.PrepareMon;
                        }
                        if (header.Contains("ABW"))
                        {
                            masterVariantEn.PrepareTue = importVariantEn.PrepareTue;
                        }
                        if (header.Contains("ABX"))
                        {
                            masterVariantEn.PrepareWed = importVariantEn.PrepareWed;
                        }
                        if (header.Contains("ABY"))
                        {
                            masterVariantEn.PrepareThu = importVariantEn.PrepareThu;
                        }
                        if (header.Contains("ABZ"))
                        {
                            masterVariantEn.PrepareFri = importVariantEn.PrepareFri;
                        }
                        if (header.Contains("ACA"))
                        {
                            masterVariantEn.PrepareSat = importVariantEn.PrepareSat;
                        }
                        if (header.Contains("ACB"))
                        {
                            masterVariantEn.PrepareSun = importVariantEn.PrepareSun;
                        }
                        if (header.Contains("ACC"))
                        {
                            masterVariantEn.Length = importVariantEn.Length;
                        }
                        if (header.Contains("ACD"))
                        {
                            masterVariantEn.Height = importVariantEn.Height;
                        }
                        if (header.Contains("ACE"))
                        {
                            masterVariantEn.Width = importVariantEn.Width;
                        }
                        if (header.Contains("ACF"))
                        {
                            masterVariantEn.Weight = importVariantEn.Weight;
                        }

                        if (header.Contains("ACN"))
                        {
                            masterVariantEn.SeoEn = importVariantEn.SeoEn;
                        }
                        if (header.Contains("ACO"))
                        {
                            masterVariantEn.SeoTh = importVariantEn.SeoTh;
                        }

                        if (header.Contains("ACP"))
                        {
                            masterVariantEn.MetaTitleEn = importVariantEn.MetaTitleEn;
                        }
                        if (header.Contains("ACQ"))
                        {
                            masterVariantEn.MetaTitleTh = importVariantEn.MetaTitleTh;
                        }
                        if (header.Contains("ACR)"))
                        {
                            masterVariantEn.MetaDescriptionEn = importVariantEn.MetaDescriptionEn;
                        }
                        if (header.Contains("ACS"))
                        {
                            masterVariantEn.MetaDescriptionTh = importVariantEn.MetaDescriptionTh;
                        }
                        if (header.Contains("ACT"))
                        {
                            masterVariantEn.MetaKeyEn = importVariantEn.MetaKeyEn;
                        }
                        if (header.Contains("ACU"))
                        {
                            masterVariantEn.MetaKeyTh = importVariantEn.MetaKeyTh;
                        }
                        if (header.Contains("ACW"))
                        {
                            masterVariantEn.BoostWeight = importVariantEn.BoostWeight;
                        }
                        if (header.Contains("ACX"))
                        {
                            masterVariantEn.GlobalBoostWeight = importVariantEn.GlobalBoostWeight;
                        }
                        if (header.Contains("ADL"))
                        {
                            masterVariantEn.Visibility = importVariantEn.Visibility;
                        }
                        #endregion
                    }
                    #endregion
                    #region Default Attribute
                    var defaultAttribute = masterVariantEn.ProductStageAttributes.ToList();
                    if (importVariantEn.ProductStageAttributes != null)
                    {
                        foreach (var tmpAttri in importVariantEn.ProductStageAttributes)
                        {
                            bool isNew = false;
                            if (defaultAttribute == null || defaultAttribute.Count == 0)
                            {
                                isNew = true;
                            }
                            if (!isNew)
                            {
                                var current = defaultAttribute.Where(w => w.AttributeId == tmpAttri.AttributeId 
                                    && w.ValueEn.Equals(tmpAttri.ValueEn)
                                    && w.ValueTh.Equals(tmpAttri.ValueTh)).SingleOrDefault();
                                if (current != null)
                                {
                                    defaultAttribute.Remove(current);
                                }
                                else
                                {
                                    isNew = true;
                                }
                            }
                            if (isNew)
                            {
                                masterVariantEn.ProductStageAttributes.Add(new ProductStageAttribute()
                                {
                                    AttributeId = tmpAttri.AttributeId,
                                    ValueEn = tmpAttri.ValueEn,
                                    ValueTh = tmpAttri.ValueTh,
                                    CheckboxValue = tmpAttri.CheckboxValue,
                                    CreateBy = tmpAttri.CreateBy,
                                    CreateOn = tmpAttri.CreateOn,
                                    IsAttributeValue = tmpAttri.IsAttributeValue,
                                    Position = tmpAttri.Position,
                                    UpdateBy = tmpAttri.UpdateBy,
                                    UpdateOn = tmpAttri.UpdateOn
                                });
                            }
                        }
                    }
                    if (defaultAttribute != null && defaultAttribute.Count > 0)
                    {
                        db.ProductStageAttributes.RemoveRange(defaultAttribute);
                    }
                    #endregion
                    #region Variants
                    var stageList = groupEn.ProductStages.Where(w=>w.IsVariant==true).ToList();
                    if (g.ProductStages != null)
                    {
                        foreach (var staging in g.ProductStages.Where(w => w.IsVariant == true))
                        {
                            bool isNewStage = false;
                            if (stageList == null || stageList.Count == 0)
                            {
                                isNewStage = true;
                            }
                            if (!isNewStage)
                            {
                                var currentStage = stageList.Where(w => w.Pid.Equals(staging.Pid)).SingleOrDefault();
                                if (currentStage != null)
                                {
                                    stageList.Remove(currentStage);
                                    currentStage.Status = Constant.PRODUCT_STATUS_DRAFT;
                                    #region Setup Variant
                                    if (header.Contains("AAC"))
                                    {
                                        currentStage.DefaultVariant = staging.DefaultVariant;
                                    }
                                    if (header.Contains("AAE"))
                                    {
                                        currentStage.ProductNameEn = staging.ProductNameEn;
                                    }
                                    if (header.Contains("AAF"))
                                    {
                                        currentStage.ProductNameTh = staging.ProductNameTh;
                                    }
                                    if (header.Contains("AAG"))
                                    {
                                        currentStage.ProdTDNameEn = staging.ProdTDNameEn;
                                    }
                                    if (header.Contains("AAH"))
                                    {
                                        currentStage.ProdTDNameTh = staging.ProdTDNameTh;
                                    }
                                    if (header.Contains("AAI"))
                                    {
                                        currentStage.Sku = staging.Sku;
                                    }
                                    if (header.Contains("AAJ"))
                                    {
                                        currentStage.Upc = staging.Upc;
                                    }
                                    if (header.Contains("AAM"))
                                    {
                                        currentStage.OriginalPrice = staging.OriginalPrice;
                                    }
                                    if (header.Contains("AAL"))
                                    {
                                        currentStage.SalePrice = staging.SalePrice;
                                    }
                                    if (header.Contains("AAN"))
                                    {
                                        currentStage.Installment = staging.Installment;
                                    }
                                    if (header.Contains("AAO"))
                                    {
                                        currentStage.PromotionPrice = staging.PromotionPrice;
                                    }
                                    if (header.Contains("AAP"))
                                    {
                                        currentStage.EffectiveDatePromotion = staging.EffectiveDatePromotion;
                                    }
                                    if (header.Contains("AAQ"))
                                    {
                                        currentStage.ExpireDatePromotion = staging.ExpireDatePromotion;
                                    }
                                    if (header.Contains("AAR"))
                                    {
                                        currentStage.UnitPrice = staging.UnitPrice;
                                    }
                                    if (header.Contains("AAS"))
                                    {
                                        currentStage.PurchasePrice = staging.PurchasePrice;
                                    }
                                    if (header.Contains("AAT"))
                                    {
                                        currentStage.SaleUnitEn = staging.SaleUnitEn;
                                    }
                                    if (header.Contains("AAU"))
                                    {
                                        currentStage.SaleUnitTh = staging.SaleUnitTh;
                                    }
                                    if (header.Contains("AAV"))
                                    {
                                        currentStage.IsVat = staging.IsVat;
                                    }

                                    if (header.Contains("AAW"))
                                    {
                                        currentStage.DescriptionFullEn = staging.DescriptionFullEn;
                                    }
                                    if (header.Contains("AAX"))
                                    {
                                        currentStage.DescriptionFullTh = staging.DescriptionFullTh;
                                    }
                                    if (header.Contains("AAY"))
                                    {
                                        currentStage.MobileDescriptionEn = staging.MobileDescriptionEn;
                                    }
                                    if (header.Contains("AAZ"))
                                    {
                                        currentStage.MobileDescriptionTh = staging.MobileDescriptionTh;
                                    }

                                    if (header.Contains("ABA"))
                                    {
                                        currentStage.DescriptionShortEn = staging.DescriptionShortEn;
                                    }
                                    if (header.Contains("ABB"))
                                    {
                                        currentStage.DescriptionShortTh = staging.DescriptionShortTh;
                                    }
                                    if (header.Contains("ABC"))
                                    {
                                        currentStage.KillerPoint1En = staging.KillerPoint1En;
                                    }
                                    if (header.Contains("ABD"))
                                    {
                                        currentStage.KillerPoint1Th = staging.KillerPoint1Th;
                                    }
                                    if (header.Contains("ABE"))
                                    {
                                        currentStage.KillerPoint2En = staging.KillerPoint2En;
                                    }
                                    if (header.Contains("ABF"))
                                    {
                                        currentStage.KillerPoint2Th = staging.KillerPoint2Th;
                                    }
                                    if (header.Contains("ABG"))
                                    {
                                        currentStage.KillerPoint3En = staging.KillerPoint3En;
                                    }
                                    if (header.Contains("ABH"))
                                    {
                                        currentStage.KillerPoint3Th = staging.KillerPoint3Th;
                                    }
                                    if (header.Contains("ABJ"))
                                    {
                                        if (currentStage.Inventory != null)
                                        {
                                            currentStage.Inventory.Quantity = staging.Inventory.Quantity;
                                        }
                                        else
                                        {
                                            currentStage.Inventory = staging.Inventory;
                                        }
                                    }
                                    if (header.Contains("ABL"))
                                    {
                                        if (currentStage.Inventory != null)
                                        {
                                            currentStage.Inventory.SafetyStockSeller = staging.Inventory.SafetyStockSeller;
                                        }
                                        else
                                        {
                                            currentStage.Inventory = staging.Inventory;
                                        }
                                    }
                                    if (header.Contains("ABM"))
                                    {
                                        if (currentStage.Inventory != null)
                                        {
                                            currentStage.Inventory.MinQtyAllowInCart = staging.Inventory.MinQtyAllowInCart;
                                        }
                                        else
                                        {
                                            currentStage.Inventory = staging.Inventory;
                                        }
                                    }
                                    if (header.Contains("ABN"))
                                    {
                                        if (currentStage.Inventory != null)
                                        {
                                            currentStage.Inventory.MaxQtyAllowInCart = staging.Inventory.MaxQtyAllowInCart;
                                        }
                                        else
                                        {
                                            currentStage.Inventory = staging.Inventory;
                                        }
                                    }
                                    if (header.Contains("ABP"))
                                    {
                                        if (currentStage.Inventory != null)
                                        {
                                            currentStage.Inventory.MaxQtyPreOrder = staging.Inventory.MaxQtyPreOrder;
                                        }
                                        else
                                        {
                                            currentStage.Inventory = staging.Inventory;
                                        }
                                    }
                                    if (header.Contains("ABO"))
                                    {
                                        if (currentStage.Inventory != null)
                                        {
                                            currentStage.Inventory.StockType = staging.Inventory.StockType;
                                        }
                                        else
                                        {
                                            currentStage.Inventory = staging.Inventory;
                                        }
                                    }
                                    if (header.Contains("ABQ"))
                                    {
                                        currentStage.IsHasExpiryDate = staging.IsHasExpiryDate;
                                    }
                                    if (header.Contains("ABS"))
                                    {
                                        currentStage.ExpressDelivery = staging.ExpressDelivery;
                                    }
                                    if (header.Contains("ABT"))
                                    {
                                        currentStage.DeliveryFee = staging.DeliveryFee;
                                    }

                                    if (header.Contains("ABU"))
                                    {
                                        currentStage.PrepareDay = staging.PrepareDay;
                                    }
                                    if (header.Contains("ABV"))
                                    {
                                        currentStage.PrepareMon = staging.PrepareMon;
                                    }
                                    if (header.Contains("ABW"))
                                    {
                                        currentStage.PrepareTue = staging.PrepareTue;
                                    }
                                    if (header.Contains("ABX"))
                                    {
                                        currentStage.PrepareWed = staging.PrepareWed;
                                    }
                                    if (header.Contains("ABY"))
                                    {
                                        currentStage.PrepareThu = staging.PrepareThu;
                                    }
                                    if (header.Contains("ABZ"))
                                    {
                                        currentStage.PrepareFri = staging.PrepareFri;
                                    }
                                    if (header.Contains("ACA"))
                                    {
                                        currentStage.PrepareSat = staging.PrepareSat;
                                    }
                                    if (header.Contains("ACB"))
                                    {
                                        currentStage.PrepareSun = staging.PrepareSun;
                                    }
                                    if (header.Contains("ACC"))
                                    {
                                        currentStage.Length = staging.Length;
                                    }
                                    if (header.Contains("ACD"))
                                    {
                                        currentStage.Height = staging.Height;
                                    }
                                    if (header.Contains("ACE"))
                                    {
                                        currentStage.Width = staging.Width;
                                    }
                                    if (header.Contains("ACF"))
                                    {
                                        currentStage.Weight = staging.Weight;
                                    }

                                    if (header.Contains("ACN"))
                                    {
                                        currentStage.SeoEn = staging.SeoEn;
                                    }
                                    if (header.Contains("ACO"))
                                    {
                                        currentStage.SeoTh = staging.SeoTh;
                                    }

                                    if (header.Contains("ACP"))
                                    {
                                        currentStage.MetaTitleEn = staging.MetaTitleEn;
                                    }
                                    if (header.Contains("ACQ"))
                                    {
                                        currentStage.MetaTitleTh = staging.MetaTitleTh;
                                    }
                                    if (header.Contains("ACR)"))
                                    {
                                        currentStage.MetaDescriptionEn = staging.MetaDescriptionEn;
                                    }
                                    if (header.Contains("ACS"))
                                    {
                                        currentStage.MetaDescriptionTh = staging.MetaDescriptionTh;
                                    }
                                    if (header.Contains("ACT"))
                                    {
                                        currentStage.MetaKeyEn = staging.MetaKeyEn;
                                    }
                                    if (header.Contains("ACU"))
                                    {
                                        currentStage.MetaKeyTh = staging.MetaKeyTh;
                                    }
                                    if (header.Contains("ACW"))
                                    {
                                        currentStage.BoostWeight = staging.BoostWeight;
                                    }
                                    if (header.Contains("ACX"))
                                    {
                                        currentStage.GlobalBoostWeight = staging.GlobalBoostWeight;
                                    }
                                    if (header.Contains("ADL"))
                                    {
                                        currentStage.Visibility = staging.Visibility;
                                    }
                                    #endregion
                                    #region Setup Attribute
                                    var attributeTmp = currentStage.ProductStageAttributes.ToList();
                                    if (staging.ProductStageAttributes != null && staging.ProductStageAttributes.Count > 0)
                                    {
                                        foreach (var tmpAttri in staging.ProductStageAttributes)
                                        {
                                            bool isNew = false;
                                            if (attributeTmp == null || attributeTmp.Count == 0)
                                            {
                                                isNew = true;
                                            }
                                            if (!isNew)
                                            {
                                                var current = attributeTmp.Where(w => w.AttributeId == tmpAttri.AttributeId && w.ValueEn.Equals(tmpAttri.ValueEn)).SingleOrDefault();
                                                if (current != null)
                                                {
                                                    attributeTmp.Remove(current);
                                                }
                                                else
                                                {
                                                    isNew = true;
                                                }
                                            }
                                            if (isNew)
                                            {
                                                currentStage.ProductStageAttributes.Add(new ProductStageAttribute()
                                                {
                                                    AttributeId = tmpAttri.AttributeId,
                                                    ValueEn = tmpAttri.ValueEn,
                                                    ValueTh = tmpAttri.ValueTh,
                                                    CheckboxValue = tmpAttri.CheckboxValue,
                                                    CreateBy = tmpAttri.CreateBy,
                                                    CreateOn = tmpAttri.CreateOn,
                                                    IsAttributeValue = tmpAttri.IsAttributeValue,
                                                    Position = tmpAttri.Position,
                                                    UpdateBy = tmpAttri.UpdateBy,
                                                    UpdateOn = tmpAttri.UpdateOn
                                                });
                                            }
                                        }
                                    }
                                    if (attributeTmp != null && attributeTmp.Count > 0)
                                    {
                                        db.ProductStageAttributes.RemoveRange(attributeTmp);
                                    }
                                    #endregion
                                }
                                else
                                {
                                    isNewStage = true;
                                }
                            }
                            if (isNewStage)
                            {
                                if (string.IsNullOrEmpty(staging.Pid))
                                {
                                    errorMessage.Add("New variant cannot have Pid " + staging.Pid);
                                    continue;
                                }
                                staging.Status = Constant.PRODUCT_STATUS_DRAFT;
                                groupEn.ProductStages.Add(staging);
                            }
                        }
                    }
                    if (stageList != null && stageList.Count > 0)
                    {
                        db.ProductStages.RemoveRange(stageList);
                    }
                    #endregion
                    #region Flag
                    var masterVariant = groupEn.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
                    if (!string.IsNullOrWhiteSpace(masterVariant.ProductNameEn)
                        && !string.IsNullOrWhiteSpace(masterVariant.ProductNameTh)
                        && !string.IsNullOrWhiteSpace(masterVariant.Sku)
                        && groupEn.BrandId != null
                        && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullEn)
                        && !string.IsNullOrWhiteSpace(masterVariant.DescriptionFullTh)
                        && !string.IsNullOrWhiteSpace(masterVariant.MobileDescriptionEn)
                        && !string.IsNullOrWhiteSpace(masterVariant.MobileDescriptionTh))
                    {
                        groupEn.InfoFlag = true;
                    }
                    else
                    {
                        groupEn.InfoFlag = false;
                    }
                    #endregion
                    masterVariant.VariantCount = groupEn.ProductStages.Where(w => w.IsVariant == true).ToList().Count;
                    groupEn.UpdateOn = DateTime.Now;
                    groupEn.UpdateBy = User.UserRequest().Email;
                }
                #region Validate Error Message
                if (errorMessage.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
                }
                #endregion
                #region Setup Product for database
                foreach (var product in groupList)
                {
                    var masterProduct = product.Value.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
                    masterProduct.VariantCount = product.Value.ProductStages.Where(w => w.IsVariant == true).Count();
                    if(masterProduct.VariantCount == 0)
                    {
                        masterProduct.IsSell = true;
                    }

                    AutoGenerate.GeneratePid(db, product.Value.ProductStages);
                }
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK, "Total " + groupList.Count + " products updated");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            finally
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }


        /*
       

        [Route("api/ProductStages/Guidance/Export")]
        [HttpGet]
        public HttpResponseMessage GetAllGuidance()
        {
            try
            {
                var guidance = db.ImportHeaders.Where(w=>!w.MapName.Equals("ATS") && !w.MapName.Equals("VO1")
                && !w.MapName.Equals("VO2")).Select(s => new { s.GroupName, s.HeaderName,s.MapName,s.ImportHeaderId }).OrderBy(o=>o.ImportHeaderId);
                return Request.CreateResponse(HttpStatusCode.OK, guidance);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/All")]
        [HttpGet]
        public HttpResponseMessage GetProductAllStages([FromUri] ProductRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;

                var products = (
                             from mast in db.ProductStages
                             join proImg in db.ProductStageImages on mast.Pid equals proImg.Pid into proImgJoin
                             join variant in db.ProductStageVariants on mast.ProductId equals variant.ProductId into varJoin
                             from vari in varJoin.DefaultIfEmpty()
                             join varImg in db.ProductStageImages on vari.Pid equals varImg.Pid into varImgJoin
                             let comm = db.ProductStageComments.Where(w => w.Pid.Equals(mast.Pid)).OrderByDescending(o => o.UpdateOn).FirstOrDefault()
                             let commVar = db.ProductStageComments.Where(w => w.Pid.Equals(vari.Pid)).OrderByDescending(o => o.UpdateOn).FirstOrDefault()
                             where mast.ShopId == shopId
                             select new
                             {

                                 mast.ProductId,
                                 Sku = vari != null ? vari.Sku : mast.Sku,
                                 Upc = vari != null ? vari.Upc : mast.Upc,
                                 ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
                                 ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
                                 Pid = vari != null ? vari.Pid : mast.Pid,
                                 Status = vari != null ? vari.Status : mast.Status,
                                 MasterImg = proImgJoin.Select(s => new ImageRequest { ImageId = s.ImageId, url = s.ImageUrlEn, tmpPath = s.Path, position = s.Position }).OrderBy(o => o.position),
                                 VariantImg = varImgJoin.Select(s => new ImageRequest { ImageId = s.ImageId, url = s.ImageUrlEn, tmpPath = s.Path, position = s.Position }).OrderBy(o => o.position),
                                 IsVariant = vari != null ? true : false,
                                 Comment = commVar != null ? commVar.Comment : commVar.Comment,
                                 VariantAttribute = vari.ProductStageVariantArrtibuteMaps.Select(s => new
                                 {
                                     s.Attribute.AttributeNameEn,
                                     Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.Value) select tt.AttributeValueEn).FirstOrDefault()
                                         : s.Value,
                                 })
                             });
                var products = (from stage in db.ProductStages
                                join proImg in db.ProductStageImages on stage.Pid equals proImg.Pid into proImgJoin
                                join variant in db.ProductStageVariants.Include(i => i.ProductStageVariantArrtibuteMaps) on stage.ProductId equals variant.ProductId into varJoin
                                from varJ in varJoin.DefaultIfEmpty()
                                    //join varMap in db.ProductStageVariantArrtibuteMaps on varJ.VariantId equals varMap.VariantId into varMapJ
                                    //from varMap in varMapJ.DefaultIfEmpty()
                                    //join attrVal in db.AttributeValues on varMap.Value equals attrVal.MapValue into attrValJ
                                    //from attraVal in attrValJ.DefaultIfEmpty()
                                join varImg in db.ProductStageImages on varJ.Pid equals varImg.Pid into varImgJoin
                                let comm = db.ProductStageComments.Where(w => w.Pid.Equals(stage.Pid)).OrderByDescending(o => o.UpdateOn).FirstOrDefault()
                                let commVar = db.ProductStageComments.Where(w => w.Pid.Equals(varJ.Pid)).OrderByDescending(o => o.UpdateOn).FirstOrDefault()
                                where stage.ShopId == shopId
                                select new
                                {
                                    stage.ProductId,
                                    Sku = varJ != null ? varJ.Sku : stage.Sku,
                                    Upc = varJ != null ? varJ.Upc : stage.Upc,
                                    ProductNameEn = varJ != null ? varJ.ProductNameEn : stage.ProductNameEn,
                                    ProductNameTh = varJ != null ? varJ.ProductNameTh : stage.ProductNameTh,
                                    Pid = varJ != null ? varJ.Pid : stage.Pid,
                                    VariantValue = "", //todo
                                    Status = varJ != null ? varJ.Status : stage.Status,
                                    MasterImg = proImgJoin.Select(s => new ImageRequest { ImageId = s.ImageId, url = s.ImageUrlEn, tmpPath = s.Path, position = s.Position }).OrderBy(o => o.position),
                                    VariantImg = varImgJoin.Select(s => new ImageRequest { ImageId = s.ImageId, url = s.ImageUrlEn, tmpPath = s.Path, position = s.Position }).OrderBy(o => o.position),
                                    IsVariant = varJ != null ? true : false,
                                    Comment = commVar != null ? commVar.Comment : commVar.Comment,
                                });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, products);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText)
                    || p.Pid.Contains(request.SearchText)
                    || p.Upc.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request.Pid))
                {
                    products = products.Where(p => p.Pid.Equals(request.Pid));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("ImageMissing", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(w => w.MasterImg.Count() == 0 && w.VariantImg.Count() == 0);
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                    else if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
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

        [Route("api/ProductStages/All/Image")]
        [HttpPut]
        public HttpResponseMessage SaveChangeAllImage(List<VariantRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                int shopId = this.User.ShopRequest().ShopId.Value;
                var productIds = request.Where(w => w.ProductId != null).Select(s => s.ProductId).ToList();
                var products = db.ProductStages.Where(w => w.ShopId == shopId && productIds.Contains(w.ProductId)).Include(i=>i.ProductStageVariants).ToList();
                foreach (VariantRequest varRq in request)
                {
                    if (varRq.IsVariant == null)
                    {
                        throw new Exception("Invalid variant flag");
                    }
                    var pro = products.Where(w => w.ProductId == varRq.ProductId).SingleOrDefault();
                    if (varRq.IsVariant.Value)
                    {
                        pro.FeatureImgUrl = SaveChangeImg(db, varRq.Pid, shopId, varRq.VariantImg, this.User.UserRequest().Email);
                    }
                    else
                    {
                        pro.FeatureImgUrl = SaveChangeImg(db, varRq.Pid, shopId, varRq.MasterImg, this.User.UserRequest().Email);
                    }
                    
                    if(pro != null && Constant.PRODUCT_STATUS_APPROVE.Equals(pro.Status))
                    {
                        pro.Status = Constant.PRODUCT_STATUS_DRAFT;
                        pro.ProductStageVariants.ToList().ForEach(e => e.Status = Constant.PRODUCT_STATUS_DRAFT);
                    }
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateErrorResponse(HttpStatusCode.OK, "Save successful");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Template")]
        [HttpPost]
        public HttpResponseMessage ExportTemplate(CSVTemplateRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var guidance = db.ImportHeaders.OrderBy(o=>o.ImportHeaderId).ToList();
                List<string> header = new List<string>();
                foreach (var g in guidance)
                {
                    header.Add(g.HeaderName);
                }

                if (request.GlobalCategories != null)
                {
                    List<int> categoryIds = request.GlobalCategories.Select(s => s.CategoryId.Value).ToList();
                    var categories = db.GlobalCategories.Where(w => categoryIds.Contains(w.CategoryId)).Select(s=>
                    new {
                        s.NameEn,
                        s.CategoryId,
                        AttribuyeSet = s.CategoryAttributeSetMaps.Select(se=>
                        new {
                            se.AttributeSet.AttributeSetNameEn,
                            Attribute = se.AttributeSet.AttributeSetMaps.Select(sa=>sa.Attribute.AttributeNameEn)
                        })
                    }).ToList();
                    if(categories != null && categories.Count > 0)
                    {
                        HashSet<string> attribute = new HashSet<string>();
                        foreach(var cat in categories)
                        {
                            foreach(var attibutS in cat.AttribuyeSet)
                            {
                                foreach(var attr in attibutS.Attribute)
                                {
                                    attribute.Add(attr);
                                }
                            }
                        }
                        if(attribute != null && attribute.Count > 0)
                        {
                            header.AddRange(attribute.ToList());
                        }
                    }
                }
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                var csv = new CsvWriter(writer);
                foreach (string h in header)
                {
                    csv.WriteField(h);
                }
                csv.NextRecord();
                writer.Flush();
                stream.Position = 0;
                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "file.csv";
                return result;
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Import")]
        [HttpPost]
        public async Task<HttpResponseMessage> ImportProduct()
        {
            string fileName = string.Empty;
            HashSet<string> errorMessage = new HashSet<string>();
            int row = 2;
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Content Multimedia");
                }
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                if(streamProvider.FileData == null || streamProvider.FileData.Count == 0)
                {
                    throw new Exception("No file uploaded");
                }
                fileName = streamProvider.FileData[0].LocalFileName;
                var fileReader = File.OpenText(fileName);
                using (var csvResult = new CsvReader(fileReader))
                {
                    if (!csvResult.Read())
                    {
                        throw new Exception("File is not in a proper format");
                    }
                    Dictionary<string, int> headDic = new Dictionary<string, int>();
                    IEnumerable<IEnumerable<string>> csvRows = null;
                    int i = 0;
                    string[] headers = csvResult.FieldHeaders;
                    List<string> firstRow = new List<string>();
                    foreach (string head in headers)
                    {
                        if (headDic.ContainsKey(head))
                        {
                            throw new Exception(head + " is duplicate header");
                        }
                        headDic.Add(head, i++);
                        firstRow.Add(csvResult.GetField<string>(head));
                    }
                    csvRows = ReadExcel(csvResult, headers,firstRow);

                    List<ProductStage> products = new List<ProductStage>();
                    #region Default Query
                    int shopId = this.User.ShopRequest().ShopId.Value;
                    var brands = db.Brands.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE)).Select(s => new { s.BrandNameEn, s.BrandId }).ToList();
                    var globalCatId = db.GlobalCategories.Where(w => w.Rgt - w.Lft == 1).Select(s => new { s.CategoryId }).ToList();
                    var localCatId = db.LocalCategories.Where(w => w.Rgt - w.Lft == 1 && w.ShopId == shopId).Select(s => new { s.CategoryId }).ToList();
                    var attributeSet = db.AttributeSets
                        .Where(w => w.Status.Equals(Constant.STATUS_ACTIVE))
                        .Select(s => new {
                            s.AttributeSetId,
                            s.AttributeSetNameEn,
                            Attribute = s.AttributeSetMaps.Select(se => new {
                                se.Attribute.AttributeId,
                                se.Attribute.AttributeNameEn,
                                se.Attribute.VariantStatus,
                                se.Attribute.DataType,
                                AttributeValue = se.Attribute.AttributeValueMaps.Select(sv => new { sv.AttributeValue.AttributeValueId, sv.AttributeValue.AttributeValueEn })
                            })
                        }).ToList();
                    var shipping = db.Shippings.ToList();
                    #endregion
                    #region Initialize
                    Dictionary<Tuple<string, int>, Inventory> inventoryList = new Dictionary<Tuple<string, int>, Inventory>();
                    Dictionary<string, ProductStage> groupList = new Dictionary<string, ProductStage>();
                    int tmpGroupId = 0;
                    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                    List<string> body = null;
                    string groupId = null;
                    bool isNew = true;
                    ProductStage group = null;
                    ProductStageVariant variant = null;
                    #endregion
                    foreach (var b in csvRows)
                    {
                        body = b.ToList();
                        #region Group
                        isNew = true;
                        groupId = string.Empty;
                        group = null;
                        if (headDic.ContainsKey("Group ID"))
                        {
                            Get column 'Group Id'.
                            groupId = body[headDic["Group ID"]];
                            if (rg.IsMatch(groupId))
                            {
                                errorMessage.Add("Invalid Group ID at row" + row);
                                continue;
                            }
                            if (groupList.ContainsKey(groupId))
                            {
                                group = groupList[groupId];
                                isNew = false;
                            }
                            else
                            {

                            }
                        }
                        if (group == null)
                        {
                            if (string.IsNullOrEmpty(groupId))
                            {
                                groupId = string.Concat("((", tmpGroupId++, "))");
                            }
                            group = new ProductStage()
                            {
                                ShopId = shopId,
                                Status = Constant.PRODUCT_STATUS_DRAFT,
                                Visibility = true,
                                CreateBy = this.User.UserRequest().Email,
                                CreateOn = DateTime.Now,
                                UpdateBy = this.User.UserRequest().Email,
                                UpdateOn = DateTime.Now
                            };
                        }
                        #endregion
                        #region Variant Detail
                        Initialise product stage variant
                        variant = new ProductStageVariant()
                        {
                            ShopId = shopId,
                            DefaultVaraint = false,
                            Status = Constant.PRODUCT_STATUS_DRAFT,
                            Visibility = true,
                            CreateBy = this.User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = this.User.UserRequest().Email,
                            UpdateOn = DateTime.Now
                        };
                        if (headDic.ContainsKey("Default Variant"))
                        {
                            string defaultVar = body[headDic["Default Variant"]];
                            variant.DefaultVaraint = "Yes".Equals(defaultVar);
                        }


                        variant.Sku = Validation.ValidateCSVStringColumn(headDic, body, "SKU", true, 300, errorMessage,row);
                        variant.Upc = Validation.ValidateCSVStringColumn(headDic, body, "UPC", false, 300, errorMessage, row);
                        variant.ProductNameEn = Validation.ValidateCSVStringColumn(headDic, body, "Product Name (English)", true, 300, errorMessage, row);
                        variant.ProductNameTh = Validation.ValidateCSVStringColumn(headDic, body, "Product Name (Thai)", true, 300, errorMessage, row);
                        variant.DescriptionFullEn = Validation.ValidateCSVStringColumn(headDic, body, "Description (English)", true, Int32.MaxValue, errorMessage, row);
                        variant.DescriptionFullTh = Validation.ValidateCSVStringColumn(headDic, body, "Description (Thai)", true, Int32.MaxValue, errorMessage, row);
                        variant.DescriptionShortEn = Validation.ValidateCSVStringColumn(headDic, body, "Short Description (English)", false, 500, errorMessage, row);
                        variant.DescriptionShortTh = Validation.ValidateCSVStringColumn(headDic, body, "Short Description (Thai)", false, 500, errorMessage, row);

                        
                        #endregion
                        #region Brand 
                        if (headDic.ContainsKey("Brand Name"))
                        {
                            var brandId = brands.Where(w => w.BrandNameEn.Equals(body[headDic["Brand Name"]])).Select(s => s.BrandId).FirstOrDefault();
                            if (brandId != 0)
                            {
                                group.BrandId = brandId;
                            }
                            else
                            {
                                errorMessage.Add("Invalid Brand Name at row " + row);
                            }
                        }
                        #endregion
                        #region Shipping 
                        if (headDic.ContainsKey("Shipping Method"))
                        {
                            var shippingId = shipping.Where(w => w.ShippingMethodEn.Equals(body[headDic["Shipping Method"]])).Select(s => s.ShippingId).FirstOrDefault();
                            if (shippingId != 0)
                            {
                                group.ShippingId = shippingId;
                            }
                            else
                            {
                                group.ShippingId = 1;
                            }
                        }
                        #endregion
                        #region Global category
                        if (headDic.ContainsKey("Global Category ID"))
                        {
                            try
                            {
                                var catIdSt = body[headDic["Global Category ID"]];
                                if (!string.IsNullOrWhiteSpace(catIdSt))
                                {
                                    int catId = Int32.Parse(catIdSt);
                                    var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                    if (cat != 0)
                                    {
                                        group.GlobalCatId = cat;
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                errorMessage.Add("Invalid Global Category ID at row " + row);
                            }
                        }
                        
                        #endregion
                        #region Local Category
                        if (headDic.ContainsKey("Local Category ID"))
                        {
                            try
                            {
                                var catIdSt = body[headDic["Local Category ID"]];
                                if (!string.IsNullOrWhiteSpace(catIdSt))
                                {
                                    int catId = Int32.Parse(catIdSt);
                                    var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                    if (cat != 0)
                                    {
                                        group.LocalCatId = cat;
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Local Category ID at row " + row);
                            }
                        }
                        
                        #endregion
                        #region Original Price
                        if (headDic.ContainsKey("Original Price"))
                        {
                            try
                            {
                                var originalPriceSt = body[headDic["Original Price"]];
                                if (!string.IsNullOrWhiteSpace(originalPriceSt))
                                {
                                    decimal originalPrice = Decimal.Parse(originalPriceSt);
                                    variant.OriginalPrice = originalPrice;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Original Price at row " + row);
                            }
                        }
                        #endregion
                        #region Sale Price
                        if (headDic.ContainsKey("Sale Price"))
                        {
                            try
                            {
                                var salePriceSt = body[headDic["Sale Price"]];
                                if (!string.IsNullOrWhiteSpace(salePriceSt))
                                {
                                    decimal salePrice = Decimal.Parse(salePriceSt);
                                    variant.SalePrice = salePrice;
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Sale Price at row " + row);
                            }
                        }
                        #endregion
                        #region Preparation Time
                        if (headDic.ContainsKey("Preparation Time"))
                        {
                            try
                            {
                                string preDay = body[headDic["Preparation Time"]];
                                if (!string.IsNullOrWhiteSpace(preDay))
                                {
                                    variant.PrepareDay = Int32.Parse(preDay);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Preparation Time at row " + row);
                            }
                        }
                        #endregion
                        #region Package Dimension
                        if (headDic.ContainsKey("Package Dimension - Lenght (mm)"))
                        {
                            try
                            {
                                string val = body[headDic["Package Dimension - Lenght (mm)"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    variant.Length = Decimal.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Package Dimension - Lenght (mm) at row " + row);
                            }
                        }

                        if (headDic.ContainsKey("Package Dimension - Height (mm)"))
                        {
                            try
                            {
                                string val = body[headDic["Package Dimension - Height (mm)"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    variant.Height = Decimal.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Package Dimension - Height (mm) at row " + row);
                            }
                        }

                        if (headDic.ContainsKey("Package Dimension - Width (mm)"))
                        {
                            try
                            {
                                string val = body[headDic["Package Dimension - Width (mm)"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    variant.Width = Decimal.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Package Dimension - Width (mm)  at row " + row);
                            }
                        }

                        if (headDic.ContainsKey("Package - Weight (g)"))
                        {
                            try
                            {
                                string val = body[headDic["Package - Weight (g)"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    variant.Weight = Decimal.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Package - Weight (g) at row " + row);
                            }
                        }
                        variant.DimensionUnit = "MM";
                        variant.WeightUnit = "G";
                        #endregion
                        #region Inventory Amount
                        Inventory inventory = null;
                        if (headDic.ContainsKey("Inventory Amount"))
                        {
                            try
                            {
                                string val = body[headDic["Inventory Amount"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (inventory == null)
                                    {
                                        inventory = new Inventory()
                                        {
                                            CreateBy = this.User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = this.User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["Stock Type"]]))
                                        {
                                            inventory.StockAvailable = Constant.STOCK_TYPE[body[headDic["Stock Type"]]];
                                        }
                                        else
                                        {
                                            inventory.StockAvailable = 1;
                                        }
                                        
                                    }
                                    inventory.Quantity = Int32.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Inventory Amount at row " + row);
                            }
                        }
                        #endregion
                        #region Safety Stock Amount
                        if (headDic.ContainsKey("Safety Stock Amount"))
                        {
                            try
                            {
                                string val = body[headDic["Safety Stock Amount"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    if (inventory == null)
                                    {
                                        inventory = new Inventory()
                                        {
                                            CreateBy = this.User.UserRequest().Email,
                                            CreateOn = DateTime.Now,
                                            UpdateBy = this.User.UserRequest().Email,
                                            UpdateOn = DateTime.Now,
                                        };
                                        if (Constant.STOCK_TYPE.ContainsKey(body[headDic["Stock Type"]]))
                                        {
                                            inventory.StockAvailable = Constant.STOCK_TYPE[body[headDic["Stock Type"]]];
                                        }
                                        else
                                        {
                                            inventory.StockAvailable = 1;
                                        }
                                    }
                                    inventory.SafetyStockSeller = Int32.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Safety Stock Amount at row " + row);
                            }
                        }
                        if (inventory != null)
                        {
                            inventoryList.Add(new Tuple<string, int>(groupId, group.ProductStageVariants.Count), inventory);
                        }
                        #endregion
                        #region Product Boosting Weight
                        if (headDic.ContainsKey("Product Boosting Weight"))
                        {
                            try
                            {
                                string val = body[headDic["Product Boosting Weight"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    variant.BoostWeight = Int32.Parse(val);
                                }
                            }
                            catch
                            {
                                errorMessage.Add("Invalid Product Boosting Weight at row " + row);
                            }
                        }
                        #endregion
                        #region SEO

                        variant.MetaTitleEn = Validation.ValidateCSVStringColumn(headDic, body, "Meta Title (English)", false, 300, errorMessage, row);
                        variant.MetaTitleTh = Validation.ValidateCSVStringColumn(headDic, body, "Meta Title (Thai)", false, 300, errorMessage, row);
                        variant.MetaDescriptionEn = Validation.ValidateCSVStringColumn(headDic, body, "Meta Description (English)", false, 500, errorMessage, row);
                        variant.MetaDescriptionTh = Validation.ValidateCSVStringColumn(headDic, body, "Meta Description (Thai)", false, 500, errorMessage, row);
                        variant.MetaKeyEn = Validation.ValidateCSVStringColumn(headDic, body, "Meta Keywords (English)", false, 300, errorMessage, row);
                        variant.MetaKeyTh = Validation.ValidateCSVStringColumn(headDic, body, "Meta Keywords (Thai)", false, 300, errorMessage, row);
                        variant.UrlEn = Validation.ValidateCSVStringColumn(headDic, body, "Product URL Key (English)", false, 300, errorMessage, row);
                        variant.Display = "GROUP";
                        #endregion
                        #region More Detail
                        group.Tag = Validation.ValidateCSVStringColumn(headDic, body, "Search Tag", false, 630, errorMessage,row);
                        group.EffectiveDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "Effective Date");
                        group.EffectiveTime = Validation.ValidateCSVTimeSpanColumn(headDic, body, "Effective Time");
                        group.ExpiryDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "Expire Date");
                        group.ExpiryTime = Validation.ValidateCSVTimeSpanColumn(headDic, body, "Expire Time");
                        group.Remark = Validation.ValidateCSVStringColumn(headDic, body, "Remark", false, 500, errorMessage,row);

                        if (headDic.ContainsKey("Flag 1"))
                        {
                            group.ControlFlag1 = string.Equals(body[headDic["Flag 1"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false ;
                        }
                        if (headDic.ContainsKey("Flag 2"))
                        {
                            group.ControlFlag2 = string.Equals(body[headDic["Flag 2"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                        }
                        if (headDic.ContainsKey("Flag 3"))
                        {
                            group.ControlFlag3 = string.Equals(body[headDic["Flag 3"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
                        }

                        if (variant.DefaultVaraint.Value || isNew)
                        {
                            group.Sku = variant.Sku;
                            group.Upc = variant.Upc;
                            group.ProductNameEn = variant.ProductNameEn;
                            group.ProductNameTh = variant.ProductNameTh;
                            group.DescriptionFullEn = variant.DescriptionFullEn;
                            group.DescriptionFullTh = variant.DescriptionFullTh;
                            group.DescriptionShortEn = variant.DescriptionShortEn;
                            group.DescriptionShortTh = variant.DescriptionShortTh;
                            group.SalePrice = variant.SalePrice;
                            group.OriginalPrice = variant.OriginalPrice;
                            group.PrepareDay = variant.PrepareDay;
                            group.Length = variant.Length;
                            group.Height = variant.Height;
                            group.Width = variant.Width;
                            group.Weight = variant.Weight;
                            group.DimensionUnit = variant.DimensionUnit;
                            group.WeightUnit = variant.WeightUnit;
                            group.BoostWeight = variant.BoostWeight;
                            group.MetaTitleEn = variant.MetaTitleEn;
                            group.MetaTitleTh = variant.MetaTitleTh;
                            group.MetaDescriptionEn = variant.MetaDescriptionEn;
                            group.MetaDescriptionTh = variant.MetaDescriptionTh;
                            group.MetaKeyEn = variant.MetaKeyEn;
                            group.MetaKeyTh = variant.MetaKeyTh;
                            group.UrlEn = variant.UrlEn;

                            if (headDic.ContainsKey("Alternative Global Category 1"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["Alternative Global Category 1"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = Int32.Parse(catIdSt);
                                        var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                                            {
                                                CategoryId = cat,
                                                Status = Constant.STATUS_ACTIVE,
                                                CreateBy = this.User.UserRequest().Email,
                                                CreateOn = DateTime.Now,
                                                UpdateBy = this.User.UserRequest().Email,
                                                UpdateOn = DateTime.Now,
                                            });
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    errorMessage.Add("Invalid Alternative Global Category 1 at row " + row);
                                }
                            }
                            if (headDic.ContainsKey("Alternative Global Category 2"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["Alternative Global Category 2"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = Int32.Parse(catIdSt);
                                        var cat = globalCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            group.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                                            {
                                                CategoryId = cat,
                                                Status = Constant.STATUS_ACTIVE,
                                                CreateBy = this.User.UserRequest().Email,
                                                CreateOn = DateTime.Now,
                                                UpdateBy = this.User.UserRequest().Email,
                                                UpdateOn = DateTime.Now,
                                            });
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    errorMessage.Add("Invalid Alternative Global Category 2 at row " + row);
                                }
                            }
                            if (headDic.ContainsKey("Alternative Local Category 1"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["Alternative Local Category 1"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = Int32.Parse(catIdSt);
                                        var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                                            {
                                                CategoryId = cat,
                                                Status = Constant.STATUS_ACTIVE,
                                                CreateBy = this.User.UserRequest().Email,
                                                CreateOn = DateTime.Now,
                                                UpdateBy = this.User.UserRequest().Email,
                                                UpdateOn = DateTime.Now,
                                            });
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch
                                {
                                    errorMessage.Add("Invalid Alternative Local Category 1 at row " + row);
                                }
                            }
                            if (headDic.ContainsKey("Alternative Local Category 2"))
                            {
                                try
                                {
                                    var catIdSt = body[headDic["Alternative Local Category 2"]];
                                    if (!string.IsNullOrWhiteSpace(catIdSt))
                                    {
                                        int catId = Int32.Parse(catIdSt);
                                        var cat = localCatId.Where(w => w.CategoryId == catId).Select(s => s.CategoryId).FirstOrDefault();
                                        if (cat != 0)
                                        {
                                            group.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                                            {
                                                CategoryId = cat,
                                                Status = Constant.STATUS_ACTIVE,
                                                CreateBy = this.User.UserRequest().Email,
                                                CreateOn = DateTime.Now,
                                                UpdateBy = this.User.UserRequest().Email,
                                                UpdateOn = DateTime.Now,
                                            });
                                        }
                                        else
                                        {
                                            throw new Exception();
                                        }
                                    }
                                }
                                catch
                                {
                                    errorMessage.Add("Invalid Local Category ID at row " + row);
                                }
                            }
                        }

                        #endregion
                        #region Attribute Set
                        if (headDic.ContainsKey("Attribute Set"))
                        {
                            try
                            {
                                string val = body[headDic["Attribute Set"]];
                                if (!string.IsNullOrWhiteSpace(val))
                                {
                                    var attrSet = attributeSet.Where(w => w.AttributeSetNameEn.Equals(val)).SingleOrDefault();
                                    if (attrSet == null)
                                    {
                                        throw new Exception("Attribute set " + val + " not found in database");
                                    }
                                    group.AttributeSetId = attrSet.AttributeSetId;
                                    var variant1 = Validation.ValidateCSVStringColumn(headDic, body, "Variation Option 1", false, 300, errorMessage,row);
                                    var variant2 = Validation.ValidateCSVStringColumn(headDic, body, "Variation Option 2", false, 300, errorMessage,row);
                                    foreach (var attr in attrSet.Attribute)
                                    {
                                        if (headDic.ContainsKey(attr.AttributeNameEn))
                                        {
                                            var value = Validation.ValidateCSVStringColumn(headDic, body, attr.AttributeNameEn, false, 300, errorMessage,row);
                                            bool isValue = false;
                                            if (attr.DataType.Equals(Constant.DATA_TYPE_LIST))
                                            {
                                                var valueId = attr.AttributeValue.Where(w => w.AttributeValueEn.Equals(value)).Select(s => s.AttributeValueId).FirstOrDefault();
                                                if (valueId == 0)
                                                {
                                                    throw new Exception("Invalid attribute value " + value + " in attribute " + attr.AttributeNameEn);
                                                }
                                                value = string.Concat("((", valueId, "))");
                                                isValue = true;
                                            }
                                            if (attr.AttributeNameEn.Equals(variant1))
                                            {
                                                if (!attr.VariantStatus.Value)
                                                {
                                                    throw new Exception("Invalid varint type");
                                                }
                                                if (variant.ProductStageVariantArrtibuteMaps.All(a => a.AttributeId != attr.AttributeId))
                                                {
                                                    variant.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                                    {
                                                        AttributeId = attr.AttributeId,
                                                        VariantId = variant.VariantId,
                                                        Value = value,
                                                        IsAttributeValue = isValue
                                                    });
                                                }

                                            }
                                            else if (attr.AttributeNameEn.Equals(variant2))
                                            {
                                                if (!attr.VariantStatus.Value)
                                                {
                                                    throw new Exception();
                                                }
                                                if (variant.ProductStageVariantArrtibuteMaps.All(a => a.AttributeId != attr.AttributeId))
                                                {
                                                    variant.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                                    {
                                                        AttributeId = attr.AttributeId,
                                                        VariantId = variant.VariantId,
                                                        Value = value,
                                                        IsAttributeValue = isValue
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                if (group.ProductStageAttributes.All(a => a.AttributeId != attr.AttributeId))
                                                {
                                                    group.ProductStageAttributes.Add(new ProductStageAttribute()
                                                    {
                                                        AttributeId = attr.AttributeId,
                                                        ProductId = group.ProductId,
                                                        ValueEn = value,
                                                        IsAttributeValue = isValue
                                                    });
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                errorMessage.Add(e.Message + " at row " + row);
                            }
                        }
                        #endregion
                        variant.ProductId = group.ProductId;
                        group.ProductStageVariants.Add(variant);

                        if (!groupList.ContainsKey(groupId))
                        {
                            groupList.Add(groupId, group);
                        }
                        row++;
                    }
                    int varCount = 0;
                    foreach (var product in groupList)
                    {
                        string masterPid = AutoGenerate.NextPID(db, product.Value.GlobalCatId);
                        product.Value.Pid = masterPid;
                        int masterQuantity = 0;
                        int safetyStock = 0;
                        for (int varIndex = 0; varIndex < product.Value.ProductStageVariants.Count; varIndex++)
                        {
                            Tuple<string, int> tmpInventory = new Tuple<string, int>(product.Key, varIndex);
                            if (product.Value.ProductStageVariants.ElementAt(varIndex).ProductStageVariantArrtibuteMaps.Count == 0)
                            {
                                if (inventoryList.ContainsKey(tmpInventory))
                                {
                                    masterQuantity = inventoryList[tmpInventory].Quantity;
                                    safetyStock = inventoryList[tmpInventory].SafetyStockSeller;
                                    inventoryList.Remove(tmpInventory);
                                }
                                product.Value.ProductStageVariants.Remove(product.Value.ProductStageVariants.ElementAt(varIndex--));
                            }
                            else
                            {
                                string pid = AutoGenerate.NextPID(db, product.Value.GlobalCatId);
                                product.Value.ProductStageVariants.ElementAt(varIndex).Pid = pid;
                                if (inventoryList.ContainsKey(tmpInventory))
                                {
                                    inventoryList[tmpInventory].Pid = pid;
                                    db.Inventories.Add(inventoryList[tmpInventory]);
                                    if (product.Value.ProductStageVariants.ElementAt(varIndex).DefaultVaraint.Value)
                                    {
                                        masterQuantity = inventoryList[tmpInventory].Quantity;
                                        safetyStock = inventoryList[tmpInventory].SafetyStockSeller;
                                    }
                                }
                                ++varCount;
                            }
                        }
                        db.Inventories.Add(new Inventory()
                        {
                            Pid = masterPid,
                            Quantity = masterQuantity,
                            SafetyStockSeller = safetyStock,
                            StockAvailable = 1,
                            CreateBy = this.User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = this.User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                        if (string.IsNullOrEmpty(product.Value.UrlEn))
                        {
                            product.Value.UrlEn = masterPid;
                        }

                        db.ProductStages.Add(product.Value);
                    }

                    if (errorMessage.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
                    }
                    Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                    return Request.CreateResponse(HttpStatusCode.OK, "Total " + groupList.Count + " products with " + varCount + " variants imported successfully");
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            finally
            {
                if(File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }


        
        

        [Route("api/ProductStages")]
        [HttpGet]
        [ClaimsAuthorize(Permission=new string[] { "View Product", "View All Product" })]
        public HttpResponseMessage GetProductStages([FromUri] ProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var products = (from p in db.ProductStages
                                where !Constant.STATUS_REMOVE.Equals(p.Status)
                                select new {
                                    p.Sku,
                                    p.Pid,
                                    p.Upc,
                                    p.ProductId,
                                    p.ProductNameEn,
                                    p.ProductNameTh,
                                    p.OriginalPrice,
                                    p.SalePrice,
                                    p.Status,
                                    p.ImageFlag,
                                    p.InfoFlag,
                                    p.Visibility,
                                    VariantCount = p.ProductStageVariants.Where(w => w.Visibility == true).ToList().Count,
                                    ImageUrl = p.FeatureImgUrl,
                                    p.GlobalCatId,
                                    p.LocalCatId,
                                    p.AttributeSetId,
                                    p.ProductStageAttributes,
                                    p.UpdateOn,
                                    p.ShopId,
                                    p.InformationTabStatus,
                                    p.ImageTabStatus,
                                    p.CategoryTabStatus,
                                    p.VariantTabStatus,
                                    p.MoreOptionTabStatus,
                                    PriceTo = p.ProductStageVariants.Max(m => m.SalePrice),
                                    PriceFrom = p.ProductStageVariants.Min(m => m.SalePrice),
                                    PriceTo = p.ProductStageVariants.Count == 0 ?  p.SalePrice :
                                            p.SalePrice < p.ProductStageVariants.Max(m => m.SalePrice)
                                            ? p.ProductStageVariants.Where(w => true).Max(m => m.SalePrice) : p.SalePrice,
                                    PriceFrom = p.SalePrice < p.ProductStageVariants.Where(w => true).Min(m => m.SalePrice)
                                            ? p.SalePrice : p.ProductStageVariants.Where(w => true).Min(m => m.SalePrice),
                                    Shop = new { p.Shop.ShopId, p.Shop.ShopNameEn }
                                });
                if (this.User.HasPermission("View Product"))
                {
                    int shopId = this.User.ShopRequest().ShopId.Value;
                    products = products.Where(w => w.ShopId == shopId);
                }
                request.DefaultOnNull();
                if (request.GlobalCatId != null)
                {
                    products = products.Where(p => p.GlobalCatId == request.GlobalCatId);
                }
                if (request.LocalCatId != null)
                {
                    products = products.Where(p => p.LocalCatId == request.LocalCatId);
                }
                if (request.AttributeSetId != null)
                {
                    products = products.Where(p => p.LocalCatId == request.LocalCatId);
                }
                if (request.AttributeId != null)
                {
                    products = products.Where(p => p.ProductStageAttributes.All(a => a.AttributeId == request.AttributeId));
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText)
                    || p.Pid.Contains(request.SearchText)
                    || p.Upc.Contains(request.SearchText));
                }
                if (!string.IsNullOrEmpty(request.Pid))
                {
                    products = products.Where(p => p.Pid.Equals(request.Pid));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {

                    if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                }
                if (!string.IsNullOrEmpty(request._missingfilter))
                {
                    if (string.Equals("Information", request._missingfilter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("Image", request._missingfilter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("Variation", request._missingfilter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("More", request._missingfilter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => !p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if (string.Equals("ReadyForAction", request._missingfilter, StringComparison.OrdinalIgnoreCase))
                    {
                        products = products.Where(p => 
                           p.InformationTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.ImageTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.VariantTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE)
                        && p.MoreOptionTabStatus.Equals(Constant.PRODUCT_STATUS_APPROVE));
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

        [Route("api/ProductStages/{productId}")]
        [HttpGet]
        public HttpResponseMessage GetProductStage(int productId)
        {
            try
            {
                var response = SetupProductStageRequestFromProductId(db, productId);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages")]
        [HttpPost]
        public HttpResponseMessage AddProduct(ProductStageRequest request)
        { 
            ProductStage stage = null;
            try
            {
                if (this.User.ShopRequest().ShopId == 0)
                {
                    throw new Exception("Shop is invalid. Cannot find shop in session");
                }
                Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                #region Setup Master Product
                stage = new ProductStage();

                SetupProductStage(db, stage, request);
                stage.Status = Constant.PRODUCT_STATUS_JUNK;
                int shopId = this.User.ShopRequest().ShopId.Value;
                stage.ShopId = shopId;
                string masterPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                stage.Pid = masterPid;
                if (string.IsNullOrWhiteSpace(request.SEO.ProductUrlKeyEn))
                {
                    stage.UrlEn = stage.Pid;
                }
                else
                {
                    stage.UrlEn = request.SEO.ProductUrlKeyEn;
                }

                stage.OnlineFlag = false;
                stage.Visibility = true;
                stage.CreateBy = this.User.UserRequest().Email;
                stage.CreateOn = DateTime.Now;
                stage.UpdateBy = this.User.UserRequest().Email;
                stage.UpdateOn = DateTime.Now;
               
                #endregion
                stage.Status = request.Status;

                #region Setup Master Attribute
                if (request.MasterAttribute != null)
                {
                    SetupAttributeEntity(db, request.MasterAttribute, stage.ProductId, masterPid, this.User.UserRequest().Email);
                }
                #endregion
                #region Setup Inventory
                Inventory masterInventory = new Inventory();
                masterInventory.Quantity = Validation.ValidationInteger(request.MasterVariant.Quantity, "Quantity", true, Int32.MaxValue, 0).Value;
                masterInventory.SafetyStockSeller = request.MasterVariant.SafetyStock;
                if (request.MasterVariant.StockType != null)
                {
                    if (Constant.STOCK_TYPE.ContainsKey(request.MasterVariant.StockType))
                    {
                        masterInventory.StockAvailable = Constant.STOCK_TYPE[request.MasterVariant.StockType];
                    }
                }
                masterInventory.Pid = masterPid;
                masterInventory.CreateBy = this.User.UserRequest().Email;
                masterInventory.CreateOn = DateTime.Now;
                db.Inventories.Add(masterInventory);

                InventoryHistory masterInventoryHist = new InventoryHistory();
                masterInventoryHist.StockAvailable = request.MasterVariant.Quantity;
                masterInventoryHist.SafetyStockSeller = request.MasterVariant.SafetyStock;
                if (request.MasterVariant.StockType != null)
                {
                    if (Constant.STOCK_TYPE.ContainsKey(request.MasterVariant.StockType))
                    {
                        masterInventoryHist.StockAvailable = Constant.STOCK_TYPE[request.MasterVariant.StockType];
                    }
                }
                masterInventoryHist.Pid = masterPid;
                masterInventoryHist.Description = "Add new product";
                masterInventoryHist.CreateBy = this.User.UserRequest().Email;
                masterInventoryHist.CreateOn = DateTime.Now;
                db.InventoryHistories.Add(masterInventoryHist);
                #endregion
                stage.FeatureImgUrl = SetupImgEntity(db, request.MasterVariant.Images, masterPid, shopId, this.User.UserRequest().Email);
                stage.ImageFlag = stage.FeatureImgUrl == null ? false : true;
                SetupImg360Entity(db, request.MasterVariant.Images360, masterPid, shopId, this.User.UserRequest().Email);
                SetupVdoEntity(db, request.MasterVariant.VideoLinks, masterPid, shopId, this.User.UserRequest().Email);
                #region Setup Related GlobalCategories
                if (request.GlobalCategories != null)
                {
                    foreach (CategoryRequest cat in request.GlobalCategories)
                    {
                        if (cat == null || cat.CategoryId == null) { continue; }
                        ProductStageGlobalCatMap map = new ProductStageGlobalCatMap();
                        map.CategoryId = cat.CategoryId.Value;
                        map.ProductId = stage.ProductId;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreateBy = this.User.UserRequest().Email;
                        map.CreateOn = DateTime.Now;
                        db.ProductStageGlobalCatMaps.Add(map);
                    }
                }
                #endregion
                #region Setup Related LocalCategories
                if (request.LocalCategories != null)
                {
                    foreach (CategoryRequest cat in request.LocalCategories)
                    {
                        if (cat == null || cat.CategoryId == null) { continue; }
                        ProductStageLocalCatMap map = new ProductStageLocalCatMap();
                        map.CategoryId = cat.CategoryId.Value;
                        map.ProductId = stage.ProductId;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreateBy = this.User.UserRequest().Email;
                        map.CreateOn = DateTime.Now;
                        db.ProductStageLocalCatMaps.Add(map);
                    }
                }
                #endregion
                #region Setup Related Product
                if (request.RelatedProducts != null)
                {
                    foreach (var pro in request.RelatedProducts)
                    {
                        if (pro == null) { continue; }
                        ProductStageRelated relate = new ProductStageRelated();
                        relate.Pid1 = masterPid;
                        relate.Pid2 = pro.Pid;
                        relate.ShopId = shopId;
                        relate.CreateBy = this.User.UserRequest().Email;
                        relate.CreateOn = DateTime.Now;
                        relate.UpdateBy = this.User.UserRequest().Email;
                        relate.UpdateOn = DateTime.Now;
                        db.ProductStageRelateds.Add(relate);
                    }
                }
                #endregion
                #region Setup variant
                if (request.Variants != null && request.Variants.Count > 0)
                {
                    foreach (VariantRequest variantRq in request.Variants)
                    {
                        if (variantRq.FirstAttribute == null &&
                            variantRq.SecondAttribute == null)
                        {
                            throw new Exception("Invalid variant format");
                        }

                        ProductStageVariant variant = new ProductStageVariant();
                       
                        variant.ProductId = stage.ProductId;
                        variant.Status = request.Status;
                        if (variantRq.FirstAttribute != null && variantRq.FirstAttribute.AttributeId != null)
                        {
                            if(variantRq.FirstAttribute.AttributeValues != null && variantRq.FirstAttribute.AttributeValues.Count > 0)
                            {
                                foreach(AttributeValueRequest val in variantRq.FirstAttribute.AttributeValues)
                                {
                                    variant.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap() {
                                        VariantId = variant.VariantId,
                                        AttributeId = variantRq.FirstAttribute.AttributeId.Value,
                                        Value = string.Concat("((",val.AttributeValueId,"))"),
                                        IsAttributeValue = true,
                                        CreateBy = this.User.UserRequest().Email,
                                        CreateOn = DateTime.Now,
                                        UpdateBy = this.User.UserRequest().Email,
                                        UpdateOn = DateTime.Now
                                    });
                                }
                            }
                            else
                            {
                                if (rg.IsMatch(variantRq.FirstAttribute.ValueEn))
                                {
                                    throw new Exception("Attribute value not allow");
                                }
                                variant.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                {
                                    VariantId = variant.VariantId,
                                    AttributeId = variantRq.FirstAttribute.AttributeId.Value,
                                    Value = variantRq.FirstAttribute.ValueEn,
                                    IsAttributeValue = false,
                                    CreateBy = this.User.UserRequest().Email,
                                    CreateOn = DateTime.Now,
                                    UpdateBy = this.User.UserRequest().Email,
                                    UpdateOn = DateTime.Now
                                });
                                
                            }
                        }
                        if (variantRq.SecondAttribute != null && variantRq.SecondAttribute.AttributeId != null)
                        {
                            if (variantRq.SecondAttribute.AttributeValues != null && variantRq.SecondAttribute.AttributeValues.Count > 0)
                            {
                                foreach (AttributeValueRequest val in variantRq.SecondAttribute.AttributeValues)
                                {
                                    variant.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                    {
                                        VariantId = variant.VariantId,
                                        AttributeId = variantRq.SecondAttribute.AttributeId.Value,
                                        Value = string.Concat("((", val.AttributeValueId, "))"),
                                        IsAttributeValue = true,
                                        CreateBy = this.User.UserRequest().Email,
                                        CreateOn = DateTime.Now,
                                        UpdateBy = this.User.UserRequest().Email,
                                        UpdateOn = DateTime.Now
                                    });
                                }
                            }
                            else
                            {
                                if (rg.IsMatch(variantRq.SecondAttribute.ValueEn))
                                {
                                    throw new Exception("Attribute value not allow");
                                }
                                variant.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                {
                                    VariantId = variant.VariantId,
                                    AttributeId = variantRq.SecondAttribute.AttributeId.Value,
                                    Value = variantRq.SecondAttribute.ValueEn,
                                    IsAttributeValue = false,
                                    CreateBy = this.User.UserRequest().Email,
                                    CreateOn = DateTime.Now,
                                    UpdateBy = this.User.UserRequest().Email,
                                    UpdateOn = DateTime.Now
                                });
                            }
                            
                        }
                        string variantPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                        variant.Pid = variantPid;
                        #region Setup Variant Inventory
                        Inventory variantInventory = new Inventory();
                        variantInventory.Quantity = Validation.ValidationInteger(variantRq.Quantity, "Quantity", true, Int32.MaxValue, 0).Value;
                        variantInventory.SafetyStockSeller = variantRq.SafetyStock;
                        if (!string.IsNullOrEmpty(request.MasterVariant.StockType))
                        {
                            if (Constant.STOCK_TYPE.ContainsKey(request.MasterVariant.StockType))
                            {
                                variantInventory.StockAvailable = Constant.STOCK_TYPE[request.MasterVariant.StockType];
                            }
                        }

                        variantInventory.Pid = variantPid;
                        db.Inventories.Add(variantInventory);

                        InventoryHistory variantInventoryHist = new InventoryHistory();
                        variantInventoryHist.StockAvailable = variantRq.Quantity;
                        variantInventoryHist.SafetyStockSeller = variantRq.SafetyStock;
                        if (!string.IsNullOrEmpty(request.MasterVariant.StockType))
                        {
                            if (Constant.STOCK_TYPE.ContainsKey(request.MasterVariant.StockType))
                            {
                                variantInventoryHist.StockAvailable = Constant.STOCK_TYPE[request.MasterVariant.StockType];
                            }
                        }
                        variantInventoryHist.Pid = variantPid;
                        variantInventoryHist.Description = "Add new variant";
                        variantInventoryHist.CreateBy = this.User.UserRequest().Email;
                        variantInventoryHist.CreateOn = DateTime.Now;
                        db.InventoryHistories.Add(masterInventoryHist);
                        #endregion
                        
                        if (string.IsNullOrWhiteSpace(variantRq.SEO.ProductUrlKeyEn))
                        {
                            variant.UrlEn = variantPid;
                        }
                        else
                        {
                            variant.UrlEn = variantRq.SEO.ProductUrlKeyEn;
                        }
                        SetupImgEntity(db, variantRq.Images, variantPid, shopId, this.User.UserRequest().Email);
                        SetupVdoEntity(db, variantRq.VideoLinks, variantPid, shopId, this.User.UserRequest().Email);
                        SetupProductStageVariant(variant, variantRq);
                        variant.ShopId = stage.ShopId;
                        variant.CreateBy = this.User.UserRequest().Email;
                        variant.CreateOn = DateTime.Now;
                        variant.UpdateBy = this.User.UserRequest().Email;
                        variant.UpdateOn = DateTime.Now;
                        stage.ProductStageVariants.Add(variant);
                    }
                }

                #endregion
                db.ProductStages.Add(stage);
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return GetProductStage(stage.ProductId);
            }
            catch (Exception ex)
            {
                #region Rollback
                db.Dispose();
                if (stage != null && stage.ProductId > 0)
                {
                    db = new ColspEntities();
                    db.ProductStageAttributes.RemoveRange(db.ProductStageAttributes.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStageGlobalCatMaps.RemoveRange(db.ProductStageGlobalCatMaps.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStageLocalCatMaps.RemoveRange(db.ProductStageLocalCatMaps.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStageVariants.RemoveRange(db.ProductStageVariants.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStages.RemoveRange(db.ProductStages.Where(w => w.ProductId.Equals(stage.ProductId)));
                    try
                    {
                        Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                    }
                    catch (Exception ex1)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex1.Message);
                    }
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
        }

        [Route("api/ProductStages/{productId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeProduct([FromUri]int productId, ProductStageRequest request)
        {
            try
            {
                var tmpStage = db.ProductStages.Where(w => w.ProductId == productId)
                    .Include(i => i.ProductStageVariants.Select(s => s.ProductStageVariantArrtibuteMaps))
                    .Include(i => i.ProductStageAttributes);
                if(this.User.ShopRequest() != null)
                {
                    int shopId = this.User.ShopRequest().ShopId.Value;
                    tmpStage = tmpStage.Where(w => w.ShopId == shopId);
                }
                var stage = tmpStage.SingleOrDefault();
                if(stage == null)
                {
                    throw new Exception("Product is invalid");
                }
                Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                #region Setup Master
                SetupProductStage(db, stage, request);
                stage.Status = request.Status;
                stage.UpdateBy = this.User.UserRequest().Email;
                stage.UpdateOn = DateTime.Now;
                #region Setup Attribute
                List<ProductStageAttribute> attrListEntity = stage.ProductStageAttributes.ToList();
                if (request.MasterAttribute != null)
                {
                    int index = 0;
                    foreach (AttributeRequest attr in request.MasterAttribute)
                    {
                        bool addNew = false;
                        if (attrListEntity == null || attrListEntity.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            ProductStageAttribute current = attrListEntity.Where(w => w.AttributeId == attr.AttributeId).SingleOrDefault();
                            if (current != null)
                            {
                                if (attr.AttributeValues != null && attr.AttributeValues.Count > 0)
                                {
                                    foreach (AttributeValueRequest val in attr.AttributeValues)
                                    {
                                        current.ValueEn = string.Concat("((", val.AttributeValueId, "))");
                                        current.IsAttributeValue = true;
                                        break;
                                    }
                                }
                                else 
                                {
                                    current.ValueEn = attr.ValueEn;
                                    current.IsAttributeValue = false;
                                }
                                current.UpdateBy = this.User.UserRequest().Email;
                                current.UpdateOn = DateTime.Now;
                                attrListEntity.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            ProductStageAttribute attriEntity = new ProductStageAttribute();
                            attriEntity.Position = index++;
                            attriEntity.ProductId = stage.ProductId;
                            attriEntity.AttributeId = attr.AttributeId.Value;
                            if (attr.AttributeValues != null && attr.AttributeValues.Count > 0)
                            {
                                foreach (AttributeValueRequest val in attr.AttributeValues)
                                {
                                    attriEntity.ValueEn = string.Concat("((", val.AttributeValueId, "))");
                                    attriEntity.IsAttributeValue = true;
                                    break;
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(attr.ValueEn))
                            {
                                if (rg.IsMatch(attriEntity.ValueEn))
                                {
                                    throw new Exception("Attribute value not allow");
                                }
                                attriEntity.ValueEn = attr.ValueEn;
                                attriEntity.IsAttributeValue = false;
                            }
                            else
                            {
                                throw new Exception("Invalid attribute value");
                            }
                            attriEntity.Status = Constant.STATUS_ACTIVE;
                            attriEntity.CreateBy = this.User.UserRequest().Email;
                            attriEntity.CreateOn = DateTime.Now;
                            attriEntity.UpdateBy = this.User.UserRequest().Email;
                            attriEntity.UpdateOn = DateTime.Now;
                            db.ProductStageAttributes.Add(attriEntity);
                        }

                    }
                }

                if (attrListEntity != null && attrListEntity.Count > 0)
                {
                    db.ProductStageAttributes.RemoveRange(attrListEntity);
                }
                #endregion
                SaveChangeInventory(db, stage.Pid, request.MasterVariant, this.User.UserRequest().Email);
                SaveChangeInventoryHistory(db, stage.Pid, request.MasterVariant, this.User.UserRequest().Email);
                stage.FeatureImgUrl = SaveChangeImg(db, stage.Pid, stage.ShopId, request.MasterVariant.Images, this.User.UserRequest().Email);
                stage.ImageFlag = stage.FeatureImgUrl == null ? false : true;
                SaveChangeImg360(db, stage.Pid, stage.ShopId, request.MasterVariant.Images360, this.User.UserRequest().Email);
                SaveChangeVideoLinks(db, stage.Pid, stage.ShopId, request.MasterVariant.VideoLinks, this.User.UserRequest().Email);
                SaveChangeRelatedProduct(db, stage.Pid, stage.ShopId, request.RelatedProducts, this.User.UserRequest().Email);
                SaveChangeGlobalCat(db, stage.ProductId, request.GlobalCategories, this.User.UserRequest().Email);
                SaveChangeLocalCat(db, stage.ProductId, request.LocalCategories, this.User.UserRequest().Email);
                #endregion
                #region Setup Variant
                List<ProductStageVariant> varListEntity = null;
                if (stage.ProductStageVariants != null && stage.ProductStageVariants.Count > 0)
                {
                    varListEntity = stage.ProductStageVariants.ToList();
                }

                foreach (VariantRequest varRq in request.Variants)
                {
                    bool addNew = false;
                    if (varListEntity == null || varListEntity.Count == 0)
                    {
                        addNew = true;
                    }
                    ProductStageVariant current = null;
                    if (!addNew)
                    {
                        current = varListEntity.Where(w => w.VariantId == varRq.VariantId).SingleOrDefault();
                        if (current != null)
                        {
                            varListEntity.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        current = new ProductStageVariant();
                        string variantPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                        current.Pid = variantPid;
                        current.ProductId = stage.ProductId;
                        current.ShopId = stage.ShopId;
                        current.CreateBy = this.User.UserRequest().Email;
                        current.CreateOn = DateTime.Now;
                    }
                    List<ProductStageVariantArrtibuteMap> valList = null;
                    if (current.ProductStageVariantArrtibuteMaps != null && current.ProductStageVariantArrtibuteMaps.Count > 0)
                    {
                        valList = current.ProductStageVariantArrtibuteMaps.ToList();
                    }
                    if (varRq.FirstAttribute != null && varRq.FirstAttribute.AttributeId != null)
                    {
                        if (varRq.FirstAttribute.AttributeValues != null && varRq.FirstAttribute.AttributeValues.Count > 0)
                        {
                            foreach (AttributeValueRequest val in varRq.FirstAttribute.AttributeValues)
                            {
                                bool isTmpNew = false;
                                if (valList == null || valList.Count == 0)
                                {
                                    isTmpNew = true;
                                }
                                if (!isTmpNew)
                                {
                                    var currentVal = valList.Where(w => w.AttributeId == varRq.FirstAttribute.AttributeId && w.Value.Equals(string.Concat("((", val.AttributeValueId, "))"))).SingleOrDefault();
                                    if (currentVal != null)
                                    {
                                        valList.Remove(currentVal);
                                    }
                                    else
                                    {
                                        isTmpNew = true;
                                    }
                                }
                                if (isTmpNew)
                                {
                                    current.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                    {
                                        VariantId = current.VariantId,
                                        AttributeId = varRq.FirstAttribute.AttributeId.Value,
                                        Value = string.Concat("((", val.AttributeValueId, "))"),
                                        IsAttributeValue = true,
                                        CreateBy = this.User.UserRequest().Email,
                                        CreateOn = DateTime.Now,
                                        UpdateBy = this.User.UserRequest().Email,
                                        UpdateOn = DateTime.Now
                                    });
                                }

                            }
                        }
                        else
                        {
                            bool isTmpNew = false;
                            if (valList == null || valList.Count == 0)
                            {
                                isTmpNew = true;
                            }
                            if (!isTmpNew)
                            {
                                var currentVal = valList.Where(w => w.AttributeId == varRq.FirstAttribute.AttributeId).SingleOrDefault();
                                if (currentVal != null)
                                {
                                    if (rg.IsMatch(varRq.FirstAttribute.ValueEn))
                                    {
                                        throw new Exception("Attribute value not allow");
                                    }
                                    currentVal.Value = varRq.FirstAttribute.ValueEn;
                                    currentVal.IsAttributeValue = false;
                                    currentVal.UpdateBy = this.User.UserRequest().Email;
                                    currentVal.UpdateOn = DateTime.Now;
                                    valList.Remove(currentVal);
                                }
                                else
                                {
                                    isTmpNew = true;
                                }
                            }
                            if(isTmpNew)
                            {
                                if (rg.IsMatch(varRq.FirstAttribute.ValueEn))
                                {
                                    throw new Exception("Attribute value not allow");
                                }
                                current.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                {
                                    VariantId = current.VariantId,
                                    AttributeId = varRq.FirstAttribute.AttributeId.Value,
                                    Value = varRq.FirstAttribute.ValueEn,
                                    IsAttributeValue = false,
                                    CreateBy = this.User.UserRequest().Email,
                                    CreateOn = DateTime.Now,
                                    UpdateBy = this.User.UserRequest().Email,
                                    UpdateOn = DateTime.Now
                                });
                            }
                        }
                    }
                    if (varRq.SecondAttribute != null && varRq.SecondAttribute.AttributeId != null)
                    {
                        if (varRq.SecondAttribute.AttributeValues != null && varRq.SecondAttribute.AttributeValues.Count > 0)
                        {
                            foreach (AttributeValueRequest val in varRq.SecondAttribute.AttributeValues)
                            {
                                bool isTmpNew = false;
                                if (valList == null || valList.Count == 0)
                                {
                                    isTmpNew = true;
                                }
                                if (!isTmpNew)
                                {
                                    var currentVal = valList.Where(w => w.AttributeId == varRq.SecondAttribute.AttributeId && w.Value.Equals(string.Concat("((", val.AttributeValueId, "))"))).SingleOrDefault();
                                    if (currentVal != null)
                                    {
                                        valList.Remove(currentVal);
                                    }
                                    else
                                    {
                                        isTmpNew = true;
                                    }
                                }
                                if (isTmpNew)
                                {
                                    current.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                    {
                                        VariantId = current.VariantId,
                                        AttributeId = varRq.SecondAttribute.AttributeId.Value,
                                        Value = string.Concat("((", val.AttributeValueId, "))"),
                                        IsAttributeValue = true,
                                        CreateBy = this.User.UserRequest().Email,
                                        CreateOn = DateTime.Now,
                                        UpdateBy = this.User.UserRequest().Email,
                                        UpdateOn = DateTime.Now
                                    });
                                }

                            }
                        }
                        else
                        {
                            bool isTmpNew = false;
                            if (valList == null || valList.Count == 0)
                            {
                                isTmpNew = true;
                            }
                            if (!isTmpNew)
                            {
                                var currentVal = valList.Where(w => w.AttributeId == varRq.SecondAttribute.AttributeId).SingleOrDefault();
                                if (currentVal != null)
                                {
                                    if (rg.IsMatch(varRq.SecondAttribute.ValueEn))
                                    {
                                        throw new Exception("Attribute value not allow");
                                    }
                                    currentVal.Value = varRq.SecondAttribute.ValueEn;
                                    currentVal.IsAttributeValue = false;
                                    currentVal.UpdateBy = this.User.UserRequest().Email;
                                    currentVal.UpdateOn = DateTime.Now;
                                    valList.Remove(currentVal);
                                }
                                else
                                {
                                    isTmpNew = true;
                                }
                            }
                            if (isTmpNew)
                            {
                                if (rg.IsMatch(varRq.SecondAttribute.ValueEn))
                                {
                                    throw new Exception("Attribute value not allow");
                                }
                                current.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                {
                                    VariantId = current.VariantId,
                                    AttributeId = varRq.SecondAttribute.AttributeId.Value,
                                    Value = varRq.SecondAttribute.ValueEn,
                                    IsAttributeValue = false,
                                    CreateBy = this.User.UserRequest().Email,
                                    CreateOn = DateTime.Now,
                                    UpdateBy = this.User.UserRequest().Email,
                                    UpdateOn = DateTime.Now
                                });
                            }
                        }
                    }
                    if (valList != null && valList.Count > 0)
                    {
                        db.ProductStageVariantArrtibuteMaps.RemoveRange(valList);
                    }
                    varRq.StockType = request.MasterVariant.StockType;
                    if (string.IsNullOrWhiteSpace(varRq.SEO.ProductUrlKeyEn))
                    {
                        current.UrlEn = current.Pid;
                    }
                    else
                    {
                        current.UrlEn = varRq.SEO.ProductUrlKeyEn;
                    }
                    SaveChangeInventory(db, current.Pid, varRq, this.User.UserRequest().Email);
                    SaveChangeInventoryHistory(db, current.Pid, varRq, this.User.UserRequest().Email);
                    SaveChangeImg(db, current.Pid, stage.ShopId, varRq.Images, this.User.UserRequest().Email);
                    SaveChangeVideoLinks(db, current.Pid, stage.ShopId, varRq.VideoLinks, this.User.UserRequest().Email);
                    current.Status = stage.Status;
                    SetupProductStageVariant(current, varRq);
                    current.UpdateBy = this.User.UserRequest().Email;
                    current.UpdateOn = DateTime.Now;
                    if (addNew)
                    {
                        db.ProductStageVariants.Add(current);
                    }
                }
                if (varListEntity != null && varListEntity.Count > 0)
                {
                    db.ProductStageVariants.RemoveRange(varListEntity);
                }
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return GetProductStage(stage.ProductId);
               
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Visibility")]
        [HttpPut]
        public HttpResponseMessage VisibilityProductStage(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                int shopId = this.User.ShopRequest().ShopId.Value;
                var productList = db.ProductStages.Where(w => w.ShopId == shopId).ToList();
                if(productList == null || productList.Count == 0)
                {
                    throw new Exception("No product found in this shop");
                }
                foreach (ProductStageRequest proRq in request)
                {

                    var current = productList.Where(w => w.ProductId.Equals(proRq.ProductId)).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find product " + proRq.ProductId + " in shop " + shopId);
                    }
                    current.Visibility = proRq.Visibility.Value;
                    current.UpdateBy = this.User.UserRequest().Email;
                    current.UpdateOn = DateTime.Now;
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        duplicate
        [Route("api/ProductStages/{productId}")]
        [HttpPost]
        public HttpResponseMessage DuplicateProductStage(int productId)
        {
            try
            {
                ProductStageRequest response = SetupProductStageRequestFromProductId(db, productId);
                if (response == null)
                {
                    throw new Exception("Cannot find product with id " + productId);
                }
                else
                {
                    response.ProductId = null;
                    if(response.MasterVariant != null)
                    {
                        response.MasterVariant.Pid = null;
                        response.SEO.ProductUrlKeyEn = null;
                    }
                    if(response.Variants != null)
                    {
                        response.Variants.Where(w=>w.SEO != null).ToList().ForEach(f => { f.ProductId = null; f.VariantId = null; f.SEO.ProductUrlKeyEn = null;f.Pid = null; });
                    }
                    
                    return AddProduct(response);
                }
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages")]
        [HttpDelete]
        public HttpResponseMessage DeleteProduct(List<ProductStageRequest> request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var productList = db.ProductStages.Where(w => w.ShopId == shopId);
                foreach (ProductStageRequest proRq in request)
                {
                    var pro = productList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
                    if (pro == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }
                    db.ProductStages.Remove(pro);
                    db.ProductStageImages.RemoveRange(db.ProductStageImages.Where(w => w.Pid.Equals(pro.Pid)));
                    db.ProductStageImage360.RemoveRange(db.ProductStageImage360.Where(w => w.Pid.Equals(pro.Pid)));
                    db.ProductStageVideos.RemoveRange(db.ProductStageVideos.Where(w => w.Pid.Equals(pro.Pid)));
                    db.Inventories.RemoveRange(db.Inventories.Where(w => w.Pid.Equals(pro.Pid)));
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Approve")]
        [HttpPut]
        public HttpResponseMessage ApproveProduct(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var productList = db.ProductStages.Where(w => true);
                foreach (ProductStageRequest proRq in request)
                {
                    var pro = productList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
                    if (pro == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }
                    pro.Status = Constant.PRODUCT_STATUS_APPROVE;
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Reject")]
        [HttpPut]
        public HttpResponseMessage RejectProduct(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                var productList = db.ProductStages.Where(w=>true);
                foreach (ProductStageRequest proRq in request)
                {
                    var pro = productList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
                    if (pro == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }
                    if (!Constant.PRODUCT_STATUS_DRAFT.Equals(pro.Status))
                    {
                        throw new Exception("Cannot delete product that is not draft");
                    }
                    pro.Status = Constant.PRODUCT_STATUS_NOT_APPROVE;
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Publish")]
        [HttpPost]
        public HttpResponseMessage PublishProduct(List<ProductStageRequest> request)
        {
            try
            {

                if (request == null || request.Count == 0)
                {
                    throw new Exception("Invalid request");
                }
                int shopId = this.User.ShopRequest().ShopId.Value;
                var productList = db.ProductStages.Where(w => w.ShopId == shopId).ToList();
                if (productList == null || productList.Count == 0)
                {
                    throw new Exception("No product found in this shop");
                }


                foreach (ProductStageRequest rq in request)
                {
                    var current = productList.Where(w => w.ProductId.Equals(rq.ProductId)).SingleOrDefault();
                    if (current == null)
                    {
                        throw new Exception("Cannot find product " + rq.ProductId + " in shop " + shopId);
                    }
                    if (!current.Status.Equals(Constant.PRODUCT_STATUS_DRAFT))
                    {
                        throw new Exception("ProudctId " + rq.ProductId.Value + " is not drafted");
                    }
                    current.Status = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                    current.UpdateBy = this.User.UserRequest().Email;
                    current.UpdateOn = DateTime.Now;
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK, "Published success");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Export")]
        [HttpPost]
        public HttpResponseMessage ExportProductProducts(ExportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                #region Setup Header
                int i = 0;
                Dictionary<string, Tuple<string,int>> headDicTmp = new Dictionary<string, Tuple<string, int>>();
                var guidance = db.ImportHeaders.OrderBy(o=>o.ImportHeaderId).ToList();

                foreach (var current in guidance)
                {
                    var op = request.Options.Where(w => w.Equals(current.MapName)).SingleOrDefault();
                    if(op == null)
                    {
                        continue;
                    }
                    if (!headDicTmp.ContainsKey(current.HeaderName))
                    {
                        headDicTmp.Add(current.MapName, new Tuple<string, int>(current.HeaderName, i++));
                        if (current.MapName.Equals("PRS")) { request.ProductStatus = true; }
                        if (current.MapName.Equals("GID")) { request.GroupID = true; }
                        if (current.MapName.Equals("DFV")) { request.DefaultVariant = true; }
                        if (current.MapName.Equals("PID")) { request.PID = true; }
                        if (current.MapName.Equals("PNE")) { request.ProductNameEn = true; }
                        if (current.MapName.Equals("PNT")) { request.ProductNameTh = true; }
                        if (current.MapName.Equals("SKU")) { request.SKU = true; }
                        if (current.MapName.Equals("UPC")) { request.UPC = true; }
                        if (current.MapName.Equals("BRN")) { request.BrandName = true; }
                        if (current.MapName.Equals("ORP")) { request.OriginalPrice = true; }
                        if (current.MapName.Equals("SAP")) { request.SalePrice = true; }
                        if (current.MapName.Equals("DCE")) { request.DescriptionEn = true; }
                        if (current.MapName.Equals("DCT")) { request.DescriptionTh = true; }
                        if (current.MapName.Equals("SDE")) { request.ShortDescriptionEn = true; }
                        if (current.MapName.Equals("SDT")) { request.ShortDescriptionTh = true; }
                        if (current.MapName.Equals("KEW")) { request.SearchTag = true; }
                        if (current.MapName.Equals("INA")) { request.InventoryAmount = true; }
                        if (current.MapName.Equals("SSA")) { request.SafetytockAmount = true; }
                        if (current.MapName.Equals("STT")) { request.StockType = true; }
                        if (current.MapName.Equals("SHM")) { request.ShippingMethod = true; }
                        if (current.MapName.Equals("PRT")) { request.PreparationTime = true; }
                        if (current.MapName.Equals("LEN")) { request.PackageLenght = true; }
                        if (current.MapName.Equals("HEI")) { request.PackageHeight = true; }
                        if (current.MapName.Equals("WID")) { request.PackageWidth = true; }
                        if (current.MapName.Equals("WEI")) { request.PackageWeight = true; }
                        if (current.MapName.Equals("GCI")) { request.GlobalCategory = true; }
                        if (current.MapName.Equals("AG1")) { request.GlobalCategory01 = true; }
                        if (current.MapName.Equals("AG2")) { request.GlobalCategory02 = true; }
                        if (current.MapName.Equals("LCI")) { request.LocalCategory = true; }
                        if (current.MapName.Equals("AL1")) { request.LocalCategory01 = true; }
                        if (current.MapName.Equals("AL2")) { request.LocalCategory02 = true; }
                        if (current.MapName.Equals("REP")) { request.RelatedProducts = true; }
                        if (current.MapName.Equals("MTE")) { request.MetaTitleEn = true; }
                        if (current.MapName.Equals("MTT")) { request.MetaTitleTh = true; }
                        if (current.MapName.Equals("MDE")) { request.MetaDescriptionEn = true; }
                        if (current.MapName.Equals("MDT")) { request.MetaDescriptionTh = true; }
                        if (current.MapName.Equals("MKE")) { request.MetaKeywordEn = true; }
                        if (current.MapName.Equals("MKT")) { request.MetaKeywordTh = true; }
                        if (current.MapName.Equals("PUK")) { request.ProductURLKeyEn = true; }
                        if (current.MapName.Equals("PBW")) { request.ProductBoostingWeight = true; }
                        if (current.MapName.Equals("EFD")) { request.EffectiveDate = true; }
                        if (current.MapName.Equals("EFT")) { request.EffectiveTime = true; }
                        if (current.MapName.Equals("EXD")) { request.ExpiryDate = true; }
                        if (current.MapName.Equals("EXT")) { request.ExpiryTime = true; }
                        if (current.MapName.Equals("FL1")) { request.FlagControl1 = true; }
                        if (current.MapName.Equals("FL2")) { request.FlagControl2 = true; }
                        if (current.MapName.Equals("FL3")) { request.FlagControl3 = true; }
                        if (current.MapName.Equals("REM")) { request.Remark = true; }
                    }
                }
                #endregion
                #region Query

                var query = (
                             from mast in db.ProductStages
                             join variant in db.ProductStageVariants on mast.ProductId equals variant.ProductId into varJoin
                             from vari in varJoin.DefaultIfEmpty()
                                 where productIds.Contains(mast.ProductId) && mast.ShopId == shopId
                             select new
                             {
                                 ShopId = vari != null ? vari.ShopId : mast.ShopId,
                                 Status = vari != null ? vari.Status : mast.Status,
                                 Sku = vari != null ? vari.Sku : mast.Sku,
                                 Pid = vari != null ? vari.Pid : mast.Pid,
                                 Upc = vari != null ? vari.Upc : mast.Upc,
                                 ProductId = vari != null ? vari.ProductId : mast.ProductId,
                                 GroupNameEn = mast.ProductNameEn,
                                 GroupNameTh = mast.ProductNameTh,
                                 ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
                                 ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
                                 DefaultVaraint = vari != null ? vari.DefaultVaraint == true ? "Yes" : "No" : "Yes",
                                 ControlFlag1 = mast.ControlFlag1 == true ? "Yes" : "No",
                                 ControlFlag2 = mast.ControlFlag2 == true ? "Yes" : "No",
                                 ControlFlag3 = mast.ControlFlag3 == true ? "Yes" : "No",
                                 mast.Brand.BrandNameEn,
                                 mast.GlobalCatId,
                                 RelatedGlobalCat = mast.ProductStageGlobalCatMaps.Select(s=>s.GlobalCategory.CategoryId),
                                 mast.LocalCatId,
                                 RelatedLocalCat = mast.ProductStageLocalCatMaps.Select(s => s.LocalCategory.CategoryId),
                                 OriginalPrice = vari != null ? vari.OriginalPrice : mast.OriginalPrice,
                                 SalePrice = vari != null ? vari.SalePrice : mast.SalePrice,
                                 DescriptionShortEn = vari != null ? vari.DescriptionShortEn : mast.DescriptionShortEn,
                                 DescriptionShortTh = vari != null ? vari.DescriptionShortTh : mast.DescriptionShortTh,
                                 DescriptionFullEn = vari != null ? vari.DescriptionFullEn : mast.DescriptionFullEn,
                                 DescriptionFullTh = vari != null ? vari.DescriptionFullTh : mast.DescriptionFullTh,
                                 AttributeSet = new { mast.AttributeSetId, mast.AttributeSet.AttributeSetNameEn, Attribute = mast.AttributeSet.AttributeSetMaps.Select(s => s.Attribute) },
                                 mast.PrepareDay,
                                 Length = vari != null ? vari.Length : mast.Length,
                                 Height = vari != null ? vari.Height : mast.Height,
                                 Width = vari != null ? vari.Width : mast.Width,
                                 Weight = vari != null ? vari.Weight : mast.Weight,
                                 mast.Tag,
                                 mast.MetaTitleEn,
                                 mast.MetaTitleTh,
                                 mast.MetaDescriptionEn,
                                 mast.MetaDescriptionTh,
                                 mast.MetaKeyEn,
                                 mast.MetaKeyTh,
                                 mast.UrlEn,
                                 mast.BoostWeight,
                                 mast.EffectiveDate,
                                 mast.EffectiveTime,
                                 mast.ExpiryDate,
                                 mast.ExpiryTime,
                                 mast.Remark,
                                 mast.Shipping.ShippingMethodEn,
                                 VariantAttribute = vari.ProductStageVariantArrtibuteMaps.Select(s => new
                                 {
                                     s.Attribute.AttributeNameEn,
                                     Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.Value) select tt.AttributeValueEn).FirstOrDefault()
                                         : s.Value,
                                 }),
                                 MasterAttribute = mast.ProductStageAttributes.Select(s => new
                                 {
                                     s.AttributeId,
                                     s.Attribute.AttributeNameEn,
                                     ValueEn = s.IsAttributeValue ?
                                                (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
                                                : s.ValueEn,
                                 }),
                                 RelatedProduct = (from rel in db.ProductStageRelateds where rel.Pid1.Equals(mast.Pid) select rel.Pid2).ToList(),
                                 Inventory = vari != null ? (from inv in db.Inventories where inv.Pid.Equals(vari.Pid) select inv).FirstOrDefault() :
                                              (from inv in db.Inventories where inv.Pid.Equals(mast.Pid) select inv).FirstOrDefault(),
                             });
                var productIds = request.ProductList.Where(w => w.ProductId != null).Select(s => s.ProductId.Value).ToList();

                if (productIds != null && productIds.Count > 0)
                {
                    if (productIds.Count > 2000)
                    {
                        throw new Exception("Too many product selected");
                    }
                    query = query.Where(w => productIds.Contains(w.ProductId));
                }
                if (this.User.ShopRequest() != null)
                {
                    var shopId = this.User.ShopRequest().ShopId.Value;
                    query = query.Where(w => w.ShopId == shopId);
                }
                var productList = query.ToList();

                #endregion
                List<List<string>> rs = new List<List<string>>();
                List<string> bodyList = null;
                if (request.AttributeSets != null && request.AttributeSets.Count > 0)
                {
                    headDicTmp.Add("ATS", new Tuple<string, int>("Attribute Set", i++));
                    headDicTmp.Add("VO1", new Tuple<string, int>("Variation Option 1", i++));
                    headDicTmp.Add("VO2", new Tuple<string, int>("Variation Option 2", i++));
                }
                foreach (var p in productList)
                {
                    bodyList = new List<string>(new string[headDicTmp.Count]);
                    #region Assign Value
                    if (request.ProductStatus)
                    {
                        if (Constant.PRODUCT_STATUS_DRAFT.Equals(p.Status))
                        {
                            bodyList[headDicTmp["PRS"].Item2] = "Draft";
                        }
                        else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(p.Status))
                        {
                            bodyList[headDicTmp["PRS"].Item2] = "Wait for Approval";
                        }
                        else if (Constant.PRODUCT_STATUS_APPROVE.Equals(p.Status))
                        {
                            bodyList[headDicTmp["PRS"].Item2] = "Approve";
                        }
                        else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(p.Status))
                        {
                            bodyList[headDicTmp["PRS"].Item2] = "Not Approve";
                        }
                    }
                    if (request.SKU)
                    {
                        bodyList[headDicTmp["SKU"].Item2] = p.Sku;
                    }
                    if (request.PID)
                    {
                        bodyList[headDicTmp["PID"].Item2] = p.Pid;
                    }
                    if (request.UPC)
                    {
                        bodyList[headDicTmp["UPC"].Item2] = p.Upc;
                    }
                    if (request.GroupID)
                    {
                        bodyList[headDicTmp["GID"].Item2] = string.Concat(p.ProductId);
                    }
                    if (request.DefaultVariant)
                    {
                        bodyList[headDicTmp["DFV"].Item2] = p.DefaultVaraint;
                    }
                    if (request.ProductNameEn)
                    {
                        bodyList[headDicTmp["PNE"].Item2] = p.ProductNameEn;
                    }
                    if (request.ProductNameTh)
                    {
                        bodyList[headDicTmp["PNT"].Item2] = p.ProductNameTh;
                    }
                    if (request.BrandName)
                    {
                        bodyList[headDicTmp["BRN"].Item2] = p.BrandNameEn;
                    }
                    if (request.GlobalCategory)
                    {
                        bodyList[headDicTmp["GCI"].Item2] = string.Concat(p.GlobalCatId);
                    }
                    if (request.GlobalCategory01)
                    {
                        if(p.RelatedGlobalCat != null && p.RelatedGlobalCat.ToList().Count > 0)
                        {
                            bodyList[headDicTmp["AG1"].Item2] = string.Concat(p.RelatedGlobalCat.ToList()[0]);
                        }
                    }
                    if (request.GlobalCategory02)
                    {
                        if (p.RelatedGlobalCat != null && p.RelatedGlobalCat.ToList().Count > 1)
                        {
                            bodyList[headDicTmp["AG2"].Item2] = string.Concat(p.RelatedGlobalCat.ToList()[1]);
                        }
                    }
                    if (request.LocalCategory)
                    {
                        bodyList[headDicTmp["LCI"].Item2] = string.Concat(p.LocalCatId);
                    }
                    if (request.LocalCategory01)
                    {
                        if (p.RelatedLocalCat != null && p.RelatedLocalCat.ToList().Count > 0)
                        {
                            bodyList[headDicTmp["AL1"].Item2] = string.Concat(p.RelatedLocalCat.ToList()[0]);
                        }
                    }
                    if (request.LocalCategory02)
                    {
                        if (p.RelatedLocalCat != null && p.RelatedLocalCat.ToList().Count > 1)
                        {
                            bodyList[headDicTmp["AL2"].Item2] = string.Concat(p.RelatedLocalCat.ToList()[1]);
                        }
                    }
                    if (request.OriginalPrice)
                    {
                        bodyList[headDicTmp["ORP"].Item2] = string.Concat(p.OriginalPrice);
                    }
                    if (request.SalePrice)
                    {
                        bodyList[headDicTmp["SAP"].Item2] = string.Concat(p.SalePrice);
                    }
                    if (request.DescriptionEn)
                    {
                        bodyList[headDicTmp["DCE"].Item2] = p.DescriptionFullEn;
                    }
                    if (request.DescriptionTh)
                    {
                        bodyList[headDicTmp["DCT"].Item2] = p.DescriptionFullTh;
                    }
                    if (request.ShortDescriptionEn)
                    {
                        bodyList[headDicTmp["SDE"].Item2] = p.DescriptionShortEn;
                    }
                    if (request.ShortDescriptionTh)
                    {
                        bodyList[headDicTmp["SDT"].Item2] = p.DescriptionShortTh;
                    }
                    if (request.PreparationTime)
                    {
                        bodyList[headDicTmp["PRT"].Item2] = string.Concat(p.PrepareDay);
                    }
                    if (request.PackageLenght)
                    {
                        bodyList[headDicTmp["LEN"].Item2] = string.Concat(p.Length);
                    }
                    if (request.PackageHeight)
                    {
                        bodyList[headDicTmp["HEI"].Item2] = string.Concat(p.Height);
                    }
                    if (request.PackageWidth)
                    {
                        bodyList[headDicTmp["WID"].Item2] = string.Concat(p.Width);
                    }
                    if (request.PackageWeight)
                    {
                        bodyList[headDicTmp["WEI"].Item2] = string.Concat(p.Weight);
                    }

                    if (request.InventoryAmount)
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["INA"].Item2] = string.Concat(p.Inventory.Quantity);
                        }
                        else
                        {
                            bodyList[headDicTmp["INA"].Item2] = string.Empty;
                        }
                    }
                    if (request.StockType)
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["STT"].Item2] = Constant.STOCK_TYPE.Where(w => w.Value.Equals(p.Inventory.StockAvailable)).SingleOrDefault().Key;
                        }
                        else
                        {
                            bodyList[headDicTmp["STT"].Item2] = string.Empty;
                        }
                    }
                    if (request.ShippingMethod)
                    {
                        bodyList[headDicTmp["SHM"].Item2] = p.ShippingMethodEn;
                    }
                    if (request.SafetytockAmount)
                    {
                        if (p.Inventory != null)
                        {
                            bodyList[headDicTmp["SSA"].Item2] = string.Concat(p.Inventory.SafetyStockSeller);
                        }
                        else
                        {
                            bodyList[headDicTmp["SSA"].Item2] = string.Empty;
                        }
                    }
                    if (request.SearchTag)
                    {
                        bodyList[headDicTmp["KEW"].Item2] = p.Tag;
                    }
                    if (request.RelatedProducts)
                    {
                        if (p.RelatedProduct != null && p.RelatedProduct.Count > 0)
                        {
                            bodyList[headDicTmp["REP"].Item2] = string.Join(",", p.RelatedProduct);
                        }
                        else
                        {
                            bodyList[headDicTmp["REP"].Item2] = string.Empty;
                        }
                    }
                    if (request.MetaTitleEn)
                    {
                        bodyList[headDicTmp["MTE"].Item2] = p.MetaTitleEn;
                    }
                    if (request.MetaTitleTh)
                    {
                        bodyList[headDicTmp["MTT"].Item2] = p.MetaTitleTh;
                    }
                    if (request.MetaDescriptionEn)
                    {
                        bodyList[headDicTmp["MDE"].Item2] = p.MetaDescriptionEn;
                    }
                    if (request.MetaDescriptionTh)
                    {
                        bodyList[headDicTmp["MDT"].Item2] = p.MetaDescriptionTh;
                    }
                    if (request.MetaKeywordEn)
                    {
                        bodyList[headDicTmp["MKE"].Item2] = p.MetaKeyEn;
                    }
                    if (request.MetaKeywordTh)
                    {
                        bodyList[headDicTmp["MKT"].Item2] = p.MetaKeyTh;
                    }
                    if (request.ProductURLKeyEn)
                    {
                        bodyList[headDicTmp["PUK"].Item2] = p.UrlEn;
                    }
                    if (request.ProductBoostingWeight)
                    {
                        bodyList[headDicTmp["PBW"].Item2] = string.Concat(p.BoostWeight);
                    }
                    if (request.EffectiveDate)
                    {
                        if(p.ExpiryDate != null)
                        {
                            bodyList[headDicTmp["EFD"].Item2] = p.ExpiryDate.ToString();
                        }
                    }
                    if (request.EffectiveTime)
                    {
                        if (p.EffectiveTime != null)
                        {
                            bodyList[headDicTmp["EFT"].Item2] = p.EffectiveTime.ToString();
                        }
                    }

                    if (request.ExpiryDate)
                    {
                        if (p.ExpiryDate != null)
                        {
                            bodyList[headDicTmp["EXD"].Item2] = p.ExpiryDate.ToString();
                        }
                    }
                    if (request.ExpiryTime)
                    {
                        if (p.ExpiryTime != null)
                        {
                            bodyList[headDicTmp["EXT"].Item2] = p.ExpiryTime.ToString();
                        }
                    }
                    if (request.Remark)
                    {
                        bodyList[headDicTmp["REM"].Item2] = p.Remark;
                    }
                    if (request.FlagControl1)
                    {
                        bodyList[headDicTmp["FL1"].Item2] = p.ControlFlag1;
                    }
                    if (request.FlagControl2)
                    {
                        bodyList[headDicTmp["FL2"].Item2] = p.ControlFlag2;
                    }
                    if (request.FlagControl3)
                    {
                        bodyList[headDicTmp["FL3"].Item2] = p.ControlFlag3;
                    }

                    #endregion

                    #region Attibute Section
                    if (request.AttributeSets != null && request.AttributeSets.Count > 0)
                    {
                        if (p.AttributeSet != null)
                        {
                            var set = request.AttributeSets.Where(w => w.AttributeSetId == p.AttributeSet.AttributeSetId).SingleOrDefault();
                            if (set != null)
                            {
                                make header for attribute
                                foreach (var attr in p.AttributeSet.Attribute)
                                {
                                    if (!headDicTmp.ContainsKey(attr.AttributeNameEn))
                                    {
                                        headDicTmp.Add(attr.AttributeNameEn, new Tuple<string, int>(attr.AttributeNameEn, i++));
                                        bodyList.Add(string.Empty);
                                    }
                                }

                                bodyList[headDicTmp["ATS"].Item2] = p.AttributeSet.AttributeSetNameEn;
                                make vaiant option 1 value
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
                                {
                                    bodyList[headDicTmp["VO1"].Item2] = p.VariantAttribute.ToList()[0].AttributeNameEn;
                                }
                                make vaiant option 2 value
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 1)
                                {
                                    bodyList[headDicTmp["VO2"].Item2] = p.VariantAttribute.ToList()[1].AttributeNameEn;
                                }
                                make master attribute value
                                if (p.MasterAttribute != null && p.MasterAttribute.ToList().Count > 0)
                                {
                                    foreach (var masterValue in p.MasterAttribute)
                                    {
                                        if (headDicTmp.ContainsKey(masterValue.AttributeNameEn))
                                        {
                                            int desColumn = headDicTmp[masterValue.AttributeNameEn].Item2;
                                            for (int j = bodyList.Count; j <= desColumn; j++)
                                            {
                                                bodyList.Add(string.Empty);
                                            }
                                            bodyList[desColumn] = masterValue.ValueEn;
                                        }
                                    }
                                }
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
                                {
                                    foreach (var variantValue in p.VariantAttribute)
                                    {
                                        if (headDicTmp.ContainsKey(variantValue.AttributeNameEn))
                                        {
                                            int desColumn = headDicTmp[variantValue.AttributeNameEn].Item2;
                                            for (int j = bodyList.Count; j <= desColumn; j++)
                                            {
                                                bodyList.Add(string.Empty);
                                            }
                                            bodyList[desColumn] = variantValue.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }


                    #endregion
                    rs.Add(bodyList);
                }

                #region Write header

                stream = new MemoryStream();
                writer = new StreamWriter(stream, Encoding.UTF8);
                var csv = new CsvWriter(writer);
                string headers = string.Empty;
                foreach (KeyValuePair<string, Tuple<string,int>> entry in headDicTmp)
                {
                    csv.WriteField(entry.Value.Item1);
                }
                csv.NextRecord();
                #endregion
                #region Write body
                foreach (List<string> r in rs)
                {
                    foreach (string field in r)
                    {
                        csv.WriteField(field);
                    }
                    csv.NextRecord();
                }
                #endregion
                #region Create Response
                writer.Flush();
                stream.Position = 0;

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "file.csv";
                #endregion
                return result;

            }
            catch (Exception e)
            {
                #region close writer
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Export")]
        [HttpPost]
        public HttpResponseMessage ExportProduct(ExportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                #region Query

                var query = (
                             from mast in db.ProductStages
                             join variant in db.ProductStageVariants on mast.ProductId equals variant.ProductId into varJoin
                             from vari in varJoin.DefaultIfEmpty()
                             //where productIds.Contains(mast.ProductId) && mast.ShopId == shopId
                             select new
                             {
                                 ShopId = vari != null ? vari.ShopId : mast.ShopId,
                                 Status = vari != null ? vari.Status : mast.Status,
                                 Sku = vari != null ? vari.Sku : mast.Sku,
                                 Pid = vari != null ? vari.Pid : mast.Pid,
                                 Upc = vari != null ? vari.Upc : mast.Upc,
                                 ProductId = vari != null ? vari.ProductId : mast.ProductId,
                                 //GroupNameEn = mast.ProductNameEn,
                                 //GroupNameTh = mast.ProductNameTh,
                                 ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
                                 ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
                                 DefaultVaraint = vari != null ? vari.DefaultVaraint == true ? "Yes" : "No" : "Yes",
                                 mast.Brand.BrandNameEn,
                                 mast.GlobalCatId,
                                 mast.LocalCatId,
                                 OriginalPrice = vari != null ? vari.OriginalPrice : mast.OriginalPrice,
                                 SalePrice = vari != null ? vari.SalePrice : mast.SalePrice,
                                 DescriptionShortEn = vari != null ? vari.DescriptionShortEn : mast.DescriptionShortEn,
                                 DescriptionShortTh = vari != null ? vari.DescriptionShortTh : mast.DescriptionShortTh,
                                 DescriptionFullEn = vari != null ? vari.DescriptionFullEn : mast.DescriptionFullEn,
                                 DescriptionFullTh = vari != null ? vari.DescriptionFullTh : mast.DescriptionFullTh,
                                 AttributeSet = new { mast.AttributeSetId, mast.AttributeSet.AttributeSetNameEn, Attribute = mast.AttributeSet.AttributeSetMaps.Select(s => s.Attribute) },
                                 mast.PrepareDay,
                                 Length = vari != null ? vari.Length : mast.Length,
                                 Height = vari != null ? vari.Height : mast.Height,
                                 Width = vari != null ? vari.Width : mast.Width,
                                 Weight = vari != null ? vari.Weight : mast.Weight,
                                 mast.Tag,
                                 mast.MetaTitleEn,
                                 mast.MetaTitleTh,
                                 mast.MetaDescriptionEn,
                                 mast.MetaDescriptionTh,
                                 mast.MetaKeyEn,
                                 mast.MetaKeyTh,
                                 mast.UrlEn,
                                 mast.BoostWeight,
                                 mast.EffectiveDate,
                                 mast.EffectiveTime,
                                 mast.ExpiryDate,
                                 mast.ExpiryTime,
                                 mast.Remark,
                                 VariantAttribute = vari.ProductStageVariantArrtibuteMaps.Select(s => new
                                 {
                                     s.Attribute.AttributeNameEn,
                                     Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.Value) select tt.AttributeValueEn).FirstOrDefault()
                                         : s.Value,
                                 }),
                                 MasterAttribute = mast.ProductStageAttributes.Select(s => new
                                 {
                                     s.AttributeId,
                                     s.Attribute.AttributeNameEn,
                                     ValueEn = s.IsAttributeValue ?
                                                (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
                                                : s.ValueEn,
                                 }),
                                 RelatedProduct = (from rel in db.ProductStageRelateds where rel.Pid1.Equals(mast.Pid) select rel.Pid2).ToList(),
                                 Inventory = vari != null ? (from inv in db.Inventories where inv.Pid.Equals(vari.Pid) select inv).FirstOrDefault() :
                                              (from inv in db.Inventories where inv.Pid.Equals(mast.Pid) select inv).FirstOrDefault(),
                             });
                var productIds = request.ProductList.Where(w=>w.ProductId != null).Select(s => s.ProductId.Value).ToList();
               
                if (productIds != null && productIds.Count > 0)
                {
                    if (productIds.Count > 2000)
                    {
                        throw new Exception("Too many product selected");
                    }
                    query = query.Where(w => productIds.Contains(w.ProductId));
                }
                if (this.User.ShopRequest() != null)
                {
                    var shopId = this.User.ShopRequest().ShopId.Value;
                    query = query.Where(w => w.ShopId==shopId);
                }
                var productList = query.ToList();

                #endregion
                #region Initiate Header
                int i = 0;
                Dictionary<string, int> headDic = new Dictionary<string, int>();
                if (request.ProductStatus)
                {
                    headDic.Add("Product Status",i++);
                }
                if (request.SKU)
                {
                    headDic.Add("SKU*", i++);
                }
                if (request.PID)
                {
                    headDic.Add("PID", i++);
                }
                if (request.UPC)
                {
                    headDic.Add("UPC", i++);
                }
                if (request.GroupID)
                {
                    headDic.Add("Group ID", i++);
                }
                //if (request.GroupNameEn)
                //{
                //    headDic.Add("Group Name (English)", i++);
                //}
                //if (request.GroupNameTh)
                //{
                //    headDic.Add("Group Name (Thai)", i++);
                //}
                if (request.DefaultVariant)
                {
                    headDic.Add("Default Variant", i++);
                }
                if (request.ProductNameEn)
                {
                    headDic.Add("Product Name (English)*", i++);
                }
                if (request.ProductNameTh)
                {
                    headDic.Add("Product Name (Thai)*", i++);
                }
                if (request.BrandName)
                {
                    headDic.Add("Brand Name*", i++);
                }
                if (request.GlobalCategory)
                {
                    headDic.Add("Global Category ID*", i++);
                }
                if (request.LocalCategory)
                {
                    headDic.Add("Local Category ID*", i++);
                }
                if (request.OriginalPrice)
                {
                    headDic.Add("Original Price*", i++);
                }
                if (request.SalePrice)
                {
                    headDic.Add("Sale Price", i++);
                }
                if (request.DescriptionEn)
                {
                    headDic.Add("Description (English)*", i++);
                }
                if (request.DescriptionTh)
                {
                    headDic.Add("Description (Thai)*", i++);
                }
                if (request.ShortDescriptionEn)
                {
                    headDic.Add("Short Description (English)", i++);
                }
                if (request.ShortDescriptionTh)
                {
                    headDic.Add("Short Description (Thai)", i++);
                }
                if (request.PreparationTime)
                {
                    headDic.Add("Preparation Time*", i++);
                }
                if (request.PackageLenght)
                {
                    headDic.Add("Package Dimension - Lenght (mm)*", i++);
                }
                if (request.PackageHeight)
                {
                    headDic.Add("Package Dimension - Height (mm)*", i++);
                }
                if (request.PackageWidth)
                {
                    headDic.Add("Package Dimension - Width (mm)*", i++);
                }
                if (request.PackageWeight)
                {
                    headDic.Add("Package -Weight (g)*", i++);
                }

                if (request.InventoryAmount)
                {
                    headDic.Add("Inventory Amount", i++);
                }
                if (request.SafetytockAmount)
                {
                    headDic.Add("Safety Stock Amount", i++);
                }
                if (request.SearchTag)
                {
                    headDic.Add("Search Tag*", i++);
                }
                if (request.RelatedProducts)
                {
                    headDic.Add("Related Products", i++);
                }
                if (request.MetaTitleEn)
                {
                    headDic.Add("Meta Title (English)", i++);
                }
                if (request.MetaTitleTh)
                {
                    headDic.Add("Meta Title (Thai)", i++);
                }
                if (request.MetaDescriptionEn)
                {
                    headDic.Add("Meta Description (English)", i++);
                }
                if (request.MetaDescriptionTh)
                {
                    headDic.Add("Meta Description (Thai)", i++);
                }
                if (request.MetaKeywordEn)
                {
                    headDic.Add("Meta Keywords (English)", i++);
                }
                if (request.MetaKeywordTh)
                {
                    headDic.Add("Meta Keywords (Thai)", i++);
                }
                if (request.ProductURLKeyEn)
                {
                    headDic.Add("Product URL Key(English)", i++);
                }
                if (request.ProductBoostingWeight)
                {
                    headDic.Add("Product Boosting Weight", i++);
                }
                if (request.EffectiveDate)
                {
                    headDic.Add("Effective Date", i++);
                }
                if (request.EffectiveTime)
                {
                    headDic.Add("Effective Time", i++);
                }

                if (request.ExpiryDate)
                {
                    headDic.Add("Expiry Date", i++);
                }
                if (request.ExpiryTime)
                {
                    headDic.Add("Expiry Time", i++);
                }
                if (request.Remark)
                {
                    headDic.Add("Remark", i++);
                }
                #endregion
                List<List<string>> rs = new List<List<string>>();
                foreach (var p in productList)
                {
                    List<string> bodyList = new List<string>();
                    #region Assign Value
                    if (request.ProductStatus)
                    {
                        if (Constant.PRODUCT_STATUS_DRAFT.Equals(p.Status))
                        {
                            bodyList.Add(Validation.ValidateCSVColumn("Draft"));
                        }
                        else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(p.Status))
                        {
                            bodyList.Add(Validation.ValidateCSVColumn("Wait for Approval"));
                        }
                        else if (Constant.PRODUCT_STATUS_APPROVE.Equals(p.Status))
                        {
                            bodyList.Add(Validation.ValidateCSVColumn("Approve"));
                        }
                        else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(p.Status))
                        {
                            bodyList.Add(Validation.ValidateCSVColumn("Not Approve"));
                        }
                    }
                    if (request.SKU)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Sku));
                    }
                    if (request.PID)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Pid));
                    }
                    if (request.UPC)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Upc));
                    }
                    if (request.GroupID)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.ProductId));
                    }
                    //if (request.GroupNameEn)
                    //{
                    //    bodyList.Add(Validation.ValidaetCSVColumn(p.GroupNameEn));
                    //}
                    //if (request.GroupNameTh)
                    //{
                    //    bodyList.Add(Validation.ValidaetCSVColumn(p.GroupNameTh));
                    //}
                    if (request.DefaultVariant)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.DefaultVaraint));
                    }
                    if (request.ProductNameEn)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.ProductNameEn));
                    }
                    if (request.ProductNameTh)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.ProductNameTh));
                    }
                    if (request.BrandName)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.BrandNameEn));
                    }
                    if (request.GlobalCategory)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.GlobalCatId));
                    }
                    if (request.LocalCategory)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.LocalCatId));
                    }
                    if (request.OriginalPrice)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.OriginalPrice));
                    }
                    if (request.SalePrice)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.SalePrice));
                    }
                    if (request.DescriptionEn)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionFullEn));
                    }
                    if (request.DescriptionTh)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionFullTh));
                    }
                    if (request.ShortDescriptionEn)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionShortEn));
                    }
                    if (request.ShortDescriptionTh)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionShortTh));
                    }
                    if (request.PreparationTime)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.PrepareDay));
                    }
                    if (request.PackageLenght)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Length));
                    }
                    if (request.PackageHeight)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Height));
                    }
                    if (request.PackageWidth)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Width));
                    }
                    if (request.PackageWeight)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Weight));
                    }

                    if (request.InventoryAmount)
                    {
                        if (p.Inventory != null)
                        {
                            bodyList.Add(Validation.ValidateCSVColumn(p.Inventory.Quantity));
                        }
                        else
                        {
                            bodyList.Add(string.Empty);
                        }
                    }
                    if (request.SafetytockAmount)
                    {
                        if (p.Inventory != null)
                        {
                            bodyList.Add(Validation.ValidateCSVColumn(p.Inventory.SaftyStockSeller));
                        }
                        else
                        {
                            bodyList.Add(string.Empty);
                        }
                    }
                    if (request.SearchTag)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Tag));
                    }
                    if (request.RelatedProducts)
                    {
                        if (p.RelatedProduct != null && p.RelatedProduct.Count > 0)
                        {
                            bodyList.Add(Validation.ValidateCSVColumn(string.Join(",", p.RelatedProduct)));
                        }
                        else
                        {
                            bodyList.Add(string.Empty);
                        }
                    }
                    if (request.MetaTitleEn)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.MetaTitleEn));
                    }
                    if (request.MetaTitleTh)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.MetaTitleTh));
                    }
                    if (request.MetaDescriptionEn)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.MetaDescriptionEn));
                    }
                    if (request.MetaDescriptionTh)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.MetaDescriptionTh));
                    }
                    if (request.MetaKeywordEn)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.MetaKeyEn));
                    }
                    if (request.MetaKeywordTh)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.MetaKeyTh));
                    }
                    if (request.ProductURLKeyEn)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.UrlEn));
                    }
                    if (request.ProductBoostingWeight)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.BoostWeight));
                    }
                    if (request.EffectiveDate)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.EffectiveDate));
                    }
                    if (request.EffectiveTime)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.EffectiveTime));
                    }

                    if (request.ExpiryDate)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.ExpiryDate));
                    }
                    if (request.ExpiryTime)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.ExpiryTime));
                    }
                    if (request.Remark)
                    {
                        bodyList.Add(Validation.ValidateCSVColumn(p.Remark));
                    }
                    #endregion
                    #region Attibute Section
                    if (request.AttributeSets != null && request.AttributeSets.Count > 0)
                    {
                        if (p.AttributeSet != null)
                        {
                            var set = request.AttributeSets.Where(w => w.AttributeSetId == p.AttributeSet.AttributeSetId).SingleOrDefault();
                            if (set != null)
                            {
                                if (!headDic.ContainsKey("Attribute Set"))
                                {
                                    headDic.Add("Attribute Set",i++);
                                    headDic.Add("Variation Option 1", i++);
                                    headDic.Add("Variation Option 2", i++);
                                }
                                bodyList.Add(Validation.ValidateCSVColumn(p.AttributeSet.AttributeSetNameEn));
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
                                {
                                    bodyList.Add(Validation.ValidateCSVColumn(p.VariantAttribute.ToList()[0].AttributeNameEn));
                                    if(p.VariantAttribute.ToList().Count > 1)
                                    {
                                        bodyList.Add(Validation.ValidateCSVColumn(p.VariantAttribute.ToList()[1].AttributeNameEn));
                                    }
                                    else
                                    {
                                        bodyList.Add(string.Empty);
                                    }
                                }
                                else
                                {
                                    bodyList.Add(string.Empty);
                                    bodyList.Add(string.Empty);
                                }
                                foreach (var attr in p.AttributeSet.Attribute)
                                {
                                    if (!headDic.ContainsKey(attr.AttributeNameEn))
                                    {
                                        headDic.Add(attr.AttributeNameEn, i++);
                                    }
                                    bodyList.Add(string.Empty);
                                }
                                if(p.MasterAttribute != null && p.MasterAttribute.ToList().Count > 0)
                                {
                                    foreach (var masterValue in p.MasterAttribute)
                                    {
                                        if (headDic.ContainsKey(masterValue.AttributeNameEn))
                                        {
                                            int desColumn = headDic[masterValue.AttributeNameEn];
                                            for(int j = bodyList.Count;j <= desColumn; j++)
                                            {
                                                bodyList.Add(string.Empty);
                                            }
                                            bodyList[desColumn] = masterValue.ValueEn;
                                        }
                                    }
                                }
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
                                {
                                    foreach (var variantValue in p.VariantAttribute)
                                    {
                                        if (headDic.ContainsKey(variantValue.AttributeNameEn))
                                        {
                                            int desColumn = headDic[variantValue.AttributeNameEn];
                                            for (int j = bodyList.Count; j <= desColumn; j++)
                                            {
                                                bodyList.Add(string.Empty);
                                            }
                                            bodyList[desColumn] = variantValue.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    rs.Add(bodyList);
                }
                #region Write header
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                var csv = new CsvWriter(writer);
                foreach (KeyValuePair<string, int> entry in headDic)
                {
                    csv.WriteField(entry.Key);
                }
                csv.NextRecord();
                #endregion
                #region Write body
                foreach (List<string> r in rs)
                {
                    foreach( string field in r)
                    {
                        csv.WriteField(field);
                    }
                    csv.NextRecord();
                }
                #endregion
                #region Create Response
                writer.Flush();
                stream.Position = 0;

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
                {
                    CharSet = Encoding.UTF8.WebName
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "file.csv";
                #endregion
                return result;
            }
            catch (Exception e)
            {
                #region close writer
                if (writer != null)
                {
                    writer.Close();
                    writer.Dispose();
                }
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        [Route("api/ProductStages/AttributeSet")]
        [HttpPost]
        public HttpResponseMessage GetAttributeSet(List<ProductStageRequest> request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var productIds = request.Where(w => w.ProductId != null).Select(s => s.ProductId.Value).ToList();
                var attrSet = db.AttributeSets.Where(w => w.ProductStages.Any(a => productIds.Contains(a.ProductId))).Select(s => new { s.AttributeSetId, s.AttributeSetNameEn, ProductCount = s.ProductStages.Where(w=> productIds.Contains(w.ProductId)) });

                return Request.CreateResponse(HttpStatusCode.OK, attrSet);

            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            
        }

        private ProductStageRequest SetupProductStageRequestFromProductId(ColspEntities db, int productId)
        {
            
            #region Query
            var stage = (from productStage in db.ProductStages
                       join brand in db.Brands on productStage.BrandId equals brand.BrandId
                       join productStageAttribute in db.ProductStageAttributes on productStage.ProductId equals productStageAttribute.ProductId
                       join productStageVariant in db.ProductStageVariants.Include(i=>i.ProductStageVariantArrtibuteMaps) on productStage.ProductId equals productStageVariant.ProductId into Variant
                       where productStage.ProductId == productId && productStage.ShopId == shopId
                       select new
                       {
                           productStage.ProductNameTh,
                           productStage.ProductNameEn,
                           productStage.Sku,
                           productStage.Upc,
                           Brand  = new { brand.BrandId, brand.BrandNameEn },
                           productStage.OriginalPrice,
                           productStage.SalePrice,
                           productStage.DescriptionFullTh,
                           productStage.DescriptionShortTh,
                           productStage.DescriptionFullEn,
                           productStage.DescriptionShortEn,
                           productStage.AttributeSetId,
                           productStage.Tag,
                           productStage.ShippingId,
                           productStage.PrepareDay,
                           productStage.Length,
                           productStage.Height,
                           productStage.Width,
                           productStage.Weight,
                           productStage.DimensionUnit,
                           productStage.WeightUnit,
                           productStage.GlobalCatId,
                           productStage.LocalCatId,
                           productStage.MetaTitleEn,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           productStage.Weight,
                           ProductStageVariants = Variant.ToList(),

                       }).SingleOrDefault();

            var tmpStage = db.ProductStages.Where(w => w.ProductId == productId)
                    .Include(i => i.ProductStageAttributes.Select(s => s.Attribute))
                    .Include(i => i.Brand)
                    .Include(i => i.ProductStageVariants.Select(s => s.ProductStageVariantArrtibuteMaps));

            if(this.User.ShopRequest() != null)
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                tmpStage = tmpStage.Where(w => w.ShopId == shopId);
            }
            var stage = tmpStage.SingleOrDefault();

            #endregion
            #region Validate
            if (stage == null)
            {
                throw new Exception("Product " + productId + " not found");
            }
            #endregion
            #region Initiate default value
            ProductStageRequest response = new ProductStageRequest();
            response.MasterVariant.ProductNameTh = stage.ProductNameTh;
            response.MasterVariant.ProductNameEn = stage.ProductNameEn;
            response.MasterVariant.Sku = stage.Sku;
            response.MasterVariant.Upc = stage.Upc;
            if (stage.Brand != null)
            {
                response.Brand.BrandId = stage.Brand.BrandId;
                response.Brand.BrandNameEn = stage.Brand.BrandNameEn;
            }
            response.MasterVariant.OriginalPrice = stage.OriginalPrice;
            response.MasterVariant.SalePrice = stage.SalePrice;
            response.MasterVariant.DescriptionFullTh = stage.DescriptionFullTh;
            response.MasterVariant.DescriptionShortTh = stage.DescriptionShortTh;
            response.MasterVariant.DescriptionFullEn = stage.DescriptionFullEn;
            response.MasterVariant.DescriptionShortEn = stage.DescriptionShortEn;
            response.AttributeSet.AttributeSetId = stage.AttributeSetId;
            response.Keywords = stage.Tag;
            response.ShippingMethod = stage.ShippingId;
            response.PrepareDay = stage.PrepareDay;
            response.MasterVariant.DimensionUnit = stage.DimensionUnit;
            response.MasterVariant.WeightUnit = stage.WeightUnit;
            if ("CM".Equals(response.MasterVariant.DimensionUnit))
            {
                response.MasterVariant.Length = decimal.Divide(stage.Length, 10);
                response.MasterVariant.Height = decimal.Divide(stage.Height, 10);
                response.MasterVariant.Width = decimal.Divide(stage.Width, 10);
            }
            else if ("M".Equals(response.MasterVariant.DimensionUnit))
            {
                response.MasterVariant.Length = decimal.Divide(stage.Length, 1000);
                response.MasterVariant.Height = decimal.Divide(stage.Height, 10);
                response.MasterVariant.Width = decimal.Divide(stage.Width, 10);
            }
            else
            {
                response.MasterVariant.Length = stage.Length;
                response.MasterVariant.Height = stage.Height;
                response.MasterVariant.Width = stage.Width;
            }
            if ("KG".Equals(response.MasterVariant.WeightUnit))
            {
                response.MasterVariant.Weight = decimal.Divide(stage.Weight, 1000);
            }
            else
            {
                response.MasterVariant.Weight = stage.Weight;
            }
           
            response.GlobalCategory = stage.GlobalCatId;
            response.LocalCategory = stage.LocalCatId;
            response.SEO.MetaTitleEn = stage.MetaTitleEn;
            response.SEO.MetaTitleTh = stage.MetaTitleTh;
            response.SEO.MetaDescriptionEn = stage.MetaDescriptionEn;
            response.SEO.MetaDescriptionTh = stage.MetaDescriptionTh;
            response.SEO.MetaKeywordEn = stage.MetaKeyEn;
            response.SEO.MetaKeywordTh = stage.MetaKeyTh;
            response.SEO.ProductUrlKeyEn = stage.UrlEn;
            response.SEO.ProductBoostingWeight = stage.BoostWeight;
            #region Setup Effective Date & Time 
            if (stage.EffectiveDate != null)
            {
                response.EffectiveDate = stage.EffectiveDate.Value.ToString("MMMM dd, yyyy");
            }
            if (stage.EffectiveTime != null)
            {
                response.EffectiveTime = stage.EffectiveTime.Value.ToString(@"hh\:mm");
            }
            #endregion
            #region Setup Expire Date & Time
            if (stage.ExpiryDate != null)
            {
                response.ExpireDate = stage.ExpiryDate.Value.ToString("MMMM dd, yyyy");
            }
            if (stage.ExpiryTime != null)
            {
                response.ExpireTime = stage.ExpiryTime.Value.ToString(@"hh\:mm");
            }
            #endregion
            response.ControlFlags.Flag1 = stage.ControlFlag1;
            response.ControlFlags.Flag2 = stage.ControlFlag2;
            response.ControlFlags.Flag3 = stage.ControlFlag3;
            response.Remark = stage.Remark;
            response.Status = stage.Status;
            response.ShopId = stage.ShopId;
            response.MasterVariant.Pid = stage.Pid;
            response.ProductId = stage.ProductId;
            response.InfoFlag = stage.InfoFlag;
            response.ImageFlag = stage.ImageFlag;
            response.OnlineFlag = stage.OnlineFlag;
            response.Visibility = stage.Visibility;
            response.VariantCount = stage.ProductStageVariants.Count;
            response.MasterAttribute = SetupAttributeResponse(stage.ProductStageAttributes);
            #region Setup Inventory
            var inventory = (from inv in db.Inventories
                             where inv.Pid.Equals(stage.Pid)
                             select inv).SingleOrDefault();
            if (inventory != null)
            {
                response.MasterVariant.SafetyStock = inventory.SafetyStockSeller;
                response.MasterVariant.Quantity = inventory.Quantity;
                response.MasterVariant.StockType = Constant.STOCK_TYPE.Where(w => w.Value.Equals(inventory.StockAvailable)).SingleOrDefault().Key;
            }
            #endregion
            response.MasterVariant.Images = SetupImgResponse(db, stage.Pid);
            response.MasterVariant.Images360 = SetupImg360Response(db, stage.Pid);
            response.MasterVariant.VideoLinks = SetupVdoResponse(db, stage.Pid);
            #region Setup Related GlobalCategories
            var globalCatList = (from map in db.ProductStageGlobalCatMaps
                                 join cat in db.GlobalCategories on map.CategoryId equals cat.CategoryId
                                 where map.ProductId.Equals(stage.ProductId)
                                 select map.GlobalCategory).ToList();

            if (globalCatList != null && globalCatList.Count > 0)
            {
                List<CategoryRequest> catList = new List<CategoryRequest>();
                foreach (GlobalCategory c in globalCatList)
                {
                    CategoryRequest cat = new CategoryRequest();
                    cat.CategoryId = c.CategoryId;
                    cat.NameEn = c.NameEn;
                    catList.Add(cat);
                }
                response.GlobalCategories = catList;
            }
            #endregion
            #region Setup Related LocalCategories
            var localCatList = (from map in db.ProductStageLocalCatMaps
                                join cat in db.LocalCategories on map.CategoryId equals cat.CategoryId
                                where map.ProductId.Equals(stage.ProductId)
                                select map.LocalCategory).ToList();
            if (localCatList != null && localCatList.Count > 0)
            {
                List<CategoryRequest> catList = new List<CategoryRequest>();
                foreach (LocalCategory c in localCatList)
                {
                    CategoryRequest cat = new CategoryRequest();
                    cat.CategoryId = c.CategoryId;
                    cat.NameEn = c.NameEn;
                    catList.Add(cat);
                }
                response.LocalCategories = catList;
            }
            #endregion
            #region Setup Related Product

            var tmpList = (from relateProduct in db.ProductStageRelateds
                           join productStage in db.ProductStages on relateProduct.Pid2 equals productStage.Pid
                           where relateProduct.Pid1.Equals(stage.Pid)
                           select new
                           {
                               ProductStage = productStage
                           });
            if(this.User.ShopRequest() != null)
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                tmpList.Where(w => w.ProductStage.ShopId == shopId);
            }

            var relatedList = tmpList.ToList();
            if (relatedList != null && relatedList.Count > 0)
            {
                List<VariantRequest> relate = new List<VariantRequest>();
                foreach (var r in relatedList)
                {
                    VariantRequest va = new VariantRequest();
                    va.ProductId = r.ProductStage.ProductId;
                    va.Pid = r.ProductStage.Pid;
                    va.ProductNameTh = r.ProductStage.ProductNameTh;
                    va.ProductNameEn = r.ProductStage.ProductNameEn;
                    va.Sku = r.ProductStage.Sku;
                    va.Upc = r.ProductStage.Upc;
                    va.OriginalPrice = r.ProductStage.OriginalPrice;
                    va.SalePrice = r.ProductStage.SalePrice;
                    va.DescriptionFullTh = r.ProductStage.DescriptionFullTh;
                    va.DescriptionShortTh = r.ProductStage.DescriptionShortTh;
                    va.DescriptionFullEn = r.ProductStage.DescriptionFullEn;
                    va.DescriptionShortEn = r.ProductStage.DescriptionShortEn;
                    va.Length = r.ProductStage.Length;
                    va.Height = r.ProductStage.Height;
                    va.Width = r.ProductStage.Width;
                    va.Weight = r.ProductStage.Weight;
                    va.DimensionUnit = r.ProductStage.DimensionUnit;
                    va.WeightUnit = r.ProductStage.WeightUnit;
                    relate.Add(va);
                }
                response.RelatedProducts = relate;
            }
            #endregion
            #endregion
            List<VariantRequest> varList = new List<VariantRequest>();
            foreach (ProductStageVariant variantEntity in stage.ProductStageVariants)
            {
                VariantRequest varient = new VariantRequest();
                varient.VariantId = variantEntity.VariantId;
                varient.Pid = variantEntity.Pid;

                if (variantEntity.ProductStageVariantArrtibuteMaps != null && variantEntity.ProductStageVariantArrtibuteMaps.Count > 0)
                {
                    var joinList = variantEntity.ProductStageVariantArrtibuteMaps
                        .GroupJoin(db.AttributeValues, p => p.Value, v => v.MapValue,
                            (varAttrMap, attrValue) => new { varAttrMap, attrValue }).ToList();
                    if (joinList != null && joinList.Count > 0)
                    {
                        varient.FirstAttribute.AttributeId = joinList[0].varAttrMap.AttributeId;
                        if (joinList[0].attrValue != null && joinList[0].attrValue.ToList().Count > 0)
                        {
                            foreach (var val in joinList[0].attrValue)
                            {
                                varient.FirstAttribute.AttributeValues.Add(new AttributeValueRequest()
                                {
                                    AttributeValueId = val.AttributeValueId,
                                    AttributeValueEn = val.AttributeValueEn
                                });
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(joinList[0].varAttrMap.Value))
                        {
                            varient.FirstAttribute.ValueEn = joinList[0].varAttrMap.Value;
                        }

                        if (joinList.Count > 1)
                        {
                            varient.SecondAttribute.AttributeId = joinList[1].varAttrMap.AttributeId;
                            if (joinList[1].attrValue != null && joinList[1].attrValue.ToList().Count > 0)
                            {
                                foreach (var val in joinList[1].attrValue)
                                {
                                    varient.SecondAttribute.AttributeValues.Add(new AttributeValueRequest()
                                    {
                                        AttributeValueId = val.AttributeValueId,
                                        AttributeValueEn = val.AttributeValueEn
                                    });
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(joinList[1].varAttrMap.Value))
                            {
                                varient.SecondAttribute.ValueEn = joinList[1].varAttrMap.Value;
                            }
                        }
                    }
                }
                varient.DefaultVariant = variantEntity.DefaultVaraint;
                varient.Display = variantEntity.Display;
                #region Setup Variant Inventory
                inventory = (from inv in db.Inventories
                             where inv.Pid.Equals(variantEntity.Pid)
                             select inv).SingleOrDefault();
                if (inventory != null)
                {
                    varient.SafetyStock = inventory.SafetyStockSeller;
                    varient.Quantity = inventory.Quantity;
                    varient.StockType = Constant.STOCK_TYPE.Where(w => w.Value.Equals(inventory.StockAvailable)).SingleOrDefault().Key;
                }
                #endregion

                varient.Images = SetupImgResponse(db, variantEntity.Pid);
                varient.VideoLinks = SetupVdoResponse(db, variantEntity.Pid);
                varient.SEO = new SEORequest();
                varient.SEO.MetaTitleEn = variantEntity.MetaTitleEn;
                varient.SEO.MetaTitleTh = variantEntity.MetaTitleTh;
                varient.SEO.MetaDescriptionEn = variantEntity.MetaDescriptionEn;
                varient.SEO.MetaDescriptionTh = variantEntity.MetaDescriptionTh;
                varient.SEO.MetaKeywordEn = variantEntity.MetaKeyEn;
                varient.SEO.MetaKeywordTh = variantEntity.MetaKeyTh;
                varient.SEO.ProductUrlKeyEn = variantEntity.UrlEn;
                varient.SEO.ProductBoostingWeight = variantEntity.BoostWeight;
                varient.ProductNameTh = variantEntity.ProductNameTh;
                varient.ProductNameEn = variantEntity.ProductNameEn;
                varient.Sku = variantEntity.Sku;
                varient.Upc = variantEntity.Upc;
                varient.OriginalPrice = variantEntity.OriginalPrice;
                varient.SalePrice = variantEntity.SalePrice;
                varient.DescriptionFullTh = variantEntity.DescriptionFullTh;
                varient.DescriptionShortTh = variantEntity.DescriptionShortTh;
                varient.DescriptionFullEn = variantEntity.DescriptionFullEn;
                varient.DescriptionShortEn = variantEntity.DescriptionShortEn;
                varient.PrepareDay = variantEntity.PrepareDay;
                varient.DimensionUnit = variantEntity.DimensionUnit;
                varient.WeightUnit = variantEntity.WeightUnit;
                if ("CM".Equals(varient.DimensionUnit))
                {
                    varient.Length = decimal.Divide(variantEntity.Length,10);
                    varient.Height = decimal.Divide(variantEntity.Height, 10);
                    varient.Width = decimal.Divide(variantEntity.Width, 10);
                }
                else if ("M".Equals(varient.DimensionUnit))
                {
                    varient.Length = decimal.Divide(variantEntity.Length, 1000);
                    varient.Height = decimal.Divide(variantEntity.Height, 10);
                    varient.Width = decimal.Divide(variantEntity.Width, 10);
                }
                else
                {
                    varient.Length = variantEntity.Length;
                    varient.Height = variantEntity.Height;
                    varient.Width = variantEntity.Width;
                }
                if ("KG".Equals(varient.WeightUnit))
                {
                    varient.Weight = decimal.Divide(variantEntity.Weight, 1000);
                }
                else
                {
                    varient.Weight = variantEntity.Weight;
                }
                varient.Visibility = variantEntity.Visibility;
                varList.Add(varient);
            }
            response.Variants = varList;
            return response;
        }

        private List<AttributeRequest> SetupAttributeResponse(ICollection<ProductStageAttribute> productStageAttributes)
        {
            List<AttributeRequest> newList = new List<AttributeRequest>();
            if (productStageAttributes != null)
            {
                var joinAttrVal =  productStageAttributes
                            .GroupJoin(db.AttributeValues, p => p.ValueEn, v => v.MapValue, (proAttrMap, attrValue) => new { proAttrMap, attrValue }).ToList();
                foreach (var attr in joinAttrVal)
                {
                    AttributeRequest attrRq = new AttributeRequest();
                    attrRq.AttributeId = attr.proAttrMap.AttributeId;
                    if(attr.attrValue.Count() > 0)
                    {
                        attrRq.AttributeValues.Add(new AttributeValueRequest()
                        {
                            AttributeValueId = attr.attrValue.ToList()[0].AttributeValueId,
                            AttributeValueEn = attr.attrValue.ToList()[0].AttributeValueEn
                        });
                    }
                    else if(!string.IsNullOrWhiteSpace(attr.proAttrMap.ValueEn))
                    {
                        attrRq.ValueEn = attr.proAttrMap.ValueEn;
                    }
                    else
                    {
                        throw new Exception("Invalid attribute value");
                    }
                    newList.Add(attrRq);
                }
            }
            return newList;
        }

        private void SetupProductStageVariant(ProductStageVariant variant, VariantRequest variantRq)
        {
            variant.ProductNameTh = Validation.ValidateString(variantRq.ProductNameTh, "Variation Product Name (Thai)", true, 300, true);
            variant.ProductNameEn = Validation.ValidateString(variantRq.ProductNameEn, "Variation Product Name (English)", true, 300, true);
            variant.Sku = Validation.ValidateString(variantRq.Sku, "Variation SKU", false, 300, true);
            variant.Upc = Validation.ValidateString(variantRq.Upc, "Variation UPC", false, 300, true);
            variant.OriginalPrice = Validation.ValidateDecimal(variantRq.OriginalPrice, "Variation Original Price", false, 20, 2, true).Value;
            variant.SalePrice = Validation.ValidateDecimal(variantRq.SalePrice, "Variation Sale Price", false, 20, 2, true, 0).Value;
           
            variant.DescriptionFullTh = Validation.ValidateString(variantRq.DescriptionFullTh, "Variation Description (Thai)", false, 2000, false);
            variant.DescriptionShortTh = Validation.ValidateString(variantRq.DescriptionShortTh, "Variation Short Description (Thai)", false, 500, true);
            variant.DescriptionFullEn = Validation.ValidateString(variantRq.DescriptionFullEn, "Variation Description (English)", false, 2000, false);
            variant.DescriptionShortEn = Validation.ValidateString(variantRq.DescriptionShortEn, "Variation Short Description (English)", false, 500, true);
            variant.DimensionUnit = variantRq.DimensionUnit;
            variant.WeightUnit = variantRq.WeightUnit;
            if (Constant.PRODUCT_STATUS_DRAFT.Equals(variant.Status))
            {

                var tmp = Validation.ValidateDecimal(variantRq.Length, "Length", false, 11, 2, true);
                variant.Length = tmp != null ? tmp.Value : 0;
                if ("CM".Equals(variant.DimensionUnit))
                {
                    variant.Length *= 10;
                }
                else if ("M".Equals(variant.DimensionUnit))
                {
                    variant.Length *= 1000;
                }
                tmp = Validation.ValidateDecimal(variantRq.Height, "Height", false, 11, 2, true);
                variant.Height = tmp != null ? tmp.Value : 0;
                if ("CM".Equals(variant.DimensionUnit))
                {
                    variant.Height *= 10;
                }
                else if ("M".Equals(variant.DimensionUnit))
                {
                    variant.Height *= 1000;
                }
                tmp = Validation.ValidateDecimal(variantRq.Width, "Width", false, 11, 2, true);
                variant.Width = tmp != null ? tmp.Value : 0;
                if ("CM".Equals(variant.DimensionUnit))
                {
                    variant.Width *= 10;
                }
                else if ("M".Equals(variant.DimensionUnit))
                {
                    variant.Width *= 1000;
                }
                tmp = Validation.ValidateDecimal(variantRq.Weight, "Weight", false, 11, 2, true);
                variant.Weight = tmp != null ? tmp.Value : 0;
                if ("KG".Equals(variant.DimensionUnit))
                {
                    variant.Weight *= 1000;
                }
                tmp = Validation.ValidateDecimal(variantRq.PrepareDay, "Preparation Time", false, 5, 2, true);
                variant.PrepareDay = tmp != null ? tmp.Value : 0;
                variant.MetaKeyEn = Validation.ValidateString(variantRq.SEO.MetaKeywordEn, "Meta Keywords (English)", false, 630, false);
                variant.MetaKeyTh = Validation.ValidateString(variantRq.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, 630, false);

            }
            else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(variant.Status))
            {
                variant.Length = Validation.ValidateDecimal(variantRq.Length, "Length", true, 11, 2, true).Value;
                if ("CM".Equals(variant.DimensionUnit))
                {
                    variant.Length *= 10;
                }
                else if ("M".Equals(variant.DimensionUnit))
                {
                    variant.Length *= 1000;
                }
                variant.Height = Validation.ValidateDecimal(variantRq.Height, "Height", true,11, 2, true).Value;
                if ("CM".Equals(variant.DimensionUnit))
                {
                    variant.Height *= 10;
                }
                else if ("M".Equals(variant.DimensionUnit))
                {
                    variant.Height *= 1000;
                }
                variant.Width = Validation.ValidateDecimal(variantRq.Width, "Width", true, 5, 11, true).Value;
                if ("CM".Equals(variant.DimensionUnit))
                {
                    variant.Width *= 10;
                }
                else if ("M".Equals(variant.DimensionUnit))
                {
                    variant.Width *= 1000;
                }
                variant.Weight = Validation.ValidateDecimal(variantRq.Weight, "Weight", true, 11, 2, true).Value;
                if ("KG".Equals(variant.DimensionUnit))
                {
                    variant.Weight *= 1000;
                }
                variant.MetaKeyEn = Validation.ValidateTaging(variantRq.SEO.MetaKeywordEn, "Meta Keywords (English)", false, false, 20, 30);
                variant.MetaKeyTh = Validation.ValidateTaging(variantRq.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, false, 20, 30);
                variant.PrepareDay = Validation.ValidateDecimal(variantRq.PrepareDay, "Preparation Time", true, 5, 2, true).Value;
            }
            else
            {
                throw new Exception("Invalid status");
            }
            variant.Display = Validation.ValidateString(variantRq.Display, "Display", false, 20, true);
            
            variant.DefaultVaraint = variantRq.DefaultVariant;
            variant.Visibility = variantRq.Visibility.Value;


            variant.MetaTitleEn = Validation.ValidateString(variantRq.SEO.MetaTitleEn, "Meta Title (English)", false, 60, false);
            variant.MetaTitleTh = Validation.ValidateString(variantRq.SEO.MetaTitleTh, "Meta Title (Thai)", false, 60, false);
            variant.MetaDescriptionEn = Validation.ValidateString(variantRq.SEO.MetaDescriptionEn, "Meta Description (English)", false, 150, false);
            variant.MetaDescriptionTh = Validation.ValidateString(variantRq.SEO.MetaDescriptionTh, "Meta Description (Thai)", false, 150, false);
            variant.BoostWeight = variantRq.SEO.ProductBoostingWeight;
        }

        private void SetupProductStage(ColspEntities db, ProductStage stage, ProductStageRequest request)
        {
            stage.ProductNameTh = Validation.ValidateString(request.MasterVariant.ProductNameTh, "Product Name (Thai)", true,300,true);
            stage.ProductNameEn = Validation.ValidateString(request.MasterVariant.ProductNameEn, "Product Name (English)", true, 300, true);
            stage.Sku = Validation.ValidateString(request.MasterVariant.Sku, "SKU", false, 300, true); 
            stage.Upc = Validation.ValidateString(request.MasterVariant.Upc, "UPC", false, 300, true);
            if(request.Brand != null && request.Brand.BrandId != null && request.Brand.BrandId != 0)
            {
                var brand = db.Brands.Find(request.Brand.BrandId);
                if(brand == null)
                {
                    throw new Exception("Cannot find specific brand");
                }
                stage.BrandId = brand.BrandId;
            }
            decimal? op = Validation.ValidateDecimal(request.MasterVariant.OriginalPrice, "Original Price", false, 20, 2, true);
            if(op != null)
            {
                stage.OriginalPrice = op.Value;
            }
            else
            {
                stage.OriginalPrice = 0;
            }
            stage.SalePrice = Validation.ValidateDecimal(request.MasterVariant.SalePrice, "Sale Price", false, 20, 2, true,0).Value;
            
            stage.DescriptionFullTh = Validation.ValidateString(request.MasterVariant.DescriptionFullTh, "Description (Thai)", false, 2000, false);
            stage.DescriptionShortTh = Validation.ValidateString(request.MasterVariant.DescriptionShortTh, "Short Description (Thai)", false, 500, true);
            stage.DescriptionFullEn = Validation.ValidateString(request.MasterVariant.DescriptionFullEn, "Description (English)", false, 2000, false);
            stage.DescriptionShortEn = Validation.ValidateString(request.MasterVariant.DescriptionShortEn, "Short Description (English)", false, 500, true);
            stage.DimensionUnit = request.MasterVariant.DimensionUnit;
            stage.WeightUnit = request.MasterVariant.WeightUnit;
            if (request.AttributeSet != null && request.AttributeSet.AttributeSetId != null && request.AttributeSet.AttributeSetId != 0)
            {
                var attributeSet = db.AttributeSets.Find(request.AttributeSet.AttributeSetId);
                if (attributeSet == null)
                {
                    throw new Exception("Cannot find specific attribute set");
                }
                stage.AttributeSetId = attributeSet.AttributeSetId;
            }
            
            if (request.ShippingMethod != null && request.ShippingMethod != 0)
            {
                var shipping = db.Shippings.Find(request.ShippingMethod);
                if (shipping == null)
                {
                    throw new Exception("Cannot find specific shipping");
                }
                stage.ShippingId = shipping.ShippingId;
            }
            if (Constant.PRODUCT_STATUS_DRAFT.Equals(request.Status))
            {
                var tmp = Validation.ValidateDecimal(request.PrepareDay, "Preparation Time", false, 5, 2, true);
                stage.PrepareDay = tmp != null ? tmp.Value : 0;
                tmp = Validation.ValidateDecimal(request.MasterVariant.Length, "Length", false, 11, 2, true);
                stage.Length = tmp != null ? tmp.Value : 0;
                tmp = Validation.ValidateDecimal(request.MasterVariant.Height, "Height", false, 11, 2, true);
                stage.Height = tmp != null ? tmp.Value : 0;
                tmp = Validation.ValidateDecimal(request.MasterVariant.Width, "Width", false, 11, 2, true);
                stage.Width = tmp != null ? tmp.Value : 0;
                tmp = Validation.ValidateDecimal(request.MasterVariant.Weight, "Weight", false, 11, 2, true);
                stage.Weight = tmp != null ? tmp.Value : 0;
                if ("CM".Equals(stage.DimensionUnit))
                {
                    stage.Length *= 10;
                    stage.Height *= 10;
                    stage.Width *= 10;
                }
                else if ("M".Equals(stage.DimensionUnit))
                {
                    stage.Length *= 1000;
                    stage.Height *= 1000;
                    stage.Width *= 1000;
                }
                if ("KG".Equals(stage.DimensionUnit))
                {
                    stage.Weight *= 1000;
                }
                stage.Tag = Validation.ValidateString(request.Keywords, "Search Tag", false, 630,false);
                stage.MetaKeyEn = Validation.ValidateString(request.SEO.MetaKeywordEn, "Meta Keywords (English)", false, 630, false);
                stage.MetaKeyTh = Validation.ValidateString(request.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, 630, false);
            }
            else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(request.Status))
            {
                stage.PrepareDay = Validation.ValidateDecimal(request.PrepareDay, "Preparation Time", true, 5, 2, true).Value;
                stage.Length = Validation.ValidateDecimal(request.MasterVariant.Length, "Length", true, 11, 2, true).Value;
                stage.Height = Validation.ValidateDecimal(request.MasterVariant.Height, "Height", true, 11, 2, true).Value;
                stage.Width = Validation.ValidateDecimal(request.MasterVariant.Width, "Width", true, 11, 2, true).Value;
                stage.Weight = Validation.ValidateDecimal(request.MasterVariant.Weight, "Weight", true, 11, 2, true).Value;
                if ("CM".Equals(stage.DimensionUnit))
                {
                    stage.Length *= 10;
                    stage.Height *= 10;
                    stage.Width *= 10;
                }
                else if ("M".Equals(stage.DimensionUnit))
                {
                    stage.Length *= 1000;
                    stage.Height *= 1000;
                    stage.Width *= 1000;
                }
                if ("KG".Equals(stage.DimensionUnit))
                {
                    stage.Weight *= 1000;
                }
                stage.Tag = Validation.ValidateTaging(request.Keywords, "Search Tag", false, false, 20, 30);
                stage.MetaKeyEn = Validation.ValidateTaging(request.SEO.MetaKeywordEn, "Meta Keywords (English)", false, false, 20, 30);
                stage.MetaKeyTh = Validation.ValidateTaging(request.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, false, 20, 30);
            }
            else
            {
                throw new Exception("Invalid status");
            }
            
            if(request.GlobalCategory != null && request.GlobalCategory != 0)
            {
                var globalCat = db.GlobalCategories.Find(request.GlobalCategory);
                if (globalCat == null)
                {
                    throw new Exception("Cannot find specific global category");
                }
                stage.GlobalCatId = globalCat.CategoryId;
            }
            else
            {
                throw new Exception("Global category is required");
            }
            if (request.LocalCategory != null && request.LocalCategory != 0)
            {
                var localCat = db.LocalCategories.Find(request.LocalCategory);
                if (localCat == null)
                {
                    throw new Exception("Cannot find specific local category");
                }
                stage.LocalCatId = localCat.CategoryId;
            }
            else
            {
                stage.LocalCatId = null;
            }

            stage.MetaTitleEn = Validation.ValidateString(request.SEO.MetaTitleEn, "Meta Title (English)", false, 60, false);
            stage.MetaTitleTh = Validation.ValidateString(request.SEO.MetaTitleTh, "Meta Title (Thai)", false, 60, false);
            stage.MetaDescriptionEn = Validation.ValidateString(request.SEO.MetaDescriptionEn, "Meta Description (English)", false, 150, false);
            stage.MetaDescriptionTh = Validation.ValidateString(request.SEO.MetaDescriptionTh, "Meta Description (Thai)", false, 150, false);
            stage.BoostWeight = request.SEO.ProductBoostingWeight;
            if (stage.BoostWeight != null 
                && stage.BoostWeight < 1 
                && stage.BoostWeight > 10000)
            {
                throw new Exception("Boost numbers from 1 to 10000 is allowed");
            }
            stage.ControlFlag1 = request.ControlFlags.Flag1;
            stage.ControlFlag2 = request.ControlFlags.Flag2;
            stage.ControlFlag3 = request.ControlFlags.Flag3;
            #region Setup Effective Date & Time 
            if (!string.IsNullOrEmpty(request.EffectiveDate))
            {
                try
                {
                    stage.EffectiveDate = Convert.ToDateTime(request.EffectiveDate);
                }
                catch
                {
                    throw new Exception("Invalid effective date format");
                }
            }
            else
            {
                stage.EffectiveDate = null;
            }
            if (!string.IsNullOrEmpty(request.EffectiveTime))
            {
                try
                {
                    stage.EffectiveTime = TimeSpan.Parse(request.EffectiveTime);
                }
                catch
                {
                    throw new Exception("Invalid effective time format");
                }
            }
            else
            {
                stage.EffectiveTime = null;
            }
            #endregion
            #region Setup Expire Date & Time
            if (!string.IsNullOrEmpty(request.ExpireDate))
            {
                try
                {
                    stage.ExpiryDate = Convert.ToDateTime(request.ExpireDate);
                }
                catch
                {
                    throw new Exception("Invalid expiry date format");
                }

            }
            else
            {
                stage.ExpiryDate = null;
            }
            if (!string.IsNullOrEmpty(request.ExpireTime))
            {
                try
                {
                    stage.ExpiryTime = TimeSpan.Parse(request.ExpireTime);
                }
                catch
                {
                    throw new Exception("Invalid expire time format");
                }
            }
            else
            {
                stage.ExpiryTime = null;
            }
            #endregion
            stage.Remark = Validation.ValidateString(request.Remark, "Remark", false, 2000, false);

            if(!string.IsNullOrEmpty(stage.ProductNameEn)
                && !string.IsNullOrEmpty(stage.ProductNameTh)
                && !string.IsNullOrEmpty(stage.ProductNameTh)
                )
            {
                stage.InfoFlag = true;
            }
            else
            {
                stage.InfoFlag = false;
            }
        }

        private void SaveChangeInventoryHistory(ColspEntities db, string pid, VariantRequest variant, string email)
        {
            InventoryHistory masterInventoryHist = new InventoryHistory();
            masterInventoryHist.StockAvailable = variant.Quantity;
            masterInventoryHist.SafetyStockSeller = variant.SafetyStock;
            if (variant.StockType != null)
            {
                if (Constant.STOCK_TYPE.ContainsKey(variant.StockType))
                {
                    masterInventoryHist.StockAvailable = Constant.STOCK_TYPE[variant.StockType];
                }
            }
            masterInventoryHist.Pid = pid;
            masterInventoryHist.Description = "Edit product";
            masterInventoryHist.CreateBy = email;
            masterInventoryHist.CreateOn = DateTime.Now;
            masterInventoryHist.UpdateBy = email;
            masterInventoryHist.UpdateOn = DateTime.Now;
            db.InventoryHistories.Add(masterInventoryHist);
        }

        private void SaveChangeInventory(ColspEntities db, string pid, VariantRequest variant, string email)
        {
            Inventory masterInventory = db.Inventories.Find(pid);
            bool isNew = false;
            if (masterInventory == null)
            {
                masterInventory = new Inventory();
                masterInventory.Pid = pid;
                masterInventory.CreateBy = email;
                masterInventory.CreateOn = DateTime.Now;
                isNew = true;
            }
            masterInventory.UpdateBy = email;
            masterInventory.UpdateOn = DateTime.Now;
            masterInventory.Quantity = Validation.ValidationInteger(variant.Quantity, "Quantity", true, Int32.MaxValue, 0).Value;
            masterInventory.SafetyStockSeller = variant.SafetyStock;
            if (variant.StockType != null)
            {
                if (Constant.STOCK_TYPE.ContainsKey(variant.StockType))
                {
                    masterInventory.StockAvailable = Constant.STOCK_TYPE[variant.StockType];
                }
            }
            if (isNew)
            {
                db.Inventories.Add(masterInventory);
            }
        }

        private void SaveChangeLocalCat(ColspEntities db, int ProductId, List<CategoryRequest> localCategories, string email)
        {
            var catList = db.ProductStageLocalCatMaps.Where(w => w.ProductId == ProductId).ToList();
            if (localCategories != null)
            {
                foreach (var cat in localCategories)
                {
                    if (cat == null) { continue; }
                    bool addNew = false;
                    if (catList == null || catList.Count == 0)
                    {
                        addNew = true;
                    }
                    if (!addNew)
                    {
                        ProductStageLocalCatMap current = catList.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
                        if (current != null)
                        {
                            current.UpdateBy = email;
                            current.UpdateOn = DateTime.Now;
                            catList.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        if (cat == null) { continue; }
                        ProductStageLocalCatMap catEntity = new ProductStageLocalCatMap();
                        catEntity.CategoryId = cat.CategoryId.Value;
                        catEntity.ProductId = ProductId;
                        catEntity.Status = Constant.STATUS_ACTIVE;
                        catEntity.CreateBy = email;
                        catEntity.CreateOn = DateTime.Now;
                        catEntity.UpdateBy = email;
                        catEntity.UpdateOn = DateTime.Now;
                        db.ProductStageLocalCatMaps.Add(catEntity);
                    }
                }
            }
            if (catList != null && catList.Count > 0)
            {
                db.ProductStageLocalCatMaps.RemoveRange(catList);
            }
        }

        private void SaveChangeGlobalCat(ColspEntities db, int ProductId, List<CategoryRequest> globalCategories, string email)
        {

            var catList = db.ProductStageGlobalCatMaps.Where(w => w.ProductId.Equals(ProductId)).ToList();
            if (globalCategories != null)
            {
                foreach (var cat in globalCategories)
                {
                    if (cat == null) { continue; }
                    bool addNew = false;
                    if (catList == null || catList.Count == 0)
                    {
                        addNew = true;
                    }
                    if (!addNew)
                    {
                        ProductStageGlobalCatMap current = catList.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
                        if (current != null)
                        {
                            current.UpdateBy = email;
                            current.UpdateOn = DateTime.Now;
                            catList.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        if (cat == null || cat.CategoryId == null) { continue; }
                        ProductStageGlobalCatMap catEntity = new ProductStageGlobalCatMap();
                        catEntity.CategoryId = cat.CategoryId.Value;
                        catEntity.ProductId = ProductId;
                        catEntity.Status = Constant.STATUS_ACTIVE;
                        catEntity.CreateBy = email;
                        catEntity.CreateOn = DateTime.Now;
                        catEntity.UpdateBy = email;
                        catEntity.UpdateOn = DateTime.Now;
                        db.ProductStageGlobalCatMaps.Add(catEntity);
                    }
                }
            }
            if (catList != null && catList.Count > 0)
            {
                db.ProductStageGlobalCatMaps.RemoveRange(catList);
            }
        }

        private void SaveChangeRelatedProduct(ColspEntities db, string pid, int shopId, List<VariantRequest> pidList, string email)
        {
            var relateList = db.ProductStageRelateds.Where(w => w.Pid1 == pid && w.ShopId== shopId).ToList();
            if (relateList != null)
            {
                foreach (var pro in pidList)
                {
                    bool addNew = false;
                    if (relateList == null || relateList.Count == 0)
                    {
                        addNew = true;
                    }
                    if (!addNew)
                    {
                        ProductStageRelated current = relateList.Where(w => w.Pid2 == pro.Pid).SingleOrDefault();
                        if (current != null)
                        {
                            current.UpdateBy = email;
                            current.UpdateOn = DateTime.Now;
                            relateList.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        ProductStageRelated proEntity = new ProductStageRelated();
                        proEntity.Pid1 = pid;
                        proEntity.Pid2 = pro.Pid;
                        proEntity.ShopId = shopId;
                        proEntity.Status = Constant.STATUS_ACTIVE;
                        proEntity.CreateBy = email;
                        proEntity.CreateOn = DateTime.Now;
                        proEntity.UpdateBy = email;
                        proEntity.UpdateOn = DateTime.Now;
                        db.ProductStageRelateds.Add(proEntity);
                    }
                }
            }
            if (relateList != null && relateList.Count > 0)
            {
                db.ProductStageRelateds.RemoveRange(relateList);
            }
        }

        private void SaveChangeVideoLinks(ColspEntities db, string pid, int shopId, List<VideoLinkRequest> videoRequest, string email)
        {
            var vdoList = db.ProductStageVideos.Where(w => w.Pid.Equals(pid) && w.ShopId == shopId).ToList();
            if (videoRequest != null)
            {
                int index = 0;
               
                foreach (VideoLinkRequest vdo in videoRequest)
                {
                    bool addNew = false;
                    if (vdoList == null || vdoList.Count == 0)
                    {
                        addNew = true;
                    }
                    if (!addNew)
                    {
                        ProductStageVideo current = vdoList.Where(w => w.VideoId == vdo.VideoId).SingleOrDefault();
                        if (current != null)
                        {
                            current.Position = index++;
                            current.VideoUrlEn = vdo.Url;
                            current.UpdateBy = email;
                            current.UpdateOn = DateTime.Now;
                            vdoList.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        ProductStageVideo vdoEntity = new ProductStageVideo();
                        vdoEntity.Pid = pid;
                        vdoEntity.ShopId = shopId;
                        vdoEntity.Position = index++;
                        vdoEntity.VideoUrlEn = vdo.Url;
                        vdoEntity.Status = Constant.STATUS_ACTIVE;
                        vdoEntity.CreateBy = email;
                        vdoEntity.CreateOn = DateTime.Now;
                        vdoEntity.UpdateBy = email;
                        vdoEntity.UpdateOn = DateTime.Now;
                        db.ProductStageVideos.Add(vdoEntity);
                    }
                }
            }
            if (vdoList != null && vdoList.Count > 0)
            {
                db.ProductStageVideos.RemoveRange(vdoList);
            }
        }

        private void SaveChangeImg360(ColspEntities db, string pid, int shopId, List<ImageRequest> img360Request, string email)
        {
            var img360List = db.ProductStageImage360.Where(w => w.Pid.Equals(pid) && w.ShopId == shopId).ToList();
            if (img360Request != null)
            {
                int index = 0;
                
                foreach (ImageRequest img in img360Request)
                {
                    bool addNew = false;
                    if (img360List == null || img360List.Count == 0)
                    {
                        addNew = true;
                    }
                    if (!addNew)
                    {
                        ProductStageImage360 current = img360List.Where(w => w.ImageId == img.ImageId).SingleOrDefault();
                        if (current != null)
                        {
                            current.Position = index++;
                            current.UpdateBy = email;
                            current.UpdateOn = DateTime.Now;
                            img360List.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        ProductStageImage360 imgEntity = new ProductStageImage360();
                        imgEntity.Pid = pid;
                        imgEntity.ShopId = shopId;
                        imgEntity.Position = index++;
                        imgEntity.Path = img.tmpPath;
                        imgEntity.ImageUrlEn = img.url;
                        imgEntity.Status = Constant.STATUS_ACTIVE;
                        imgEntity.CreateBy = email;
                        imgEntity.CreateOn = DateTime.Now;
                        imgEntity.UpdateBy = email;
                        imgEntity.UpdateOn = DateTime.Now;
                        db.ProductStageImage360.Add(imgEntity);
                    }
                }
            }
            if (img360List != null && img360List.Count > 0)
            {
                db.ProductStageImage360.RemoveRange(img360List);
            }
        }

        private string SaveChangeImg(ColspEntities db, string pid, int shopId, List<ImageRequest> imgRequest, string email)
        {
            
            var imgList = db.ProductStageImages.Where(w => w.Pid.Equals(pid) && w.ShopId == shopId).ToList();
            bool featureImg = true;
            string featureImgUrl = null;
            if (imgRequest != null && imgRequest.Count > 0)
            {
                int index = 0;
                foreach (ImageRequest img in imgRequest)
                {
                    bool addNew = false;
                    if (imgList == null || imgList.Count == 0)
                    {
                        addNew = true;
                    }
                    if (!addNew)
                    {
                        ProductStageImage current = imgList.Where(w => w.ImageId == img.ImageId).SingleOrDefault();
                        if (current != null)
                        {
                            current.FeatureFlag = featureImg;
                            current.Position = index++;
                            current.UpdateBy = email;
                            current.UpdateOn = DateTime.Now;
                            imgList.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        ProductStageImage imgEntity = new ProductStageImage();
                        imgEntity.Pid = pid;
                        imgEntity.ShopId = shopId;
                        imgEntity.FeatureFlag = featureImg;
                        imgEntity.Position = index++;
                        imgEntity.Path = img.tmpPath;
                        imgEntity.ImageUrlEn = img.url;
                        imgEntity.Status = Constant.STATUS_ACTIVE;
                        imgEntity.CreateBy = email;
                        imgEntity.CreateOn = DateTime.Now;
                        imgEntity.UpdateBy = email;
                        imgEntity.UpdateOn = DateTime.Now;
                        db.ProductStageImages.Add(imgEntity);
                    }
                    featureImg = false;
                }
                featureImgUrl = imgRequest[0].url;
            }
            if (imgList != null && imgList.Count > 0)
            {
                db.ProductStageImages.RemoveRange(imgList);
            }
            return featureImgUrl;
        }

        private List<VideoLinkRequest> SetupVdoResponse(ColspEntities db, string pid)
        {
            try
            {
                var vdos = (from vdo in db.ProductStageVideos
                            where vdo.Pid.Equals(pid) && vdo.Status.Equals(Constant.STATUS_ACTIVE)
                            orderby vdo.Position ascending
                            select vdo).ToList();
                if (vdos != null && vdos.Count > 0)
                {
                    List<VideoLinkRequest> newList = new List<VideoLinkRequest>();
                    foreach (ProductStageVideo v in vdos)
                    {
                        VideoLinkRequest vdo = new VideoLinkRequest();
                        vdo.VideoId = v.VideoId;
                        vdo.Url = v.VideoUrlEn;
                        newList.Add(vdo);
                    }
                    return newList;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private List<ImageRequest> SetupImg360Response(ColspEntities db, string pid)
        {
            try
            {
                var imgs = (from img in db.ProductStageImage360
                            where img.Pid.Equals(pid) && img.Status.Equals(Constant.STATUS_ACTIVE)
                            orderby img.Position ascending
                            select img).ToList();
                if (imgs != null && imgs.Count > 0)
                {
                    List<ImageRequest> newList = new List<ImageRequest>();
                    foreach (ProductStageImage360 i in imgs)
                    {
                        ImageRequest img = new ImageRequest();
                        img.ImageId = i.ImageId;
                        img.position = i.Position;
                        img.url = i.ImageUrlEn;
                        img.ImageName = i.ImageName;
                        newList.Add(img);
                    }
                    return newList;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private List<ImageRequest> SetupImgResponse(ColspEntities db, string pid)
        {
            try
            {
                var imgs = (from img in db.ProductStageImages
                            where img.Pid.Equals(pid) && img.Status.Equals(Constant.STATUS_ACTIVE)
                            orderby img.Position ascending
                            select img).ToList();
                if (imgs != null && imgs.Count > 0)
                {
                    List<ImageRequest> newList = new List<ImageRequest>();
                    foreach (ProductStageImage i in imgs)
                    {
                        ImageRequest img = new ImageRequest();
                        img.ImageId = i.ImageId;
                        img.position = i.Position;
                        img.url = i.ImageUrlEn;
                        img.ImageName = i.ImageName;
                        newList.Add(img);
                    }
                    return newList;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private void SetupAttributeEntity(ColspEntities db, List<AttributeRequest> attrList, int productId, string pid, string email)
        {
            int index = 0;
            foreach (AttributeRequest attr in attrList)
            {
                ProductStageAttribute attriEntity = new ProductStageAttribute();
                attriEntity.Position = index++;
                attriEntity.ProductId = productId;

                if(attr.AttributeValues != null && attr.AttributeValues.Count > 0)
                {
                    foreach (AttributeValueRequest val in attr.AttributeValues)
                    {
                        attriEntity.ValueEn = string.Concat("((", val.AttributeValueId, "))");
                        attriEntity.IsAttributeValue = true;
                        break;
                    }
                }
                else if(!string.IsNullOrWhiteSpace(attr.ValueEn))
                {
                    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                    if (rg.IsMatch(attr.ValueEn))
                    {
                        throw new Exception("Attribute value not allow");
                    }
                    attriEntity.ValueEn = attr.ValueEn;
                }
                else
                {
                    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                    if (rg.IsMatch(attr.ValueEn))
                    {
                        throw new Exception("Attribute value not allow");
                    }
                    attriEntity.ValueEn = attr.ValueEn;
                }


                attriEntity.AttributeId = attr.AttributeId.Value;
                attriEntity.Status = Constant.STATUS_ACTIVE;
                attriEntity.CreateBy = email;
                attriEntity.CreateOn = DateTime.Now;
                attriEntity.UpdateBy = email;
                attriEntity.UpdateOn = DateTime.Now;
                db.ProductStageAttributes.Add(attriEntity);
            }

        }

        private string SetupImgEntity(ColspEntities db, List<ImageRequest> imgs, string pid,int shopId, string email)
        {
            if (imgs == null || imgs.Count == 0) { return null; }
            string featureImgUrl = imgs[0].url;
            int index = 0;
            bool featureImg = true;
            foreach (ImageRequest imgRq in imgs)
            {
                ProductStageImage img = new ProductStageImage();
                img.Pid = pid;
                img.ShopId = shopId;
                img.Position = index++;
                img.FeatureFlag = featureImg;
                featureImg = false;
                img.Path = imgRq.tmpPath;
                img.ImageUrlEn = imgRq.url;
                img.Status = Constant.STATUS_ACTIVE;
                img.CreateBy = email;
                img.CreateOn = DateTime.Now;
                img.UpdateBy = email;
                img.UpdateOn = DateTime.Now;
                db.ProductStageImages.Add(img);
            }
            return featureImgUrl;
        }

        private void SetupImg360Entity(ColspEntities db, List<ImageRequest> imgs, string pid, int shopId, string email)
        {
            if (imgs == null) { return; }
            int index = 0;
            bool featureImg = true;
            foreach (ImageRequest imgRq in imgs)
            {
                ProductStageImage360 img = new ProductStageImage360();
                img.Pid = pid;
                img.ShopId = shopId;
                img.Position = index++;
                img.FeatureFlag = featureImg;
                featureImg = false;
                img.Path = imgRq.tmpPath;
                img.ImageUrlEn = imgRq.url;
                img.Status = Constant.STATUS_ACTIVE;
                img.CreateBy = email;
                img.CreateOn = DateTime.Now;
                img.UpdateBy = email;
                img.UpdateOn = DateTime.Now;
                db.ProductStageImage360.Add(img);
            }
        }

        private void SetupVdoEntity(ColspEntities db, List<VideoLinkRequest> vdos, string pid, int shopId, string email)
        {
            if (vdos == null) { return; }
            int index = 0;
            foreach (VideoLinkRequest vdoRq in vdos)
            {
                ProductStageVideo vdo = new ProductStageVideo();
                vdo.Pid = pid;
                vdo.ShopId = shopId;
                vdo.Position = index++;
                vdo.VideoUrlEn = vdoRq.Url;
                vdo.Status = Constant.STATUS_ACTIVE;
                vdo.CreateBy = email;
                vdo.CreateOn = DateTime.Now;
                vdo.UpdateBy = email;
                vdo.UpdateOn = DateTime.Now;
                db.ProductStageVideos.Add(vdo);
            }
        }

        List<List<string>> ReadExcel(CsvReader csvResult, string[] header, List<string> firstRow)
        {
            List<List<string>> listRow = new List<List<string>>() { firstRow };
            List<string> listColumn = null;
            while (csvResult.Read())
            {
                listColumn = new List<string>();
                foreach (string h in header)
                {
                    listColumn.Add(csvResult.GetField<string>(h));
                }
                listRow.Add(listColumn);
            }
            return listRow;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductStageExists(int id)
        {
            return db.ProductStages.Count(e => e.ProductId == id) > 0;
        }
      */
    }

}