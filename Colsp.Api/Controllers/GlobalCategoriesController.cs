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
using Colsp.Api.Constants;

namespace Colsp.Api.Controllers
{
    public class GlobalCategoriesController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/GlobalCategories")]
        [HttpGet]
        public HttpResponseMessage GetGlobalCategory()
        {
            try
            {
                var globalCat = (from node in db.GlobalCategories
                           from parent in db.GlobalCategories
                           where node.Lft >= parent.Lft && node.Lft <= parent.Rgt
                           group node by new { node.CategoryId, node.NameEn, node.Lft, node.CategoryAbbreviation } into g
                           orderby g.Key.Lft
                           select new
                           {
                               NameEn = g.Key.NameEn,
                               CategoryId = g.Key.CategoryId,
                               CategoryAbbreviation = g.Key.CategoryAbbreviation,
                               Depth = g.ToList().Count()
                           }).ToList();
                if (globalCat != null && globalCat.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, globalCat);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
                
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/GlobalCategories/{catId}/Attributes")]
        [HttpGet]
        public HttpResponseMessage GetVarientAttribute(int catId)
        {
            try
            {
                var attribute = (from cat in db.GlobalCategories
                                 join catMap in db.CategoryAttributeSetMaps on cat.CategoryId equals catMap.CategoryId
                                 join attrSet in db.AttributeSets on catMap.AttributeSetId equals attrSet.AttributeSetId
                                 where cat.CategoryId.Equals(catId)
                                         && cat.Status.Equals(Constant.STATUS_ACTIVE)
                                         && attrSet.Status.Equals(Constant.STATUS_ACTIVE)
                                 select new
                                 {
                                     attrSet,
                                     attrSetMap = from a in db.AttributeSetMaps
                                                  where a.Status.Equals(Constant.STATUS_ACTIVE)
                                                  select a,
                                     attr = from a in db.Attributes
                                            where a.Status.Equals(Constant.STATUS_ACTIVE) && a.VariantStatus.Equals(true)
                                            select a
                                 }).AsEnumerable().Select(t => t.attr).ToList();
                if (attribute != null && attribute.Count > 0)
                {
                    return Request.CreateResponse(attribute);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
            }
        }

        [Route("api/GlobalCategories/{catId}/AttributeSets")]
        [HttpGet]
        public HttpResponseMessage GetAttributeSetFromCat(int catId)
        {
            try
            {
                var attributeSet = (from cat in db.GlobalCategories
                                    join catMap in db.CategoryAttributeSetMaps on cat.CategoryId equals catMap.CategoryId
                                    join attrSet in db.AttributeSets on catMap.AttributeSetId equals attrSet.AttributeSetId
                                    where cat.CategoryId.Equals(catId)
                                            && cat.Status.Equals(Constant.STATUS_ACTIVE)
                                            && attrSet.Status.Equals(Constant.STATUS_ACTIVE)
                                    select new
                                    {
                                        attrSet,
                                        attrSetMap = from a in db.AttributeSetMaps
                                                     where a.Status.Equals(Constant.STATUS_ACTIVE)
                                                     select a,
                                        attr = from a in db.Attributes
                                               where a.Status.Equals(Constant.STATUS_ACTIVE)
                                               select a,
                                        attrValMap = from a in db.AttributeValueMaps
                                                     where a.Status.Equals(Constant.STATUS_ACTIVE)
                                                     select a,
                                        attrVal = from a in db.AttributeValues
                                                  where a.Status.Equals(Constant.STATUS_ACTIVE)
                                                  select a
                                    }).AsEnumerable().Select(t => t.attrSet).ToList();
                if (attributeSet != null && attributeSet.Count > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, attributeSet);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, HttpErrorMessage.NotFound);
                }

            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, HttpErrorMessage.InternalServerError);
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

        private bool GlobalCategoryExists(int id)
        {
            return db.GlobalCategories.Count(e => e.CategoryId == id) > 0;
        }
    }
}