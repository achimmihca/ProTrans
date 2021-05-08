using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProTrans
{
    public static class PropertiesFileParser
    {
        private static readonly KeyValuePair<string, string> emptyKeyValuePair = new KeyValuePair<string, string>("", "");

        public static Dictionary<string, string> ParseFile(string path)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            return ParseText(text);
        }

        public static Dictionary<string, string> ParseText(string text)
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
            return map;
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
    }
}
