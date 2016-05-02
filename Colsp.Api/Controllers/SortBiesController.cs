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
    public class SortBiesController : ApiController
    {
        private ColspEntities db = new ColspEntities();


        [Route("api/SortBy")]
        [HttpGet]
        public HttpResponseMessage GetSortBy()
        {
            try
            {
                var sortList = db.SortBies.Select(s => new
                {
                    s.SortById,
                    s.SortByName,
                    s.NameEn,
                    s.NameTh
                });
                return Request.CreateResponse(HttpStatusCode.OK, sortList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        /*
        [Route("api/Brand/SortBy")]
        [HttpGet]
        public HttpResponseMessage GetSortByBrand()
        {
            try
            {
                var sortList = db.SortBies.Where(w=>w.SortByType.Equals("B")).Select(s => new
                {
                    s.SortById,
                    s.SortByName,
                    s.NameEn,
                    s.NameTh
                });
                return Request.CreateResponse(HttpStatusCode.OK, sortList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/GlobalCategory/SortBy")]
        [HttpGet]
        public HttpResponseMessage GetSortByGlobalCategory()
        {
            try
            {
                var sortList = db.SortBies.Where(w => w.SortByType.Equals("G")).Select(s => new
                {
                    s.SortById,
                    s.SortByName,
                    s.NameEn,
                    s.NameTh
                });
                return Request.CreateResponse(HttpStatusCode.OK, sortList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/LocalCategory/SortBy")]
        [HttpGet]
        public HttpResponseMessage GetSortByLocalCategory()
        {
            try
            {
                var sortList = db.SortBies.Where(w => w.SortByType.Equals("L")).Select(s => new
                {
                    s.SortById,
                    s.SortByName,
                    s.NameEn,
                    s.NameTh
                });
                return Request.CreateResponse(HttpStatusCode.OK, sortList);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }
        */



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SortByExists(int id)
        {
            return db.SortBies.Count(e => e.SortById == id) > 0;
        }
    }
}