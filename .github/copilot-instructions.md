# GitHub Copilot Instructions for AR Sticky Notes Project

## Project Overview
This is a Unity XR (AR) application for creating and managing sticky notes in augmented reality. The app targets both Android mobile devices and desktop platforms, using Unity UI Toolkit for responsive cross-platform UI.

**Key Features:**
- Create, edit, and delete notes
- View notes in a scrollable list
- Display notes on an AR whiteboard in 3D space
- Toast notification system for user feedback
- Persistent storage of notes

## Architecture & Design Patterns

### Core Principles
- **Singleton Pattern**: Use for managers (e.g., `UIDOCUMENT_ToastManager.Instance`)
- **MVC Pattern**: Separate models (`ARStickyNotes.Models`), views (UI Toolkit), and controllers (MonoBehaviour scripts)
- **Service Layer**: Use dedicated service classes for business logic (e.g., `NoteManager`)
- **Dependency Injection**: Prefer SerializeField for dependencies over FindObjectOfType
- **Static Facades**: Create static utility classes for easy access to singletons (e.g., `UIDOCUMENT_ToastNotifier`)

### Namespace Convention
```csharp
ARStickyNotes.{Domain}
// Examples:
ARStickyNotes.Models      // Data models (Note, ToastData, etc.)
ARStickyNotes.Services    // Business logic (NoteManager)
ARStickyNotes.UI          // UI controllers (ARStickyNotesController, ToastManager)
ARStickyNotes.Utilities   // Utility classes (ErrorReporter, ARSpawner)
```

### File Organization
```
Assets/
├── ARStickyNotes/
│   ├── Models/               // Data models
│   ├── Services/             // Business logic
│   ├── UI/                   // UI controllers
│   └── Utilities/            // Utility classes
├── Scenes/                   // Unity scenes
├── Prefabs/                  // Prefabs for AR objects
├── Materials/                // Materials for AR objects
└── Textures/                 // Textures for AR objects
```

## Unity-Specific Best Practices

### MonoBehaviour Lifecycle
```csharp
// Correct order and usage:
private void Awake()    // Initialize self, create singletons
private void Start()    // Initialize with dependencies, subscribe to events
private void OnEnable() // Subscribe to events
private void OnDisable() // Unsubscribe from events
private void OnDestroy() // Cleanup, unsubscribe global events
```

### UI Toolkit (2025 Standards)

#### USS vs Inline Styles - CRITICAL LESSONS LEARNED

**The Reality of USS in Unity UI Toolkit:**
- USS styles are NOT reliably applied when cloning VisualTreeAssets
- Inline C# styles ALWAYS override USS (higher specificity)
- For production code, use inline styles for layout-critical properties

```csharp
// ✓ CORRECT: Set all critical styles inline during initialization
private void EnsureToastUI()
{
    if (toastRoot == null)
    {
        // Clone and query elements first
        toastRoot = toastUXML.CloneTree().Q<VisualElement>("toast-root");
        
        // Then immediately set ALL critical inline styles
        toastRoot.style.position = Position.Absolute;
        toastRoot.style.left = new StyleLength(new Length(5, LengthUnit.Percent));
        toastRoot.style.right = new StyleLength(new Length(5, LengthUnit.Percent));
        toastRoot.style.top = new StyleLength(80);
        toastRoot.style.height = new StyleLength(new Length(15, LengthUnit.Percent));
        // ... all other critical layout properties
        
        // Set display last, after all layout is configured
        toastRoot.style.display = DisplayStyle.None;
    }
}
```

**USS Should Only Be Used For:**
- Initial structure in UXML (as fallback only)
- Documentation of intended styles
- Non-critical visual properties

**Always Set Inline for:**
- Position (absolute, relative, etc.)
- Size (width, height, min/max)
- Layout properties (top, left, right, bottom)
- Padding and margins
- Any property that affects touch targets or visibility

#### Dynamic Content Height

```csharp
// Use percentage-based height for responsive containers
toastRoot.style.height = new StyleLength(new Length(15, LengthUnit.Percent));

// For fixed-height containers, use pixels
toastRoot.style.height = new StyleLength(140);

// Never rely on auto-height for critical UI elements
// Unity UI Toolkit auto-sizing is unreliable
```

