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

namespace Colsp.Api.Controllers
{
    public class ProductStagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        private readonly string root = HttpContext.Current.Server.MapPath("~/Excel");

        [Route("api/ProductStages/All")]
        [HttpGet]
        public HttpResponseMessage GetProductAllStages([FromUri] ProductRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var products = (from stage in db.ProductStages
                          join proImg in db.ProductStageImages on stage.Pid equals proImg.Pid into proImgJoin
                          join variant in db.ProductStageVariants on stage.ProductId equals variant.ProductId into varJoin
                          from varJ in varJoin.DefaultIfEmpty()
                          join varImg in db.ProductStageImages on varJ.Pid equals varImg.Pid into varImgJoin
                          where stage.ShopId == shopId 
                          select new
                          {
                              stage.ProductId,
                              Sku = varJ != null ? varJ.Sku : stage.Sku,
                              Upc = varJ != null ? varJ.Upc : stage.Upc,
                              ProductNameEn = varJ != null ? varJ.ProductNameEn : stage.ProductNameEn,
                              ProductNameTh = varJ != null ? varJ.ProductNameTh : stage.ProductNameTh,
                              Pid = varJ != null ? varJ.Pid : stage.Pid,
                              VariantValue = varJ != null ? varJ.ValueEn1 + " , " + varJ.ValueEn2 : null,
                              Status = varJ != null ? varJ.Status : stage.Status,
                              MasterImg = proImgJoin.Select(s=>new { s.ImageId, s.ImageUrlEn,s.Path,s.Position }).OrderBy(o=>o.Position),
                              VariantImg = varImgJoin.Select(s => new { s.ImageId, s.ImageUrlEn,s.Path,s.Position }).OrderBy(o => o.Position),
                              IsVariant = varJ != null ? true : false
                          });
                if(request == null)
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

        [Route("api/ProductStages/Excel/Upload")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadExcelFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("Content Multimedia");
                }
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);
                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    //fileName = fileData.LocalFileName;
                    //string tmp = fileData.Headers.ContentDisposition.FileName;
                    //if (tmp.StartsWith("\"") && tmp.EndsWith("\""))
                    //{
                    //    tmp = tmp.Trim('"');
                    //}
                    //ext = Path.GetExtension(tmp);
                    //break;
                }
                throw new Exception("todo");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages")]
        [HttpGet]
        public HttpResponseMessage GetProductStages([FromUri] ProductRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var products = (from p in db.ProductStages
                          where !Constant.STATUS_REMOVE.Equals(p.Status) && p.ShopId == shopId
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
                              VariantCount = p.ProductStageVariants.Count,
                              ImageUrl = p.FeatureImgUrl,
                              p.GlobalCatId,
                              p.LocalCatId,
                              p.AttributeSetId,
                              p.ProductStageAttributes,
                              p.UpdatedDt,
                          }).Take(100);
                if(request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, products);
                }
                request.DefaultOnNull();
                if(request.GlobalCatId != null)
                {
                    products = products.Where(p => p.GlobalCatId == request.GlobalCatId);
                }
                if(request.LocalCatId != null)
                {
                    products = products.Where(p => p.LocalCatId == request.LocalCatId);
                }
                if(request.AttributeSetId != null)
                {
                    products = products.Where(p => p.LocalCatId == request.LocalCatId);
                }
                if(request.AttributeId != null)
                {
                    products = products.Where(p=>p.ProductStageAttributes.All(a=>a.AttributeId==request.AttributeId));
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
                var total = products.Count();
                var pagedProducts = products.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProducts, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch(Exception e)
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
                var response = SetupProductStageRequestFromProductId(db,productId);
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

                #region Setup Master Product
                stage = new ProductStage();

                SetupProductStage(db,stage, request);
                stage.Status = Constant.PRODUCT_STATUS_JUNK;
                stage.ShopId = this.User.ShopRequest().ShopId.Value;
                string masterPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                stage.Pid = masterPid;

                stage.OnlineFlag = false;
                stage.Visibility = true;
                stage.CreatedBy = this.User.UserRequest().Email;
                stage.CreatedDt = DateTime.Now;
                stage.UpdatedBy = this.User.UserRequest().Email;
                stage.UpdatedDt = DateTime.Now;
                db.ProductStages.Add(stage);
                #endregion
                db.SaveChanges();
                stage.Status = request.Status;

                #region Setup Master Attribute
                if (request.MasterAttribute != null)
                {
                    SetupAttributeEntity(db, request.MasterAttribute, stage.ProductId, masterPid, this.User.UserRequest().Email);
                }
                #endregion
                #region Setup Inventory
                Inventory masterInventory = new Inventory();
                masterInventory.Quantity = request.MasterVariant.Quantity;
                masterInventory.SaftyStockSeller = request.MasterVariant.SafetyStock;
                if (request.MasterVariant.StockType != null)
                {
                    if (Constant.STOCK_TYPE.ContainsKey(request.MasterVariant.StockType))
                    {
                        masterInventory.StockAvailable = Constant.STOCK_TYPE[request.MasterVariant.StockType];
                    }
                }
                masterInventory.Pid = masterPid;
                masterInventory.CreatedBy = this.User.UserRequest().Email;
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
                masterInventoryHist.CreatedBy = this.User.UserRequest().Email;
                masterInventoryHist.CreatedDt = DateTime.Now;
                db.InventoryHistories.Add(masterInventoryHist);
                #endregion
                stage.FeatureImgUrl = SetupImgEntity(db, request.MasterVariant.Images, masterPid, this.User.UserRequest().Email);
                stage.ImageFlag = stage.FeatureImgUrl == null ? false : true;
                SetupImg360Entity(db, request.MasterVariant.Images360, masterPid, this.User.UserRequest().Email);
                SetupVdoEntity(db, request.MasterVariant.VideoLinks, masterPid, this.User.UserRequest().Email);
                #region Setup Related GlobalCategories
                if (request.GlobalCategories != null)
                {
                    foreach (CategoryRequest cat in request.GlobalCategories)
                    {
                        if (cat == null) { continue; }
                        ProductStageGlobalCatMap map = new ProductStageGlobalCatMap();
                        map.CategoryId = cat.CategoryId;
                        map.ProductId = stage.ProductId;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        db.ProductStageGlobalCatMaps.Add(map);
                    }
                }
                #endregion
                #region Setup Related LocalCategories
                if (request.LocalCategories != null)
                {
                    foreach (CategoryRequest cat in request.LocalCategories)
                    {
                        if (cat == null) { continue; }
                        ProductStageLocalCatMap map = new ProductStageLocalCatMap();
                        map.CategoryId = cat.CategoryId;
                        map.ProductId = stage.ProductId;
                        map.Status = Constant.STATUS_ACTIVE;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
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
                        relate.CreatedBy = this.User.UserRequest().Email;
                        relate.CreatedDt = DateTime.Now;
                        relate.UpdatedBy = this.User.UserRequest().Email;
                        relate.UpdatedDt = DateTime.Now;
                        db.ProductStageRelateds.Add(relate);
                    }
                }
                #endregion
                #region Setup variant
                if(request.Variants != null && request.Variants.Count > 0)
                {
                    foreach (VariantRequest variantRq in request.Variants)
                    {
                        if (variantRq.FirstAttribute == null &&
                            variantRq.SecondAttribute == null)
                        {
                            throw new Exception("Invalid variant format");
                        }

                        ProductStageVariant variant = new ProductStageVariant();
                        string variantPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                        variant.Pid = variantPid;
                        variant.ProductId = stage.ProductId;
                        variant.Status = request.Status;
                        if (variantRq.FirstAttribute != null && variantRq.FirstAttribute.AttributeId != null)
                        {
                            variant.Attribute1Id = variantRq.FirstAttribute.AttributeId;
                            variant.ValueEn1 = variantRq.FirstAttribute.ValueEn;
                        }
                        if (variantRq.SecondAttribute != null && variantRq.SecondAttribute.AttributeId != null)
                        {
                            variant.Attribute2Id = variantRq.SecondAttribute.AttributeId;
                            variant.ValueEn2 = variantRq.SecondAttribute.ValueEn;
                        }


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
                        variantInventoryHist.CreatedBy = this.User.UserRequest().Email;
                        variantInventoryHist.CreatedDt = DateTime.Now;
                        db.InventoryHistories.Add(masterInventoryHist);
                        #endregion

                        SetupImgEntity(db, variantRq.Images, variantPid, this.User.UserRequest().Email);
                        SetupVdoEntity(db, variantRq.VideoLinks, variantPid, this.User.UserRequest().Email);
                        SetupProductStageVariant(variant, variantRq);
                        variant.ShopId = stage.ShopId;
                        variant.CreatedBy = this.User.UserRequest().Email;
                        variant.CreatedDt = DateTime.Now;
                        variant.UpdatedBy = this.User.UserRequest().Email;
                        variant.UpdatedDt = DateTime.Now;
                        db.ProductStageVariants.Add(variant);
                    }
                }
               
                #endregion
                db.SaveChanges();
                return GetProductStage(stage.ProductId);
            }
            catch (Exception ex)
            {
                #region Rollback
                db.Dispose();
                if(stage != null && stage.ProductId > 0)
                {
                    db = new ColspEntities();
                    db.ProductStageAttributes.RemoveRange(db.ProductStageAttributes.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStageGlobalCatMaps.RemoveRange(db.ProductStageGlobalCatMaps.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStageLocalCatMaps.RemoveRange(db.ProductStageLocalCatMaps.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStageVariants.RemoveRange(db.ProductStageVariants.Where(w => w.ProductId.Equals(stage.ProductId)));
                    db.ProductStages.RemoveRange(db.ProductStages.Where(w => w.ProductId.Equals(stage.ProductId)));
                    try
                    {
                        db.SaveChanges();
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
                int shopId = this.User.ShopRequest().ShopId.Value;
                if (shopId == 0)
                {
                    throw new Exception("Shop is invalid. Cannot find shop in session");
                }
                var stage = db.ProductStages.Where(w => w.ProductId == productId && w.ShopId == shopId)
                    .Include(i => i.ProductStageVariants)
                    .Include(i => i.ProductStageAttributes).SingleOrDefault();
                if (stage != null)
                {
                    if (stage.Status == null || !stage.Status.Equals(Constant.PRODUCT_STATUS_DRAFT))
                    {
                        throw new Exception("Product is not allow");
                    }
                    #region Setup Master
                    SetupProductStage(db,stage, request);
                    if (string.IsNullOrEmpty(request.Status))
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Status is required");
                    }
                    stage.Status = request.Status;
                    stage.UpdatedBy = this.User.UserRequest().Email;
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
                                    current.UpdatedBy = this.User.UserRequest().Email;
                                    current.UpdatedDt = DateTime.Now;
                                    attrList.Remove(current);
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
                                attriEntity.Pid = stage.Pid;
                                attriEntity.ValueEn = attr.ValueEn;
                                attriEntity.Status = Constant.STATUS_ACTIVE;
                                attriEntity.CreatedBy = this.User.UserRequest().Email;
                                attriEntity.CreatedDt = DateTime.Now;
                                attriEntity.UpdatedBy = this.User.UserRequest().Email;
                                attriEntity.UpdatedDt = DateTime.Now;
                                db.ProductStageAttributes.Add(attriEntity);
                            }

                        }
                    }

                    if (attrList != null && attrList.Count > 0)
                    {
                        db.ProductStageAttributes.RemoveRange(attrList);
                    }
                    #endregion
                    SaveChangeInventory(db, stage.Pid, request.MasterVariant, this.User.UserRequest().Email);
                    SaveChangeInventoryHistory(db, stage.Pid, request.MasterVariant, this.User.UserRequest().Email);
                    stage.FeatureImgUrl = SaveChangeImg(db, stage.Pid, request.MasterVariant.Images, this.User.UserRequest().Email);
                    stage.ImageFlag = stage.FeatureImgUrl == null ? false : true;
                    SaveChangeImg360(db, stage.Pid, request.MasterVariant.Images360, this.User.UserRequest().Email);
                    SaveChangeVideoLinks(db, stage.Pid, request.MasterVariant.VideoLinks, this.User.UserRequest().Email);
                    SaveChangeRelatedProduct(db, stage.Pid, request.RelatedProducts, this.User.UserRequest().Email);
                    SaveChangeGlobalCat(db, stage.ProductId, request.GlobalCategories, this.User.UserRequest().Email);
                    SaveChangeLocalCat(db, stage.ProductId, request.LocalCategories, this.User.UserRequest().Email);
                    #endregion
                    #region Setup Variant
                    List<ProductStageVariant> varList = null;
                    if (stage.ProductStageVariants != null && stage.ProductStageVariants.Count > 0)
                    {
                        varList = stage.ProductStageVariants.ToList();
                    }

                    foreach (VariantRequest var in request.Variants)
                    {
                        if (var.FirstAttribute == null &&
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

                            current.CreatedBy = this.User.UserRequest().Email;
                            current.CreatedDt = DateTime.Now;
                        }

                        if (var.FirstAttribute != null && var.FirstAttribute.AttributeId != null)
                        {
                            current.Attribute1Id = var.FirstAttribute.AttributeId;
                            current.ValueEn1 = var.FirstAttribute.ValueEn;
                        }
                        if (var.SecondAttribute != null && var.SecondAttribute.AttributeId != null)
                        {
                            current.Attribute2Id = var.SecondAttribute.AttributeId;
                            current.ValueEn2 = var.SecondAttribute.ValueEn;
                        }

                        SaveChangeInventory(db, current.Pid, var, this.User.UserRequest().Email);
                        SaveChangeInventoryHistory(db, current.Pid, var, this.User.UserRequest().Email);
                        SaveChangeImg(db, current.Pid, var.Images, this.User.UserRequest().Email);
                        SaveChangeVideoLinks(db, current.Pid, var.VideoLinks, this.User.UserRequest().Email);
                        current.Status = stage.Status;
                        SetupProductStageVariant(current, var);
                        current.UpdatedBy = this.User.UserRequest().Email;
                        current.UpdatedDt = DateTime.Now;
                        if (addNew)
                        {
                            db.ProductStageVariants.Add(current);
                        }
                    }
                    if (varList != null && varList.Count > 0)
                    {
                        db.ProductStageVariants.RemoveRange(varList);
                    }
                    #endregion
                    db.SaveChanges();
                    return GetProductStage(stage.ProductId);
                }
                else
                {
                    throw new Exception("Product is invalid");
                }
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
                var proList = db.ProductStages.ToList();
                foreach (ProductStageRequest proRq in request)
                {

                    var current = proList.Where(w => w.ProductId.Equals(proRq.ProductId)).SingleOrDefault();
                    if (current == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                    }
                    current.Visibility = proRq.Visibility.Value;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        //duplicate
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
                    if (!Constant.PRODUCT_STATUS_DRAFT.Equals(pro.Status))
                    {
                        throw new Exception("Cannot delete product that is not draft");
                    }
                    db.ProductStages.Remove(pro);
                    db.ProductStageImages.RemoveRange(db.ProductStageImages.Where(w => w.Pid.Equals(pro.Pid)));
                    db.ProductStageImage360.RemoveRange(db.ProductStageImage360.Where(w => w.Pid.Equals(pro.Pid)));
                    db.ProductStageVideos.RemoveRange(db.ProductStageVideos.Where(w => w.Pid.Equals(pro.Pid)));
                    db.Inventories.RemoveRange(db.Inventories.Where(w => w.Pid.Equals(pro.Pid)));
                }
                db.SaveChanges();
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
                foreach (ProductStageRequest rq in request)
                {
                    if(rq.ProductId == null) { throw new Exception("Product Id cannot be null"); }
                    var stage = db.ProductStages.Find(rq.ProductId.Value);
                    if(stage == null)
                    {
                        throw new Exception("Cannot find Product with id " + rq.ProductId.Value );
                    }
                    if (!stage.Status.Equals(Constant.PRODUCT_STATUS_DRAFT))
                    {
                        throw new Exception("ProudctId " + rq.ProductId.Value + " is not drafted");
                    }
                    stage.Status = Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Published success");
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Export")]
        [HttpPost]
        public HttpResponseMessage ExportProduct(List<ProductStageRequest> request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                string header = "SKU*,PID,Group ID,Product Status,Product Name (English)*,Product Name (Thai)*,Global Category (ID)*,Local Category (ID),Attribute Set (ID),Product Variation 1 (Option),Product Variation 1 (Value),Product Variation 2 (Option),Product Variation 2 (Value),Brand Name (ID)*,Original Price*,Sale Price,Description (Thai)*,Description (English)*,Short Description (Thai),Short Description (English),Stock Type*,Preparation Time*,Package Dimension - Lenght (mm)*,Package Dimension - Height (mm)*,Package Dimension - Width (mm)*,Weight (g)*,Inventory Amount,Safety Stock Amount";
                writer.WriteLine(header);
                StringBuilder sb = null;
                foreach (ProductStageRequest rq in request)
                {
                    sb = new StringBuilder();
                    if (rq.ProductId == null) { throw new Exception("Product Id cannot be null"); }
                    var pro = db.ProductStages.Find(rq.ProductId.Value);
                    if(pro == null)
                    {
                        throw new Exception("Cannot find Product with id " + rq.ProductId.Value);
                    }
                    sb.Append(pro.Sku); sb.Append(",");
                    sb.Append(pro.Pid); sb.Append(",");
                    sb.Append("<Group ID>"); sb.Append(",");
                    sb.Append(pro.Status); sb.Append(",");
                    sb.Append(pro.ProductNameEn); sb.Append(",");
                    sb.Append(pro.ProductNameTh); sb.Append(",");
                    sb.Append(pro.GlobalCatId); sb.Append(",");
                    sb.Append(pro.LocalCatId); sb.Append(",");
                    sb.Append(pro.AttributeSetId); sb.Append(",");
                    sb.Append("Product Variation 1 (Option)"); sb.Append(",");
                    sb.Append("Product Variation 1 (Value)"); sb.Append(",");
                    sb.Append("Product Variation 2 (Option)"); sb.Append(",");
                    sb.Append("Product Variation 2 (Value)"); sb.Append(",");
                    sb.Append("Brand Name (ID)*"); sb.Append(",");
                    sb.Append(pro.OriginalPrice); sb.Append(",");
                    sb.Append(pro.SalePrice); sb.Append(",");
                    if (!string.IsNullOrEmpty(pro.DescriptionFullTh))
                    {
                        if (pro.DescriptionFullTh.Contains("\""))
                        {
                            pro.DescriptionFullTh = String.Format("\"{0}\"", pro.DescriptionFullTh.Replace("\"", "\"\""));
                        }
                        if (pro.DescriptionFullTh.Contains(","))
                        {
                            pro.DescriptionFullTh = String.Format("\"{0}\"", pro.DescriptionFullTh);
                        }
                        if (pro.DescriptionFullTh.Contains(System.Environment.NewLine))
                        {
                            pro.DescriptionFullTh = String.Format("\"{0}\"", pro.DescriptionFullTh);
                        }
                    }
                    if (!string.IsNullOrEmpty(pro.DescriptionFullEn))
                    {
                        if (pro.DescriptionFullEn.Contains("\""))
                        {
                            pro.DescriptionFullEn = String.Format("\"{0}\"", pro.DescriptionFullEn.Replace("\"", "\"\""));
                        }
                        if (pro.DescriptionFullEn.Contains(","))
                        {
                            pro.DescriptionFullEn = String.Format("\"{0}\"", pro.DescriptionFullEn);
                        }
                        if (pro.DescriptionFullEn.Contains(System.Environment.NewLine))
                        {
                            pro.DescriptionFullEn = String.Format("\"{0}\"", pro.DescriptionFullEn);
                        }
                    }
                    sb.Append("\"" + pro.DescriptionFullTh + "\""); sb.Append(",");
                    sb.Append("\"" + pro.DescriptionFullEn + "\""); sb.Append(",");
                    sb.Append(pro.DescriptionShortTh); sb.Append(",");
                    sb.Append(pro.DescriptionShortEn); sb.Append(",");
                    sb.Append("Stock Type*"); sb.Append(",");
                    sb.Append(pro.PrepareDay);  sb.Append(",");
                    sb.Append(pro.Length); sb.Append(",");
                    sb.Append(pro.Height); sb.Append(",");
                    sb.Append(pro.Width); sb.Append(",");
                    sb.Append(pro.Weight); sb.Append(",");
                    sb.Append("Inventory Amount"); sb.Append(",");
                    sb.Append("Safety Stock Amount");

                    writer.WriteLine(sb);
                }
                writer.Flush();
                stream.Position = 0;

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream") {
                    CharSet = Encoding.UTF8.WebName
                };
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = "file.csv";
                return result;
            }
            catch (Exception e)
            {
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
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e);
            }
        }

        private ProductStageRequest SetupProductStageRequestFromProductId(ColspEntities db, int productId)
        {
            int shopId = this.User.ShopRequest().ShopId.Value;
            var stage = db.ProductStages.Where(w => w.ProductId == productId && w.ShopId == shopId)
                    .Include(i => i.ProductStageVariants.Select(s => s.Attribute))
                    .Include(i => i.ProductStageVariants.Select(s => s.Attribute1))
                    .Include(i => i.ProductStageAttributes.Select(s => s.Attribute))
                    .Include(i => i.Brand).SingleOrDefault();

            if (stage != null)
            {
                ProductStageRequest response = new ProductStageRequest();
                response.MasterVariant.ProductNameTh = stage.ProductNameTh;
                response.MasterVariant.ProductNameEn = stage.ProductNameEn;
                response.MasterVariant.Sku = stage.Sku;
                response.MasterVariant.Upc = stage.Upc;
                response.Brand.BrandId = stage.BrandId;
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
                if (stage.ProductStageVariants != null)
                {
                    response.VariantCount = stage.ProductStageVariants.Count;
                }
                response.VariantCount = stage.ProductStageVariants.Count;
                response.MasterAttribute = SetupAttributeResponse(stage.ProductStageAttributes.ToList());
                #region Setup Inventory
                var inventory = (from inv in db.Inventories
                                 where inv.Pid.Equals(stage.Pid)
                                 select inv).SingleOrDefault();
                if (inventory != null)
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
                var relatedList = db.ProductStageRelateds.Join(db.ProductStages,
                    rel => rel.Pid2,
                    stg => stg.Pid,
                    (rel, stg) => new { RelatedProduct = rel, ProductStage = stg })
                    .Where(w => w.RelatedProduct.Pid1.Equals(stage.Pid)).ToList();
                if (relatedList != null && relatedList.Count > 0)
                {
                    List<VariantRequest> relate = new List<VariantRequest>();
                    foreach (var r in relatedList)
                    {
                        VariantRequest va = new VariantRequest();
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
                List<VariantRequest> varList = new List<VariantRequest>();
                foreach (ProductStageVariant variantEntity in stage.ProductStageVariants)
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
                    varient.Visibility = variantEntity.Visibility;
                    varList.Add(varient);
                }
                response.Variants = varList;
                return response;
            }
            else
            {
                throw new Exception("Product " + productId + " not found");
            }
        }

        private void SetupProductStageVariant(ProductStageVariant variant, VariantRequest variantRq)
        {
            variant.ProductNameTh = Validation.ValidateString(variantRq.ProductNameTh, "Variation Product Name (Thai)", true, 300, true);
            variant.ProductNameEn = Validation.ValidateString(variantRq.ProductNameEn, "Variation Product Name (English)", true, 300, true);
            variant.Sku = Validation.ValidateString(variantRq.Sku, "Variation SKU", false, 300, true);
            variant.Upc = Validation.ValidateString(variantRq.Upc, "Variation UPC", false, 300, true);
            variant.OriginalPrice = Validation.ValidateDecimal(variantRq.OriginalPrice, "Variation Original Price", true, 20, 2, true).Value;
            variant.SalePrice = Validation.ValidateDecimal(variantRq.SalePrice, "Variation Sale Price", false, 20, 2, true);
           
            variant.DescriptionFullTh = Validation.ValidateString(variantRq.DescriptionFullTh, "Variation Description (Thai)", false, 2000, false);
            variant.DescriptionShortTh = Validation.ValidateString(variantRq.DescriptionShortTh, "Variation Short Description (Thai)", false, 500, true);
            variant.DescriptionFullEn = Validation.ValidateString(variantRq.DescriptionFullEn, "Variation Description (English)", false, 2000, false);
            variant.DescriptionShortEn = Validation.ValidateString(variantRq.DescriptionShortEn, "Variation Short Description (English)", false, 500, true);
            if (Constant.PRODUCT_STATUS_DRAFT.Equals(variant.Status))
            {
                var tmp = Validation.ValidateDecimal(variantRq.Length, "Length", false, 11, 2, true);
                variant.Length = tmp != null ? tmp.Value : 0;
                tmp = Validation.ValidateDecimal(variantRq.Height, "Height", false, 11, 2, true);
                variant.Height = tmp != null ? tmp.Value : 0;
                tmp = Validation.ValidateDecimal(variantRq.Width, "Width", false, 11, 2, true);
                variant.Width = tmp != null ? tmp.Value : 0;
                tmp = Validation.ValidateDecimal(variantRq.Weight, "Weight", false, 11, 2, true);
                variant.Weight = tmp != null ? tmp.Value : 0;

            }
            else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(variant.Status))
            {
                if (variant.SalePrice != null
                    && variant.OriginalPrice < variant.SalePrice)
                {
                    throw new Exception("Variation Sale price must be lower than the original price");
                }
                variant.Length = Validation.ValidateDecimal(variantRq.Length, "Length", true, 11, 2, true).Value;
                variant.Height = Validation.ValidateDecimal(variantRq.Height, "Height", true,11, 2, true).Value;
                variant.Width = Validation.ValidateDecimal(variantRq.Width, "Width", true, 5, 11, true).Value;
                variant.Weight = Validation.ValidateDecimal(variantRq.Weight, "Weight", true, 11, 2, true).Value;
            }
            else
            {
                throw new Exception("Invalid status");
            }
            variant.DimensionUnit = variantRq.DimensionUnit;
            variant.WeightUnit = variantRq.WeightUnit;
            variant.DefaultVaraint = variantRq.DefaultVariant;
            variant.Visibility = variantRq.Visibility.Value;
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
            stage.OriginalPrice = Validation.ValidateDecimal(request.MasterVariant.OriginalPrice, "Original Price",true,20,2,true).Value;
            stage.SalePrice = Validation.ValidateDecimal(request.MasterVariant.SalePrice, "Sale Price", false, 20, 2, true);
            
            stage.DescriptionFullTh = Validation.ValidateString(request.MasterVariant.DescriptionFullTh, "Description (Thai)", false, 2000, false);
            stage.DescriptionShortTh = Validation.ValidateString(request.MasterVariant.DescriptionShortTh, "Short Description (Thai)", false, 500, true);
            stage.DescriptionFullEn = Validation.ValidateString(request.MasterVariant.DescriptionFullEn, "Description (English)", false, 2000, false);
            stage.DescriptionShortEn = Validation.ValidateString(request.MasterVariant.DescriptionShortEn, "Short Description (English)", false, 500, true);
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
                stage.Tag = Validation.ValidateString(request.Keywords, "Search Tag", false, 630,false);
                stage.MetaKey = Validation.ValidateString(request.SEO.MetaKeywords, "Meta Keywords", false, 630, false);
            }
            else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(request.Status))
            {
                if (stage.SalePrice != null
                    && stage.OriginalPrice < stage.SalePrice)
                {
                    throw new Exception("Sale price must be lower than the original price");
                }
                stage.PrepareDay = Validation.ValidateDecimal(request.PrepareDay, "Preparation Time", true, 5, 2, true).Value;
                stage.Length = Validation.ValidateDecimal(request.MasterVariant.Length, "Length", true, 11, 2, true).Value;
                stage.Height = Validation.ValidateDecimal(request.MasterVariant.Height, "Height", true, 11, 2, true).Value;
                stage.Width = Validation.ValidateDecimal(request.MasterVariant.Width, "Width", true, 11, 2, true).Value;
                stage.Weight = Validation.ValidateDecimal(request.MasterVariant.Weight, "Weight", true, 11, 2, true).Value;
                stage.Tag = Validation.ValidateTaging(request.Keywords, "Search Tag", false, false, 20, 30);
                stage.MetaKey = Validation.ValidateTaging(request.SEO.MetaKeywords, "Meta Keywords", false, false, 20, 30);
            }
            else
            {
                throw new Exception("Invalid status");
            }
            stage.DimensionUnit = request.MasterVariant.DimensionUnit;
            stage.WeightUnit = request.MasterVariant.WeightUnit;
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
            stage.MetaTitle = Validation.ValidateString(request.SEO.MetaTitle, "Meta Title", false, 60, false);
            stage.MetaDescription = Validation.ValidateString(request.SEO.MetaDescription, "Meta Description", false, 150, false);
            

            stage.UrlEn = request.SEO.ProductUrlKeyEn;
            stage.UrlTh = request.SEO.ProductUrlKeyTh;
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
            masterInventoryHist.UpdatedBy = email;
            masterInventoryHist.UpdatedDt = DateTime.Now;
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
                masterInventory.CreatedBy = email;
                masterInventory.CreatedDt = DateTime.Now;
                isNew = true;
            }
            masterInventory.UpdatedBy = email;
            masterInventory.UpdatedDt = DateTime.Now;
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
                            current.UpdatedBy = email;
                            current.UpdatedDt = DateTime.Now;
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
                        catEntity.CategoryId = cat.CategoryId;
                        catEntity.ProductId = ProductId;
                        catEntity.Status = Constant.STATUS_ACTIVE;
                        catEntity.CreatedBy = email;
                        catEntity.CreatedDt = DateTime.Now;
                        catEntity.UpdatedBy = email;
                        catEntity.UpdatedDt = DateTime.Now;
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
                            current.UpdatedBy = email;
                            current.UpdatedDt = DateTime.Now;
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
                        ProductStageGlobalCatMap catEntity = new ProductStageGlobalCatMap();
                        catEntity.CategoryId = cat.CategoryId;
                        catEntity.ProductId = ProductId;
                        catEntity.Status = Constant.STATUS_ACTIVE;
                        catEntity.CreatedBy = email;
                        catEntity.CreatedDt = DateTime.Now;
                        catEntity.UpdatedBy = email;
                        catEntity.UpdatedDt = DateTime.Now;
                        db.ProductStageGlobalCatMaps.Add(catEntity);
                    }
                }
            }
            if (catList != null && catList.Count > 0)
            {
                db.ProductStageGlobalCatMaps.RemoveRange(catList);
            }
        }

        private void SaveChangeRelatedProduct(ColspEntities db, string pid, List<VariantRequest> pidList, string email)
        {
            var relateList = db.ProductStageRelateds.Where(w => w.Pid1 == pid).ToList();
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
                            current.UpdatedBy = email;
                            current.UpdatedDt = DateTime.Now;
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
                        proEntity.Status = Constant.STATUS_ACTIVE;
                        proEntity.CreatedBy = email;
                        proEntity.CreatedDt = DateTime.Now;
                        proEntity.UpdatedBy = email;
                        proEntity.UpdatedDt = DateTime.Now;
                        db.ProductStageRelateds.Add(proEntity);
                    }
                }
            }
            if (relateList != null && relateList.Count > 0)
            {
                db.ProductStageRelateds.RemoveRange(relateList);
            }
        }

        private void SaveChangeVideoLinks(ColspEntities db, string pid, List<VideoLinkRequest> videoRequest, string email)
        {
            var vdoList = db.ProductStageVideos.Where(w => w.Pid.Equals(pid)).ToList();
            if (videoRequest != null)
            {
                int index = 0;
                bool addNew = false;
                foreach (VideoLinkRequest vdo in videoRequest)
                {
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
                        vdoEntity.Status = Constant.STATUS_ACTIVE;
                        vdoEntity.CreatedBy = email;
                        vdoEntity.CreatedDt = DateTime.Now;
                        vdoEntity.UpdatedBy = email;
                        vdoEntity.UpdatedDt = DateTime.Now;
                        db.ProductStageVideos.Add(vdoEntity);
                    }
                }
            }
            if (vdoList != null && vdoList.Count > 0)
            {
                db.ProductStageVideos.RemoveRange(vdoList);
            }
        }

        private void SaveChangeImg360(ColspEntities db, string pid, List<ImageRequest> img360Request, string email)
        {
            var img360List = db.ProductStageImage360.Where(w => w.Pid.Equals(pid)).ToList();
            if (img360Request != null)
            {
                int index = 0;
                bool addNew = false;
                foreach (ImageRequest img in img360Request)
                {
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
                            current.UpdatedBy = email;
                            current.UpdatedDt = DateTime.Now;
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
                        imgEntity.Status = Constant.STATUS_ACTIVE;
                        imgEntity.CreatedBy = email;
                        imgEntity.CreatedDt = DateTime.Now;
                        imgEntity.UpdatedBy = email;
                        imgEntity.UpdatedDt = DateTime.Now;
                        db.ProductStageImage360.Add(imgEntity);
                    }
                }
            }
            if (img360List != null && img360List.Count > 0)
            {
                db.ProductStageImage360.RemoveRange(img360List);
            }
        }

        private string SaveChangeImg(ColspEntities db, string pid, List<ImageRequest> imgRequest, string email)
        {
            
            var imgList = db.ProductStageImages.Where(w => w.Pid.Equals(pid)).ToList();
            bool featureImg = true;
            string featureImgUrl = null;
            if (imgRequest != null && imgRequest.Count > 0)
            {
                int index = 0;
                bool addNew = false;
                foreach (ImageRequest img in imgRequest)
                {
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
                            current.UpdatedBy = email;
                            current.UpdatedDt = DateTime.Now;
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
                        imgEntity.FeatureFlag = featureImg;
                        imgEntity.Position = index++;
                        imgEntity.Path = img.tmpPath;
                        imgEntity.ImageUrlEn = img.url;
                        imgEntity.Status = Constant.STATUS_ACTIVE;
                        imgEntity.CreatedBy = email;
                        imgEntity.CreatedDt = DateTime.Now;
                        imgEntity.UpdatedBy = email;
                        imgEntity.UpdatedDt = DateTime.Now;
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
                attriEntity.AttributeId = attr.AttributeId.Value;
                attriEntity.Pid = pid;
                attriEntity.ValueEn = attr.ValueEn;
                attriEntity.Status = Constant.STATUS_ACTIVE;
                attriEntity.CreatedBy = email;
                attriEntity.CreatedDt = DateTime.Now;
                attriEntity.UpdatedBy = email;
                attriEntity.UpdatedDt = DateTime.Now;
                db.ProductStageAttributes.Add(attriEntity);
            }

        }

        private string SetupImgEntity(ColspEntities db, List<ImageRequest> imgs, string pid, string email)
        {
            if (imgs == null || imgs.Count == 0) { return null; }
            string featureImgUrl = imgs[0].url;
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
                img.Status = Constant.STATUS_ACTIVE;
                img.CreatedBy = email;
                img.CreatedDt = DateTime.Now;
                img.UpdatedBy = email;
                img.UpdatedDt = DateTime.Now;
                db.ProductStageImages.Add(img);
            }
            return featureImgUrl;
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
                img.Status = Constant.STATUS_ACTIVE;
                img.CreatedBy = email;
                img.CreatedDt = DateTime.Now;
                img.UpdatedBy = email;
                img.UpdatedDt = DateTime.Now;
                db.ProductStageImage360.Add(img);
            }
        }

        private void SetupVdoEntity(ColspEntities db, List<VideoLinkRequest> vdos, string pid, string email)
        {
            if (vdos == null) { return; }
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
                vdo.UpdatedBy = email;
                vdo.UpdatedDt = DateTime.Now;
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