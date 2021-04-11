using System;
using System.Collections;
using System.Collections.Generic;
using ProTrans;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GetTranslationExampleText : AbstractTranslatorBehaviour
{
    public string mood = "happy";
    public int age = 20;
    
    public override void UpdateTranslation()
    {
        // Static method to access translations.
        // In this example, a generated constant is used to access the translation (avoids typos and makes refactoring easier). 
        // After the translation key, further arguments can be given in the form [key1, value1, key2, value2, ...].
        GetComponent<Text>().text = TranslationManager.GetTranslation(R.String.sampleScene_staticMethodAccessExample,
            "mood", mood,
            "age", age);
    }
}