#### Text Button Best Practices (Android Compatibility)

```csharp
// ✓ ALWAYS use simple ASCII for button text
button.text = "X";  // Works everywhere

// ✓ Set button text in C# after cloning, not just in UXML
toastClose.text = "X"; // Override UXML default

// ✓ Use large, bold fonts with high contrast
button.style.fontSize = new StyleLength(48);
button.style.unityFontStyleAndWeight = FontStyle.Bold;

// ✓ Add visual background for buttons without borders
button.style.backgroundColor = new Color(0, 0, 0, 0.5f);

// ✓ Add border for better definition on all backgrounds
button.style.borderTopWidth = 2;
button.style.borderBottomWidth = 2;
button.style.borderLeftWidth = 2;
button.style.borderRightWidth = 2;
button.style.borderTopColor = Color.white;
// ... repeat for all sides

// ✓ Make circular buttons with border-radius = width/2
button.style.width = new StyleLength(60);
button.style.height = new StyleLength(60);
button.style.borderTopLeftRadius = new StyleLength(30);
button.style.borderTopRightRadius = new StyleLength(30);
button.style.borderBottomLeftRadius = new StyleLength(30);
button.style.borderBottomRightRadius = new StyleLength(30);

// ✓ Zero out all padding/margin to prevent clipping
button.style.paddingLeft = 0;
button.style.paddingRight = 0;
button.style.paddingTop = 0;
button.style.paddingBottom = 0;
button.style.marginLeft = 0;
button.style.marginRight = 0;
button.style.marginTop = 0;
button.style.marginBottom = 0;
```

#### Safe Area Handling (Android)
```csharp
// Always account for notches and camera cutouts
var safeArea = Screen.safeArea;
float topSafeAreaOffset = Screen.height - (safeArea.y + safeArea.height);

// Use Mathf.Max to ensure minimum offset for desktop
element.style.top = new StyleLength(Mathf.Max(topSafeAreaOffset + 20, 80));

// Log safe area for debugging on device
Debug.Log($"Safe area: {safeArea}, Top offset: {topSafeAreaOffset}");
```

#### Absolute Positioning Within Containers

```csharp
// For complex layouts, use absolute positioning for all children
// This gives pixel-perfect control on all devices

// Parent container
container.style.position = Position.Absolute;
container.style.width = new StyleLength(new Length(90, LengthUnit.Percent));
container.style.height = new StyleLength(new Length(15, LengthUnit.Percent));

// Child 1: Top-right button
button.style.position = Position.Absolute;
button.style.top = new StyleLength(8);
button.style.right = new StyleLength(8);

// Child 2: Centered message
message.style.position = Position.Absolute;
message.style.left = 20;
message.style.right = 90; // Leave space for button
message.style.top = 50;

// Child 3: Bottom-right counter
counter.style.position = Position.Absolute;
counter.style.right = 20;
counter.style.bottom = 15;
```

## Error Handling & Logging

### Exception Handling
```csharp
// Always use ErrorReporter utility for consistent error reporting
try
{
    // Operation
}
catch (Exception ex)
{
    ErrorReporter.Report("User-friendly error message", ex);
    // Optionally show toast notification
    UIDOCUMENT_ToastNotifier.ShowErrorMessage("Operation failed");
}
```

### Null Checks
```csharp
// Required null checks in Awake/Start for SerializeField
if (uiDocument == null)
    throw new Exception("ComponentName: fieldName reference is not assigned in the inspector.");

// Use null-conditional operators for optional references
optionalComponent?.DoSomething();
```

### Debug Logging
```csharp
// Use structured logging with context
Debug.Log($"ComponentName: Action completed. Value: {value}");
Debug.LogWarning($"ComponentName: Potential issue detected. Context: {context}");
Debug.LogError($"ComponentName: Critical error. Details: {details}");
```

## Code Style & Conventions

