using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Colsp.Api.Results;
using Colsp.Api.Services;
using Colsp.Api.Constants;
using Colsp.Api.Security;
using System;
using System.Net.Http;
using System.Net;

namespace Colsp.Api.Filters
{
    public class BasicAuthenticateAttribute : System.Attribute, IAuthenticationFilter
    {
        //private SaltedSha256PasswordHasher salt = new SaltedSha256PasswordHasher();

        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        public  Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var authHeader = context.Request.Headers.Authorization;
            if (authHeader == null || !Constant.AUTHEN_SCHEMA.Equals(authHeader.Scheme))
            {
                var failResult = new AuthenticationFailureResult("Missing credentials", context.Request);
                return failResult.ExecuteAsync(cancellationToken);
            }

            object cachedPrincipal = Cache.Get(authHeader.Parameter);
            if(cachedPrincipal is UsersPrincipal)
            {
                var principal = (UsersPrincipal)cachedPrincipal;
                if((DateTime.Now - principal.LoginDt).TotalHours > Constant.CACHE_TIMEOUT)
                {
                    Cache.Delete(authHeader.Parameter);
                    return Task.FromResult(context.Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                else
                {
                    principal.LoginDt = DateTime.Now;
                }
            }
            if(cachedPrincipal == null)
            {
                var failResult = new AuthenticationFailureResult("Missing credentials", context.Request);
                return failResult.ExecuteAsync(cancellationToken);
            }
            context.Principal = (IPrincipal)cachedPrincipal;
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}