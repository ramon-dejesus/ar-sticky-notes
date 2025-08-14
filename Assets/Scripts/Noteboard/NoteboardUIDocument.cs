using System;
using System.Collections.Generic;
using System.Linq;
using ARStickyNotes.Models;
using ARStickyNotes.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityUtils;

public class NoteboardUIDocument : MonoBehaviour
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

    [Tooltip("Pixels per world unit. Determines the real-world size of the panel.")]
    [SerializeField] float pixelsPerUnit = 500.0f;

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
        SetListView();
    }

    public void LoadNotes(NoteList notes)
    {
        if (uiDocument.rootVisualElement == null)
        {
            uiDocument.visualTreeAsset = visualTreeAsset;
        }
        notesListView.itemsSource = notes.Items;
        notesListView.RefreshItems();
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

        transform.localScale = new Vector3(panelWidth / pixelsPerUnit, panelHeight / pixelsPerUnit, 1.0f);
    }

    void SetListView()
    {
        notesListView = uiDocument.rootVisualElement.Q<ListView>("notesListView");

        if (notesListView == null)
        {
            throw new Exception("Notes ListView was not found.");
        }
        notesListView.makeItem = () =>
        {
            var row = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };
            var title = new Label
            {
                name = "lblTitle",
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    whiteSpace = WhiteSpace.NoWrap,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis
                }
            };
            var createdAt = new Label
            {
                name = "lblCreatedAt",
                style =
                {
                    flexGrow = 0,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    marginLeft = 10,
                    whiteSpace = WhiteSpace.NoWrap,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis
                }
            };
            row.Add(title);
            row.Add(createdAt);
            return row;
        };
        notesListView.bindItem = (element, i) =>
        {
            var note = notesListView.itemsSource[i] as Note;
            var createdAt = element.Q<Label>("lblCreatedAt");
            var title = element.Q<Label>("lblTitle");
            title.text = note.Title ?? "(Untitled)";
            createdAt.text = note.CreatedAt.ToString("MM/dd/yyyy hh:mm:ss tt");
        };
        //notesListView.selectionChanged += OnSelectionChange;
    }

    private void OnSelectionChange(IEnumerable<object> item)
    {
        try
        {
            var note = item.FirstOrDefault() as Note;
            ToastNotifier.ShowSuccessMessage($"Selected note: {note?.Title ?? "None"}");
        }
        catch (Exception e)
        {
            ErrorReporter.Report("An error occurred while selecting a note.", e, false);
        }
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
