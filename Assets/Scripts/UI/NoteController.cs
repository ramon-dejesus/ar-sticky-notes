using System;
using System.Collections.Generic;
using System.Linq;
using ARStickyNotes.Models;
using ARStickyNotes.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityUtils;


public class NoteController : MonoBehaviour
{
    #region Fields

    const string k_transparentShader = "Unlit/Transparent";
    const string k_textureShader = "Unlit/Texture";
    const string k_mainTex = "_MainTex";
    static readonly int MainTex = Shader.PropertyToID(k_mainTex);

    [Header("Panel Configuration")]
    [Tooltip("Width of the panel in pixels.")]
    [SerializeField] int panelWidth = 1280;

    [Tooltip("Height of the panel in pixels.")]
    [SerializeField] int panelHeight = 720;

    [Tooltip("Scale of the panel (like zoom in a browser).")]
    [SerializeField] float panelScale = 1.0f;

    //[Tooltip("Pixels per world unit. Determines the real-world size of the panel.")]
    //[SerializeField] float pixelsPerUnit = 500.0f;

    [Header("Dependencies")]
    [Tooltip("Visual tree asset for this panel.")]
    [SerializeField] VisualTreeAsset visualTreeAsset;

    [Tooltip("PanelSettings prefab instance.")]
    [SerializeField] PanelSettings panelSettingsAsset;

    [Tooltip("RenderTexture prefab instance.")]
    [SerializeField] RenderTexture renderTextureAsset;

    MeshRenderer meshRenderer;
    UIDocument uiDocument;
    PanelSettings panelSettings;
    RenderTexture renderTexture;
    Material material;

    private ListView notesListView;

    #endregion

    void Awake()
    {
        InitializeComponents();
        BuildPanel();
        SetControls();
    }

    public void LoadControls(Note item)
    {
        if (uiDocument.rootVisualElement == null)
        {
            uiDocument.visualTreeAsset = visualTreeAsset;
        }
        var lbl = uiDocument.rootVisualElement.Q<Label>("noteTitleLabel");
        var deleteBtn = uiDocument.rootVisualElement.Q<Button>("deleteNoteButton");
        var saveBtn = uiDocument.rootVisualElement.Q<Button>("saveNoteButton");
        if (lbl == null || deleteBtn == null || saveBtn == null)
        {
            throw new Exception("Note controls were not found.");
        }
        lbl.text = item.Title;
        deleteBtn.clicked += () => DeleteNote(item);
        saveBtn.clicked += () => SaveNote(item);
    }

    private void SaveNote(Note item)
    {
        ToastNotifier.ShowSuccessMessage($"Save note: {item?.Title ?? "None"}");
    }

    private void DeleteNote(Note item)
    {
        ToastNotifier.ShowSuccessMessage($"Delete note: {item?.Title ?? "None"}");
    }

    void InitializeComponents()
    {
        InitializeMeshRenderer();
        MeshFilter meshFilter = gameObject.GetOrAdd<MeshFilter>();
        meshFilter.sharedMesh = GetQuadMesh();
    }

    void InitializeMeshRenderer()
    {
        meshRenderer = gameObject.GetOrAdd<MeshRenderer>();
        meshRenderer.sharedMaterial = null;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        meshRenderer.lightProbeUsage = LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    void BuildPanel()
    {
        CreateRenderTexture();
        CreatePanelSettings();
        CreateUIDocument();
        CreateMaterial();

        SetMaterialToRenderer();
        SetPanelSize();
    }

    void CreateRenderTexture()
    {
        var descriptor = renderTextureAsset.descriptor;
        descriptor.width = panelWidth;
        descriptor.height = panelHeight;
        renderTexture = new RenderTexture(descriptor)
        {
            name = name + "RenderTexture"
        };
    }

    void CreatePanelSettings()
    {
        panelSettings = Instantiate(panelSettingsAsset);
        panelSettings.targetTexture = renderTexture;
        panelSettings.clearColor = true;
        panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
        panelSettings.scale = panelScale;
        panelSettings.name = name + "PanelSettings";
    }

    void CreateUIDocument()
    {
        uiDocument = gameObject.GetOrAdd<UIDocument>();
        uiDocument.panelSettings = panelSettings;
        uiDocument.visualTreeAsset = visualTreeAsset;
    }

    void CreateMaterial()
    {
        string shaderName = panelSettings.colorClearValue.a < 1.0f ? k_transparentShader : k_textureShader;
        material = new Material(Shader.Find(shaderName));
        material.SetTexture(MainTex, renderTexture);
    }

    void SetMaterialToRenderer()
    {
        if (meshRenderer != null)
        {
            meshRenderer.sharedMaterial = material;
        }
    }

    void SetPanelSize()
    {
        if (renderTexture != null && (renderTexture.width != panelWidth || renderTexture.height != panelHeight))
        {
            renderTexture.Release();
            renderTexture.width = panelWidth;
            renderTexture.height = panelHeight;
            renderTexture.Create();

            uiDocument?.rootVisualElement?.MarkDirtyRepaint();
        }

        //transform.localScale = new Vector3(panelWidth / pixelsPerUnit, panelHeight / pixelsPerUnit, 1.0f);
    }

    void SetControls()
    {
        // Set up controls for the UI Document.
    }
    static Mesh GetQuadMesh()
    {
        GameObject tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Mesh quadMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tempQuad);

        return quadMesh;
    }

    void DestroyGeneratedAssets()
    {
        if (uiDocument) Destroy(uiDocument);
        if (renderTexture) Destroy(renderTexture);
        if (panelSettings) Destroy(panelSettings);
        if (material) Destroy(material);
    }

    void OnDestroy()
    {
        DestroyGeneratedAssets();
    }
}
