using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using Colsp.Api.Filters;
using Colsp.Api.Extensions;
using System;

namespace Colsp.Api.Controllers
{
	public class UsersController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        // GET: api/Users
		[ClaimsAuthorize(Permission = "ListUser")]
		[ResponseType(typeof(PaginatedResponse))]
        public IHttpActionResult GetUsers([FromUri] UserRequest request)
        {
			request.DefaultOnNull();
			var users = db.Users
					.Where(u => u.Username.Contains(request.Name));
			var total = users.Count();
			var	pagedUsers = users.Paginate(request)
									.Select(u => new {
										u.UserId,
										u.Username,
										u.Email,
										u.NameEn,
										u.NameTh,
										u.Mobile,
										u.Phone,
										u.Fax,
										u.Status,
										u.LastLoginDt,
										u.CreatedBy,
										u.CreatedDt,
										u.UpdatedBy,
										u.UpdatedDt
									});
			var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);

			return Ok(response);
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
		[ClaimsAuthorize(Permission = "DeleteUser")]
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