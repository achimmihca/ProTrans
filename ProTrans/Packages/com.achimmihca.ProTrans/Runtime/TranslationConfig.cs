using System;
using System.Globalization;

namespace ProTrans
{
    public class TranslationConfig
    {
        public static TranslationConfig Singleton { get; set; } = new();

        public LogLevel minimumLogLevel = LogLevel.Warning;
        public Func<CultureInfo, PropertiesFile> propertiesFileGetter = StreamingAssetsPropertiesFileLoader.GetPropertiesFile;
    }
}
