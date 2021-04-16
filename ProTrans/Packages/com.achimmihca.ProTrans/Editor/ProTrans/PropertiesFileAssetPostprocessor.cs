using System;
using System.Collections;
using System.Collections.Generic;
using ProTrans;
using UnityEditor;
using UnityEngine;

public class PropertiesFileAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        TranslationManager translationManager = TranslationManager.Instance;
        if (translationManager == null)
        {
            return;
        }

        string currentLanguagePropertiesFileNameSuffix = translationManager.currentLanguage != translationManager.defaultPropertiesFileLanguage
                ? "_" + LanguageHelper.Get2LetterIsoCodeFromSystemLanguage(translationManager.currentLanguage, "en").ToLowerInvariant()
                : "";
        string currentLanguagePropertiesFileName = translationManager.propertiesFileName + currentLanguagePropertiesFileNameSuffix + ".properties";
        string fallbackLanguagePropertiesFileName = translationManager.propertiesFileName + ".properties";
        
        bool currentTranslationsUpdated = false;
        bool fallbackTranslationsUpdated = false;
        
        string[][] pathArrays = { importedAssets, deletedAssets, movedAssets };
        foreach (string[] pathArray in pathArrays)
        {
            foreach (string path in pathArray)
            {
                if (path.EndsWith(fallbackLanguagePropertiesFileName))
                {
                    if (translationManager.LogInfoNow)
                    {
                        Debug.Log("Reloading default language translations because of changed file: " + path);
                    }
                    translationManager.TryReloadFallbackLanguageTranslations();
                    fallbackTranslationsUpdated = true;
                }
                else if (path.EndsWith(currentLanguagePropertiesFileName))
                {
                    if (translationManager.LogInfoNow)
                    {
                        Debug.Log("Reloading current language translations because of changed file: " + path);
                    }
                    translationManager.TryReloadCurrentLanguageTranslations();
                    currentTranslationsUpdated = true;
                }

                if (currentTranslationsUpdated
                    && fallbackTranslationsUpdated)
                {
                    // All languages have been updated already.
                    break;
                }
            }
        }

        if (currentTranslationsUpdated
            || fallbackTranslationsUpdated)
        {
            translationManager.UpdateTranslatorsInScene();
        }
        if (translationManager.generateConstantsOnResourceChange
            && fallbackTranslationsUpdated)
        {
            CreateTranslationConstantsMenuItems.CreateTranslationConstants();
        }
    }
}
