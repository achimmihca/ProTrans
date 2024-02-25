using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ProTrans
{
    public static class Translation
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticInit()
        {
        }

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

    }
}
