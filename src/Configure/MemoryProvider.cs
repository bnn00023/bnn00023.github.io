using System.Collections;

namespace Configure
{
    public class MemoryProvider : ConfigurationProvider, IEnumerable<KeyValuePair<string, string?>>
    {
        public MemoryProvider()
        {
            Data = new Dictionary<string, string?>() 
            {
                ["TestOption:Text"] = " Hello MemoryProvider"
            };
        }
        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        internal void Change(string text)
        {
            Data["TestOption:Text"] = text;
            OnReload();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
