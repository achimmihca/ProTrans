using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace ProTrans
{
    public class TranslationTests
    {
        [SetUp]
        public void SetUp()
        {
            TranslationConfig.Singleton = new();
        }

        [TearDown]
        public void TearDown()
        {
            TranslationConfig.Singleton = new();
        }

        [Test]
        public void TranslatedCultureInfosTest()
        {
            List<CultureInfo> translatedCultureInfos = Translation.GetTranslatedCultureInfos();
            Assert.That(translatedCultureInfos.Count == 3);
            Assert.AreEqual("en", translatedCultureInfos[0].ToString());
            Assert.AreEqual("de", translatedCultureInfos[1].ToString());
            Assert.AreEqual("de-AT", translatedCultureInfos[2].ToString());
        }

        [Test]
        public void NullPlaceholderValueTest()
        {
            Assert.AreEqual("Hello !", Translation.Get("hello", "name", null));
        }

        [Test]
        public void NonMainThreadTest()
        {
            string translation = null;
            Thread thread = new Thread(() =>
            {
                translation = Translation.Get("hello", "name", "Alice");
            });
            thread.Start();
            thread.Join();
            Assert.AreEqual("Hello Alice!", translation);
        }

        [Test]
        public void GetTranslationTest()
        {
            Assert.AreEqual("missing_translation", Translation.Get("missing_translation"));

            Assert.AreEqual("Hello Alice!", Translation.Get("hello",
                "name", "Alice"));

            Assert.AreEqual("Hello Bob!", Translation.Get("hello",
                new Dictionary<string, string>()
                {
                    { "name", "Bob"},
                }));
            Assert.AreEqual("Hello Charlie!", Translation.Get("hello",
                new List<string>() { "name", "Charlie" }));
        }

        [Test]
        public void ChangeCurrentCultureTest()
        {
            TranslationConfig.Singleton.CurrentCultureInfo = new CultureInfo("en");
            Assert.AreEqual("Hello Alice!", Translation.Get("hello", "name", "Alice"));

            TranslationConfig.Singleton.CurrentCultureInfo = new CultureInfo("de");
            Assert.AreEqual("Hallo Alice!", Translation.Get("hello", "name", "Alice"));

            TranslationConfig.Singleton.CurrentCultureInfo = new CultureInfo("de-AT");
            Assert.AreEqual("Servus Alice!", Translation.Get("hello", "name", "Alice"));
        }

        [Test]
        public void FallbackCultureInfoTest()
        {
            TranslationConfig.Singleton.CurrentCultureInfo = new CultureInfo("en");
            Assert.AreEqual("Partially translated (en)", Translation.Get("partial_translation"));

            // de-AT should fall back to use the translation from de
            TranslationConfig.Singleton.CurrentCultureInfo = new CultureInfo("de-AT");
            Assert.AreEqual("Teilweise übersetzt (de)", Translation.Get("partial_translation"));
        }

        [Test]
        public void UnexpectedPlaceholderTest()
        {
            TranslationConfig.Singleton.UnexpectedPlaceholderStrategy = UnexpectedPlaceholderStrategy.Throw;
            Assert.Throws<TranslationException>(() => Translation.Get("hello",
                "name", "Alice",
                "wrongPlaceholder", "Bob"));

            TranslationConfig.Singleton.UnexpectedPlaceholderStrategy = UnexpectedPlaceholderStrategy.Ignore;
            Assert.AreEqual("Hello Alice!", Translation.Get("hello",
                "name", "Alice",
                "wrongPlaceholder", "Alice"));
        }

        [Test]
        public void MissingPlaceholderTest()
        {
            TranslationConfig.Singleton.MissingPlaceholderStrategy = MissingPlaceholderStrategy.Throw;
            Assert.Throws<TranslationException>(() => Translation.Get("hello"));

            TranslationConfig.Singleton.MissingPlaceholderStrategy = MissingPlaceholderStrategy.Ignore;
            Assert.AreEqual("Hello {name}!", Translation.Get("hello"));
        }

        [Test]
        public void TranslationKeyIsCaseInsensitiveTest()
        {
            TranslationConfig.Singleton.CurrentCultureInfo = new CultureInfo("en");
            Assert.AreEqual("Hello Alice!", Translation.Get("hello", "name", "Alice"));
            Assert.AreEqual("Hello Alice!", Translation.Get("HeLLo", "name", "Alice"));
        }
    }
}
