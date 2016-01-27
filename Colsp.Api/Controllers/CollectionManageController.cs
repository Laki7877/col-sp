using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Helper;
using System.Data.Entity;

namespace Colsp.Api.Controllers
{
    public class CollectionManageController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        //[Route("api/Collection")]
        //[HttpGet]
        //public HttpResponseMessage GetCollectionStages([FromUri] CollectionListRequest request)
        //{
        //    try
        //    {
        //        var collectionItem = (from c in db.TBCollectionTemps
        //                              select new
        //                              {
        //                                  c.CollectionId,
        //                                  c.CollectionName,
        //                                  c.CollectionType,
        //                                  c.URLKeyEN,
        //                                  c.URLKeyTH,
        //                                  c.ShopId,
        //                                  c.CollectionSortDefualt,
        //                                  c.CollectionStart,
        //                                  c.CollectionEnd,
        //                                  c.CollectionCount,
        //                                  c.IsActive,
        //                                  c.UpdateBy,
        //                                  c.UpdateDate,
        //                                  c.CreateBy,
        //                                  c.CreateDate
        //                              }).Take(100);
        //        if (request == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, collectionItem);
        //        }
        //        request.DefaultOnNull();

        //        //if (request.SellerId != null)
        //        //{
        //        //    collectionItem = collectionItem.Where(p => p.SellerId.Equals(request.SellerId));
        //        //}

        //        var total = collectionItem.Count();
        //        var pagedCollection = collectionItem.Paginate(request);
        //        var response = PaginatedResponse.CreateResponse(pagedCollection, request, total);
        //        return Request.CreateResponse(HttpStatusCode.OK, response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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
