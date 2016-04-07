using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;

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

        [Route("api/Themes/{ThemeId}")]
        [HttpGet]
        public HttpResponseMessage GetTheme([FromUri] int themeId)
        {
            try
            {
                var themes = db.Themes
                    .Where(w => w.ThemeId == themeId)
                    .Select(s => new
                    {
                        s.ThemeId,
                        s.ThemeName,
                        s.ThemeImage,
                        ThemeComponentMaps = s.ThemeComponentMaps.Select(sc => new
                        {
                            sc.ThemeComponent.ComponentName,
                            sc.Count,
                        })
                    }).SingleOrDefault();
                return Request.CreateResponse(HttpStatusCode.OK, themes);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
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