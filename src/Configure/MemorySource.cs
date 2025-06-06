
namespace Configure
{
    public class MemorySource : IConfigurationSource
    {
        private MemoryProvider _provider;
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            _provider = new MemoryProvider();
            return _provider;
        }

        internal void Change(string text)
        {
            _provider.Change(text);
        }
    }
}
