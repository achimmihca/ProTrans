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
        public List<string> placeholders = new();

        private TMP_Text uiText;
        private string lastKey;

        private void Awake()
        {
            uiText = GetComponent<TMP_Text>();
        }

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

            uiText.text = Translation.Get(trimmedKey, placeholders);
        }
    }
}
