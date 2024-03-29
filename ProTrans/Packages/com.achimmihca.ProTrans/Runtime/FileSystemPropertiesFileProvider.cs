using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ProTrans
{
    public abstract class FileSystemPropertiesFileProvider : IPropertiesFileProvider
    {
        private Dictionary<CultureInfo, PropertiesFile> cultureInfoToPropertiesFileCache = new();
        private PropertiesFile basePropertiesFileCache;

        protected abstract string GetFullPropertiesFilePath(CultureInfo cultureInfo);

        private PropertiesFile GetBasePropertiesFile()
        {
            if (basePropertiesFileCache != null)
            {
                return basePropertiesFileCache;
            }

            string path = GetFullPropertiesFilePath(null);
            if (!File.Exists(path))
            {
                return null;
            }
            PropertiesFile propertiesFile = PropertiesFileParser.ParseFile(path, TranslationConfig.Singleton.DefaultCultureInfo);

            basePropertiesFileCache = propertiesFile;
            return propertiesFile;
        }

        public PropertiesFile GetPropertiesFile(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                return GetBasePropertiesFile();
            }

            if (cultureInfoToPropertiesFileCache.TryGetValue(cultureInfo, out PropertiesFile cachedPropertiesFile))
            {
                return cachedPropertiesFile;
            }

            string path = GetFullPropertiesFilePath(cultureInfo);
            if (!File.Exists(path))
            {
                return null;
            }

            PropertiesFile propertiesFile = PropertiesFileParser.ParseFile(path, TranslationConfig.Singleton.DefaultCultureInfo);

            cultureInfoToPropertiesFileCache[cultureInfo] = propertiesFile;
            return propertiesFile;
        }

        public void ClearCache()
        {
            basePropertiesFileCache = null;
            cultureInfoToPropertiesFileCache = new ();
        }
    }
}
