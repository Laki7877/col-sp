﻿using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;

namespace Colsp.Api.Controllers
{
    public class ShippingsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Shippings")]
        [HttpGet]
        public HttpResponseMessage GetShippings()
        {
            try
            {
                var shippingList = db.Shippings.Select(s => new
                {
                    s.ShippingId,
                    s.ShippingMethodEn,
                });
                return Request.CreateResponse(HttpStatusCode.OK, shippingList);
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

        private bool ShippingExists(int id)
        {
            return db.Shippings.Count(e => e.ShippingId == id) > 0;
        }
    }
}