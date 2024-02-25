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

        public static PropertiesFile ParseFile(string path)
        {
            if (!TryParseLanguageAndRegion(Path.GetFileName(path), out string language, out string region))
            {
                throw new TranslationException("Properties file name is missing language suffix.");
            }
            string languageLower = language.ToLowerInvariant();
            string regionUpper = region.ToUpperInvariant();

            string text = File.ReadAllText(path, Encoding.UTF8);
            return ParseText(text, new CultureInfo($"{languageLower}-{regionUpper}"));
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

        private static KeyValuePair<string, string> ParseLine(string line, StringReader stringReader)
        {
            string lineTimmedStart = line.TrimStart();
            bool isComment = lineTimmedStart.StartsWith("#")
                             || lineTimmedStart.StartsWith("!");
            if (isComment)
            {
                return emptyKeyValuePair;
            }

            int indexOfEquals = line.IndexOf("=", StringComparison.InvariantCulture);
            if (indexOfEquals < 0)
            {
                return emptyKeyValuePair;
            }
            string keyPart = line.Substring(0, indexOfEquals);
            string valuePart = line.Substring(indexOfEquals + 1, line.Length - indexOfEquals - 1);

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

        private static string GetSubstring(string text, int startIndex)
        {
            if (startIndex <= 0)
            {
                return text;
            }

            return text.Substring(startIndex);
        }

        public static bool TryParseLanguageAndRegion(string fileName, out string language, out string region)
        {
            string languageAndRegionSuffix = GetSubstring(fileName, fileName.IndexOf("_", StringComparison.InvariantCultureIgnoreCase));
            string[] languageAndRegionArray = languageAndRegionSuffix.Split("_");
            if (languageAndRegionArray.Length >= 2)
            {
                language = languageAndRegionArray[0];
                region = languageAndRegionArray[1];
                return true;
            }

            if (languageAndRegionArray.Length >= 1)
            {
                language = languageAndRegionArray[0];
                region = "";
                return true;
            }

            language = "";
            region= "";
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
    }
}
