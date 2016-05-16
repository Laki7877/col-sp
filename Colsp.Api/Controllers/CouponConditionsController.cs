using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;

namespace Colsp.Api.Controllers
{
    public class CouponConditionsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/CouponConditions/Criteria")]
        [HttpGet]
        public HttpResponseMessage GetCriteria()
        {
            try
            {
                var criteria = db.CouponConditions.Where(w => w.ConditionType.Equals("C"));
                return Request.CreateResponse(HttpStatusCode.OK, criteria);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/CouponConditions/Filter")]
        [HttpGet]
        public HttpResponseMessage GetFilter()
        {
            try
            {
                var criteria = db.CouponConditions.Where(w => w.ConditionType.Equals("F"));
                return Request.CreateResponse(HttpStatusCode.OK, criteria);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.GetBaseException().Message);
            }
        }

        [Route("api/CouponConditions/Action")]
        [HttpGet]
        public HttpResponseMessage GetAction()
        {
            try
            {
                var criteria = db.CouponConditions.Where(w => w.ConditionType.Equals("A"));
                return Request.CreateResponse(HttpStatusCode.OK, criteria);
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

        private bool CouponConditionExists(int id)
        {
            return db.CouponConditions.Count(e => e.ConditionId == id) > 0;
        }

    }
}