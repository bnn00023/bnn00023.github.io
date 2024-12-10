namespace AOP.Services
{
    public class BarService : IBarService
    {
        public Task DoSomeThing(string text)
        {
            return Task.CompletedTask;
        }
    }
}
