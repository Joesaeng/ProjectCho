using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_DefeatEnemies : UI_Base
{
    enum Texts
    {
        Text_DefeatEnemies,

        Text_Energy,
        Text_Fire,
        Text_Water,
        Text_Lightning,
        Text_Earth,
        Text_Wind,

        Text_EnergyValue,
        Text_FireValue,
        Text_WaterValue,
        Text_LightningValue,
        Text_EarthValue,
        Text_WindValue,
    }

    Dictionary<ElementType,TextMeshProUGUI> _textElementTypes = new();
    Dictionary<ElementType,TextMeshProUGUI> _textElementTypesValue = new();

    public override void Init()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
        GetText((int)Texts.Text_DefeatEnemies).text = Language.GetLanguage("CountOfDefeatEnemies");
        for (int i = 0; i < Enum.GetValues(typeof(ElementType)).Length; i++)
        {
            _textElementTypes[(ElementType)i] = GetText((int)Util.Parse<Texts>($"Text_{(ElementType)i}"));
            _textElementTypesValue[(ElementType)i] = GetText((int)Util.Parse<Texts>($"Text_{(ElementType)i}Value"));
        }

    }

    public void SetDefeatEnemies(Dictionary<ElementType, int> defeatEnemies)
    {
        foreach (var elementTypeEnemyPair in defeatEnemies)
        {
            _textElementTypes[elementTypeEnemyPair.Key].text = Language.GetLanguage(elementTypeEnemyPair.Key.ToString());
            _textElementTypesValue[elementTypeEnemyPair.Key].text = elementTypeEnemyPair.Value.ToString();
        }
    }
}
