using System.Collections.Generic;
using System.Globalization;

namespace ProTrans
{
    internal static class TranslationUtils
    {
        internal static List<CultureInfo> GetTranslatedCultureInfos()
        {
            return new List<CultureInfo>();
        }

        internal static string Get(string key)
        {
            return key;
        }

        internal static string Get(string key, params object[] placeholderStrings)
        {
            Dictionary<string, string> placeholders = CreatePlaceholderDictionary(key, placeholderStrings);
            return Get(key, placeholders);
        }

        internal static string Get(string key, Dictionary<string, string> placeholders)
        {
            if (!TryGet(key, placeholders, out string translation)
                || placeholders == null)
            {
                // No proper translation found or no placeholders that should be replaced.
                return translation;
            }

            return ReplacePlaceholders(translation, placeholders);
        }

        internal static bool TryGet(string key, Dictionary<string,string> placeholders, out string translation)
        {
            translation = "";
            return false;
        }

        private static Dictionary<string,string> CreatePlaceholderDictionary(string key, params object[] placeholderStrings)
        {
            if (placeholderStrings == null
                || placeholderStrings.Length == 0)
            {
                return null;
            }

            if (placeholderStrings.Length % 2 != 0)
            {
                LogUtils.Log(LogLevel.Warning, () => $"Uneven number of placeholders for '{key}'. Format in array should be [key1, value1, key2, value2, ...]");
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
