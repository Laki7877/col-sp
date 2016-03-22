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
using CsvHelper;
using System.Drawing;
using System.Drawing.Imaging;

namespace Colsp.Api.Controllers
{
    public class ProductStagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        private readonly string root = HttpContext.Current.Server.MapPath("~/Import");

        //[Route("api/ProductStages/Export")]
        //[HttpPost]
        //public HttpResponseMessage ExportProductProducts(ExportRequest request)
        //{
        //    MemoryStream stream = null;
        //    StreamWriter writer = null;
        //    try
        //    {
        //        if (request == null)
        //        {
        //            throw new Exception("Invalid request");
        //        }
        //        #region Setup Header
        //        int i = 0;
        //        Dictionary<string, Tuple<string, int>> headDicTmp = new Dictionary<string, Tuple<string, int>>();
        //        var guidance = db.ImportHeaders.OrderBy(o => o.ImportHeaderId).ToList();

        //        foreach (var current in guidance)
        //        {
        //            var op = request.Options.Where(w => w.Equals(current.MapName)).SingleOrDefault();
        //            if (op == null)
        //            {
        //                continue;
        //            }
        //            if (!headDicTmp.ContainsKey(current.HeaderName))
        //            {
        //                headDicTmp.Add(current.MapName, new Tuple<string, int>(current.HeaderName, i++));
        //                //if (current.MapName.Equals("PRS")) { request.ProductStatus = true; }
        //                //if (current.MapName.Equals("GID")) { request.GroupID = true; }
        //                //if (current.MapName.Equals("DFV")) { request.DefaultVariant = true; }
        //                //if (current.MapName.Equals("PID")) { request.PID = true; }
        //                //if (current.MapName.Equals("PNE")) { request.ProductNameEn = true; }
        //                //if (current.MapName.Equals("PNT")) { request.ProductNameTh = true; }
        //                //if (current.MapName.Equals("SKU")) { request.SKU = true; }
        //                //if (current.MapName.Equals("UPC")) { request.UPC = true; }
        //                //if (current.MapName.Equals("BRN")) { request.BrandName = true; }
        //                //if (current.MapName.Equals("ORP")) { request.OriginalPrice = true; }
        //                //if (current.MapName.Equals("SAP")) { request.SalePrice = true; }
        //                //if (current.MapName.Equals("DCE")) { request.DescriptionEn = true; }
        //                //if (current.MapName.Equals("DCT")) { request.DescriptionTh = true; }
        //                //if (current.MapName.Equals("SDE")) { request.ShortDescriptionEn = true; }
        //                //if (current.MapName.Equals("SDT")) { request.ShortDescriptionTh = true; }
        //                //if (current.MapName.Equals("KEW")) { request.SearchTag = true; }
        //                //if (current.MapName.Equals("INA")) { request.InventoryAmount = true; }
        //                //if (current.MapName.Equals("SSA")) { request.SafetytockAmount = true; }
        //                //if (current.MapName.Equals("STT")) { request.StockType = true; }
        //                //if (current.MapName.Equals("SHM")) { request.ShippingMethod = true; }
        //                //if (current.MapName.Equals("PRT")) { request.PreparationTime = true; }
        //                //if (current.MapName.Equals("LEN")) { request.PackageLenght = true; }
        //                //if (current.MapName.Equals("HEI")) { request.PackageHeight = true; }
        //                //if (current.MapName.Equals("WID")) { request.PackageWidth = true; }
        //                //if (current.MapName.Equals("WEI")) { request.PackageWeight = true; }
        //                //if (current.MapName.Equals("GCI")) { request.GlobalCategory = true; }
        //                //if (current.MapName.Equals("AG1")) { request.GlobalCategory01 = true; }
        //                //if (current.MapName.Equals("AG2")) { request.GlobalCategory02 = true; }
        //                //if (current.MapName.Equals("LCI")) { request.LocalCategory = true; }
        //                //if (current.MapName.Equals("AL1")) { request.LocalCategory01 = true; }
        //                //if (current.MapName.Equals("AL2")) { request.LocalCategory02 = true; }
        //                //if (current.MapName.Equals("REP")) { request.RelatedProducts = true; }
        //                //if (current.MapName.Equals("MTE")) { request.MetaTitleEn = true; }
        //                //if (current.MapName.Equals("MTT")) { request.MetaTitleTh = true; }
        //                //if (current.MapName.Equals("MDE")) { request.MetaDescriptionEn = true; }
        //                //if (current.MapName.Equals("MDT")) { request.MetaDescriptionTh = true; }
        //                //if (current.MapName.Equals("MKE")) { request.MetaKeywordEn = true; }
        //                //if (current.MapName.Equals("MKT")) { request.MetaKeywordTh = true; }
        //                //if (current.MapName.Equals("PUK")) { request.ProductURLKeyEn = true; }
        //                //if (current.MapName.Equals("PBW")) { request.ProductBoostingWeight = true; }
        //                //if (current.MapName.Equals("EFD")) { request.EffectiveDate = true; }
        //                //if (current.MapName.Equals("EFT")) { request.EffectiveTime = true; }
        //                //if (current.MapName.Equals("EXD")) { request.ExpiryDate = true; }
        //                //if (current.MapName.Equals("EXT")) { request.ExpiryTime = true; }
        //                //if (current.MapName.Equals("FL1")) { request.FlagControl1 = true; }
        //                //if (current.MapName.Equals("FL2")) { request.FlagControl2 = true; }
        //                //if (current.MapName.Equals("FL3")) { request.FlagControl3 = true; }
        //                //if (current.MapName.Equals("REM")) { request.Remark = true; }
        //            }
        //        }
        //        #endregion
        //        #region Query
        //        var query = (
        //                     from mast in db.ProductStages
        //                     join variant in db.ProductStageVariants on mast.ProductId equals variant.ProductId into varJoin
        //                     from vari in varJoin.DefaultIfEmpty()
        //                         //where productIds.Contains(mast.ProductId) && mast.ShopId == shopId
        //                     select new
        //                     {
        //                         ShopId = vari != null ? vari.ShopId : mast.ShopId,
        //                         Status = vari != null ? vari.Status : mast.Status,
        //                         Sku = vari != null ? vari.Sku : mast.Sku,
        //                         Pid = vari != null ? vari.Pid : mast.Pid,
        //                         Upc = vari != null ? vari.Upc : mast.Upc,
        //                         ProductId = vari != null ? vari.ProductId : mast.ProductId,
        //                         //GroupNameEn = mast.ProductNameEn,
        //                         //GroupNameTh = mast.ProductNameTh,
        //                         ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
        //                         ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
        //                         DefaultVaraint = vari != null ? vari.DefaultVaraint == true ? "Yes" : "No" : "Yes",
        //                         ControlFlag1 = mast.ControlFlag1 == true ? "Yes" : "No",
        //                         ControlFlag2 = mast.ControlFlag2 == true ? "Yes" : "No",
        //                         ControlFlag3 = mast.ControlFlag3 == true ? "Yes" : "No",
        //                         mast.Brand.BrandNameEn,
        //                         mast.GlobalCatId,
        //                         RelatedGlobalCat = mast.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory.CategoryId),
        //                         mast.LocalCatId,
        //                         RelatedLocalCat = mast.ProductStageLocalCatMaps.Select(s => s.LocalCategory.CategoryId),
        //                         OriginalPrice = vari != null ? vari.OriginalPrice : mast.OriginalPrice,
        //                         SalePrice = vari != null ? vari.SalePrice : mast.SalePrice,
        //                         DescriptionShortEn = vari != null ? vari.DescriptionShortEn : mast.DescriptionShortEn,
        //                         DescriptionShortTh = vari != null ? vari.DescriptionShortTh : mast.DescriptionShortTh,
        //                         DescriptionFullEn = vari != null ? vari.DescriptionFullEn : mast.DescriptionFullEn,
        //                         DescriptionFullTh = vari != null ? vari.DescriptionFullTh : mast.DescriptionFullTh,
        //                         AttributeSet = new { mast.AttributeSetId, mast.AttributeSet.AttributeSetNameEn, Attribute = mast.AttributeSet.AttributeSetMaps.Select(s => s.Attribute) },
        //                         mast.PrepareDay,
        //                         Length = vari != null ? vari.Length : mast.Length,
        //                         Height = vari != null ? vari.Height : mast.Height,
        //                         Width = vari != null ? vari.Width : mast.Width,
        //                         Weight = vari != null ? vari.Weight : mast.Weight,
        //                         mast.Tag,
        //                         mast.MetaTitleEn,
        //                         mast.MetaTitleTh,
        //                         mast.MetaDescriptionEn,
        //                         mast.MetaDescriptionTh,
        //                         mast.MetaKeyEn,
        //                         mast.MetaKeyTh,
        //                         mast.UrlEn,
        //                         mast.BoostWeight,
        //                         mast.EffectiveDate,
        //                         mast.EffectiveTime,
        //                         mast.ExpiryDate,
        //                         mast.ExpiryTime,
        //                         mast.Remark,
        //                         mast.Shipping.ShippingMethodEn,
        //                         VariantAttribute = vari.ProductStageVariantArrtibuteMaps.Select(s => new
        //                         {
        //                             s.Attribute.AttributeNameEn,
        //                             Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.Value) select tt.AttributeValueEn).FirstOrDefault()
        //                                 : s.Value,
        //                         }),
        //                         MasterAttribute = mast.ProductStageAttributes.Select(s => new
        //                         {
        //                             s.AttributeId,
        //                             s.Attribute.AttributeNameEn,
        //                             ValueEn = s.IsAttributeValue ?
        //                                        (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
        //                                        : s.ValueEn,
        //                         }),
        //                         RelatedProduct = (from rel in db.ProductStageRelateds where rel.Pid1.Equals(mast.Pid) select rel.Pid2).ToList(),
        //                         Inventory = vari != null ? (from inv in db.Inventories where inv.Pid.Equals(vari.Pid) select inv).FirstOrDefault() :
        //                                      (from inv in db.Inventories where inv.Pid.Equals(mast.Pid) select inv).FirstOrDefault(),
        //                     });
        //        var productIds = request.ProductList.Select(s => s.ProductId).ToList();

        //        if (productIds != null && productIds.Count > 0)
        //        {
        //            if (productIds.Count > 2000)
        //            {
        //                throw new Exception("Too many product selected");
        //            }
        //            query = query.Where(w => productIds.Contains(w.ProductId));
        //        }
        //        if (this.User.ShopRequest() != null)
        //        {
        //            var shopId = this.User.ShopRequest().ShopId;
        //            query = query.Where(w => w.ShopId == shopId);
        //        }
        //        var productList = query.ToList();

        //        #endregion
        //        List<List<string>> rs = new List<List<string>>();
        //        List<string> bodyList = null;
        //        if (request.AttributeSets != null && request.AttributeSets.Count > 0)
        //        {
        //            headDicTmp.Add("ATS", new Tuple<string, int>("Attribute Set", i++));
        //            headDicTmp.Add("VO1", new Tuple<string, int>("Variation Option 1", i++));
        //            headDicTmp.Add("VO2", new Tuple<string, int>("Variation Option 2", i++));
        //        }
        //        foreach (var p in productList)
        //        {
        //            bodyList = new List<string>(new string[headDicTmp.Count]);
        //            #region Assign Value
        //            if (request.ProductStatus)
        //            {
        //                if (Constant.PRODUCT_STATUS_DRAFT.Equals(p.Status))
        //                {
        //                    bodyList[headDicTmp["PRS"].Item2] = "Draft";
        //                }
        //                else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(p.Status))
        //                {
        //                    bodyList[headDicTmp["PRS"].Item2] = "Wait for Approval";
        //                }
        //                else if (Constant.PRODUCT_STATUS_APPROVE.Equals(p.Status))
        //                {
        //                    bodyList[headDicTmp["PRS"].Item2] = "Approve";
        //                }
        //                else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(p.Status))
        //                {
        //                    bodyList[headDicTmp["PRS"].Item2] = "Not Approve";
        //                }
        //            }
        //            if (request.SKU)
        //            {
        //                bodyList[headDicTmp["SKU"].Item2] = p.Sku;
        //            }
        //            if (request.PID)
        //            {
        //                bodyList[headDicTmp["PID"].Item2] = p.Pid;
        //            }
        //            if (request.UPC)
        //            {
        //                bodyList[headDicTmp["UPC"].Item2] = p.Upc;
        //            }
        //            if (request.GroupID)
        //            {
        //                bodyList[headDicTmp["GID"].Item2] = string.Concat(p.ProductId);
        //            }
        //            if (request.DefaultVariant)
        //            {
        //                bodyList[headDicTmp["DFV"].Item2] = p.DefaultVaraint;
        //            }
        //            if (request.ProductNameEn)
        //            {
        //                bodyList[headDicTmp["PNE"].Item2] = p.ProductNameEn;
        //            }
        //            if (request.ProductNameTh)
        //            {
        //                bodyList[headDicTmp["PNT"].Item2] = p.ProductNameTh;
        //            }
        //            if (request.BrandName)
        //            {
        //                bodyList[headDicTmp["BRN"].Item2] = p.BrandNameEn;
        //            }
        //            if (request.GlobalCategory)
        //            {
        //                bodyList[headDicTmp["GCI"].Item2] = string.Concat(p.GlobalCatId);
        //            }
        //            if (request.GlobalCategory01)
        //            {
        //                if (p.RelatedGlobalCat != null && p.RelatedGlobalCat.ToList().Count > 0)
        //                {
        //                    bodyList[headDicTmp["AG1"].Item2] = string.Concat(p.RelatedGlobalCat.ToList()[0]);
        //                }
        //            }
        //            if (request.GlobalCategory02)
        //            {
        //                if (p.RelatedGlobalCat != null && p.RelatedGlobalCat.ToList().Count > 1)
        //                {
        //                    bodyList[headDicTmp["AG2"].Item2] = string.Concat(p.RelatedGlobalCat.ToList()[1]);
        //                }
        //            }
        //            if (request.LocalCategory)
        //            {
        //                bodyList[headDicTmp["LCI"].Item2] = string.Concat(p.LocalCatId);
        //            }
        //            if (request.LocalCategory01)
        //            {
        //                if (p.RelatedLocalCat != null && p.RelatedLocalCat.ToList().Count > 0)
        //                {
        //                    bodyList[headDicTmp["AL1"].Item2] = string.Concat(p.RelatedLocalCat.ToList()[0]);
        //                }
        //            }
        //            if (request.LocalCategory02)
        //            {
        //                if (p.RelatedLocalCat != null && p.RelatedLocalCat.ToList().Count > 1)
        //                {
        //                    bodyList[headDicTmp["AL2"].Item2] = string.Concat(p.RelatedLocalCat.ToList()[1]);
        //                }
        //            }
        //            if (request.OriginalPrice)
        //            {
        //                bodyList[headDicTmp["ORP"].Item2] = string.Concat(p.OriginalPrice);
        //            }
        //            if (request.SalePrice)
        //            {
        //                bodyList[headDicTmp["SAP"].Item2] = string.Concat(p.SalePrice);
        //            }
        //            if (request.DescriptionEn)
        //            {
        //                bodyList[headDicTmp["DCE"].Item2] = p.DescriptionFullEn;
        //            }
        //            if (request.DescriptionTh)
        //            {
        //                bodyList[headDicTmp["DCT"].Item2] = p.DescriptionFullTh;
        //            }
        //            if (request.ShortDescriptionEn)
        //            {
        //                bodyList[headDicTmp["SDE"].Item2] = p.DescriptionShortEn;
        //            }
        //            if (request.ShortDescriptionTh)
        //            {
        //                bodyList[headDicTmp["SDT"].Item2] = p.DescriptionShortTh;
        //            }
        //            if (request.PreparationTime)
        //            {
        //                bodyList[headDicTmp["PRT"].Item2] = string.Concat(p.PrepareDay);
        //            }
        //            if (request.PackageLenght)
        //            {
        //                bodyList[headDicTmp["LEN"].Item2] = string.Concat(p.Length);
        //            }
        //            if (request.PackageHeight)
        //            {
        //                bodyList[headDicTmp["HEI"].Item2] = string.Concat(p.Height);
        //            }
        //            if (request.PackageWidth)
        //            {
        //                bodyList[headDicTmp["WID"].Item2] = string.Concat(p.Width);
        //            }
        //            if (request.PackageWeight)
        //            {
        //                bodyList[headDicTmp["WEI"].Item2] = string.Concat(p.Weight);
        //            }

