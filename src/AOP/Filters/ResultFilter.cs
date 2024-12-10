using Microsoft.AspNetCore.Mvc.Filters;

namespace AOP.Filters
{
    public class ResultFilter : IAsyncResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
