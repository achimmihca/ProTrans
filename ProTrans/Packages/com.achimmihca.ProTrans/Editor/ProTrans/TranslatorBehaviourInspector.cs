using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

namespace ProTrans
{
    [CustomEditor(typeof(AbstractTranslatorBehaviour), true)]
    public class TranslatorBehaviourInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ITranslator translator = target as ITranslator;
            if (GUILayout.Button("Update Translation"))
            {
                translator.UpdateTranslation();
            }
        }
    }
}
