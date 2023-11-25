using T3mmyStoreApi.Middlewares;

namespace T3mmyStoreApi.Services
{
        public static class SwaggerAuthorizeExtensions
        {
            public static IApplicationBuilder UseSwaggerAuthorized(this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<SwaggerBasicAuthMiddleware>();
            }
        }
    
}
