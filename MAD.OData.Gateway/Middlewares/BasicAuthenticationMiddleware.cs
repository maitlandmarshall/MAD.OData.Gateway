using System.Net;
using System.Text;

namespace MAD.OData.Gateway.Middlewares
{
    public class BasicAuthenticationMiddleware : IMiddleware
    {
        private readonly AuthConfig authConfig;

        public BasicAuthenticationMiddleware(AuthConfig authConfig)
        {
            this.authConfig = authConfig;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (string.IsNullOrWhiteSpace(this.authConfig.Username) == false
                && string.IsNullOrWhiteSpace(this.authConfig.Password) == false)
            {
                if (this.IsAuthenticated(context) == false)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.Headers["WWW-Authenticate"] = "Basic";

                    return;
                }
            }

            await next(context);
        }

        private bool IsAuthenticated(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authHeader))
                return false;

            if (authHeader.StartsWith("Basic") == false)
                return false;

            var base64 = authHeader.Substring("Basic ".Length);
            var usernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var usernamePasswordSplit = usernamePassword.Split(":");

            return usernamePasswordSplit[0] == this.authConfig.Username && usernamePasswordSplit[1] == this.authConfig.Password;
        }
    }
}
