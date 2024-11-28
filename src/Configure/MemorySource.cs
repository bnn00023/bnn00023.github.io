
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

        internal void Chnage(string text)
        {
            _provider.Chnage(text);
        }
    }
}
