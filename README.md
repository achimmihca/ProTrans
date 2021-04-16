
[![Build Status](https://travis-ci.org/achimmihca/ProTrans.svg?branch=main)](https://travis-ci.org/achimmihca/ProTrans)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/achimmihca/ProTrans/blob/main/LICENSE)

# ProTrans
Properties file translation for Unity3D

# Demo
- Clone this repo
- Open the Unity project, and take a look at the sample scenes.

# How to Use

## Get the Code

- You can add a dependency in `Packages/manifest.json` in the following form:
  `com.achimmihca.protrans": "https://github.com/achimmihca/ProTrans.git?path=ProTrans/Packages/com.achimmihca.ProTrans#v1.0.0"`
    - Note that `#v1.0.0` specifies a tag of this git repository. Remove this part to use the latest (possibly unstable) version.
    - Note further that the path parameter (`?path=...`) points to the folder in this git repository, where the Unity package is placed.

## Prepare Translations
- Create `Assets/Resources/Translations/messages.properties` in your project and add some key-value pairs (e.g. `sampleScene_helloWorld = Hello world!`).
    - Note that ProTrans assumes translations to be placed in the [Resources](https://docs.unity3d.com/Manual/SpecialFolders.html) special Unity folder.
        - This is used to load translations at runtime without further configuration (e.g. when adding a new language).
    - Use an underscore and [two letter country code](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2) suffix for different languages.
        - Example: `messages.properties` is the default (and fallback) language, `messages_de.properties` is for German translations, `messages_es.properties` is for Spanish translations

## Prepare the Scene
- Create a new tag "TranslationManager"
- Place an instance of TranslationManager in your scene
    - Add the "TranslationManager" tag to this instance
- Now it is possible to call TranslationManager.GetTranslation manually or implement ITranslator interface
- To translate a scene, the TranslationManager will call ITranslator.UpdateTranslation for every ITranslator instance in the scene.
- ITranslator implementations should also call their own UpdateTranslation in Start().
    - See `TranslatedText.cs` for an example

## (Optional) Generate Constants
- Use the corresponding menu item to create C# constants for your translation properties.
    - Using constants instead of strings enables auto-completion, avoids typos, and makes refactoring easier.
    - Example: `TranslationManager.GetTranslation(R.String.sampleScene_helloWorld)`

# Properties Files in ProTrans

Properties files are a standard in the Java world.
However, ProTrans has some specifics.

## Encoding
- Properties files for ProTrans should be encoded in UTF-8.
    - Note that this encoding differs from the encoding that properties files in Java typically use.
    - Encoded unicode characters are not supported (e.g. `\u002c`).
        - Instead, directly use the unescaped character in the UTF-8 encoded file.

## Syntax
- Key and value are separated by an equals sign.
    - Note that a colon cannot be used to separate key and value.
- Whitespace around the key and the equals sign is trimmed.
    - Thus the following are equivalent.
        - `sayHello=Hello!`
        - `sayHello  = Hello!`
    - Note that space at the end of the line is NOT trimmed.
- Comments start with `#` or `!`
- Use curly braces for named placeholders.
    - Example: `sayHelloWithName = Hello {name}`
- The characters newline, carriage return, and tab can be inserted as \n, \r, and \t, respectively.
    - Example: `multiLineExample = First line\nSecond line`
- You can also use a backslash at the end of a line. Whitespace of such a continued line will be trimmed at the start (but not at the end)
    - Example:
    ```
    multiLineExample2 = First line\
                        Second line
    ```
- The backslash character must be escaped as a double backslash.
    - Example: `path=c:\\docs\\doc1`
- To include space at the start of a translation, one can escape this space character
    - Example: `exampleWithLeadingSpace=\ space at the start`

### Example:
```
# This is a comment
demoScene_button_quit_label = Quit
demoScene_button_quit_tooltip = Closes the game
demoScene_hello = Hello {name}!
demoScene_multilineUsingNewline = First line\nSecond line
demoScene_multilineUsingBackslash = First line\
                                    Second line
```

# History
ProTrans has been created originally for [UltraStar Play](https://github.com/UltraStar-Deluxe/Play).
If you like singing, karaoke, or sing-along games then go check it out ;)
