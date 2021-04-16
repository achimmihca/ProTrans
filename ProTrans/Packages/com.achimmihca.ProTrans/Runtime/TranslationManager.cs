using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            translatedLanguages?.Clear();
        }

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
        private static List<SystemLanguage> translatedLanguages = new List<SystemLanguage>();

        
        public string propertiesFolderRelativeToResourcesFolder = "Translations";
        public string propertiesFileName = "messages";
        
        public bool logInfoInEditMode;
        public bool logInfoInPlayMode = true;
        
        public string generatedConstantsFolder = "Assets/GeneratedScripts";
        public bool generateConstantsOnResourceChange;
        
        public bool useFallbackIfNotInCurrentLanguage = true;
        public SystemLanguage currentLanguage = SystemLanguage.English;
        public SystemLanguage defaultPropertiesFileLanguage = SystemLanguage.English;
        
        public bool LogInfoNow => Application.isPlaying && logInfoInPlayMode 
                                  || !Application.isPlaying && logInfoInEditMode;
        
        protected virtual void Awake()
        {
            InitSingleInstance();
            if (instance != this)
            {
                return;
            }
            
            if (fallbackMessages.IsNullOrEmpty())
            {
                ReloadTranslationsAndUpdateScene();
            }
        }

        public virtual List<string> GetKeys()
        {
            return fallbackMessages.Keys.ToList();
        }

        public virtual List<SystemLanguage> GetTranslatedLanguages()
        {
            if (translatedLanguages.IsNullOrEmpty())
            {
                foreach (SystemLanguage systemLanguage in GetEnumValuesAsList<SystemLanguage>())
                {
                    string suffix = GetCountryCodeSuffixForPropertiesFile(systemLanguage);
                    string propertiesFilePath = GetPropertiesFilePathInResources(propertiesFileName + suffix);
                    TextAsset textAsset = Resources.Load<TextAsset>(propertiesFilePath);
                    if (textAsset != null)
                    {
                        translatedLanguages.Add(systemLanguage);
                    }
                }
            }
            return translatedLanguages;
        }

        public static string GetTranslation(string key, params object[] placeholderStrings)
        {
            if (placeholderStrings == null
                || placeholderStrings.Length == 0)
            {
                TryGetTranslation(key, out string translation);
                return translation;
            }
            
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
            if (!TryGetActiveInstance(out TranslationManager translationManager))
            {
                return key;
            }

            if (!TryGetTranslation(key, out string translation)
                || placeholders == null)
            {
                // No proper translation found or no placeholders that should be replaced.  
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

        private static bool TryGetTranslation(string key, out string translation)
        {
            if (!TryGetActiveInstance(out TranslationManager translationManager))
            {
                translation = key;
                return false;
            }
            
            if (fallbackMessages.IsNullOrEmpty())
            {
                translationManager.ReloadTranslationsAndUpdateScene();
                // TranslationManager was deactivated during the attempt to update translations. There are no translations.
                if (!translationManager.gameObject.activeSelf)
                {
                    translation = key;
                    return false;
                }
            }

            if (translationManager.currentLanguage != translationManager.defaultPropertiesFileLanguage)
            {
                if (currentLanguageMessages.IsNullOrEmpty())
                {
                    if (!translationManager.TryReloadCurrentLanguageTranslations())
                    {
                        translation = key;
                        return false;
                    }
                }
                
                if (currentLanguageMessages.TryGetValue(key, out string currentLanguageTranslation))
                {
                    translation = currentLanguageTranslation;
                    return true;
                }
                Debug.LogWarning($"Missing translation in language '{translationManager.currentLanguage}' for key '{key}'");
            }
            
            if (fallbackMessages.TryGetValue(key, out string fallbackTranslation))
            {
                if (translationManager.useFallbackIfNotInCurrentLanguage)
                {
                    translation = fallbackTranslation;
                    return true;
                }
                
                translation = key;
                return false;
            }
            
            Debug.LogError($"No translation for key '{key}' in fallback language.");
            translation = key;
            return false;
        }

        public void ReloadTranslationsAndUpdateScene()
        {
            ReloadTranslations();
            UpdateTranslatorsInScene();
        }

        private void ReloadTranslations()
        {
            if (!TryReloadFallbackLanguageTranslations())
            {
                return;
            }

            if (currentLanguage != defaultPropertiesFileLanguage)
            {
                TryReloadCurrentLanguageTranslations();
            }
        }

        public virtual bool TryReloadFallbackLanguageTranslations()
        {
            fallbackMessages = new Dictionary<string, string>();
            translatedLanguages = new List<SystemLanguage>();
            
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
                return false;
            }
            
            if (fallbackMessages.Count == 0)
            {
                Debug.LogError("No fallback translations found. Deactivating TranslationManager", gameObject);
                gameObject.SetActive(false);
                return false;
            }
            else if (!gameObject.activeSelf)
            {
                Debug.Log("Found fallback translations. Activating TranslationManager", gameObject);
                gameObject.SetActive(true);
            }
            
            if (LogInfoNow)
            {
                Debug.Log("Loaded " + fallbackMessages.Count + " translations from " + fallbackPropertiesPath);
            }

            return true;
        }

        public virtual bool TryReloadCurrentLanguageTranslations()
        {
            currentLanguageMessages = new Dictionary<string, string>();
            translatedLanguages = new List<SystemLanguage>();
            
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
                return false;
            }

            if (LogInfoNow)
            {
                Debug.Log("Loaded " + currentLanguageMessages.Count + " translations from " + propertiesFilePath);
            }

            return true;
        }

        public virtual void UpdateTranslatorsInScene()
        {
            LinkedList<ITranslator> translators = new LinkedList<ITranslator>();
            Scene scene = SceneManager.GetActiveScene();
            if (scene != null && scene.isLoaded)
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject rootObject in rootObjects)
                {
                    UpdateTranslatorsRecursively(rootObject, translators);
                }

                if (LogInfoNow)
                {
                    Debug.Log($"Updated ITranslator instances in scene: {translators.Count}");
                }
            }
        }

        private void UpdateTranslatorsRecursively(GameObject localGameObject, LinkedList<ITranslator> translators)
        {
            MonoBehaviour[] scripts = localGameObject.GetComponents<MonoBehaviour>();
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

            foreach (Transform child in localGameObject.transform)
            {
                UpdateTranslatorsRecursively(child.gameObject, translators);
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
            if (language != defaultPropertiesFileLanguage)
            {
                string countryCode = LanguageHelper.Get2LetterIsoCodeFromSystemLanguage(language);
                return countryCode != null ? ("_" +countryCode.ToLower()) : propertiesFileName;
            }
            else
            {
                return "";
            }
        }

        protected virtual string GetPropertiesFilePathInResources(string propertiesFileNameWithCountryCode)
        {
            if (propertiesFolderRelativeToResourcesFolder.IsNullOrEmpty())
            {
                return propertiesFileNameWithCountryCode;
            }
            
            return propertiesFolderRelativeToResourcesFolder + "/" + propertiesFileNameWithCountryCode;
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

        private static bool TryGetActiveInstance(out TranslationManager translationManager)
        {
            translationManager = Instance;
            return translationManager != null
                   && translationManager.gameObject.activeSelf;
        }
    }
}
