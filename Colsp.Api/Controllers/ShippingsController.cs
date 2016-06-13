using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Api.Filters;

namespace Colsp.Api.Controllers
{
    public class ShippingsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Shippings")]
        [HttpGet]
		[ClaimsAuthorize(Permission = new string[] { "2", "3", "35", "34" })]
		public HttpResponseMessage GetShippings()
        {
            try
            {
                var tmpShipping = db.Shippings.Where(w => w.Status.Equals(Constant.STATUS_ACTIVE));
                if(User.ShopRequest() != null)
                {
                    var shopTypeId = User.ShopRequest().ShopType.ShopTypeId;
                    tmpShipping = tmpShipping.Where(w => w.ShopTypeShippingMaps.Any(a => a.ShopTypeId.Equals(shopTypeId)));
                }
                var shippingList = tmpShipping.Select(s => new
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