        //            if (request.InventoryAmount)
        //            {
        //                if (p.Inventory != null)
        //                {
        //                    bodyList[headDicTmp["INA"].Item2] = string.Concat(p.Inventory.Quantity);
        //                }
        //                else
        //                {
        //                    bodyList[headDicTmp["INA"].Item2] = string.Empty;
        //                }
        //            }
        //            if (request.StockType)
        //            {
        //                if (p.Inventory != null)
        //                {
        //                    bodyList[headDicTmp["STT"].Item2] = Constant.STOCK_TYPE.Where(w => w.Value.Equals(p.Inventory.StockAvailable)).SingleOrDefault().Key;
        //                }
        //                else
        //                {
        //                    bodyList[headDicTmp["STT"].Item2] = string.Empty;
        //                }
        //            }
        //            if (request.ShippingMethod)
        //            {
        //                bodyList[headDicTmp["SHM"].Item2] = p.ShippingMethodEn;
        //            }
        //            if (request.SafetytockAmount)
        //            {
        //                if (p.Inventory != null)
        //                {
        //                    bodyList[headDicTmp["SSA"].Item2] = string.Concat(p.Inventory.SafetyStockSeller);
        //                }
        //                else
        //                {
        //                    bodyList[headDicTmp["SSA"].Item2] = string.Empty;
        //                }
        //            }
        //            if (request.SearchTag)
        //            {
        //                bodyList[headDicTmp["KEW"].Item2] = p.Tag;
        //            }
        //            if (request.RelatedProducts)
        //            {
        //                if (p.RelatedProduct != null && p.RelatedProduct.Count > 0)
        //                {
        //                    bodyList[headDicTmp["REP"].Item2] = string.Join(",", p.RelatedProduct);
        //                }
        //                else
        //                {
        //                    bodyList[headDicTmp["REP"].Item2] = string.Empty;
        //                }
        //            }
        //            if (request.MetaTitleEn)
        //            {
        //                bodyList[headDicTmp["MTE"].Item2] = p.MetaTitleEn;
        //            }
        //            if (request.MetaTitleTh)
        //            {
        //                bodyList[headDicTmp["MTT"].Item2] = p.MetaTitleTh;
        //            }
        //            if (request.MetaDescriptionEn)
        //            {
        //                bodyList[headDicTmp["MDE"].Item2] = p.MetaDescriptionEn;
        //            }
        //            if (request.MetaDescriptionTh)
        //            {
        //                bodyList[headDicTmp["MDT"].Item2] = p.MetaDescriptionTh;
        //            }
        //            if (request.MetaKeywordEn)
        //            {
        //                bodyList[headDicTmp["MKE"].Item2] = p.MetaKeyEn;
        //            }
        //            if (request.MetaKeywordTh)
        //            {
        //                bodyList[headDicTmp["MKT"].Item2] = p.MetaKeyTh;
        //            }
        //            if (request.ProductURLKeyEn)
        //            {
        //                bodyList[headDicTmp["PUK"].Item2] = p.UrlEn;
        //            }
        //            if (request.ProductBoostingWeight)
        //            {
        //                bodyList[headDicTmp["PBW"].Item2] = string.Concat(p.BoostWeight);
        //            }
        //            if (request.EffectiveDate)
        //            {
        //                if (p.ExpiryDate != null)
        //                {
        //                    bodyList[headDicTmp["EFD"].Item2] = p.ExpiryDate.ToString();
        //                }
        //            }
        //            if (request.EffectiveTime)
        //            {
        //                if (p.EffectiveTime != null)
        //                {
        //                    bodyList[headDicTmp["EFT"].Item2] = p.EffectiveTime.ToString();
        //                }
        //            }

        //            if (request.ExpiryDate)
        //            {
        //                if (p.ExpiryDate != null)
        //                {
        //                    bodyList[headDicTmp["EXD"].Item2] = p.ExpiryDate.ToString();
        //                }
        //            }
        //            if (request.ExpiryTime)
        //            {
        //                if (p.ExpiryTime != null)
        //                {
        //                    bodyList[headDicTmp["EXT"].Item2] = p.ExpiryTime.ToString();
        //                }
        //            }
        //            if (request.Remark)
        //            {
        //                bodyList[headDicTmp["REM"].Item2] = p.Remark;
        //            }
        //            if (request.FlagControl1)
        //            {
        //                bodyList[headDicTmp["FL1"].Item2] = p.ControlFlag1;
        //            }
        //            if (request.FlagControl2)
        //            {
        //                bodyList[headDicTmp["FL2"].Item2] = p.ControlFlag2;
        //            }
        //            if (request.FlagControl3)
        //            {
        //                bodyList[headDicTmp["FL3"].Item2] = p.ControlFlag3;
        //            }

        //            #endregion

        //            #region Attibute Section
        //            if (request.AttributeSets != null && request.AttributeSets.Count > 0)
        //            {
        //                if (p.AttributeSet != null)
        //                {
        //                    var set = request.AttributeSets.Where(w => w.AttributeSetId == p.AttributeSet.AttributeSetId).SingleOrDefault();
        //                    if (set != null)
        //                    {
        //                        //make header for attribute
        //                        foreach (var attr in p.AttributeSet.Attribute)
        //                        {
        //                            if (!headDicTmp.ContainsKey(attr.AttributeNameEn))
        //                            {
        //                                headDicTmp.Add(attr.AttributeNameEn, new Tuple<string, int>(attr.AttributeNameEn, i++));
        //                                bodyList.Add(string.Empty);
        //                            }
        //                        }

        //                        bodyList[headDicTmp["ATS"].Item2] = p.AttributeSet.AttributeSetNameEn;
        //                        //make vaiant option 1 value
        //                        if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
        //                        {
        //                            bodyList[headDicTmp["VO1"].Item2] = p.VariantAttribute.ToList()[0].AttributeNameEn;
        //                        }
        //                        //make vaiant option 2 value
        //                        if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 1)
        //                        {
        //                            bodyList[headDicTmp["VO2"].Item2] = p.VariantAttribute.ToList()[1].AttributeNameEn;
        //                        }
        //                        //make master attribute value
        //                        if (p.MasterAttribute != null && p.MasterAttribute.ToList().Count > 0)
        //                        {
        //                            foreach (var masterValue in p.MasterAttribute)
        //                            {
        //                                if (headDicTmp.ContainsKey(masterValue.AttributeNameEn))
        //                                {
        //                                    int desColumn = headDicTmp[masterValue.AttributeNameEn].Item2;
        //                                    for (int j = bodyList.Count; j <= desColumn; j++)
        //                                    {
        //                                        bodyList.Add(string.Empty);
        //                                    }
        //                                    bodyList[desColumn] = masterValue.ValueEn;
        //                                }
        //                            }
        //                        }
        //                        if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
        //                        {
        //                            foreach (var variantValue in p.VariantAttribute)
        //                            {
        //                                if (headDicTmp.ContainsKey(variantValue.AttributeNameEn))
        //                                {
        //                                    int desColumn = headDicTmp[variantValue.AttributeNameEn].Item2;
        //                                    for (int j = bodyList.Count; j <= desColumn; j++)
        //                                    {
        //                                        bodyList.Add(string.Empty);
        //                                    }
        //                                    bodyList[desColumn] = variantValue.Value;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }


        //            #endregion
        //            rs.Add(bodyList);
        //        }

        //        #region Write header

        //        stream = new MemoryStream();
        //        writer = new StreamWriter(stream, Encoding.UTF8);
        //        var csv = new CsvWriter(writer);
        //        string headers = string.Empty;
        //        foreach (KeyValuePair<string, Tuple<string, int>> entry in headDicTmp)
        //        {
        //            csv.WriteField(entry.Value.Item1);
        //        }
        //        csv.NextRecord();
        //        #endregion
        //        #region Write body
        //        foreach (List<string> r in rs)
        //        {
        //            foreach (string field in r)
        //            {
        //                csv.WriteField(field);
        //            }
        //            csv.NextRecord();
        //        }
        //        #endregion
        //        #region Create Response
        //        writer.Flush();
        //        stream.Position = 0;

        //        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
        //        result.Content = new StreamContent(stream);
        //        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
        //        {
        //            CharSet = Encoding.UTF8.WebName
        //        };
        //        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //        result.Content.Headers.ContentDisposition.FileName = "file.csv";
        //        #endregion
        //        return result;

        //    }
        //    catch (Exception e)
        //    {
        //        #region close writer
        //        if (writer != null)
        //        {
        //            writer.Close();
        //            writer.Dispose();
        //        }
        //        if (stream != null)
        //        {
        //            stream.Close();
        //            stream.Dispose();
        //        }
        //        #endregion
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
        //    }
        //}

