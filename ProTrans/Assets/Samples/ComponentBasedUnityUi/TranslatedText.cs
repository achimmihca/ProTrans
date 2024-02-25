using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProTrans
{
    [RequireComponent(typeof(TMP_Text))]
    [ExecuteInEditMode]
    public class TranslatedText : MonoBehaviour
    {
        [Delayed]
        public string key;
        public Dictionary<string, string> placeholders = new();

        private TMP_Text uiText;
        private string lastKey;

        private void Start()
        {
            UpdateTranslation();
        }

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

        public void UpdateTranslation()
        {
            string trimmedKey = key?.Trim();
            if (string.IsNullOrEmpty(trimmedKey))
            {
                Debug.LogWarning($"Missing translation key for object '{gameObject.name}'", gameObject);
                return;
            }

            string translation = Translation.Get(trimmedKey, placeholders);

            if (uiText == null)
            {
                uiText = GetComponent<TMP_Text>();
            }
            uiText.text = translation;
        }
    }
}
