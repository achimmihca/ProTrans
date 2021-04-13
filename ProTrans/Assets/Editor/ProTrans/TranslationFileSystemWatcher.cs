using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine.UI;

namespace ProTrans
{
    [InitializeOnLoad]
    public static class TranslationFileSystemWatcher
    {
        private static bool triggerUpdate;
        private static ConcurrentBag<string> changedFiles = new ConcurrentBag<string>();
        private static readonly string absoluteProjectFolder;
        
        static TranslationFileSystemWatcher()
        {
            string propertiesFileFolderRelativeToProjectFolder = TranslationManager.propertiesFileFolderRelativeToProjectFolder;
            Debug.Log("dataPath: " + Application.dataPath);
            absoluteProjectFolder = Application.dataPath.Replace("/Assets", "");
            if (propertiesFileFolderRelativeToProjectFolder.IsNullOrEmpty())
            {
                Debug.LogWarning($"Path to properties files folder not set. Not watching properties files for changes.");
                return;
            }
            if (!Directory.Exists(propertiesFileFolderRelativeToProjectFolder))
            {
                Debug.LogWarning($"Properties files folder does not exist. Not watching properties files for changes. Path: {propertiesFileFolderRelativeToProjectFolder}");
                return;
            }
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(propertiesFileFolderRelativeToProjectFolder, "*.properties");
            fileSystemWatcher.Changed += OnFileChanged;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.EnableRaisingEvents = true;

            EditorApplication.update += OnEditorApplicationUpdate;

            Debug.Log("Watching translation files in: " + propertiesFileFolderRelativeToProjectFolder);
        }

        private static void OnEditorApplicationUpdate()
        {
            // Note that this is called very often (100/sec). Thus, code in here should be fast.
            if (triggerUpdate)
            {
                triggerUpdate = false;

                TranslationManager translationManager = TranslationManager.Instance;
                if (translationManager != null)
                {
                    List<string> quotedChangedFiles = changedFiles.Distinct().Select(it => $"'{it}'").ToList();
                    string changedFilesCsv = string.Join(", ", quotedChangedFiles);
                    Debug.Log("Reloading translations because of changed files: " + changedFilesCsv);
                    
                    // Unity needs a hint that the asset has changed.
                    foreach (string changedFile in changedFiles)
                    {
                        AssetDatabase.ImportAsset(changedFile);
                    }
                    
                    changedFiles = new ConcurrentBag<string>();
                    translationManager.UpdateCurrentLanguageAndTranslations();
                }
            }
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Never call any Unity API because we are not in the main thread
            string projectRelativePath = e.FullPath.Substring(absoluteProjectFolder.Length + 1); // +1 for the last slash
            changedFiles.Add(projectRelativePath);
            triggerUpdate = true;
        }
    }
}
