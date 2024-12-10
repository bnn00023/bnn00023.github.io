using Microsoft.AspNetCore.Mvc.Filters;

namespace AOP.Filters
{
    public class AuthFilter : IAsyncAuthorizationFilter
    {
        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            throw new NotImplementedException();
        }
    }
}
