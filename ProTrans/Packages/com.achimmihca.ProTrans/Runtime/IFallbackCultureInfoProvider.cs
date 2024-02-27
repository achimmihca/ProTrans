using System.Globalization;

namespace ProTrans
{
    public interface IFallbackCultureInfoProvider
    {
        CultureInfo GetFallbackCultureInfo(CultureInfo cultureInfo);
    }
}
