using Cenergy.Dazzle.Admin.Security.Cryptography;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Colsp.Api.Controllers
{
    public class DefaultController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Default")]
        [HttpGet]
        public HttpResponseMessage GetTest()
        {
            try
            {
                var query = db.ProductStages.Where(w=>w.ShopId==3).Select(s => new
                {
                    ProductStageGroup = new
                    {
                        s.ProductStageGroup.Brand.BrandNameEn,
                        Tags = s.ProductStageGroup.ProductStageTags.Select(st => st.Tag),
                        s.ProductStageGroup.GlobalCatId,
                        s.ProductStageGroup.LocalCatId,
                        ProductStageGlobalCatMaps = s.ProductStageGroup.ProductStageGlobalCatMaps.Select(sc => sc.CategoryId),
                        ProductStageLocalCatMaps = s.ProductStageGroup.ProductStageLocalCatMaps.Select(sc => sc.CategoryId),
                        ProductStageRelateds1 = s.ProductStageGroup.ProductStageRelateds1.Select(sp => sp.ProductStageGroup1.ProductStages.Where(w => w.IsVariant == false).Select(sv => sv.Pid)),
                        s.ProductStageGroup.EffectiveDate,
                        s.ProductStageGroup.ExpireDate,
                        s.ProductStageGroup.ControlFlag1,
                        s.ProductStageGroup.ControlFlag2,
                        s.ProductStageGroup.ControlFlag3,
                        s.ProductStageGroup.Remark,
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
                    s.DefaultVaraint,
                    s.Pid,
                    s.ProductNameEn,
                    s.ProductNameTh,
                    s.Sku,
                    s.Upc,
                    s.OriginalPrice,
                    s.SalePrice,
                    s.Installment,
                    s.DescriptionFullEn,
                    s.DescriptionFullTh,
                    s.DescriptionShortEn,
                    s.DescriptionShortTh,
                    s.KillerPoint1En,
                    s.KillerPoint1Th,
                    s.KillerPoint2En,
                    s.KillerPoint2Th,
                    s.KillerPoint3En,
                    s.KillerPoint3Th,
                    Inventory = s.Inventory == null ? null : new
                    {
                        s.Inventory.Quantity,
                        s.Inventory.SafetyStockSeller,
                        s.Inventory.StockAvailable,
                    },
                    s.Shipping.ShippingMethodEn,
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
                    s.UrlEn,
                    s.BoostWeight,
                    s.GlobalBoostWeight,
                    s.GiftWrap,
                    ProductStageAttributes = s.ProductStageAttributes.Select(ss => new
                    {
                        ss.IsAttributeValue,
                        ss.CheckboxValue,
                        ss.ValueEn,
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
                }).ToList();
            }
            catch (Exception e)
            {

            }
            

            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "false");
        }


        [Route("api/Test")]
        [HttpGet]
        [OverrideAuthentication, OverrideAuthorization]
        public HttpResponseMessage Test()
        {
            SaltedSha256PasswordHasher salt = new SaltedSha256PasswordHasher();

            return Request.CreateResponse(HttpStatusCode.OK, salt.HashPassword("vader"));
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
