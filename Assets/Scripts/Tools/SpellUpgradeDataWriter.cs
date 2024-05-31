using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpellUpgradeDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/SpellUpgradeData.json";

    public int spellId;
    public List<SpellUpgradeData> datas;

    public void WriteData()
    {
        SpellUpgradeDatas newData = new SpellUpgradeDatas()
        {
            spellId = spellId,
            spellUpgradeDatas = datas
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}
