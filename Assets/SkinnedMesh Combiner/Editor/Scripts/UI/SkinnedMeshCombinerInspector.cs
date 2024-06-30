using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace SMC.Editor
{
    /// <summary>
    /// Editor-only class that defines custom inspector functionality when a SkinnedMeshCombinerComponent is selected
    /// </summary>
    [CustomEditor(typeof(SkinnedMeshCombinerComponent))]
    internal sealed class SkinnedMeshCombinerInspector : UnityEditor.Editor
    {
        private const string
            INSPECTOR_WARNING_CONTENT_NAME = "inspector-warning-content",
            INSPECTOR_WARNING_ICON_NAME = "inspector-warning-icon",
            MESH_RENDERER_LIST_NAME = "mesh-renderer-list",
            ROOT_RENDERER_FIELD_NAME = "root-mesh-renderer-field",
            TOOLBAR_AUTODETECT_BUTTON_NAME = "toolbar-auto-detect-button",
            TOOLBAR_CLEAR_BUTTON_NAME = "toolbar-clear-button",
            TOOLBAR_COMBINE_BUTTON_NAME = "toolbar-combine-button",
            USS_INSPECTOR_ERROR_CLASS = "inspector-warning-icon_error",
            USS_INSPECTOR_WARNING_CLASS = "inspector-warning-icon_warning";

        private VisualElement rootVisualElement;
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        public override VisualElement CreateInspectorGUI()
        {
            if (visualTreeAsset)
            {
                rootVisualElement = visualTreeAsset.CloneTree();

                ObjectField rootMeshRendererField = rootVisualElement.Q<ObjectField>(ROOT_RENDERER_FIELD_NAME);
                ListView meshRendererList = rootVisualElement.Q<ListView>(MESH_RENDERER_LIST_NAME);

                meshRendererList.Q<Foldout>().viewDataKey = "SMC-inspector-renderer-list-foldout";
                meshRendererList.itemsAdded += (_) => RefreshWarnings();
                meshRendererList.itemsRemoved += (_) => RefreshWarnings();
                rootMeshRendererField.RegisterValueChangedCallback((_) => RefreshWarnings());

                Toolbar toolbar = rootVisualElement.Q<Toolbar>();
                if (toolbar != null)
                {
                    ToolbarButton
                        autoDetectButton = toolbar.Q<ToolbarButton>(TOOLBAR_AUTODETECT_BUTTON_NAME),
                        clearButton = toolbar.Q<ToolbarButton>(TOOLBAR_CLEAR_BUTTON_NAME),
                        combineButton = toolbar.Q<ToolbarButton>(TOOLBAR_COMBINE_BUTTON_NAME);

                    if (autoDetectButton != null)
                        autoDetectButton.clicked += AutoDetectSettings;

                    if (clearButton != null)
                        clearButton.clicked += ClearSettings;

                    if (serializedObject != null && serializedObject.targetObject is SkinnedMeshCombinerComponent smc)
                    {
                        if (combineButton != null)
                            combineButton.clicked += () => smc.Combine();

                        ToolbarMenu toolbarMenu = toolbar.Q<ToolbarMenu>();
                        if (toolbarMenu != null)
                        {
                            toolbarMenu.menu.AppendAction
                            (
                                "Combine to New",
                                (_) => smc.Combine(CombineMode.CombineToNew)
                            );
                            toolbarMenu.menu.AppendAction
                            (
                                "Combine to Prefab",
                                (_) => smc.Combine(CombineMode.CombineToPrefab)
                            );
                        }
                    }
                }

                rootVisualElement.TrackPropertyValue
                (
                    serializedObject.FindProperty("m_RootSkinnedMeshRenderer"),
                    (_) => RefreshWarnings()
                );
                rootVisualElement.TrackPropertyValue
                (
                    serializedObject.FindProperty("m_Settings.m_CreateVirtualBones"),
                    (_) => RefreshWarnings()
                );

                RefreshWarnings();

                return rootVisualElement;
            }

            return base.CreateInspectorGUI();
        }

        // Finds and assign all child renderers, analyzing them to determine best combiner settings
        private void AutoDetectSettings()
        {
            if (serializedObject != null && serializedObject.targetObject is SkinnedMeshCombinerComponent smc)
            {
                List<Renderer> renderers = new List<Renderer>();
                SkinnedMeshCombinerSettings settings = new SkinnedMeshCombinerSettings();

                UVChannels uvChannels = 0;
                bool combineBlendshapes = false, combineBoneweights = false, combineTextures = false;
                foreach (SkinnedMeshRenderer smr in smc.GetComponentsInChildren<SkinnedMeshRenderer>(false))
                {
                    if (smr.sharedMesh == null) continue;

                    // Change UV channels flags based on presence of different UV maps
                    if (smr.sharedMesh.uv?.Length > 0)
                        uvChannels |= UVChannels.UV0;
                    if (smr.sharedMesh.uv2?.Length > 0)
                        uvChannels |= UVChannels.UV1;
                    if (smr.sharedMesh.uv3?.Length > 0)
                        uvChannels |= UVChannels.UV2;
                    if (smr.sharedMesh.uv4?.Length > 0)
                        uvChannels |= UVChannels.UV3;
                    if (smr.sharedMesh.uv5?.Length > 0)
                        uvChannels |= UVChannels.UV4;
                    if (smr.sharedMesh.uv6?.Length > 0)
                        uvChannels |= UVChannels.UV5;
                    if (smr.sharedMesh.uv7?.Length > 0)
                        uvChannels |= UVChannels.UV6;
                    if (smr.sharedMesh.uv8?.Length > 0)
                        uvChannels |= UVChannels.UV7;

                    if (!combineBlendshapes && smr.sharedMesh.blendShapeCount > 0)
                        combineBlendshapes = true;

                    if (!combineBoneweights && smr.sharedMesh.boneWeights.Length > 0)
                        combineBoneweights = true;

                    if (!combineTextures && smr.sharedMaterial != null && smr.sharedMaterial.mainTexture != null)
                        combineTextures = true;

                    if (smc.RootSkinnedMeshRenderer)
                        renderers.Add(smr);
                    else
                        smc.RootSkinnedMeshRenderer = smr;
                }
                foreach (MeshRenderer mr in smc.GetComponentsInChildren<MeshRenderer>(false))
                {
                    // To be considered a valid renderer to combine, the MeshRenderer must also have a MeshFilter
                    if (!mr.TryGetComponent(out MeshFilter mf) || mf.sharedMesh == null) continue;

                    // Change UV channels flags based on presence of different UV maps
                    if (mf.sharedMesh.uv?.Length > 0)
                        uvChannels |= UVChannels.UV0;
                    if (mf.sharedMesh.uv2?.Length > 0)
                        uvChannels |= UVChannels.UV1;
                    if (mf.sharedMesh.uv3?.Length > 0)
                        uvChannels |= UVChannels.UV2;
                    if (mf.sharedMesh.uv4?.Length > 0)
                        uvChannels |= UVChannels.UV3;
                    if (mf.sharedMesh.uv5?.Length > 0)
                        uvChannels |= UVChannels.UV4;
                    if (mf.sharedMesh.uv6?.Length > 0)
                        uvChannels |= UVChannels.UV5;
                    if (mf.sharedMesh.uv7?.Length > 0)
                        uvChannels |= UVChannels.UV6;
                    if (mf.sharedMesh.uv8?.Length > 0)
                        uvChannels |= UVChannels.UV7;

                    if (!combineTextures && mr.sharedMaterial != null && mr.sharedMaterial.mainTexture != null)
                        combineTextures = true;

                    // If mesh renderers are present, virtual bones may need to be created
                    settings.m_CreateVirtualBones = true;

                    renderers.Add(mr);
                }
                settings.m_UVChannels = uvChannels;
                settings.m_CombineBlendshapes = combineBlendshapes;
                settings.m_CombineBones = combineBoneweights;
                settings.m_CombineTextures = combineTextures;

                smc.SetRenderersToCombine(new List<Renderer>(renderers));
                smc.Settings = settings;
            }
        }

        // Reset all fields to defaults
        private void ClearSettings()
        {
            if (serializedObject != null && serializedObject.targetObject is SkinnedMeshCombinerComponent smc)
            {
                smc.RootSkinnedMeshRenderer = null;
                smc.SetRenderersToCombine(new List<Renderer>());
                smc.Settings = default;
            }
        }

        private void RefreshWarnings()
        {
            if (rootVisualElement == null) { return; }

            ToolbarButton combineButton = rootVisualElement.Q<ToolbarButton>(TOOLBAR_COMBINE_BUTTON_NAME);
            VisualElement inspectorWarningContentElement = rootVisualElement.Q<VisualElement>(INSPECTOR_WARNING_CONTENT_NAME);
            if (inspectorWarningContentElement != null)
            {
                VisualElement inspectorWarningIcon = inspectorWarningContentElement.Q<VisualElement>(INSPECTOR_WARNING_ICON_NAME);
                if (inspectorWarningIcon != null)
                {
                    inspectorWarningIcon.RemoveFromClassList(USS_INSPECTOR_ERROR_CLASS);
                    inspectorWarningIcon.RemoveFromClassList(USS_INSPECTOR_WARNING_CLASS);
                }

                if (serializedObject != null && serializedObject.targetObject is SkinnedMeshCombinerComponent smc)
                {
                    int errorCode = smc.CanCombine(out string errorString);

                    if (errorCode != 0)
                    {
                        switch (errorCode)
                        {
                            case 1:
                                inspectorWarningIcon?.AddToClassList(USS_INSPECTOR_WARNING_CLASS);

                                break;
                            case 2:
                                inspectorWarningIcon?.AddToClassList(USS_INSPECTOR_ERROR_CLASS);

                                combineButton?.SetEnabled(false);

                                break;
                        }

                        Label inspectorWarningLabel = inspectorWarningContentElement.Q<Label>();
                        if (inspectorWarningLabel != null)
                            inspectorWarningLabel.text = errorString;

                        inspectorWarningContentElement.style.display = DisplayStyle.Flex;

                        return;
                    }
                }

                inspectorWarningContentElement.style.display = DisplayStyle.None;
            }

            combineButton?.SetEnabled(true);
        }
    }
}