        [Route("api/ProductStages/Master/{productId}")]
        [HttpPut]
        public HttpResponseMessage SaveMasterProduct([FromUri]long productId, MasterProductRequest request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var masterProduct = db.ProductStageMasters.Where(w => w.MasterPid.Equals(request.MasterProduct.Pid)).ToList();
                if(masterProduct == null || masterProduct.Count == 0)
                {
                    throw new Exception("Cannot find master product");
                }
                foreach(var child in request.ChildProducts)
                {
                    bool isNew = false;
                    if(masterProduct == null || masterProduct.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = masterProduct.Where(w => w.ChildPid.Equals(child.Pid)).SingleOrDefault();
                        if(current != null)
                        {
                            masterProduct.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        db.ProductStageMasters.Add(new ProductStageMaster()
                        {
                            MasterPid = masterProduct[0].MasterPid,
                            ChildPid = child.Pid,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
                        });
                    }
                }
                if(masterProduct != null && masterProduct.Count > 0)
                {
                    db.ProductStageMasters.RemoveRange(masterProduct);
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStageMaster");
                return GetMasterProduct(productId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Master")]
        [HttpPost]
        public HttpResponseMessage AddMasterProduct(MasterProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invaide request");
                }
                var tmpStage = GetProductStageRequestFromId(db,request.MasterProduct.ProductId);
                tmpStage.Variants = new List<VariantRequest>();
                tmpStage.AdminApprove.Information = Constant.PRODUCT_STATUS_APPROVE;
                tmpStage.AdminApprove.Image = Constant.PRODUCT_STATUS_APPROVE;
                tmpStage.AdminApprove.Category = Constant.PRODUCT_STATUS_APPROVE;
                tmpStage.AdminApprove.MoreOption = Constant.PRODUCT_STATUS_APPROVE;
                tmpStage.AdminApprove.Variation = Constant.PRODUCT_STATUS_APPROVE;
                tmpStage.Status = Constant.PRODUCT_STATUS_APPROVE;
                ProductStageGroup group = SetupProduct(db, tmpStage,0);
                if (string.IsNullOrEmpty(request.MasterProduct.Pid))
                {
                    throw new Exception("Pid is required for master");
                }
                group.ProductStages.ElementAt(0).ProductStageMasters1.Add(new ProductStageMaster()
                {
                    ChildPid = request.MasterProduct.Pid,
                    CreatedBy = this.User.UserRequest().Email,
                    CreatedDt = DateTime.Now,
                    UpdatedBy = this.User.UserRequest().Email,
                    UpdatedDt = DateTime.Now
                });
                foreach (var child in request.ChildProducts)
                {
                    if (string.IsNullOrEmpty(child.Pid))
                    {
                        throw new Exception("Pid is required for child");
                    }
                    group.ProductStages.ElementAt(0).ProductStageMasters1.Add(new ProductStageMaster()
                    {
                        ChildPid = child.Pid,
                        CreatedBy = this.User.UserRequest().Email,
                        CreatedDt = DateTime.Now,
                        UpdatedBy = this.User.UserRequest().Email,
                        UpdatedDt = DateTime.Now
                    });
                }

                group.ProductStages.ToList().ForEach(e => { e.Pid = null; e.UrlEn = null; e.IsMaster = true; e.IsVariant = false; });
                AutoGenerate.GeneratePid(db, group.ProductStages);
                group.ProductId = db.GetNextProductStageGroupId().Single().Value;
                db.ProductStageGroups.Add(group);
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return GetMasterProduct(group.ProductId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Master/{productId}")]
        [HttpGet]
        public HttpResponseMessage GetMasterProduct(long productId)
        {
            try
            {
                var master = (from mast in db.ProductStageMasters
                          join stage in db.ProductStages on mast.MasterPid equals stage.Pid
                          join child in db.ProductStages on mast.ChildPid equals child.Pid
                          where stage.ProductId == productId
                          group mast by mast.MasterPid into masterGroup
                          select new
                          {
                              MasterProduct = new
                              {
                                  masterGroup.FirstOrDefault().ProductStage1.ProductId,
                                  masterGroup.FirstOrDefault().ProductStage1.ProductNameEn,
                                  masterGroup.FirstOrDefault().ProductStage1.Pid
                               },
                              ChildProducts = masterGroup.Select(s=> new
                              {
                                  s.ProductStage.ProductId,
                                  s.ProductStage.Pid,
                                  s.ProductStage.ProductNameEn
                              }),

                          }).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, master);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ProductStages/Master")]
        [HttpGet]
        public HttpResponseMessage GetMasterProduct([FromUri]ProductRequest request)
        {
            try
            {

                var master = (from mast in db.ProductStageMasters
                              join stage in db.ProductStages on mast.MasterPid equals stage.Pid
                              group mast by mast.MasterPid into masterGroup
                              select new
                              {
                                  masterGroup.FirstOrDefault().ProductStage1.ProductId,
                                  masterGroup.FirstOrDefault().ProductStage1.ProductNameEn,
                                  masterGroup.FirstOrDefault().ProductStage1.Pid,
                                  ChildPids = masterGroup.Select(s => new
                                  {
                                      s.ProductStage.Pid
                                  }),

                              });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, master);
                }
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
                var guidance = db.ImportHeaders.OrderBy(o => o.ImportHeaderId).ToList();
                List<string> header = new List<string>();
                foreach (var g in guidance)
                {
                    header.Add(g.HeaderName);
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
                var groupList = db.ProductStageGroups.Where(w => productIds.Contains(w.ProductId)).Include(i=>i.ProductStages).ToList();
                foreach (ProductStageRequest proRq in request)
                {
                    var pro = groupList.Where(w => w.ProductId == proRq.ProductId).SingleOrDefault();
                    if (pro == null)
                    {
                        throw new Exception("Cannot find deleted product");
                    }
                    pro.Status = Constant.PRODUCT_STATUS_APPROVE;
                    pro.ProductStages.ToList().ForEach(e => e.Status = Constant.PRODUCT_STATUS_APPROVE);
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                return Request.CreateResponse(HttpStatusCode.OK);
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
                var guidance = db.ImportHeaders.Where(w => w.HeaderName.Contains(SearchText)).Select(s => new ImportHeaderRequest()
                {
                    HeaderName = s.HeaderName,
                    Description = s.Description,
                    Example = s.Example,
                    GroupName = s.GroupName,
                    Note = s.Note,
                    IsAttribute = false
                }).Take(_limit).ToList();
                if (guidance != null && guidance.Count < _limit)
                {
                    var attribute = db.Attributes.Where(w => w.AttributeNameEn.Contains(SearchText)).Select(s => new ImportHeaderRequest()
                    {
                        HeaderName = s.AttributeNameEn,
                        GroupName = s.AttributeNameEn,
                        IsVariant = s.VariantStatus,
                        AttributeType = s.DataType,
                        IsAttribute = true,
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
                int shopId = this.User.ShopRequest().ShopId;
                var pids = request.Select(s => s.Pid).ToList();
                var products = db.ProductStages.Where(w => w.ShopId == shopId && pids.Contains(w.Pid)).Include(i => i.ProductStageImages).ToList();
                var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
                var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;
                foreach (VariantRequest varRq in request)
                {
                    var pro = products.Where(w => w.Pid.Equals(varRq.Pid)).SingleOrDefault();
                    if (pro.IsVariant)
                    {
                        SetupImage(pro, varRq.VariantImg, db);
                    }
                    else
                    {
                        SetupImage(pro, varRq.MasterImg, db);
                    }
                    
                    SetupStageAfterSave(pro, schema, imageUrl);
                    
                    if (pro != null && Constant.PRODUCT_STATUS_APPROVE.Equals(pro.Status))
                    {
                        pro.Status = Constant.PRODUCT_STATUS_DRAFT;
                    }
                }
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
               int shopId = this.User.ShopRequest().ShopId;

                var products = (
                             from productStage in db.ProductStages
                             where productStage.ShopId == shopId && 
                             (productStage.VariantCount == 0 || productStage.IsVariant == true) && productStage.IsMaster == false
                             select new
                             {
                                 productStage.ProductId,
                                 productStage.Sku,
                                 productStage.Upc,
                                 productStage.ProductNameEn,
                                 productStage.ProductNameTh,
                                 productStage.Pid,
                                 productStage.Status,
                                 MasterImg = productStage.ProductStageImages.Select(s => new { ImageId = s.ImageId, url = s.ImageUrlEn, position = s.Position }).OrderBy(o => o.position),
                                 VariantImg = productStage.ProductStageImages.Select(s => new { ImageId = s.ImageId, url = s.ImageUrlEn, position = s.Position }).OrderBy(o => o.position),
                                 productStage.IsVariant,
                                 productStage.ProductStageComments.FirstOrDefault().Comment,
                                 VariantAttribute = productStage.ProductStageAttributes.Select(s => new
                                 {
                                     s.Attribute.AttributeNameEn,
                                     Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
                                         : s.ValueEn,
                                 })
                             });
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
                                    p.Shop.ShopNameEn,
                                    p.Status,
                                    p.ImageFlag,
                                    p.InfoFlag,
                                    p.Visibility,
                                    p.ProductStages.FirstOrDefault().VariantCount,
                                    ImageUrl = p.ProductStages.FirstOrDefault().FeatureImgUrl,
                                    GlobalCategory = p.GlobalCategory != null ? new { p.GlobalCategory.CategoryId, p.GlobalCategory.NameEn, p.GlobalCategory.Lft, p.GlobalCategory.Rgt } : null,
                                    LocalCategory = p.LocalCategory != null ? new { p.LocalCategory.CategoryId, p.LocalCategory.NameEn, p.LocalCategory.Lft, p.LocalCategory.Rgt } : null,
                                    Brand = p.Brand != null ? new { p.Brand.BrandId, p.Brand.BrandNameEn } : null,
                                    Tag = p.ProductStageTags.Select(s=>s.Tag),
                                    p.CreatedDt,
                                    p.UpdatedDt,
                                    //PriceTo = p.SalePrice < p.ProductStageVariants.Max(m => m.SalePrice)
                                    //        ? p.ProductStageVariants.Max(m => m.SalePrice) : p.SalePrice,
                                    //PriceFrom = p.SalePrice < p.ProductStageVariants.Min(m => m.SalePrice)
                                    //        ? p.SalePrice : p.ProductStageVariants.Min(m => m.SalePrice),
                                    Shop = new { p.Shop.ShopId, p.Shop.ShopNameEn }
                                });
                //check if its seller permission
                if (this.User.HasPermission("View Product"))
                {
                    //add shopid criteria for seller request
                    int shopId = this.User.ShopRequest().ShopId;
                    products = products.Where(w => w.Shop.ShopId == shopId);
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
                    var lft = request.GlobalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
                    var rgt = request.GlobalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();
                    if (lft != null && lft.Count > 0 && rgt != null && rgt.Count > 0)
                    {
                        products = products.Where(w => lft.Any(a => a <= w.GlobalCategory.Lft) && rgt.Any(a => a >= w.GlobalCategory.Rgt));
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
                    var lft = request.LocalCategories.Where(w => w.Lft != 0).Select(s => s.Lft).ToList();
                    var rgt = request.LocalCategories.Where(w => w.Rgt != 0).Select(s => s.Rgt).ToList();

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
                    products = products.Where(w => request.Tags.Any(a =>w.Tag.Contains(a)));
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

        [Route("api/ProductStages")]
        [HttpGet]
        [ClaimsAuthorize(Permission = new string[] { "View Product", "View All Product" })]
        public HttpResponseMessage GetProductStages([FromUri] ProductRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var products = (from p in db.ProductStageGroups
                                where p.ProductStages.Any(a=>a.IsVariant == false && a.IsMaster == false)
                                select new
                                {
                                    p.ProductStages.FirstOrDefault().Sku,
                                    p.ProductStages.FirstOrDefault().Pid,
                                    p.ProductStages.FirstOrDefault().Upc,
                                    p.ProductId,
                                    p.ProductStages.FirstOrDefault().ProductNameEn,
                                    p.ProductStages.FirstOrDefault().ProductNameTh,
                                    p.ProductStages.FirstOrDefault().OriginalPrice,
                                    p.ProductStages.FirstOrDefault().SalePrice,
                                    p.Status,
                                    p.ImageFlag,
                                    p.InfoFlag,
                                    p.Visibility,
                                    p.ProductStages.FirstOrDefault().VariantCount,
                                    ImageUrl = p.ProductStages.FirstOrDefault().FeatureImgUrl,
                                    p.GlobalCatId,
                                    p.LocalCatId,
                                    p.AttributeSetId,
                                    p.ProductStages.FirstOrDefault().ProductStageAttributes,
                                    p.UpdatedDt,
                                    p.ShopId,
                                    p.InformationTabStatus,
                                    p.ImageTabStatus,
                                    p.CategoryTabStatus,
                                    p.VariantTabStatus,
                                    p.MoreOptionTabStatus,
                                    Tags = p.ProductStageTags.Select(s=>s.Tag),
                                    //PriceTo = p.ProductStageVariants.Max(m => m.SalePrice),
                                    //PriceFrom = p.ProductStageVariants.Min(m => m.SalePrice),
                                    //PriceTo = p.ProductStageVariants.Count == 0 ?  p.SalePrice :
                                    //        p.SalePrice < p.ProductStageVariants.Max(m => m.SalePrice)
                                    //        ? p.ProductStageVariants.Where(w => true).Max(m => m.SalePrice) : p.SalePrice,
                                    //PriceFrom = p.SalePrice < p.ProductStageVariants.Where(w => true).Min(m => m.SalePrice)
                                    //        ? p.SalePrice : p.ProductStageVariants.Where(w => true).Min(m => m.SalePrice),
                                    Shop = new { p.Shop.ShopId, p.Shop.ShopNameEn }
                                });
                if (this.User.HasPermission("View Product"))
                {
                    int shopId = this.User.ShopRequest().ShopId;
                    products = products.Where(w => w.ShopId == shopId);
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
        [HttpPut]
        public HttpResponseMessage SaveChangeProduct([FromUri]long productId, ProductStageRequest request)
        {
            try
            {

                #region Query
                var tmpProduct = db.ProductStageGroups.Where(w => w.ProductId == productId)
                    .Include(i => i.ProductStages.Select(s => s.ProductStageAttributes.Select(sa => sa.Attribute.AttributeValueMaps.Select(sv => sv.AttributeValue))))
                    .Include(i => i.ProductStages.Select(s => s.Inventory))
                    .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
                    .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
                    .Include(i => i.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory))
                    .Include(i => i.ProductStageLocalCatMaps.Select(s => s.LocalCategory))
                    .Include(i=>i.ProductStageTags)
                    .Include(i=>i.ProductStageRelateds1);
                ProductStageGroup group = null;
                if (this.User.HasPermission("Edit Product"))
                {
                    group = tmpProduct.SingleOrDefault();
                }
                else if(this.User.ShopRequest() != null)
                {
                    var shopId = this.User.ShopRequest().ShopId;
                    group = tmpProduct.Where(w => w.ShopId == shopId).SingleOrDefault();
                }
                else
                {
                    throw new Exception("Invalid permission");
                }

                //var product = db.ProductStageGroups.Where(w => w.ShopId == shopId
                //    && w.ProductId == productId)
                //    .Include(i => i.ProductStages.Select(s => s.ProductStageAttributes.Select(sa=>sa.Attribute.AttributeValueMaps.Select(sv=>sv.AttributeValue))))
                //    .Include(i => i.ProductStages.Select(s => s.Inventory))
                //    .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
                //    .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
                //    .Include(i => i.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory))
                //    .Include(i => i.ProductStageLocalCatMaps.Select(s => s.LocalCategory)).SingleOrDefault();
                if(group == null)
                {
                    throw new Exception("Cannot find product "+ productId);
                }
                var attributeList = db.Attributes.Include(i => i.AttributeValueMaps).ToList();
                var shippingList = db.Shippings.ToList();
                #endregion
                string email = this.User.UserRequest().Email;
                #region Setup Group
                SetupGroup(group, request,false,email, User.HasPermission("Approve product"), User.HasPermission("Add Product"), db);
                #endregion
                #region Master Variant
                var masterVariant = group.ProductStages.Where(w=>w.IsVariant==false).FirstOrDefault();
                request.MasterVariant.Status = group.Status;
                SetupVariant(masterVariant, request.MasterVariant, false, email, User.HasPermission("Approve product"), User.HasPermission("Add Product"), db, shippingList);
                SetupAttribute(masterVariant, request.MasterAttribute, attributeList, email, db);
                SetupAttribute(masterVariant, request.DefaultAttributes, attributeList, email, db);
                #endregion
                #region Variants
                var tmpVariant = group.ProductStages.Where(w => w.IsVariant == true).ToList();
                if (request.Variants != null)
                {
                    masterVariant.VariantCount = request.Variants.Count;
                    foreach (var variantRq in request.Variants)
                    {
                        bool isNew = false;
                        ProductStage variant = null;
                        if (tmpVariant == null || tmpVariant.Count == 0)
                        {
                            isNew = true;
                        }
                        if (!isNew)
                        {
                            var current = tmpVariant.Where(w => w.Pid.Equals(variantRq.Pid)).SingleOrDefault();
                            if(current != null)
                            {
                                variant = current;
                                tmpVariant.Remove(current);
                            }
                            else
                            {
                                isNew = true;
                            }
                        }
                        if (isNew)
                        {
                            variant = new ProductStage();
                            variant.ShopId = masterVariant.ShopId;
                            variant.IsVariant = true;
                            variant.CreatedBy = email;
                            variant.CreatedDt = DateTime.Now;
                            variant.UpdatedBy = email;
                            variant.UpdatedDt = DateTime.Now;
                            group.ProductStages.Add(variant);
                        }
                        variantRq.Status = group.Status;
                        SetupVariant(variant, variantRq, false, email, User.HasPermission("Approve product"), User.HasPermission("Add Product"), db, shippingList);
                        SetupAttribute(variant, new List<AttributeRequest>() { variantRq.FirstAttribute, variantRq.SecondAttribute }, attributeList, email, db);
                    }
                }
                else
                {
                    masterVariant.VariantCount = 0;
                }
                if(tmpVariant != null && tmpVariant.Count > 0)
                {
                    db.ProductStages.RemoveRange(tmpVariant);
                }
                #endregion
                AutoGenerate.GeneratePid(db, group.ProductStages);
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                SetupGroupAfterSave(group);
                Util.DeadlockRetry(db.SaveChanges, "ProductStageImage");
                return GetProductStage(group.ProductId);
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
            
            try
            {
                #region Validation
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                if (this.User.ShopRequest() == null)
                {
                    throw new Exception("Shop is required");
                }
                #endregion
                var shopId = this.User.ShopRequest().ShopId;
                ProductStageGroup group = SetupProduct(db, request,shopId);
                AutoGenerate.GeneratePid(db, group.ProductStages);
                group.ProductId = db.GetNextProductStageGroupId().Single().Value;
                db.ProductStageGroups.Add(group);
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                SetupGroupAfterSave(group,db,true);
                Util.DeadlockRetry(db.SaveChanges, "ProductStageImage");
                return GetProductStage(group.ProductId) ;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, ex.Message);
            }
        }

        [Route("api/ProductStages/{productId}")]
        [HttpGet]
        public HttpResponseMessage GetProductStage(long productId)
        {
            try
            {
                var response = GetProductStageRequestFromId(db, productId);

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        //duplicate
        [Route("api/ProductStages/{productId}")]
        [HttpPost]
        public HttpResponseMessage DuplicateProductStage(long productId)
        {
            try
            {
                if (this.User.ShopRequest() == null)
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
                }
                if (response.Variants != null)
                {
                    response.Variants.Where(w => w.SEO != null).ToList().ForEach(f => { f.SEO.ProductUrlKeyEn = null; f.Pid = null; });
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
                int shopId = this.User.ShopRequest().ShopId;
                var ids = request.Select(s => s.ProductId).ToList();
                var producGrouptList = db.ProductStageGroups.Where(w => w.ShopId == shopId && ids.Contains(w.ProductId))
                    .Include(i=>i.ProductStages.Select(s=>s.Inventory))
                    .Include(i=>i.ProductStages.Select(s=>s.ProductStageMasters))
                    .Include(i=>i.ProductStageRelateds)
                    ;
                //var productApprovedList = db.Products.Where(w => w.ShopId == shopId);
                //var relatedProductList = db.ProductStageRelateds.Where(w => w.ShopId == shopId && (ids.Contains(w.Parent)||ids.Contains(w.Child)));
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
                        db.InventoryHistories.Add(new InventoryHistory()
                        {
                            Pid = inventory.Pid,
                            StockAvailable = inventory.StockAvailable,
                            Defect = inventory.Defect,
                            MaxQuantity = inventory.MaxQuantity,
                            MinQuantity = inventory.MinQuantity,
                            OnHold = inventory.OnHold,
                            Quantity = inventory.Quantity,
                            Reserve = inventory.Reserve,
                            SafetyStockAdmin = inventory.SafetyStockAdmin,
                            SafetyStockSeller = inventory.SafetyStockSeller,
                            Status = Constant.INVENTORY_STATUS_DELETE,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                    db.ProductStageRelateds.RemoveRange(productGroup.ProductStageRelateds);
                    foreach(var stage in productGroup.ProductStages)
                    {
                        db.ProductStageMasters.RemoveRange(stage.ProductStageMasters);
                    }
                    //var tmpRelatedProduct = relatedProductList.Where(w => w.Parent == productGroup.ProductId || w.Child == productGroup.ProductId);
                    //if (tmpRelatedProduct != null && tmpRelatedProduct.ToList().Count > 0)
                    //{
                    //    db.ProductStageRelateds.RemoveRange(tmpRelatedProduct);
                    //}
                    //var productApproved = productApprovedList.Where(w => pids.Contains(w.Pid)).ToList();
                    //productApproved.ForEach(e => e.Status = Constant.STATUS_REMOVE);
                    db.ProductStageGroups.Remove(productGroup);
                }
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
                if (User.HasPermission("Tag Management"))
                {

                }
                else if (User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    productList = productList.Where(w => w.ShopId == shopId);
                }
                else
                {
                    throw new Exception("Has no permission");
                }
                foreach (var product in productList)
                {
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
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
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
                FileUploadRespond fileUpload = await Util.SetupImage(Request
                    , AppSettingKey.IMAGE_ROOT_PATH
                    , AppSettingKey.PRODUCT_FOLDER
                    , 1500, 1500, 2000, 2000, 5, true);
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private ProductStageGroup SetupProduct(ColspEntities db, ProductStageRequest request, int shopId)
        {
            ProductStageGroup group = null;
            var attributeList = db.Attributes.Include(i => i.AttributeValueMaps).ToList();
            var shippingList = db.Shippings.ToList();
            string email = this.User.UserRequest().Email;
            #region Setup Group
            group = new ProductStageGroup();
            group.ShopId = shopId;
            SetupGroup(group, request, true, email, User.HasPermission("Approve product"), User.HasPermission("Add Product"), db);
            #endregion
            #region Master Variant
            var masterVariant = new ProductStage();
            request.MasterVariant.Status = group.Status;
            masterVariant.ShopId = shopId;
            masterVariant.IsVariant = false;
            SetupVariant(masterVariant, request.MasterVariant, true, email, User.HasPermission("Approve product"), User.HasPermission("Add Product"), db, shippingList);
            SetupAttribute(masterVariant, request.MasterAttribute, attributeList, email, db);
            SetupAttribute(masterVariant, request.DefaultAttributes, attributeList, email, db);
            group.ProductStages.Add(masterVariant);
            #endregion
            #region Variants
            if (request.Variants != null)
            {
                masterVariant.VariantCount = request.Variants.Count;
                foreach (var variantRq in request.Variants)
                {
                    var variant = new ProductStage();
                    variant.ShopId = shopId;
                    variant.IsVariant = true;
                    variant.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL });
                    SetupVariant(variant, variantRq, true, email, User.HasPermission("Approve product"), User.HasPermission("Add Product"), db, shippingList);
                    SetupAttribute(variant, new List<AttributeRequest>() { variantRq.FirstAttribute, variantRq.SecondAttribute }, attributeList, email, db);
                    group.ProductStages.Add(variant);
                }
            }
            else
            {
                masterVariant.VariantCount = 0;
            }
            #endregion
            return group;
        }

        private void SetupAttributeResponse(ProductStage variant, List<AttributeRequest> attributeList,bool isDefault = false)
        {
            foreach (var attribute in variant.ProductStageAttributes.Where(w=>w.Attribute.DefaultAttribute==isDefault))
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
                            CheckboxValue = attribute.CheckboxValue
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

        private ProductStageRequest GetProductStageRequestFromId(ColspEntities db, long productId)
        {
            var tmpProduct = db.ProductStageGroups.Where(w =>w.ProductId == productId)
                    .Include(i=>i.ProductStageTags)
                    .Include(i=>i.ProductStageRelateds1.Select(s=>s.ProductStageGroup.ProductStages))
                    .Include(i=>i.Brand)
                    .Include(i => i.ProductStages.Select(s => s.ProductStageAttributes.Select(sa => sa.Attribute.AttributeValueMaps.Select(sv => sv.AttributeValue))))
                    .Include(i => i.ProductStages.Select(s => s.Inventory))
                    .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
                    .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
                    .Include(i => i.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory))
                    .Include(i => i.ProductStageLocalCatMaps.Select(s => s.LocalCategory));
            ProductStageGroup product = null;
            if (this.User.HasPermission("Edit Product") || this.User.HasPermission("Duplicate Product"))
            {
                product = tmpProduct.SingleOrDefault();
            }
            else if (this.User.ShopRequest() != null)
            {
                var shopId = this.User.ShopRequest().ShopId;
                product = tmpProduct.Where(w => w.ShopId == shopId).SingleOrDefault();
            }
            else
            {
                throw new Exception("Invalid permission");
            }
            if (product == null)
            {
                throw new Exception("Cannot find product " + productId);
            }

            ProductStageRequest response = new ProductStageRequest();
            SetupResponse(product, response);
            return response;
        }

        private void SetupResponse(ProductStageGroup product, ProductStageRequest response)
        {
            SetupGroupResponse(product, response);
            var masterVariant = product.ProductStages.Where(w => w.IsVariant == false).FirstOrDefault();
            SetupVariantResponse(masterVariant, response.MasterVariant);
            SetupAttributeResponse(masterVariant, response.MasterAttribute);
            SetupAttributeResponse(masterVariant, response.DefaultAttributes,true);
            var variants = product.ProductStages.Where(w => w.IsVariant == true).ToList();
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
        }

        private void SetupGroup(ProductStageGroup group, ProductStageRequest request,bool addNew,string email,bool adminPermission,bool sellerPermission, ColspEntities db)
        {
            #region Status
            if (adminPermission)
            {
                group.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            }
            else if (sellerPermission)
            {
                group.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL });
            }
            else
            {
                throw new Exception("Has no permission");
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
                            CreatedBy = email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = email,
                            UpdatedDt = DateTime.Now,
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
                            CreatedBy = email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = email,
                            UpdatedDt = DateTime.Now,
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
                            CreatedBy = email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = email,
                            UpdatedDt = DateTime.Now,
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
                            CreatedBy = email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = email,
                            UpdatedDt = DateTime.Now
                        });
                    }
                }
            }
            if(tmpRelated != null && tmpRelated.Count > 0)
            {
                db.ProductStageRelateds.RemoveRange(tmpRelated);
            }
            #endregion
            #region Other field
            group.TheOneCardEarn = request.TheOneCardEarn;
            group.GiftWrap = Validation.ValidateString(request.GiftWrap, "Gift Wrap", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            group.EffectiveDate = request.EffectiveDate;
            group.ExpireDate = request.ExpireDate;
            group.ControlFlag1 = request.ControlFlags.Flag1;
            group.ControlFlag2 = request.ControlFlags.Flag2;
            group.ControlFlag3 = request.ControlFlags.Flag3;
            group.Remark = Validation.ValidateString(request.Remark, "Remark", true, 500, false, string.Empty);
            group.InfoFlag = false;
            group.ImageFlag = false;
            group.OnlineFlag = false;
            group.InformationTabStatus = Validation.ValidateString(request.AdminApprove.Information, "Information Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() { Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.ImageTabStatus = Validation.ValidateString(request.AdminApprove.Image, "Image Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.CategoryTabStatus = Validation.ValidateString(request.AdminApprove.Category, "Category Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.MoreOptionTabStatus = Validation.ValidateString(request.AdminApprove.MoreOption, "More Option Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.VariantTabStatus = Validation.ValidateString(request.AdminApprove.Variation, "Variant Tab Status", true, 2, true, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, new List<string>() {Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            group.RejectReason = Validation.ValidateString(request.AdminApprove.RejectReason, "Reject Reason", true, 500, true, string.Empty);
            group.Visibility = request.Visibility;
            #endregion
            #region Create/Update
            if (addNew)
            {
                group.CreatedBy = email;
                group.CreatedDt = DateTime.Now;
            }
            group.UpdatedBy = email;
            group.UpdatedDt = DateTime.Now;
            #endregion

        }

        private void SetupAttribute(ProductStage variant, List<AttributeRequest> requestList, List<Entity.Models.Attribute> attributeList, string email, ColspEntities db, bool isDefault = false)
        {
            var tmpAttribute = variant.ProductStageAttributes.Where(w=>w.Attribute.DefaultAttribute== isDefault).ToList();
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
                    }
                    else
                    {
                        throw new Exception(string.Concat("Attribute value ",
                            string.Join(",", request.AttributeValues.Select(s => s.AttributeValueEn))));
                    }
                }
                else if (Constant.DATA_TYPE_CHECKBOX.Equals(attribute.DataType))
                {
                    if (request.AttributeValues == null)
                    {
                        throw new Exception(string.Concat("Attribute ", attribute.AttributeNameEn, " should have variant"));
                    }
                    if (attribute.AttributeValueMaps.All(a => request.AttributeValues
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
                                        current.UpdatedBy = email;
                                        current.UpdatedDt = DateTime.Now;
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
                                    CreatedBy = email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = email,
                                    UpdatedDt = DateTime.Now,
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
                        CreatedBy = email,
                        CreatedDt = DateTime.Now,
                        UpdatedBy = email,
                        UpdatedDt = DateTime.Now,
                    });
                }
            }
            if(tmpAttribute != null && tmpAttribute.Count > 0)
            {
                db.ProductStageAttributes.RemoveRange(tmpAttribute);
            }
        }

        private void SetupVariant(ProductStage variant,VariantRequest request, bool addNew, string email, bool adminPermission, bool sellerPermission, ColspEntities db,List<Shipping> shippingList)
        {
            #region Status
            if (adminPermission)
            {
                variant.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL, Constant.PRODUCT_STATUS_APPROVE, Constant.PRODUCT_STATUS_NOT_APPROVE });
            }
            else if (sellerPermission)
            {
                variant.Status = Validation.ValidateString(request.Status, "Status", true, 2, true, Constant.PRODUCT_STATUS_DRAFT, new List<string>() { Constant.PRODUCT_STATUS_DRAFT, Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL });
            }
            else
            {
                throw new Exception("Has no permission");
            }
            #endregion
            #region Variant Field
            variant.ProductNameTh = Validation.ValidateString(request.ProductNameTh, "Product Name (Thai)", true, 300, true);
            variant.ProductNameEn = Validation.ValidateString(request.ProductNameEn, "Product Name (English)", true, 300, true);
            variant.Sku = Validation.ValidateString(request.Sku, "SKU", false, 300, true, string.Empty);
            variant.Upc = Validation.ValidateString(request.Upc, "UPC", false, 300, true, string.Empty);
            variant.OriginalPrice = Validation.ValidateDecimal(request.OriginalPrice, "Original Price", false, 10, 3, true, 0).Value;
            variant.SalePrice = Validation.ValidateDecimal(request.SalePrice, "Sale Price", true, 10, 3, true).Value;
            variant.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Description (Thai)", false, Int32.MaxValue, false, string.Empty);
            variant.DescriptionShortTh = Validation.ValidateString(request.DescriptionShortTh, "Short Description (Thai)", false, 500, true, string.Empty);
            variant.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Description (English)", false, Int32.MaxValue, false, string.Empty);
            variant.DescriptionShortEn = Validation.ValidateString(request.DescriptionShortEn, "Short Description (English)", false, 500, true, string.Empty);
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
            variant.KillerPoint1En = Validation.ValidateString(request.KillerPoint1En, "Killer Point 1 (English)", false, 100, false, string.Empty);
            variant.KillerPoint2En = Validation.ValidateString(request.KillerPoint2En, "Killer Point 2 (English)", false, 100, false, string.Empty);
            variant.KillerPoint3En = Validation.ValidateString(request.KillerPoint3En, "Killer Point 3 (English)", false, 100, false, string.Empty);
            variant.KillerPoint1Th = Validation.ValidateString(request.KillerPoint1Th, "Killer Point 1 (Thai)", false, 100, false, string.Empty);
            variant.KillerPoint2Th = Validation.ValidateString(request.KillerPoint2Th, "Killer Point 2 (Thai)", false, 100, false, string.Empty);
            variant.KillerPoint3Th = Validation.ValidateString(request.KillerPoint3Th, "Killer Point 3 (Thai)", false, 100, false, string.Empty);
            variant.Installment = Validation.ValidateString(request.Installment, "Installment", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
            variant.TheOneCardEarn = request.TheOneCardEarn;
            variant.GiftWrap = Validation.ValidateString(request.GiftWrap, "Gift Wrap", true, 1, true, Constant.STATUS_NO, new List<string>() { Constant.STATUS_YES, Constant.STATUS_NO });
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
            variant.UrlEn = Validation.ValidateString(request.SEO.ProductUrlKeyEn, "Product Url Key", false, 300, false, string.Empty);
            variant.BoostWeight = request.SEO.ProductBoostingWeight;
            variant.Visibility = request.Visibility;
            variant.DefaultVaraint = request.DefaultVariant;
            variant.Display = Validation.ValidateString(request.Display, "Display", true, 20, true, Constant.VARIANT_DISPLAY_GROUP, new List<string>() { Constant.VARIANT_DISPLAY_GROUP, Constant.VARIANT_DISPLAY_INDIVIDUAL });
            #endregion
            #region Shipping
            if (request.ShippingMethod == 0)
            {
                request.ShippingMethod = 1;
            }
            var tmpShipping = shippingList.Where(w => w.ShippingId != request.ShippingMethod).SingleOrDefault();
            if (tmpShipping == null)
            {
                throw new Exception("Invalid Shipping");
            }
            variant.ShippingId = request.ShippingMethod;
            #endregion
            #region Inventory
            int stockType = Constant.STOCK_TYPE["Stock"];
            if (Constant.STOCK_TYPE.ContainsKey(request.StockType))
            {
                stockType = Constant.STOCK_TYPE[request.StockType];
            }
            if (variant.Inventory != null)
            {
                if (variant.Inventory.Quantity != request.Quantity 
                    || variant.Inventory.SafetyStockSeller != request.SafetyStock
                    || variant.Inventory.StockAvailable != stockType)
                {
                    variant.Inventory.Quantity = request.Quantity;
                    variant.Inventory.SafetyStockSeller = request.SafetyStock;
                    variant.Inventory.StockAvailable = stockType;
                    variant.Inventory.UpdatedBy = this.User.UserRequest().Email;
                    variant.Inventory.UpdatedDt = DateTime.Now;

                    InventoryHistory history = new InventoryHistory()
                    {
                        Pid = variant.Pid,
                        StockAvailable = variant.Inventory.StockAvailable,
                        Defect = variant.Inventory.Defect,
                        MaxQuantity = variant.Inventory.MaxQuantity,
                        MinQuantity = variant.Inventory.MinQuantity,
                        OnHold = variant.Inventory.OnHold,
                        Quantity = variant.Inventory.Quantity,
                        Reserve = variant.Inventory.Reserve,
                        SafetyStockAdmin = variant.Inventory.SafetyStockAdmin,
                        SafetyStockSeller = variant.Inventory.SafetyStockSeller,
                        Status = Constant.INVENTORY_STATUS_UPDATE,
                        CreatedBy = this.User.UserRequest().Email,
                        CreatedDt = DateTime.Now,
                        UpdatedBy = this.User.UserRequest().Email,
                        UpdatedDt = DateTime.Now,
                    };
                    db.InventoryHistories.Add(history);
                }
            }
            else
            {
                variant.Inventory = new Inventory()
                {
                    StockAvailable = stockType,
                    Defect = 0,
                    MaxQuantity = 0,
                    MinQuantity = 0,
                    OnHold = 0,
                    Quantity = request.Quantity,
                    Reserve = 0,
                    SafetyStockAdmin = request.SafetyStock,
                    SafetyStockSeller = request.SafetyStock,
                    CreatedBy = this.User.UserRequest().Email,
                    CreatedDt = DateTime.Now,
                    UpdatedBy = this.User.UserRequest().Email,
                    UpdatedDt = DateTime.Now,
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
                    if (image == null && string.IsNullOrWhiteSpace(image.url)) continue;
                    bool isNew = false;
                    if (tmpImage == null || tmpImage.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpImage.Where(w => w.ImageId == image.ImageId).SingleOrDefault();
                        if (current != null)
                        {
                            if (current.Position != position || current.FeatureFlag != featureImg)
                            {
                                current.Position = position;
                                current.FeatureFlag = featureImg;
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
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
                            ImageUrlEn = image.url,
                            Position = position++,
                            FeatureFlag = featureImg,
                            ImageName = string.Empty,
                            ImageOriginName = string.Empty,
                            Status = Constant.STATUS_ACTIVE,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                    if (featureImg)
                    {
                        variant.FeatureImgUrl = image.url;
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
                        var current = tmpVideo.Where(w => w.VideoId == video.VideoId).SingleOrDefault();
                        if (current != null)
                        {
                            if(current.Position != position)
                            {
                                current.Position = position;
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
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
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
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
                variant.CreatedBy = email;
                variant.CreatedDt = DateTime.Now;
            }
            variant.UpdatedBy = email;
            variant.UpdatedDt = DateTime.Now;
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
                    if (image == null && string.IsNullOrWhiteSpace(image.url)) continue;
                    bool isNew = false;
                    if (tmpImage == null || tmpImage.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = tmpImage.Where(w => w.ImageId == image.ImageId).SingleOrDefault();
                        if (current != null)
                        {
                            if (current.Position != position || current.FeatureFlag != featureImg)
                            {
                                string lastPart = current.ImageUrlEn.Split('/').Last();
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
                                    current.ImageUrlEn = filename; 
                                }
                                current.Position = position;
                                current.FeatureFlag = featureImg;
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
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
                            ImageUrlEn = image.url,
                            Position = position++,
                            FeatureFlag = featureImg,
                            ImageName = string.Empty,
                            ImageOriginName = string.Empty,
                            Status = Constant.STATUS_ACTIVE,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                    if (featureImg)
                    {
                        variant.FeatureImgUrl = image.url;
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

        private void SetupGroupAfterSave(ProductStageGroup group, ColspEntities db = null, bool isNew = false)
        {
            var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
            var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;
            foreach (var stage in group.ProductStages)
            {
                SetupStageAfterSave(stage, schema, imageUrl, db, isNew);
            }
        }

        private void SetupStageAfterSave(ProductStage stage,string schema,string imageUrl, ColspEntities db = null, bool isNew = false)
        {
            #region Image
            foreach (var image in stage.ProductStageImages)
            {
                string lastPart = image.ImageUrlEn.Split('/').Last();
                string newFile = Path.Combine(
                    AppSettingKey.IMAGE_ROOT_PATH
                    , AppSettingKey.PRODUCT_FOLDER
                    , string.Concat(stage.Pid, "_", image.Position, Path.GetExtension(lastPart)));
                string oldFile = Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.PRODUCT_FOLDER, lastPart);
                if (File.Exists(oldFile))
                {
                    if (File.Exists(newFile))
                    {
                        continue;
                    }
                    File.Move(oldFile, newFile);
                    image.ImageUrlEn = string.Concat(
                        schema, "://", imageUrl, "/"
                        , AppSettingKey.IMAGE_ROOT_FOLDER
                        , "/", AppSettingKey.PRODUCT_FOLDER
                        , "/", Path.GetFileName(newFile));
                    if (image.FeatureFlag)
                    {
                        stage.FeatureImgUrl = image.ImageUrlEn;
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
                    StockAvailable = stage.Inventory.StockAvailable,
                    Defect = stage.Inventory.Defect,
                    MaxQuantity = stage.Inventory.MaxQuantity,
                    MinQuantity = stage.Inventory.MinQuantity,
                    OnHold = stage.Inventory.OnHold,
                    Quantity = stage.Inventory.Quantity,
                    Reserve = stage.Inventory.Reserve,
                    SafetyStockAdmin = stage.Inventory.SafetyStockAdmin,
                    SafetyStockSeller = stage.Inventory.SafetyStockSeller,
                    Status = Constant.INVENTORY_STATUS_ADD,
                    CreatedBy = this.User.UserRequest().Email,
                    CreatedDt = DateTime.Now,
                    UpdatedBy = this.User.UserRequest().Email,
                    UpdatedDt = DateTime.Now,
                };
                db.InventoryHistories.Add(history);
            }

            #endregion
        }

        private void SetupVariantResponse(ProductStage variant, VariantRequest response)
        {
            response.Pid = variant.Pid;
            response.ShopId = variant.ShopId;
            response.ShippingMethod = variant.ShippingId;
            response.ProductNameTh = variant.ProductNameTh;
            response.ProductNameEn = variant.ProductNameEn;
            response.Sku = variant.Sku;
            response.Upc = variant.Upc;
            response.OriginalPrice = variant.OriginalPrice;
            response.SalePrice = variant.SalePrice;
            response.DescriptionFullTh = variant.DescriptionFullTh;
            response.DescriptionShortTh = variant.DescriptionShortTh;
            response.DescriptionFullEn = variant.DescriptionFullEn;
            response.DescriptionShortEn = variant.DescriptionShortEn;
            response.Quantity = variant.Inventory.Quantity;
            response.SafetyStock = variant.Inventory.SafetyStockSeller;
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
            response.TheOneCardEarn = variant.TheOneCardEarn;
            response.GiftWrap = variant.GiftWrap;
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
            response.SEO.ProductUrlKeyEn = variant.UrlEn;
            response.SEO.ProductBoostingWeight = variant.BoostWeight;
            response.Visibility = variant.Visibility;
            response.DefaultVariant = variant.DefaultVaraint;
            response.Quantity = variant.Inventory.Quantity;
            response.SafetyStock = variant.Inventory.SafetyStockSeller;
            response.StockType = Constant.STOCK_TYPE.Where(w => w.Value.Equals(variant.Inventory.StockAvailable)).SingleOrDefault().Key;
            response.Display = variant.Display;
            if (variant.ProductStageImages != null && variant.ProductStageImages.Count > 0)
            {
                variant.ProductStageImages = variant.ProductStageImages.OrderBy(o => o.Position).ToList();
                foreach (var image in variant.ProductStageImages)
                {
                    response.Images.Add(new ImageRequest()
                    {
                        ImageId = image.ImageId,
                        url = image.ImageUrlEn,
                        position = image.Position
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
                        VideoId = video.VideoId,
                        Url = video.VideoUrlEn
                    });
                }
            }
        }

        private void SetupGroupResponse(ProductStageGroup product, ProductStageRequest response)
        {
            response.ProductId = product.ProductId;
            response.ShopId = product.ShopId;
            response.MainGlobalCategory = new CategoryRequest() { CategoryId = product.GlobalCatId };
            if (product.LocalCatId != null)
            {
                response.MainLocalCategory = new CategoryRequest() { CategoryId = product.LocalCatId.Value };
            }
            if (product.Brand != null)
            {
                response.Brand = new BrandRequest() { BrandId = product.Brand.BrandId, BrandNameEn = product.Brand.BrandNameEn  };
            }
            if (product.AttributeSetId != null)
            {
                response.AttributeSet = new AttributeSetRequest() { AttributeSetId = product.AttributeSetId.Value };
            }
            if (product.ProductStageGlobalCatMaps != null && product.ProductStageGlobalCatMaps.Count > 0)
            {
                foreach (var category in product.ProductStageGlobalCatMaps)
                {
                    response.GlobalCategories.Add(new CategoryRequest()
                    {
                        CategoryId = category.CategoryId,
                        NameEn = category.GlobalCategory.NameEn,
                        NameTh = category.GlobalCategory.NameTh
                    });
                }
            }
            if (product.ProductStageLocalCatMaps != null && product.ProductStageLocalCatMaps.Count > 0)
            {
                foreach (var category in product.ProductStageLocalCatMaps)
                {
                    response.LocalCategories.Add(new CategoryRequest()
                    {
                        CategoryId = category.CategoryId,
                        NameEn = category.LocalCategory.NameEn,
                        NameTh = category.LocalCategory.NameTh
                    });
                }
            }
            if(product.ProductStageRelateds1 != null && product.ProductStageRelateds1.Count > 0)
            {
                foreach (var pro in product.ProductStageRelateds1)
                {
                    response.RelatedProducts.Add(new VariantRequest()
                    {
                        ProductId = pro.ProductStageGroup.ProductId,
                        ProductNameEn = pro.ProductStageGroup.ProductStages.Where(w=>w.IsVariant==false).FirstOrDefault().ProductNameEn,
                    });
                }
            }
            if (product.ProductStageTags != null && product.ProductStageTags.Count > 0)
            {
                response.Tags = product.ProductStageTags.Select(s => s.Tag).ToList();
            }
            response.TheOneCardEarn = product.TheOneCardEarn;
            response.GiftWrap = product.GiftWrap;
            response.EffectiveDate = product.EffectiveDate;
            response.ExpireDate = product.ExpireDate;
            response.ControlFlags.Flag1 = product.ControlFlag1;
            response.ControlFlags.Flag2 = product.ControlFlag2;
            response.ControlFlags.Flag3 = product.ControlFlag3;
            response.Remark = product.Remark;
            response.AdminApprove.Information = product.InformationTabStatus;
            response.AdminApprove.Image = product.ImageTabStatus;
            response.AdminApprove.Category = product.CategoryTabStatus;
            response.AdminApprove.Variation = product.VariantTabStatus;
            response.AdminApprove.MoreOption = product.MoreOptionTabStatus;
            response.AdminApprove.RejectReason = product.RejectReason;
            response.InfoFlag = false;
            response.ImageFlag = false;
            response.OnlineFlag = false;
            response.Visibility = product.Visibility;
            response.Status = product.Status;
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
                             let comm = db.ProductStageComments.Where(w => w.Pid.Equals(mast.Pid)).OrderByDescending(o => o.UpdatedDt).FirstOrDefault()
                             let commVar = db.ProductStageComments.Where(w => w.Pid.Equals(vari.Pid)).OrderByDescending(o => o.UpdatedDt).FirstOrDefault()
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
                //var products = (from stage in db.ProductStages
                //                join proImg in db.ProductStageImages on stage.Pid equals proImg.Pid into proImgJoin
                //                join variant in db.ProductStageVariants.Include(i => i.ProductStageVariantArrtibuteMaps) on stage.ProductId equals variant.ProductId into varJoin
                //                from varJ in varJoin.DefaultIfEmpty()
                //                    //join varMap in db.ProductStageVariantArrtibuteMaps on varJ.VariantId equals varMap.VariantId into varMapJ
                //                    //from varMap in varMapJ.DefaultIfEmpty()
                //                    //join attrVal in db.AttributeValues on varMap.Value equals attrVal.MapValue into attrValJ
                //                    //from attraVal in attrValJ.DefaultIfEmpty()
                //                join varImg in db.ProductStageImages on varJ.Pid equals varImg.Pid into varImgJoin
                //                let comm = db.ProductStageComments.Where(w => w.Pid.Equals(stage.Pid)).OrderByDescending(o => o.UpdatedDt).FirstOrDefault()
                //                let commVar = db.ProductStageComments.Where(w => w.Pid.Equals(varJ.Pid)).OrderByDescending(o => o.UpdatedDt).FirstOrDefault()
                //                where stage.ShopId == shopId
                //                select new
                //                {
                //                    stage.ProductId,
                //                    Sku = varJ != null ? varJ.Sku : stage.Sku,
                //                    Upc = varJ != null ? varJ.Upc : stage.Upc,
                //                    ProductNameEn = varJ != null ? varJ.ProductNameEn : stage.ProductNameEn,
                //                    ProductNameTh = varJ != null ? varJ.ProductNameTh : stage.ProductNameTh,
                //                    Pid = varJ != null ? varJ.Pid : stage.Pid,
                //                    VariantValue = "", //todo
                //                    Status = varJ != null ? varJ.Status : stage.Status,
                //                    MasterImg = proImgJoin.Select(s => new ImageRequest { ImageId = s.ImageId, url = s.ImageUrlEn, tmpPath = s.Path, position = s.Position }).OrderBy(o => o.position),
                //                    VariantImg = varImgJoin.Select(s => new ImageRequest { ImageId = s.ImageId, url = s.ImageUrlEn, tmpPath = s.Path, position = s.Position }).OrderBy(o => o.position),
                //                    IsVariant = varJ != null ? true : false,
                //                    Comment = commVar != null ? commVar.Comment : commVar.Comment,
                //                });
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
                            //Get column 'Group Id'.
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
                                CreatedBy = this.User.UserRequest().Email,
                                CreatedDt = DateTime.Now,
                                UpdatedBy = this.User.UserRequest().Email,
                                UpdatedDt = DateTime.Now
                            };
                        }
                        #endregion
                        #region Variant Detail
                        //Initialise product stage variant
                        variant = new ProductStageVariant()
                        {
                            ShopId = shopId,
                            DefaultVaraint = false,
                            Status = Constant.PRODUCT_STATUS_DRAFT,
                            Visibility = true,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
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
                                            CreatedBy = this.User.UserRequest().Email,
                                            CreatedDt = DateTime.Now,
                                            UpdatedBy = this.User.UserRequest().Email,
                                            UpdatedDt = DateTime.Now,
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
                                            CreatedBy = this.User.UserRequest().Email,
                                            CreatedDt = DateTime.Now,
                                            UpdatedBy = this.User.UserRequest().Email,
                                            UpdatedDt = DateTime.Now,
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
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


        [Route("api/ProductStages/Import")]
        [HttpPut]
        public async Task<HttpResponseMessage> ImportSaveProduct()
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

                if (streamProvider.FileData == null || streamProvider.FileData.Count == 0)
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
                    var pids = new HashSet<string>();
                    foreach (string head in headers)
                    {
                        if (headDic.ContainsKey(head))
                        {
                            throw new Exception(head + " is duplicate header");
                        }
                        headDic.Add(head, i++);
                        firstRow.Add(csvResult.GetField<string>(head));
                    }
                    csvRows = ReadExcel(csvResult, headers, firstRow);

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
                    Dictionary<string, Inventory> inventoryList = new Dictionary<string, Inventory>();
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
                            //Get column 'Group Id'.
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
                                CreatedBy = this.User.UserRequest().Email,
                                CreatedDt = DateTime.Now,
                                UpdatedBy = this.User.UserRequest().Email,
                                UpdatedDt = DateTime.Now
                            };
                        }
                        #endregion
                        #region Variant Detail
                        variant = new ProductStageVariant()
                        {
                            ShopId = shopId,
                            DefaultVaraint = false,
                            Status = Constant.PRODUCT_STATUS_DRAFT,
                            Visibility = true,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
                        };
                        if (headDic.ContainsKey("Default Variant"))
                        {
                            string defaultVar = body[headDic["Default Variant"]];
                            variant.DefaultVaraint = "Yes".Equals(defaultVar);
                        }

                        variant.Sku = Validation.ValidateCSVStringColumn(headDic, body, "SKU", true, 300, errorMessage, row);
                        variant.Upc = Validation.ValidateCSVStringColumn(headDic, body, "UPC", false, 300, errorMessage, row);
                        variant.ProductNameEn = Validation.ValidateCSVStringColumn(headDic, body, "Product Name (English)", true, 300, errorMessage, row);
                        variant.ProductNameTh = Validation.ValidateCSVStringColumn(headDic, body, "Product Name (Thai)", true, 300, errorMessage, row);
                        variant.DescriptionFullEn = Validation.ValidateCSVStringColumn(headDic, body, "Description (English)", true, Int32.MaxValue, errorMessage, row);
                        variant.DescriptionFullTh = Validation.ValidateCSVStringColumn(headDic, body, "Description (Thai)", true, Int32.MaxValue, errorMessage, row);
                        variant.DescriptionShortEn = Validation.ValidateCSVStringColumn(headDic, body, "Short Description (English)", false, 500, errorMessage, row);
                        variant.DescriptionShortTh = Validation.ValidateCSVStringColumn(headDic, body, "Short Description (Thai)", false, 500, errorMessage, row);
                        #endregion

                        #region PID
                        if (headDic.ContainsKey("PID"))
                        {
                            variant.Pid = body[headDic["PID"]];
                            pids.Add(variant.Pid);
                        }
                        else
                        {
                            errorMessage.Add("No PID column found");
                        }
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
                                    variant.PrepareDay = Decimal.Parse(preDay);
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
                                            CreatedBy = this.User.UserRequest().Email,
                                            CreatedDt = DateTime.Now,
                                            UpdatedBy = this.User.UserRequest().Email,
                                            UpdatedDt = DateTime.Now,
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
                                            CreatedBy = this.User.UserRequest().Email,
                                            CreatedDt = DateTime.Now,
                                            UpdatedBy = this.User.UserRequest().Email,
                                            UpdatedDt = DateTime.Now,
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
                            if (!inventoryList.ContainsKey(variant.Pid))
                            {
                                inventoryList.Add(variant.Pid, inventory);
                            }
                            else
                            {
                                errorMessage.Add("Duplicate PID "+ variant.Pid + " at row " + row);
                            }
                            
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
                        group.Tag = Validation.ValidateCSVStringColumn(headDic, body, "Search Tag", false, 630, errorMessage, row);
                        group.EffectiveDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "Effective Date");
                        group.EffectiveTime = Validation.ValidateCSVTimeSpanColumn(headDic, body, "Effective Time");
                        group.ExpiryDate = Validation.ValidateCSVDatetimeColumn(headDic, body, "Expire Date");
                        group.ExpiryTime = Validation.ValidateCSVTimeSpanColumn(headDic, body, "Expire Time");
                        group.Remark = Validation.ValidateCSVStringColumn(headDic, body, "Remark", false, 500, errorMessage, row);

                        if (headDic.ContainsKey("Flag 1"))
                        {
                            group.ControlFlag1 = string.Equals(body[headDic["Flag 1"]], "yes", StringComparison.OrdinalIgnoreCase) ? true : false;
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
                            group.Pid = variant.Pid;
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                                                CreatedBy = this.User.UserRequest().Email,
                                                CreatedDt = DateTime.Now,
                                                UpdatedBy = this.User.UserRequest().Email,
                                                UpdatedDt = DateTime.Now,
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
                                    var variant1 = Validation.ValidateCSVStringColumn(headDic, body, "Variation Option 1", false, 300, errorMessage, row);
                                    var variant2 = Validation.ValidateCSVStringColumn(headDic, body, "Variation Option 2", false, 300, errorMessage, row);
                                    foreach (var attr in attrSet.Attribute)
                                    {
                                        if (headDic.ContainsKey(attr.AttributeNameEn))
                                        {
                                            var value = Validation.ValidateCSVStringColumn(headDic, body, attr.AttributeNameEn, false, 300, errorMessage, row);
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
                    var productList = db.ProductStages.Where(w => pids.Contains(w.Pid) && w.ShopId == shopId)
                        .Include(a=>a.ProductStageAttributes)
                        .Include(a=>a.ProductStageVariants.Select(s=>s.ProductStageVariantArrtibuteMaps))
                        .Include(a=>a.ProductStageGlobalCatMaps)
                        .Include(a=>a.ProductStageLocalCatMaps).ToList();
                    var variantList = db.ProductStageVariants.Where(w => pids.Contains(w.Pid) && w.ShopId == shopId)
                        .Include(a=>a.ProductStage.ProductStageAttributes)
                        .Include(a=>a.ProductStage.ProductStageGlobalCatMaps)
                        .Include(a=>a.ProductStage.ProductStageLocalCatMaps)
                        .Include(a=>a.ProductStageVariantArrtibuteMaps).ToList();
                    var invenList = db.Inventories.Where(w => pids.Contains(w.Pid)).ToList();
                    foreach (var product in groupList)
                    {

                        ProductStage stage = productList.Where(w=>w.Pid.Equals(product.Value.Pid)).SingleOrDefault();
                        if(stage == null)
                        {
                            stage = variantList.Where(w => w.Pid.Equals(product.Value.Pid)).Select(s=>s.ProductStage).SingleOrDefault();
                        }
                        if(stage == null)
                        {
                            errorMessage.Add("Pid " + product.Value.Pid + " is not found");
                            continue;
                        }
                        #region Setup Product Stage
                        if(headDic.ContainsKey("Product Name (Thai)"))
                        {
                            stage.ProductNameTh = product.Value.ProductNameTh;
                        }
                        if (headDic.ContainsKey("Product Name (English)"))
                        {
                            stage.ProductNameEn = product.Value.ProductNameEn;
                        }
                        if (headDic.ContainsKey("SKU"))
                        {
                            stage.Sku = product.Value.Sku;
                        }
                        if (headDic.ContainsKey("UPC"))
                        {
                            stage.Upc = product.Value.Upc;
                        }
                        if (headDic.ContainsKey("Brand Name"))
                        {
                            stage.BrandId = product.Value.BrandId;
                        }
                        if (headDic.ContainsKey("Original Price"))
                        {
                            stage.OriginalPrice = product.Value.OriginalPrice;
                        }
                        if (headDic.ContainsKey("Sale Price"))
                        {
                            stage.SalePrice = product.Value.SalePrice;
                        }
                        if (headDic.ContainsKey("Description (English)"))
                        {
                            stage.DescriptionFullEn = product.Value.DescriptionFullEn;
                        }
                        if (headDic.ContainsKey("Description (Thai)"))
                        {
                            stage.DescriptionFullTh = product.Value.DescriptionFullTh;
                        }
                        if (headDic.ContainsKey("Short Description (English)"))
                        {
                            stage.DescriptionShortEn = product.Value.DescriptionShortEn;
                        }
                        if (headDic.ContainsKey("Short Description (Thai)"))
                        {
                            stage.DescriptionShortTh = product.Value.DescriptionShortTh;
                        }
                        if (headDic.ContainsKey("Keywords"))
                        {
                            stage.Tag = product.Value.Tag;
                        }
                        if (headDic.ContainsKey("Shipping Method"))
                        {
                            stage.ShippingId = product.Value.ShippingId;
                        }
                        if (headDic.ContainsKey("Preparation Time"))
                        {
                            stage.PrepareDay = product.Value.PrepareDay;
                        }
                        if (headDic.ContainsKey("Package Dimension - Lenght (mm)"))
                        {
                            stage.Length = product.Value.Length;
                            stage.DimensionUnit = "MM";
                        }
                        if (headDic.ContainsKey("Package Dimension - Height (mm)"))
                        {
                            stage.Height = product.Value.Height;
                            stage.DimensionUnit = "MM";
                        }
                        if (headDic.ContainsKey("Package Dimension - Width (mm)"))
                        {
                            stage.Width = product.Value.Width;
                            stage.DimensionUnit = "MM";
                        }
                        if (headDic.ContainsKey("Package - Weight (g)"))
                        {
                            stage.Weight = product.Value.Weight;
                            stage.WeightUnit = "G";
                        }
                        if (headDic.ContainsKey("Global Category ID"))
                        {
                            stage.GlobalCatId = product.Value.GlobalCatId;
                        }
                        if (headDic.ContainsKey("Alternative Global Category 1")
                            || headDic.ContainsKey("Alternative Global Category 2"))
                        {
                            var oldList = stage.ProductStageGlobalCatMaps.ToList();
                            foreach(var cat in product.Value.ProductStageGlobalCatMaps)
                            {
                                bool isNewCat = false;
                                if(oldList == null || oldList.ToList().Count == 0)
                                {
                                    isNewCat = true;
                                }
                                if (!isNewCat)
                                {
                                    var currentCat = oldList.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
                                    if(currentCat != null)
                                    {
                                        currentCat.UpdatedBy = this.User.UserRequest().Email;
                                        currentCat.UpdatedDt = DateTime.Now;
                                        oldList.Remove(currentCat);
                                    }
                                    else
                                    {
                                        isNewCat = true;
                                    }
                                }
                                if (isNewCat)
                                {
                                    stage.ProductStageGlobalCatMaps.Add(new ProductStageGlobalCatMap()
                                    {
                                        CategoryId = cat.CategoryId,
                                        Status = Constant.STATUS_ACTIVE,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now,
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                    });
                                }
                            }
                            if(oldList != null && oldList.Count > 0)
                            {
                                db.ProductStageGlobalCatMaps.RemoveRange(oldList);
                            }
                        }
                        if (headDic.ContainsKey("Alternative Local Category 1")
                            || headDic.ContainsKey("Alternative Local Category 2"))
                        {
                            var oldList = stage.ProductStageLocalCatMaps.ToList();
                            foreach (var cat in product.Value.ProductStageLocalCatMaps)
                            {
                                bool isNewCat = false;
                                if (oldList == null || oldList.ToList().Count == 0)
                                {
                                    isNewCat = true;
                                }
                                if (!isNewCat)
                                {
                                    var currentCat = oldList.Where(w => w.CategoryId == cat.CategoryId).SingleOrDefault();
                                    if (currentCat != null)
                                    {
                                        currentCat.UpdatedBy = this.User.UserRequest().Email;
                                        currentCat.UpdatedDt = DateTime.Now;
                                        oldList.Remove(currentCat);
                                    }
                                    else
                                    {
                                        isNewCat = true;
                                    }
                                }
                                if (isNewCat)
                                {
                                    stage.ProductStageLocalCatMaps.Add(new ProductStageLocalCatMap()
                                    {
                                        CategoryId = cat.CategoryId,
                                        Status = Constant.STATUS_ACTIVE,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now,
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                    });
                                }
                            }
                            if (oldList != null && oldList.Count > 0)
                            {
                                db.ProductStageLocalCatMaps.RemoveRange(oldList);
                            }
                        }
                        if (headDic.ContainsKey("Local Category ID"))
                        {
                            stage.LocalCatId = product.Value.LocalCatId;
                        }
                        if (headDic.ContainsKey("Meta Title (English)"))
                        {
                            stage.MetaTitleEn = product.Value.MetaTitleEn;
                        }
                        if (headDic.ContainsKey("Meta Title (Thai)"))
                        {
                            stage.MetaTitleTh = product.Value.MetaTitleTh;
                        }
                        if (headDic.ContainsKey("Meta Description (English)"))
                        {
                            stage.MetaDescriptionEn = product.Value.MetaDescriptionEn;
                        }
                        if (headDic.ContainsKey("Meta Description (Thai)"))
                        {
                            stage.MetaDescriptionTh = product.Value.MetaDescriptionTh;
                        }
                        if (headDic.ContainsKey("Meta Keywords (English)"))
                        {
                            stage.MetaKeyEn = product.Value.MetaKeyEn;
                        }
                        if (headDic.ContainsKey("Meta Keywords (Thai)"))
                        {
                            stage.MetaKeyTh = product.Value.MetaKeyTh;
                        }
                        if (headDic.ContainsKey("Product URL Key(English)"))
                        {
                            if (!string.IsNullOrEmpty(product.Value.UrlEn))
                            {
                                stage.UrlEn = product.Value.UrlEn;
                            }
                        }
                        if (headDic.ContainsKey("Product Boosting Weight"))
                        {
                            stage.BoostWeight = product.Value.BoostWeight;
                        }
                        if (headDic.ContainsKey("Effective Date"))
                        {
                            stage.EffectiveDate = product.Value.EffectiveDate;
                        }
                        if (headDic.ContainsKey("Effective Time"))
                        {
                            stage.EffectiveTime = product.Value.EffectiveTime;
                        }
                        if (headDic.ContainsKey("Expire Date"))
                        {
                            stage.ExpiryDate = product.Value.ExpiryDate;
                        }
                        if (headDic.ContainsKey("Expire Time"))
                        {
                            stage.ExpiryTime = product.Value.ExpiryTime;
                        }
                        if (headDic.ContainsKey("Remark"))
                        {
                            stage.Remark = product.Value.Remark;
                        }
                        if (headDic.ContainsKey("Flag 1"))
                        {
                            stage.ControlFlag1 = product.Value.ControlFlag1;
                        }
                        if (headDic.ContainsKey("Flag 2"))
                        {
                            stage.ControlFlag2 = product.Value.ControlFlag2;
                        }
                        if (headDic.ContainsKey("Flag 3"))
                        {
                            stage.ControlFlag3 = product.Value.ControlFlag3;
                        }
                        if (headDic.ContainsKey("Attribute Set"))
                        {
                            stage.AttributeSetId = product.Value.AttributeSetId;
                        }
                        var oldAttribute = stage.ProductStageAttributes.ToList();
                        foreach(var att in product.Value.ProductStageAttributes)
                        {
                            bool isNewSet = false;
                            if (oldAttribute == null || oldAttribute.Count == 0)
                            {
                                isNewSet = true;
                            }
                            if (!isNewSet)
                            {
                                var current = oldAttribute.Where(w => w.AttributeId == att.AttributeId).SingleOrDefault();
                                if(current != null)
                                {
                                    current.ValueEn = att.ValueEn;
                                    current.ValueTh = att.ValueTh;
                                    current.UpdatedBy = this.User.UserRequest().Email;
                                    current.UpdatedDt = DateTime.Now;
                                    oldAttribute.Remove(current);
                                }
                                else
                                {
                                    isNewSet = true;
                                }
                            }
                            if (isNewSet)
                            {
                                stage.ProductStageAttributes.Add(att);
                            }
                        }
                        if(oldAttribute != null && oldAttribute.Count > 0)
                        {
                            db.ProductStageAttributes.RemoveRange(oldAttribute);
                        }
                        #endregion
                        var oldVariantList = stage.ProductStageVariants.ToList();
                        for (int varIndex = 0; varIndex < product.Value.ProductStageVariants.Count; varIndex++)
                        {
                            string tmpInventory = product.Value.ProductStageVariants.ElementAt(varIndex).Pid;
                            if (product.Value.ProductStageVariants.ElementAt(varIndex).ProductStageVariantArrtibuteMaps.Count == 0)
                            {
                                if (inventoryList.ContainsKey(tmpInventory))
                                {
                                    if (!inventoryList.ContainsKey(product.Value.Pid))
                                    {
                                        inventoryList.Add(product.Value.Pid, inventoryList[tmpInventory]);
                                    }
                                    inventoryList.Remove(tmpInventory);
                                }
                                product.Value.ProductStageVariants.Remove(product.Value.ProductStageVariants.ElementAt(varIndex--));
                            }
                            else
                            {
                                var tmpVariant = product.Value.ProductStageVariants.ElementAt(varIndex);
                                bool isNewSet = false;
                                if(oldVariantList == null || oldVariantList.Count == 0)
                                {
                                    isNewSet = true;
                                }
                                if (!isNewSet)
                                {
                                    var current = oldVariantList.Where(w => w.Pid.Equals(tmpVariant.Pid)).SingleOrDefault();
                                    if (current != null)
                                    {

                                        if (current.DefaultVaraint != null && current.DefaultVaraint.Value)
                                        {
                                            if (!inventoryList.ContainsKey(product.Value.Pid))
                                            {
                                                inventoryList.Add(product.Value.Pid, inventoryList[tmpInventory]);
                                            }
                                        }

                                        #region Setup Variant
                                        if (headDic.ContainsKey("Product Name (Thai)"))
                                        {
                                            current.ProductNameTh = tmpVariant.ProductNameTh;
                                        }
                                        if (headDic.ContainsKey("Product Name (English)"))
                                        {
                                            current.ProductNameEn = tmpVariant.ProductNameEn;
                                        }
                                        if (headDic.ContainsKey("SKU"))
                                        {
                                            current.Sku = tmpVariant.Sku;
                                        }
                                        if (headDic.ContainsKey("UPC"))
                                        {
                                            current.Upc = tmpVariant.Upc;
                                        }
                                        if (headDic.ContainsKey("Original Price"))
                                        {
                                            current.OriginalPrice = tmpVariant.OriginalPrice;
                                        }
                                        if (headDic.ContainsKey("Sale Price"))
                                        {
                                            current.SalePrice = tmpVariant.SalePrice;
                                        }
                                        if (headDic.ContainsKey("Description (English)"))
                                        {
                                            current.DescriptionFullEn = tmpVariant.DescriptionFullEn;
                                        }
                                        if (headDic.ContainsKey("Description (Thai)"))
                                        {
                                            current.DescriptionFullTh = tmpVariant.DescriptionFullTh;
                                        }
                                        if (headDic.ContainsKey("Short Description (English)"))
                                        {
                                            current.DescriptionShortEn = tmpVariant.DescriptionShortEn;
                                        }
                                        if (headDic.ContainsKey("Short Description (Thai)"))
                                        {
                                            current.DescriptionShortTh = tmpVariant.DescriptionShortTh;
                                        }
                                        if (headDic.ContainsKey("Short Description (Thai)"))
                                        {
                                            current.DescriptionShortTh = tmpVariant.DescriptionShortTh;
                                        }
                                        if (headDic.ContainsKey("Package Dimension - Lenght (mm)"))
                                        {
                                            current.Length = tmpVariant.Length;
                                            current.DimensionUnit = "MM";
                                        }
                                        if (headDic.ContainsKey("Package Dimension - Height (mm)"))
                                        {
                                            current.Height = tmpVariant.Height;
                                            current.DimensionUnit = "MM";
                                        }
                                        if (headDic.ContainsKey("Package Dimension - Width (mm)"))
                                        {
                                            current.Width = tmpVariant.Width;
                                            current.DimensionUnit = "MM";
                                        }
                                        if (headDic.ContainsKey("Package - Weight (g)"))
                                        {
                                            current.Weight = tmpVariant.Weight;
                                            current.WeightUnit = "G";
                                        }
                                        if (headDic.ContainsKey("Meta Title (English)"))
                                        {
                                            current.MetaTitleEn = tmpVariant.MetaTitleEn;
                                        }
                                        if (headDic.ContainsKey("Meta Title (Thai)"))
                                        {
                                            current.MetaTitleTh = tmpVariant.MetaTitleTh;
                                        }
                                        if (headDic.ContainsKey("Meta Description (English)"))
                                        {
                                            current.MetaDescriptionEn = tmpVariant.MetaDescriptionEn;
                                        }
                                        if (headDic.ContainsKey("Meta Description (Thai)"))
                                        {
                                            current.MetaDescriptionTh = tmpVariant.MetaDescriptionTh;
                                        }
                                        if (headDic.ContainsKey("Meta Keywords (English)"))
                                        {
                                            current.MetaKeyEn = tmpVariant.MetaKeyEn;
                                        }
                                        if (headDic.ContainsKey("Meta Keywords (Thai)"))
                                        {
                                            current.MetaKeyTh = tmpVariant.MetaKeyTh;
                                        }
                                        if (headDic.ContainsKey("Product URL Key(English)"))
                                        {
                                            if (!string.IsNullOrEmpty(tmpVariant.UrlEn))
                                            {
                                                current.UrlEn = tmpVariant.UrlEn;
                                            }
                                        }
                                        if (headDic.ContainsKey("Product Boosting Weight"))
                                        {
                                            current.BoostWeight = tmpVariant.BoostWeight;
                                        }
                                        #endregion
                                        var map = current.ProductStageVariantArrtibuteMaps.ToList();
                                        foreach (var variantMapAttr in tmpVariant.ProductStageVariantArrtibuteMaps)
                                        {
                                            bool isNewMap = false;
                                            if (map == null || map.Count == 0)
                                            {
                                                isNewMap = true;
                                            }
                                            if (!isNewMap)
                                            {
                                                var currentMap = map.Where(w => w.AttributeId == variantMapAttr.AttributeId).SingleOrDefault();
                                                if (currentMap != null)
                                                {
                                                    currentMap.Value = variantMapAttr.Value;
                                                    currentMap.IsAttributeValue = variantMapAttr.IsAttributeValue;
                                                    currentMap.UpdatedBy = this.User.UserRequest().Email;
                                                    currentMap.UpdatedDt = DateTime.Now;
                                                    map.Remove(currentMap);
                                                }
                                            }
                                            if (isNewMap)
                                            {
                                                current.ProductStageVariantArrtibuteMaps.Add(variantMapAttr);
                                            }
                                        }

                                        current.UpdatedBy = this.User.UserRequest().Email;
                                        current.UpdatedDt = DateTime.Now;
                                        oldVariantList.Remove(current);
                                    }
                                    else
                                    {
                                        isNewSet = true;
                                    }
                                }
                                if (isNewSet)
                                {
                                    string pid = AutoGenerate.NextPID(db, product.Value.GlobalCatId);
                                    tmpVariant.Pid = pid;
                                    if (string.IsNullOrWhiteSpace(tmpVariant.UrlEn))
                                    {
                                        tmpVariant.UrlEn = pid;
                                    }
                                    inventoryList.Add(pid, inventoryList[product.Value.Pid]);
                                    stage.ProductStageVariants.Add(tmpVariant);
                                }
                            }
                        }
                        if(oldVariantList!=null&&oldVariantList.Count > 0)
                        {
                            db.ProductStageVariants.RemoveRange(oldVariantList);
                        }
                    }
                    if (errorMessage.Count > 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, errorMessage.ToList());
                    }
                    var inventoryPid = inventoryList.Select(s => s.Key).ToList();
                    var invenLst = db.Inventories.Where(w => inventoryPid.Contains(w.Pid)).ToList();
                    foreach (var key in inventoryList)
                    {
                        var current = invenLst.Where(w => w.Pid.Equals(key.Key)).SingleOrDefault();
                        if(current != null)
                        {
                            current.Quantity = key.Value.Quantity;
                            current.SafetyStockSeller = key.Value.SafetyStockSeller;
                            current.UpdatedBy = this.User.UserRequest().Email;
                            current.UpdatedDt = DateTime.Now;
                        }
                    }
                    Util.DeadlockRetry(db.SaveChanges, "ProductStage");
                    return Request.CreateResponse(HttpStatusCode.OK, "Total " + groupList.Count + " products");
                }
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
                                    //PriceTo = p.ProductStageVariants.Max(m => m.SalePrice),
                                    //PriceFrom = p.ProductStageVariants.Min(m => m.SalePrice),
                                    //PriceTo = p.ProductStageVariants.Count == 0 ?  p.SalePrice :
                                    //        p.SalePrice < p.ProductStageVariants.Max(m => m.SalePrice)
                                    //        ? p.ProductStageVariants.Where(w => true).Max(m => m.SalePrice) : p.SalePrice,
                                    //PriceFrom = p.SalePrice < p.ProductStageVariants.Where(w => true).Min(m => m.SalePrice)
                                    //        ? p.SalePrice : p.ProductStageVariants.Where(w => true).Min(m => m.SalePrice),
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
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
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
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
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
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
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
                        variantInventoryHist.CreatedBy = this.User.UserRequest().Email;
                        variantInventoryHist.CreatedDt = DateTime.Now;
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
                        variant.CreatedBy = this.User.UserRequest().Email;
                        variant.CreatedDt = DateTime.Now;
                        variant.UpdatedBy = this.User.UserRequest().Email;
                        variant.UpdatedDt = DateTime.Now;
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
                stage.UpdatedBy = this.User.UserRequest().Email;
                stage.UpdatedDt = DateTime.Now;
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
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
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
                            attriEntity.CreatedBy = this.User.UserRequest().Email;
                            attriEntity.CreatedDt = DateTime.Now;
                            attriEntity.UpdatedBy = this.User.UserRequest().Email;
                            attriEntity.UpdatedDt = DateTime.Now;
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
                        current.CreatedBy = this.User.UserRequest().Email;
                        current.CreatedDt = DateTime.Now;
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
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
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
                                    currentVal.UpdatedBy = this.User.UserRequest().Email;
                                    currentVal.UpdatedDt = DateTime.Now;
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
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
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
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
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
                                    currentVal.UpdatedBy = this.User.UserRequest().Email;
                                    currentVal.UpdatedDt = DateTime.Now;
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
                                    CreatedBy = this.User.UserRequest().Email,
                                    CreatedDt = DateTime.Now,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
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
                    current.UpdatedBy = this.User.UserRequest().Email;
                    current.UpdatedDt = DateTime.Now;
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
                    current.UpdatedBy = this.User.UserRequest().Email;
                    current.UpdatedDt = DateTime.Now;
                }
                Util.DeadlockRetry(db.SaveChanges, "ProductStage");
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
                    current.UpdatedBy = this.User.UserRequest().Email;
                    current.UpdatedDt = DateTime.Now;
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
                                //make header for attribute
                                foreach (var attr in p.AttributeSet.Attribute)
                                {
                                    if (!headDicTmp.ContainsKey(attr.AttributeNameEn))
                                    {
                                        headDicTmp.Add(attr.AttributeNameEn, new Tuple<string, int>(attr.AttributeNameEn, i++));
                                        bodyList.Add(string.Empty);
                                    }
                                }

                                bodyList[headDicTmp["ATS"].Item2] = p.AttributeSet.AttributeSetNameEn;
                                //make vaiant option 1 value
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
                                {
                                    bodyList[headDicTmp["VO1"].Item2] = p.VariantAttribute.ToList()[0].AttributeNameEn;
                                }
                                //make vaiant option 2 value
                                if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 1)
                                {
                                    bodyList[headDicTmp["VO2"].Item2] = p.VariantAttribute.ToList()[1].AttributeNameEn;
                                }
                                //make master attribute value
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

        //[Route("api/ProductStages/Export")]
        //[HttpPost]
        //public HttpResponseMessage ExportProduct(ExportRequest request)
        //{
        //    MemoryStream stream = null;
        //    StreamWriter writer = null;
        //    try
        //    {
        //        if(request == null)
        //        {
        //            throw new Exception("Invalid request");
        //        }
        //        #region Query

        //        var query = (
        //                     from mast in db.ProductStages
        //                     join variant in db.ProductStageVariants on mast.ProductId equals variant.ProductId into varJoin
        //                     from vari in varJoin.DefaultIfEmpty()
        //                     //where productIds.Contains(mast.ProductId) && mast.ShopId == shopId
        //                     select new
        //                     {
        //                         ShopId = vari != null ? vari.ShopId : mast.ShopId,
        //                         Status = vari != null ? vari.Status : mast.Status,
        //                         Sku = vari != null ? vari.Sku : mast.Sku,
        //                         Pid = vari != null ? vari.Pid : mast.Pid,
        //                         Upc = vari != null ? vari.Upc : mast.Upc,
        //                         ProductId = vari != null ? vari.ProductId : mast.ProductId,
        //                         //GroupNameEn = mast.ProductNameEn,
        //                         //GroupNameTh = mast.ProductNameTh,
        //                         ProductNameEn = vari != null ? vari.ProductNameEn : mast.ProductNameEn,
        //                         ProductNameTh = vari != null ? vari.ProductNameTh : mast.ProductNameTh,
        //                         DefaultVaraint = vari != null ? vari.DefaultVaraint == true ? "Yes" : "No" : "Yes",
        //                         mast.Brand.BrandNameEn,
        //                         mast.GlobalCatId,
        //                         mast.LocalCatId,
        //                         OriginalPrice = vari != null ? vari.OriginalPrice : mast.OriginalPrice,
        //                         SalePrice = vari != null ? vari.SalePrice : mast.SalePrice,
        //                         DescriptionShortEn = vari != null ? vari.DescriptionShortEn : mast.DescriptionShortEn,
        //                         DescriptionShortTh = vari != null ? vari.DescriptionShortTh : mast.DescriptionShortTh,
        //                         DescriptionFullEn = vari != null ? vari.DescriptionFullEn : mast.DescriptionFullEn,
        //                         DescriptionFullTh = vari != null ? vari.DescriptionFullTh : mast.DescriptionFullTh,
        //                         AttributeSet = new { mast.AttributeSetId, mast.AttributeSet.AttributeSetNameEn, Attribute = mast.AttributeSet.AttributeSetMaps.Select(s => s.Attribute) },
        //                         mast.PrepareDay,
        //                         Length = vari != null ? vari.Length : mast.Length,
        //                         Height = vari != null ? vari.Height : mast.Height,
        //                         Width = vari != null ? vari.Width : mast.Width,
        //                         Weight = vari != null ? vari.Weight : mast.Weight,
        //                         mast.Tag,
        //                         mast.MetaTitleEn,
        //                         mast.MetaTitleTh,
        //                         mast.MetaDescriptionEn,
        //                         mast.MetaDescriptionTh,
        //                         mast.MetaKeyEn,
        //                         mast.MetaKeyTh,
        //                         mast.UrlEn,
        //                         mast.BoostWeight,
        //                         mast.EffectiveDate,
        //                         mast.EffectiveTime,
        //                         mast.ExpiryDate,
        //                         mast.ExpiryTime,
        //                         mast.Remark,
        //                         VariantAttribute = vari.ProductStageVariantArrtibuteMaps.Select(s => new
        //                         {
        //                             s.Attribute.AttributeNameEn,
        //                             Value = s.IsAttributeValue ? (from tt in db.AttributeValues where tt.MapValue.Equals(s.Value) select tt.AttributeValueEn).FirstOrDefault()
        //                                 : s.Value,
        //                         }),
        //                         MasterAttribute = mast.ProductStageAttributes.Select(s => new
        //                         {
        //                             s.AttributeId,
        //                             s.Attribute.AttributeNameEn,
        //                             ValueEn = s.IsAttributeValue ?
        //                                        (from tt in db.AttributeValues where tt.MapValue.Equals(s.ValueEn) select tt.AttributeValueEn).FirstOrDefault()
        //                                        : s.ValueEn,
        //                         }),
        //                         RelatedProduct = (from rel in db.ProductStageRelateds where rel.Pid1.Equals(mast.Pid) select rel.Pid2).ToList(),
        //                         Inventory = vari != null ? (from inv in db.Inventories where inv.Pid.Equals(vari.Pid) select inv).FirstOrDefault() :
        //                                      (from inv in db.Inventories where inv.Pid.Equals(mast.Pid) select inv).FirstOrDefault(),
        //                     });
        //        var productIds = request.ProductList.Where(w=>w.ProductId != null).Select(s => s.ProductId.Value).ToList();
               
        //        if (productIds != null && productIds.Count > 0)
        //        {
        //            if (productIds.Count > 2000)
        //            {
        //                throw new Exception("Too many product selected");
        //            }
        //            query = query.Where(w => productIds.Contains(w.ProductId));
        //        }
        //        if (this.User.ShopRequest() != null)
        //        {
        //            var shopId = this.User.ShopRequest().ShopId.Value;
        //            query = query.Where(w => w.ShopId==shopId);
        //        }
        //        var productList = query.ToList();

        //        #endregion
        //        #region Initiate Header
        //        int i = 0;
        //        Dictionary<string, int> headDic = new Dictionary<string, int>();
        //        if (request.ProductStatus)
        //        {
        //            headDic.Add("Product Status",i++);
        //        }
        //        if (request.SKU)
        //        {
        //            headDic.Add("SKU*", i++);
        //        }
        //        if (request.PID)
        //        {
        //            headDic.Add("PID", i++);
        //        }
        //        if (request.UPC)
        //        {
        //            headDic.Add("UPC", i++);
        //        }
        //        if (request.GroupID)
        //        {
        //            headDic.Add("Group ID", i++);
        //        }
        //        //if (request.GroupNameEn)
        //        //{
        //        //    headDic.Add("Group Name (English)", i++);
        //        //}
        //        //if (request.GroupNameTh)
        //        //{
        //        //    headDic.Add("Group Name (Thai)", i++);
        //        //}
        //        if (request.DefaultVariant)
        //        {
        //            headDic.Add("Default Variant", i++);
        //        }
        //        if (request.ProductNameEn)
        //        {
        //            headDic.Add("Product Name (English)*", i++);
        //        }
        //        if (request.ProductNameTh)
        //        {
        //            headDic.Add("Product Name (Thai)*", i++);
        //        }
        //        if (request.BrandName)
        //        {
        //            headDic.Add("Brand Name*", i++);
        //        }
        //        if (request.GlobalCategory)
        //        {
        //            headDic.Add("Global Category ID*", i++);
        //        }
        //        if (request.LocalCategory)
        //        {
        //            headDic.Add("Local Category ID*", i++);
        //        }
        //        if (request.OriginalPrice)
        //        {
        //            headDic.Add("Original Price*", i++);
        //        }
        //        if (request.SalePrice)
        //        {
        //            headDic.Add("Sale Price", i++);
        //        }
        //        if (request.DescriptionEn)
        //        {
        //            headDic.Add("Description (English)*", i++);
        //        }
        //        if (request.DescriptionTh)
        //        {
        //            headDic.Add("Description (Thai)*", i++);
        //        }
        //        if (request.ShortDescriptionEn)
        //        {
        //            headDic.Add("Short Description (English)", i++);
        //        }
        //        if (request.ShortDescriptionTh)
        //        {
        //            headDic.Add("Short Description (Thai)", i++);
        //        }
        //        if (request.PreparationTime)
        //        {
        //            headDic.Add("Preparation Time*", i++);
        //        }
        //        if (request.PackageLenght)
        //        {
        //            headDic.Add("Package Dimension - Lenght (mm)*", i++);
        //        }
        //        if (request.PackageHeight)
        //        {
        //            headDic.Add("Package Dimension - Height (mm)*", i++);
        //        }
        //        if (request.PackageWidth)
        //        {
        //            headDic.Add("Package Dimension - Width (mm)*", i++);
        //        }
        //        if (request.PackageWeight)
        //        {
        //            headDic.Add("Package -Weight (g)*", i++);
        //        }

        //        if (request.InventoryAmount)
        //        {
        //            headDic.Add("Inventory Amount", i++);
        //        }
        //        if (request.SafetytockAmount)
        //        {
        //            headDic.Add("Safety Stock Amount", i++);
        //        }
        //        if (request.SearchTag)
        //        {
        //            headDic.Add("Search Tag*", i++);
        //        }
        //        if (request.RelatedProducts)
        //        {
        //            headDic.Add("Related Products", i++);
        //        }
        //        if (request.MetaTitleEn)
        //        {
        //            headDic.Add("Meta Title (English)", i++);
        //        }
        //        if (request.MetaTitleTh)
        //        {
        //            headDic.Add("Meta Title (Thai)", i++);
        //        }
        //        if (request.MetaDescriptionEn)
        //        {
        //            headDic.Add("Meta Description (English)", i++);
        //        }
        //        if (request.MetaDescriptionTh)
        //        {
        //            headDic.Add("Meta Description (Thai)", i++);
        //        }
        //        if (request.MetaKeywordEn)
        //        {
        //            headDic.Add("Meta Keywords (English)", i++);
        //        }
        //        if (request.MetaKeywordTh)
        //        {
        //            headDic.Add("Meta Keywords (Thai)", i++);
        //        }
        //        if (request.ProductURLKeyEn)
        //        {
        //            headDic.Add("Product URL Key(English)", i++);
        //        }
        //        if (request.ProductBoostingWeight)
        //        {
        //            headDic.Add("Product Boosting Weight", i++);
        //        }
        //        if (request.EffectiveDate)
        //        {
        //            headDic.Add("Effective Date", i++);
        //        }
        //        if (request.EffectiveTime)
        //        {
        //            headDic.Add("Effective Time", i++);
        //        }

        //        if (request.ExpiryDate)
        //        {
        //            headDic.Add("Expiry Date", i++);
        //        }
        //        if (request.ExpiryTime)
        //        {
        //            headDic.Add("Expiry Time", i++);
        //        }
        //        if (request.Remark)
        //        {
        //            headDic.Add("Remark", i++);
        //        }
        //        #endregion
        //        List<List<string>> rs = new List<List<string>>();
        //        foreach (var p in productList)
        //        {
        //            List<string> bodyList = new List<string>();
        //            #region Assign Value
        //            if (request.ProductStatus)
        //            {
        //                if (Constant.PRODUCT_STATUS_DRAFT.Equals(p.Status))
        //                {
        //                    bodyList.Add(Validation.ValidateCSVColumn("Draft"));
        //                }
        //                else if (Constant.PRODUCT_STATUS_WAIT_FOR_APPROVAL.Equals(p.Status))
        //                {
        //                    bodyList.Add(Validation.ValidateCSVColumn("Wait for Approval"));
        //                }
        //                else if (Constant.PRODUCT_STATUS_APPROVE.Equals(p.Status))
        //                {
        //                    bodyList.Add(Validation.ValidateCSVColumn("Approve"));
        //                }
        //                else if (Constant.PRODUCT_STATUS_NOT_APPROVE.Equals(p.Status))
        //                {
        //                    bodyList.Add(Validation.ValidateCSVColumn("Not Approve"));
        //                }
        //            }
        //            if (request.SKU)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Sku));
        //            }
        //            if (request.PID)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Pid));
        //            }
        //            if (request.UPC)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Upc));
        //            }
        //            if (request.GroupID)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.ProductId));
        //            }
        //            //if (request.GroupNameEn)
        //            //{
        //            //    bodyList.Add(Validation.ValidaetCSVColumn(p.GroupNameEn));
        //            //}
        //            //if (request.GroupNameTh)
        //            //{
        //            //    bodyList.Add(Validation.ValidaetCSVColumn(p.GroupNameTh));
        //            //}
        //            if (request.DefaultVariant)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.DefaultVaraint));
        //            }
        //            if (request.ProductNameEn)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.ProductNameEn));
        //            }
        //            if (request.ProductNameTh)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.ProductNameTh));
        //            }
        //            if (request.BrandName)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.BrandNameEn));
        //            }
        //            if (request.GlobalCategory)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.GlobalCatId));
        //            }
        //            if (request.LocalCategory)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.LocalCatId));
        //            }
        //            if (request.OriginalPrice)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.OriginalPrice));
        //            }
        //            if (request.SalePrice)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.SalePrice));
        //            }
        //            if (request.DescriptionEn)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionFullEn));
        //            }
        //            if (request.DescriptionTh)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionFullTh));
        //            }
        //            if (request.ShortDescriptionEn)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionShortEn));
        //            }
        //            if (request.ShortDescriptionTh)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.DescriptionShortTh));
        //            }
        //            if (request.PreparationTime)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.PrepareDay));
        //            }
        //            if (request.PackageLenght)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Length));
        //            }
        //            if (request.PackageHeight)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Height));
        //            }
        //            if (request.PackageWidth)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Width));
        //            }
        //            if (request.PackageWeight)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Weight));
        //            }

        //            if (request.InventoryAmount)
        //            {
        //                if (p.Inventory != null)
        //                {
        //                    bodyList.Add(Validation.ValidateCSVColumn(p.Inventory.Quantity));
        //                }
        //                else
        //                {
        //                    bodyList.Add(string.Empty);
        //                }
        //            }
        //            if (request.SafetytockAmount)
        //            {
        //                if (p.Inventory != null)
        //                {
        //                    bodyList.Add(Validation.ValidateCSVColumn(p.Inventory.SaftyStockSeller));
        //                }
        //                else
        //                {
        //                    bodyList.Add(string.Empty);
        //                }
        //            }
        //            if (request.SearchTag)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Tag));
        //            }
        //            if (request.RelatedProducts)
        //            {
        //                if (p.RelatedProduct != null && p.RelatedProduct.Count > 0)
        //                {
        //                    bodyList.Add(Validation.ValidateCSVColumn(string.Join(",", p.RelatedProduct)));
        //                }
        //                else
        //                {
        //                    bodyList.Add(string.Empty);
        //                }
        //            }
        //            if (request.MetaTitleEn)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.MetaTitleEn));
        //            }
        //            if (request.MetaTitleTh)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.MetaTitleTh));
        //            }
        //            if (request.MetaDescriptionEn)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.MetaDescriptionEn));
        //            }
        //            if (request.MetaDescriptionTh)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.MetaDescriptionTh));
        //            }
        //            if (request.MetaKeywordEn)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.MetaKeyEn));
        //            }
        //            if (request.MetaKeywordTh)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.MetaKeyTh));
        //            }
        //            if (request.ProductURLKeyEn)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.UrlEn));
        //            }
        //            if (request.ProductBoostingWeight)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.BoostWeight));
        //            }
        //            if (request.EffectiveDate)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.EffectiveDate));
        //            }
        //            if (request.EffectiveTime)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.EffectiveTime));
        //            }

        //            if (request.ExpiryDate)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.ExpiryDate));
        //            }
        //            if (request.ExpiryTime)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.ExpiryTime));
        //            }
        //            if (request.Remark)
        //            {
        //                bodyList.Add(Validation.ValidateCSVColumn(p.Remark));
        //            }
        //            #endregion
        //            #region Attibute Section
        //            if (request.AttributeSets != null && request.AttributeSets.Count > 0)
        //            {
        //                if (p.AttributeSet != null)
        //                {
        //                    var set = request.AttributeSets.Where(w => w.AttributeSetId == p.AttributeSet.AttributeSetId).SingleOrDefault();
        //                    if (set != null)
        //                    {
        //                        if (!headDic.ContainsKey("Attribute Set"))
        //                        {
        //                            headDic.Add("Attribute Set",i++);
        //                            headDic.Add("Variation Option 1", i++);
        //                            headDic.Add("Variation Option 2", i++);
        //                        }
        //                        bodyList.Add(Validation.ValidateCSVColumn(p.AttributeSet.AttributeSetNameEn));
        //                        if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
        //                        {
        //                            bodyList.Add(Validation.ValidateCSVColumn(p.VariantAttribute.ToList()[0].AttributeNameEn));
        //                            if(p.VariantAttribute.ToList().Count > 1)
        //                            {
        //                                bodyList.Add(Validation.ValidateCSVColumn(p.VariantAttribute.ToList()[1].AttributeNameEn));
        //                            }
        //                            else
        //                            {
        //                                bodyList.Add(string.Empty);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            bodyList.Add(string.Empty);
        //                            bodyList.Add(string.Empty);
        //                        }
        //                        foreach (var attr in p.AttributeSet.Attribute)
        //                        {
        //                            if (!headDic.ContainsKey(attr.AttributeNameEn))
        //                            {
        //                                headDic.Add(attr.AttributeNameEn, i++);
        //                            }
        //                            bodyList.Add(string.Empty);
        //                        }
        //                        if(p.MasterAttribute != null && p.MasterAttribute.ToList().Count > 0)
        //                        {
        //                            foreach (var masterValue in p.MasterAttribute)
        //                            {
        //                                if (headDic.ContainsKey(masterValue.AttributeNameEn))
        //                                {
        //                                    int desColumn = headDic[masterValue.AttributeNameEn];
        //                                    for(int j = bodyList.Count;j <= desColumn; j++)
        //                                    {
        //                                        bodyList.Add(string.Empty);
        //                                    }
        //                                    bodyList[desColumn] = masterValue.ValueEn;
        //                                }
        //                            }
        //                        }
        //                        if (p.VariantAttribute != null && p.VariantAttribute.ToList().Count > 0)
        //                        {
        //                            foreach (var variantValue in p.VariantAttribute)
        //                            {
        //                                if (headDic.ContainsKey(variantValue.AttributeNameEn))
        //                                {
        //                                    int desColumn = headDic[variantValue.AttributeNameEn];
        //                                    for (int j = bodyList.Count; j <= desColumn; j++)
        //                                    {
        //                                        bodyList.Add(string.Empty);
        //                                    }
        //                                    bodyList[desColumn] = variantValue.Value;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            #endregion
        //            rs.Add(bodyList);
        //        }
        //        #region Write header
        //        stream = new MemoryStream();
        //        writer = new StreamWriter(stream);
        //        var csv = new CsvWriter(writer);
        //        foreach (KeyValuePair<string, int> entry in headDic)
        //        {
        //            csv.WriteField(entry.Key);
        //        }
        //        csv.NextRecord();
        //        #endregion
        //        #region Write body
        //        foreach (List<string> r in rs)
        //        {
        //            foreach( string field in r)
        //            {
        //                csv.WriteField(field);
        //            }
        //            csv.NextRecord();
        //        }
        //        #endregion
        //        #region Create Response
        //        writer.Flush();
        //        stream.Position = 0;

        //        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
        //        result.Content = new StreamContent(stream);
        //        result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream")
        //        {
        //            CharSet = Encoding.UTF8.WebName
        //        };
        //        result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
        //        result.Content.Headers.ContentDisposition.FileName = "file.csv";
        //        #endregion
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        #region close writer
        //        if (writer != null)
        //        {
        //            writer.Close();
        //            writer.Dispose();
        //        }
        //        if (stream != null)
        //        {
        //            stream.Close();
        //            stream.Dispose();
        //        }
        //        #endregion
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
        //    }
        //}


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
            //var stage = (from productStage in db.ProductStages
            //           join brand in db.Brands on productStage.BrandId equals brand.BrandId
            //           join productStageAttribute in db.ProductStageAttributes on productStage.ProductId equals productStageAttribute.ProductId
            //           join productStageVariant in db.ProductStageVariants.Include(i=>i.ProductStageVariantArrtibuteMaps) on productStage.ProductId equals productStageVariant.ProductId into Variant
            //           where productStage.ProductId == productId && productStage.ShopId == shopId
            //           select new
            //           {
            //               productStage.ProductNameTh,
            //               productStage.ProductNameEn,
            //               productStage.Sku,
            //               productStage.Upc,
            //               Brand  = new { brand.BrandId, brand.BrandNameEn },
            //               productStage.OriginalPrice,
            //               productStage.SalePrice,
            //               productStage.DescriptionFullTh,
            //               productStage.DescriptionShortTh,
            //               productStage.DescriptionFullEn,
            //               productStage.DescriptionShortEn,
            //               productStage.AttributeSetId,
            //               productStage.Tag,
            //               productStage.ShippingId,
            //               productStage.PrepareDay,
            //               productStage.Length,
            //               productStage.Height,
            //               productStage.Width,
            //               productStage.Weight,
            //               productStage.DimensionUnit,
            //               productStage.WeightUnit,
            //               productStage.GlobalCatId,
            //               productStage.LocalCatId,
            //               productStage.MetaTitleEn,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               productStage.Weight,
            //               ProductStageVariants = Variant.ToList(),

            //           }).SingleOrDefault();

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
                    //else
                    //{
                    //    throw new Exception("Invalid attribute value");
                    //}
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
                //else if(!string.IsNullOrWhiteSpace(attr.ValueEn))
                //{
                //    Regex rg = new Regex(@"/(\(\()\d+(\)\))/");
                //    if (rg.IsMatch(attr.ValueEn))
                //    {
                //        throw new Exception("Attribute value not allow");
                //    }
                //    attriEntity.ValueEn = attr.ValueEn;
                //}
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