using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ProTrans
{
    public static class Translation
    {
        public static string Get(string key, params object[] placeholderStrings)
        {
            return TranslationUtils.Get(key, placeholderStrings);
        }

        public static string Get(string key, Dictionary<string, string> placeholders)
        {
            return TranslationUtils.Get(key, placeholders);
        }

        public static bool TryGet(string key, Dictionary<string, string> placeholders)
        {
            return TranslationUtils.TryGet(TranslationConfig.Singleton.CurrentCultureInfo, key, placeholders, out string translation);
        }

        public static List<CultureInfo> GetTranslatedCultureInfos()
        {
            return TranslationUtils.GetTranslatedCultureInfos();
        }

        public static PropertiesFile GetPropertiesFile(CultureInfo cultureInfo)
        {
            return TranslationUtils.GetPropertiesFile(cultureInfo);
        }

        public static CultureInfo GetFallbackCultureInfo(CultureInfo cultureInfo = null)
        {
            return TranslationUtils.GetFallbackCultureInfo(cultureInfo);
        }
    }
}