### Naming Conventions
```csharp
// Classes: PascalCase with purpose prefix
public class UIDOCUMENT_ToastManager  // UI Document component
public class ARSpawner                // AR utility

// Private fields: camelCase
private VisualElement toastRoot;
private float displayDuration;

// SerializeField: camelCase with [Header]
[Header("UI References")]
[SerializeField] private UIDocument uiDocument;

// Constants: PascalCase or UPPER_CASE
private const float DefaultDuration = 2f;

// Public methods: PascalCase
public void ShowToast(string message)

// Private methods: PascalCase
private void HandleManualDismiss()
```

### XML Documentation
```csharp
/// <summary>
/// Brief description of what the method/class does.
/// </summary>
/// <param name="paramName">Parameter description</param>
/// <returns>Return value description</returns>
public ReturnType MethodName(ParamType paramName)
```

### UI Element Querying
```csharp
// Use Q<T> with type safety
var button = root.Q<Button>("buttonName");
var label = root.Q<Label>("labelName");

// Always null-check queried elements
if (button == null)
    throw new Exception("buttonName not found in UXML.");

// Use consistent naming: lowercase-with-hyphens in UXML
<ui:Button name="create-note-button" />
```

## Performance Optimization

### UI Toolkit
```csharp
// ✓ Cache VisualElement references during initialization
private VisualElement toastRoot;
private Label toastMessage;
private Button toastClose;

private void EnsureToastUI()
{
    if (toastRoot == null) // Only initialize once
    {
        toastRoot = toastUXML.CloneTree().Q<VisualElement>("toast-root");
        toastMessage = toastRoot.Q<Label>("toast-message");
        toastClose = toastRoot.Q<Button>("toast-close");
    }
}

// ✗ Don't query repeatedly
void Update()
{
    root.Q<Label>("label").text = "Bad"; // Never do this
}
```

### StyleLength Creation
```csharp
// Create StyleLength properly for all style values
element.style.width = new StyleLength(100); // pixels
element.style.height = new StyleLength(new Length(50, LengthUnit.Percent));

// For border radius (all corners)
element.style.borderTopLeftRadius = new StyleLength(12);
element.style.borderTopRightRadius = new StyleLength(12);
element.style.borderBottomLeftRadius = new StyleLength(12);
element.style.borderBottomRightRadius = new StyleLength(12);
```

### Coroutines
```csharp
// Store coroutine references for proper cleanup
private Coroutine currentRoutine;

public void StartOperation()
{
    if (currentRoutine != null)
        StopCoroutine(currentRoutine);
    currentRoutine = StartCoroutine(OperationCoroutine());
}

// Always clean up on disable/destroy
private void OnDisable()
{
    if (currentRoutine != null)
    {
        StopCoroutine(currentRoutine);
        currentRoutine = null;
    }
}
```

### Memory Management
```csharp
// Unsubscribe from events to prevent memory leaks
private void OnEnable() => button.clicked += OnButtonClicked;
private void OnDisable() => button.clicked -= OnButtonClicked;

// Clear collections when no longer needed
private void OnDestroy()
{
    queue.Clear();
    list.Clear();
}
```

## Testing & Validation

### Inspector Validation
```csharp
// Validate all SerializeField references in Awake
private void Awake()
{
    ValidateReferences();
}

private void ValidateReferences()
{
    if (requiredComponent == null)
        throw new Exception("Required component not assigned");
}
```

### Platform Testing
- Test all UI on both Android and Desktop (different resolutions)
- Verify touch targets are adequate for mobile (minimum 44-60px)
- Test with device notches/camera cutouts (safe area)
- Verify text rendering on Android devices (avoid special Unicode)
- Test AR features on actual devices, not just editor

### Edge Cases
```csharp
// Handle empty/null data gracefully
if (string.IsNullOrWhiteSpace(title))
    title = "(Untitled)";

// Validate array/list access
if (index >= 0 && index < items.Count)
    return items[index];
```

## Code Review Checklist

When reviewing code, verify:

### ✅ Architecture
- [ ] Follows namespace conventions
- [ ] Uses appropriate design patterns
- [ ] Separates concerns (Model/View/Controller)
- [ ] Avoids tight coupling

