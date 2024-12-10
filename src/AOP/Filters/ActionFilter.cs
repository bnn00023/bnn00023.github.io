using Microsoft.AspNetCore.Mvc.Filters;

namespace AOP.Filters
{
    public class ActionFilter : IAsyncActionFilter
    {
        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
