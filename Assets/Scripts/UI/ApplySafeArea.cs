using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ApplySafeArea : MonoBehaviour
{
    UIDocument doc;
    VisualElement root, topBar, bottomBar;

    void OnEnable()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        // Dynamically detect top and bottom bars based on active UXML
        topBar = root.Q(className: "top-actions-element") ?? root.Q(className: "note-editor-actions");
        bottomBar = root.Q(className: "bottom-bar") ?? root.Q(className: "note-content");

        root.RegisterCallback<GeometryChangedEvent>(_ => Apply());
        Apply();
    }

    void Apply()
    {
        var sa = Screen.safeArea;

        float topInsetPx = Screen.height - (sa.y + sa.height);
        float bottomInsetPx = sa.y;
        float leftInsetPx = sa.x;
        float rightInsetPx = Screen.width - (sa.x + sa.width);

        float topPct = 100f * topInsetPx / Screen.height;
        float bottomPct = 100f * bottomInsetPx / Screen.height;
        float leftPct = 100f * leftInsetPx / Screen.width;
        float rightPct = 100f * rightInsetPx / Screen.width;

        if (topBar != null) topBar.style.marginTop = new Length(topPct, LengthUnit.Percent);
        if (bottomBar != null) bottomBar.style.marginBottom = new Length(bottomPct, LengthUnit.Percent);

        root.style.paddingLeft = new Length(leftPct, LengthUnit.Percent);
        root.style.paddingRight = new Length(rightPct, LengthUnit.Percent);
    }
}