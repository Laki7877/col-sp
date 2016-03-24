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

namespace Colsp.Api.Controllers
{
    public class ProductHistoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        //[Route("api/ProductHistories/{historyId}")]
        //[HttpGet]
        //public HttpResponseMessage GetProductHistory(long historyId)
        //{
        //    try
        //    {
        //        db.ProductHistoryGroups.Where(w=>w.HistoryId == historyId)
        //            .Include(i => i.ProductHistories)
        //            .Include(i => i.ProductHistories.Select(s => s.ProductHistoryAttributes.Select(sa => sa.Attribute.AttributeValueMaps.Select(sv => sv.AttributeValue))))
        //            .Include(i => i.ProductStages.Select(s => s.Inventory))
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageImages))
        //            .Include(i => i.ProductStages.Select(s => s.ProductStageVideos))
        //            .Include(i => i.ProductStageGlobalCatMaps.Select(s => s.GlobalCategory))
        //            .Include(i => i.ProductStageLocalCatMaps.Select(s => s.LocalCategory));
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