using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using System.Threading.Tasks;
using Colsp.Api.Constants;
using System.IO;
using System.Configuration;
using System.Web;
using System.Drawing;
using Colsp.Api.Helpers;
using System.Net.Http.Headers;
using Colsp.Api.Extensions;

namespace Colsp.Api.Controllers
{
    public class ProductImagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        private readonly string root = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[AppSettingKey.IMAGE_ROOT_PATH]);

        [Route("api/ProductImages/Upload")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.FORMAT_ERROR);
                }

                string tmpFolder = Path.Combine(root, ConfigurationManager.AppSettings[AppSettingKey.IMAGE_TMP_FOLDER]);
                var streamProvider = new MultipartFormDataStreamProvider(tmpFolder);
                await Request.Content.ReadAsMultipartAsync(streamProvider);

                string ShopID = streamProvider.FormData[HttpParameterConstant.SHOP_ID];
                if (String.IsNullOrEmpty(ShopID))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.MISSING_PARAMETER_ERROR);
                }
                string PID = streamProvider.FormData[HttpParameterConstant.PID];
                if (String.IsNullOrEmpty(PID))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.MISSING_PARAMETER_ERROR);
                }

                int position = 0;
                try
                {
                    position = Int32.Parse(streamProvider.FormData[HttpParameterConstant.POSITION]);
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.MISSING_PARAMETER_ERROR);
                }
                string ShopPath = Path.Combine(root, ShopID);
                ProductImage productImg = null;

                if (streamProvider.FileData.Count > 1)
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.FORMAT_ERROR);
                }

                //move from tmp to shop folder
                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, HttpErrorMessage.FORMAT_ERROR);
                    }

                    if (!Directory.Exists(ShopPath))
                    {
                        Directory.CreateDirectory(ShopPath);
                    }
                    string oldfileName = fileData.Headers.ContentDisposition.FileName;
                    if (oldfileName.StartsWith("\"") && oldfileName.EndsWith("\""))
                    {
                        oldfileName = oldfileName.Trim('"');
                    }
                    string[] extSplit = oldfileName.Split('.');
                    string ext = string.Empty;
                    if (extSplit.Length > 0)
                    {
                        ext = extSplit[extSplit.Length - 1];
                    }
                    Size dimension = FileHelper.GetImageDimention(fileData.LocalFileName);
                    string newfileName = Path.Combine(ShopPath, PID + "_" + dimension.Width + "x" + dimension.Height + "." + ext);
                    newfileName = FileHelper.GetNextImageFileName(newfileName).FullName;

                    productImg = new ProductImage();
                    productImg.Pid = PID;
                    productImg.Path = newfileName;
                    productImg.ImageOriginName = oldfileName;
                    productImg.ImageName = Path.GetFileName(newfileName);
                    productImg.Position = position;
                    productImg.CreatedBy = this.User.Email();
                    productImg.CreatedDt = DateTime.Now;

                    db.ProductImages.Add(productImg);
                    db.SaveChanges();

                    File.Move(fileData.LocalFileName, newfileName);
                }

                return Request.CreateResponse(productImg);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.INTERNAL_SERVER_ERROR);
            }
        }

        [Route("api/ProductImages/GetImage/{pid}/{position}")]
        [HttpGet]
        public HttpResponseMessage GetImage(string pid, int position)
        {
            try
            {
                var productImg = (from pi in db.ProductImages
                                    where pi.Pid == pid && pi.Position == position
                                   select pi).ToList();
                if(productImg != null && productImg.Count > 0)
                {
                    ProductImage pImg = productImg[0];
                    Image img = System.Drawing.Image.FromFile(pImg.Path);
                    MemoryStream ms = new MemoryStream();                   
                    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                    string[] fileSplit = pImg.ImageName.Split('.');
                    string ext = string.Empty;
                    if(fileSplit.Length > 1)
                    {
                        ext = fileSplit[1];
                    }
                    switch (ext.ToLower())
                    {
                        case "png":
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                            break;
                        case "jpg":
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                            break;
                        case "jpeg":
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                            break;
                        case "gif":
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/gif");
                            break;
                        default:
                            break;
                    }
                    img.Dispose();
                    result.Content = new ByteArrayContent(ms.ToArray());
                    return result;
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
                }
                
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.INTERNAL_SERVER_ERROR);
            }
            
        }

        [Route("api/ProductImages/DeleteImage/{pid}/{position}")]
        [HttpDelete]
        public HttpResponseMessage DeleteImage(string pid, int position)
        {
            try
            {
                var productImg = (from pi in db.ProductImages
                                  where pi.Pid == pid && pi.Position == position
                                  select pi).ToList();
                if (productImg != null && productImg.Count > 0)
                {
                    db.ProductImages.Remove(productImg[0]);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.INTERNAL_SERVER_ERROR);
            }
        }

        [Route("api/ProductImages/DeleteImage/{id}")]
        [HttpDelete]
        public HttpResponseMessage DeleteImageById(int id)
        {
            try
            {
                var ProductImg = db.ProductImages.Find(id);
                if(ProductImg != null)
                {
                    db.ProductImages.Remove(ProductImg);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.INTERNAL_SERVER_ERROR);
            }
        }

        [Route("api/ProductImages/ShiftPosition/{pid}/{fromPos}/{toPos}")]
        [HttpGet]
        public HttpResponseMessage ShiftPosition(string pid, int fromPos, int toPos)
        {
            try
            {
              

                var ProductImgs = (from img in db.ProductImages
                                  where img.Pid == pid && img.Position == fromPos
                                   select img).ToList();
                if(ProductImgs != null && ProductImgs.Count > 0)
                {
                    ProductImgs[0].Position = toPos;
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
                }

                ProductImgs = (from img in db.ProductImages
                                where img.Pid == pid && img.Position == toPos
                                select img).ToList();
                if(ProductImgs != null && ProductImgs.Count > 0)
                {
                    ProductImgs[0].Position = fromPos;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.INTERNAL_SERVER_ERROR);
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
    }
}