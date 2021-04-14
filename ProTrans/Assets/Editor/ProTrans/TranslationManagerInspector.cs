using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ProTrans
{
    [CustomEditor(typeof(TranslationManager))]
    public class TranslationManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            TranslationManager translationManager = target as TranslationManager;
            DrawDefaultInspectorWithoutProperties(this.serializedObject, "currentLanguage", "defaultPropertiesFileLanguage");

            // currentLanguage popup field should only show translated languages
            DrawCurrentLanguagePopup(translationManager);

            // defaultPropertiesFileLanguage
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultPropertiesFileLanguage"));
            
            if (GUILayout.Button("Reload Translations"))
            {
                UpdateCurrentLanguageAndTranslations(translationManager);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateCurrentLanguageAndTranslations(TranslationManager translationManager)
        {
            translationManager.UpdateCurrentLanguageAndTranslations();
            
            // Mark all GameObjects with ITranslator component as dirty. Otherwise Unity will not redraw the scene in the editor.
            foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>())
            {
                if (gameObject.GetComponent<ITranslator>() != null)
                {
                    EditorUtility.SetDirty(gameObject);
                }
            }
        }

        private void DrawCurrentLanguagePopup(TranslationManager translationManager)
        {
            string[] translatedLanguages = translationManager.GetTranslatedLanguages()
                .Select(it => it.ToString())
                .ToArray();
            if (translatedLanguages.IsNullOrEmpty())
            {
                // No translations found. Just use the default field
                EditorGUILayout.PropertyField(serializedObject.FindProperty("currentLanguage"));
                return;
            }
            
            int index = Array.IndexOf(translatedLanguages, translationManager.currentLanguage.ToString());
            if (index < 0)
            {
                index = 0;
            }
            
            int newIndex = EditorGUILayout.Popup("Current Language", index, translatedLanguages);
            if (newIndex != index)
            {
                if (Enum.TryParse(translatedLanguages[newIndex], false, out SystemLanguage newLanguage))
                {
                    translationManager.currentLanguage = newLanguage;
                    UpdateCurrentLanguageAndTranslations(translationManager);
                }
                else
                {
                    Debug.LogError($"Could not parse new value for currentLanguage: {translatedLanguages[newIndex]}");
                }
            }
        }

        /// Draws the default inspector without certain properties.
        /// Can be used to draw certain properties in a custom way and still keep the rest of the default inspector.
        private static void DrawDefaultInspectorWithoutProperties(SerializedObject obj, params string[] ignoreProperties)
        {
            EditorGUI.BeginChangeCheck();
            obj.Update();
            SerializedProperty iterator = obj.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                if (ignoreProperties.Contains(iterator.propertyPath))
                {
                    continue;
                }

                if (iterator.propertyPath == "m_Script")
                {
                    GUI.enabled = false;
                }

                EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
                enterChildren = false;

                if (iterator.propertyPath == "m_Script")
                {
                    GUI.enabled = true;
                }
            }
            obj.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}
