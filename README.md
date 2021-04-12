# ProTrans
Properties file translation for Unity3D

# Demo
- Clone this repo
- Navigate to `Tools/DownloadDependencies` and run `sh download-dependencies.sh` in your favorite shell
    - On Windows you can use git-bash to run the `sh` command
- Open the Unity project, and take a look at the sample scene.

# How to Use

## Get the Code

- This project uses [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) (MIT license) to extract the StreamingAssets folder on Android.
    - You could use the `download-dependencies.sh` script to download this library.
    - If you don't build your project for Android, then you can safely delete the `AndroidStreamingAssets.cs` file and are not required to use SharpZipLib.
- The relevant code of ProTrans is in `ProTrans/Assets/Scripts` and `ProTrans/Assets/Editor`. Copy these to your project.
- Note that there is a [ProTrans.asmdef](https://docs.unity3d.com/Manual/cus-asmdef.html) file.
    - You might need to reference this in your project.
    - The asmdef has a dependency on `Plugins` to reference SharpZipLib.
        - Again, if you don't build your project for Android, then you can safely remove the reference to `Plugins` from `ProTrans.asmdef`.

## Prepare Translations
- Create `Assets/StreamingAssets/Translations/messages.properties` in your project and add some key-value pairs (e.g. `sampleScene_helloWorld = Hello world!`).
    - Note that ProTrans assumes translations to be placed in the [StreamingAssets](https://docs.unity3d.com/Manual/StreamingAssets.html) special Unity folder.
        - The idea behind this is to allow users to create and test translations on their own.
    - Use an underscore and [two letter country code](https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2) suffix for different languages.
        - Example: `messages.properties` is the default (and fallback) language, `messages_de.properties` is for German translations, `messages_es.properties` is for Spanish translations

## Prepare the Scene
- Place an instance of TranslationManager in your scene
- Now it is possible to call TranslationManager.GetTranslation manually or implement ITranslator interface
- The TranslationManager calls ITranslator.UpdateTranslation to translate scenes.
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
    - Encoded unicode characters are not supported (e.g. `\u002c`). This also differs from properties files in Java.
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