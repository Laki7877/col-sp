using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using Colsp.Entity.Models;
using Colsp.Model.Requests;
using Colsp.Model.Responses;
using Colsp.Api.Extensions;
using System;
using System.Net.Http;
using Colsp.Api.Constants;
using System.Collections.Generic;
using Colsp.Api.Helpers;
using Colsp.Api.Services;
using System.Security.Claims;
using Colsp.Api.Security;
using System.Data.Entity.SqlServer;
using System.Text;
using Cenergy.Dazzle.Admin.Security.Cryptography;

namespace Colsp.Api.Controllers
{
    public class UsersController : ApiController
    {
        private ColspEntities db = new ColspEntities();
        private SaltedSha256PasswordHasher salt = new SaltedSha256PasswordHasher();

        [Route("api/Users/Seller")]
        [HttpDelete]
        public HttpResponseMessage DeleteUserSeller(List<UserRequest> request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                var usr = db.Users.Where(w => true);
                var userIds = request.Where(w => w.UserId != 0).Select(s => s.UserId).ToList();
                usr = db.Users.Where(w => Constant.USER_TYPE_SELLER.Equals(w.Type) && userIds.Contains(w.UserId));
                if (User.ShopRequest() != null)
                {
                    var shopId = this.User.ShopRequest().ShopId;
                    usr = usr.Where(w => w.UserShopMaps.Any(a => a.ShopId == shopId));
                }
                if (usr == null)
                {
                    throw new Exception("No deleted users found");
                }
                var shopOwnerUser = usr.Any(a => a.Shops.Count != 0);
                if (shopOwnerUser)
                {
                    throw new Exception("This user is the shop owner and cannot be deleted. Please contact your administrator for more details.");
                }
                db.Users.RemoveRange(usr);
                Util.DeadlockRetry(db.SaveChanges, "User");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Seller")]
        [HttpGet]
        public HttpResponseMessage GetUserSeller([FromUri] UserRequest request)
        {
            try
            {

                var tmpUser = db.Users
                    .Where(w => Constant.USER_TYPE_SELLER.Equals(w.Type))
                    .Include(i => i.UserGroupMaps)
                    .Include(i => i.UserGroupMaps.Select(s => s.UserGroup))
                    .Include(i=>i.UserShopMaps.Select(s=>s.Shop));
                if (User.HasPermission("Delete Seller"))
                {

                }
                else if (this.User.ShopRequest() != null)
                {
                    int shopId = this.User.ShopRequest().ShopId;
                    tmpUser = tmpUser.Where(w => w.UserShopMaps.Any(a => a.ShopId == shopId));
                }
                else
                {
                    throw new Exception("No permission to do");
                }

                var userList = tmpUser.Select(s => new
                {
                    s.UserId,
                    s.NameEn,
                    s.NameTh,
                    s.Email,
                    s.UpdatedDt,
                    UserGroup = s.UserGroupMaps.Select(ug => ug.UserGroup.GroupNameEn),
                    Shops = s.UserShopMaps.Select(sh=>sh.Shop.ShopNameEn),
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
                if (!string.IsNullOrEmpty(request._filter))
                {
                    if (string.Equals("ShopOwner", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        userList = userList.Where(w => w.UserGroup.Any(a=>a.Equals("Shop Owner")));
                    }
                    if (string.Equals("NotShopOwner", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        userList = userList.Where(w => w.UserGroup.All(a => !a.Equals("Shop Owner")));
                    }
                    if (string.Equals("NoShop", request._filter, StringComparison.OrdinalIgnoreCase))
                    {
                        userList = userList.Where(w => w.Shops.All(a=>a==null));
                    }
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

        [Route("api/Users/Seller/{userId}")]
        [HttpGet]
        public HttpResponseMessage GetUserSeller(int userId)
        {
            try
            {
                int shopId = this.User.ShopRequest().ShopId;
                var usr = db.Users.Include(i => i.UserGroupMaps.Select(s => s.UserGroup))
                    .Where(w => w.UserId == userId 
                                && !w.Status.Equals(Constant.STATUS_REMOVE) 
                                && w.Type.Equals(Constant.USER_TYPE_SELLER)
                                && w.UserShopMaps.Any(a => a.ShopId == shopId))
                    .Select(s => new {
                        s.UserId,
                        s.Email,
                        s.NameEn,
                        s.NameTh,
                        s.Phone,
                        s.Position,
                        s.Division,
                        s.EmployeeId,
                        UserGroup = s.UserGroupMaps.Select(ug => new { ug.UserGroup.GroupId, ug.UserGroup.GroupNameEn, Permission = ug.UserGroup.UserGroupPermissionMaps.Select(p => new { p.Permission.PermissionId, p.Permission.PermissionName }) })
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

        [Route("api/Users/Seller")]
        [HttpPost]
        public HttpResponseMessage AddUserSeller(UserRequest request)
        {
            User user = null;
            try
            {
                user = new User();
                SetupUser(user, request);
                #region Password
                user.Password = salt.HashPassword(Validation.ValidateString(request.Password, "Password", true, 100, false));
                user.PasswordLastChg = string.Empty;
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_SELLER;
                user.CreatedBy = this.User.UserRequest().Email;
                user.CreatedDt = DateTime.Now;
                user.UpdatedBy = this.User.UserRequest().Email;
                user.UpdatedDt = DateTime.Now;
                #region User Group
                if (request.UserGroup != null)
                {
                    foreach (UserGroupRequest usrGrp in request.UserGroup)
                    {
                        if (usrGrp.GroupId == 0)
                        {
                            throw new Exception("User group id is null");
                        }
                        user.UserGroupMaps.Add(new UserGroupMap()
                        {
                            GroupId = usrGrp.GroupId,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                user.UserShopMaps.Add(new UserShopMap()
                {
                    ShopId = this.User.ShopRequest().ShopId,
                    UserId = user.UserId,
                    CreatedBy = this.User.UserRequest().Email,
                    CreatedDt = DateTime.Now,
                    UpdatedBy = this.User.UserRequest().Email,
                    UpdatedDt = DateTime.Now,
                });
                #endregion
                db.Users.Add(user);
                Util.DeadlockRetry(db.SaveChanges, "User");
                return GetUserSeller(user.UserId);
            }
            catch (Exception e)
            {
                #region Rollback
                if (user != null && user.UserId != 0)
                {
                    db.Users.Remove(user);
                    Util.DeadlockRetry(db.SaveChanges, "User");
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Seller/{userId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeUserSeller([FromUri]int userId, UserRequest request)
        {
            try
            {
                var user = db.Users.Where(w => w.UserId == userId && Constant.USER_TYPE_SELLER.Equals(w.Type)).SingleOrDefault();
                #region Validation
                if (user == null || user.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("User not found");
                }
                #endregion
                SetupUser(user, request);
                #region Password
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordLastChg = user.Password;
                    user.Password = salt.HashPassword(request.Password);
                }
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_SELLER;
                user.UpdatedBy = this.User.UserRequest().Email;
                user.UpdatedDt = DateTime.Now;
                #region User Group
                var usrGrpList = db.UserGroupMaps.Where(w => w.UserId == user.UserId).ToList();
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
                            map.UserId = user.UserId;
                            map.GroupId = grp.GroupId;
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
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "User");
                return GetUserSeller(user.UserId);
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
                        UserGroup = s.UserGroupMaps.Select(ug=>new { ug.UserGroup.GroupNameEn })
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
                    || a.Email.Contains(request.SearchText)
                    || SqlFunctions.StringConvert((double)a.UserId).Contains(request.SearchText));
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
                        UserGroup = s.UserGroupMaps.Select(ug => new { ug.UserGroup.GroupId, ug.UserGroup.GroupNameEn, Permission = ug.UserGroup.UserGroupPermissionMaps.Select(p => new { p.Permission.PermissionId, p.Permission.PermissionName}) })
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
            User user = null;
            try
            {
                user = new User();
                SetupUser(user, request);
                #region Password
                user.Password = salt.HashPassword(Validation.ValidateString(request.Password, "Password", true, 100, false));
                user.PasswordLastChg = string.Empty;
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_ADMIN;
                user.CreatedBy = this.User.UserRequest().Email;
                user.CreatedDt = DateTime.Now;
                user.UpdatedBy = this.User.UserRequest().Email;
                user.UpdatedDt = DateTime.Now;
                #region User Group
                if (request.UserGroup != null)
                {
                    foreach (UserGroupRequest usrGrp in request.UserGroup)
                    {
                        if (usrGrp.GroupId == 0)
                        {
                            throw new Exception("User group id is null");
                        }
                        user.UserGroupMaps.Add(new UserGroupMap()
                        {
                            GroupId = usrGrp.GroupId,
                            CreatedBy = this.User.UserRequest().Email,
                            CreatedDt = DateTime.Now,
                            UpdatedBy = this.User.UserRequest().Email,
                            UpdatedDt = DateTime.Now,
                        });
                    }
                }
                #endregion
                db.Users.Add(user);
                Util.DeadlockRetry(db.SaveChanges, "User");
                return GetUserAdmin(user.UserId);
            }
            catch (Exception e)
            {
                #region Rollback
                if (user != null && user.UserId != 0)
                {
                    db.Users.Remove(user);
                    Util.DeadlockRetry(db.SaveChanges, "User");
                }
                #endregion
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Admin/{userId}")]
        [HttpPut]
        public HttpResponseMessage SaveChangeUserAdmin([FromUri]int userId, UserRequest request)
        {
            try
            {
                var user = db.Users.Find(userId);
                #region Validation
                if (user == null || user.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("User not found");
                }
                if (!user.Type.Equals(Constant.USER_TYPE_ADMIN))
                {
                    throw new Exception("This user is not admin");
                }
                #endregion
                SetupUser(user, request);
                #region Password
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordLastChg = user.Password;
                    user.Password = salt.HashPassword(request.Password);
                }
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_ADMIN;
                user.UpdatedBy = this.User.UserRequest().Email;
                user.UpdatedDt = DateTime.Now;
                #region User Group
                var usrGrpList = db.UserGroupMaps.Where(w => w.UserId == user.UserId).ToList();
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
                            map.UserId = user.UserId;
                            map.GroupId = grp.GroupId;
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
                #endregion
                Util.DeadlockRetry(db.SaveChanges, "User");
                return GetUserAdmin(user.UserId);
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
                if (request == null)
                {
                    throw new Exception("Invalid request");
                }
                var userIds = request.Where(w => w.UserId != 0).Select(s => s.UserId).ToList();
                var usr = db.Users.Where(w => Constant.USER_TYPE_ADMIN.Equals(w.Type) && userIds.Contains(w.UserId)).ToList();
                if(usr == null || usr.Count == 0)
                {
                    throw new Exception("No deleted users found");
                }
                db.Users.RemoveRange(usr);
                Util.DeadlockRetry(db.SaveChanges, "User");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/ClearCache")]
        [HttpGet]
        public HttpResponseMessage ClearCache()
        {
            Cache.Clear();
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("api/Users/Login")]
        [HttpGet]
        public HttpResponseMessage LoginUser()
        {
            try
            {
                ClaimRequest claim = new ClaimRequest();
                
                var claimsIdentity = User.Identity as ClaimsIdentity;
                claim.Permission = claimsIdentity.Claims
                    .Where(w => w.Type.Equals("Permission")).Select(s => new { Permission = s.Value, PermissionGroup = s.ValueType }).ToList();
                claim.Shop = User.ShopRequest();
                claim.User = new { NameEn = this.User.UserRequest().NameEn , Email = this.User.UserRequest().Email, IsAdmin = Constant.USER_TYPE_ADMIN.Equals(this.User.UserRequest().Type) };
                return Request.CreateResponse(HttpStatusCode.OK, claim);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        //[Route("api/Users/Login")]
        //[HttpPost]
        //public HttpResponseMessage LoginUser(string username,string password,bool IsAdmin = false)
        //{
        //    try
        //    {
        //        ClaimRequest claim = new ClaimRequest();

        //        var claimsIdentity = User.Identity as ClaimsIdentity;
        //        claim.Permission = claimsIdentity.Claims
        //            .Where(w => w.Type.Equals("Permission")).Select(s => new { Permission = s.Value, PermissionGroup = s.ValueType }).ToList();
        //        claim.Shop = User.ShopRequest();
        //        claim.User = new { NameEn = this.User.UserRequest().NameEn, Email = User.UserRequest().Email, IsAdmin = Constant.USER_TYPE_ADMIN.Equals(this.User.UserRequest().Type) };
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
        //    }
        //}


        [Route("api/Users/Admin/Login/{userId}")]
        [HttpGet]
        public HttpResponseMessage LoginAs(int userId)
        {
            try
            {
                var user = db.Users.Where(u => u.UserId==userId)
                            .Select(u => new
                            {
                                u.UserId,
                                u.NameEn,
                                u.NameTh,
                                u.Email,
                                Shops = u.UserShopMaps.Select(s=>s.Shop),
                                u.Type,
                                Permission = u.UserGroupMaps.Select(um => um.UserGroup.UserGroupPermissionMaps.Select(pm => pm.Permission))
                            })
                            .FirstOrDefault();

                // Check for user
                if (user == null)
                {
                    throw new Exception("Cannot find user with id "+ userId);
                }

                // Get all permissions
                var userPermissions = user.Permission;

                // Assign claims
                var claims = new List<Claim>();
                foreach (var userGroup in userPermissions)
                {
                    foreach (var p in userGroup)
                    {
                        if (!claims.Exists(m => m.Value.Equals(p.PermissionName)))
                        {
                            Claim claim = new Claim("Permission", p.PermissionName, p.PermissionGroup, null);
                            claims.Add(claim);
                        }
                    }

                }
                var identity = new ClaimsIdentity(claims, "Basic");
                var principal = new UsersPrincipal(identity,
                    user.Shops == null ? null : user.Shops.Select(s => new ShopRequest { ShopId = s.ShopId, ShopNameEn = s.ShopNameEn }).ToList(),
                    this.User.UserRequest());

                ClaimRequest claimRq = new ClaimRequest();

                var claimsIdentity = identity;
                claimRq.Permission = claims.Where(w => w.Type.Equals("Permission")).Select(s => new { Permission = s.Value, PermissionGroup = s.ValueType }).ToList();
                claimRq.Shop = principal.ShopRequest();
                claimRq.User = this.User.UserRequest();

                Cache.Delete(Request.Headers.Authorization.Parameter);
                Cache.Add(Request.Headers.Authorization.Parameter, principal);
                return Request.CreateResponse(HttpStatusCode.OK, claimRq);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Admin/LogoutAs")]
        [HttpGet]
        public HttpResponseMessage LogoutAs()
        {
            try
            {
                string email = this.User.UserRequest().Email;
                var user = db.Users.Where(u => u.Email == email)
                            .Select(u => new
                            {
                                u.UserId,
                                u.NameEn,
                                u.NameTh,
                                u.Email,
                                Shops = u.UserShopMaps.Select(s => s.Shop),
                                u.Type,
                                Permission = u.UserGroupMaps.Select(um => um.UserGroup.UserGroupPermissionMaps.Select(pm => pm.Permission))
                            })
                            .FirstOrDefault();

                // Check for user
                if (user == null)
                {
                    throw new Exception("Cannot find user with email " + email);
                }

                // Get all permissions
                var userPermissions = user.Permission;

                // Assign claims
                var claims = new List<Claim>();
                foreach (var userGroup in userPermissions)
                {
                    foreach (var p in userGroup)
                    {
                        if (!claims.Exists(m => m.Value.Equals(p.PermissionName)))
                        {
                            Claim claim = new Claim("Permission", p.PermissionName, p.PermissionGroup, null);
                            claims.Add(claim);
                        }
                    }

                }
                var identity = new ClaimsIdentity(claims, "Basic");
                var principal = new UsersPrincipal(identity,
                    user.Shops == null ? null : user.Shops.Select(s => new ShopRequest { ShopId = s.ShopId, ShopNameEn = s.ShopNameEn }).ToList(),
                    this.User.UserRequest());

                ClaimRequest claimRq = new ClaimRequest();

                var claimsIdentity = identity;
                claimRq.Permission = claims.Where(w => w.Type.Equals("Permission")).Select(s => new { Permission = s.Value, PermissionGroup = s.ValueType }).ToList();
                claimRq.Shop = principal.ShopRequest();
                claimRq.User = new { NameEn = this.User.UserRequest().NameEn, Email = this.User.UserRequest().Email, IsAdmin = Constant.USER_TYPE_ADMIN.Equals(this.User.UserRequest().Type) };

                Cache.Delete(Request.Headers.Authorization.Parameter);
                Cache.Add(Request.Headers.Authorization.Parameter, principal);
                return Request.CreateResponse(HttpStatusCode.OK, claimRq);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/ChangePassword")]
        [HttpPut]
        public HttpResponseMessage ChangePassword(UserRequest request)
        {
            try
            {
                string email = this.User.UserRequest().Email;
                User user = null;

                if (!string.IsNullOrEmpty(request.Email)&&Constant.USER_TYPE_ADMIN.Equals( this.User.UserRequest().Type))
                {
                    user = db.Users.Where(w => w.Email.Equals(request.Email)).ToList().SingleOrDefault();
                }
                else
                {
                    user = db.Users.Where(w => w.Email.Equals(email)).SingleOrDefault();
                    if(!salt.CheckPassword(request.Password, user.Password))
                    {
                        user = null;
                    }
                }
                if (user == null)
                {
                    throw new Exception("User and password not match");
                }
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    throw new Exception("Password cannot be empty");
                }
                user.PasswordLastChg = user.Password;
                user.Password = salt.HashPassword(request.NewPassword);
                Util.DeadlockRetry(db.SaveChanges, "User");
                Cache.Delete(Request.Headers.Authorization.Parameter);
                this.User.UserRequest().Password = user.Password;
                return Request.CreateResponse(HttpStatusCode.OK, Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Concat(user.Email, ":", user.Password))));
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Logout")]
        [HttpGet]
        public HttpResponseMessage Logout()
        {
            try
            {
                Cache.Delete(Request.Headers.Authorization.Parameter);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Onboarding")]
        [HttpGet]
        public HttpResponseMessage Onboarding()
        {
            try
            {
                bool IsPasswordChange = this.User.UserRequest().IsPasswordChange;
                var shopId = this.User.ShopRequest().ShopId;
                var productTotalCount = db.ProductStages.Where(w => w.ShopId == shopId).Count();
                var shopBanner = db.ShopImages.Where(w => w.ShopId == shopId).Count();
                var productApprove = db.ProductStages.Where(w => w.ShopId == shopId && Constant.PRODUCT_STATUS_APPROVE.Equals(w.Status)).Count();
                bool IsProduct = productTotalCount > 0 ? true : false;
                bool isBanner = shopBanner > 0 ? true : false;
                bool IsProductApprove = productApprove > 0 ? true : false; 
                return Request.CreateResponse(new OnBoardRequest()
                {
                    AddProduct = IsProduct,
                    ChangePassword = IsPasswordChange,
                    ProductApprove = IsProductApprove,
                    DecorateStore = isBanner,
                    SetUpShop = this.User.ShopRequest().IsShopReady
                });
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }


        private void SetupUser(User user, UserRequest request)
        {
            user.Email = Validation.ValidateString(request.Email, "Email", true, 100, false);
            user.NameEn = Validation.ValidateString(request.NameEn, "Name", true, 100, false);
            user.NameTh = Validation.ValidateString(request.NameTh, "Name", false, 100, false, string.Empty);
            user.Division = Validation.ValidateString(request.Division, "Division", false, 100, false, string.Empty);
            user.Phone = Validation.ValidateString(request.Phone, "Phone", true, 100, false);
            user.Position = Validation.ValidateString(request.Position, "Position", false, 100, false, string.Empty);
            user.EmployeeId = Validation.ValidateString(request.EmployeeId, "Employee Id", false, 100, false, string.Empty);
            user.Mobile = Validation.ValidateString(request.Mobile, "Mobile", false, 20, false, string.Empty);
            user.Fax = Validation.ValidateString(request.Fax, "Fax", false, 20, false, string.Empty);
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