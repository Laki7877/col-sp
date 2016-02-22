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
using Colsp.Api.Filters;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Colsp.Api.Controllers
{
    public class ProductStagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        private readonly string root = HttpContext.Current.Server.MapPath("~/Excel");

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
                var products = (from p in db.ProductStages
                                where !Constant.STATUS_REMOVE.Equals(p.Status)
                                select new
                                {
                                    p.Sku,
                                    p.Pid,
                                    p.Upc,
                                    p.ProductId,
                                    p.ProductNameEn,
                                    p.ProductNameTh,
                                    p.SalePrice,
                                    p.OriginalPrice,
                                    p.Shop.ShopNameEn,
                                    p.Status,
                                    p.ImageFlag,
                                    p.InfoFlag,
                                    p.Visibility,
                                    VariantCount = p.ProductStageVariants.Where(w => w.Visibility == true).ToList().Count,
                                    ImageUrl = p.FeatureImgUrl,
                                    GlobalCategory = p.GlobalCategory != null ? new { p.GlobalCategory.CategoryId, p.GlobalCategory.NameEn,p.GlobalCategory.Lft,p.GlobalCategory.Rgt } : null,
                                    LocalCategory = p.LocalCategory != null ? new { p.LocalCategory.CategoryId, p.LocalCategory.NameEn, p.LocalCategory.Lft,p.LocalCategory.Rgt } : null,
                                    Brand = p.Brand != null ? new { p.Brand.BrandId,p.Brand.BrandNameEn } : null,
                                    p.Tag,
                                    p.CreatedDt,
                                    p.UpdatedDt,
                                    Shop = new { p.Shop.ShopId, p.Shop.ShopNameEn }
                                });
                //check if its seller permission
                if (this.User.HasPermission("View Product"))
                {
                    //add shopid criteria for seller request
                    int shopId = this.User.ShopRequest().ShopId.Value;
                    products = products.Where(w => w.Shop.ShopId == shopId);
                }
                //set request default value
                request.DefaultOnNull();
                //add ProductName criteria
                if(request.ProductNames != null && request.ProductNames.Count > 0)
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
                    List<int> brandIds = request.Brands.Where(w => w.BrandId != null).Select(s => s.BrandId.Value).ToList();
                    if (brandIds != null && brandIds.Count > 0)
                    {
                        products = products.Where(w => brandIds.Contains(w.Brand.BrandId));
                    }
                    //if request send brand name, add brand name criteria
                    List<string> brandNames = request.Brands.Where(w => w.BrandNameEn != null).Select(s => s.BrandNameEn).ToList();
                    if (brandNames != null && brandNames.Count > 0)
                    {
                        products = products.Where(w => brandNames.Any(a => w.Brand.BrandNameEn.Contains(a)));
                    }
                }
                //add Global category criteria
                if (request.GlobalCategories != null && request.GlobalCategories.Count > 0)
                {
                    //if request send category parent left and right, add category parent left and right criteria
                    var lft = request.GlobalCategories.Where(w=>w.Lft!=null).Select(s => s.Lft).ToList();
                    var rgt = request.GlobalCategories.Where(w => w.Rgt != null).Select(s => s.Rgt).ToList();
                    if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
                    {
                        products = products.Where(w => lft.Any(a => a <= w.GlobalCategory.Lft) && rgt.Any(a=>a >= w.GlobalCategory.Rgt));
                    }
                    //if request send category name, add category category name criteria
                    List<string> catNames = request.GlobalCategories.Where(w => w.NameEn != null).Select(s => s.NameEn).ToList();
                    if (catNames != null && catNames.Count > 0)
                    {
                        products = products.Where(w => catNames.Any(a => w.GlobalCategory.NameEn.Contains(a)));
                    }
                }
                //add Local category criteria
                if (request.LocalCategories != null && request.LocalCategories.Count > 0)
                {
                    //if request send category parent left and right, add category parent left and right criteria
                    var lft = request.LocalCategories.Where(w => w.Lft != null).Select(s => s.Lft).ToList();
                    var rgt = request.LocalCategories.Where(w => w.Rgt != null).Select(s => s.Rgt).ToList();

                    if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
                    {
                        products = products.Where(w => lft.Any(a => a <= w.LocalCategory.Lft) && rgt.Any(a => a >= w.LocalCategory.Rgt));
                    }
                    //if request send category name, add category category name criteria
                    List<string> catNames = request.LocalCategories.Where(w => w.NameEn != null).Select(s => s.NameEn).ToList();
                    if (catNames != null && catNames.Count > 0)
                    {
                        products = products.Where(w => catNames.Any(a => w.LocalCategory.NameEn.Contains(a)));
                    }
                }
                //add Tag criteria
                if (request.Tags != null && request.Tags.Count > 0)
                {
                    products = products.Where(w => request.Tags.Any(a => w.Tag.Contains(a)));
                }
                //add sale price(from) criteria
                if (request.PriceFrom != null)
                {
                    products = products.Where(w => w.SalePrice >= request.PriceFrom);
                }
                //add sale price(to) criteria
                if (request.PriceTo != null)
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
                    || p.Upc.Contains(request.SearchText));
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
                                let comm = db.ProductStageComments.Where(w => w.Pid.Equals(stage.Pid)).OrderByDescending(o => o.UpdatedDt).FirstOrDefault()
                                let commVar = db.ProductStageComments.Where(w => w.Pid.Equals(varJ.Pid)).OrderByDescending(o => o.UpdatedDt).FirstOrDefault()
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
                foreach (VariantRequest varRq in request)
                {
                    if (varRq.IsVariant == null)
                    {
                        throw new Exception("Invalid variant flag");
                    }
                    if (varRq.IsVariant.Value)
                    {
                        SaveChangeImg(db, varRq.Pid, shopId, varRq.VariantImg, this.User.UserRequest().Email);
                    }
                    else
                    {
                        SaveChangeImg(db, varRq.Pid, shopId, varRq.MasterImg, this.User.UserRequest().Email);
                    }

                }
                db.SaveChanges();
                return Request.CreateErrorResponse(HttpStatusCode.OK, "Save successful");
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
                                    p.UpdatedDt,
                                    p.ShopId,
                                    p.InformationTabStatus,
                                    p.ImageTabStatus,
                                    p.CategoryTabStatus,
                                    p.VariantTabStatus,
                                    p.MoreOptionTabStatus,
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
                masterInventory.Quantity = Validation.ValidationInteger(request.MasterVariant.Quantity, "Quantity", true, Int32.MaxValue, 0).Value;
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
                        if (cat == null || cat.CategoryId == null) { continue; }
                        ProductStageLocalCatMap map = new ProductStageLocalCatMap();
                        map.CategoryId = cat.CategoryId.Value;
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
                        relate.ShopId = shopId;
                        relate.CreatedBy = this.User.UserRequest().Email;
                        relate.CreatedDt = DateTime.Now;
                        relate.UpdatedBy = this.User.UserRequest().Email;
                        relate.UpdatedDt = DateTime.Now;
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
                        string variantPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                        variant.Pid = variantPid;
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
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
                                    });
                                }
                            }else if (!string.IsNullOrWhiteSpace(variantRq.FirstAttribute.ValueEn))
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
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
                                });
                            }
                            else
                            {
                                throw new Exception("Invalid variant value");
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
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
                                    });
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(variantRq.SecondAttribute.ValueEn))
                            {
                                if (rg.IsMatch(variantRq.FirstAttribute.ValueEn))
                                {
                                    throw new Exception("Attribute value not allow");
                                }
                                variant.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                {
                                    VariantId = variant.VariantId,
                                    AttributeId = variantRq.SecondAttribute.AttributeId.Value,
                                    Value = variantRq.SecondAttribute.ValueEn,
                                    IsAttributeValue = false,
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
                                });
                            }
                            else
                            {
                                throw new Exception("Invalid variant value");
                            }
                            
                        }


                        #region Setup Variant Inventory
                        Inventory variantInventory = new Inventory();
                        variantInventory.Quantity = Validation.ValidationInteger(variantRq.Quantity, "Quantity", true, Int32.MaxValue, 0).Value;
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

                        SetupImgEntity(db, variantRq.Images, variantPid, shopId, this.User.UserRequest().Email);
                        SetupVdoEntity(db, variantRq.VideoLinks, variantPid, shopId, this.User.UserRequest().Email);
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
                    .Include(i => i.ProductStageVariants.Select(s=>s.ProductStageVariantArrtibuteMaps))
                    .Include(i => i.ProductStageAttributes).SingleOrDefault();
                if (stage != null)
                {
                    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
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
                                    if (attr.AttributeValues != null && attr.AttributeValues.Count > 0)
                                    {
                                        foreach (AttributeValueRequest val in attr.AttributeValues)
                                        {
                                            current.ValueEn = string.Concat("((", val.AttributeValueId, "))");
                                            current.IsAttributeValue = true;
                                            break;
                                        }
                                    }
                                    else if (!string.IsNullOrWhiteSpace(attr.ValueEn))
                                    {
                                        current.ValueEn = attr.ValueEn;
                                        current.IsAttributeValue = false;
                                    }
                                    else
                                    {
                                        throw new Exception("Invalid attribute value");
                                    }
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
                    stage.FeatureImgUrl = SaveChangeImg(db, stage.Pid, shopId, request.MasterVariant.Images, this.User.UserRequest().Email);
                    stage.ImageFlag = stage.FeatureImgUrl == null ? false : true;
                    SaveChangeImg360(db, stage.Pid, shopId, request.MasterVariant.Images360, this.User.UserRequest().Email);
                    SaveChangeVideoLinks(db, stage.Pid, shopId, request.MasterVariant.VideoLinks, this.User.UserRequest().Email);
                    SaveChangeRelatedProduct(db, stage.Pid, shopId, request.RelatedProducts, this.User.UserRequest().Email);
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
                            current.ShopId = shopId;
                            current.CreatedBy = this.User.UserRequest().Email;
                            current.CreatedDt = DateTime.Now;
                        }
                        List<ProductStageVariantArrtibuteMap> valList = null;
                        if(current.ProductStageVariantArrtibuteMaps != null && current.ProductStageVariantArrtibuteMaps.Count > 0)
                        {
                            valList = current.ProductStageVariantArrtibuteMaps.ToList();
                        }
                        if (var.FirstAttribute != null && var.FirstAttribute.AttributeId != null)
                        {
                            if (var.FirstAttribute.AttributeValues != null && var.FirstAttribute.AttributeValues.Count > 0)
                            {
                                bool isTmpNew = false;
                                foreach (AttributeValueRequest val in var.FirstAttribute.AttributeValues)
                                {
                                    if(valList == null || valList.Count == 0)
                                    {
                                        isTmpNew = true;
                                    }
                                    if (!isTmpNew)
                                    {
                                        var currentVal =  valList.Where(w => w.AttributeId==var.FirstAttribute.AttributeId && w.Value.Equals(string.Concat("((",val.AttributeValueId,"))"))).SingleOrDefault();
                                        if(currentVal != null)
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
                                            AttributeId = var.FirstAttribute.AttributeId.Value,
                                            Value = string.Concat("((", val.AttributeValueId, "))"),
                                            IsAttributeValue = true,
                                            CreatedBy = this.User.UserRequest().Email,
                                            CreatedDt = DateTime.Now,
                                            UpdatedBy = this.User.UserRequest().Email,
                                            UpdatedDt = DateTime.Now
                                        });
                                    }
                                    
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(var.FirstAttribute.ValueEn))
                            {
                                var currentVal = valList.Where(w => w.AttributeId == var.FirstAttribute.AttributeId).SingleOrDefault();
                                if (currentVal != null)
                                {
                                    if (rg.IsMatch(var.FirstAttribute.ValueEn))
                                    {
                                        throw new Exception("Attribute value not allow");
                                    }
                                    currentVal.Value = var.FirstAttribute.ValueEn;
                                    currentVal.UpdatedBy = this.User.UserRequest().Email;
                                    currentVal.UpdatedDt = DateTime.Now;
                                }
                                else
                                {
                                    if (rg.IsMatch(var.FirstAttribute.ValueEn))
                                    {
                                        throw new Exception("Attribute value not allow");
                                    }
                                    current.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                    {
                                        VariantId = current.VariantId,
                                        AttributeId = var.FirstAttribute.AttributeId.Value,
                                        Value = var.FirstAttribute.ValueEn,
                                        IsAttributeValue = false,
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
                                    });
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid variant value");
                            }
                        }
                        if (var.SecondAttribute != null && var.SecondAttribute.AttributeId != null)
                        {

                            if (var.SecondAttribute.AttributeValues != null && var.SecondAttribute.AttributeValues.Count > 0)
                            {
                                bool isTmpNew = false;
                                foreach (AttributeValueRequest val in var.SecondAttribute.AttributeValues)
                                {
                                    if (valList == null || valList.Count == 0)
                                    {
                                        isTmpNew = true;
                                    }
                                    if (!isTmpNew)
                                    {
                                        var currentVal = valList.Where(w => w.AttributeId == var.SecondAttribute.AttributeId && w.Value.Equals(string.Concat("((", val.AttributeValueId, "))"))).SingleOrDefault();
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
                                            AttributeId = var.SecondAttribute.AttributeId.Value,
                                            Value = string.Concat("((", val.AttributeValueId, "))"),
                                            IsAttributeValue = true,
                                            CreatedBy = this.User.UserRequest().Email,
                                            CreatedDt = DateTime.Now,
                                            UpdatedBy = this.User.UserRequest().Email,
                                            UpdatedDt = DateTime.Now
                                        });
                                    }

                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(var.SecondAttribute.ValueEn))
                            {
                                var currentVal = valList.Where(w => w.AttributeId == var.SecondAttribute.AttributeId).SingleOrDefault();
                                if (currentVal != null)
                                {
                                    if (rg.IsMatch(var.SecondAttribute.ValueEn))
                                    {
                                        throw new Exception("Attribute value not allow");
                                    }
                                    currentVal.Value = var.SecondAttribute.ValueEn;
                                    currentVal.IsAttributeValue = false;
                                    currentVal.UpdatedBy = this.User.UserRequest().Email;
                                    currentVal.UpdatedDt = DateTime.Now;
                                }
                                else
                                {
                                    if (rg.IsMatch(var.SecondAttribute.ValueEn))
                                    {
                                        throw new Exception("Attribute value not allow");
                                    }
                                    current.ProductStageVariantArrtibuteMaps.Add(new ProductStageVariantArrtibuteMap()
                                    {
                                        VariantId = current.VariantId,
                                        AttributeId = var.SecondAttribute.AttributeId.Value,
                                        Value = var.SecondAttribute.ValueEn,
                                        IsAttributeValue = false,
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
                                    });
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid variant value");
                            }
                        }
                        if(valList != null && valList.Count > 0)
                        {
                            db.ProductStageVariantArrtibuteMaps.RemoveRange(valList);
                        }

                        SaveChangeInventory(db, current.Pid, var, this.User.UserRequest().Email);
                        SaveChangeInventoryHistory(db, current.Pid, var, this.User.UserRequest().Email);
                        SaveChangeImg(db, current.Pid, shopId, var.Images, this.User.UserRequest().Email);
                        SaveChangeVideoLinks(db, current.Pid, shopId, var.VideoLinks, this.User.UserRequest().Email);
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
                    current.UpdatedBy = this.User.UserRequest().Email;
                    current.UpdatedDt = DateTime.Now;
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
                    current.UpdatedBy = this.User.UserRequest().Email;
                    current.UpdatedDt = DateTime.Now;
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
                var shopId = this.User.ShopRequest().ShopId.Value;
                var productList = (
                             from mast in db.ProductStages
                             join variant in db.ProductStageVariants on mast.ProductId equals variant.ProductId into varJoin
                             from vari in varJoin.DefaultIfEmpty()
                             where  mast.ProductId == 4 || mast.ProductId == 2
                             select new
                             {
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
                                 mast.Brand.BrandNameEn,
                                 mast.GlobalCatId,
                                 mast.LocalCatId,
                                 OriginalPrice = vari != null ? vari.OriginalPrice : mast.OriginalPrice,
                                 SalePrice = vari != null ? vari.SalePrice : mast.SalePrice,
                                 DescriptionShortEn = vari != null ? vari.DescriptionShortEn:mast.DescriptionShortEn,
                                 DescriptionShortTh = vari != null ? vari.DescriptionShortTh : mast.DescriptionShortTh,
                                 DescriptionFullEn = vari != null ? vari.DescriptionFullEn : mast.DescriptionFullEn,
                                 DescriptionFullTh = vari != null ? vari.DescriptionFullTh : mast.DescriptionFullTh,
                                 AttributeSet = new { mast.AttributeSetId, mast.AttributeSet.AttributeSetNameEn, Attribute = mast.AttributeSet.AttributeSetMaps.Select(s=>s.Attribute) },
                                 mast.PrepareDay,
                                 Length = vari != null ? vari.Length:mast.Length,
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
                                         :s.Value,
                                 }),
                                 MasterAttribute = mast.ProductStageAttributes.Select(s=> new
                                 {
                                     s.AttributeId,
                                     s.Attribute.AttributeNameEn,
                                     ValueEn =  s.IsAttributeValue ?  
                                                (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault() 
                                                : s.ValueEn,
                                 }),
                                 RelatedProduct = (from rel in db.ProductStageRelateds where rel.Pid1.Equals(mast.Pid) select rel.Pid2).ToList(),
                                 Inventory = vari != null ? (from inv in db.Inventories where inv.Pid.Equals(vari.Pid) select inv).FirstOrDefault() :
                                              (from inv in db.Inventories where inv.Pid.Equals(mast.Pid) select inv).FirstOrDefault(),
                             }).ToList();
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
                if (request.GroupNameEn)
                {
                    headDic.Add("Group Name (English)", i++);
                }
                if (request.GroupNameTh)
                {
                    headDic.Add("Group Name (Thai)", i++);
                }
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
                            bodyList.Add(Validation.ValidaetCSVColumn("Draft"));
                        }
                        else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(p.Status))
                        {
                            bodyList.Add(Validation.ValidaetCSVColumn("Wait for Approval"));
                        }
                        else if (Constant.PRODUCT_STATUS_APPROVE.Equals(p.Status))
                        {
                            bodyList.Add(Validation.ValidaetCSVColumn("Approve"));
                        }
                        else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(p.Status))
                        {
                            bodyList.Add(Validation.ValidaetCSVColumn("Not Approve"));
                        }
                    }
                    if (request.SKU)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Sku));
                    }
                    if (request.PID)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Pid));
                    }
                    if (request.UPC)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Upc));
                    }
                    if (request.GroupID)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.ProductId));
                    }
                    if (request.GroupNameEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.GroupNameEn));
                    }
                    if (request.GroupNameTh)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.GroupNameTh));
                    }
                    if (request.DefaultVariant)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.DefaultVaraint));
                    }
                    if (request.ProductNameEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.ProductNameEn));
                    }
                    if (request.ProductNameTh)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.ProductNameTh));
                    }
                    if (request.BrandName)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.BrandNameEn));
                    }
                    if (request.GlobalCategory)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.GlobalCatId));
                    }
                    if (request.LocalCategory)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.LocalCatId));
                    }
                    if (request.OriginalPrice)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.OriginalPrice));
                    }
                    if (request.SalePrice)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.SalePrice));
                    }
                    if (request.DescriptionEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.DescriptionFullEn));
                    }
                    if (request.DescriptionTh)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.DescriptionFullTh));
                    }
                    if (request.ShortDescriptionEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.DescriptionShortEn));
                    }
                    if (request.ShortDescriptionTh)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.DescriptionShortTh));
                    }
                    if (request.PreparationTime)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.PrepareDay));
                    }
                    if (request.PackageLenght)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Length));
                    }
                    if (request.PackageHeight)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Height));
                    }
                    if (request.PackageWidth)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Width));
                    }
                    if (request.PackageWeight)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Weight));
                    }

                    if (request.InventoryAmount)
                    {
                        if (p.Inventory != null)
                        {
                            bodyList.Add(Validation.ValidaetCSVColumn(p.Inventory.Quantity));
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
                            bodyList.Add(Validation.ValidaetCSVColumn(p.Inventory.SaftyStockSeller));
                        }
                        else
                        {
                            bodyList.Add(string.Empty);
                        }
                    }
                    if (request.SearchTag)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Tag));
                    }
                    if (request.RelatedProducts)
                    {
                        if (p.RelatedProduct != null && p.RelatedProduct.Count > 0)
                        {
                            bodyList.Add(Validation.ValidaetCSVColumn(string.Join(",", p.RelatedProduct)));
                        }
                        else
                        {
                            bodyList.Add(string.Empty);
                        }
                    }
                    if (request.MetaTitleEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.MetaTitleEn));
                    }
                    if (request.MetaTitleTh)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.MetaTitleTh));
                    }
                    if (request.MetaDescriptionEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.MetaDescriptionEn));
                    }
                    if (request.MetaDescriptionTh)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.MetaDescriptionTh));
                    }
                    if (request.MetaKeywordEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.MetaKeyEn));
                    }
                    if (request.MetaKeywordTh)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.MetaKeyTh));
                    }
                    if (request.ProductURLKeyEn)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.UrlEn));
                    }
                    if (request.ProductBoostingWeight)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.BoostWeight));
                    }
                    if (request.EffectiveDate)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.EffectiveDate));
                    }
                    if (request.EffectiveTime)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.EffectiveTime));
                    }

                    if (request.ExpiryDate)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.ExpiryDate));
                    }
                    if (request.ExpiryTime)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.ExpiryTime));
                    }
                    if (request.Remark)
                    {
                        bodyList.Add(Validation.ValidaetCSVColumn(p.Remark));
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
                                    headDic.Add(" Variation Option 1", i++);
                                    headDic.Add(" Variation Option 2", i++);
                                }
                                bodyList.Add(Validation.ValidaetCSVColumn(p.AttributeSet.AttributeSetNameEn));
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
                                {
                                    bodyList.Add(Validation.ValidaetCSVColumn(p.VariantAttribute.ToList()[0].AttributeNameEn));
                                    if(p.VariantAttribute.ToList().Count > 1)
                                    {
                                        bodyList.Add(Validation.ValidaetCSVColumn(p.VariantAttribute.ToList()[1].AttributeNameEn));
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
                string headers = string.Empty;
                foreach (KeyValuePair<string, int> entry in headDic)
                {
                    headers += string.Concat(entry.Key, ",");
                }
                stream = new MemoryStream();
                writer = new StreamWriter(stream);
                writer.WriteLine(headers);
                #endregion
                #region Write body
                foreach (List<string> r in rs)
                {
                    foreach (string body in r)
                    {
                        writer.Write(body+",");
                    }
                    writer.WriteLine();
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

        private ProductStageRequest SetupProductStageRequestFromProductId(ColspEntities db, int productId)
        {
            int shopId = this.User.ShopRequest().ShopId.Value;
            var stage = db.ProductStages.Where(w => w.ProductId == productId && w.ShopId == shopId)
                    .Include(i => i.ProductStageAttributes.Select(s => s.Attribute))
                    .Include(i => i.Brand)
                    .Include(i => i.ProductStageVariants.Select(s=>s.ProductStageVariantArrtibuteMaps)).SingleOrDefault();

            if (stage != null)
            {
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
                response.MasterVariant.Length = stage.Length;
                response.MasterVariant.Height = stage.Height;
                response.MasterVariant.Width = stage.Width;
                response.MasterVariant.Weight = stage.Weight;
                response.MasterVariant.DimensionUnit = stage.DimensionUnit;
                response.MasterVariant.WeightUnit = stage.WeightUnit;
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

                    if(variantEntity.ProductStageVariantArrtibuteMaps != null && variantEntity.ProductStageVariantArrtibuteMaps.Count > 0)
                    {
                        var joinList = variantEntity.ProductStageVariantArrtibuteMaps
                            .GroupJoin(db.AttributeValues, p => p.Value, v => v.MapValue, 
                                (varAttrMap, attrValue) => new { varAttrMap, attrValue }).ToList();
                        if(joinList != null && joinList.Count > 0)
                        {
                            varient.FirstAttribute.AttributeId = joinList[0].varAttrMap.AttributeId;
                            if (joinList[0].attrValue != null && joinList[0].attrValue.ToList().Count > 0)
                            {
                                foreach(var val in joinList[0].attrValue)
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
                            
                            if(joinList.Count > 1)
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

                    //varient.FirstAttribute.AttributeId = variantEntity.Attribute1Id;
                    //varient.FirstAttribute.ValueEn = variantEntity.ValueEn1;
                    //varient.SecondAttribute.AttributeId = variantEntity.Attribute2Id;
                    //varient.SecondAttribute.ValueEn = variantEntity.ValueEn2;
                    varient.DefaultVariant = variantEntity.DefaultVaraint;
                    varient.Display = variantEntity.Display;
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
            variant.Display = Validation.ValidateString(variantRq.Display, "Display", false, 20, true);
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
            decimal? op = Validation.ValidateDecimal(request.MasterVariant.OriginalPrice, "Original Price", false, 20, 2, true);
            if(op != null)
            {
                stage.OriginalPrice = op.Value;
            }
            else
            {
                stage.OriginalPrice = 0;
            }
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
                stage.MetaKeyEn = Validation.ValidateString(request.SEO.MetaKeywordEn, "Meta Keywords (English)", false, 630, false);
                stage.MetaKeyTh = Validation.ValidateString(request.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, 630, false);
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
                stage.MetaKeyEn = Validation.ValidateTaging(request.SEO.MetaKeywordEn, "Meta Keywords (English)", false, false, 20, 30);
                stage.MetaKeyTh = Validation.ValidateTaging(request.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, false, 20, 30);
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
            masterInventory.Quantity = Validation.ValidationInteger(variant.Quantity, "Quantity", true, Int32.MaxValue, 0).Value;
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
                        catEntity.CategoryId = cat.CategoryId.Value;
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
                        if (cat == null || cat.CategoryId == null) { continue; }
                        ProductStageGlobalCatMap catEntity = new ProductStageGlobalCatMap();
                        catEntity.CategoryId = cat.CategoryId.Value;
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
                        proEntity.ShopId = shopId;
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

        private void SaveChangeVideoLinks(ColspEntities db, string pid, int shopId, List<VideoLinkRequest> videoRequest, string email)
        {
            var vdoList = db.ProductStageVideos.Where(w => w.Pid.Equals(pid) && w.ShopId == shopId).ToList();
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
                        vdoEntity.ShopId = shopId;
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

        private void SaveChangeImg360(ColspEntities db, string pid, int shopId, List<ImageRequest> img360Request, string email)
        {
            var img360List = db.ProductStageImage360.Where(w => w.Pid.Equals(pid) && w.ShopId == shopId).ToList();
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
                        imgEntity.ShopId = shopId;
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

        private string SaveChangeImg(ColspEntities db, string pid, int shopId, List<ImageRequest> imgRequest, string email)
        {
            
            var imgList = db.ProductStageImages.Where(w => w.Pid.Equals(pid) && w.ShopId == shopId).ToList();
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
                        imgEntity.ShopId = shopId;
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
                    throw new Exception("Invalid attribute value");
                }


                attriEntity.AttributeId = attr.AttributeId.Value;
                attriEntity.Status = Constant.STATUS_ACTIVE;
                attriEntity.CreatedBy = email;
                attriEntity.CreatedDt = DateTime.Now;
                attriEntity.UpdatedBy = email;
                attriEntity.UpdatedDt = DateTime.Now;
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
                img.CreatedBy = email;
                img.CreatedDt = DateTime.Now;
                img.UpdatedBy = email;
                img.UpdatedDt = DateTime.Now;
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
                img.CreatedBy = email;
                img.CreatedDt = DateTime.Now;
                img.UpdatedBy = email;
                img.UpdatedDt = DateTime.Now;
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