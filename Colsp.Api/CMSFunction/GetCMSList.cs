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

namespace Colsp.Api.CMSFunction
{
    public class GetCMSList
    {
        //Abstract 
        public void GetByShopId(CMSShopRequest Request)
        {
        }
        public void GetByGobal(int UserId)
        {
        }

    }
}