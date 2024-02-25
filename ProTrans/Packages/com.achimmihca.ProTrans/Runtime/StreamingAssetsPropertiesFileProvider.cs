using System.Globalization;
using System.Threading;
using System.Transactions;
using UnityEngine;

namespace ProTrans
{
    public class StreamingAssetsPropertiesFileProvider : FileSystemPropertiesFileProvider
    {
        public string PropertiesFileBaseName { get; set; } = "messages";
        public string FolderInStreamingAssets { get; set; } = "Translations";

        protected override string GetFullPropertiesFilePath(CultureInfo cultureInfo)
        {
            string languageAndRegionSuffix = PropertiesFileParser.GetLanguageAndRegionSuffix(cultureInfo);
            return $"{Application.streamingAssetsPath}/{FolderInStreamingAssets}/{PropertiesFileBaseName}{languageAndRegionSuffix}.properties";
        }
    }
}
