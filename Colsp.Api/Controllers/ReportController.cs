using Colsp.Api.Extensions;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Colsp.Api.Controllers
{
    public class ReportController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [HttpGet]
        [Route("api/Report")]
        public HttpResponseMessage GetReportData([FromUri] ReportRequest request)
        {
            try
            {
                // Get Report Data


                // Save Log
                SaveReportLog(request.ReportName);

                return Request.CreateResponse(HttpStatusCode.OK, true);

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            
        }

        [HttpGet]
        [Route("api/Report/ReportNames")]
        public HttpResponseMessage GetAllReportName()
        {
            try
            {
                var query = from report in db.Reports select report;
                var items = query.ToList();

                return Request.CreateResponse(HttpStatusCode.OK, items);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private void SaveReportLog(string reportName)
        {
            try
            {
                var shopId      = User.ShopRequest() == null ? 0 : User.ShopRequest().ShopId;
                var createBy    = User.UserRequest().Email;

                ReportLog log   = new ReportLog();
                log.ReportName  = reportName;
                log.ShopId      = shopId;
                log.CreateBy    = createBy;
                log.CreateOn    = DateTime.Now;

                db.ReportLogs.Add(log);
                db.SaveChanges();

            }
            catch (Exception e)
            {

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
    }
}
