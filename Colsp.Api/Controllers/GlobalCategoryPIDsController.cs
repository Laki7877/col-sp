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

namespace Colsp.Api.Controllers
{
    public class GlobalCategoryPIDsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        // GET: api/GlobalCategoryPIDs
        public IQueryable<GlobalCategoryPID> GetGlobalCategoryPIDs()
        {
            return db.GlobalCategoryPIDs;
        }

        // GET: api/GlobalCategoryPIDs/5
        [ClaimsAuthorize(Permission = "AddProduct")]
        [ResponseType(typeof(GlobalCategoryPID))]
        public IHttpActionResult GetGlobalCategoryPID(string id)
        {
            GlobalCategoryPID globalCategoryPID = db.GlobalCategoryPIDs.Find(id);
            if (globalCategoryPID == null)
            {
                return NotFound();
            }
            globalCategoryPID.CurrentKey = AutoGenerate.NextPID(globalCategoryPID.CurrentKey);
            return Ok(globalCategoryPID);
        }

        // PUT: api/GlobalCategoryPIDs/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutGlobalCategoryPID(string id, GlobalCategoryPID globalCategoryPID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != globalCategoryPID.CategoryAbbreviation)
            {
                return BadRequest();
            }

            db.Entry(globalCategoryPID).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GlobalCategoryPIDExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/GlobalCategoryPIDs
        [ResponseType(typeof(GlobalCategoryPID))]
        public IHttpActionResult PostGlobalCategoryPID(GlobalCategoryPID globalCategoryPID)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.GlobalCategoryPIDs.Add(globalCategoryPID);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (GlobalCategoryPIDExists(globalCategoryPID.CategoryAbbreviation))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = globalCategoryPID.CategoryAbbreviation }, globalCategoryPID);
        }

        // DELETE: api/GlobalCategoryPIDs/5
        [ResponseType(typeof(GlobalCategoryPID))]
        public IHttpActionResult DeleteGlobalCategoryPID(string id)
        {
            GlobalCategoryPID globalCategoryPID = db.GlobalCategoryPIDs.Find(id);
            if (globalCategoryPID == null)
            {
                return NotFound();
            }

            db.GlobalCategoryPIDs.Remove(globalCategoryPID);
            db.SaveChanges();

            return Ok(globalCategoryPID);
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