### ✅ Unity Specifics
- [ ] Correct MonoBehaviour lifecycle usage
- [ ] SerializeField properly validated
- [ ] Event subscriptions cleaned up
- [ ] Coroutines properly managed

### ✅ UI Toolkit
- [ ] Minimal inline style overrides
- [ ] Responsive percentage-based layouts
- [ ] Safe area handling for Android
- [ ] Cross-platform text rendering
- [ ] Touch-friendly button sizes

### ✅ Error Handling
- [ ] Try-catch for risky operations
- [ ] ErrorReporter used consistently
- [ ] User-friendly error messages
- [ ] Proper null checks

### ✅ Performance
- [ ] No style changes in Update/per-frame
- [ ] VisualElements cached, not queried repeatedly
- [ ] Coroutines cleaned up properly
- [ ] Events unsubscribed

### ✅ Code Quality
- [ ] XML documentation for public APIs
- [ ] Consistent naming conventions
- [ ] Meaningful variable names
- [ ] No magic numbers (use constants)
- [ ] Code is self-documenting

### ✅ Cross-Platform
- [ ] Tested on Android and Desktop
- [ ] Safe area handling implemented
- [ ] Touch targets adequate for mobile
- [ ] No platform-specific code without guards

## Common Anti-Patterns to Avoid

### ❌ Don't Rely on USS for Critical Layout

```csharp
// ✗ BAD: Relying on USS to position elements
// In USS: .toast-root { top: 80px; }
// In C#: (nothing)
// Result: Element may not appear or be positioned incorrectly

// ✓ GOOD: Set all critical layout in C#
toastRoot.style.position = Position.Absolute;
toastRoot.style.top = new StyleLength(80);
toastRoot.style.left = new StyleLength(new Length(5, LengthUnit.Percent));
// ... all critical properties
```

### ❌ Don't Use Special Unicode on Android

```csharp
// ✗ BAD: Special characters may not render
button.text = "✖";  // Might show as empty box on Android
button.text = "⨯";  // Might not render
button.text = "×";  // Might not render

// ✓ GOOD: Use simple ASCII
button.text = "X";
button.text = "CLOSE";
button.text = "OK";
```

### ❌ Don't Forget to Override UXML Defaults in C#

```csharp
// ✗ BAD: Assuming UXML text will render correctly
// In UXML: <ui:Button text="✖" />
// On Android: Might not display

// ✓ GOOD: Override in C# after cloning
toastClose = toastRoot.Q<Button>("toast-close");
toastClose.text = "X"; // Always override for Android compatibility
```

### ❌ Don't Ignore Safe Area

```csharp
// ✗ BAD: Fixed positioning at top
element.style.top = new StyleLength(0);

// ✓ GOOD: Account for safe area
var safeArea = Screen.safeArea;
float topOffset = Screen.height - (safeArea.y + safeArea.height);
element.style.top = new StyleLength(Mathf.Max(topOffset + 20, 80));
```

## Testing Checklist for Toast/Notification Systems

### Cross-Platform Testing
- [ ] Test on desktop (Editor and build)
- [ ] Test on Android device (multiple screen sizes)
- [ ] Test with device notch/camera cutout (Pixel, iPhone X+)
- [ ] Test in portrait and landscape (if applicable)
- [ ] Verify all text renders correctly (no empty boxes)
- [ ] Verify touch targets are adequate (minimum 60x60px)
- [ ] Test rapid-fire notifications (queue behavior)
- [ ] Test manual dismissal (close button)
- [ ] Test counter display (1/3, 2/3, 3/3)

### Visual Verification
- [ ] Background colors display correctly (Info/Success/Error)
- [ ] Text is readable on all background colors
- [ ] Close button is visible and clickable
- [ ] Counter is visible in bottom-right
- [ ] Message text wraps properly (long messages)
- [ ] Fade in/out animations are smooth
- [ ] Safe area padding is correct on notched devices

### Functional Testing
- [ ] Multiple toasts queue properly (sequential display)
- [ ] Manual dismiss works (close button)
- [ ] Auto-dismiss works after timeout
- [ ] Callbacks fire correctly (onDismiss)
- [ ] No memory leaks (event unsubscription)
- [ ] No exceptions in logs
