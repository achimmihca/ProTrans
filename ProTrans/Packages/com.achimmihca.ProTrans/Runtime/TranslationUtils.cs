using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProTrans
{
    internal static class TranslationUtils
    {
        internal static List<CultureInfo> GetTranslatedCultureInfos()
        {
            List<CultureInfo> result = new List<CultureInfo>();

            PropertiesFile defaultPropertiesFile = GetPropertiesFile(null);
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
                    PropertiesFile propertiesFile = GetPropertiesFile(cultureInfo);
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
            PropertiesFile propertiesFile = GetPropertiesFile(cultureInfo);
            if (propertiesFile != null
                && propertiesFile.TryGetValue(key, out translation))
            {
                translation = ReplacePlaceholders(key, translation, placeholders);
                CheckMissingPlaceholders(key, translation);
                return true;
            }

            if (cultureInfo != null)
            {
                CultureInfo fallbackCultureInfo = GetFallbackCultureInfo(cultureInfo);
                if (!Equals(fallbackCultureInfo, cultureInfo))
                {
                    return TryGet(fallbackCultureInfo, key, placeholders, out translation);
                }
            }

            translation = key;
            return false;
        }

        private static void CheckMissingPlaceholders(string key, string translation)
        {
            if (TranslationConfig.Singleton.MissingPlaceholderStrategy is not MissingPlaceholderStrategy.Ignore)
            {
                MatchCollection matches = Regex.Matches(translation, @"\{(?<placeholder>\w+)\}");
                foreach (Match match in matches)
                {
                    HandleMissingPlaceholder(key, match.Groups["placeholder"].Value);
                }
            }
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
                string placeholderValue = placeholderStrings[i + 1]?.ToString();
                placeholders[placeholderKey] = placeholderValue;
            }
            return placeholders;
        }

        private static string ReplacePlaceholders(string key, string translation, Dictionary<string,string> placeholders)
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
                else
                {
                    HandleUnexpectedPlaceholder(key, translation, placeholder.Key);
                }
            }

            return translationWithoutPlaceholders;
        }

        private static PropertiesFile GetPropertiesFile(CultureInfo cultureInfo)
        {
            return TranslationConfig.Singleton.PropertiesFileProvider.GetPropertiesFile(cultureInfo);
        }

        private static CultureInfo GetFallbackCultureInfo(CultureInfo cultureInfo)
        {
            return TranslationConfig.Singleton.FallbackCultureInfoProvider.GetFallbackCultureInfo(cultureInfo);
        }

        private static void HandleUnexpectedPlaceholder(string key, string translation, string placeholderKey)
        {
            switch (TranslationConfig.Singleton.UnexpectedPlaceholderStrategy)
            {
                case UnexpectedPlaceholderStrategy.Ignore:
                    return;
                case UnexpectedPlaceholderStrategy.Log:
                    LogUtils.Log(LogLevel.Warning, () => $"Unexpected placeholder '{placeholderKey}' for translation of '{key}'");
                    return;
                case UnexpectedPlaceholderStrategy.Throw:
                    throw new TranslationException($"Unexpected placeholder '{placeholderKey}' for translation of '{key}'");
            }
        }

        private static void HandleMissingPlaceholder(string key, string placeholderKey)
        {
            switch (TranslationConfig.Singleton.MissingPlaceholderStrategy)
            {
                case MissingPlaceholderStrategy.Ignore:
                    return;
                case MissingPlaceholderStrategy.Log:
                    LogUtils.Log(LogLevel.Warning, () => $"Missing placeholder '{placeholderKey}' for translation of '{key}'");
                    return;
                case MissingPlaceholderStrategy.Throw:
                    throw new TranslationException($"Missing placeholder '{placeholderKey}' for translation of '{key}'");
            }
        }
    }
}
