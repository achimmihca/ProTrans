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
- Properties files for ProTrans should be encoded in UTF-8 with BOM.
    - This will ensure that the C# API will load the files correctly.
    - Note that this encoding differs from the encoding that properties files in Java typically use.

## Syntax
- Key and value are separated by an equals sign. Whitespace around the key and value is trimmed.
- Comments start with `#` or `!`
- You can quote strings if needed (e.g. when you want whitespace at the end of the value).
    - Example: `quotesExample = "These quotes are optional"
- Use curly braces for named placeholders.
    - Example: `helloLabel = Hello {name}`
- Use `\n` for line breaks.
    - Example: `multiLineExample = This is the first line\nThis is the second line`

## Examples:
```
# This is a comment
demoScene_button_quit_label = Quit
demoScene_button_quit_tooltip = Closes the game
demoScene_button_start = "Start"
demoScene_hello = Hello {name}!
demoScene_multiline = First line\nSecond line
```

# History
ProTrans has been created originally for [UltraStar Play](https://github.com/UltraStar-Deluxe/Play).
If you like singing, karaoke, or sing-along games then go check it out ;)