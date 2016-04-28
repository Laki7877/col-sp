using System.Linq;
using System.Web.Http;
using Colsp.Entity.Models;
using System.Net.Http;
using System;
using System.Net;

namespace Colsp.Api.Controllers
{
    public class ProductHistoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        //[Route("api/ProductHistories/{historyId}/Restore")]
        //[HttpGet]
        //public HttpResponseMessage RestoreProductHistory(long historyId)
        //{
        //    try
        //    {
        //        db.ProductHistoryGroups.Where(w => w.HistoryId == historyId)
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

        private bool ProductHistoryExists(long id)
        {
            return db.ProductHistories.Count(e => e.HistoryId == id) > 0;
        }
    }
}