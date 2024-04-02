using System.Collections.Generic;
using System.Globalization;

namespace ProTrans
{
    public class CachingPropertiesFileProvider : IPropertiesFileProvider
    {
        private readonly IPropertiesFileProvider uncachedPropertiesFileProvider;
        private readonly Dictionary<CultureInfo, PropertiesFile> cultureInfoToPropertiesFileCache = new();
        private PropertiesFile defaultCultureInfoPropertiesFile;

        public CachingPropertiesFileProvider(IPropertiesFileProvider uncachedPropertiesFileProvider)
        {
            this.uncachedPropertiesFileProvider = uncachedPropertiesFileProvider;
        }

        public void ClearCache()
        {
            cultureInfoToPropertiesFileCache.Clear();
        }

        public PropertiesFile GetPropertiesFile(CultureInfo cultureInfo)
        {
            // Try get cached value
            if (cultureInfo == null)
            {
                if (defaultCultureInfoPropertiesFile != null)
                {
                    return defaultCultureInfoPropertiesFile;
                }
            }
            else if (cultureInfoToPropertiesFileCache.TryGetValue(cultureInfo, out PropertiesFile cachedPropertiesFile))
            {
                return cachedPropertiesFile;
            }

            // Get uncached value
            PropertiesFile propertiesFile = uncachedPropertiesFileProvider.GetPropertiesFile(cultureInfo);

            // Store in cache
            if (cultureInfo == null)
            {
                defaultCultureInfoPropertiesFile = propertiesFile;
            }
            else
            {
                cultureInfoToPropertiesFileCache[cultureInfo] = propertiesFile;
            }
            return propertiesFile;
        }
    }
}
