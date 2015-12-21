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
using Colsp.Models;

namespace Colsp.Controllers
{
    public class GlobalCategoryController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        // GET: api/GlobalCategory
        public IQueryable<Global_Category> GetGlobal_Category()
        {
            return db.Global_Category;
        }

        // GET: api/GlobalCategory/5
        [ResponseType(typeof(Global_Category))]
        public IHttpActionResult GetGlobal_Category(int id)
        {
            Global_Category global_Category = db.Global_Category.Find(id);
            if (global_Category == null)
            {
                return NotFound();
            }

            return Ok(global_Category);
        }

        // PUT: api/GlobalCategory/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutGlobal_Category(int id, Global_Category global_Category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != global_Category.category_id)
            {
                return BadRequest();
            }

            db.Entry(global_Category).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Global_CategoryExists(id))
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

        // POST: api/GlobalCategory
        [ResponseType(typeof(Global_Category))]
        public IHttpActionResult PostGlobal_Category(Global_Category global_Category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Global_Category.Add(global_Category);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = global_Category.category_id }, global_Category);
        }

        // DELETE: api/GlobalCategory/5
        [ResponseType(typeof(Global_Category))]
        public IHttpActionResult DeleteGlobal_Category(int id)
        {
            Global_Category global_Category = db.Global_Category.Find(id);
            if (global_Category == null)
            {
                return NotFound();
            }

            db.Global_Category.Remove(global_Category);
            db.SaveChanges();

            return Ok(global_Category);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool Global_CategoryExists(int id)
        {
            return db.Global_Category.Count(e => e.category_id == id) > 0;
        }
    }
}