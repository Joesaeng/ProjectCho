using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BaseSpellDataWriter))]
public class SpellDataWriterButtons : Editor
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

[CustomEditor(typeof(ProjectileDataWriter))]
public class ProjectileDataWriterButtons : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ProjectileDataWriter writer = (ProjectileDataWriter)target;
        if (GUILayout.Button("Write Data"))
        {
            writer.WriteData();
        }
    }
}

[CustomEditor(typeof(AOEEffectDataWriter))]
public class AOEEffectDataWriterButtons : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AOEEffectDataWriter writer = (AOEEffectDataWriter)target;
        if (GUILayout.Button("Write Data"))
        {
            writer.WriteData();
        }
    }
}

[CustomEditor(typeof(BaseEnemyDataWriter))]
public class BaseEnemyDataWriterButtons : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BaseEnemyDataWriter writer = (BaseEnemyDataWriter)target;
        if (GUILayout.Button("Write Data"))
        {
            writer.WriteData();
        }
    }
}

[CustomEditor(typeof(SpellUpgradeDataWriter))]
public class SpellUpgradeDataWriterButtons : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SpellUpgradeDataWriter writer = (SpellUpgradeDataWriter)target;
        if (GUILayout.Button("Write Data"))
        {
            writer.WriteData();
        }
    }
}
