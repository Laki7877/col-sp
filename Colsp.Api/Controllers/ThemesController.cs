using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using System.Threading.Tasks;
using System.IO;
using Colsp.Model.Requests;
using Colsp.Api.Helpers;

namespace Colsp.Api.Controllers
{
    public class ThemesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Themes")]
        [HttpGet]
        public HttpResponseMessage GetThemes()
        {
            try
            {
                var themes = db.Themes
                    .Where(w => Constant.STATUS_ACTIVE.Equals(w.Status))
                    .Select(s=>new
                    {
                        s.ThemeId,
                        s.ThemeName,
                        s.ThemeImage
                    });
                return Request.CreateResponse(HttpStatusCode.OK,themes);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/Theme/Images")]
        [HttpPost]
        [OverrideAuthentication, OverrideAuthorization]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {

                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new Exception("In valid content multi-media");
                }
                var streamProvider = new MultipartFormDataStreamProvider(Path.Combine(AppSettingKey.IMAGE_ROOT_PATH, "Theme"));
                try
                {
                    await Request.Content.ReadAsMultipartAsync(streamProvider);
                }
                catch (Exception)
                {
                    throw new Exception("Image size exceeded " + 5 + " mb");
                }
                #region Validate Image
                string type = streamProvider.FormData["Type"];
                ImageRequest fileUpload = null;
                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    fileUpload = Util.SetupImage(Request,
                        fileData,
                        AppSettingKey.IMAGE_ROOT_FOLDER,
                        "Theme", 0, 0, int.MaxValue, int.MaxValue, int.MaxValue, false);
                    break;
                }
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, fileUpload);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }



        //[Route("api/Themes/{ThemeId}")]
        //[HttpGet]
        //public HttpResponseMessage GetTheme([FromUri] int themeId)
        //{
        //    try
        //    {
        //        var themes = db.Themes
        //            .Where(w => w.ThemeId == themeId)
        //            .Select(s => new
        //            {
        //                s.ThemeId,
        //                s.ThemeName,
        //                s.ThemeImage,
        //                //ThemeComponentMaps = s.ThemeComponentMaps.Select(sc => new
        //                //{
        //                //    sc.ThemeComponent.ComponentName,
        //                //    sc.Count,
        //                //    sc.Width,
        //                //    sc.Height,
        //                //})
        //            }).SingleOrDefault();
        //        return Request.CreateResponse(HttpStatusCode.OK, themes);
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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

        private bool ThemeExists(int id)
        {
            return db.Themes.Count(e => e.ThemeId == id) > 0;
        }
    }
}