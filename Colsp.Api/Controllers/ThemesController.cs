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
using Colsp.Api.Extensions;

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
                var tmpThemes = db.Themes.Where(w => Constant.STATUS_ACTIVE.Equals(w.Status));
                #region Validation
                var shop = User.ShopRequest();
                if (shop != null)
                {
                    if(shop.ShopType == null)
                    {
                        throw new Exception("Invalid shop type");
                    }
                    var shopTypeId = shop.ShopType.ShopTypeId;
                    tmpThemes = tmpThemes.Where(w => w.ShopTypeThemeMaps.Any(a => a.ShopTypeId == shopTypeId));
                }
                #endregion
                var themes = tmpThemes.Select(s=>new
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