using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseSpellDataWriter))]
public class DataWriterButtons : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BaseSpellDataWriter writer = (BaseSpellDataWriter)target;
        if (GUILayout.Button("Write Data"))
        {
            writer.WriteData();
        }
    }
}
