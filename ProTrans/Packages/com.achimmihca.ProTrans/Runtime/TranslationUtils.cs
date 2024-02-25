using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ProTrans
{
    internal static class TranslationUtils
    {
        internal static string Get(string key)
        {
            return key;
        }

        internal static List<CultureInfo> GetTranslatedLanguages()
        {
            return new List<CultureInfo>();
        }

        internal static string Get(string key, params object[] placeholderStrings)
        {
            if (placeholderStrings == null
                || placeholderStrings.Length == 0)
            {
                TryGet(key, null, out string translation);
                return translation;
            }

            if (placeholderStrings.Length % 2 != 0)
            {
                Debug.LogWarning($"Uneven number of placeholders for '{key}'. Format in array should be [key1, value1, key2, value2, ...]");
            }

            // Create dictionary from placeholderStrings
            Dictionary<string, string> placeholders = new Dictionary<string, string>();
            for (int i = 0; i < placeholderStrings.Length && i + 1 < placeholderStrings.Length; i += 2)
            {
                string placeholderKey = placeholderStrings[i].ToString();
                string placeholderValue = placeholderStrings[i + 1].ToString();
                placeholders[placeholderKey] = placeholderValue;
            }
            return GetTranslation(key, placeholders);
        }

        internal static string GetTranslation(string key, Dictionary<string, string> placeholders)
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

        private static string ReplacePlaceholders(string translation, Dictionary<string,string> placeholders)
        {
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
