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
				// Query authenticated user
				var user = await Task.Run<User>( () =>
						(from u in db.Users
						where u.Username.Equals(username) && u.Password.Equals(password)
						select u).FirstOrDefault()
				);
				if (user == null)											
				{
					return null;
				}

				// TODO: get permissions from db somehow

				var claims = new List<Claim> { new Claim("Permission", "GetUsers") };
				var identity = new ClaimsIdentity(claims, "Basic");

				return new ClaimsPrincipal(identity);
			}
		}
	}
}