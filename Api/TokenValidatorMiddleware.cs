using Api.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Api
{
    public class TokenValidatorMiddleware
    {
        private readonly RequestDelegate _next;
        public TokenValidatorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserService service)
        {
            var sessionIdString = context.User.Claims.FirstOrDefault(x => x.Type == "sessionId")?.Value;
            var isOk = true;
            if(Guid.TryParse(sessionIdString, out var sessionId))
            {
                var session = await service.GetSessinById(sessionId);
                if(!session.IsActive)
                {
                    isOk = false;
                    context.Response.Clear();
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Session is not active");
                }
            }
            if(isOk)
            {
                await _next(context);
            }


        }
    }
    public static class TokenValidatorMiddlewareExstensions
    {
        public static IApplicationBuilder UseTokenValidator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenValidatorMiddleware>();
        }
    }
}
