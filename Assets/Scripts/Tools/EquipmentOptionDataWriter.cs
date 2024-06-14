using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentOptionDataWriter : MonoBehaviour
{
    string jsonpath = "Resources/Data/EquipmentOptionData.json";

    public List<EquipmentType> capableOfEquipmentType;
    public EquipmentRarity requireRarity;
    public StatusType optionType;
    public string prefix;
    public int weight;
    public int intParam1;
    public int intParam2;
    public float floatParam1;
    public float floatParam2;

    public void WriteData()
    {
        EquipmentOptionData newData = new EquipmentOptionData()
        {
            weight = weight,
            requireRarity = requireRarity,
            capableOfEquipmentType = capableOfEquipmentType,
            optionType = optionType,
            prefix = prefix,
            intParam1 = intParam1,
            intParam2 = intParam2,
            floatParam1 = floatParam1,
            floatParam2 = floatParam2,
        };
        JsonDataWriter.WriteData(jsonpath, newData);
    }
}
