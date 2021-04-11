using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProTrans
{
    [CustomEditor(typeof(TranslationManager))]
    public class TranslationManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            TranslationManager i18nManager = target as TranslationManager;
            DrawDefaultInspector();

            if (GUILayout.Button("Reload Translations"))
            {
                i18nManager.UpdateCurrentLanguageAndTranslations();
            }
        }
    }
}
