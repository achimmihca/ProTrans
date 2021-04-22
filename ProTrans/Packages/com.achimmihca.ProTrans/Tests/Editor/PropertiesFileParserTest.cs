using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ProTrans
{
    public class PropertiesFileParserTest
    {
        [Test]
        public void DoPropertiesFileParserTest()
        {
            string filePath = "Packages/com.achimmihca.ProTrans/Tests/Editor/TestProperties.properties";
            Dictionary<string, string> properties = PropertiesFileParser.ParseFile(filePath);

            bool containsComment = AnyKeyToLowerContains(properties, "comment")
                                   || AnyValueToLowerContains(properties, "comment");
            Assert.That(!containsComment, "Contains comment in key or value");
            
            AssertPropertyIgnoreCase(properties, "test_helloWorld", "Hello world!");
            AssertPropertyIgnoreCase(properties, "test_equals", "before=after");
            AssertPropertyIgnoreCase(properties, "test_parameters", "First name: {firstName}, second name: {secondName}");
            AssertPropertyIgnoreCase(properties, "test_nonAscii", "üäößêéèáà");
            AssertPropertyIgnoreCase(properties, "test_quotes", "\"All these outer and 'single quotes' and \"double quotes\" are kept\"");
            AssertPropertyIgnoreCase(properties, "test_spaceAtTheEndNotAroundEquals", "Space around the equals sign is trimmed, but the ending space is kept: ");
            AssertPropertyIgnoreCase(properties, "test_tab", "Before tab\tafter tab");
            AssertPropertyIgnoreCase(properties, "test_newline", "First line\nAfter newline\rAfter carriage return");
            AssertPropertyIgnoreCase(properties, "test_backslash", "\\Just two backslashes\\");
            AssertPropertyIgnoreCase(properties, "test_escapeNewline", "First line with backslash at the end\\\nSecond line\nThird line\nFourth line");
            AssertPropertyIgnoreCase(properties, "test_escapeNewlineQuoteKeepSpace", " First line \n Second line ");
        }

        private static bool AnyKeyToLowerContains(Dictionary<string, string> properties, string substring)
        {
            foreach (KeyValuePair<string, string> entry in properties)
            {
                if (entry.Key.ToLowerInvariant().Contains(substring))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnyValueToLowerContains(Dictionary<string, string> properties, string substring)
        {
            foreach (KeyValuePair<string, string> entry in properties)
            {
                if (entry.Value.ToLowerInvariant().Contains(substring))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertPropertyIgnoreCase(Dictionary<string, string> properties, string key, string expectedValue)
        {
            if (properties.TryGetValue(key, out string actualValue))
            {
                if (!string.Equals(expectedValue, actualValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    Assert.Fail($"Mismatch for key '{key}': Expected '{expectedValue}' but was '{actualValue}'");
                }
            }
            else if (expectedValue != null)
            {
                Assert.Fail($"Mismatch for key '{key}': No value found");
            }
        }
    }
}
