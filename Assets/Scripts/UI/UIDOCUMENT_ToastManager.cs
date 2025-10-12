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
                try // Best Practice: Error handling
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

                    toastClose.clicked += HandleManualDismiss;

                    // Add to rootVisualElement if not already present
                    if (!root.Contains(toastRoot))
                        root.Add(toastRoot);

                    toastRoot.style.display = DisplayStyle.None;
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
            // Set background color dynamically
            Color color;
            switch (type)
            {
                case ToastType.Success:
                    color = new Color(0.2f, 0.8f, 0.2f, 0.95f); // Green
                    break;
                case ToastType.Error:
                    color = new Color(0.9f, 0.2f, 0.2f, 0.95f); // Red
                    break;
                case ToastType.Info:
                default:
                    color = new Color(0f, 0.2f, 0.5f, 0.95f); // Blue
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