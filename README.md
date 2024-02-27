[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/achimmihca/ProTrans/blob/main/LICENSE)
[![Sponsor this project](https://img.shields.io/badge/-Sponsor-fafbfc?logo=GitHub%20Sponsors)](https://github.com/sponsors/achimmihca)

# ProTrans
Properties file based translations for the Unity game engine.

Properties files are [simple](https://docs.oracle.com/cd/E23095_01/Platform.93/ATGProgGuide/html/s0204propertiesfileformat01.html), industry proven in Java, and well supported by many localization tools.

## Features
- Get strings for different languages from properties files
- Support regional dialects and fallback chains
  - For example `en-US` and `en-GB` can fall back to common `en` for a missing translation
- Add a new language or regional dialect by adding a properties file with corresponding suffix
- Sensible defaults for missing translations
- Simple static methods that can be accessed from anywhere
- Usable on any thread, not only on the main thread
- Built on top of [System.Globalization.CultureInfo](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo)

## Example:
```
# Example properties file
hello = Hello {name}!
multilineUsingNewline = First line\nSecond line
multilineUsingBackslash = First line\
    Second line
umlautDirectlyInUtf8File = Äpfel
umlautAsEscapedAscii = \u00C4pfel
colonAsSeparator: Separate key and value via colon or equals
```

```
// Get translations via static methods 
Translation.Get("multilineUsingNewline");

// Specify placeholders via varargs or a dictionary 
Translation.Get("hello", "name", "Alice");
Translation.Get("hello", new Dictionary<string, string>() { {"name", "Alice"} });

// Change current language and other configuration via static properties
TranslationConfig.Singleton.CurrentCultureInfo = new CultureInfo("en-US");
```

## Installation
- You can add a dependency to your `Packages/manifest.json` using a [Git URL](https://docs.unity3d.com/Documentation/Manual/upm-git.html) in the following form:
  `"com.achimmihca.protrans": "https://github.com/achimmihca/ProTrans.git?path=ProTrans/Packages/com.achimmihca.ProTrans"`
    - Note that `#v1.0.0` can be used to specify a tag or commit hash.

## Configuration
- See [TranslationConfig](https://github.com/achimmihca/ProTrans/blob/main/ProTrans/Packages/com.achimmihca.ProTrans/Runtime/TranslationConfig.cs) for available properties
- By default, properties files are expected in `StreamingAssets/Translations` and with name `messages.properties`, `messages_de_CH.properties`, etc.
  - This can be changed by setting `TranslationConfig.PropertiesFileProvider`
- By default, the base properties file (i.e. without any language and region suffix) is expected to contain English translations.
  - This can be changed by setting `TranslationConfig.DefaultCultureInfo`
- By default a language is searched in the following order
  - (1) currently selected language (e.g. `messages_de_CH.properties`)
  - (2) without the region suffix (e.g. `messages_de.properties`)
  - (3) without any suffix (e.g., `messages.properties`)
  - This can be changed by setting `TranslationConfig.FallbackCultureInfoProvider`

## Prepare Translations
- Use underscores and [two letter country codes](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2) as suffix for different languages and regional dialects.
  - Example: `messages.properties` is the default (and fallback) language, `messages_de.properties` is for German translations, `messages_de_CH.properties` is for German as spoken in Switzerland.
  - See also `CultureInfo.GetCultures(CultureTypes.AllCultures)`

# History
ProTrans has been created originally for [UltraStar Play](https://github.com/UltraStar-Deluxe/Play).
If you like singing, karaoke, or sing-along games then go check it out ;)
