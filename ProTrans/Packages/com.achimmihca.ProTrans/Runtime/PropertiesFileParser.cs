using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ProTrans
{
    public static class PropertiesFileParser
    {
        private static readonly KeyValuePair<string, string> emptyKeyValuePair = new KeyValuePair<string, string>("", "");

        public static PropertiesFile ParseFile(string path, CultureInfo defaultCultureInfo)
        {
            if (path == null
                || !File.Exists(path))
            {
                throw new FileNotFoundException("Properties file does not exist: " + path);
            }

            TryGetLanguageAndRegion(Path.GetFileNameWithoutExtension(path), out string language, out string region);

            string text = File.ReadAllText(path, Encoding.UTF8);
            return ParseText(text, GetCultureInfo(language, region, defaultCultureInfo));
        }

        public static PropertiesFile ParseText(string text, CultureInfo cultureInfo)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            using (StringReader stringReader = new StringReader(text))
            {
                for (string line = stringReader.ReadLine(); line != null; line = stringReader.ReadLine())
                {
                    KeyValuePair<string, string> entry = ParseLine(line, stringReader);
                    if (!entry.Equals(emptyKeyValuePair))
                    {
                        map.Add(entry.Key, entry.Value);
                    }
                }
            }
            return new PropertiesFile(map, cultureInfo);
        }

        public static bool TryGetLanguageAndRegion(string fileNameWithoutException, out string language, out string region)
        {
            language = "";
            region = "";

            int indexOfUnderscore = fileNameWithoutException.IndexOf("_", StringComparison.InvariantCultureIgnoreCase);
            if (indexOfUnderscore < 0)
            {
                return true;
            }
            string languageAndRegionSuffix = fileNameWithoutException.Substring(indexOfUnderscore + 1);
            string[] languageAndRegionArray = languageAndRegionSuffix.Split("_");
            if (languageAndRegionArray.Length <= 0)
            {
                return true;
            }

            language = languageAndRegionArray[0];
            if (languageAndRegionArray.Length > 1)
            {
                region = languageAndRegionArray[1];
            }

            if (language.Length != 2
                || (region.Length > 0 && region.Length != 2))
            {
                throw new TranslationException($"Expected properties file suffix with two letter language and region codes separated by underscore but got '{languageAndRegionSuffix}' (language: {language}, region: {region})");
            }

            return false;
        }

        public static string GetLanguageAndRegionSuffix(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
            {
                return "";
            }

            string language = cultureInfo.TwoLetterISOLanguageName;
            string languageAndRegionSuffix = $"_{language.ToLowerInvariant()}";

            if (!cultureInfo.IsNeutralCulture)
            {
                RegionInfo regionInfo = new RegionInfo(cultureInfo.ToString());
                string region = regionInfo.TwoLetterISORegionName;
                languageAndRegionSuffix += $"_{region.ToUpperInvariant()}";
            }

            return languageAndRegionSuffix;
        }

        private static CultureInfo GetCultureInfo(string language, string region, CultureInfo defaultCultureInfo)
        {
            string languageLower = language?.ToLowerInvariant();
            string regionUpper = region?.ToUpperInvariant();

            if (!string.IsNullOrEmpty(languageLower)
                && !string.IsNullOrEmpty(regionUpper))
            {
                return new CultureInfo($"{languageLower}-{regionUpper}");
            }
            else if (!string.IsNullOrEmpty(languageLower))
            {
                return new CultureInfo($"{languageLower}");
            }
            else
            {
                return defaultCultureInfo;
            }
        }

        private static KeyValuePair<string, string> ParseLine(string line, StringReader stringReader)
        {
            if (IsComment(line))
            {
                return emptyKeyValuePair;
            }

            int indexOfSeparator = FindIndexOfSeparator(line);
            if (indexOfSeparator < 0)
            {
                return emptyKeyValuePair;
            }
            string keyPart = line.Substring(0, indexOfSeparator);
            string valuePart = line.Substring(indexOfSeparator + 1, line.Length - indexOfSeparator - 1);

            string trimmedKeyPart = keyPart.Trim();
            // For the value, only the start (around the equals sign) is trimmed.
            string trimmedValuePart = valuePart.TrimStart();

            // Write characters of line to StringBuilder.
            // Thereby, replace escaped characters.
            // When line ends with a backslash, also consume the following line.
            StringBuilder sb = new StringBuilder();
            ParseEscapedCharacters(trimmedValuePart, sb, stringReader);

            return new KeyValuePair<string, string>(trimmedKeyPart, sb.ToString());
        }

        private static void ParseEscapedCharacters(string line, StringBuilder sb, StringReader stringReader)
        {
            bool isBackslashForFollowingCharacter = false;
            foreach (char c in line)
            {
                if (isBackslashForFollowingCharacter)
                {
                    isBackslashForFollowingCharacter = false;
                    switch (c)
                    {
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
                else if (c == '\\')
                {
                    isBackslashForFollowingCharacter = true;
                }
                else
                {
                    sb.Append(c);
                }
            }

            // Line has ended. Check if it continues on next line.
            if (isBackslashForFollowingCharacter)
            {
                string nextLine = stringReader.ReadLine();
                if (nextLine != null)
                {
                    ParseEscapedCharacters(nextLine.TrimStart(), sb, stringReader);
                }
            }
        }

        private static int FindIndexOfSeparator(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                switch (line[i])
                {
                    case '=':
                    case ':': return i;
                }
            }

            return -1;
        }

        private static bool IsComment(string line)
        {
            string lineTrimmedStart = line.TrimStart();
            return lineTrimmedStart.StartsWith("#")
                   || lineTrimmedStart.StartsWith("!");
        }
    }
}
