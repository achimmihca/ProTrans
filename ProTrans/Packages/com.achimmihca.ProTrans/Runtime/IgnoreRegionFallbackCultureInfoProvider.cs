using System.Globalization;

namespace ProTrans
{
    public class IgnoreRegionFallbackCultureInfoProvider : IFallbackCultureInfoProvider
    {
        public CultureInfo GetFallbackCultureInfo(CultureInfo cultureInfo)
        {
            if (cultureInfo == null
                || cultureInfo.IsNeutralCulture)
            {
                return null;
            }

            // Remove the region, only keep the language
            return new CultureInfo(cultureInfo.TwoLetterISOLanguageName);
        }
    }
}
