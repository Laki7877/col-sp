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
using System.Net.Http;
using Colsp.Api.Constants;
using System.Collections.Generic;
using Colsp.Api.Helpers;
using Colsp.Api.Services;
using System.Security.Principal;
using System.Security.Claims;

namespace Colsp.Api.Controllers
{
	public class UsersController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/Users/Seller")]
        [HttpGet]
        public HttpResponseMessage GetUserSeller([FromUri] UserRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId.Value;
                var userList = db.Users
                    .Include(i => i.UserGroupMaps.Select(s => s.UserGroup))
                    .Include(i=>i.UserShops)
                    .Where(w => w.Type.Equals(Constant.USER_TYPE_SELLER) 
                        && !w.Status.Equals(Constant.STATUS_REMOVE) 
                        && w.UserShops.Any(a=>a.ShopId== shopId))
                    .Select(s => new
                    {
                        s.UserId,
                        s.NameEn,
                        s.NameTh,
                        s.Email,
                        s.UpdatedDt,
                        UserGroup = s.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn)
                    });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, userList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    userList = userList.Where(a => a.NameEn.Contains(request.SearchText)
                    || a.NameTh.Contains(request.SearchText)
                    || a.Email.Contains(request.SearchText));
                }
                var total = userList.Count();
                var pagedUsers = userList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Admin")]
        [HttpGet]
        public HttpResponseMessage GetUserAdmin([FromUri] UserRequest request)
        {
            try
            {
                var userList = db.Users.Include(i=>i.UserGroupMaps.Select(s=>s.UserGroup))
                    .Where(w => w.Type.Equals(Constant.USER_TYPE_ADMIN) && !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new
                    {
                        s.UserId,
                        s.NameEn,
                        s.NameTh,
                        s.Email,
                        s.UpdatedDt,
                        UserGroup = s.UserGroupMaps.Select(ug=>ug.UserGroup.GroupNameEn)
                    });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, userList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    userList = userList.Where(a => a.NameEn.Contains(request.SearchText)
                    || a.NameTh.Contains(request.SearchText)
                    || a.Email.Contains(request.SearchText));
                }
                var total = userList.Count();
                var pagedUsers = userList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Admin/{userId}")]
        [HttpGet]
        public HttpResponseMessage GetUserAdmin(int userId)
        {
            try
            {
                var usr = db.Users.Include(i=>i.UserGroupMaps.Select(s=>s.UserGroup))
                    .Where(w => w.UserId == userId && !w.Status.Equals(Constant.STATUS_REMOVE) && w.Type.Equals(Constant.USER_TYPE_ADMIN))
                    .Select(s=>new {
                        s.UserId,
                        s.Email,
                        s.NameEn,
                        s.NameTh,
                        s.Phone,
                        s.Position,
                        s.Division,
                        s.EmployeeId,
                        UserGroup = s.UserGroupMaps.Select(ug => new { ug.UserGroup.GroupId, ug.UserGroup.GroupNameEn, ug.UserGroup.GroupNameTh, Permission = ug.UserGroup.UserGroupPermissionMaps.Select(p => new { p.Permission.PermissionId, p.Permission.PermissionName}) })
                    }).ToList();
                if (usr == null || usr.Count == 0)
                {
                    throw new Exception("User not found");
                }
                
                return Request.CreateResponse(HttpStatusCode.OK, usr[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Admin")]
        [HttpPost]
        public HttpResponseMessage AddUserAdmin(UserRequest request)
        {
            User usr = null;
            try
            {
                usr = new User();
                usr.Email = Validation.ValidateString(request.Email,"Email",true,100,false);
                usr.Password = request.Password;
                usr.NameEn = request.NameEn;
                usr.NameTh = request.NameTh;
                usr.Division = request.Division;
                usr.Phone = request.Phone;
                usr.Position = request.Position;
                usr.EmployeeId = request.EmployeeId;
                usr.Status = Constant.STATUS_ACTIVE;
                usr.Type = Constant.USER_TYPE_ADMIN;
                usr.CreatedBy = this.User.UserRequest().Email;
                usr.CreatedDt = DateTime.Now;
                usr.UpdatedBy = this.User.UserRequest().Email;
                usr.UpdatedDt = DateTime.Now;
                db.Users.Add(usr);
                db.SaveChanges();
                if(request.UserGroup != null)
                {
                    foreach (UserGroupRequest usrGrp in request.UserGroup)
                    {
                        if (usrGrp.GroupId == null)
                        {
                            throw new Exception("User group id is null");
                        }
                        UserGroupMap map = new UserGroupMap();
                        map.UserId = usr.UserId;
                        map.GroupId = usrGrp.GroupId.Value;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.UserRequest().Email;
                        map.UpdatedDt = DateTime.Now;
                        db.UserGroupMaps.Add(map);
                    }
                    db.SaveChanges();
                }
                return GetUserAdmin(usr.UserId);
            }
            catch (Exception e)
            {
                if(usr != null && usr.UserId != 0)
                {
                    db.Users.Remove(usr);
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Admin/{userId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeUserAdmin([FromUri]int userId, UserRequest request)
        {
            try
            {
                var usr = db.Users.Find(userId);
                if (usr == null || usr.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("User not found");
                }
                if (!usr.Type.Equals(Constant.USER_TYPE_ADMIN))
                {
                    throw new Exception("This user is not admin");
                }
                usr.Email = Validation.ValidateString(request.Email, "Email", true, 100, false);
                if (!string.IsNullOrEmpty(request.Password))
                {
                    usr.Password = request.Password;
                }
                usr.NameEn = request.NameEn;
                usr.NameTh = request.NameTh;
                usr.Division = request.Division;
                usr.Phone = request.Phone;
                usr.Position = request.Position;
                usr.EmployeeId = request.EmployeeId;
                usr.UpdatedBy = this.User.UserRequest().Email;
                usr.UpdatedDt = DateTime.Now;
                var usrGrpList = db.UserGroupMaps.Where(w => w.UserId == usr.UserId).ToList();
                if (request.UserGroup != null && request.UserGroup.Count > 0)
                {
                    bool addNew = false;
                    foreach (UserGroupRequest grp in request.UserGroup)
                    {
                        if (usrGrpList == null || usrGrpList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            UserGroupMap current = usrGrpList.Where(w => w.GroupId == grp.GroupId).SingleOrDefault();
                            if (current != null)
                            {
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
                                usrGrpList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            UserGroupMap map = new UserGroupMap();
                            map.UserId = usr.UserId;
                            map.GroupId = grp.GroupId.Value;
                            map.CreatedBy = this.User.UserRequest().Email;
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.UserRequest().Email;
                            map.UpdatedDt = DateTime.Now;
                            db.UserGroupMaps.Add(map);
                        }
                    }
                }
                if (usrGrpList != null && usrGrpList.Count > 0)
                {
                    db.UserGroupMaps.RemoveRange(usrGrpList);
                }
                db.SaveChanges();
                return GetUserAdmin(usr.UserId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Admin")]
        [HttpDelete]
        public HttpResponseMessage DeleteUserAdmin(List<UserRequest> request)
        {
            try
            {
                
                foreach (UserRequest userRq in request)
                {
                    if(userRq.UserId == null)
                    {
                        throw new Exception("User id cannot be null");
                    }
                    var usr = db.Users.Find(userRq.UserId.Value);
                    if (usr == null)
                    {
                        throw new Exception("Cannot find user " + userRq.UserId.Value);
                    }
                    if (!usr.Type.Equals(Constant.USER_TYPE_ADMIN))
                    {
                        throw new Exception("Cannot deleted non admin user " + userRq.UserId.Value);
                    }
                    usr.Status = Constant.STATUS_REMOVE;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Login")]
        [HttpGet]
        public HttpResponseMessage LoginUser()
        {
            try
            {
                ClaimRequest claim = new ClaimRequest();
                
                var claimsIdentity = this.User.Identity as ClaimsIdentity;
                claim.Permission = claimsIdentity.Claims
                    .Where(w => w.Type.Equals("Permission")).Select(s => new { Permission = s.Value, PermissionGroup = s.ValueType }).ToList();
                claim.Shop = this.User.Shops().Select(s=>new { s.ShopId, s.ShopNameEn }).ToList()[0];
                claim.User = new { Email = this.User.UserRequest().Email, IsAdmin = Constant.USER_TYPE_ADMIN.Equals(this.User.UserRequest().Type) };
                return Request.CreateResponse(HttpStatusCode.OK, claim);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
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

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }

        //      // GET: api/Users
        //[ClaimsAuthorize(Permission = "ListUser")]
        //[ResponseType(typeof(PaginatedResponse))]
        //      public IHttpActionResult GetUsers([FromUri] UserRequest request)
        //      {
        //	request.DefaultOnNull();
        //	var users = db.Users
        //			.Where(u => u.Username.Contains(request.Name));
        //	var total = users.Count();
        //	var	pagedUsers = users.Paginate(request)/*
        //							.Select(u => new {
        //								u.UserId,
        //								u.Username,
        //								u.Email,
        //								u.NameEn,
        //								u.NameTh,
        //								u.Mobile,
        //								u.Phone,
        //								u.Fax,
        //								u.Status,
        //								u.LastLoginDt,
        //								u.CreatedBy,
        //								u.CreatedDt,
        //								u.UpdatedBy,
        //								u.UpdatedDt
        //							})*/;
        //	var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);

        //	return Ok(response);
        //      }

        //      // GET: api/Users/5
        //[ClaimsAuthorize(Permission = "GetUser")]
        //      [ResponseType(typeof(User))]
        //      public IHttpActionResult GetUser(int id)
        //      {
        //          User user = db.Users.Find(id);
        //          if (user == null)
        //          {
        //              return NotFound();
        //          }

        //          return Ok(user);
        //      }

        //      // PUT: api/Users/5
        //[ClaimsAuthorize(Permission = "UpdateUser")]
        //      [ResponseType(typeof(void))]
        //      public IHttpActionResult PutUser(int id, User user)
        //      {
        //          if (!ModelState.IsValid)
        //          {
        //              return BadRequest(ModelState);
        //          }

        //          if (id != user.UserId)
        //          {
        //              return BadRequest();
        //          }

        //          db.Entry(user).State = EntityState.Modified;

        //          try
        //          {
        //              db.SaveChanges();
        //          }
        //          catch (DbUpdateConcurrencyException)
        //          {
        //              if (!UserExists(id))
        //              {
        //                  return NotFound();
        //              }
        //              else
        //              {
        //                  throw;
        //              }
        //          }

        //          return StatusCode(HttpStatusCode.NoContent);
        //      }

        //      // POST: api/Users
        //[ClaimsAuthorize(Permission = "AddUser")]
        //      [ResponseType(typeof(User))]
        //      public IHttpActionResult PostUser(User user)
        //      {
        //          if (!ModelState.IsValid)
        //          {
        //              return BadRequest(ModelState);
        //          }

        //          db.Users.Add(user);
        //          db.SaveChanges();

        //          return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        //      }

        //      // DELETE: api/Users/5
        //[ClaimsAuthorize(Permission = "DeleteUser")]
        //      [ResponseType(typeof(User))]
        //      public IHttpActionResult DeleteUser(int id)
        //      {
        //          User user = db.Users.Find(id);
        //          if (user == null)
        //          {
        //              return NotFound();
        //          }

        //          db.Users.Remove(user);
        //          db.SaveChanges();

        //          return Ok(user);
        //      }
    }
}