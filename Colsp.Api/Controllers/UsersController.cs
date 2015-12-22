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
using Colsp.Api.Entities;
using Colsp.Api.Commons;
using Colsp.Api.Models;
using System.Security.Claims;

namespace Colsp.Controllers
{
	public class UsersController : ApiController
	{
		private ColspEntities db = new ColspEntities();

		// GET: api/Users
		[ClaimsAuthorize(Permission = "ListUser")]
        public IQueryable<User> GetUsers([FromUri] UserRequest request)
        {
			return db.Users;
			//return QueryHelper.ChainPaginatedQuery<User>(db.Users, request._order, request._offset, request._limit, request._direction == "DESC" ? true : false);
        }
		// GET: api/Users/5
		[ClaimsAuthorize(Permission = "GetUser")]
		[ResponseType(typeof(User))]
        public IHttpActionResult GetUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

		// PUT: api/Users/5
		[ClaimsAuthorize(Permission = "UpdateUser")]
		[ResponseType(typeof(void))]
        public IHttpActionResult PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.UserId)
            {
                return BadRequest();
            }

            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

		// POST: api/Users
		[ClaimsAuthorize(Permission = "AddUser")]
		[ResponseType(typeof(User))]
        public IHttpActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

		// DELETE: api/Users/5
		[ClaimsAuthorize(Permission = "DeleteUser ")]
		[ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }
    }
}