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
            PropertiesFile propertiesFile = PropertiesFileParser.ParseFile(filePath);

            bool containsComment = AnyKeyToLowerContains(propertiesFile, "comment")
                                   || AnyValueToLowerContains(propertiesFile, "comment");
            Assert.That(!containsComment, "Contains comment in key or value");

            AssertPropertyIgnoreCase(propertiesFile, "test_helloWorld", "Hello world!");
            AssertPropertyIgnoreCase(propertiesFile, "test_equals", "before=after");
            AssertPropertyIgnoreCase(propertiesFile, "test_parameters", "First name: {firstName}, second name: {secondName}");
            AssertPropertyIgnoreCase(propertiesFile, "test_nonAscii", "üäößêéèáà");
            AssertPropertyIgnoreCase(propertiesFile, "test_quotes", "\"All these outer and 'single quotes' and \"double quotes\" are kept\"");
            AssertPropertyIgnoreCase(propertiesFile, "test_spaceAtTheEndNotAroundEquals", "Space around the equals sign is trimmed, but the ending space is kept: ");
            AssertPropertyIgnoreCase(propertiesFile, "test_tab", "Before tab\tafter tab");
            AssertPropertyIgnoreCase(propertiesFile, "test_newline", "First line\nAfter newline\rAfter carriage return");
            AssertPropertyIgnoreCase(propertiesFile, "test_backslash", "\\Just two backslashes\\");
            AssertPropertyIgnoreCase(propertiesFile, "test_targetCities", "Detroit,Chicago,Los Angeles");
            AssertPropertyIgnoreCase(propertiesFile, "test_KeepSpace", " <- white-space -> ");
        }

        private static bool AnyKeyToLowerContains(PropertiesFile propertiesFile, string substring)
        {
            foreach (KeyValuePair<string, string> entry in propertiesFile.Dictionary)
            {
                if (entry.Key.ToLowerInvariant().Contains(substring))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AnyValueToLowerContains(PropertiesFile propertiesFile, string substring)
        {
            foreach (KeyValuePair<string, string> entry in propertiesFile.Dictionary)
            {
                if (entry.Value.ToLowerInvariant().Contains(substring))
                {
                    return true;
                }
            }

            return false;
        }

        private static void AssertPropertyIgnoreCase(PropertiesFile propertiesFile, string key, string expectedValue)
        {
            if (propertiesFile.TryGetValue(key, out string actualValue))
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
