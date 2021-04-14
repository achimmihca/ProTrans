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

        string[][] pathArrays = { importedAssets, deletedAssets, movedAssets };
        foreach (string[] pathArray in pathArrays)
        {
            foreach (string path in importedAssets)
            {
                if (path.EndsWith(".properties"))
                {
                    if (translationManager.logInfo)
                    {
                        Debug.Log("Reloading translations because of changed file: " + path);
                    }
                    translationManager.UpdateCurrentLanguageAndTranslations();
                    // Updating the translations once is enough, no matter how many properties files have changed.
                    return;
                }
            }
        }
    }
}
