using Colsp.Entity.Models;
using Colsp.Model.Requests;
using System;
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
                var query = db.ProductStages.Where(w=>w.ShopId==19).Select(s => new
                {
                    s.Status,
                    s.ProductId,
                    s.DefaultVaraint,
                    s.Pid,
                    s.ProductNameEn,
                    s.ProductNameTh,
                    s.Sku,
                    s.Upc,
                    s.ProductStageGroup.Brand.BrandNameEn,
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
                    s.ProductStageGroup.ProductStageTags,
                    Inventory = new
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
                    s.ProductStageGroup.GlobalCatId,
                    s.ProductStageGroup.LocalCatId,
                    s.ProductStageGroup.ProductStageGlobalCatMaps,
                    s.ProductStageGroup.ProductStageLocalCatMaps,
                    ProductStageRelated = s.ProductStageGroup.ProductStageRelateds1.Select(sp => sp.ProductStageGroup1.ProductStages.Where(w => w.IsVariant == false).Select(sv => sv.Pid)),
                    s.MetaDescriptionEn,
                    s.MetaDescriptionTh,
                    s.MetaKeyEn,
                    s.MetaKeyTh,
                    s.MetaTitleEn,
                    s.MetaTitleTh,
                    s.UrlEn,
                    s.BoostWeight,
                    s.GlobalBoostWeight,
                    s.ProductStageGroup.EffectiveDate,
                    s.ProductStageGroup.ExpireDate,
                    s.GiftWrap,
                    s.ProductStageGroup.ControlFlag1,
                    s.ProductStageGroup.ControlFlag2,
                    s.ProductStageGroup.ControlFlag3,
                    s.ProductStageGroup.Remark,
                    ProductStageAttributes = s.ProductStageAttributes.Select(ss => new
                    {
                        ss.IsAttributeValue,
                        ss.CheckboxValue,
                        ss.ValueEn,
                        Attribute = new AttributeRequest()
                        {
                            AttributeNameEn = ss.Attribute.AttributeNameEn,
                            DataType = ss.Attribute.DataType,
                            DefaultAttribute = ss.Attribute.DefaultAttribute,
                            AttributeValues =
                            ss.Attribute.AttributeValueMaps.Select(sv => new AttributeValueRequest()
                            {
                                AttributeValueEn = sv.AttributeValue.AttributeValueEn,
                                AttributeValueId = sv.AttributeValue.AttributeValueId,

                            }).ToList()
                        }
                    }),
                    AttributeSet = new
                    {
                        s.ProductStageGroup.AttributeSet.AttributeSetNameEn,
                        Attribute = new 
                        {
                            AttributeNameEn = s.ProductStageGroup.AttributeSet.AttributeSetMaps.Select(sa => sa.Attribute.AttributeNameEn),
                            AttributeValue = s.ProductStageGroup.AttributeSet.AttributeSetMaps.Select(sa => sa.Attribute.AttributeValueMaps.Select(sv => new
                            {
                                sv.AttributeValue.AttributeValueEn,
                            }))
                        }
                    },
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
