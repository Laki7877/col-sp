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
using Colsp.Api.Constants;

namespace Colsp.Api.Controllers
{
    public class LocalCategoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/LocalCategories/GetLocalCategory/{shopId}")]
        [HttpGet]
        public HttpResponseMessage GetLocalCategories(int shopId)
        {
            try
            {
                var localCat = (from node in db.LocalCategories
                                 from parent in db.GlobalCategories
                                 where node.Lft >= parent.Lft && node.Lft <= parent.Rgt
                                 group node by new { node.NameEn, node.Lft} into g
                                 orderby g.Key.Lft
                                 select new
                                 {
                                     NameEn = g.Key.NameEn,
                                     Depth = g.ToList().Count()
                                 }).ToList();
                if(localCat != null && localCat.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,localCat);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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

        private bool LocalCategoryExists(int id)
        {
            return db.LocalCategories.Count(e => e.CategoryId == id) > 0;
        }
    }
}