﻿using System;
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

namespace Colsp.Api.Filters
{
	public class BasicAuthenticateAttribute : System.Attribute, IAuthenticationFilter
	{
		public bool AllowMultiple
		{
			get
			{
				return false;
			}
		}

		public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
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
				context.ErrorResult = new AuthenticationFailureResult("Invalid credentials", request);
				return;
			}

			var userName = userNameAndPassword.Item1;
			var password = userNameAndPassword.Item2;

			// Authenticate
			IPrincipal principal = await AuthenticateAsync(userName, password, cancellationToken);

			if (principal == null)
			{
				context.ErrorResult = new AuthenticationFailureResult("Invalid username or password", request);
			}
			else
			{
				// Cached with encoded basic auth as key
				Cache.Add(authorization.Parameter, principal);
				context.Principal = principal;
			}
		}
		// Challenge response
		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			return Task.FromResult(0);
		}

		// Convert base64 to username and password
		private static Tuple<string, string> ExtractUserNameAndPassword(string authorizationParameter)
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
		private async Task<IPrincipal> AuthenticateAsync(string username, string password, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			using (var db = new ColspEntities())
			{
				// TODO: salt the password
				// Query authenticated user-permission
				var userPermissions = await Task.Run(() =>
				   db.Users.Join(db.UserGroupMaps, u => u.UserId, g => g.GroupId, (u, g) => new { User = u, UserGroupMap = g })
							.Join(db.UserGroups, a => a.UserGroupMap.GroupId, g => g.GroupId, (a, g) => new { User = a.User, UserGroup = g })
							.Join(db.UserGroupPermissionMaps, a => a.UserGroup.GroupId, m => m.GroupId, (a, m) => new { User = a.User, UserGroupPermissionMap = m })
							.Join(db.UserPermissions, a => a.UserGroupPermissionMap.PermissionId, p => p.PermissionId, (a, p) => new { User = a.User, Permission = p })
							.Where(u => u.User.Username.Equals(username) && u.User.Password.Equals(password))
							.Select(a => new
							{
								Username = a.User.Username,
								Permission = a.Permission.PermissionName
							})
							.ToList()
				);

				if (userPermissions.Count <= 0)											
				{
					return null;
				}

				
				// Assign claims
				var claims = new List<Claim>();
				foreach (var item in userPermissions)
				{
					if (!claims.Exists(m => m.Value.Equals(item.Permission)))
					{
						claims.Add(new Claim("Permission", item.Permission));
					}
				}
				var identity = new ClaimsIdentity(claims, "Basic");

				return new ClaimsPrincipal(identity);
			}
		}
	}
}