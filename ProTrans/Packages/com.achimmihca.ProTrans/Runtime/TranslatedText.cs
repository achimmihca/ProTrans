using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProTrans
{
    [RequireComponent(typeof(Text))]
    [ExecuteInEditMode]
    public class TranslatedText : AbstractTranslatorBehaviour
    {
        [Delayed]
        public string key;
        private string lastKey;

        private Text uiText;
        
#if UNITY_EDITOR
        private void Update()
        {
            if (lastKey != key)
            {
                lastKey = key;
                UpdateTranslation();
            }
        }
#endif

        public override void UpdateTranslation()
        {
            if (key == null)
            {
                Debug.LogWarning($"Missing translation key for object '{gameObject.name}'", gameObject);
                return;
            }

            string trimmedKey = key.Trim();
            if (string.IsNullOrEmpty(trimmedKey))
            {
                Debug.LogWarning($"Missing translation key for object '{gameObject.name}'", gameObject);
                return;
            }

            Dictionary<string, string> translationArguments = GetTranslationArguments();
            string translation = TranslationManager.GetTranslation(trimmedKey, translationArguments);
            
            if (uiText == null)
            {
                uiText = GetComponent<Text>();
            }
            uiText.text = translation;
        }

        protected virtual Dictionary<string, string> GetTranslationArguments()
        {
            return null;
        }
    }
}
