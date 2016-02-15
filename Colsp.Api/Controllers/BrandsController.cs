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

        private readonly string root = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[AppSettingKey.IMAGE_ROOT_PATH]);

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
                string tmpFolder = Path.Combine(root, "Brand");
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
                fileUpload.url = string.Concat(schema, "://", imageUrl, "/Images/Brand/", name);

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
                var brand = db.Brands.Find(brandId);
                if(brand != null)
                {
                    BrandRequest response = new BrandRequest();
                    response.BrandId = brand.BrandId;
                    response.BrandNameEn = brand.BrandNameEn;
                    response.BrandNameTh = brand.BrandNameTh;
                    response.DescriptionEn = brand.DescriptionEn;
                    response.DescriptionTh = brand.DescriptionTh;
                    response.SEO = new SEORequest();
                    response.SEO.MetaDescriptionEn = brand.MetaDescriptionEn;
                    response.SEO.MetaDescriptionTh = brand.MetaDescriptionTh;
                    response.SEO.MetaKeywordEn = brand.MetaKeyEn;
                    response.SEO.MetaKeywordTh = brand.MetaKeyTh;
                    response.SEO.MetaTitleEn = brand.MetaTitleEn;
                    response.SEO.MetaTitleTh = brand.MetaTitleTh;
                    response.SEO.ProductUrlKeyEn = brand.UrlEn;
                    //response.SEO.ProductUrlKeyTh = brand.UrlTh;
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
                var brand = db.Brands.Where(w => w.BrandId == brandId).SingleOrDefault();
                if(brand != null)
                {
                    SetupBrand(brand, request);
                    brand.Status = Constant.STATUS_ACTIVE;
                    brand.UpdatedBy = this.User.UserRequest().Email;
                    brand.UpdatedDt = DateTime.Now;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private void SetupBrand(Brand brand, BrandRequest request)
        {
            brand.BrandNameEn = Validation.ValidateString(request.BrandNameEn, "Brand Name (English)", true,100,true);
            brand.BrandNameTh = Validation.ValidateString(request.BrandNameTh, "Brand Name (Thai)", true, 100, true);
            brand.DescriptionEn = Validation.ValidateString(request.DescriptionEn, "Brand Description (English)", false, 500, false);
            brand.DescriptionTh = Validation.ValidateString(request.DescriptionTh, "Brand Description (Thai)", false, 500, false);
            if(request.SEO != null)
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