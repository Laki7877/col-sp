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
                    response.BrandNameEn = brand.BrandNameEn;
                    response.BrandNameTh = brand.BrandNameTh;
                    response.DescriptionEn = brand.DescriptionEn;
                    response.DescriptionTh = brand.DescriptionTh;
                    response.SEO = new SEORequest();
                    response.SEO.MetaDescription = brand.MetaDescription;
                    response.SEO.MetaKeywords = brand.MetaKey;
                    response.SEO.MetaTitle = brand.MetaTitle;
                    response.SEO.ProductUrlKeyEn = brand.UrlEn;
                    response.SEO.ProductUrlKeyTh = brand.UrlTh;
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
                brand.BrandNameEn = request.BrandNameEn;
                brand.BrandNameTh = request.BrandNameTh;
                brand.DescriptionEn = request.DescriptionEn;
                brand.DescriptionTh = request.DescriptionTh;
                if(request.SEO != null)
                {
                    brand.MetaDescription = request.SEO.MetaDescription;
                    brand.MetaKey = request.SEO.MetaKeywords;
                    brand.MetaTitle = request.SEO.MetaTitle;
                    brand.UrlEn = request.SEO.ProductUrlKeyEn;
                    brand.UrlTh = request.SEO.ProductUrlKeyTh;
                }
                if(request.BrandImage != null)
                {
                    brand.Path = request.BrandImage.tmpPath;
                    brand.PicUrl = request.BrandImage.url;
                }
                brand.Status = Constant.STATUS_ACTIVE;
                brand.CreatedBy = this.User.Email();
                brand.CreatedDt = DateTime.Now;
                brand.UpdatedBy = this.User.Email();
                brand.UpdatedDt = DateTime.Now;
                db.Brands.Add(brand);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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
                    brand.BrandNameEn = request.BrandNameEn;
                    brand.BrandNameTh = request.BrandNameTh;
                    brand.DescriptionEn = request.DescriptionEn;
                    brand.DescriptionTh = request.DescriptionTh;
                    if(request.SEO != null)
                    {
                        brand.MetaDescription = request.SEO.MetaDescription;
                        brand.MetaKey = request.SEO.MetaKeywords;
                        brand.MetaTitle = request.SEO.MetaTitle;
                        brand.UrlEn = request.SEO.ProductUrlKeyEn;
                        brand.UrlTh = request.SEO.ProductUrlKeyTh;
                    }
                    if (request.BrandImage != null)
                    {
                        brand.Path = request.BrandImage.tmpPath;
                        brand.PicUrl = request.BrandImage.url;
                    }
                    else
                    {
                        brand.Path = null;
                        brand.PicUrl = null;
                    }
                    brand.Status = Constant.STATUS_ACTIVE;
                    brand.UpdatedBy = this.User.Email();
                    brand.UpdatedDt = DateTime.Now;
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,HttpErrorMessage.NotFound);
                }
                
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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