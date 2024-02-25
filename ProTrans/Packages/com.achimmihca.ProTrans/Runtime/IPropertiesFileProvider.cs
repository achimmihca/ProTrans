using System.Globalization;

namespace ProTrans
{
    public interface IPropertiesFileProvider
    {
        PropertiesFile GetPropertiesFile(CultureInfo cultureInfo);
    }
}
