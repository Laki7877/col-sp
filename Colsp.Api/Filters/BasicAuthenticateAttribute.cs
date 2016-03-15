using System;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using Colsp.Api.Results;
using Colsp.Api.Services;
using Colsp.Entity.Models;
using Colsp.Api.Security;
using Colsp.Model.Requests;
using Cenergy.Dazzle.Admin.Security.Cryptography;
using System.Data.Entity;
using Colsp.Api.Helpers;

namespace Colsp.Api.Filters
{
	public class BasicAuthenticateAttribute : System.Attribute, IAuthenticationFilter
	{
        private SaltedSha256PasswordHasher salt = new SaltedSha256PasswordHasher();

        public bool AllowMultiple
		{
			get
			{
				return false;
			}
		}

		public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
            try
            {
                var request = context.Request;
                var authorization = request.Headers.Authorization;

                // Check for auth
                if (authorization == null)
                {
                    return;
                }

                // Check for auth scheme
                if (authorization.Scheme != "Basic")
                {
                    context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                    return;
                }

                // Check for auth parameter
                if (String.IsNullOrEmpty(authorization.Parameter))
                {
                    context.ErrorResult = new AuthenticationFailureResult("Missing credentials", request);
                    return;
                }

                // Check for existing cache
                object cachedPrincipal = Cache.Get(authorization.Parameter);
                if (cachedPrincipal != null)
                {
                    context.Principal = (IPrincipal)cachedPrincipal;
                    return;
                }

                var userNameAndPassword = ExtractUserNameAndPassword(authorization.Parameter);
                
                // Check for parsed username password
                if (userNameAndPassword == null)
                {
                    // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                    throw new Exception("Invalid credentials");
                }

                var email = userNameAndPassword.Item1;
                var password = userNameAndPassword.Item2;

                
                // Authenticate
                IPrincipal principal = await AuthenticateAsync(email, password, cancellationToken);

                if (principal == null)
                {
                    throw new Exception("Invalid Email or Password");
                }
                else
                {
                    // Cached with encoded basic auth as key
                    Cache.Add(authorization.Parameter, principal);
                    context.Principal = principal;
                }
            }
            catch(Exception e)
            {
                context.ErrorResult = new AuthenticationFailureResult(e.Message, context.Request);
            }
			
		}
		
        // Challenge response
		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			return Task.FromResult(0);
		}

		// Convert base64 to username and password
		private Tuple<string, string> ExtractUserNameAndPassword(string authorizationParameter)
		{
			byte[] credentialBytes;

			try
			{
				credentialBytes = Convert.FromBase64String(authorizationParameter);
			}
			catch (FormatException)
			{
				return null;
			}

			Encoding encoding = Encoding.ASCII;
			encoding = (Encoding)encoding.Clone();
			encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
			string decodedCredentials;

			try
			{
				decodedCredentials = encoding.GetString(credentialBytes);
			}
			catch (DecoderFallbackException)
			{
				return null;
			}

			if (String.IsNullOrEmpty(decodedCredentials))
			{
				return null;
			}

			int colonIndex = decodedCredentials.IndexOf(':');

			if (colonIndex == -1)
			{
				return null;
			}

			string userName = decodedCredentials.Substring(0, colonIndex);
			string password = decodedCredentials.Substring(colonIndex + 1);
			return new Tuple<string, string>(userName, password);
		}

		// Authenticate with database
		private async Task<IPrincipal> AuthenticateAsync(string email, string password, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			using (var db = new ColspEntities())
			{
				// TODO: salt the password
				// Query authenticated user-permission
				var user = await Task.Run(() =>
					db.Users.Where(u => u.Email.Equals(email))
                    .Include(i=>i.UserGroupMaps
                        .Select(s=>s.UserGroup.UserGroupPermissionMaps
                        .Select(sp=>sp.Permission)))
                    .Include(i=>i.UserShopMaps.Select(s=>s.Shop))
                    .FirstOrDefault()
				);
                
                // Check for user
                if (user == null || !salt.CheckPassword(password, user.Password))
				{
                    user.LoginFailCount = user.LoginFailCount + 1;
                    Util.DeadlockRetry(db.SaveChanges, "User");
                    return null;
				}
                
                // Get all permissions
                var userPermissions = user.UserGroupMaps.Select(s=>s.UserGroup.UserGroupPermissionMaps.Select(sp=>sp.Permission));

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
                    user.Shops == null ? null : user.UserShopMaps.Select(s => new ShopRequest { ShopId = s.ShopId, ShopNameEn = s.Shop.ShopNameEn, Status = s.Shop.Status, IsShopReady = string.IsNullOrWhiteSpace(s.Shop.ShopDescriptionEn) ? false : true }).ToList(),
                    new UserRequest { UserId = user.UserId, Email = user.Email, NameEn = user.NameEn, NameTh = user.NameTh, Type = user.Type, IsPasswordChange = string.IsNullOrEmpty(user.PasswordLastChg) ? false : true });

                user.LastLoginDt = DateTime.Now;
                user.LoginFailCount = 0;
                Util.DeadlockRetry(db.SaveChanges, "User");
                return principal;
			}
		}

    }
}