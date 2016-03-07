using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using Colsp.Api.Helpers;

namespace Colsp.Api.Controllers
{
    public class BrandsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/BrandImages")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Content Multimedia");
                }
                string tmpFolder = Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.BRAND_FOLDER);
                var streamProvider = new MultipartFormDataStreamProvider(tmpFolder);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                FileUploadRespond fileUpload = new FileUploadRespond();
                string fileName = string.Empty;
                string ext = string.Empty;
                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    fileName = fileData.LocalFileName;
                    string tmp = fileData.Headers.ContentDisposition.FileName;
                    if (tmp.StartsWith("\"") && tmp.EndsWith("\""))
                    {
                        tmp = tmp.Trim('"');
                    }
                    ext = Path.GetExtension(tmp);
                    break;
                }

                string newName = string.Concat(fileName, ext);
                File.Move(fileName, newName);
                fileUpload.tmpPath = newName;

                var name = Path.GetFileName(newName);
                var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
                var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;
                fileUpload.url = string.Concat(schema, "://", imageUrl,"/", AppSettingKey.IMAGE_ROOT_FOLDER,"/", AppSettingKey.BRAND_FOLDER,"/", name);

                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,e);
            }
        }

        [Route("api/Brands")]
        [HttpGet]
        public HttpResponseMessage GetBrand([FromUri] BrandRequest request)
        {
            try
            {

                //var attrList = from brand in db.Brands
                //               where !brand.Status.Equals(Constant.STATUS_REMOVE)
                //               select new
                //               {
                //                   brand.BrandNameEn,
                //                   brand.BrandNameTh,
                //                   brand.PicUrl,
                //                   brand.DisplayNameTh,
                //                   brand.VariantStatus,
                //                   brand.DataType,
                //                   brand.Status,
                //                   brand.CreatedDt,
                //                   brand.UpdatedDts
                //               };



                IQueryable<Brand> brands = null;
                // List all brand
                brands = db.Brands.Where(p => !p.Status.Equals(Constant.STATUS_REMOVE));
                //brands = brands.Where(b => b.Status.Equals(Constant.STATUS_ACTIVE));

                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, brands);
                }
                else
                {
                    request.DefaultOnNull();
                    if (request.SearchText != null)
                    {
                        brands = brands.Where(b => b.BrandNameEn.Contains(request.SearchText)
                        || b.BrandNameTh.Contains(request.SearchText));
                    }
                    if (request.BrandId != null)
                    {
                        brands = brands.Where(p => p.BrandId.Equals(request.BrandId));
                    }
                    var total = brands.Count();
                    var response = PaginatedResponse.CreateResponse(brands.Paginate(request), request, total);
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Brands/{brandId}")]
        [HttpGet]
        public HttpResponseMessage GetBrand([FromUri]int brandId)
        {
            try
            {
                var brand = db.Brands
                    .Where(w => w.BrandId == brandId).Include(i => i.BrandImages)
                    .Include(i=>i.BrandFeatureProducts.Select(s=>s.ProductStage)).SingleOrDefault();
                if(brand != null)
                {
                    BrandRequest response = new BrandRequest();
                    response.BrandId = brand.BrandId;
                    response.BrandNameEn = brand.BrandNameEn;
                    response.BrandNameTh = brand.BrandNameTh;
                    response.DisplayNameEn = brand.DisplayNameEn;
                    response.DisplayNameTh = brand.DisplayNameTh;
                    response.DescriptionFullEn = brand.DescriptionFullEn;
                    response.DescriptionFullTh = brand.DescriptionFullEn;
                    response.DescriptionShortEn = brand.DescriptionShortEn;
                    response.DescriptionShortTh = brand.DescriptionShortTh;
                    response.SEO = new SEORequest();
                    response.SEO.MetaDescriptionEn = brand.MetaDescriptionEn;
                    response.SEO.MetaDescriptionTh = brand.MetaDescriptionTh;
                    response.SEO.MetaKeywordEn = brand.MetaKeyEn;
                    response.SEO.MetaKeywordTh = brand.MetaKeyTh;
                    response.SEO.MetaTitleEn = brand.MetaTitleEn;
                    response.SEO.MetaTitleTh = brand.MetaTitleTh;
                    response.SEO.ProductUrlKeyEn = brand.UrlEn;
                    response.FeatureTitle = brand.FeatureTitle;
                    response.TitleShowcase = brand.TitleShowcase;
                    if (brand.BrandImages != null && brand.BrandImages.Count > 0)
                    {
                        var productImgEn = brand.BrandImages.Where(w => Constant.LANG_EN.Equals(w.EnTh)).OrderBy(o=>o.Position).ToList();
                        foreach(var img in productImgEn)
                        {
                            response.BrandBannerEn.Add(new ImageRequest()
                            {
                                ImageId = img.BrandImageId,
                                url = img.ImageUrl,
                                position = img.Position,
                            });
                        }
                        var productImgTh = brand.BrandImages.Where(w => Constant.LANG_TH.Equals(w.EnTh)).OrderBy(o => o.Position).ToList();
                        foreach (var img in productImgTh)
                        {
                            response.BrandBannerTh.Add(new ImageRequest()
                            {
                                url = img.ImageUrl,
                                position = img.Position,
                            });
                        }
                    }
                    if(brand.BrandFeatureProducts != null && brand.BrandFeatureProducts.Count > 0)
                    {
                        foreach(var pro in brand.BrandFeatureProducts)
                        {
                            response.FeatureProducts.Add(new ProductRequest()
                            {
                                ProductId = pro.ProductStage.ProductId,
                                Pid = pro.ProductStage.Pid,
                                ProductNameEn = pro.ProductStage.ProductNameEn
                            });
                        }
                    }
                    if (!string.IsNullOrEmpty(brand.PicUrl))
                    {
                        response.BrandImage = new ImageRequest();
                        response.BrandImage.url = brand.PicUrl;
                        response.BrandImage.tmpPath = brand.Path;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Brands")]
        [HttpDelete]
        public HttpResponseMessage DeleteBrand(List<BrandRequest> request)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var brandList = db.Brands.Include(i => i.ProductStages).ToList();
                foreach (BrandRequest brandRq in request)
                {
                    var current = brandList.Where(w => w.BrandId.Equals(brandRq.BrandId)).SingleOrDefault();
                    if (current == null)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                    }
                    if (current.ProductStages != null && current.ProductStages.Count > 0)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Brand has product or variant associate");
                    }
                    current.Status = Constant.STATUS_REMOVE;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/Brands")]
        [HttpPost]
        public HttpResponseMessage AddBrand(BrandRequest request)
        {
            try
            {
                Brand brand = new Brand();
                SetupBrand(brand, request);
                #region BranImage En
                if (request.BrandBannerEn != null && request.BrandBannerEn.Count > 0)
                {
                    int position = 0;
                    foreach(ImageRequest img in request.BrandBannerEn)
                    {
                        brand.BrandImages.Add(new BrandImage()
                        {
                            ImageUrl = img.url,
                            Position = position++,
                            EnTh = Constant.LANG_EN,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
                        });
                    }
                }
                #endregion
                #region BranImage Th
                if (request.BrandBannerTh != null && request.BrandBannerTh.Count > 0)
                {
                    int position = 0;
                    foreach (ImageRequest img in request.BrandBannerTh)
                    {
                        brand.BrandImages.Add(new BrandImage()
                        {
                            ImageUrl = img.url,
                            Position = position++,
                            EnTh = Constant.LANG_TH,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now
                        });
                    }
                }
                #endregion
                brand.Status = Constant.STATUS_ACTIVE;
                brand.CreatedBy = this.User.UserRequest().Email;
                brand.CreatedDt = DateTime.Now;
                brand.UpdatedBy = this.User.UserRequest().Email;
                brand.UpdatedDt = DateTime.Now;
                db.Brands.Add(brand);
                db.SaveChanges();
                return GetBrand(brand.BrandId); ;
            }
            catch (DbUpdateException e)
            {
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        string message = ((SqlException)e.InnerException.InnerException).Message;
                        if (message.Contains("UrlEn"))
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "URL Key has already been used");
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "This brand name has already been used");
                        }
                        
                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Brands/{brandId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeBrand([FromUri]int brandId,BrandRequest request)
        {
            try
            {
                if(brandId == 0 || request == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid request");
                }
                var brand = db.Brands.Where(w => w.BrandId == brandId)
                    .Include(i=>i.BrandImages)
                    .Include(i=>i.BrandFeatureProducts).SingleOrDefault();
                if(brand != null)
                {
                    SetupBrand(brand, request);

                    #region BranImage En
                    var imageOldEn = brand.BrandImages.Where(w => Constant.LANG_EN.Equals(w.EnTh)).ToList();
                    if (request.BrandBannerEn != null && request.BrandBannerEn.Count > 0)
                    {
                        int position = 0;
                        foreach (ImageRequest img in request.BrandBannerEn)
                        {
                            bool isNew = false;
                            if(imageOldEn == null || imageOldEn.Count == 0)
                            {
                                isNew = true;
                            }
                            if (!isNew)
                            {
                                var current = imageOldEn.Where(w => w.BrandImageId == img.ImageId).SingleOrDefault();
                                if(current != null)
                                {
                                    current.ImageUrl = img.url;
                                    current.Position = position++;
                                    current.UpdatedBy = this.User.UserRequest().Email;
                                    current.UpdatedDt = DateTime.Now;
                                    imageOldEn.Remove(current);
                                }
                                else
                                {
                                    isNew = true;
                                }
                            }
                            if (isNew)
                            {
                                brand.BrandImages.Add(new BrandImage()
                                {
                                    ImageUrl = img.url,
                                    Position = position++,
                                    EnTh = Constant.LANG_EN,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
                                });
                            }
                        }
                    }
                    if(imageOldEn != null && imageOldEn.Count > 0)
                    {
                        db.BrandImages.RemoveRange(imageOldEn);
                    }
                    #endregion
                    #region BranImage Th
                    var imageOldTh = brand.BrandImages.Where(w => Constant.LANG_TH.Equals(w.EnTh)).ToList();
                    if (request.BrandBannerTh != null && request.BrandBannerTh.Count > 0)
                    {
                        int position = 0;
                        foreach (ImageRequest img in request.BrandBannerTh)
                        {
                            bool isNew = false;
                            if (imageOldTh == null || imageOldTh.Count == 0)
                            {
                                isNew = true;
                            }
                            if (!isNew)
                            {
                                var current = imageOldTh.Where(w => w.BrandImageId == img.ImageId).SingleOrDefault();
                                if (current != null)
                                {
                                    current.ImageUrl = img.url;
                                    current.Position = position++;
                                    current.UpdatedBy = this.User.UserRequest().Email;
                                    current.UpdatedDt = DateTime.Now;
                                    imageOldTh.Remove(current);
                                }
                                else
                                {
                                    isNew = true;
                                }
                            }
                            if (isNew)
                            {
                                brand.BrandImages.Add(new BrandImage()
                                {
                                    ImageUrl = img.url,
                                    Position = position++,
                                    EnTh = Constant.LANG_TH,
                                    UpdatedBy = this.User.UserRequest().Email,
                                    UpdatedDt = DateTime.Now
                                });
                            }
                        }
                    }
                    if (imageOldTh != null && imageOldTh.Count > 0)
                    {
                        db.BrandImages.RemoveRange(imageOldTh);
                    }
                    #endregion
                    #region Brand Feature Product
                    var brandProList = brand.BrandFeatureProducts.ToList();
                    if (request.FeatureProducts != null && request.FeatureProducts.Count > 0)
                    {
                        int? brandIdTmp = brand.BrandId;
                        var proStageList = db.ProductStages
                            .Where(w => w.BrandId==brandIdTmp)
                            .Select(s=>s.ProductId).ToList();
                        foreach (var pro in request.FeatureProducts)
                        {
                            bool isNew = false;
                            if(brandProList == null || brandProList.Count == 0)
                            {
                                isNew = true;
                            }
                            if (!isNew)
                            {
                                var current = brandProList.Where(w => w.ProductId==pro.ProductId).SingleOrDefault();
                                if(current != null)
                                {
                                    brandProList.Remove(current);
                                }
                                else
                                {
                                    isNew = true;
                                }
                            }
                            if (isNew)
                            {
                                var isPid = proStageList.Where(w => w == pro.ProductId).ToList();
                                if(isPid != null && isPid.Count > 0)
                                {
                                    brand.BrandFeatureProducts.Add(new BrandFeatureProduct()
                                    {
                                        BrandId = brand.BrandId,
                                        ProductId = pro.ProductId,
                                        CreatedBy = this.User.UserRequest().Email,
                                        CreatedDt = DateTime.Now,
                                        UpdatedBy = this.User.UserRequest().Email,
                                        UpdatedDt = DateTime.Now
                                    });
                                }
                                else
                                {
                                    throw new Exception("Pid " + pro.Pid + " is not in this brand." );
                                }
                            }
                        }
                    }
                    if(brandProList != null && brandProList.Count > 0)
                    {
                        db.BrandFeatureProducts.RemoveRange(brandProList);
                    }
                    #endregion
                    brand.Status = Constant.STATUS_ACTIVE;
                    brand.UpdatedBy = this.User.UserRequest().Email;
                    brand.UpdatedDt = DateTime.Now;
                    db.SaveChanges();
                    return GetBrand(brand.BrandId);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,HttpErrorMessage.NotFound);
                }
                
            }
            catch (DbUpdateException e)
            {
                if (e != null && e.InnerException != null && e.InnerException.InnerException != null)
                {
                    int sqlError = ((SqlException)e.InnerException.InnerException).Number;
                    if (sqlError == 2627)
                    {
                        string message = ((SqlException)e.InnerException.InnerException).Message;
                        if ("UrlEn".Contains(message))
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "URL Key has already been used");
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable
                           , "This brand name has already been used");
                        }

                    }
                }
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private void SetupBrand(Brand brand, BrandRequest request)
        {
            brand.BrandNameEn = Validation.ValidateString(request.BrandNameEn, "Brand Name (English)", true,100,true);
            brand.BrandNameTh = Validation.ValidateString(request.BrandNameTh, "Brand Name (Thai)", true, 100, false);
            brand.DisplayNameEn = Validation.ValidateString(request.DisplayNameEn, "Brand Display Name (Thai)", true, 300, false);
            brand.DisplayNameTh = Validation.ValidateString(request.DisplayNameTh, "Brand Display Name (Thai)", true, 300, false);
            brand.DescriptionFullEn = Validation.ValidateString(request.DescriptionFullEn, "Brand Description (English)", false, Int32.MaxValue, false);
            brand.DescriptionFullTh = Validation.ValidateString(request.DescriptionFullTh, "Brand Description (Thai)", false, Int32.MaxValue, false);
            brand.DescriptionShortEn = Validation.ValidateString(request.DescriptionFullEn, "Brand Description (English)", false, Int32.MaxValue, false);
            brand.DescriptionShortTh = Validation.ValidateString(request.DescriptionFullTh, "Brand Description (Thai)", false, Int32.MaxValue, false);
            brand.FeatureTitle = Validation.ValidateString(request.FeatureTitle, "Feature Products Title", false, 100, false);
            brand.TitleShowcase = request.TitleShowcase;
            if (request.SEO != null)
            {
                brand.MetaDescriptionEn = Validation.ValidateString(request.SEO.MetaDescriptionEn, "Meta Description (English)", false, 500, false);
                brand.MetaDescriptionTh = Validation.ValidateString(request.SEO.MetaDescriptionTh, "Meta Description (Thai)", false, 500, false);
                brand.MetaKeyEn = Validation.ValidateString(request.SEO.MetaKeywordEn, "Meta Keywords (English)", false, 500, false);
                brand.MetaKeyTh = Validation.ValidateString(request.SEO.MetaKeywordTh, "Meta Keywords (Thai)", false, 500, false);
                brand.MetaTitleEn = Validation.ValidateString(request.SEO.MetaTitleEn, "Meta Title (English)", false, 100, false);
                brand.MetaTitleTh = Validation.ValidateString(request.SEO.MetaTitleTh, "Meta Title (Thai)", false, 100, false);
               // brand.UrlTh = request.SEO.ProductUrlKeyTh;
            }
            if (request.SEO == null || string.IsNullOrWhiteSpace(request.SEO.ProductUrlKeyEn))
            {
                brand.UrlEn = brand.BrandNameEn;
            }
            else
            {
                brand.UrlEn = request.SEO.ProductUrlKeyEn;
            }
            if (request.BrandImage != null)
            {
                brand.Path = request.BrandImage.tmpPath;
                brand.PicUrl = request.BrandImage.url;
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

        private bool BrandExists(int id)
        {
            return db.Brands.Count(e => e.BrandId == id) > 0;
        }
    }
}