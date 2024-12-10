using Microsoft.AspNetCore.Mvc.Filters;

namespace AOP.Filters
{
    public class ExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
