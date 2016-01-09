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

		// GET: api/ProductStages
		[ResponseType(typeof(PaginatedResponse))]
		public IHttpActionResult GetProductStages([FromUri] ProductRequest request)
		{
			request.DefaultOnNull();
			IQueryable<ProductStage> products = null;

			// List all product
			products = db.ProductStages.Where(p => true);
			if (request.SearchText != null)
			{
				products = products.Where(p => p.Sku.Contains(request.SearchText) 
                || p.ProductNameEn.Contains(request.SearchText) 
                || p.ProductNameTh.Contains(request.SearchText) );
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
			return Ok(response);
		}

		// GET: api/ProductStages/5
		[ResponseType(typeof(ProductStage))]
        public IHttpActionResult GetProductStage(int id)
        {
            ProductStage productStage = db.ProductStages.Find(id);
            if (productStage == null)
            {
                return NotFound();
            }

            return Ok(productStage);
        }

        // PUT: api/ProductStages/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProductStage(int id, ProductStage productStage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != productStage.ProductId)
            {
                return BadRequest();
            }

            db.Entry(productStage).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductStageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/ProductStages
        [ResponseType(typeof(ProductStage))]
        public IHttpActionResult PostProductStage(ProductStage productStage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.ProductStages.Add(productStage);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = productStage.ProductId }, productStage);
        }

        // DELETE: api/ProductStages/5
        [ResponseType(typeof(ProductStage))]
        public IHttpActionResult DeleteProductStage(int id)
        {
            ProductStage productStage = db.ProductStages.Find(id);
            if (productStage == null)
            {
                return NotFound();
            }

            db.ProductStages.Remove(productStage);
            db.SaveChanges();

            return Ok(productStage);
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
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.DATE_FORMAT_ERROR);
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
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.TIME_FORMAT_ERROR);
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
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.DATE_FORMAT_ERROR);
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
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.TIME_FORMAT_ERROR);
                    }
                }
                #endregion

                stage.Remark = request.Remark;
                stage.Status = request.Status;
                stage.SellerId = request.SellerId;
                stage.ShopId = request.ShopId;
                string masterPid = AutoGenerate.NextPID(db, stage.GlobalCatId);
                stage.Pid = masterPid;

                #region Setup Attribute
                if (request.DefaultVariant.FirstAttribute != null)
                {
                    SetupAttribute(db, request.DefaultVariant.FirstAttribute, masterPid);
                }
                if(request.DefaultVariant.SecondAttribute != null)
                {
                    SetupAttribute(db, request.DefaultVariant.SecondAttribute, masterPid);
                }
                #endregion

                #region Setup Inventory
                Inventory masterInventory = new Inventory();
                masterInventory.StockAvailable = request.DefaultVariant.Quantity;
                masterInventory.SaftyStockSeller = request.DefaultVariant.SafetyStock;
                if (Constant.STOCK_TYPE.ContainsKey(request.DefaultVariant.StockType))
                {
                    masterInventory.StockAvailable = Constant.STOCK_TYPE[request.DefaultVariant.StockType];
                }
                masterInventory.Pid = masterPid;
                db.Inventories.Add(masterInventory);
                #endregion

                SetupImg(db, request.DefaultVariant.Images, masterPid);
                SetupVdo(db, request.DefaultVariant.VideoLinks, masterPid);


                #region Master Validation

                if(stage.SellerId == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Seller is required");
                }
                if (stage.ShopId == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Shop is required");
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

                #endregion

                db.SaveChanges();

                #region Setup variant
                foreach (VariantRequest variantRq in request.Variants)
                {
                    if (variantRq.FirstAttribute == null ||
                        variantRq.SecondAttribute == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Invalid variant format");
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
                    if (Constant.STOCK_TYPE.ContainsKey(variantRq.StockType))
                    {
                        variantInventory.StockAvailable = Constant.STOCK_TYPE[variantRq.StockType];
                    }
                    variantInventory.Pid = variantPid;
                    db.Inventories.Add(variantInventory);
                    #endregion

                    //SetupAttribute(db, variantRq.FirstAttribute, variantPid);
                    //SetupAttribute(db, variantRq.SecondAttribute, variantPid);

                    varient.ValueEn = variantRq.ValueEn;

                    SetupImg(db, variantRq.Images, variantPid);
                    SetupVdo(db, variantRq.VideoLinks, variantPid);
                    
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

                    db.ProductStageVariants.Add(varient);
                }
                #endregion

                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                //rollback
                db.ProductStages.Remove(stage);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private void SetupAttribute(ColspEntities db,AttributeRequest attr, string pid)
        {
            ProductStageAttribute masterAtrributeFirst = new ProductStageAttribute();
            masterAtrributeFirst.Pid = pid;
            masterAtrributeFirst.AttributeId = attr.AttributeId;
            masterAtrributeFirst.ValueEn = attr.ValueEn;
            db.ProductStageAttributes.Add(masterAtrributeFirst);
        }

        private void SetupImg(ColspEntities db,List<ImageRequest> imgs,string pid)
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
                db.ProductStageImages.Add(img);
            }
        }

        private void SetupVdo(ColspEntities db,List<VideoLinkRequest> vdos,string pid)
        {
            if(vdos == null) { return; }
            int index = 0;
            foreach (VideoLinkRequest vdoRq in vdos)
            {
                ProductStageVideo vdo = new ProductStageVideo();
                vdo.Pid = pid;
                vdo.Position = index++;
                vdo.VideoUrlEn = vdoRq.Url;
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