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
using Colsp.Api.Helper;
using Colsp.Api.Filters;
using Colsp.Api.Constants;

namespace Colsp.Api.Controllers
{
    public class GlobalCategoryPIDsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/GlobalCategoryPIDs/GetFromAttributeSetCatCode/{catCode}")]
        [ClaimsAuthorize(Permission = "AddProduct")]
        [HttpGet]
        public HttpResponseMessage GetGlobalCategoryPID(string catCode)
        {
            GlobalCategoryPID globalCategoryPID = null;
            string PID = GlobalCatPIDCache.GetPID(catCode);
            if (!String.IsNullOrEmpty(PID))
            {
                globalCategoryPID = new GlobalCategoryPID();
                globalCategoryPID.CategoryAbbreviation = catCode;
                globalCategoryPID.CurrentKey = AutoGenerate.NextPID(PID);
                GlobalCatPIDCache.UpdateKey(globalCategoryPID.CategoryAbbreviation,globalCategoryPID.CurrentKey);
                return Request.CreateResponse(HttpStatusCode.OK, string.Concat(catCode,globalCategoryPID));
            }
            globalCategoryPID = db.GlobalCategoryPIDs.Find(catCode);
            if (globalCategoryPID == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NOT_FOUND);
            }
            globalCategoryPID.CurrentKey = AutoGenerate.NextPID(globalCategoryPID.CurrentKey);
            GlobalCatPIDCache.AddPID(globalCategoryPID.CategoryAbbreviation, globalCategoryPID.CurrentKey);
            return Request.CreateResponse(HttpStatusCode.OK, string.Concat(catCode, globalCategoryPID));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GlobalCategoryPIDExists(string id)
        {
            return db.GlobalCategoryPIDs.Count(e => e.CategoryAbbreviation == id) > 0;
        }
    }
}