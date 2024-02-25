using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ProTrans;
using TMPro;
using UnityEngine;

public class LanguageChooser : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private List<CultureInfo> translatedCultureInfos;

    private void Awake()
    {
        translatedCultureInfos = Translation.GetTranslatedCultureInfos();
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        dropdown.options = translatedCultureInfos
            .Select(cultureInfo => new TMP_Dropdown.OptionData(cultureInfo.ToString()))
            .ToList();
        dropdown.value = translatedCultureInfos.IndexOf(TranslationConfig.Singleton.CurrentCultureInfo);

        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index)
    {
        TranslationConfig.Singleton.CurrentCultureInfo = translatedCultureInfos[index];
        foreach (TranslatedText translatedText in FindObjectsByType<TranslatedText>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            translatedText.UpdateTranslation();
        }
    }
}
