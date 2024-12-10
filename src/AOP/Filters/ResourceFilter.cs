using Microsoft.AspNetCore.Mvc.Filters;

namespace AOP.Filters
{
    public class ResourceFilter : IAsyncResourceFilter
    {
        public Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
