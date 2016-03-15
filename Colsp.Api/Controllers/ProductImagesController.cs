using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using System.Threading.Tasks;
using Colsp.Api.Constants;
using System.IO;
using System.Drawing;
using Colsp.Model.Responses;
using System.Drawing.Imaging;

namespace Colsp.Api.Controllers
{
    public class ProductImagesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        //[Route("api/ProductImages")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> UploadFile()
        //{
        //    try
        //    {
        //        if (!Request.Content.IsMimeMultipartContent())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.NotAcceptable, "Content Multimedia");
        //        }
        //        string tmpFolder = Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, AppSettingKey.TMP_FOLDER);
        //        var streamProvider = new MultipartFormDataStreamProvider(tmpFolder);
        //        await Request.Content.ReadAsMultipartAsync(streamProvider);

        //        if(streamProvider.FileData == null || streamProvider.FileData.Count == 0)
        //        {
        //            throw new Exception("Wrong file format. Please upload only JPG or PNG file. Image size is too small.Please upload 1500x1500 px to 2000x2000 px");
        //        }

        //        using (Image img = Image.FromFile(streamProvider.FileData[0].LocalFileName))
        //        {
        //            if (!ImageFormat.Jpeg.Equals(img.RawFormat)
        //                && !ImageFormat.Png.Equals(img.RawFormat))
        //            {
        //                throw new Exception("Wrong file format. Please upload only JPG or PNG file.Image size is too small. Please upload 1500x1500 px to 2000x2000 px");
        //            }
        //            if(img.Width < 1500 || img.Height < 1500)
        //            {
        //                throw new Exception("Image size is too small.The size should be between 1500x1500 px to 2000x2000 px. Square image only and not over 5 mbs per image");
        //            }
        //            if(img.Width > 2000 || img.Height > 2000)
        //            {
        //                throw new Exception("Image size is too big.The size should be between 1500x1500 px to 2000x2000 px. Square image only and not over 5 mbs per image");
        //            }
        //            if(img.Height != img.Width)
        //            {
        //                throw new Exception("The size should be between 1500x1500 px to 2000x2000 px. Square image only and not over 5 mbs per image");
        //            }
        //        }
             
        //        FileUploadRespond fileUpload = new FileUploadRespond();
        //        string fileName = string.Empty;
        //        string ext = string.Empty;
        //        foreach (MultipartFileData fileData in streamProvider.FileData)
        //        {
        //            fileName = fileData.LocalFileName;
        //            string tmp = fileData.Headers.ContentDisposition.FileName;
        //            if (tmp.StartsWith("\"") && tmp.EndsWith("\""))
        //            {
        //                tmp = tmp.Trim('"');
        //            }
        //            ext = Path.GetExtension(tmp);
        //            break;
        //        }
        //        //UriHelper uu = Request.GetRequestContext().Url;
                
        //        string newName = string.Concat(fileName,ext);
        //        File.Move(fileName, newName);
        //        fileUpload.tmpPath = newName;

        //        var name = Path.GetFileName(newName);
        //        var schema = Request.GetRequestContext().Url.Request.RequestUri.Scheme;
        //        var imageUrl = Request.GetRequestContext().Url.Request.RequestUri.Authority;
        //        fileUpload.url = string.Concat(schema,"://",imageUrl,"/",AppSettingKey.IMAGE_ROOT_FOLDER,"/",AppSettingKey.TMP_FOLDER,"/", name);

        //        return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
        //    }
        //}
        

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