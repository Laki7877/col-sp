using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Model.Requests;
using Colsp.Api.Extensions;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;
using Colsp.Api.CMSFunction;
using Colsp.Api.ByOneGetOneFunction;
using Colsp.Model.Request;
using Colsp.Api.Helpers;
using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace Colsp.Api.Controllers
{
    public class BuyOneGetOneController : ApiController
    {
        private ColspEntities db = new ColspEntities();
     
        //CRUD Sequence

        #region Create By 1 Get 1
        [Route("api/ProBuy1Get1/Create")]
        [HttpPost]
        public HttpResponseMessage CreateBy1Get1Item(Buy1Get1ItemRequest model)
        {
            try
            {
                if (model != null)
                {
                    int Id = 0;

                    BuyOneGetOneProcess bg = new BuyOneGetOneProcess();
                    Id = bg.CreateBuy1Get1Item(model);
                    return GetBuy1Get1Item(Id);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot save data");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
        }
        #endregion
        
        #region Read By 1 Get 1
        [Route("api/ProBuy1Get1/List")]
        [HttpGet]
        public HttpResponseMessage ListBuy1Get1Item([FromUri] CMSShopRequest request)
        {           
            try
            {
                IQueryable<PromotionBuy1Get1Item> ProBuy1Get1;
               
                    ProBuy1Get1 = (from c in db.PromotionBuy1Get1Item
                           select c
                          );
              
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, ProBuy1Get1); 
                }

                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    ProBuy1Get1 = ProBuy1Get1.Where(c => (c.NameEN.Contains(request.SearchText) 
                      || c.NameTH.Contains(request.SearchText)  
                      || c.ShortDescriptionEN.Contains(request.SearchText) 
                      || c.ShortDescriptionTH.Contains(request.SearchText)
                      || c.LongDescriptionEN.Contains(request.SearchText)
                      || c.ProductBoxBadge.Contains(request.SearchText)
                    ));
                }

                if (!string.IsNullOrEmpty(request._filter))
                {
                    
                    if (string.Equals("Draft", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_DRAFT));
                    }
                    else if (string.Equals("Approved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_APPROVE));
                    }
                    else if (string.Equals("NotApproved", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_NOT_APPROVE));
                    }
                    else if (string.Equals("WaitforApproval", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        ProBuy1Get1 = ProBuy1Get1.Where(p => p.CMSStatusFlowId.Equals(Constant.CMS_STATUS_WAIT_FOR_APPROVAL));
                    }

                }

                var total = ProBuy1Get1.Count();
                var pagedProBuy1Get1 = ProBuy1Get1.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedProBuy1Get1, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, request);            
            }

        }

        [Route("api/ProBuy1Get1/{Id}")]
        [HttpGet]
        public HttpResponseMessage GetBuy1Get1Item(int? Id)
        {
            try
            {
                Buy1Get1ItemResponse response = new Buy1Get1ItemResponse();
                if (Id != null && Id.HasValue)
                {
                    if (Id == 0)
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Item id is invalid. Cannot find item in System");
                    using (ColspEntities db = new ColspEntities())
                    {
                        var B1G = db.PromotionBuy1Get1Item.Where(c => c.PromotionBuy1Get1ItemId == Id).FirstOrDefault();
                        if (B1G != null)
                        {
                            response.PromotionBuy1Get1ItemId = B1G.PromotionBuy1Get1ItemId;
                            response.NameEN = B1G.NameEN;
                            response.NameTH = B1G.NameTH;
                            response.URLKey = B1G.URLKey;
                            response.PIDBuy = B1G.PIDBuy;
                            response.PIDGet = B1G.PIDGet;
                            response.ShortDescriptionTH = B1G.ShortDescriptionTH;
                            response.LongDescriptionTH = B1G.LongDescriptionTH;
                            response.ShortDescriptionEN = B1G.ShortDescriptionEN;
                            response.LongDescriptionEN = B1G.LongDescriptionEN;
                            response.EffectiveDate = B1G.EffectiveDate;
                            response.EffectiveTime = B1G.EffectiveTime;
                            response.ExpiryDate = B1G.ExpiryDate;
                            response.ExpiryTime = B1G.ExpiryTime;
                            response.ProductBoxBadge = B1G.ProductBoxBadge;
                            response.Sequence = B1G.Sequence;
                            response.Status = B1G.Status;
                            response.CreateBy = B1G.CreateBy;
                            response.Createdate = (DateTime)B1G.Createdate;
                            response.UpdateBy = B1G.UpdateBy;
                            response.UpdateDate = (DateTime)B1G.UpdateDate;
                            response.CreateIP = B1G.CreateIP;
                            response.UpdateIP = B1G.UpdateIP;

                            return Request.CreateResponse(HttpStatusCode.OK, response);
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Cannot find item in System");
                        }
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
        
        #endregion

        #region Update & Delete By 1 Get 1
        //single object input
        [Route("api/ProBuy1Get1/UpdateItem")]
        [HttpPost]
        public HttpResponseMessage UpdateProBuy1Get1Item(Buy1Get1ItemRequest model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {                   
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.UpdateProBuy1Get1(model);                   
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Update Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }

        [Route("api/ProBuy1Get1/EditItem")]
        [HttpPost]
        public HttpResponseMessage EditProBuy1Get1Item(Buy1Get1ItemRequest model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {                   
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.EditProBuy1Get1(model);                   
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Edit Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }

        //listing object input
        [Route("api/ProBuy1Get1/UpdateList")]
        [HttpPost]
        public HttpResponseMessage UpdateProBuy1Get1List(List<Buy1Get1ItemRequest> model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {
                    foreach (var item in model)
                    {                       
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.UpdateProBuy1Get1(item);                       
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Update Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }

        [Route("api/ProBuy1Get1/EditList")]
        [HttpPost]
        public HttpResponseMessage EditProBuy1Get1List(List<Buy1Get1ItemRequest> model)
        {
            try
            {
                int Id = 0;
                if (model != null)
                {
                    foreach (var item in model)
                    {
                        BuyOneGetOneProcess cms = new BuyOneGetOneProcess();
                        Id = cms.EditProBuy1Get1(item);
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "");
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Edit Complete");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }

        }
        #endregion


        #region Duplicate & Import , Export

        [Route("api/ProBuy1Get1/Export")]
        [HttpPost]
        public HttpResponseMessage ExportProduct(ExportRequest request)
        {
            MemoryStream stream = null;
            StreamWriter writer = null;
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                #region Query
                var shopId = this.User.ShopRequest().ShopId.Value;
                var productList = (
                             from mast in db.ProductStages
                             join variant in db.ProductStageVariants on mast.ProductId equals variant.ProductId into varJoin
                             from vari in varJoin.DefaultIfEmpty()
                             where mast.ProductId == 4 || mast.ProductId == 2
                             select new
                             {
                                 Status = vari != null ? vari.Status : mast.Status,
                                 Sku = vari != null ? vari.Sku : mast.Sku,
                                 Pid = vari != null ? vari.Pid : mast.Pid,
                                 Upc = vari != null ? vari.Upc : mast.Upc,
                                 ProductId = vari != null ? vari.ProductId : mast.ProductId,
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
                             }).ToList();
                #endregion
                #region Initiate Header
                int i = 0;
                Dictionary<string, int> headDic = new Dictionary<string, int>();
                if (request.ProductStatus)
                {
                    headDic.Add("Product Status", i++);
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
                                    headDic.Add("Attribute Set", i++);
                                    headDic.Add("Variation Option 1", i++);
                                    headDic.Add("Variation Option 2", i++);
                                }
                                bodyList.Add(Validation.ValidateCSVColumn(p.AttributeSet.AttributeSetNameEn));
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
                                {
                                    bodyList.Add(Validation.ValidateCSVColumn(p.VariantAttribute.ToList()[0].AttributeNameEn));
                                    if (p.VariantAttribute.ToList().Count > 1)
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
                                if (p.MasterAttribute != null && p.MasterAttribute.ToList().Count > 0)
                                {
                                    foreach (var masterValue in p.MasterAttribute)
                                    {
                                        if (headDic.ContainsKey(masterValue.AttributeNameEn))
                                        {
                                            int desColumn = headDic[masterValue.AttributeNameEn];
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
                        writer.Write(body + ",");
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

        #endregion
    }
}