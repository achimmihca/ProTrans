using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ProTrans
{
    public static class StreamingAssetsTranslationResourceDiscovery
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticInit()
        {
            ClearCache();
        }
        private static Dictionary<string, PropertiesFile> pathToPropertiesFileCache = new();
        private static PropertiesFile basePropertiesFileCache;

        public static string PropertiesFileBaseName { get; set; } = "messages";
        public static string FolderInStreamingAssets { get; set; } = "Translations";

        public static PropertiesFile GetBasePropertiesFile()
        {
            if (basePropertiesFileCache == null)
            {
                string path = GetFullPathInStreamingAssets(null);
                basePropertiesFileCache = PropertiesFileParser.ParseFile(path);
            }
            return basePropertiesFileCache;
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
            pathToPropertiesFileCache = new();
        }

        private static string GetFullPathInStreamingAssets(CultureInfo cultureInfo)
        {
            string languageAndRegionSuffix = PropertiesFileParser.GetLanguageAndRegionSuffix(cultureInfo);
            return $"{Application.streamingAssetsPath}/{FolderInStreamingAssets}/{PropertiesFileBaseName}{languageAndRegionSuffix}";
        }
    }
}
