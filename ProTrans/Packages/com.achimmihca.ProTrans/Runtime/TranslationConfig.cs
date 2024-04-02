using System;
using System.Globalization;
using UnityEngine;

namespace ProTrans
{
    public class TranslationConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void StaticInit()
        {
            Singleton = new();
        }

        public static TranslationConfig Singleton { get; set; } = new();

        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Warning;
        public CultureInfo DefaultCultureInfo { get; set; } = new CultureInfo("en");
        public CultureInfo CurrentCultureInfo { get; set; } = new CultureInfo("en");
        public MissingPlaceholderStrategy MissingPlaceholderStrategy { get; set; } = MissingPlaceholderStrategy.Throw;
        public UnexpectedPlaceholderStrategy UnexpectedPlaceholderStrategy { get; set; } = UnexpectedPlaceholderStrategy.Throw;
        public IPropertiesFileProvider PropertiesFileProvider { get; set; } = new CachingPropertiesFileProvider(new StreamingAssetsPropertiesFileProvider());
        public IFallbackCultureInfoProvider FallbackCultureInfoProvider { get; set; } = new IgnoreRegionFallbackCultureInfoProvider();
    }
}
