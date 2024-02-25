using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace ProTrans
{
    internal static class TranslationUtils
    {
        internal static List<CultureInfo> GetTranslatedCultureInfos()
        {
            List<CultureInfo> result = new List<CultureInfo>();

            PropertiesFile defaultPropertiesFile = TranslationConfig.Singleton.PropertiesFileGetter(null);
            if (defaultPropertiesFile != null)
            {
                result.Add(TranslationConfig.Singleton.DefaultCultureInfo);
            }

            CultureInfo[] allCultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures);
            foreach (CultureInfo cultureInfo in allCultureInfos)
            {
                if (string.IsNullOrEmpty(cultureInfo.ToString()))
                {
                    continue;
                }

                try
                {
                    PropertiesFile propertiesFile = TranslationConfig.Singleton.PropertiesFileGetter(cultureInfo);
                    if (propertiesFile != null)
                    {
                        result.Add(cultureInfo);
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.Log(LogLevel.Error, () => $"Failed to load properties file for CultureInfo {cultureInfo.ToString()}: {ex.Message}", ex);
                }
            }

            return result;
        }

        internal static string Get(string key, params object[] placeholderStrings)
        {
            Dictionary<string, string> placeholders = CreatePlaceholderDictionary(key, placeholderStrings);
            return Get(key, placeholders);
        }

        internal static string Get(string key, Dictionary<string, string> placeholders)
        {
            TryGet(TranslationConfig.Singleton.CurrentCultureInfo, key, placeholders, out string translation);
            return translation;
        }

        internal static bool TryGet(CultureInfo cultureInfo, string key, Dictionary<string,string> placeholders, out string translation)
        {
            PropertiesFile propertiesFile = TranslationConfig.Singleton.PropertiesFileGetter(cultureInfo);
            if (propertiesFile != null
                && propertiesFile.TryGetValue(key, out translation))
            {
                translation = ReplacePlaceholders(translation, placeholders);
                return true;
            }

            if (cultureInfo != null)
            {
                CultureInfo fallbackCultureInfo = TranslationConfig.Singleton.FallbackCultureInfoGetter(cultureInfo);
                if (!Equals(fallbackCultureInfo, cultureInfo))
                {
                    return TryGet(fallbackCultureInfo, key, placeholders, out translation);
                }
            }

            translation = key;
            return false;
        }

        private static Dictionary<string,string> CreatePlaceholderDictionary(string key, params object[] placeholderStrings)
        {
            if (placeholderStrings == null
                || placeholderStrings.Length == 0)
            {
                return null;
            }

            if (placeholderStrings.Length == 1
                && placeholderStrings[0] is IList list)
            {
                if (list.Count <= 0)
                {
                    return null;
                }

                List<object> objects = new List<object>(list.Count);
                foreach (object o in list)
                {
                    objects.Add(o);
                }
                return CreatePlaceholderDictionary(key, objects.ToArray());
            }

            if (placeholderStrings.Length % 2 != 0)
            {
                LogUtils.Log(LogLevel.Warning, () => $"Uneven number of placeholders for key '{key}'. Format in array should be [key1, value1, key2, value2, ...]");
            }

            Dictionary<string, string> placeholders = new Dictionary<string, string>();
            for (int i = 0; i < placeholderStrings.Length && i + 1 < placeholderStrings.Length; i += 2)
            {
                string placeholderKey = placeholderStrings[i].ToString();
                string placeholderValue = placeholderStrings[i + 1].ToString();
                placeholders[placeholderKey] = placeholderValue;
            }
            return placeholders;
        }

        private static string ReplacePlaceholders(string translation, Dictionary<string,string> placeholders)
        {
            if (placeholders == null
                || placeholders.Count == 0)
            {
                return translation;
            }

            string translationWithoutPlaceholders = translation;
            foreach (KeyValuePair<string, string> placeholder in placeholders)
            {
                string placeholderText = "{" + placeholder.Key + "}";
                if (translationWithoutPlaceholders.Contains(placeholderText))
                {
                    translationWithoutPlaceholders = translationWithoutPlaceholders.Replace(placeholderText, placeholder.Value);
                }
            }
            return translationWithoutPlaceholders;
        }
    }
}
