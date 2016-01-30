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
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;

namespace Colsp.Api.Controllers
{
    public class UserGroupsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/UserGroups/Admin")]
        [HttpGet]
        public HttpResponseMessage GetUserGroupAdmin([FromUri] UserGroupRequest request)
        {
            try
            {
                var userGroupList = db.UserGroups.Include(i=>i.UserGroupPermissionMaps.Select(s=>s.UserPermission)).Where(w=>w.Type.Equals(Constant.USER_TYPE_ADMIN) && !w.Status.Equals(Constant.STATUS_REMOVE)).Select(s=>new {
                    s.GroupId,
                    s.GroupNameEn,
                    s.GroupNameTh,
                    Permission = s.UserGroupPermissionMaps.Select(p=>new { p.UserPermission.PermissionId, p.UserPermission.PermissionName })
                });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, userGroupList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.GroupNameEn))
                {
                    userGroupList = userGroupList.Where(a => a.GroupNameEn.Contains(request.GroupNameEn)
                    || a.GroupNameTh.Contains(request.GroupNameTh));
                }
                var total = userGroupList.Count();
                var pagedUsers = userGroupList.Paginate(request);
                var response = PaginatedResponse.CreateResponse(pagedUsers, request, total);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/UserGroups/Admin/{usergroupid}")]
        [HttpGet]
        public HttpResponseMessage GetUserGroupAdmin(int usergroupid)
        {
            try
            {
                var usr = db.UserGroups.Include(i => i.UserGroupPermissionMaps.Select(s => s.UserPermission))
                    .Where(w => w.GroupId == usergroupid && !w.Status.Equals(Constant.STATUS_REMOVE))
                    .Select(s => new {
                        s.GroupId,
                        s.GroupNameEn,
                        s.GroupNameTh,
                        Permission = s.UserGroupPermissionMaps.Select(ug => ug.UserPermission)
                    }).ToList();
                if (usr == null || usr.Count == 0)
                {
                    throw new Exception("User group not found");
                }

                return Request.CreateResponse(HttpStatusCode.OK, usr[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/UserGroups/Admin")]
        [HttpPost]
        public HttpResponseMessage AddUserAdmin(UserGroupRequest request)
        {
            UserGroup usrGrp = null;
            try
            {
                usrGrp = new UserGroup();
                usrGrp.GroupNameEn = request.GroupNameEn;
                usrGrp.GroupNameTh = request.GroupNameTh;
                db.UserGroups.Add(usrGrp);
                db.SaveChanges();
                if (request.Permission != null)
                {
                    foreach (UserPermissionRequest perm in request.Permission)
                    {
                        if (perm.PermissionId == null)
                        {
                            throw new Exception("Permission id is null");
                        }
                        UserGroupPermissionMap map = new UserGroupPermissionMap();
                        map.GroupId = perm.PermissionId.Value;
                        map.GroupId = usrGrp.GroupId;
                        db.UserGroupPermissionMaps.Add(map);
                    }
                    db.SaveChanges();
                }
                return GetUserGroupAdmin(usrGrp.GroupId);
            }
            catch (Exception e)
            {
                if (usrGrp != null)
                {
                    db.Dispose();
                    db = new ColspEntities();
                    db.UserGroups.Remove(usrGrp);
                }
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        [Route("api/UserGroups/Admin")]
        [HttpDelete]
        public HttpResponseMessage DeleteUserGroupAdmin(List<UserGroupRequest> request)
        {
            try
            {

                foreach (UserGroupRequest userGrpRq in request)
                {
                    if (userGrpRq.GroupId == null)
                    {
                        throw new Exception("User group id cannot be null");
                    }
                    var usrGrp = db.UserGroups.Find(userGrpRq.GroupId.Value);
                    if (usrGrp == null)
                    {
                        throw new Exception("Cannot find user " + userGrpRq.GroupId.Value);
                    }
                    if (!usrGrp.Type.Equals(Constant.USER_TYPE_ADMIN))
                    {
                        throw new Exception("Cannot deleted non admin user " + userGrpRq.GroupId.Value);
                    }
                    usrGrp.Status = Constant.STATUS_REMOVE;
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
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

        private bool UserGroupExists(int id)
        {
            return db.UserGroups.Count(e => e.GroupId == id) > 0;
        }
    }
}