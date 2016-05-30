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
                ProductStageRequest response = GetProductHistoryRequestFromId(db, historyId);
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private ProductStageRequest GetProductHistoryRequestFromId(ColspEntities db, long historyId)
        {
            #region Query
            var tmpPro = db.ProductHistoryGroups.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status) && w.HistoryId == historyId)
                .Select(s => new
                {
                    s.ProductId,
                    Shop = new
                    {
                        s.ShopId,
                        s.Shop.ShopNameEn
                    },
                    MainGlobalCategory = !s.GlobalCatId.HasValue ? null : new
                    {
                        s.GlobalCatId,
                        s.GlobalCategory.NameEn,
                    },
                    MainLocaCategory = !s.LocalCatId.HasValue ? null : new
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
                    ProductRelated = s.ProductHistoryRelateds.Select(r => new
                    {
                        Child = r.ProductStageGroup.ProductStages.Where(w => w.IsVariant == false && !Constant.STATUS_REMOVE.Equals(w.Status)).Select(sm => new
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
                    GlobalCategories = s.ProductHistoryGlobalCatMaps.Select(sg => new
                    {
                        sg.CategoryId,
                        sg.GlobalCategory.NameEn
                    }),
                    LocalCategories = s.ProductHistoryLocalCatMaps.Select(sl => new
                    {
                        sl.CategoryId,
                        sl.LocalCategory.NameEn
                    }),
                    Tags = s.ProductHistoryTags.Select(st => st.Tag),
                    ProductHistories = s.ProductHistories.Select(st => new
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
                        ProductStageAttributes = st.ProductHistoryAttributes.Select(sa => new
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
                        Images = st.ProductHistoryImages.OrderBy(o => o.SeqNo).Select(si => new
                        {
                            si.ImageId,
                            si.ImageName
                        }),
                        Videos = st.ProductHistoryVideos.OrderBy(o => o.Position).Select(sv => new
                        {
                            sv.VideoUrlEn
                        }),
                        //Inventory = st.Inventory == null ? null : new
                        //{
                        //    st.Inventory.OnHold,
                        //    st.Inventory.Reserve,
                        //    st.Inventory.Quantity,
                        //    st.Inventory.SafetyStockSeller,
                        //    st.Inventory.StockType,
                        //    st.Inventory.MaxQtyAllowInCart,
                        //    st.Inventory.MaxQtyPreOrder,
                        //    st.Inventory.MinQtyAllowInCart,
                        //    st.Inventory.Defect
                        //}
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
                    CategoryId = tmpPro.MainGlobalCategory != null ? tmpPro.MainGlobalCategory.GlobalCatId.Value : 0,
                    NameEn = tmpPro.MainGlobalCategory != null ? tmpPro.MainGlobalCategory.NameEn : string.Empty,
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
                foreach (var child in related.Child)
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
            var masterVariant = tmpPro.ProductHistories.Where(w => w.IsVariant == false).SingleOrDefault();
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
                Length = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length / 1000) : masterVariant.Length,
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
                //OnHold = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.OnHold,
                //Quantity = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Quantity,
                //Defect = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Defect,
                //Reserve = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Reserve,
                //SafetyStock = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.SafetyStockSeller,
                //StockType = masterVariant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : masterVariant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
                //MaxQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyAllowInCart,
                //MinQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MinQtyAllowInCart,
                //MaxQtyPreOrder = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyPreOrder,
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
                        Url = string.Concat(Constant.IMAGE_STATIC_URL, image.ImageName),
                        ImageId = image.ImageId,
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
            var variants = tmpPro.ProductHistories.Where(w => w.IsVariant == true && w.IsMaster == false);
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
                    //OnHold = variant.Inventory == null ? 0 : variant.Inventory.OnHold,
                    //Quantity = variant.Inventory == null ? 0 : variant.Inventory.Quantity,
                    //Reserve = variant.Inventory == null ? 0 : variant.Inventory.Reserve,
                    //Defect = variant.Inventory == null ? 0 : variant.Inventory.Defect,
                    //SafetyStock = variant.Inventory == null ? 0 : variant.Inventory.SafetyStockSeller,
                    //StockType = variant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : variant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
                    //MaxQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyAllowInCart,
                    //MinQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MinQtyAllowInCart,
                    //MaxQtyPreOrder = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyPreOrder,
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
                            ImageId = image.ImageId,
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

        //private void SetupResponse(ProductHistoryGroup group, ProductStageRequest response, List<ProductHistoryRequest> historyList)
        //{
        //    SetupGroupResponse(group, response);
        //    var masterVariant = group.ProductHistories.Where(w => w.IsVariant == false).FirstOrDefault();
        //    SetupVariantResponse(masterVariant, response.MasterVariant);
        //    SetupAttributeResponse(masterVariant, response.MasterAttribute);
        //    response.Visibility = masterVariant.Visibility;
        //    var variants = group.ProductHistories.Where(w => w.IsVariant == true).ToList();
        //    foreach (var variant in variants)
        //    {
        //        var tmpVariant = new VariantRequest();
        //        SetupVariantResponse(variant, tmpVariant);
        //        List<AttributeRequest> tmpAttributeList = new List<AttributeRequest>();
        //        SetupAttributeResponse(variant, tmpAttributeList);
        //        if (tmpAttributeList != null && tmpAttributeList.Count > 0)
        //        {
        //            tmpVariant.FirstAttribute = tmpAttributeList.ElementAt(0);
        //            if (tmpAttributeList.Count > 1)
        //            {
        //                tmpVariant.SecondAttribute = tmpAttributeList.ElementAt(1);
        //            }
        //        }
        //        response.Variants.Add(tmpVariant);
        //    }
        //    if (historyList != null)
        //    {
        //        response.Revisions = historyList;
        //    }

        //}

        //private void SetupGroupResponse(ProductHistoryGroup group, ProductStageRequest response)
        //{
        //    response.ProductId = group.ProductId;
        //    response.ShopId = group.ShopId;
        //    response.MainGlobalCategory = new CategoryRequest() { CategoryId = group.GlobalCatId };
        //    if (group.LocalCatId != null)
        //    {
        //        response.MainLocalCategory = new CategoryRequest() { CategoryId = group.LocalCatId.Value };
        //    }
        //    //if (group.Brand != null)
        //    //{
        //    //    response.Brand = new BrandRequest() { BrandId = group.BrandId.HasValue ? group.BrandId.Value : 0, BrandNameEn = group.Brand.BrandNameEn };
        //    //}
        //    if (group.AttributeSetId != null)
        //    {
        //        response.AttributeSet = new AttributeSetRequest() { AttributeSetId = group.AttributeSetId.Value };
        //    }
        //    if (group.ProductHistoryGlobalCatMaps != null && group.ProductHistoryGlobalCatMaps.Count > 0)
        //    {
        //        foreach (var category in group.ProductHistoryGlobalCatMaps)
        //        {
        //            response.GlobalCategories.Add(new CategoryRequest()
        //            {
        //                CategoryId = category.CategoryId,
        //                //NameEn = category.GlobalCategory.NameEn,
        //                //NameTh = category.GlobalCategory.NameTh
        //            });
        //        }
        //    }
        //    if (group.ProductHistoryLocalCatMaps != null && group.ProductHistoryLocalCatMaps.Count > 0)
        //    {
        //        foreach (var category in group.ProductHistoryLocalCatMaps)
        //        {
        //            response.LocalCategories.Add(new CategoryRequest()
        //            {
        //                CategoryId = category.CategoryId,
        //                //NameEn = category.LocalCategory.NameEn,
        //                //NameTh = category.LocalCategory.NameTh
        //            });
        //        }
        //    }
        //    //if (group.ProductStageRelateds1 != null && group.ProductStageRelateds1.Count > 0)
        //    //{
        //    //    foreach (var pro in group.ProductStageRelateds1)
        //    //    {
        //    //        response.RelatedProducts.Add(new VariantRequest()
        //    //        {
        //    //            ProductId = pro.ProductStageGroup1.ProductId,
        //    //            ProductNameEn = pro.ProductStageGroup1.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault().ProductNameEn,
        //    //        });
        //    //    }
        //    //}
        //    if (group.ProductHistoryTags != null && group.ProductHistoryTags.Count > 0)
        //    {
        //        response.Tags = group.ProductHistoryTags.Select(s => s.Tag).ToList();
        //    }
        //    response.ShippingMethod = group.ShippingId;
        //    response.EffectiveDate = group.EffectiveDate;
        //    response.ExpireDate = group.ExpireDate;
        //    response.ControlFlags.IsNew = group.IsNew;
        //    response.ControlFlags.IsBestSeller = group.IsBestSeller;
        //    response.ControlFlags.IsClearance = group.IsClearance;
        //    response.ControlFlags.IsOnlineExclusive = group.IsOnlineExclusive;
        //    response.ControlFlags.IsOnlyAt = group.IsOnlyAt;
        //    response.TheOneCardEarn = group.TheOneCardEarn;
        //    response.GiftWrap = group.GiftWrap;
        //    response.Remark = group.Remark;
        //    response.AdminApprove.Information = group.InformationTabStatus;
        //    response.AdminApprove.Image = group.ImageTabStatus;
        //    response.AdminApprove.Category = group.CategoryTabStatus;
        //    response.AdminApprove.Variation = group.VariantTabStatus;
        //    response.AdminApprove.MoreOption = group.MoreOptionTabStatus;
        //    response.AdminApprove.RejectReason = group.RejectReason;
        //    response.InfoFlag = group.InfoFlag;
        //    response.ImageFlag = group.ImageFlag;
        //    response.OnlineFlag = group.OnlineFlag;
        //    response.Status = group.Status;
        //}

        //private void SetupVariantResponse(ProductHistory variant, VariantRequest response)
        //{
        //    response.Pid = variant.Pid;
        //    response.ShopId = variant.ShopId;
        //    response.ProductNameTh = variant.ProductNameTh;
        //    response.ProductNameEn = variant.ProductNameEn;
        //    response.ProdTDNameEn = variant.ProdTDNameEn;
        //    response.ProdTDNameTh = variant.ProdTDNameTh;
        //    response.Sku = variant.Sku;
        //    response.Upc = variant.Upc;
        //    response.OriginalPrice = variant.OriginalPrice;
        //    response.SalePrice = variant.SalePrice;
        //    response.UnitPrice = variant.UnitPrice;
        //    response.PurchasePrice = variant.PurchasePrice;
        //    response.DescriptionFullTh = variant.DescriptionFullTh;
        //    response.DescriptionShortTh = variant.DescriptionShortTh;
        //    response.DescriptionFullEn = variant.DescriptionFullEn;
        //    response.DescriptionShortEn = variant.DescriptionShortEn;
        //    response.PrepareDay = variant.PrepareDay;
        //    response.LimitIndividualDay = variant.LimitIndividualDay;
        //    response.PrepareMon = variant.PrepareMon;
        //    response.PrepareTue = variant.PrepareTue;
        //    response.PrepareWed = variant.PrepareWed;
        //    response.PrepareThu = variant.PrepareThu;
        //    response.PrepareFri = variant.PrepareFri;
        //    response.PrepareSat = variant.PrepareSat;
        //    response.PrepareSun = variant.PrepareSun;
        //    response.KillerPoint1En = variant.KillerPoint1En;
        //    response.KillerPoint2En = variant.KillerPoint2En;
        //    response.KillerPoint3En = variant.KillerPoint3En;
        //    response.KillerPoint1Th = variant.KillerPoint1Th;
        //    response.KillerPoint2Th = variant.KillerPoint2Th;
        //    response.KillerPoint3Th = variant.KillerPoint3Th;
        //    response.Installment = variant.Installment;
        //    response.Length = variant.Length;
        //    response.Height = variant.Height;
        //    response.Width = variant.Width;
        //    response.Weight = variant.Weight;
        //    response.DimensionUnit = variant.DimensionUnit.Trim();
        //    response.WeightUnit = variant.WeightUnit.Trim();
        //    response.SEO.MetaTitleEn = variant.MetaTitleEn;
        //    response.SEO.MetaTitleTh = variant.MetaTitleTh;
        //    response.SEO.MetaDescriptionEn = variant.MetaDescriptionEn;
        //    response.SEO.MetaDescriptionTh = variant.MetaDescriptionTh;
        //    response.SEO.MetaKeywordEn = variant.MetaKeyEn;
        //    response.SEO.MetaKeywordTh = variant.MetaKeyTh;
        //    response.SEO.ProductBoostingWeight = variant.BoostWeight;
        //    response.SEO.SeoEn = variant.SeoEn;
        //    response.SEO.SeoTh = variant.SeoTh;
        //    response.Visibility = variant.Visibility;
        //    response.DefaultVariant = variant.DefaultVariant;
        //    response.PromotionPrice = variant.PromotionPrice;
        //    response.EffectiveDatePromotion = variant.EffectiveDatePromotion;
        //    response.ExpireDatePromotion = variant.ExpireDatePromotion;
        //    response.IsHasExpiryDate = variant.IsHasExpiryDate;
        //    response.ExpressDelivery = variant.ExpressDelivery;
        //    response.Display = variant.Display;
        //    response.IsHasExpiryDate = variant.IsHasExpiryDate;
        //    response.IsVat = variant.IsVat;
        //    if (variant.ProductHistoryImages != null && variant.ProductHistoryImages.Count > 0)
        //    {
        //        variant.ProductHistoryImages = variant.ProductHistoryImages.OrderBy(o => o.Position).ToList();
        //        foreach (var image in variant.ProductHistoryImages)
        //        {
        //            response.Images.Add(new ImageRequest()
        //            {
        //                ImageId = 0,
        //                Url = image.ImageUrlEn,
        //                Position = image.Position
        //            });
        //        }
        //    }
        //    if (variant.ProductHistoryVideos != null && variant.ProductHistoryVideos.Count > 0)
        //    {
        //        variant.ProductHistoryVideos = variant.ProductHistoryVideos.OrderBy(o => o.Position).ToList();
        //        foreach (var video in variant.ProductHistoryVideos)
        //        {
        //            response.VideoLinks.Add(new VideoLinkRequest()
        //            {
        //                VideoId = 0,
        //                Url = video.VideoUrlEn
        //            });
        //        }
        //    }
        //}

        //private void SetupAttributeResponse(ProductHistory variant, List<AttributeRequest> attributeList)
        //{
        //    foreach (var attribute in variant.ProductHistoryAttributes)
        //    {
        //        if (attributeList.Where(w => w.AttributeId == attribute.AttributeId).SingleOrDefault() != null)
        //        {
        //            continue;
        //        }
        //        AttributeRequest tmpAttribute = new AttributeRequest();
        //        tmpAttribute.AttributeId = attribute.AttributeId;
        //        tmpAttribute.DataType = attribute.Attribute.DataType;
        //        if (Constant.DATA_TYPE_LIST.Equals(attribute.Attribute.DataType))
        //        {
        //            var tmpValue = attribute.Attribute
        //                .AttributeValueMaps
        //                .Select(s => s.AttributeValue)
        //                .Where(w => w.MapValue.Equals(attribute.ValueEn))
        //                .FirstOrDefault();
        //            if (tmpValue != null)
        //            {
        //                tmpAttribute.AttributeValues.Add(new AttributeValueRequest()
        //                {
        //                    AttributeValueId = tmpValue.AttributeValueId,
        //                    AttributeValueEn = tmpValue.AttributeValueEn,
        //                    CheckboxValue = attribute.CheckboxValue
        //                });
        //            }
        //        }
        //        else if (Constant.DATA_TYPE_CHECKBOX.Equals(attribute.Attribute.DataType))
        //        {
        //            var tmpAttributeList = variant.ProductHistoryAttributes.Where(w => w.AttributeId == attribute.AttributeId).ToList();
        //            foreach (var attr in tmpAttributeList)
        //            {
        //                var tmpValue = attribute.Attribute
        //               .AttributeValueMaps
        //               .Select(s => s.AttributeValue)
        //               .Where(w => w.MapValue.Equals(attr.ValueEn)).ToList();
        //                if (tmpValue != null && tmpValue.Count > 0)
        //                {
        //                    foreach (var val in tmpValue)
        //                    {
        //                        tmpAttribute.AttributeValues.Add(new AttributeValueRequest()
        //                        {
        //                            AttributeValueId = val.AttributeValueId,
        //                            AttributeValueEn = val.AttributeValueEn,
        //                            CheckboxValue = attr.CheckboxValue
        //                        });
        //                    }
        //                }
        //            }

        //        }
        //        else
        //        {
        //            tmpAttribute.ValueEn = attribute.ValueEn;
        //        }
        //        attributeList.Add(tmpAttribute);
        //    }
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        //private bool ProductHistoryExists(long id)
        //{
        //    return db.ProductHistories.Count(e => e.HistoryId == id) > 0;
        //}


        //private ProductStageRequest GetProductStageRequestFromId(ColspEntities db, long productId)
        //{
        //    #region Query
        //    var tmpPro = db.ProductHistoryGroups.Where(w => !Constant.STATUS_REMOVE.Equals(w.Status) && w.ProductId == productId)
        //        .Select(s => new
        //        {
        //            s.ProductId,
        //            s.ShopId,
        //            s.GlobalCatId,
        //            s.LocalCatId,
        //            s.AttributeSetId,
        //            s.BrandId,
        //            s.ShippingId,
        //            s.EffectiveDate,
        //            s.ExpireDate,
        //            s.NewArrivalDate,
        //            s.TheOneCardEarn,
        //            s.GiftWrap,
        //            s.IsNew,
        //            s.IsClearance,
        //            s.IsBestSeller,
        //            s.IsOnlineExclusive,
        //            s.IsOnlyAt,
        //            s.Remark,
        //            s.InfoFlag,
        //            s.ImageFlag,
        //            s.OnlineFlag,
        //            s.InformationTabStatus,
        //            s.ImageTabStatus,
        //            s.CategoryTabStatus,
        //            s.VariantTabStatus,
        //            s.MoreOptionTabStatus,
        //            s.RejectReason,
        //            s.Status,
        //            s.FirstApproveBy,
        //            s.FirstApproveOn,
        //            s.ApproveBy,
        //            s.ApproveOn,
        //            s.RejecteBy,
        //            s.RejectOn,
        //            s.SubmitBy,
        //            s.SubmitOn,
        //            s.CreateBy,
        //            s.CreateOn,
        //            s.UpdateBy,
        //            s.UpdateOn,
        //            AttributeSet = s.AttributeSet == null ? null : new
        //            {
        //                s.AttributeSet.AttributeSetId,
        //                s.AttributeSet.AttributeSetNameEn,
        //                AttributeSetMap = s.AttributeSet.AttributeSetMaps.Select(sm => new
        //                {
        //                    sm.Attribute.DataType,
        //                    sm.AttributeId,
        //                    AttributeValueMap = sm.Attribute.AttributeValueMaps.Select(sv => new
        //                    {
        //                        sv.AttributeValue.AttributeValueId,
        //                        sv.AttributeValue.AttributeValueEn
        //                    })
        //                })
        //            },
        //            Brand = s.Brand == null ? null : new
        //            {
        //                s.Brand.BrandId,
        //                s.Brand.BrandNameEn
        //            },
        //            GlobalCategories = s.ProductStageGlobalCatMaps.Select(sg => new
        //            {
        //                sg.CategoryId,
        //                sg.GlobalCategory.NameEn
        //            }),
        //            LocalCategories = s.ProductStageLocalCatMaps.Select(sl => new
        //            {
        //                sl.CategoryId,
        //                sl.LocalCategory.NameEn
        //            }),
        //            Tags = s.ProductStageTags.Select(st => st.Tag),
        //            ProductStages = s.ProductStages.Select(st => new
        //            {
        //                st.Pid,
        //                st.ProductId,
        //                st.ShopId,
        //                st.ProductNameEn,
        //                st.ProductNameTh,
        //                st.ProdTDNameTh,
        //                st.ProdTDNameEn,
        //                st.JDADept,
        //                st.JDASubDept,
        //                st.SaleUnitTh,
        //                st.SaleUnitEn,
        //                st.Sku,
        //                st.Upc,
        //                st.OriginalPrice,
        //                st.SalePrice,
        //                st.DescriptionFullEn,
        //                st.DescriptionShortEn,
        //                st.DescriptionFullTh,
        //                st.DescriptionShortTh,
        //                st.MobileDescriptionEn,
        //                st.MobileDescriptionTh,
        //                st.ImageCount,
        //                st.FeatureImgUrl,
        //                st.PrepareDay,
        //                st.LimitIndividualDay,
        //                st.PrepareMon,
        //                st.PrepareTue,
        //                st.PrepareWed,
        //                st.PrepareThu,
        //                st.PrepareFri,
        //                st.PrepareSat,
        //                st.PrepareSun,
        //                st.KillerPoint1En,
        //                st.KillerPoint2En,
        //                st.KillerPoint3En,
        //                st.KillerPoint1Th,
        //                st.KillerPoint2Th,
        //                st.KillerPoint3Th,
        //                st.Installment,
        //                st.Length,
        //                st.Height,
        //                st.Width,
        //                st.DimensionUnit,
        //                st.Weight,
        //                st.WeightUnit,
        //                st.MetaTitleEn,
        //                st.MetaTitleTh,
        //                st.MetaDescriptionEn,
        //                st.MetaDescriptionTh,
        //                st.MetaKeyEn,
        //                st.MetaKeyTh,
        //                st.SeoEn,
        //                st.SeoTh,
        //                st.IsHasExpiryDate,
        //                st.IsVat,
        //                st.UrlKey,
        //                st.BoostWeight,
        //                st.GlobalBoostWeight,
        //                st.ExpressDelivery,
        //                st.DeliveryFee,
        //                st.PromotionPrice,
        //                st.EffectiveDatePromotion,
        //                st.ExpireDatePromotion,
        //                st.NewArrivalDate,
        //                st.DefaultVariant,
        //                st.Display,
        //                st.MiniQtyAllowed,
        //                st.MaxiQtyAllowed,
        //                st.UnitPrice,
        //                st.PurchasePrice,
        //                st.IsSell,
        //                st.IsVariant,
        //                st.IsMaster,
        //                st.VariantCount,
        //                st.Visibility,
        //                ProductStageAttributes = st.ProductStageAttributes.Select(sa => new
        //                {
        //                    sa.Pid,
        //                    sa.AttributeId,
        //                    sa.ValueEn,
        //                    sa.ValueTh,
        //                    sa.AttributeValueId,
        //                    sa.CheckboxValue,
        //                    sa.Position,
        //                    sa.IsAttributeValue,
        //                    Attribute = new
        //                    {
        //                        sa.Attribute.AttributeId,
        //                        sa.Attribute.AttributeNameEn,
        //                        sa.Attribute.DataType,
        //                        sa.Attribute.VariantStatus,
        //                    },
        //                    AttributeValue = sa.AttributeValue == null ? null : new
        //                    {
        //                        sa.AttributeValue.AttributeValueId,
        //                        sa.AttributeValue.AttributeValueEn,
        //                        sa.AttributeValue.AttributeValueTh,
        //                    }
        //                }),
        //                Images = st.ProductStageImages.Select(si => new
        //                {
        //                    si.ImageUrlEn
        //                }),
        //                Videos = st.ProductStageVideos.OrderBy(o => o.Position).Select(sv => new
        //                {
        //                    sv.VideoUrlEn
        //                }),
        //                Inventory = st.Inventory == null ? null : new
        //                {
        //                    st.Inventory.OnHold,
        //                    st.Inventory.Reserve,
        //                    st.Inventory.Quantity,
        //                    st.Inventory.SafetyStockSeller,
        //                    st.Inventory.StockType,
        //                    st.Inventory.MaxQtyAllowInCart,
        //                    st.Inventory.MaxQtyPreOrder,
        //                    st.Inventory.MinQtyAllowInCart,
        //                    st.Inventory.Defect
        //                }
        //            }),

        //        }).SingleOrDefault();
        //    #endregion
        //    #region group
        //    ProductStageRequest response = new ProductStageRequest()
        //    {
        //        AdminApprove = new AdminApproveRequest()
        //        {
        //            Category = tmpPro.CategoryTabStatus,
        //            Image = tmpPro.ImageTabStatus,
        //            Information = tmpPro.InformationTabStatus,
        //            MoreOption = tmpPro.MoreOptionTabStatus,
        //            RejectReason = tmpPro.RejectReason,
        //            Variation = tmpPro.VariantTabStatus
        //        },
        //        AttributeSet = tmpPro.AttributeSet == null ? new AttributeSetRequest() : new AttributeSetRequest()
        //        {
        //            AttributeSetId = tmpPro.AttributeSet.AttributeSetId,
        //            AttributeSetNameEn = tmpPro.AttributeSet.AttributeSetNameEn,
        //            Attributes = tmpPro.AttributeSet.AttributeSetMap.Select(s => new AttributeRequest()
        //            {
        //                AttributeId = s.AttributeId,
        //                DataType = s.DataType,
        //                AttributeValues = s.AttributeValueMap.Select(sv => new AttributeValueRequest()
        //                {
        //                    AttributeValueEn = sv.AttributeValueEn,
        //                    AttributeValueId = sv.AttributeValueId,
        //                }).ToList(),
        //            }).ToList(),
        //        },
        //        Brand = tmpPro.Brand == null ? new BrandRequest() : new BrandRequest()
        //        {
        //            BrandId = tmpPro.Brand.BrandId,
        //            BrandNameEn = tmpPro.Brand.BrandNameEn
        //        },
        //        ControlFlags = new ControlFlagRequest()
        //        {
        //            IsBestSeller = tmpPro.IsBestSeller,
        //            IsClearance = tmpPro.IsClearance,
        //            IsNew = tmpPro.IsNew,
        //            IsOnlineExclusive = tmpPro.IsOnlineExclusive,
        //            IsOnlyAt = tmpPro.IsOnlyAt
        //        },
        //        EffectiveDate = tmpPro.EffectiveDate,
        //        ExpireDate = tmpPro.ExpireDate,
        //        NewArrivalDate = tmpPro.NewArrivalDate,
        //        GiftWrap = tmpPro.GiftWrap,
        //        GlobalCategories = tmpPro.GlobalCategories.Select(s => new CategoryRequest()
        //        {
        //            CategoryId = s.CategoryId
        //        }).ToList(),
        //        LocalCategories = tmpPro.LocalCategories.Select(s => new CategoryRequest()
        //        {
        //            CategoryId = s.CategoryId
        //        }).ToList(),
        //        ImageFlag = tmpPro.ImageFlag,
        //        InfoFlag = tmpPro.InfoFlag,
        //        ProductId = tmpPro.ProductId,
        //        MainGlobalCategory = new CategoryRequest()
        //        {
        //            CategoryId = tmpPro.GlobalCatId
        //        },
        //        MainLocalCategory = new CategoryRequest()
        //        {
        //            CategoryId = tmpPro.LocalCatId.HasValue ? tmpPro.LocalCatId.Value : 0
        //        },
        //        OnlineFlag = tmpPro.OnlineFlag,
        //        Remark = tmpPro.Remark,
        //        ShippingMethod = tmpPro.ShippingId,
        //        ShopId = tmpPro.ShopId,
        //        Status = tmpPro.Status,
        //        Tags = tmpPro.Tags.ToList()
        //    };

        //    #endregion
        //    #region master
        //    var masterVariant = tmpPro.ProductStages.Where(w => w.IsVariant == false).SingleOrDefault();
        //    response.MasterVariant = new VariantRequest()
        //    {
        //        Pid = masterVariant.Pid,
        //        ProductId = masterVariant.ProductId,
        //        ShopId = masterVariant.ShopId,
        //        ProductNameEn = masterVariant.ProductNameEn,
        //        ProductNameTh = masterVariant.ProductNameTh,
        //        ProdTDNameTh = masterVariant.ProdTDNameTh,
        //        ProdTDNameEn = masterVariant.ProdTDNameEn,
        //        SaleUnitTh = masterVariant.SaleUnitTh,
        //        SaleUnitEn = masterVariant.SaleUnitEn,
        //        Sku = masterVariant.Sku,
        //        Upc = masterVariant.Upc,
        //        OriginalPrice = masterVariant.OriginalPrice,
        //        SalePrice = masterVariant.SalePrice,
        //        DescriptionFullEn = masterVariant.DescriptionFullEn,
        //        DescriptionShortEn = masterVariant.DescriptionShortEn,
        //        DescriptionFullTh = masterVariant.DescriptionFullTh,
        //        DescriptionShortTh = masterVariant.DescriptionShortTh,
        //        MobileDescriptionEn = masterVariant.MobileDescriptionEn,
        //        MobileDescriptionTh = masterVariant.MobileDescriptionTh,
        //        PrepareDay = masterVariant.PrepareDay,
        //        LimitIndividualDay = masterVariant.LimitIndividualDay,
        //        PrepareMon = masterVariant.PrepareMon,
        //        PrepareTue = masterVariant.PrepareTue,
        //        PrepareWed = masterVariant.PrepareWed,
        //        PrepareThu = masterVariant.PrepareThu,
        //        PrepareFri = masterVariant.PrepareFri,
        //        PrepareSat = masterVariant.PrepareSat,
        //        PrepareSun = masterVariant.PrepareSun,
        //        KillerPoint1En = masterVariant.KillerPoint1En,
        //        KillerPoint2En = masterVariant.KillerPoint2En,
        //        KillerPoint3En = masterVariant.KillerPoint3En,
        //        KillerPoint1Th = masterVariant.KillerPoint1Th,
        //        KillerPoint2Th = masterVariant.KillerPoint2Th,
        //        KillerPoint3Th = masterVariant.KillerPoint3Th,
        //        Installment = masterVariant.Installment,
        //        Length = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Length / 1000) : masterVariant.Length,
        //        Height = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Height / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Height / 1000) : masterVariant.Height,
        //        Width = Constant.DIMENSTION_CM.Equals(masterVariant.DimensionUnit) ? (masterVariant.Width / 10) : Constant.DIMENSTION_M.Equals(masterVariant.DimensionUnit) ? (masterVariant.Width / 1000) : masterVariant.Width,
        //        DimensionUnit = masterVariant.DimensionUnit,
        //        Weight = Constant.WEIGHT_MEASURE_KG.Equals(masterVariant.WeightUnit) ? (masterVariant.Weight / 1000) : masterVariant.Weight,
        //        WeightUnit = masterVariant.WeightUnit,
        //        SEO = new SEORequest()
        //        {
        //            GlobalProductBoostingWeight = masterVariant.GlobalBoostWeight,
        //            MetaDescriptionEn = masterVariant.MetaDescriptionEn,
        //            MetaDescriptionTh = masterVariant.MetaDescriptionTh,
        //            MetaKeywordEn = masterVariant.MetaKeyEn,
        //            MetaKeywordTh = masterVariant.MetaKeyTh,
        //            MetaTitleEn = masterVariant.MetaTitleEn,
        //            MetaTitleTh = masterVariant.MetaTitleTh,
        //            ProductBoostingWeight = masterVariant.BoostWeight,
        //            ProductUrlKeyEn = masterVariant.UrlKey,
        //            SeoEn = masterVariant.SeoEn,
        //            SeoTh = masterVariant.SeoTh,
        //        },
        //        IsHasExpiryDate = masterVariant.IsHasExpiryDate,
        //        IsVat = masterVariant.IsVat,
        //        ExpressDelivery = masterVariant.ExpressDelivery,
        //        DeliveryFee = masterVariant.DeliveryFee,
        //        PromotionPrice = masterVariant.PromotionPrice,
        //        EffectiveDatePromotion = masterVariant.EffectiveDatePromotion,
        //        ExpireDatePromotion = masterVariant.ExpireDatePromotion,
        //        NewArrivalDate = masterVariant.NewArrivalDate,
        //        DefaultVariant = masterVariant.DefaultVariant,
        //        Display = masterVariant.Display,
        //        MiniQtyAllowed = masterVariant.MiniQtyAllowed,
        //        MaxiQtyAllowed = masterVariant.MaxiQtyAllowed,
        //        UnitPrice = masterVariant.UnitPrice,
        //        PurchasePrice = masterVariant.PurchasePrice,
        //        IsVariant = masterVariant.IsVariant,
        //        Visibility = masterVariant.Visibility,
        //        OnHold = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.OnHold,
        //        Quantity = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Quantity,
        //        Defect = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Defect,
        //        Reserve = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.Reserve,
        //        SafetyStock = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.SafetyStockSeller,
        //        StockType = masterVariant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : masterVariant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
        //        MaxQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyAllowInCart,
        //        MinQtyAllowInCart = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MinQtyAllowInCart,
        //        MaxQtyPreOrder = masterVariant.Inventory == null ? 0 : masterVariant.Inventory.MaxQtyPreOrder,
        //    };
        //    if (masterVariant.ProductStageAttributes != null)
        //    {
        //        foreach (var attribute in masterVariant.ProductStageAttributes)
        //        {

        //            var attr = new AttributeRequest()
        //            {
        //                AttributeId = attribute.Attribute.AttributeId,
        //                DataType = attribute.Attribute.DataType,
        //                AttributeNameEn = attribute.Attribute.AttributeNameEn,
        //                ValueEn = attribute.ValueEn,
        //                ValueTh = attribute.ValueTh,
        //            };
        //            if (attribute.AttributeValue != null)
        //            {
        //                attr.ValueEn = string.Empty;
        //                attr.ValueTh = string.Empty;
        //                attr.AttributeValues.Add(new AttributeValueRequest()
        //                {
        //                    AttributeValueId = attribute.AttributeValueId.HasValue ? attribute.AttributeValueId.Value : 0,
        //                    AttributeValueEn = attribute.AttributeValue.AttributeValueEn,
        //                    CheckboxValue = attribute.CheckboxValue,
        //                    AttributeValueTh = attribute.AttributeValue.AttributeValueTh,
        //                    Position = attribute.Position,
        //                });
        //            }
        //            response.MasterAttribute.Add(attr);
        //        }
        //    }
        //    if (masterVariant.Images != null)
        //    {
        //        foreach (var image in masterVariant.Images)
        //        {
        //            response.MasterVariant.Images.Add(new ImageRequest()
        //            {
        //                Url = image.ImageUrlEn,
        //            });
        //        }
        //    }
        //    if (masterVariant.Videos != null)
        //    {
        //        foreach (var video in masterVariant.Videos)
        //        {
        //            response.MasterVariant.VideoLinks.Add(new VideoLinkRequest()
        //            {
        //                Url = video.VideoUrlEn,
        //            });
        //        }
        //    }
        //    response.Visibility = masterVariant.Visibility;
        //    #endregion
        //    #region variant
        //    var variants = tmpPro.ProductStages.Where(w => w.IsVariant == true && w.IsMaster == false);
        //    foreach (var variant in variants)
        //    {
        //        var tmpVariant = new VariantRequest()
        //        {
        //            Pid = variant.Pid,
        //            ProductId = variant.ProductId,
        //            ShopId = variant.ShopId,
        //            ProductNameEn = variant.ProductNameEn,
        //            ProductNameTh = variant.ProductNameTh,
        //            ProdTDNameTh = variant.ProdTDNameTh,
        //            ProdTDNameEn = variant.ProdTDNameEn,
        //            SaleUnitTh = variant.SaleUnitTh,
        //            SaleUnitEn = variant.SaleUnitEn,
        //            Sku = variant.Sku,
        //            Upc = variant.Upc,
        //            OriginalPrice = variant.OriginalPrice,
        //            SalePrice = variant.SalePrice,
        //            DescriptionFullEn = variant.DescriptionFullEn,
        //            DescriptionShortEn = variant.DescriptionShortEn,
        //            DescriptionFullTh = variant.DescriptionFullTh,
        //            DescriptionShortTh = variant.DescriptionShortTh,
        //            MobileDescriptionEn = variant.MobileDescriptionEn,
        //            MobileDescriptionTh = variant.MobileDescriptionTh,
        //            PrepareDay = variant.PrepareDay,
        //            LimitIndividualDay = variant.LimitIndividualDay,
        //            PrepareMon = variant.PrepareMon,
        //            PrepareTue = variant.PrepareTue,
        //            PrepareWed = variant.PrepareWed,
        //            PrepareThu = variant.PrepareThu,
        //            PrepareFri = variant.PrepareFri,
        //            PrepareSat = variant.PrepareSat,
        //            PrepareSun = variant.PrepareSun,
        //            KillerPoint1En = variant.KillerPoint1En,
        //            KillerPoint2En = variant.KillerPoint2En,
        //            KillerPoint3En = variant.KillerPoint3En,
        //            KillerPoint1Th = variant.KillerPoint1Th,
        //            KillerPoint2Th = variant.KillerPoint2Th,
        //            KillerPoint3Th = variant.KillerPoint3Th,
        //            Installment = variant.Installment,
        //            Length = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Length / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Length / 1000) : variant.Length,
        //            Height = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Height / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Height / 1000) : variant.Height,
        //            Width = Constant.DIMENSTION_CM.Equals(variant.DimensionUnit) ? (variant.Width / 10) : Constant.DIMENSTION_M.Equals(variant.DimensionUnit) ? (variant.Width / 1000) : variant.Width,
        //            DimensionUnit = variant.DimensionUnit,
        //            Weight = Constant.WEIGHT_MEASURE_KG.Equals(variant.WeightUnit) ? (variant.Weight / 1000) : variant.Weight,
        //            WeightUnit = variant.WeightUnit,
        //            SEO = new SEORequest()
        //            {
        //                GlobalProductBoostingWeight = variant.GlobalBoostWeight,
        //                MetaDescriptionEn = variant.MetaDescriptionEn,
        //                MetaDescriptionTh = variant.MetaDescriptionTh,
        //                MetaKeywordEn = variant.MetaKeyEn,
        //                MetaKeywordTh = variant.MetaKeyTh,
        //                MetaTitleEn = variant.MetaTitleEn,
        //                MetaTitleTh = variant.MetaTitleTh,
        //                ProductBoostingWeight = variant.BoostWeight,
        //                ProductUrlKeyEn = variant.UrlKey,
        //                SeoEn = variant.SeoEn,
        //                SeoTh = variant.SeoTh,
        //            },
        //            IsHasExpiryDate = variant.IsHasExpiryDate,
        //            IsVat = variant.IsVat,
        //            ExpressDelivery = variant.ExpressDelivery,
        //            DeliveryFee = variant.DeliveryFee,
        //            PromotionPrice = variant.PromotionPrice,
        //            EffectiveDatePromotion = variant.EffectiveDatePromotion,
        //            ExpireDatePromotion = variant.ExpireDatePromotion,
        //            NewArrivalDate = variant.NewArrivalDate,
        //            DefaultVariant = variant.DefaultVariant,
        //            Display = variant.Display,
        //            MiniQtyAllowed = variant.MiniQtyAllowed,
        //            MaxiQtyAllowed = variant.MaxiQtyAllowed,
        //            UnitPrice = variant.UnitPrice,
        //            PurchasePrice = variant.PurchasePrice,
        //            IsVariant = variant.IsVariant,
        //            Visibility = variant.Visibility,
        //            OnHold = variant.Inventory == null ? 0 : variant.Inventory.OnHold,
        //            Quantity = variant.Inventory == null ? 0 : variant.Inventory.Quantity,
        //            Reserve = variant.Inventory == null ? 0 : variant.Inventory.Reserve,
        //            Defect = variant.Inventory == null ? 0 : variant.Inventory.Defect,
        //            SafetyStock = variant.Inventory == null ? 0 : variant.Inventory.SafetyStockSeller,
        //            StockType = variant.Inventory == null ? Constant.DEFAULT_STOCK_TYPE : variant.Inventory.StockType == 1 ? "Stock" : "Pre-Order",
        //            MaxQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyAllowInCart,
        //            MinQtyAllowInCart = variant.Inventory == null ? 0 : variant.Inventory.MinQtyAllowInCart,
        //            MaxQtyPreOrder = variant.Inventory == null ? 0 : variant.Inventory.MaxQtyPreOrder,
        //        };

        //        if (variant.ProductStageAttributes != null)
        //        {
        //            foreach (var attribute in variant.ProductStageAttributes)
        //            {

        //                var attr = new AttributeRequest()
        //                {
        //                    AttributeId = attribute.Attribute.AttributeId,
        //                    DataType = attribute.Attribute.DataType,
        //                    AttributeNameEn = attribute.Attribute.AttributeNameEn,
        //                    ValueEn = attribute.ValueEn,
        //                    ValueTh = attribute.ValueTh,
        //                };
        //                if (attribute.AttributeValue != null)
        //                {
        //                    attr.ValueEn = string.Empty;
        //                    attr.ValueTh = string.Empty;

        //                    attr.AttributeValues.Add(new AttributeValueRequest()
        //                    {
        //                        AttributeValueId = attribute.AttributeValueId.HasValue ? attribute.AttributeValueId.Value : 0,
        //                        AttributeValueEn = attribute.AttributeValue.AttributeValueEn,
        //                        CheckboxValue = attribute.CheckboxValue,
        //                        AttributeValueTh = attribute.AttributeValue.AttributeValueTh,
        //                        Position = attribute.Position,
        //                    });
        //                }
        //                if (tmpVariant.FirstAttribute == null || tmpVariant.FirstAttribute.AttributeId == 0)
        //                {
        //                    tmpVariant.FirstAttribute = attr;
        //                }
        //                else
        //                {
        //                    tmpVariant.SecondAttribute = attr;
        //                }
        //            }
        //        }
        //        if (variant.Images != null)
        //        {
        //            foreach (var image in variant.Images)
        //            {
        //                tmpVariant.Images.Add(new ImageRequest()
        //                {
        //                    Url = image.ImageUrlEn,
        //                });
        //            }
        //        }
        //        if (variant.Videos != null)
        //        {
        //            foreach (var video in variant.Videos)
        //            {
        //                tmpVariant.VideoLinks.Add(new VideoLinkRequest()
        //                {
        //                    Url = video.VideoUrlEn,
        //                });
        //            }
        //        }
        //        response.Variants.Add(tmpVariant);
        //    }
        //    #endregion
        //    #region history
        //    var historyList = db.ProductHistoryGroups
        //        .Where(w => w.ProductId == response.ProductId)
        //        .OrderByDescending(o => o.HistoryDt)
        //        .Select(s => new ProductHistoryRequest()
        //        {
        //            ApproveOn = s.ApproveOn,
        //            SubmitBy = s.SubmitBy,
        //            HistoryId = s.HistoryId,
        //            SubmitOn = s.SubmitOn
        //        }).Take(Constant.HISTORY_REVISION);
        //    foreach (var history in historyList)
        //    {
        //        response.Revisions.Add(new ProductHistoryRequest()
        //        {
        //            ApproveOn = history.ApproveOn,
        //            HistoryId = history.HistoryId,
        //            SubmitBy = history.SubmitBy,
        //            SubmitOn = history.SubmitOn
        //        });
        //    }
        //    #endregion
        //    return response;
        //}
    }
}