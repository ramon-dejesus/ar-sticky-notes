using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ARStickyNotes.Models; // For ToastData and ToastType

namespace ARStickyNotes.UI
{

    /// <summary>
    /// Manages toast notifications using UI Toolkit (UIDocument).
    /// Provides queueing, close button, counter, and dynamic styling.
    /// </summary>
    public class UIDOCUMENT_ToastManager : MonoBehaviour
    {
        public static UIDOCUMENT_ToastManager Instance { get; private set; }

        [Header("UI Toolkit References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset toastUXML;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeDuration = 0.3f;

        private readonly Queue<ToastData> toastQueue = new();
        private VisualElement toastRoot;
        private Label toastMessage;
        private Label toastCounter;
        private Button toastClose;
        private Coroutine currentToastRoutine;
        private int currentBatchTotal = 0;
        private int currentBatchProcessed = 0;
        private bool dismissedManually = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Best Practice: Inspector assignment null checks
            if (uiDocument == null)
                throw new Exception("UIDOCUMENT_ToastManager: uiDocument reference is not assigned in the inspector.");
            if (toastUXML == null)
                throw new Exception("UIDOCUMENT_ToastManager: toastUXML reference is not assigned in the inspector.");
        }

        /// <summary>
        /// Show a toast message (info by default).
        /// </summary>
        public void ShowToast(string message, ToastType type = ToastType.Info, Action onDismiss = null)
        {
            toastQueue.Enqueue(new ToastData(message, type, onDismiss));
            if (currentToastRoutine == null)
                currentToastRoutine = StartCoroutine(ToastProcessor());
        }

        private IEnumerator ToastProcessor()
        {
            while (toastQueue.Count > 0)
            {
                currentBatchTotal = toastQueue.Count;
                currentBatchProcessed = 0;

                while (toastQueue.Count > 0 && currentBatchProcessed < currentBatchTotal)
                {
                    currentBatchProcessed++;
                    var toast = toastQueue.Dequeue();
                    yield return StartCoroutine(ShowToastCoroutine(toast, currentBatchProcessed, currentBatchTotal));
                }
            }
            currentToastRoutine = null;
            currentBatchTotal = 0;
            currentBatchProcessed = 0;
        }

        private IEnumerator ShowToastCoroutine(ToastData toast, int currentIndex, int totalCount)
        {
            try
            {
                EnsureToastUI();

                // Set message and counter
                toastMessage.text = toast.Message;
                toastCounter.text = (totalCount > 1) ? $"{currentIndex}/{totalCount}" : "";

                // Set background color based on toast type
                SetToastBackground(toast.Type);

                // Show and fade in
                toastRoot.style.display = DisplayStyle.Flex;
                toastRoot.style.opacity = 0f;
            }
            catch (Exception ex)
            {
                Debug.LogError($"UIDOCUMENT_ToastManager: Error displaying toast: {ex}");
                yield break;
            }

            yield return FadeVisualElement(toastRoot, 0f, 1f, fadeDuration);

            dismissedManually = false;

            // Wait for display duration or until manually dismissed
            float elapsed = 0f;
            while (elapsed < displayDuration && !dismissedManually)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return FadeVisualElement(toastRoot, 1f, 0f, fadeDuration);
            toastRoot.style.display = DisplayStyle.None;

            toast.OnDismiss?.Invoke();
        }

        private void EnsureToastUI()
        {
            if (toastRoot == null)
            {
                try
                {
                    var root = uiDocument.rootVisualElement;
                    toastRoot = toastUXML.CloneTree().Q<VisualElement>("toast-root");
                    toastMessage = toastRoot.Q<Label>("toast-message");
                    toastCounter = toastRoot.Q<Label>("toast-counter");
                    toastClose = toastRoot.Q<Button>("toast-close");

                    if (toastMessage == null)
                        throw new Exception("toast-message Label not found in ToastUXML.");
                    if (toastCounter == null)
                        throw new Exception("toast-counter Label not found in ToastUXML.");
                    if (toastClose == null)
                        throw new Exception("toast-close Button not found in ToastUXML.");

                    // Change button text to simple "X" for better Android compatibility
                    toastClose.text = "X";

                    toastClose.clicked += HandleManualDismiss;

                    // Add to rootVisualElement
                    if (!root.Contains(toastRoot))
                        root.Add(toastRoot);
                    else
                    {
                        root.Remove(toastRoot);
                        root.Add(toastRoot);
                    }

                    // Apply safe area padding for notch/camera cutout on Android
                    var safeArea = Screen.safeArea;
                    float topSafeAreaOffset = Screen.height - (safeArea.y + safeArea.height);
                    
                    // Force USS to apply by manually setting critical positioning
                    toastRoot.style.position = Position.Absolute;
                    toastRoot.style.left = new StyleLength(new Length(5, LengthUnit.Percent));
                    toastRoot.style.right = new StyleLength(new Length(5, LengthUnit.Percent));
                    toastRoot.style.top = new StyleLength(Mathf.Max(topSafeAreaOffset + 20, 80)); // Account for safe area
                    toastRoot.style.height = new StyleLength(new Length(15, LengthUnit.Percent));
                    toastRoot.style.paddingLeft = new StyleLength(20);
                    toastRoot.style.paddingRight = new StyleLength(20);
                    toastRoot.style.paddingTop = new StyleLength(20);
                    toastRoot.style.paddingBottom = new StyleLength(20);
                    toastRoot.style.borderTopLeftRadius = new StyleLength(12);
                    toastRoot.style.borderTopRightRadius = new StyleLength(12);
                    toastRoot.style.borderBottomLeftRadius = new StyleLength(12);
                    toastRoot.style.borderBottomRightRadius = new StyleLength(12);
                    toastRoot.style.display = DisplayStyle.None;
                    
                    // Force child element positioning - Close button with better Android visibility
                    toastClose.style.position = Position.Absolute;
                    toastClose.style.top = new StyleLength(8);
                    toastClose.style.right = new StyleLength(8);
                    toastClose.style.width = new StyleLength(60);
                    toastClose.style.height = new StyleLength(60);
                    toastClose.style.fontSize = new StyleLength(48);
                    toastClose.style.color = Color.white;
                    toastClose.style.backgroundColor = new Color(0, 0, 0, 0.5f); // Darker background for better contrast
                    toastClose.style.borderTopWidth = 2;
                    toastClose.style.borderBottomWidth = 2;
                    toastClose.style.borderLeftWidth = 2;
                    toastClose.style.borderRightWidth = 2;
                    toastClose.style.borderTopColor = Color.white;
                    toastClose.style.borderBottomColor = Color.white;
                    toastClose.style.borderLeftColor = Color.white;
                    toastClose.style.borderRightColor = Color.white;
                    toastClose.style.borderTopLeftRadius = new StyleLength(30);
                    toastClose.style.borderTopRightRadius = new StyleLength(30);
                    toastClose.style.borderBottomLeftRadius = new StyleLength(30);
                    toastClose.style.borderBottomRightRadius = new StyleLength(30);
                    toastClose.style.unityTextAlign = TextAnchor.MiddleCenter;
                    toastClose.style.unityFontStyleAndWeight = FontStyle.Bold;
                    toastClose.style.paddingLeft = 0;
                    toastClose.style.paddingRight = 0;
                    toastClose.style.paddingTop = 0;
                    toastClose.style.paddingBottom = 0;
                    toastClose.style.marginLeft = 0;
                    toastClose.style.marginRight = 0;
                    toastClose.style.marginTop = 0;
                    toastClose.style.marginBottom = 0;
                    
                    toastMessage.style.position = Position.Absolute;
                    toastMessage.style.left = 20;
                    toastMessage.style.right = 90; // More space for larger button
                    toastMessage.style.top = 50;
                    toastMessage.style.height = 40;
                    toastMessage.style.fontSize = 40;
                    toastMessage.style.color = Color.white;
                    toastMessage.style.unityFontStyleAndWeight = FontStyle.Bold;
                    toastMessage.style.unityTextAlign = TextAnchor.MiddleLeft;
                    toastMessage.style.whiteSpace = WhiteSpace.Normal;
                    
                    toastCounter.style.position = Position.Absolute;
                    toastCounter.style.right = 20;
                    toastCounter.style.bottom = 15;
                    toastCounter.style.height = 30;
                    toastCounter.style.fontSize = 40;
                    toastCounter.style.color = new Color(1, 1, 1, 0.9f);
                    toastCounter.style.unityTextAlign = TextAnchor.MiddleRight;
                    
                    // Debug: Verify button text and safe area
                    Debug.Log($"Toast close button text: '{toastClose.text}'");
                    Debug.Log($"Safe area: {safeArea}, Top offset: {topSafeAreaOffset}");
                    Debug.Log($"Toast root classes: {string.Join(", ", toastRoot.GetClasses())}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"UIDOCUMENT_ToastManager: Error initializing toast UI: {ex}");
                    throw;
                }
            }
        }

        private void HandleManualDismiss()
        {
            dismissedManually = true;
        }

        private void SetToastBackground(ToastType type)
        {
            // Set background color dynamically - this is the ONLY inline style we set
            Color color;
            switch (type)
            {
                case ToastType.Success:
                    color = new Color(0.2039f, 0.7843f, 0.349f, 0.95f); // Green
                    break;
                case ToastType.Error:
                    color = new Color(1f, 0.3686f, 0.3176f, 0.95f); // Orange-Red
                    break;
                case ToastType.Info:
                default:
                    color = new Color(0f, 0.7098f, 1f, 0.95f); // Blue
                    break;
            }
            toastRoot.style.backgroundColor = new StyleColor(color);
        }

        private IEnumerator FadeVisualElement(VisualElement ve, float from, float to, float duration)
        {
            float elapsed = 0f;
            ve.style.opacity = from;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ve.style.opacity = Mathf.Lerp(from, to, t);
                yield return null;
            }
            ve.style.opacity = to;
        }
    }
}