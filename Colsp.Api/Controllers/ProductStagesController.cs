﻿using System;
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

namespace Colsp.Api.Controllers
{
    public class ProductStagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/ProductStages")]
        [HttpGet]
        public HttpResponseMessage GetProductStages([FromUri] ProductRequest request)
		{
            try
            {
                request.DefaultOnNull();
                IQueryable<ProductStage> products = null;

                // List all product
                products = db.ProductStages.Where(p => true);
                if (request.SearchText != null)
                {
                    products = products.Where(p => p.Sku.Contains(request.SearchText)
                    || p.ProductNameEn.Contains(request.SearchText)
                    || p.ProductNameTh.Contains(request.SearchText));
                }
                if (request.SellerId != null)
                {
                    products = products.Where(p => p.SellerId.Equals(request.SellerId));
                }
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if ("Draft".Equals(request._filter))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_DRAFT));
                    }
                    else if("Approved".Equals(request._filter))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_APPROVE));
                    }
                    else if("Not Approved".Equals(request._filter))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_NOT_APPROVE));
                    }
                    else if("Wait for Approval".Equals(request._filter))
                    {
                        products = products.Where(p => p.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL));
                    }
                }

                var total = products.Count();
                var pagedProducts = products.GroupJoin(db.ProductStageImages,
                                                p => p.Pid,
                                                m => m.Pid,
                                                (p, m) => new
                                                {
                                                    p.Sku,
                                                    p.ProductId,
                                                    p.ProductNameEn,
                                                    p.ProductNameTh,
                                                    p.OriginalPrice,
                                                    p.SalePrice,
                                                    p.Status,
                                                    p.ImageFlag,
                                                    p.InfoFlag,
                                                    p.UpdatedDt,
                                                    p.CreatedDt,
                                                    ImageUrl = m.FirstOrDefault().ImageUrlEn
                                                }
                                            )
                                            .Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
			
		}

        [Route("api/ProductStages/{productId}")]
        [HttpGet]
        public HttpResponseMessage GetProductStage(int productId)
        {
            try
            {
                var stage = (from stg in db.ProductStages
                             join v in db.ProductStageVariants on stg.ProductId equals v.ProductId into j
                             from j2 in j.DefaultIfEmpty()
                             join a in db.Attributes on j2.Attribute1Id equals a.AttributeId into j3
                             from j4 in j3.DefaultIfEmpty()
                             join a in db.Attributes on j2.Attribute2Id equals a.AttributeId into j5
                             from j6 in j5.DefaultIfEmpty()
                             join b in db.Brands on stg.BrandId equals b.BrandId into j7
                             from j8 in j7.DefaultIfEmpty()
                             where stg.ProductId == productId 
                              //group stg by new { stg , j2 ,j4,j6,j8,j9} into g
                              select new
                            {
                                stg,
                                j2,
                                j4,
                                j6,
                                j8,
                                attrMap = from a in db.ProductStageAttributes.Where(w=>w.ProductId==productId).DefaultIfEmpty() select a

                            }).AsEnumerable().Select(s=>s.stg).SingleOrDefault();
                //var stage = stages[0];
                if (stage != null)
                {
                    ProductStageRequest response = new ProductStageRequest();
                    response.MasterVariant.ProductNameTh = stage.ProductNameTh;
                    response.MasterVariant.ProductNameEn = stage.ProductNameEn;
                    response.MasterVariant.Sku = stage.Sku;
                    response.MasterVariant.Upc = stage.Upc;
                    response.Brand.BrandId = stage.BrandId ;
                    response.Brand.BrandNameEn = stage.Brand.BrandNameEn;
                    response.MasterVariant.OriginalPrice = stage.OriginalPrice ;
                    response.MasterVariant.SalePrice = stage.SalePrice;
                    response.MasterVariant.DescriptionFullTh = stage.DescriptionFullTh;
                    response.MasterVariant.DescriptionShortTh = stage.DescriptionShortTh;
                    response.MasterVariant.DescriptionFullEn = stage.DescriptionFullEn ;
                    response.MasterVariant.DescriptionShortEn = stage.DescriptionShortEn ;
                    response.AttributeSet.AttributeSetId = stage.AttributeSetId;
                    response.Keywords = stage.Tag;
                    response.ShippingMethod = stage.ShippingId;
                    response.PrepareDay = stage.PrepareDay;
                    response.MasterVariant.Length = stage.Length;
                    response.MasterVariant.Height = stage.Height;
                    response.MasterVariant.Width = stage.Width;
                    response.MasterVariant.Weight = stage.Weight;
                    response.MasterVariant.DimensionUnit = stage.DimensionUnit;
                    response.MasterVariant.WeightUnit = stage.WeightUnit;
                    response.GlobalCategory = stage.GlobalCatId;
                    response.LocalCategory = stage.LocalCatId;
                    response.SEO.MetaTitle = stage.MetaTitle;
                    response.SEO.MetaDescription = stage.MetaDescription;
                    response.SEO.MetaKeywords = stage.MetaKey;
                    response.SEO.ProductUrlKeyEn = stage.UrlEn;
                    response.SEO.ProductUrlKeyTh = stage.UrlTh;
                    #region Setup Effective Date & Time 
                    if (stage.EffectiveDate != null)
                    {
                        response.EffectiveDate = stage.EffectiveDate.Value.ToString("MMMM dd, yyyy");
                    }
                    if(stage.EffectiveTime != null)
                    {
                        response.EffectiveTime = stage.EffectiveTime.Value.ToString(@"hh\:mm");
                    }
                    #endregion
                    #region Setup Expire Date & Time
                    if (stage.ExpiryDate != null)
                    {
                        response.ExpireDate = stage.ExpiryDate.Value.ToString("MMMM dd, yyyy");
                    }
                    if(stage.ExpiryTime != null)
                    {
                        response.ExpireTime = stage.ExpiryTime.Value.ToString(@"hh\:mm");
                    }
                    #endregion
                    response.Remark = stage.Remark;
                    response.Status = stage.Status;
                    response.SellerId = stage.SellerId;
                    response.ShopId = stage.ShopId;
                    response.MasterVariant.Pid = stage.Pid;
                    response.ProductId = stage.ProductId;

                    response.MasterAttribute = SetupAttributeResponse(stage.ProductStageAttributes.ToList());
                    #region Setup Inventory
                    var inventory = (from inv in db.Inventories
                                     where inv.Pid.Equals(stage.Pid)
                                     select inv).SingleOrDefault();
                    if(inventory != null)
                    {
                        response.MasterVariant.SafetyStock = inventory.SaftyStockSeller;
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
                        foreach(GlobalCategory c in globalCatList)
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
                    if(localCatList != null && localCatList.Count > 0)
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
                    var relatedList = db.ProductStageRelateds.Where(w => w.Pid1.Equals(stage.Pid)).ToList();
                    if(relatedList != null && relatedList.Count > 0)
                    {
                        List<string> relate = new List<string>();
                        foreach (ProductStageRelated r in relatedList)
                        {
                            relate.Add(r.Pid2);
                        }
                        response.RelatedProducts = relate;
                    }
                    List<VariantRequest> varList = new List<VariantRequest>();
                    foreach(ProductStageVariant variantEntity in stage.ProductStageVariants)
                    {
                        VariantRequest varient = new VariantRequest();
                        varient.VariantId = variantEntity.VariantId;
                        varient.Pid = variantEntity.Pid;
                        varient.FirstAttribute.AttributeId = variantEntity.Attribute1Id;
                        varient.FirstAttribute.ValueEn = variantEntity.ValueEn1;
                        varient.SecondAttribute.AttributeId = variantEntity.Attribute2Id;
                        varient.SecondAttribute.ValueEn = variantEntity.ValueEn2;
                        varient.DefaultVariant = variantEntity.DefaultVaraint;
                        #region Setup Variant Inventory
                        inventory = (from inv in db.Inventories
                                         where inv.Pid.Equals(variantEntity.Pid)
                                         select inv).SingleOrDefault();
                        if (inventory != null)
                        {
                            varient.SafetyStock = inventory.SaftyStockSeller;
                            varient.Quantity = inventory.Quantity;
                            varient.StockType = Constant.STOCK_TYPE.Where(w => w.Value.Equals(inventory.StockAvailable)).SingleOrDefault().Key;
                        }
                        #endregion


                        varient.Images = SetupImgResponse(db, variantEntity.Pid);
                        varient.VideoLinks = SetupVdoResponse(db, variantEntity.Pid);

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
                        varient.Length = variantEntity.Length;
                        varient.Height = variantEntity.Height;
                        varient.Width = variantEntity.Width;
                        varient.Weight = variantEntity.Weight;
                        varient.DimensionUnit = variantEntity.DimensionUnit;
                        varient.WeightUnit = variantEntity.WeightUnit;
                        varList.Add(varient);
                    }
                    response.Variants = varList;
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
        
        [Route("api/ProductStages")]
        [HttpPost]
        public HttpResponseMessage AddProduct(ProductStageRequest request)
        {
            ProductStage stage = null;
            try
            {
                if (request.SellerId == null || request.SellerId == 0 || this.User.UserId() != request.SellerId
                    || this.User.ShopIds().Where(w => w == request.SellerId).SingleOrDefault() == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Shop is invalid. Cannot find shop in session");
                }

                #region Setup Master Product
                stage = new ProductStage();

                HttpResponseMessage error = SetupProductStage(stage, request);
                if (error != null)
                {
                    return error;
                }
                stage.Status = Constant.PRODUCT_STATUS_JUNK;
                stage.SellerId = request.SellerId;
                stage.ShopId = request.ShopId;
                string masterPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                stage.Pid = masterPid;

                stage.OnlineFlag = false;

                stage.CreatedBy = this.User.Email();
                stage.CreatedDt = DateTime.Now;
                #region Validation
                if (stage.SellerId == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Seller is required");
                }
                else
                {
                    if(stage.SellerId != this.User.UserId())
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Seller is invalid");
                    }
                }
                if (stage.ShopId == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Shop is required");
                }
                else
                {
                    var shop = this.User.ShopIds().Where(w => w.Equals(stage.ShopId));
                    if(shop == null || shop.Count() == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Shop is invalid");
                    }
                }
                
                if (stage.GlobalCatId == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Global category is required");
                }
                if (string.IsNullOrEmpty(request.Status))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Status is required");
                }
                else if (request.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL))
                {
                    if (string.IsNullOrEmpty(stage.ProductNameTh))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Product Name (Thai) is required");
                    }
                    if (string.IsNullOrEmpty(stage.ProductNameEn))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Product Name (English) is required");
                    }
                    if (string.IsNullOrEmpty(stage.Sku))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "SKU is required");
                    }
                    if (stage.OriginalPrice == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Original Price is required");
                    }
                    if (stage.PrepareDay == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Preparation Time is required");
                    }
                    if (stage.Length == null || stage.Height == null || stage.Width == null || string.IsNullOrEmpty(stage.DimensionUnit))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Package Dimension is required");
                    }
                    if (stage.Weight == null || string.IsNullOrEmpty(stage.WeightUnit))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Weight is required");
                    }
                }
                else if (request.Status.Equals(Constant.PRODUCT_STATUS_DRAFT))
                {
                    if (string.IsNullOrEmpty(stage.ProductNameTh) || string.IsNullOrEmpty(stage.ProductNameEn) || string.IsNullOrEmpty(stage.Sku) 
                        || stage.OriginalPrice == null || stage.PrepareDay == null 
                        || stage.Length == null || stage.Height == null || stage.Width == null || string.IsNullOrEmpty(stage.DimensionUnit)
                        || stage.Weight == null || string.IsNullOrEmpty(stage.WeightUnit))
                    {
                        stage.InfoFlag = false;
                    }
                    else
                    {
                        stage.InfoFlag = true;
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Status is invalid");
                }
                #endregion
                db.ProductStages.Add(stage);
                #endregion
                db.SaveChanges();
                stage.Status = request.Status;

                #region Setup Master Attribute
                if (request.MasterAttribute != null)
                {
                   SetupAttributeEntity(db, request.MasterAttribute, stage.ProductId, masterPid, this.User.Email());
                }
                #endregion
                #region Setup Inventory
                Inventory masterInventory = new Inventory();
                masterInventory.Quantity = request.MasterVariant.Quantity;
                masterInventory.SaftyStockSeller = request.MasterVariant.SafetyStock;
                if(request.MasterVariant.StockType != null)
                {
                    if (Constant.STOCK_TYPE.ContainsKey(request.MasterVariant.StockType))
                    {
                        masterInventory.StockAvailable = Constant.STOCK_TYPE[request.MasterVariant.StockType];
                    }
                }
                masterInventory.Pid = masterPid;
                masterInventory.CreatedBy = this.User.Email();
                masterInventory.CreatedDt = DateTime.Now;
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
                masterInventoryHist.CreatedBy = this.User.Email();
                masterInventoryHist.CreatedDt = DateTime.Now;
                db.InventoryHistories.Add(masterInventoryHist);
                #endregion
                stage.ImageFlag = SetupImgEntity(db, request.MasterVariant.Images, masterPid,this.User.Email());
                SetupImg360Entity(db, request.MasterVariant.Images360, masterPid, this.User.Email());
                SetupVdoEntity(db, request.MasterVariant.VideoLinks, masterPid, this.User.Email());
                #region Setup Related GlobalCategories
                if(request.GlobalCategories != null)
                {
                    foreach (CategoryRequest cat in request.GlobalCategories)
                    {
                        ProductStageGlobalCatMap map = new ProductStageGlobalCatMap();
                        map.CategoryId = cat.CategoryId;
                        map.ProductId = stage.ProductId;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreatedBy = this.User.Email();
                        map.CreatedDt = DateTime.Now;
                        db.ProductStageGlobalCatMaps.Add(map);
                    }
                }
                #endregion
                #region Setup Related LocalCategories
                if(request.LocalCategories != null)
                {
                    foreach (CategoryRequest cat in request.LocalCategories)
                    {
                        ProductStageLocalCatMap map = new ProductStageLocalCatMap();
                        map.CategoryId = cat.CategoryId;
                        map.ProductId = stage.ProductId;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreatedBy = this.User.Email();
                        map.CreatedDt = DateTime.Now;
                        db.ProductStageLocalCatMaps.Add(map);
                    }
                }
                #endregion
                #region Setup Related Product
                if (request.RelatedProducts != null)
                {
                    foreach (string pro in request.RelatedProducts)
                    {
                        ProductStageRelated relate = new ProductStageRelated();
                        relate.Pid1 = masterPid;
                        relate.Pid2 = pro;
                        relate.CreatedBy = this.User.Email();
                        relate.CreatedDt = DateTime.Now;
                        db.ProductStageRelateds.Add(relate);
                    }
                }
                #endregion
                
                #region Setup variant
                foreach (VariantRequest variantRq in request.Variants)
                {
                    if (variantRq.FirstAttribute == null ||
                        variantRq.SecondAttribute == null)
                    {
                        throw new Exception("Invalid variant format");
                    }

                    ProductStageVariant variant = new ProductStageVariant();
                    string variantPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                    variant.Pid = variantPid;
                    variant.ProductId = stage.ProductId;
                    variant.Attribute1Id = variantRq.FirstAttribute.AttributeId;
                    variant.Attribute2Id = variantRq.SecondAttribute.AttributeId;
                    variant.ValueEn1 = variantRq.FirstAttribute.ValueEn;
                    variant.ValueEn2 = variantRq.SecondAttribute.ValueEn;

                    #region Setup Variant Inventory
                    Inventory variantInventory = new Inventory();
                    variantInventory.Quantity = variantRq.Quantity;
                    variantInventory.SaftyStockSeller = variantRq.SafetyStock;
                    if (variantRq.StockType != null)
                    {
                        if (Constant.STOCK_TYPE.ContainsKey(variantRq.StockType))
                        {
                            variantInventory.StockAvailable = Constant.STOCK_TYPE[variantRq.StockType];
                        }
                    }
                    
                    variantInventory.Pid = variantPid;
                    db.Inventories.Add(variantInventory);

                    InventoryHistory variantInventoryHist = new InventoryHistory();
                    variantInventoryHist.StockAvailable = variantRq.Quantity;
                    variantInventoryHist.SafetyStockSeller = variantRq.SafetyStock;
                    if (variantRq.StockType != null)
                    {
                        if (Constant.STOCK_TYPE.ContainsKey(variantRq.StockType))
                        {
                            variantInventoryHist.StockAvailable = Constant.STOCK_TYPE[variantRq.StockType];
                        }
                    }
                    variantInventoryHist.Pid = variantPid;
                    variantInventoryHist.Description = "Add new variant";
                    variantInventoryHist.CreatedBy = this.User.Email();
                    variantInventoryHist.CreatedDt = DateTime.Now;
                    db.InventoryHistories.Add(masterInventoryHist);
                    #endregion

                    SetupImgEntity(db, variantRq.Images, variantPid,this.User.Email());
                    SetupVdoEntity(db, variantRq.VideoLinks, variantPid, this.User.Email());
                    
                    variant.ProductNameTh = variantRq.ProductNameTh;
                    variant.ProductNameEn = variantRq.ProductNameEn;
                    variant.Sku = variantRq.Sku;
                    variant.Upc = variantRq.Upc;
                    variant.OriginalPrice = variantRq.OriginalPrice;
                    variant.SalePrice = variantRq.SalePrice;
                    variant.DescriptionFullTh = variantRq.DescriptionFullTh;
                    variant.DescriptionShortTh = variantRq.DescriptionShortTh;
                    variant.DescriptionFullEn = variantRq.DescriptionFullEn;
                    variant.DescriptionShortEn = variantRq.DescriptionShortEn;
                    variant.Length = variantRq.Length;
                    variant.Height = variantRq.Height;
                    variant.Width = variantRq.Width;
                    variant.Weight = variantRq.Weight;
                    variant.DimensionUnit = variantRq.DimensionUnit;
                    variant.WeightUnit = variantRq.WeightUnit;
                    variant.DefaultVaraint = variantRq.DefaultVariant;
                    variant.CreatedBy = this.User.Email();
                    variant.CreatedDt = DateTime.Now;

                    db.ProductStageVariants.Add(variant);
                }
                #endregion
                db.SaveChanges();
                return GetProductStage(stage.ProductId);
            }
            catch(Exception ex)
            {
                #region Rollback
                db.Dispose();
                db = new ColspEntities();
                db.ProductStageAttributes.RemoveRange(db.ProductStageAttributes.Where(w => w.ProductId.Equals(stage.ProductId)));
                db.ProductStageGlobalCatMaps.RemoveRange(db.ProductStageGlobalCatMaps.Where(w => w.ProductId.Equals(stage.ProductId)));
                db.ProductStageLocalCatMaps.RemoveRange(db.ProductStageLocalCatMaps.Where(w => w.ProductId.Equals(stage.ProductId)));
                db.ProductStageVariants.RemoveRange(db.ProductStageVariants.Where(w => w.ProductId.Equals(stage.ProductId)));
                db.ProductStages.RemoveRange(db.ProductStages.Where(w=>w.ProductId.Equals(stage.ProductId)));
                try
                {
                    db.SaveChanges();
                }
                catch (Exception wee)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, wee);
                }
                #endregion
      
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [Route("api/ProductStages/{productId}")]
        [HttpPut]
        public HttpResponseMessage SaveChange([FromUri]int productId,ProductStageRequest request)
        {
            try
            {
                if (request.SellerId == null || request.SellerId == 0 || this.User.ShopIds().Where(w => w == request.SellerId).SingleOrDefault() == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Shop is invalid. Cannot find shop in session");
                }
                if (productId == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Product is invalid");
                }
                var stage = (from stg in db.ProductStages
                             from var in db.ProductStageVariants.Where(w => w.ProductId==stg.ProductId).DefaultIfEmpty()
                             where stg.ProductId==productId && stg.SellerId== request.SellerId
                             select new
                             {
                                 stg,
                                 var,
                                 attrMap = from attr in db.ProductStageAttributes.Where(w => w.ProductId.Equals(stg.ProductId)).DefaultIfEmpty() select attr
                             }).AsEnumerable().Select(s => s.stg).SingleOrDefault();
                if (stage != null)
                {
                    if (stage.Status == null || !stage.Status.Equals(Constant.PRODUCT_STATUS_DRAFT))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Product is not allow");
                    }
                    #region Setup Master
                    HttpResponseMessage error = SetupProductStage(stage, request);
                    if (error != null)
                    {
                        return error;
                    }
                    stage.Status = Constant.PRODUCT_STATUS_DRAFT;
                    stage.UpdatedBy = this.User.Email();
                    stage.UpdatedDt = DateTime.Now;
                    #region Setup Attribute
                    List<ProductStageAttribute> attrList = stage.ProductStageAttributes.ToList();
                    if (request.MasterAttribute != null)
                    {
                        int index = 0;
                        foreach (AttributeRequest attr in request.MasterAttribute)
                        {
                            bool addNew = false;
                            if (attrList == null || attrList.Count == 0)
                            {
                                addNew = true;
                            }
                            if (!addNew)
                            {
                                ProductStageAttribute current = attrList.Where(w => w.AttributeId == attr.AttributeId).SingleOrDefault();
                                if (current != null)
                                {
                                    current.ValueEn = attr.ValueEn;
                                    current.UpdatedBy = this.User.Email();
                                    current.UpdatedDt = DateTime.Now;
                                    attrList.Remove(current);
                                }
                                else
                                {
                                    addNew = true;
                                }
                            }
                            if(addNew)
                            {
                                ProductStageAttribute attriEntity = new ProductStageAttribute();
                                attriEntity.Position = index++;
                                attriEntity.ProductId = stage.ProductId;
                                attriEntity.AttributeId = attr.AttributeId.Value;
                                attriEntity.Pid = stage.Pid;
                                attriEntity.ValueEn = attr.ValueEn;
                                attriEntity.Status = Constant.STATUS_ACTIVE;
                                attriEntity.CreatedBy = this.User.Email();
                                attriEntity.CreatedDt = DateTime.Now;
                                db.ProductStageAttributes.Add(attriEntity);
                            }
                           
                        }
                    }
                    
                    if (attrList != null && attrList.Count > 0)
                    {
                        db.ProductStageAttributes.RemoveRange(attrList);
                    }
                    #endregion
                    SaveChangeInventory(db, stage.Pid, request.MasterVariant, this.User.Email());
                    SaveChangeInventoryHistory(db, stage.Pid, request.MasterVariant, this.User.Email());
                    SaveChangeImg(db, stage.Pid, request.MasterVariant.Images,this.User.Email());
                    SaveChangeImg360(db, stage.Pid, request.MasterVariant.Images360,this.User.Email());
                    SaveChangeVideoLinks(db, stage.Pid, request.MasterVariant.VideoLinks,this.User.Email());
                    SaveChangeRelatedProduct(db, stage.Pid, request.RelatedProducts,this.User.Email());
                    SaveChangeGlobalCat(db, stage.ProductId, request.GlobalCategories, this.User.Email());
                    SaveChangeLocalCat(db, stage.ProductId, request.LocalCategories, this.User.Email());
                    #endregion
                    #region Setup Variant
                    List<ProductStageVariant> varList = null;
                    if (stage.ProductStageVariants == null || stage.ProductStageVariants.Count == 0)
                    {
                        varList = stage.ProductStageVariants.ToList();
                    }
                    
                    foreach (VariantRequest var in request.Variants)
                    {
                        if (var.FirstAttribute == null ||
                                var.SecondAttribute == null)
                        {
                            throw new Exception("Invalid variant format");
                        }
                        bool addNew = false;
                        if (varList == null || varList.Count == 0)
                        {
                            addNew = true;
                        }
                        ProductStageVariant current = null;
                        if (!addNew)
                        {
                            current = varList.Where(w => w.VariantId == var.VariantId).SingleOrDefault();
                            if (current != null)
                            {
                                current.UpdatedBy = this.User.Email();
                                current.UpdatedDt = DateTime.Now;
                                varList.Remove(current);
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

                            current.CreatedBy = this.User.Email();
                            current.CreatedDt = DateTime.Now;
                        }
                        current.Attribute1Id = var.FirstAttribute.AttributeId;
                        current.Attribute2Id = var.SecondAttribute.AttributeId;
                        current.ValueEn1 = var.ValueEn;
                        SaveChangeInventory(db, current.Pid, var, this.User.Email());
                        SaveChangeInventoryHistory(db, current.Pid, var, this.User.Email());
                        SaveChangeImg(db, current.Pid, var.Images, this.User.Email());
                        SaveChangeVideoLinks(db, current.Pid, var.VideoLinks, this.User.Email());

                        current.ProductNameTh = var.ProductNameTh;
                        current.ProductNameEn = var.ProductNameEn;
                        current.Sku = var.Sku;
                        current.Upc = var.Upc;
                        current.OriginalPrice = var.OriginalPrice;
                        current.SalePrice = var.SalePrice;
                        current.DescriptionFullTh = var.DescriptionFullTh;
                        current.DescriptionShortTh = var.DescriptionShortTh;
                        current.DescriptionFullEn = var.DescriptionFullEn;
                        current.DescriptionShortEn = var.DescriptionShortEn;
                        current.Length = var.Length;
                        current.Height = var.Height;
                        current.Width = var.Width;
                        current.Weight = var.Weight;
                        current.DimensionUnit = var.DimensionUnit;
                        current.WeightUnit = var.WeightUnit;
                        current.DefaultVaraint = var.DefaultVariant;
                        if (addNew)
                        {
                            db.ProductStageVariants.Add(current);
                        }
                    }
                    if(varList != null && varList.Count > 0)
                    {
                        db.ProductStageVariants.RemoveRange(varList);
                    }
                    #endregion
                    db.SaveChanges();
                    return GetProductStage(stage.ProductId);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Product is invalid");
                }
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,e);
            }
        }

        private HttpResponseMessage SetupProductStage(ProductStage stage, ProductStageRequest request)
        {
            stage.ProductNameTh = request.MasterVariant.ProductNameTh;
            stage.ProductNameEn = request.MasterVariant.ProductNameEn;
            stage.Sku = request.MasterVariant.Sku;
            stage.BrandId = request.Brand.BrandId;
            stage.OriginalPrice = request.MasterVariant.OriginalPrice;
            stage.SalePrice = request.MasterVariant.SalePrice;
            stage.DescriptionFullTh = request.MasterVariant.DescriptionFullTh;
            stage.DescriptionShortTh = request.MasterVariant.DescriptionShortTh;
            stage.DescriptionFullEn = request.MasterVariant.DescriptionFullEn;
            stage.DescriptionShortEn = request.MasterVariant.DescriptionShortEn;
            stage.AttributeSetId = request.AttributeSet.AttributeSetId;
            stage.Tag = request.Keywords;
            stage.ShippingId = request.ShippingMethod;
            stage.PrepareDay = request.PrepareDay;
            stage.Length = request.MasterVariant.Length;
            stage.Height = request.MasterVariant.Height;
            stage.Width = request.MasterVariant.Width;
            stage.Weight = request.MasterVariant.Weight;
            stage.DimensionUnit = request.MasterVariant.DimensionUnit;
            stage.WeightUnit = request.MasterVariant.WeightUnit;
            stage.GlobalCatId = request.GlobalCategory;
            stage.LocalCatId = request.LocalCategory;
            stage.MetaTitle = request.SEO.MetaTitle;
            stage.MetaDescription = request.SEO.MetaDescription;
            stage.MetaKey = request.SEO.MetaKeywords;
            stage.UrlEn = request.SEO.ProductUrlKeyEn;
            stage.UrlTh = request.SEO.ProductUrlKeyTh;
            #region Setup Effective Date & Time 
            if (!string.IsNullOrEmpty(request.EffectiveDate))
            {
                try
                {
                    stage.EffectiveDate = Convert.ToDateTime(request.EffectiveDate);
                }
                catch
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.DateFormatError);
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
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.TimeFormatError);
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
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.DateFormatError);
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
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.TimeFormatError);
                }
            }
            else
            {
                stage.ExpiryTime = null;
            }
            #endregion
            stage.Remark = request.Remark;
            
            return null;
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
            masterInventoryHist.CreatedBy = email;
            masterInventoryHist.CreatedDt = DateTime.Now;
            db.InventoryHistories.Add(masterInventoryHist);
        }

        private void SaveChangeInventory(ColspEntities db, string pid, VariantRequest variant,string email)
        {
            Inventory masterInventory = db.Inventories.Find(pid);
            bool isNew = false;
            if (masterInventory == null)
            {
                masterInventory = new Inventory();
                masterInventory.Pid = pid;
                masterInventory.CreatedBy = email;
                masterInventory.CreatedDt = DateTime.Now;
                isNew = true;
            }
            else
            {
                masterInventory.UpdatedBy = email;
                masterInventory.UpdatedDt = DateTime.Now;
            }
            masterInventory.Quantity = variant.Quantity;
            masterInventory.SaftyStockSeller = variant.SafetyStock;
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
                foreach (CategoryRequest cat in localCategories)
                {
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
                            catList.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        ProductStageLocalCatMap catEntity = new ProductStageLocalCatMap();
                        catEntity.CategoryId = cat.CategoryId;
                        catEntity.ProductId = ProductId;
                        catEntity.CreatedBy = email;
                        catEntity.Status = Constant.STATUS_ACTIVE;
                        catEntity.CreatedDt = DateTime.Now;
                        db.ProductStageLocalCatMaps.Add(catEntity);
                    }
                }
            }
            if (catList != null && catList.Count > 0)
            {
                db.ProductStageLocalCatMaps.RemoveRange(catList);
            }
        }

        private void SaveChangeGlobalCat(ColspEntities db, int ProductId, List<CategoryRequest> globalCategories,string email)
        {

            var catList = db.ProductStageGlobalCatMaps.Where(w => w.ProductId.Equals(ProductId)).ToList();
            if (globalCategories != null)
            {
                foreach (CategoryRequest cat in globalCategories)
                {
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
                            catList.Remove(current);
                        }
                        else
                        {
                            addNew = true;
                        }
                    }
                    if (addNew)
                    {
                        ProductStageGlobalCatMap catEntity = new ProductStageGlobalCatMap();
                        catEntity.CategoryId = cat.CategoryId;
                        catEntity.ProductId = ProductId;
                        catEntity.CreatedBy = email;
                        catEntity.Status = Constant.STATUS_ACTIVE;
                        catEntity.CreatedDt = DateTime.Now;
                        db.ProductStageGlobalCatMaps.Add(catEntity);
                    }
                }
            }
            if (catList != null && catList.Count > 0)
            {
                db.ProductStageGlobalCatMaps.RemoveRange(catList);
            }
        }

        private void SaveChangeRelatedProduct(ColspEntities db, string pid, List<string> pidList,string email)
        {
            var relateList = db.ProductStageRelateds.Where(w => w.Pid1 == pid).ToList();
            if (relateList != null)
            {
                foreach (string pro in pidList)
                {
                    bool addNew = false;
                    if (relateList == null || relateList.Count == 0)
                    {
                        addNew = true;
                    }
                    if (!addNew)
                    {
                        ProductStageRelated current = relateList.Where(w => w.Pid2 == pro).SingleOrDefault();
                        if (current != null)
                        {
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
                        proEntity.Pid1 = pro;
                        proEntity.CreatedBy = email;
                        proEntity.Status = Constant.STATUS_ACTIVE;
                        proEntity.CreatedDt = DateTime.Now;
                        db.ProductStageRelateds.Add(proEntity);
                    }
                }
            }
            if (relateList != null && relateList.Count > 0)
            {
                db.ProductStageRelateds.RemoveRange(relateList);
            }
        }

        private void SaveChangeVideoLinks(ColspEntities db, string pid, List<VideoLinkRequest> videoRequest,string email)
        {
            var vdoList = db.ProductStageVideos.Where(w => w.Pid.Equals(pid)).ToList();
            if (videoRequest != null)
            {
                foreach (VideoLinkRequest vdo in videoRequest)
                {
                    int index = 0;
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
                            current.VideoUrlEn = vdo.Url;
                            current.UpdatedBy = email;
                            current.UpdatedDt = DateTime.Now;
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
                        vdoEntity.Position = index++;
                        vdoEntity.VideoUrlEn = vdo.Url;
                        vdoEntity.CreatedBy = email;
                        vdoEntity.Status = Constant.STATUS_ACTIVE;
                        vdoEntity.CreatedDt = DateTime.Now;
                        db.ProductStageVideos.Add(vdoEntity);
                    }
                }
            }
            if (vdoList != null && vdoList.Count > 0)
            {
                db.ProductStageVideos.RemoveRange(vdoList);
            }
        }

        private void SaveChangeImg360(ColspEntities db, string pid, List<ImageRequest> img360Request,string email)
        {
            var img360List = db.ProductStageImage360.Where(w => w.Pid.Equals(pid)).ToList();
            if (img360Request != null)
            {
                foreach (ImageRequest img in img360Request)
                {
                    int index = 0;
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
                        imgEntity.Position = index++;
                        imgEntity.Path = img.tmpPath;
                        imgEntity.ImageUrlEn = img.url;
                        imgEntity.CreatedBy = email;
                        imgEntity.Status = Constant.STATUS_ACTIVE;
                        imgEntity.CreatedDt = DateTime.Now;
                        db.ProductStageImage360.Add(imgEntity);
                    }
                }
            }
            if (img360List != null && img360List.Count > 0)
            {
                db.ProductStageImage360.RemoveRange(img360List);
            }
        }

        private void SaveChangeImg(ColspEntities db, string pid,List<ImageRequest> imgRequest,string email)
        {
            var imgList = db.ProductStageImages.Where(w => w.Pid.Equals(pid)).ToList();
            if (imgRequest != null)
            {
                foreach (ImageRequest img in imgRequest)
                {
                    int index = 0;
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
                        imgEntity.Position = index++;
                        imgEntity.Path = img.tmpPath;
                        imgEntity.ImageUrlEn = img.url;
                        imgEntity.CreatedBy = email;
                        imgEntity.Status = Constant.STATUS_ACTIVE;
                        imgEntity.CreatedDt = DateTime.Now;
                        db.ProductStageImages.Add(imgEntity);
                    }
                }
            }
            if (imgList != null && imgList.Count > 0)
            {
                db.ProductStageImages.RemoveRange(imgList);
            }
        }

        private List<AttributeRequest> SetupAttributeResponse(List<ProductStageAttribute> attriList)
        {
            if (attriList != null && attriList.Count > 0)
            {
                List<AttributeRequest> newList = new List<AttributeRequest>();
                foreach (ProductStageAttribute a in attriList)
                {
                    AttributeRequest attr = new AttributeRequest();
                    attr.AttributeId = a.AttributeId;
                    attr.ValueEn = a.ValueEn;
                    newList.Add(attr);
                }
                return newList;
            }
            else
            {
                return null;
            }
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

        private List<ImageRequest> SetupImgResponse(ColspEntities db,string pid)
        {
            try
            {
                var imgs = (from img in db.ProductStageImages
                           where img.Pid.Equals(pid) && img.Status.Equals(Constant.STATUS_ACTIVE)
                           orderby img.Position ascending
                            select img).ToList();
                if(imgs != null && imgs.Count > 0)
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

        private void SetupAttributeEntity(ColspEntities db,List<AttributeRequest> attrList, int productId ,string pid, string email)
        {
            int index = 0;
            foreach(AttributeRequest attr in attrList)
            {
                ProductStageAttribute attriEntity = new ProductStageAttribute();
                attriEntity.Position = index++;
                attriEntity.ProductId = productId;
                attriEntity.AttributeId = attr.AttributeId.Value;
                attriEntity.Pid = pid;
                attriEntity.ValueEn = attr.ValueEn;
                attriEntity.Status = Constant.STATUS_ACTIVE;
                attriEntity.CreatedBy = email;
                attriEntity.CreatedDt = DateTime.Now;
                db.ProductStageAttributes.Add(attriEntity);
            }
           
        }

        private bool SetupImgEntity(ColspEntities db,List<ImageRequest> imgs,string pid,string email)
        {
            if(imgs == null) { return false; }
            int index = 0;
            bool featureImg = true;
            foreach (ImageRequest imgRq in imgs)
            {
                ProductStageImage img = new ProductStageImage();
                img.Pid = pid;
                img.Position = index++;
                img.FeatureFlag = featureImg;
                featureImg = false;
                img.Path = imgRq.tmpPath;
                img.ImageUrlEn = imgRq.url;
                img.CreatedBy = email;
                img.Status = Constant.STATUS_ACTIVE;
                img.CreatedDt = DateTime.Now;
                db.ProductStageImages.Add(img);
            }
            return !featureImg;
        }

        private void SetupImg360Entity(ColspEntities db, List<ImageRequest> imgs, string pid, string email)
        {
            if (imgs == null) { return; }
            int index = 0;
            bool featureImg = true;
            foreach (ImageRequest imgRq in imgs)
            {
                ProductStageImage360 img = new ProductStageImage360();
                img.Pid = pid;
                img.Position = index++;
                img.FeatureFlag = featureImg;
                featureImg = false;
                img.Path = imgRq.tmpPath;
                img.ImageUrlEn = imgRq.url;
                img.CreatedBy = email;
                img.Status = Constant.STATUS_ACTIVE;
                img.CreatedDt = DateTime.Now;
                db.ProductStageImage360.Add(img);
            }
        }

        private void SetupVdoEntity(ColspEntities db,List<VideoLinkRequest> vdos,string pid, string email)
        {
            if(vdos == null) { return; }
            int index = 0;
            foreach (VideoLinkRequest vdoRq in vdos)
            {
                ProductStageVideo vdo = new ProductStageVideo();
                vdo.Pid = pid;
                vdo.Position = index++;
                vdo.VideoUrlEn = vdoRq.Url;
                vdo.Status = Constant.STATUS_ACTIVE;
                vdo.CreatedBy = email;
                vdo.CreatedDt = DateTime.Now;
                db.ProductStageVideos.Add(vdo);
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

        private bool ProductStageExists(int id)
        {
            return db.ProductStages.Count(e => e.ProductId == id) > 0;
        }

    }
}