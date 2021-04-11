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

        public static readonly string propertiesFileFolder = "Translations";
        public static readonly string propertiesFileExtension = "properties";
        public static readonly string propertiesFileName = "messages";

        // Fields are static to be persisted across scenes
        private static TranslationManager instance;
        public static TranslationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindComponentWithTag<TranslationManager>("TranslationManager");
                    instance.InitSingleInstance();
                }
                return instance;
            }
        }
        
        private static Dictionary<string, string> currentLanguageMessages = new Dictionary<string, string>();
        private static Dictionary<string, string> fallbackMessages = new Dictionary<string, string>();

        public bool logInfo;
        public SystemLanguage currentLanguage;
        public SystemLanguage defaultPropertiesFileLanguage = SystemLanguage.English;
        public bool extractStreamingAssetsOnAndroid = true;
        
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
            if (Application.isPlaying)
            {
                return;
            }

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
                    Debug.LogError($"No translation for key '{key}'");
                    return key;
                }
            }
        }

        public void UpdateCurrentLanguageAndTranslations()
        {
            currentLanguageMessages = new Dictionary<string, string>();
            fallbackMessages = new Dictionary<string, string>();

            // Load the default properties file
            string fallbackPropertiesFilePath = GetPropertiesFilePath(propertiesFileName);
            if (File.Exists(fallbackPropertiesFilePath))
            {
                string fallbackPropertiesFileContent = File.ReadAllText(fallbackPropertiesFilePath);
                LoadPropertiesFromText(fallbackPropertiesFileContent, fallbackMessages);
            }
            else
            {
                Debug.LogError($"Properties file for fallback language not found: {fallbackPropertiesFilePath}. Deactivating TranslationManager.", gameObject);
                gameObject.SetActive(false);
                return;
            }

            // Load the properties file of the current language
            string propertiesFileNameWithCountryCode = propertiesFileName + GetCountryCodeSuffixForPropertiesFile(currentLanguage);
            string propertiesFilePath = GetPropertiesFilePath(propertiesFileNameWithCountryCode);
            if (File.Exists(propertiesFilePath))
            {
                string propertiesFileContent = File.ReadAllText(propertiesFilePath);
                LoadPropertiesFromText(propertiesFileContent, currentLanguageMessages);
            }
            else
            {
                Debug.LogWarning($"Properties file for language {currentLanguage} not found: {propertiesFilePath}", gameObject);
            }

            if (logInfo)
            {
                Debug.Log("Loaded " + fallbackMessages.Count + " translations from " + fallbackPropertiesFilePath);
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
                Debug.Log($"Updated ITranslator instances in scene: {translators.Count}");
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

        private static string GetCountryCodeSuffixForPropertiesFile(SystemLanguage language)
        {
            if (language != Instance.defaultPropertiesFileLanguage)
            {
                return "_" + LanguageHelper.Get2LetterIsoCodeFromSystemLanguage(language, "en").ToLower();
            }
            else
            {
                return "";
            }
        }

        protected virtual string GetPropertiesFilePath(string propertiesFileNameWithCountryCode)
        {
            string relativePath = propertiesFileFolder + "/" + propertiesFileNameWithCountryCode + "." + propertiesFileExtension;
            string absolutePath = GetStreamingAssetsPath() + "/" + relativePath;
            return absolutePath;
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
        
        private static string GetStreamingAssetsPath()
        {
    #if UNITY_ANDROID
            if (extractStreamingAssetsOnAndroid)
            {
                return AndroidStreamingAssets.Path;
            }
            else
            {
                return Application.streamingAssetsPath;
            }
    #else
            return Application.streamingAssetsPath;
    #endif
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
