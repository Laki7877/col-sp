using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
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
                                                    Modified = p.UpdatedDt,
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
                            from var in db.ProductStageVariants.Where(w => w.ProductId.Equals(stg.ProductId)).DefaultIfEmpty()
                            where stg.ProductId.Equals(productId)
                            select new
                            {
                                stg,
                                var,
                                attrMap = from attr in db.ProductStageAttributes.Where(w => w.ProductId.Equals(stg.ProductId)).DefaultIfEmpty() select attr
                            }).AsEnumerable().Select(s=>s.stg).SingleOrDefault();
                if(stage != null)
                {
                    ProductStageRequest response = new ProductStageRequest();
                    response.DefaultVariant.ProductNameTh = stage.ProductNameTh;
                    response.DefaultVariant.ProductNameEn = stage.ProductNameEn;
                    response.DefaultVariant.Sku = stage.Sku;
                    response.Brand.BrandId = stage.BrandId ;
                    response.DefaultVariant.OriginalPrice = stage.OriginalPrice ;
                    response.DefaultVariant.SalePrice = stage.SalePrice ;
                    response.DefaultVariant.DescriptionFullTh = stage.DescriptionFullTh;
                    response.DefaultVariant.DescriptionShortTh = stage.DescriptionShortTh;
                    response.DefaultVariant.DescriptionFullEn = stage.DescriptionFullEn ;
                    response.DefaultVariant.DescriptionShortEn = stage.DescriptionShortEn ;
                    response.AttributeSet.AttributeSetId = stage.AttributeSetId = response.AttributeSet.AttributeSetId;
                    response.Keywords = stage.Tag;
                    response.ShippingMethod = stage.ShippingId;
                    response.PrepareDay = stage.PrepareDay;
                    response.DefaultVariant.Length = stage.Length;
                    response.DefaultVariant.Height = stage.Height;
                    response.DefaultVariant.Width = stage.Width;
                    response.DefaultVariant.Weight = stage.Weight;
                    response.DefaultVariant.DimensionUnit = stage.DimensionUnit;
                    response.DefaultVariant.WeightUnit = stage.WeightUnit;
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
                    response.DefaultVariant.Pid = stage.Pid;

                    response.MasterAttribute = SetupAttributeResponse(stage.ProductStageAttributes.ToList());
                    #region Setup Inventory
                    var inventory = (from inv in db.Inventories
                                     where inv.Pid.Equals(stage.Pid)
                                     select inv).SingleOrDefault();
                    if(inventory != null)
                    {
                        response.DefaultVariant.SafetyStock = inventory.SaftyStockSeller;
                        response.DefaultVariant.Quantity = inventory.Quantity;
                        response.DefaultVariant.StockType = Constant.STOCK_TYPE.Where(w => w.Value.Equals(inventory.StockAvailable)).SingleOrDefault().Key;
                    }
                    #endregion
                    response.DefaultVariant.Images = SetupImgResponse(db, stage.Pid);
                    response.DefaultVariant.Images360 = SetupImg360Response(db, stage.Pid);
                    response.DefaultVariant.VideoLinks = SetupVdoResponse(db, stage.Pid);
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
                        varient.Pid = variantEntity.Pid;
                        varient.FirstAttribute.AttributeId = variantEntity.Attribute1Id;
                        varient.SecondAttribute.AttributeId = variantEntity.Attribute1Id;
                        varient.ValueEn = variantEntity.ValueEn;

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
                #region Setup Master Product
                stage = new ProductStage();
                stage.ProductNameTh = request.DefaultVariant.ProductNameTh;
                stage.ProductNameEn = request.DefaultVariant.ProductNameEn;
                stage.Sku = request.DefaultVariant.Sku;
                stage.BrandId = request.Brand.BrandId;
                stage.OriginalPrice = request.DefaultVariant.OriginalPrice;
                stage.SalePrice = request.DefaultVariant.SalePrice;
                stage.DescriptionFullTh = request.DefaultVariant.DescriptionFullTh;
                stage.DescriptionShortTh = request.DefaultVariant.DescriptionShortTh;
                stage.DescriptionFullEn = request.DefaultVariant.DescriptionFullEn;
                stage.DescriptionShortEn = request.DefaultVariant.DescriptionShortEn;
                stage.AttributeSetId = request.AttributeSet.AttributeSetId;
                stage.Tag = request.Keywords;
                stage.ShippingId = request.ShippingMethod;
                stage.PrepareDay = request.PrepareDay;
                stage.Length = request.DefaultVariant.Length;
                stage.Height = request.DefaultVariant.Height;
                stage.Width = request.DefaultVariant.Width;
                stage.Weight = request.DefaultVariant.Weight;
                stage.DimensionUnit = request.DefaultVariant.DimensionUnit;
                stage.WeightUnit = request.DefaultVariant.WeightUnit;
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
                #endregion
                stage.Remark = request.Remark;
                stage.Status = request.Status;
                stage.SellerId = request.SellerId;
                stage.ShopId = request.ShopId;
                string masterPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                stage.Pid = masterPid;
                stage.CreatedBy = this.User.Email();
                stage.CreatedDt = DateTime.Now;
                #region Master Validation
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
                if (string.IsNullOrEmpty(stage.Status))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Status is required");
                }
                if (stage.GlobalCatId == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Global category is required");
                }
                if (stage.Status.Equals(Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL))
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
                #endregion
                db.ProductStages.Add(stage);
                db.SaveChanges();

                #region Setup Attribute
                if(request.MasterAttribute != null)
                {
                   SetupAttributeEntity(db, request.MasterAttribute, stage.ProductId, masterPid, this.User.Email());
                }
                #endregion
                #region Setup Inventory
                Inventory masterInventory = new Inventory();
                masterInventory.StockAvailable = request.DefaultVariant.Quantity;
                masterInventory.SaftyStockSeller = request.DefaultVariant.SafetyStock;
                if(request.DefaultVariant.StockType != null)
                {
                    if (Constant.STOCK_TYPE.ContainsKey(request.DefaultVariant.StockType))
                    {
                        masterInventory.StockAvailable = Constant.STOCK_TYPE[request.DefaultVariant.StockType];
                    }
                }
               
                masterInventory.Pid = masterPid;
                db.Inventories.Add(masterInventory);
                #endregion
                SetupImgEntity(db, request.DefaultVariant.Images, masterPid,this.User.Email());
                SetupImg360Entity(db, request.DefaultVariant.Images360, masterPid, this.User.Email());
                SetupVdoEntity(db, request.DefaultVariant.VideoLinks, masterPid, this.User.Email());
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
                #endregion
                #region Setup variant
                foreach (VariantRequest variantRq in request.Variants)
                {
                    if (variantRq.FirstAttribute == null ||
                        variantRq.SecondAttribute == null)
                    {
                        throw new Exception("Invalid variant format");
                    }

                    ProductStageVariant varient = new ProductStageVariant();
                    string variantPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                    varient.Pid = variantPid;
                    varient.ProductId = stage.ProductId;
                    varient.Attribute1Id = variantRq.FirstAttribute.AttributeId;
                    varient.Attribute2Id = variantRq.SecondAttribute.AttributeId;
                    varient.ValueEn = variantRq.ValueEn;

                    #region Setup Variant Inventory
                    Inventory variantInventory = new Inventory();
                    variantInventory.StockAvailable = variantRq.Quantity;
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
                    #endregion
                    
                    SetupImgEntity(db, variantRq.Images, variantPid,this.User.Email());
                    SetupVdoEntity(db, variantRq.VideoLinks, variantPid, this.User.Email());
                    
                    varient.ProductNameTh = variantRq.ProductNameTh;
                    varient.ProductNameEn = variantRq.ProductNameEn;
                    varient.Sku = variantRq.Sku;
                    varient.OriginalPrice = variantRq.OriginalPrice;
                    varient.SalePrice = variantRq.SalePrice;
                    varient.DescriptionFullTh = variantRq.DescriptionFullTh;
                    varient.DescriptionShortTh = variantRq.DescriptionShortTh;
                    varient.DescriptionFullEn = variantRq.DescriptionFullEn;
                    varient.DescriptionShortEn = variantRq.DescriptionShortEn;
                    varient.Length = variantRq.Length;
                    varient.Height = variantRq.Height;
                    varient.Width = variantRq.Width;
                    varient.Weight = variantRq.Weight;
                    varient.DimensionUnit = variantRq.DimensionUnit;
                    varient.WeightUnit = variantRq.WeightUnit;
                    varient.CreatedBy = this.User.Email();
                    varient.CreatedDt = DateTime.Now;

                    db.ProductStageVariants.Add(varient);
                }
                #endregion

                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
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
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, wee);
                }
                #endregion
      
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
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
                attriEntity.AttributeId = attr.AttributeId;
                attriEntity.Pid = pid;
                attriEntity.ValueEn = attr.ValueEn;
                attriEntity.Status = Constant.STATUS_ACTIVE;
                attriEntity.CreatedBy = email;
                attriEntity.CreatedDt = DateTime.Now;
                db.ProductStageAttributes.Add(attriEntity);
            }
           
        }

        private void SetupImgEntity(ColspEntities db,List<ImageRequest> imgs,string pid,string email)
        {
            if(imgs == null) { return; }
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