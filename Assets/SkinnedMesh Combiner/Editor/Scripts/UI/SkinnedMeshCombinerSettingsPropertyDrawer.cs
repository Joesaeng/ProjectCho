using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace SMC.Editor
{
    [CustomPropertyDrawer(typeof(SkinnedMeshCombinerSettings))]
    internal sealed class SkinnedMeshCombinerSettingsPropertyDrawer : PropertyDrawer
    {
        private const string ASSET_PATH = "UI/SkinnedMeshCombinerSettingsPropertyDrawer";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>(ASSET_PATH);
            if (visualTreeAsset)
            {
                VisualElement root = visualTreeAsset.CloneTree();

                root.Q<MaskField>("uv-channels-field").choices = new List<string>(Enum.GetNames(typeof(UVChannels)));

                return root;
            }

            return base.CreatePropertyGUI(property);
        }
    }
}
