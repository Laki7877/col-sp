using System.Linq;
using System.Web.Http;
using Colsp.Entity.Models;
//using System.Net.Http;
using System;
using System.Net;
using System.Data.Entity;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using System.Collections.Generic;
using System.Net.Http;

namespace Colsp.Api.Controllers
{
    public class ProductHistoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        [Route("api/ProductHistories/{historyId}")]
        [HttpGet]
        public HttpResponseMessage RestoreProductHistory(long historyId)
        {
            try
            {
                var tmpProduct = db.ProductHistoryGroups.Where(w => w.HistoryId == historyId)
                    .Include(i => i.ProductHistories.Select(s => s.ProductHistoryAttributes.Select(sv=>sv.Attribute)))
                    .Include(i => i.ProductHistories.Select(s => s.ProductHistoryImages))
                    .Include(i => i.ProductHistories.Select(s => s.ProductHistoryVideos))
                    .Include(i => i.ProductHistoryGlobalCatMaps)
                    .Include(i => i.ProductHistoryLocalCatMaps)
                    .Include(i => i.ProductHistoryTags);

                ProductHistoryGroup product = null;
                if (User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    product = tmpProduct.Where(w => w.ShopId == shopId).SingleOrDefault();
                }
                else
                {
                    product = tmpProduct.SingleOrDefault();
                }
                if (product == null)
                {
                    throw new Exception("Cannot find product " + historyId);
                }

                var historyList = db.ProductHistoryGroups
                        .Where(w => w.ProductId == product.ProductId)
                        .OrderByDescending(o => o.HistoryDt)
                        .Select(s => new ProductHistoryRequest()
                        {
                            ApproveOn = s.ApproveOn,
                            SubmitBy = s.SubmitBy,
                            HistoryId = s.HistoryId,
                            SubmitOn = s.SubmitOn
                        }).ToList();
                ProductStageRequest response = new ProductStageRequest();
                SetupResponse(product, response, historyList);
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private void SetupResponse(ProductHistoryGroup group, ProductStageRequest response, List<ProductHistoryRequest> historyList)
        {
            SetupGroupResponse(group, response);
            var masterVariant = group.ProductHistories.Where(w => w.IsVariant == false).FirstOrDefault();
            SetupVariantResponse(masterVariant, response.MasterVariant);
            SetupAttributeResponse(masterVariant, response.MasterAttribute);
            response.Visibility = masterVariant.Visibility;
            var variants = group.ProductHistories.Where(w => w.IsVariant == true).ToList();
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
            if (historyList != null)
            {
                response.Revisions = historyList;
            }

        }

        private void SetupGroupResponse(ProductHistoryGroup group, ProductStageRequest response)
        {
            response.ProductId = group.ProductId;
            response.ShopId = group.ShopId;
            response.MainGlobalCategory = new CategoryRequest() { CategoryId = group.GlobalCatId };
            if (group.LocalCatId != null)
            {
                response.MainLocalCategory = new CategoryRequest() { CategoryId = group.LocalCatId.Value };
            }
            //if (group.Brand != null)
            //{
            //    response.Brand = new BrandRequest() { BrandId = group.BrandId.HasValue ? group.BrandId.Value : 0, BrandNameEn = group.Brand.BrandNameEn };
            //}
            if (group.AttributeSetId != null)
            {
                response.AttributeSet = new AttributeSetRequest() { AttributeSetId = group.AttributeSetId.Value };
            }
            if (group.ProductHistoryGlobalCatMaps != null && group.ProductHistoryGlobalCatMaps.Count > 0)
            {
                foreach (var category in group.ProductHistoryGlobalCatMaps)
                {
                    response.GlobalCategories.Add(new CategoryRequest()
                    {
                        CategoryId = category.CategoryId,
                        //NameEn = category.GlobalCategory.NameEn,
                        //NameTh = category.GlobalCategory.NameTh
                    });
                }
            }
            if (group.ProductHistoryLocalCatMaps != null && group.ProductHistoryLocalCatMaps.Count > 0)
            {
                foreach (var category in group.ProductHistoryLocalCatMaps)
                {
                    response.LocalCategories.Add(new CategoryRequest()
                    {
                        CategoryId = category.CategoryId,
                        //NameEn = category.LocalCategory.NameEn,
                        //NameTh = category.LocalCategory.NameTh
                    });
                }
            }
            //if (group.ProductStageRelateds1 != null && group.ProductStageRelateds1.Count > 0)
            //{
            //    foreach (var pro in group.ProductStageRelateds1)
            //    {
            //        response.RelatedProducts.Add(new VariantRequest()
            //        {
            //            ProductId = pro.ProductStageGroup1.ProductId,
            //            ProductNameEn = pro.ProductStageGroup1.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().ProductNameEn,
            //        });
            //    }
            //}
            if (group.ProductHistoryTags != null && group.ProductHistoryTags.Count > 0)
            {
                response.Tags = group.ProductHistoryTags.Select(s => s.Tag).ToList();
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

        private void SetupVariantResponse(ProductHistory variant, VariantRequest response)
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
            //response.Quantity = variant.Inventory.Quantity;
            //response.SafetyStock = variant.Inventory.SafetyStockSeller;
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
            response.DefaultVariant = variant.DefaultVaraint;
            response.PromotionPrice = variant.PromotionPrice;
            response.EffectiveDatePromotion = variant.EffectiveDatePromotion;
            response.ExpireDatePromotion = variant.ExpireDatePromotion;
            response.IsHasExpiryDate = variant.IsHasExpiryDate;
            //response.StockType = Constant.STOCK_TYPE.Where(w => w.Value.Equals(variant.Inventory.StockType)).SingleOrDefault().Key;
            response.Display = variant.Display;
            response.IsHasExpiryDate = variant.IsHasExpiryDate;
            response.IsVat = variant.IsVat;
            if (variant.ProductHistoryImages != null && variant.ProductHistoryImages.Count > 0)
            {
                variant.ProductHistoryImages = variant.ProductHistoryImages.OrderBy(o => o.Position).ToList();
                foreach (var image in variant.ProductHistoryImages)
                {
                    response.Images.Add(new ImageRequest()
                    {
                        ImageId = image.ImageId,
                        Url = image.ImageUrlEn,
                        Position = image.Position
                    });
                }
            }
            if (variant.ProductHistoryVideos != null && variant.ProductHistoryVideos.Count > 0)
            {
                variant.ProductHistoryVideos = variant.ProductHistoryVideos.OrderBy(o => o.Position).ToList();
                foreach (var video in variant.ProductHistoryVideos)
                {
                    response.VideoLinks.Add(new VideoLinkRequest()
                    {
                        VideoId = video.VideoId,
                        Url = video.VideoUrlEn
                    });
                }
            }
        }

        private void SetupAttributeResponse(ProductHistory variant, List<AttributeRequest> attributeList)
        {
            foreach (var attribute in variant.ProductHistoryAttributes)
            {
                if (attributeList.Where(w => w.AttributeId == attribute.AttributeId).SingleOrDefault() != null)
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
                            CheckboxValue = attribute.CheckboxValue
                        });
                    }
                }
                else if (Constant.DATA_TYPE_CHECKBOX.Equals(attribute.Attribute.DataType))
                {
                    var tmpAttributeList = variant.ProductHistoryAttributes.Where(w => w.AttributeId == attribute.AttributeId).ToList();
                    foreach (var attr in tmpAttributeList)
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
                                    CheckboxValue = attr.CheckboxValue
                                });
                            }
                        }
                    }

                }
                else
                {
                    tmpAttribute.ValueEn = attribute.ValueEn;
                }
                attributeList.Add(tmpAttribute);
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

        private bool ProductHistoryExists(long id)
        {
            return db.ProductHistories.Count(e => e.HistoryId == id) > 0;
        }
    }
}