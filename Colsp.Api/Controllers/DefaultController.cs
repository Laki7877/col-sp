using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Colsp.Api.Controllers
{
    public class DefaultController : ApiController
    {
        [Route("api/Default")]
        [HttpGet]
        public HttpResponseMessage GetTest()
        {
            return Request.CreateErrorResponse(HttpStatusCode.NotFound,"false");
        }
    }
}
