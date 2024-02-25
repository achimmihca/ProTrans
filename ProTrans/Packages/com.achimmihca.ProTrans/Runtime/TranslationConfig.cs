using System;
using System.Globalization;

namespace ProTrans
{
    public class TranslationConfig
    {
        public static TranslationConfig Singleton { get; set; } = new();

        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Warning;
        public CultureInfo DefaultCultureInfo { get; set; } = new CultureInfo("en");
        public CultureInfo CurrentCultureInfo { get; set; } = new CultureInfo("en");
        public Func<CultureInfo, PropertiesFile> PropertiesFileGetter { get; set; } = StreamingAssetsPropertiesFileLoader.GetPropertiesFile;
        public Func<CultureInfo, CultureInfo> FallbackCultureInfoGetter { get; set; } = StreamingAssetsPropertiesFileLoader.GetFallbackCultureInfo;
    }
}
