using System.Collections.Generic;
using System.Globalization;

namespace ProTrans
{
    public class PropertiesFile
    {
        public CultureInfo CultureInfo { get; }
        public IReadOnlyDictionary<string, string> Dictionary { get; }

        public PropertiesFile(Dictionary<string, string> dictionary, CultureInfo cultureInfo)
        {
            this.Dictionary = dictionary;
            this.CultureInfo = cultureInfo;
        }

        public string GetValue(string key, string fallbackValue = null)
        {
            if (TryGetValue(key, out string value))
            {
                return value;
            }
            return fallbackValue;
        }

        public bool TryGetValue(string key, out string result)
        {
            return Dictionary.TryGetValue(key, out result);
        }
    }
}
