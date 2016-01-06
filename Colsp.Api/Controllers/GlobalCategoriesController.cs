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
    public class GlobalCategoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        [Route("api/GlobalCategories/GetGlobalCategory")]
        [HttpGet]
        public HttpResponseMessage GetGlobalCategory()
        {
            try
            {
                var tmp = (from node in db.GlobalCategories
                           from parent in db.GlobalCategories
                           where node.Lft >= parent.Lft && node.Lft <= parent.Rgt
                           group node by new { node.NameEn, node.Lft, node.CategoryAbbreviation } into g
                           orderby g.Key.Lft
                           select new
                           {
                               NameEn = g.Key.NameEn,
                               CategoryAbbreviation = g.Key.CategoryAbbreviation,
                               Depth = g.ToList().Count()
                           });
                return Request.CreateResponse(tmp);
            }
            catch (Exception ex)
            {
                string tt = ex.Message;
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

        private bool GlobalCategoryExists(int id)
        {
            return db.GlobalCategories.Count(e => e.CategoryId == id) > 0;
        }
    }
}