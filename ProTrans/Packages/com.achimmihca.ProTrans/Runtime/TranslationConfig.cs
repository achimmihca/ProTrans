using System;
using System.Globalization;

namespace ProTrans
{
    public class TranslationConfig
    {
        public static TranslationConfig Singleton { get; set; } = new();

        public LogLevel minimumLogLevel = LogLevel.Warning;
        public Func<PropertiesFile> basePropertiesFileGetter = StreamingAssetsTranslationResourceDiscovery.GetBasePropertiesFile;
        public Func<CultureInfo, CultureInfo> fallbackCultureInfoGetter = StreamingAssetsTranslationResourceDiscovery.GetFallbackCultureInfo;
    }
}
