using UnityEngine;
using UnityEngine.UIElements;

[DefaultExecutionOrder(-1000)]
public class ConfigurePanelSettings : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] PanelSettings panelSettings;
    [SerializeField] Vector2Int referenceResolution = new Vector2Int(1080, 1920);
    [Range(0f, 1f)] public float match = 0.5f;

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        if (!uiDocument || !panelSettings) return;

        panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution = referenceResolution;
        panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
        panelSettings.match = match;

        uiDocument.panelSettings = panelSettings;
    }
}