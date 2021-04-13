using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace ProTrans
{
    [ExecuteInEditMode]
    public class TranslationManager : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            instance = null;
            currentLanguageMessages?.Clear();
            fallbackMessages?.Clear();
        }

        [Tooltip("Path that should be monitored by the FileSystemWatcher for modifications")]
        public static string propertiesFileFolderRelativeToProjectFolder = "Assets/Resources/Translations";
        public string propertiesFileName = "messages";

        // Fields are static to be persisted across scenes
        private static TranslationManager instance;
        public static TranslationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindComponentWithTag<TranslationManager>("TranslationManager");
                    if (instance != null)
                    {
                        instance.InitSingleInstance();
                    }
                }
                return instance;
            }
        }
        
        private static Dictionary<string, string> currentLanguageMessages = new Dictionary<string, string>();
        private static Dictionary<string, string> fallbackMessages = new Dictionary<string, string>();

        public bool logInfo;
        public SystemLanguage currentLanguage;
        public SystemLanguage defaultPropertiesFileLanguage = SystemLanguage.English;
        
        private SystemLanguage lastLanguage;

        private void Awake()
        {
            InitSingleInstance();
            if (instance != this)
            {
                return;
            }
            
            if (fallbackMessages.IsNullOrEmpty())
            {
                UpdateCurrentLanguageAndTranslations();
            }
        }

    #if UNITY_EDITOR
        private void Update()
        {
            if (fallbackMessages.IsNullOrEmpty()
                || lastLanguage != currentLanguage)
            {
                lastLanguage = currentLanguage;
                UpdateCurrentLanguageAndTranslations();
            }
        }
    #endif

        public List<string> GetKeys()
        {
            return fallbackMessages.Keys.ToList();
        }

        public List<SystemLanguage> GetTranslatedLanguages()
        {
            List<SystemLanguage> result = new List<SystemLanguage>();
            foreach (SystemLanguage systemLanguage in GetEnumValuesAsList<SystemLanguage>())
            {
                string suffix = GetCountryCodeSuffixForPropertiesFile(systemLanguage);
                string propertiesFilePath = GetPropertiesFilePathInResources(propertiesFileName + suffix);
                TextAsset textAsset = Resources.Load<TextAsset>(propertiesFilePath);
                if (textAsset != null)
                {
                    result.Add(systemLanguage);
                }
            }
            return result;
        }

        public static string GetTranslation(string key, params object[] placeholderStrings)
        {
            if (placeholderStrings.Length % 2 != 0)
            {
                Debug.LogWarning($"Uneven number of placeholders for '{key}'. Format in array should be [key1, value1, key2, value2, ...]");
            }

            // Create dictionary from placeholderStrings
            Dictionary<string, string> placeholders = new Dictionary<string, string>();
            for (int i = 0; i < placeholderStrings.Length && i + 1 < placeholderStrings.Length; i += 2)
            {
                string placeholderKey = placeholderStrings[i].ToString();
                string placeholderValue = placeholderStrings[i + 1].ToString();
                placeholders[placeholderKey] = placeholderValue;
            }
            return GetTranslation(key, placeholders);
        }

        public static string GetTranslation(string key, Dictionary<string, string> placeholders)
        {
            string translation = GetTranslation(key);
            if (placeholders == null)
            {
                return translation;
            }
            
            foreach (KeyValuePair<string, string> placeholder in placeholders)
            {
                string placeholderText = "{" + placeholder.Key + "}";
                if (translation.Contains(placeholderText))
                {
                    translation = translation.Replace(placeholderText, placeholder.Value);
                }
                else
                {
                    Debug.LogWarning($"Translation for {key} is missing placeholder {placeholderText}.");
                }
            }
            return translation;
        }

        public static string GetTranslation(string key)
        {
            if (!Instance.gameObject.activeSelf)
            {
                // TranslationManager is deactivated. There are no translations.
                return key;
            }
            if (fallbackMessages.IsNullOrEmpty())
            {
                Instance.UpdateCurrentLanguageAndTranslations();
                // TranslationManager was deactivated. There are no translations.
                if (!Instance.gameObject.activeSelf)
                {
                    return key;
                }
            }
            
            if (currentLanguageMessages.TryGetValue(key, out string translation))
            {
                return translation;
            }
            else
            {
                Debug.LogWarning($"Missing translation in language '{Instance.currentLanguage}' for key '{key}'");
                if (fallbackMessages.TryGetValue(key, out string fallbackTranslation))
                {
                    return fallbackTranslation;
                }
                else
                {
                    Debug.LogError($"No translation for key '{key}' in fallback language.");
                    return key;
                }
            }
        }

        public void UpdateCurrentLanguageAndTranslations()
        {
            currentLanguageMessages = new Dictionary<string, string>();
            fallbackMessages = new Dictionary<string, string>();

            // Load the default properties file
            string fallbackPropertiesPath = GetPropertiesFilePathInResources(propertiesFileName);
            TextAsset fallbackPropertiesTextAsset = Resources.Load<TextAsset>(fallbackPropertiesPath);
            if (fallbackPropertiesTextAsset != null)
            {
                LoadPropertiesFromText(fallbackPropertiesTextAsset.text, fallbackMessages);
            }
            else
            {
                Debug.LogError($"Properties file for fallback language not found in any Resources folder: {fallbackPropertiesPath}. Deactivating TranslationManager.", gameObject);
                gameObject.SetActive(false);
                return;
            }

            // Load the properties file of the current language
            string propertiesFileNameWithCountryCode = propertiesFileName + GetCountryCodeSuffixForPropertiesFile(currentLanguage);
            string propertiesFilePath = GetPropertiesFilePathInResources(propertiesFileNameWithCountryCode);
            TextAsset propertiesFileTextAsset = Resources.Load<TextAsset>(propertiesFilePath);
            if (propertiesFileTextAsset != null)
            {
                LoadPropertiesFromText(propertiesFileTextAsset.text, currentLanguageMessages);
            }
            else
            {
                Debug.LogWarning($"Properties file for language {currentLanguage} not found in any Resources folder: {propertiesFilePath}", gameObject);
            }

            if (logInfo)
            {
                Debug.Log("Loaded " + fallbackMessages.Count + " translations from " + fallbackPropertiesPath);
                Debug.Log("Loaded " + currentLanguageMessages.Count + " translations from " + propertiesFilePath);
            }

            if (fallbackMessages.Count == 0)
            {
                Debug.LogError("No fallback translations found. Deactivating TranslationManager", gameObject);
                gameObject.SetActive(false);
                return;
            }
            UpdateAllTranslationsInScene();
        }

        private static void UpdateAllTranslationsInScene()
        {
            LinkedList<ITranslator> translators = new LinkedList<ITranslator>();
            Scene scene = SceneManager.GetActiveScene();
            if (scene != null && scene.isLoaded)
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObject in rootObjects)
                {
                    UpdateTranslationsRecursively(rootObject, translators);
                }

                if (Instance != null && Instance.logInfo)
                {
                    Debug.Log($"Updated ITranslator instances in scene: {translators.Count}");
                }
            }
        }

        private static void UpdateTranslationsRecursively(GameObject gameObject, LinkedList<ITranslator> translators)
        {
            MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                // The script can be null if it is a missing component.
                if (script == null)
                {
                    continue;
                }

                if (script is ITranslator translator)
                {
                    translators.AddLast(translator);
                    translator.UpdateTranslation();
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                UpdateTranslationsRecursively(child.gameObject, translators);
            }
        }

        private static void LoadPropertiesFromText(string text, Dictionary<string, string> targetDictionary)
        {
            targetDictionary.Clear();
            foreach (KeyValuePair<string, string> entry in PropertiesFileParser.ParseText(text))
            {
                targetDictionary.Add(entry.Key, entry.Value);
            }
        }

        private string GetCountryCodeSuffixForPropertiesFile(SystemLanguage language)
        {
            if (language != Instance.defaultPropertiesFileLanguage)
            {
                string countryCode = LanguageHelper.Get2LetterIsoCodeFromSystemLanguage(language, null);
                return countryCode != null ? ("_" +countryCode.ToLower()) : Instance.propertiesFileName;
            }
            else
            {
                return "";
            }
        }

        protected virtual string GetPropertiesFilePathInResources(string propertiesFileNameWithCountryCode)
        {
            return propertiesFileFolderRelativeToProjectFolder.Replace("Assets/Resources/", "") + "/" + propertiesFileNameWithCountryCode;
        }
        
        /// Looks in the GameObject with the given tag
        /// for the component that is specified by the generic type parameter.
        private static T FindComponentWithTag<T>(string tag)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(tag);
            if (!obj)
            {
                return default(T);
            }

            T component = obj.GetComponent<T>();
            if (component == null)
            {
                Debug.LogError($"Did not find Component '{typeof(T)}' in GameObject with tag '{tag}'.", obj);
            }
            return component;
        }

        private static List<TEnum> GetEnumValuesAsList<TEnum>() where TEnum : Enum
        {
            return ((TEnum[])Enum.GetValues(typeof(TEnum))).ToList();
        }
        
        private void InitSingleInstance()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (instance != null
                && instance != this)
            {
                // This instance is not needed.
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            // Move object to top level in scene hierarchy.
            // Otherwise this object will be destroyed with its parent, even when DontDestroyOnLoad is used.
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }
}
