using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Api.Constants;
using Colsp.Api.Extensions;
using Colsp.Model.Responses;
using Colsp.Api.Helpers;

namespace Colsp.Api.Controllers
{
    public class UserGroupsController : ApiController
    {
        private ColspEntities db = new ColspEntities();

        [Route("api/UserGroups/Seller")]
        [HttpDelete]
        public HttpResponseMessage DeleteUserGroupSeller(List<UserGroupRequest> request)
        {

            try
            {
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var userGroupIds = request.Where(w => w.GroupId != 0).Select(s => s.GroupId).ToList();
                var usrGrp = db.UserGroups.Where(w => Constant.USER_TYPE_SELLER.Equals(w.Type) && userGroupIds.Contains(w.GroupId)).ToList();
                if (usrGrp == null || usrGrp.Count == 0)
                {
                    throw new Exception("No deleted user group found");
                }
                db.UserGroups.RemoveRange(usrGrp);
                Util.DeadlockRetry(db.SaveChanges, "UserGroup");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/UserGroups/Seller")]
        [HttpGet]
        public HttpResponseMessage GetUserGroupSeller([FromUri] UserGroupRequest request)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId;
                var userGroupList = db.UserGroups
                    .Include(i => i.UserGroupPermissionMaps.Select(s => s.Permission))
                    .Include(i => i.UserGroupMaps)
                    .Where(w => w.Type.Equals(Constant.USER_TYPE_SELLER) 
                            && !w.Status.Equals(Constant.STATUS_REMOVE)
                            && w.ShopUserGroupMaps.Any(a => a.ShopId == shopId))
                    .Select(s => new {
                        s.GroupId,
                        s.GroupNameEn,
                        //s.GroupNameTh,
                        s.UpdatedDt,
                        //Permission = s.UserGroupPermissionMaps.Select(p => new { p.Permission.PermissionId, p.Permission.PermissionName }),
                        UserCount = s.UserGroupMaps.Count
                    });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, userGroupList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    userGroupList = userGroupList.Where(a => a.GroupNameEn.Contains(request.SearchText));
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

        [Route("api/UserGroups/Seller/{usergroupid}")]
        [HttpGet]
        public HttpResponseMessage GetUserGroupSeller(int usergroupid)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId;
                var usrGrp = db.UserGroups.Include(i => i.UserGroupPermissionMaps.Select(s => s.Permission))
                    .Where(w => w.GroupId == usergroupid && !w.Status.Equals(Constant.STATUS_REMOVE) 
                        && w.Type.Equals(Constant.USER_TYPE_SELLER)
                        && w.ShopUserGroupMaps.Any(a => a.ShopId == shopId))
                    .Select(s => new {
                        s.GroupId,
                        s.GroupNameEn,
                        //s.GroupNameTh,
                        Permission = s.UserGroupPermissionMaps.Select(ug => ug.Permission)
                    }).ToList();
                if (usrGrp == null || usrGrp.Count == 0)
                {
                    throw new Exception("User group not found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, usrGrp[0]);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/UserGroups/Seller")]
        [HttpPost]
        public HttpResponseMessage AddUserGroupSeller(UserGroupRequest request)
        {
            UserGroup usrGrp = null;
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var shopId = this.User.ShopRequest().ShopId;
                usrGrp = new UserGroup();
                usrGrp.GroupNameEn = Validation.ValidateString(request.GroupNameEn, "Role Name", true, 100, true);
                var usrGroupEntity = db.UserGroups.Where(w => w.GroupNameEn.Equals(usrGrp.GroupNameEn) 
                    && w.Type.Equals(Constant.USER_TYPE_SELLER) 
                    && w.ShopUserGroupMaps.Any(a=>a.ShopId == shopId)).FirstOrDefault();
                if (usrGroupEntity != null)
                {
                    throw new Exception("This role name has already been used. Please enter a different role name.");
                }
                usrGrp.GroupNameEn = request.GroupNameEn;
                //usrGrp.GroupNameTh = request.GroupNameTh;
                usrGrp.Status = Constant.STATUS_ACTIVE;
                usrGrp.Type = Constant.USER_TYPE_SELLER;
                usrGrp.CreatedBy = this.User.UserRequest().Email;
                usrGrp.CreatedDt = DateTime.Now;
                usrGrp.UpdatedBy = this.User.UserRequest().Email;
                usrGrp.UpdatedDt = DateTime.Now;
                db.UserGroups.Add(usrGrp);
                if (request.Permission != null)
                {
                    foreach (PermissionRequest perm in request.Permission)
                    {
                        if (perm.PermissionId == null)
                        {
                            throw new Exception("Permission id is null");
                        }
                        UserGroupPermissionMap map = new UserGroupPermissionMap();
                        map.PermissionId = perm.PermissionId.Value;
                        map.GroupId = usrGrp.GroupId;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.UserRequest().Email;
                        map.UpdatedDt = DateTime.Now;
                        usrGrp.UserGroupPermissionMaps.Add(map);
                        //db.UserGroupPermissionMaps.Add(map);
                    }
                    
                }
                ShopUserGroupMap shopMap = new ShopUserGroupMap();
                shopMap.GroupId = usrGrp.GroupId;
                shopMap.ShopId = shopId;
                shopMap.CreatedBy = this.User.UserRequest().Email;
                shopMap.CreatedDt = DateTime.Now;
                shopMap.UpdatedBy = this.User.UserRequest().Email;
                shopMap.UpdatedDt = DateTime.Now;
                usrGrp.ShopUserGroupMaps.Add(shopMap);
                //db.ShopUserGroupMaps.Add(shopMap);
                Util.DeadlockRetry(db.SaveChanges, "UserGroup");
                return GetUserGroupSeller(usrGrp.GroupId);
            }
            catch (Exception e)
            {
                if (usrGrp != null && usrGrp.GroupId != 0)
                {
                    db.UserGroups.Remove(usrGrp);
                    Util.DeadlockRetry(db.SaveChanges, "UserGroup");
                }
                
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/UserGroups/Seller/{usergroupid}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeUserSeller([FromUri]int usergroupid, UserGroupRequest request)
        {
            try
            {
                var shopId = this.User.ShopRequest().ShopId;
                var usrGrp = db.UserGroups.Where(w => Constant.USER_TYPE_SELLER.Equals(w.Type) && w.GroupId == usergroupid && w.ShopUserGroupMaps.Any(a => a.ShopId == shopId)).SingleOrDefault();
                if (usrGrp == null || usrGrp.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("User group not found");
                }
                //usrGrp.GroupNameTh = request.GroupNameTh;
                usrGrp.GroupNameEn = request.GroupNameEn;
                var mapList = db.UserGroupPermissionMaps.Where(w => w.GroupId == usrGrp.GroupId).ToList();
                if (request.Permission != null && request.Permission.Count > 0)
                {
                    bool addNew = false;
                    foreach (PermissionRequest permission in request.Permission)
                    {
                        if (mapList == null || mapList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            var current = mapList.Where(w => w.PermissionId == permission.PermissionId).SingleOrDefault();
                            if (current != null)
                            {
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
                                mapList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            UserGroupPermissionMap map = new UserGroupPermissionMap();
                            map.GroupId = usrGrp.GroupId;
                            map.PermissionId = permission.PermissionId.Value;
                            map.CreatedBy = this.User.UserRequest().Email;
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.UserRequest().Email;
                            map.UpdatedDt = DateTime.Now;
                            db.UserGroupPermissionMaps.Add(map);
                        }
                    }
                }
                if (mapList != null && mapList.Count > 0)
                {
                    db.UserGroupPermissionMaps.RemoveRange(mapList);
                }
                Util.DeadlockRetry(db.SaveChanges, "UserGroup");
                return GetUserGroupSeller(usrGrp.GroupId);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/UserGroups/Admin")]
        [HttpGet]
        public HttpResponseMessage GetUserGroupAdmin([FromUri] UserGroupRequest request)
        {
            try
            {
                var userGroupList = db.UserGroups.Include(i=>i.UserGroupPermissionMaps.Select(s=>s.Permission)).Include(i=>i.UserGroupMaps).Where(w=>w.Type.Equals(Constant.USER_TYPE_ADMIN) && !w.Status.Equals(Constant.STATUS_REMOVE)).Select(s=>new {
                    s.GroupId,
                    s.GroupNameEn,
                    //s.GroupNameTh,
                    s.UpdatedDt,
                    //Permission = s.UserGroupPermissionMaps.Select(p=>new { p.Permission.PermissionId, p.Permission.PermissionName }),
                    UserCount = s.UserGroupMaps.Count
                });
                if (request == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, userGroupList);
                }
                request.DefaultOnNull();
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    userGroupList = userGroupList.Where(a => a.GroupNameEn.Contains(request.SearchText));
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
                var usrGrp = db.UserGroups.Include(i => i.UserGroupPermissionMaps.Select(s => s.Permission))
                    .Where(w => w.GroupId == usergroupid && !w.Status.Equals(Constant.STATUS_REMOVE) && w.Type.Equals(Constant.USER_TYPE_ADMIN))
                    .Select(s => new {
                        s.GroupId,
                        s.GroupNameEn,
                        //s.GroupNameTh,
                        Permission = s.UserGroupPermissionMaps.Select(ug => ug.Permission)
                    }).ToList();
                if (usrGrp == null || usrGrp.Count == 0)
                {
                    throw new Exception("User group not found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, usrGrp[0]);
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
                usrGrp.GroupNameEn = Validation.ValidateString(request.GroupNameEn, "Role Name",true,100,true);
                var usrGroupEntity = db.UserGroups.Where(w => w.GroupNameEn.Equals(usrGrp.GroupNameEn) && w.Type.Equals(Constant.USER_TYPE_ADMIN)).FirstOrDefault();
                if(usrGroupEntity != null)
                {
                    throw new Exception("This role name has already been used. Please enter a different role name.");
                }
                //usrGrp.GroupNameTh = Validation.ValidateString(request.GroupNameTh, "Role Name (Thai)", false, 100, true);
                usrGrp.Status = Constant.STATUS_ACTIVE;
                usrGrp.Type = Constant.USER_TYPE_ADMIN;
                usrGrp.CreatedBy = this.User.UserRequest().Email;
                usrGrp.CreatedDt = DateTime.Now;
                usrGrp.UpdatedBy = this.User.UserRequest().Email;
                usrGrp.UpdatedDt = DateTime.Now;
                if (request.Permission != null)
                {
                    foreach (PermissionRequest perm in request.Permission)
                    {
                        if (perm.PermissionId == null)
                        {
                            throw new Exception("Permission id is null");
                        }
                        UserGroupPermissionMap map = new UserGroupPermissionMap();
                        map.PermissionId = perm.PermissionId.Value;
                        map.GroupId = usrGrp.GroupId;
                        map.CreatedBy = this.User.UserRequest().Email;
                        map.CreatedDt = DateTime.Now;
                        map.UpdatedBy = this.User.UserRequest().Email;
                        map.UpdatedDt = DateTime.Now;
                        usrGrp.UserGroupPermissionMaps.Add(map);
                    }
                }
                db.UserGroups.Add(usrGrp);
                Util.DeadlockRetry(db.SaveChanges, "UserGroup");
                return GetUserGroupAdmin(usrGrp.GroupId);
            }
            catch (Exception e)
            {
                if (usrGrp != null && usrGrp.GroupId != 0)
                {
                    db.UserGroups.Remove(usrGrp);
                    Util.DeadlockRetry(db.SaveChanges, "UserGroup");
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
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var userGroupIds = request.Where(w => w.GroupId != 0).Select(s => s.GroupId).ToList();
                var usrGrp = db.UserGroups.Where(w => Constant.USER_TYPE_ADMIN.Equals(w.Type) && userGroupIds.Contains(w.GroupId));
                db.UserGroups.RemoveRange(usrGrp);
                Util.DeadlockRetry(db.SaveChanges, "UserGroup");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/UserGroups/Admin/{usergroupid}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeUserAdmin([FromUri]int usergroupid, UserGroupRequest request)
        {
            try
            {
                var usrGrp = db.UserGroups.Find(usergroupid);
                if (usrGrp == null || usrGrp.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("User group not found");
                }
                if (!usrGrp.Type.Equals(Constant.USER_TYPE_ADMIN))
                {
                    throw new Exception("This user group is not admin");
                }
                //usrGrp.GroupNameTh = request.GroupNameTh;
                usrGrp.GroupNameEn = request.GroupNameEn;

                //Check duplication of role name ,Preen
                var usrGroupEntity = db.UserGroups.Where(w => w.GroupNameEn.Equals(usrGrp.GroupNameEn) && w.Type.Equals(Constant.USER_TYPE_ADMIN)).FirstOrDefault();
                if (usrGroupEntity != null)
                {
                    throw new Exception("This role name has already been used. Please enter a different role name.");
                }

                var mapList = db.UserGroupPermissionMaps.Where(w => w.GroupId == usrGrp.GroupId).ToList();
                if (request.Permission != null && request.Permission.Count > 0)
                {
                    bool addNew = false;
                    foreach (PermissionRequest permission in request.Permission)
                    {
                        if (mapList == null || mapList.Count == 0)
                        {
                            addNew = true;
                        }
                        if (!addNew)
                        {
                            var current = mapList.Where(w => w.PermissionId == permission.PermissionId).SingleOrDefault();
                            if (current != null)
                            {
                                current.UpdatedBy = this.User.UserRequest().Email;
                                current.UpdatedDt = DateTime.Now;
                                mapList.Remove(current);
                            }
                            else
                            {
                                addNew = true;
                            }
                        }
                        if (addNew)
                        {
                            UserGroupPermissionMap map = new UserGroupPermissionMap();
                            map.GroupId = usrGrp.GroupId;
                            map.PermissionId = permission.PermissionId.Value;
                            map.CreatedBy = this.User.UserRequest().Email;
                            map.CreatedDt = DateTime.Now;
                            map.UpdatedBy = this.User.UserRequest().Email;
                            map.UpdatedDt = DateTime.Now;
                            db.UserGroupPermissionMaps.Add(map);
                        }
                    }
                }
                if (mapList != null && mapList.Count > 0)
                {
                    db.UserGroupPermissionMaps.RemoveRange(mapList);
                }
                Util.DeadlockRetry(db.SaveChanges, "UserGroup");
                return GetUserGroupAdmin(usrGrp.GroupId);
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