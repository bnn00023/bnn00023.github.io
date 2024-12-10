
namespace AOP.Services
{
    public class BarValidator : IBarService
    {
        private readonly IBarService _barService;

        public BarValidator(IBarService barService)
        {
            _barService = barService;
        }

        public Task DoSomeThing(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            return _barService.DoSomeThing(text);
        }
    }
}
