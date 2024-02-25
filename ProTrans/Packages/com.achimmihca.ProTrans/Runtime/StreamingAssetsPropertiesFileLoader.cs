using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Transactions;
using UnityEngine;

namespace ProTrans
{
    public static class StreamingAssetsPropertiesFileLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticInit()
        {
            ClearCache();
        }
        private static Dictionary<CultureInfo, PropertiesFile> cultureInfoToPropertiesFileCache;
        private static PropertiesFile basePropertiesFileCache;

        public static string PropertiesFileBaseName { get; set; } = "messages";
        public static string FolderInStreamingAssets { get; set; } = "Translations";

        private static PropertiesFile GetBasePropertiesFile()
        {
            if (basePropertiesFileCache != null)
            {
                return basePropertiesFileCache;
            }

            string path = GetFullPathInStreamingAssets(null);
            if (!File.Exists(path))
            {
                throw new TransactionException($"No base properties file found: {path}");
            }
            PropertiesFile propertiesFile = PropertiesFileParser.ParseFile(path, TranslationConfig.Singleton.DefaultCultureInfo);

            basePropertiesFileCache = propertiesFile;
            return propertiesFile;
        }

        public static PropertiesFile GetPropertiesFile(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                return GetBasePropertiesFile();
            }

            if (cultureInfoToPropertiesFileCache.TryGetValue(cultureInfo, out PropertiesFile cachedPropertiesFile))
            {
                return cachedPropertiesFile;
            }

            string path = GetFullPathInStreamingAssets(cultureInfo);
            if (!File.Exists(path))
            {
                return null;
            }

            PropertiesFile propertiesFile = PropertiesFileParser.ParseFile(path, TranslationConfig.Singleton.DefaultCultureInfo);

            cultureInfoToPropertiesFileCache[cultureInfo] = propertiesFile;
            return propertiesFile;
        }

        public static CultureInfo GetFallbackCultureInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo.IsNeutralCulture)
            {
                return null;
            }

            // Remove the region, only keep the language
            return new CultureInfo(cultureInfo.TwoLetterISOLanguageName);
        }

        public static void ClearCache()
        {
            basePropertiesFileCache = null;
            cultureInfoToPropertiesFileCache = new ();
        }

        private static string GetFullPathInStreamingAssets(CultureInfo cultureInfo)
        {
            string languageAndRegionSuffix = PropertiesFileParser.GetLanguageAndRegionSuffix(cultureInfo);
            return $"{Application.streamingAssetsPath}/{FolderInStreamingAssets}/{PropertiesFileBaseName}{languageAndRegionSuffix}.properties";
        }
    }
}
