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
                var userIds = request.Where(w => w.UserId != 0).Select(s => s.UserId).ToList();
                var usr = db.Users.Where(w => Constant.USER_TYPE_SELLER.Equals(w.Type) && userIds.Contains(w.UserId));
                if (User.ShopRequest() != null)
                {
                    var shopId = User.ShopRequest().ShopId;
                    usr = usr.Where(w => w.UserShopMaps.Any(a => a.ShopId == shopId));
                }
                if (usr == null)
                {
                    throw new Exception("No deleted users found");
                }
                var shopOwnerUser = usr.Any(a => a.Shops.Count != 0);
                if (shopOwnerUser)
                {
                    throw new Exception("This user is Shop Owner and cannot be deleted. Shop Owner can only be deleted when he does not belong to any shop.");
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
                //var tmpUser = db.Users
                //    .Where(w => Constant.USER_TYPE_SELLER.Equals(w.Type))
                //    .Include(i => i.UserGroupMaps)
                //    .Include(i => i.UserGroupMaps.Select(s => s.UserGroup))
                //    .Include(i=>i.UserShopMaps.Select(s=>s.Shop));
                var tmpUser = db.Users
                    .Where(w => Constant.USER_TYPE_SELLER.Equals(w.Type))
                    .Select(s => new
                    {
                        UserShopMaps = s.UserShopMaps.Select(sm=>new
                        {
                            sm.ShopId,
                            Shop = sm.Shop == null ? null : new
                            {
                                sm.Shop.ShopNameEn
                            }
                        }),
                        s.UserId,
                        s.NameEn,
                        s.NameTh,
                        s.Email,
                        s.UpdateOn,
                        UserGroupMaps = s.UserGroupMaps.Select(sm=>new
                        {
                            UserGroup = sm.UserGroup == null ? null : new
                            {
                                sm.UserGroup.GroupNameEn
                            }
                        })
                    });
                if (User.ShopRequest() != null)
                {
                    int shopId = User.ShopRequest().ShopId;
                    tmpUser = tmpUser.Where(w => w.UserShopMaps.Any(a => a.ShopId == shopId));
                }

                var userList = tmpUser.Select(s => new
                {
                    s.UserId,
                    s.NameEn,
                    s.NameTh,
                    s.Email,
                    UpdatedDt = s.UpdateOn,
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
                int shopId = User.ShopRequest().ShopId;
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
                        UserGroup = s.UserGroupMaps.Select(ug => new
                        {
                            ug.UserGroup.GroupId,
                            ug.UserGroup.GroupNameEn,
                            Permission = ug.UserGroup.UserGroupPermissionMaps.Select(p => new
                            {
                                p.Permission.PermissionId,
                                p.Permission.PermissionName
                            })
                        }),
                        Brands = s.UserBrandMaps.Select(sb => new
                        {
                            sb.BrandId,
                            sb.Brand.BrandNameEn,
                            UpdateOn = sb.Brand.UpdateOn
                        })
                    }).SingleOrDefault();
                if (usr == null )
                {
                    throw new Exception("User not found");
                }
                return Request.CreateResponse(HttpStatusCode.OK, usr);
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
                SetupUser(user, request, db);
                #region Password
                user.Password = salt.HashPassword(Validation.ValidateString(request.Password, "Password", true, 100, false));
                user.PasswordLastChg = string.Empty;
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_SELLER;
                user.CreateBy = User.UserRequest().Email;
                user.CreateOn = DateTime.Now;
                user.UpdateBy = User.UserRequest().Email;
                user.UpdateOn = DateTime.Now;
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
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                    }
                }
                user.UserShopMaps.Add(new UserShopMap()
                {
                    ShopId = User.ShopRequest().ShopId,
                    UserId = user.UserId,
                    CreateBy = User.UserRequest().Email,
                    CreateOn = DateTime.Now,
                    UpdateBy = User.UserRequest().Email,
                    UpdateOn = DateTime.Now,
                });
                #endregion
                user.UserId = db.GetNextUserId().SingleOrDefault().Value;
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
                var user = db.Users
                    .Where(w => w.UserId == userId && Constant.USER_TYPE_SELLER.Equals(w.Type))
                    .Include(i => i.UserBrandMaps)
                    .SingleOrDefault();
                #region Validation
                if (user == null || user.Status.Equals(Constant.STATUS_REMOVE))
                {
                    throw new Exception("User not found");
                }
                #endregion
                SetupUser(user, request,db);
                #region Password
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordLastChg = user.Password;
                    user.Password = salt.HashPassword(request.Password);
                }
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_SELLER;
                user.UpdateBy = User.UserRequest().Email;
                user.UpdateOn = DateTime.Now;
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
                                current.UpdateBy = User.UserRequest().Email;
                                current.UpdateOn = DateTime.Now;
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
                            map.CreateBy = User.UserRequest().Email;
                            map.CreateOn = DateTime.Now;
                            map.UpdateBy = User.UserRequest().Email;
                            map.UpdateOn = DateTime.Now;
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
                        UpdatedDt = s.UpdateOn,
                        UserGroup = s.UserGroupMaps.Select(ug=>new
                        {
                            ug.UserGroup.GroupNameEn
                        })
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
                        UserGroup = s.UserGroupMaps.Select(ug => new
                        {
                            ug.UserGroup.GroupId,
                            ug.UserGroup.GroupNameEn,
                            Permission = ug.UserGroup.UserGroupPermissionMaps.Select(p => new
                            {
                                p.Permission.PermissionId,
                                p.Permission.PermissionName
                            })
                        })
                    }).SingleOrDefault();
                if (usr == null)
                {
                    throw new Exception("User not found");
                }
                
                return Request.CreateResponse(HttpStatusCode.OK, usr);
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
                SetupUser(user, request,db);
                #region Password
                user.Password = salt.HashPassword(Validation.ValidateString(request.Password, "Password", true, 100, false));
                user.PasswordLastChg = string.Empty;
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_ADMIN;
                user.CreateBy = User.UserRequest().Email;
                user.CreateOn = DateTime.Now;
                user.UpdateBy = User.UserRequest().Email;
                user.UpdateOn = DateTime.Now;
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
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                    }
                }
                #endregion
                user.UserId = db.GetNextUserId().SingleOrDefault().Value;
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
                SetupUser(user, request, db);
                #region Password
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.PasswordLastChg = user.Password;
                    user.Password = salt.HashPassword(request.Password);
                }
                #endregion
                user.Status = Constant.STATUS_ACTIVE;
                user.Type = Constant.USER_TYPE_ADMIN;
                user.UpdateBy = User.UserRequest().Email;
                user.UpdateOn = DateTime.Now;
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
                                current.UpdateBy = User.UserRequest().Email;
                                current.UpdateOn = DateTime.Now;
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
                            map.CreateBy = User.UserRequest().Email;
                            map.CreateOn = DateTime.Now;
                            map.UpdateBy = User.UserRequest().Email;
                            map.UpdateOn = DateTime.Now;
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
                var userIds = request.Where(w => w.UserId != 0).Select(s => s.UserId);
                var usr = db.Users.Where(w => Constant.USER_TYPE_ADMIN.Equals(w.Type) && userIds.Contains(w.UserId));
                if(usr != null || usr.Count() > 0)
                {
                    db.Users.RemoveRange(usr);
                    Util.DeadlockRetry(db.SaveChanges, "User");
                }
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

        [Route("api/Users/Profile")]
        [HttpGet]
        public HttpResponseMessage RefreshProfile()
        {
            try
            {
                ClaimRequest claim = new ClaimRequest();
                
                var claimsIdentity = User.Identity as ClaimsIdentity;
                claim.Permission = claimsIdentity.Claims
                    .Where(w => w.Type.Equals("Permission"))
                    .Select(s => new
                    {
                        Permission = s.Value,
                        PermissionGroup = s.ValueType
                    });
                claim.Shop = User.ShopRequest();
                claim.User = new
                {
                    NameEn = User.UserRequest().NameEn ,
                    Email = User.UserRequest().Email,
                    IsAdmin = Constant.USER_TYPE_ADMIN.Equals(User.UserRequest().Type)
                };
                return Request.CreateResponse(HttpStatusCode.OK, claim);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Users/Login")]
        [HttpPost]
        [OverrideAuthentication,OverrideAuthorization]
        public HttpResponseMessage Login(UserRequest request)
        {
            try
            {
                if(request == null)
                {
                    throw new Exception("Invalid request");
                }
                string email = request.Email;
                #region Query
                var user = db.Users.Where(w => w.Email.Equals(email)).Select(s => new
                {
                     s.Email,
                     s.Password,
                     s.LoginFailCount,
                     s.Type,
                     s.UserId,
                     s.NameEn,
                     s.NameTh,
                     s.PasswordLastChg,
                    UserShopMaps = s.UserShopMaps.Select(sm=>new
                    {
                        Shop = new
                        {
                            sm.Shop.ShopId,
                            sm.Shop.ShopNameEn,
                            sm.Shop.Status,
                            sm.Shop.ShopGroup,
                        }
                    }),
                    UserBrandMaps = s.UserBrandMaps.Select(sb=>new
                    {
                        sb.BrandId
                    }),
                    UserGroupMaps = s.UserGroupMaps.Select(sg=>new
                    {
                        UserGroup = new
                        {
                            UserGroupPermissionMaps = sg.UserGroup.UserGroupPermissionMaps.Select(sp => new
                            {
                                Permission = new
                                {
                                    sp.PermissionId,
                                    sp.Permission.Parent,
                                    sp.Permission.OverrideParent,
                                }
                            }),
                        }
                    }),
                }).SingleOrDefault();
                #endregion
                #region Validateion
                string password = request.Password;
                if (user == null)
                {
                    throw new Exception("Email " + email + " not found");
                }
                if (!salt.CheckPassword(password, user.Password))
                {
                    db.Database.ExecuteSqlCommand(string.Concat("UPDATE [User] SET LoginFailCount = ", (user.LoginFailCount + 1)," WHERE UserId = " , user.UserId));
                    throw new Exception("Email and password not match");
                }
                if (user.Type.Equals(Constant.USER_TYPE_SELLER) && 
                   (
                        user.UserShopMaps == null 
                        || user.UserShopMaps.Count() == 0 
                        || user.UserShopMaps.Any(a => a.Shop.Status.Equals(Constant.STATUS_REMOVE))
                   ))
                {
                    throw new Exception("Please contact system administrator.");
                }
                #endregion
                // Get all permissions
                var userPermissions = user.UserGroupMaps
                    .Select(s => s.UserGroup.UserGroupPermissionMaps.Select(sp => sp.Permission));

                var claims = new List<Claim>();
                foreach (var userGroup in userPermissions)
                {
                    foreach (var p in userGroup)
                    {
                        if (!claims.Exists(m => m.Value.Equals(p.PermissionId.ToString())))
                        {
                            Claim claim = new Claim("Permission", p.PermissionId.ToString(), p.Parent.ToString(), p.OverrideParent.ToString());
                            claims.Add(claim);
                        }
                    }
                }
                var identity = new ClaimsIdentity(claims, Constant.AUTHEN_SCHEMA);
                var token = salt.HashPassword(string.Concat(user.UserId + DateTime.Now.ToString("ddMMyyHHmmss")));
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(token);
                token = Convert.ToBase64String(plainTextBytes);

                var principal = new UsersPrincipal(identity,
                    user.UserShopMaps == null ? null : user.UserShopMaps.Select(s => new ShopRequest
                    {
                        ShopId = s.Shop.ShopId,
                        ShopNameEn = s.Shop.ShopNameEn,
                        Status = s.Shop.Status,
                        ShopGroup = s.Shop.ShopGroup,
                    }).ToList(),
                    user.UserBrandMaps == null ? null : user.UserBrandMaps.Select(s=> new BrandRequest
                    {
                        BrandId = s.BrandId,
                    }).ToList(),
                    new UserRequest {
                        UserId = user.UserId,
                        Email = user.Email,
                        Token = token,
                        NameEn = user.NameEn,
                        NameTh = user.NameTh,
                        Type = user.Type,
                        IsPasswordChange = string.IsNullOrEmpty(user.PasswordLastChg) ? false : true,
                        IsAdmin = Constant.USER_TYPE_ADMIN.Equals(user.Type)
                    }
                    ,DateTime.Now);

                db.Database.ExecuteSqlCommand(string.Concat("UPDATE [User] SET LastLoginDt = '", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "', LoginFailCount = 0 ", " WHERE UserId = ", user.UserId));
                User = principal;
                var claimsIdentity = User.Identity as ClaimsIdentity;
                ClaimRequest claimRs = new ClaimRequest()
                {
                    Shop = User.ShopRequest(),
                    User = User.UserRequest(),
                    Permission = claimsIdentity.Claims.Where(w => w.Type.Equals("Permission")).Select(s => new
                    {
                        PermissionId = int.Parse(s.Value),
                        Parent = int.Parse(s.ValueType),
                        OverrideParent = int.Parse(s.Issuer)
                    }).ToList()
                };
                Cache.Add(token, principal);
                return Request.CreateResponse(HttpStatusCode.OK, claimRs);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

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
                                Brands = u.UserBrandMaps,
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
                    user.Shops == null ? null : user.Shops.Select(s => new ShopRequest
                    {
                        ShopId = s.ShopId,
                        ShopNameEn = s.ShopNameEn
                    }).ToList(),
                    user.Brands == null ? null : user.Brands.Select(s => new BrandRequest
                    {
                        BrandId = s.BrandId,
                    }).ToList(),
                    User.UserRequest(),DateTime.Now);

                ClaimRequest claimRq = new ClaimRequest();

                var claimsIdentity = identity;
                claimRq.Permission = claims.Where(w => w.Type.Equals("Permission")).Select(s => new { Permission = s.Value, PermissionGroup = s.ValueType }).ToList();
                claimRq.Shop = principal.ShopRequest();
                claimRq.User = User.UserRequest();

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
                string email = User.UserRequest().Email;
                var user = db.Users.Where(u => u.Email == email)
                            .Select(u => new
                            {
                                u.UserId,
                                u.NameEn,
                                u.NameTh,
                                u.Email,
                                Shops = u.UserShopMaps.Select(s => s.Shop),
                                u.Type,
                                Brands = u.UserBrandMaps,
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
                    user.Shops == null ? null : user.Shops.Select(s => new ShopRequest
                    {
                        ShopId = s.ShopId,
                        ShopNameEn = s.ShopNameEn
                    }).ToList(),
                    user.Brands == null ? null : user.Brands.Select(s=> new BrandRequest
                    {
                        BrandId = s.BrandId,
                    }).ToList(),
                    User.UserRequest(),DateTime.Now);

                ClaimRequest claimRq = new ClaimRequest();

                var claimsIdentity = identity;
                claimRq.Permission = claims.Where(w => w.Type.Equals("Permission")).Select(s => new { Permission = s.Value, PermissionGroup = s.ValueType }).ToList();
                claimRq.Shop = principal.ShopRequest();
                claimRq.User = new { NameEn = User.UserRequest().NameEn, Email = User.UserRequest().Email, IsAdmin = Constant.USER_TYPE_ADMIN.Equals(User.UserRequest().Type) };

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
                string email = User.UserRequest().Email;
                User user = null;

                if (!string.IsNullOrEmpty(request.Email)&&Constant.USER_TYPE_ADMIN.Equals(User.UserRequest().Type))
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

                return Login(new UserRequest()
                {
                    Email = user.Email,
                    Password = request.NewPassword,
                    IsAdmin = Constant.USER_TYPE_ADMIN.Equals(user.Type),
                });
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
                bool IsPasswordChange = User.UserRequest().IsPasswordChange;
                var shopId = User.ShopRequest().ShopId;
                var productTotalCount = db.ProductStages.Where(w => w.ShopId == shopId).Count();
                var shopBanner = db.ShopImages.Where(w => w.ShopId == shopId).Count();
                var shopDescription = db.Shops.Where(w => w.ShopId == shopId).Select(s => s.ShopDescriptionEn).SingleOrDefault();
                var productApprove = db.ProductStages.Where(w => w.ShopId == shopId && Constant.PRODUCT_STATUS_APPROVE.Equals(w.Status)).Count();
                bool IsProduct = productTotalCount > 0 ? true : false;
                bool isBanner = shopBanner > 0 ? true : false;
                bool IsProductApprove = productApprove > 0 ? true : false;
                return Request.CreateResponse(new OnBoardRequest()
                {
                    AddProduct = IsProduct,
                    ChangePassword = IsPasswordChange,
                    ProductApprove = IsProductApprove,
                    //DecorateStore = isBanner,
                    SetUpShop = !string.IsNullOrWhiteSpace(shopDescription)
                });
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        [Route("api/Tokens/Validation")]
        [HttpGet]
        [OverrideAuthentication, OverrideAuthorization]
        public HttpResponseMessage TokensValidation()
        {
            try
            {
                var parameter = Request.Headers.Authorization.Parameter;
                if(string.IsNullOrWhiteSpace(parameter) || Cache.Get(parameter) == null)
                {
                    throw new Exception("Invalid user");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
        }

        private void SetupUser(User user, UserRequest request, ColspEntities db)
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
            #region Brand Map
            var brandMap = user.UserBrandMaps.ToList();
            if(request.Brands != null && request.Brands.Count > 0)
            {
                foreach(var brand in request.Brands)
                {
                    bool isNew = false;
                    if(brandMap == null || brandMap.Count == 0)
                    {
                        isNew = true;
                    }
                    if (!isNew)
                    {
                        var current = brandMap.Where(w => w.BrandId == brand.BrandId).SingleOrDefault();
                        if(current != null)
                        {
                            brandMap.Remove(current);
                        }
                        else
                        {
                            isNew = true;
                        }
                    }
                    if (isNew)
                    {
                        user.UserBrandMaps.Add(new UserBrandMap()
                        {
                            BrandId = brand.BrandId,
                            CreateBy = User.UserRequest().Email,
                            CreateOn = DateTime.Now,
                            UpdateBy = User.UserRequest().Email,
                            UpdateOn = DateTime.Now,
                        });
                    }
                }
            }
            if(brandMap != null && brandMap.Count > 0)
            {
                db.UserBrandMaps.RemoveRange(brandMap);
            }
            #endregion
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