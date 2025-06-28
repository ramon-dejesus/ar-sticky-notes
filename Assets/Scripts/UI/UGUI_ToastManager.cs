using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.Events;

namespace ARStickyNotes.UI
{
    /// <summary>
    /// Manages toast notifications in the AR Sticky Notes app.
    /// Provides a queue-based system for showing info, success, and error messages to the user.
    /// </summary>
    public enum ToastType { Info, Success, Error }

    public class UGUI_ToastManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance for global access.
        /// </summary>
        public static UGUI_ToastManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private TextMeshProUGUI toastText;
        [SerializeField] private Image panelBackground;
        [SerializeField] private CanvasGroup toastCanvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI toastCounterText;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeDuration = 0.3f;

        [Header("Events")]
        public UnityEvent OnToastDismissed;

        private Coroutine currentToast;
        private bool dismissedManually = false;

        /// <summary>
        /// Queue to handle multiple toast messages in sequence.
        /// </summary>
        private readonly Queue<ToastData> toastQueue = new();

        // Track batch state for correct counter display, even after manual dismiss
        private int currentBatchTotal = 0;
        private int currentBatchProcessed = 0;

        /// <summary>
        /// Initializes the singleton instance and sets up the toast panel.
        /// Handles XR-specific canvas setup and close button binding.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }            

            if (closeButton != null)
                closeButton.onClick.AddListener(HandleManualDismiss);

            toastPanel.SetActive(false);
        }

        /// <summary>
        /// Shows a toast message with info type.
        /// </summary>
        /// <param name="message">The message to display in the toast.</param>
        public void ShowToast(string message)
        {
            ShowToast(message, ToastType.Info, null);
        }

        /// <summary>
        /// Shows a toast message with a specified type.
        /// </summary>
        /// <param name="message">The message to display in the toast.</param>
        /// <param name="type">The type of toast (Info, Success, Error).</param>
        public void ShowToast(string message, ToastType type)
        {
            ShowToast(message, type, null);
        }

        /// <summary>
        /// Shows a toast message with a specified type and optional callback on dismiss.
        /// </summary>
        /// <param name="message">The message to display in the toast.</param>
        /// <param name="type">The type of toast (Info, Success, Error).</param>
        /// <param name="callback">An optional callback to invoke when the toast is dismissed.</param>
        public void ShowToast(string message, ToastType type, UnityAction callback)
        {
            toastQueue.Enqueue(new ToastData(message, type, callback));

            // Start processing the queue if not already running
            if (currentToast == null)
                currentToast = StartCoroutine(ToastProcessor());
        }

        /// <summary>
        /// Processes the toast queue, showing each toast in order.
        /// The counter always reflects the current batch size, even after manual dismiss.
        /// </summary>
        private IEnumerator ToastProcessor()
        {
            // If resuming after manual dismiss, continue the current batch
            if (currentBatchTotal > 0 && currentBatchProcessed < currentBatchTotal)
            {
                while (toastQueue.Count > 0 && currentBatchProcessed < currentBatchTotal)
                {
                    currentBatchProcessed++;
                    var toast = toastQueue.Dequeue();
                    yield return StartCoroutine(ShowToastCoroutine(toast, currentBatchProcessed, currentBatchTotal));
                }
            }

            // Start a new batch if there are still toasts left
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

            currentToast = null;
            currentBatchTotal = 0;
            currentBatchProcessed = 0;
        }

        /// <summary>
        /// Handles the display, fade-in, wait, and fade-out of a single toast message.
        /// Updates the toast counter and background color based on toast type.
        /// </summary>
        /// <param name="toast">The toast data to display.</param>
        /// <param name="currentIndex">The index of the current toast in the queue (1-based).</param>
        /// <param name="totalCount">The total number of toasts in the queue.</param>
        private IEnumerator ShowToastCoroutine(ToastData toast, int currentIndex, int totalCount)
        {
            // Set the toast message text
            toastText.text = toast.Message;

            // Update the toast counter (e.g., "2/3") if more than one toast is queued
            if (toastCounterText != null)
                toastCounterText.text = (totalCount > 1) ? $"{currentIndex}/{totalCount}" : "";

            dismissedManually = false;

            // Set background color based on toast type
            if (panelBackground != null)
            {
                switch (toast.Type)
                {
                    case ToastType.Success:
                        panelBackground.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Green for success
                        break;
                    case ToastType.Error:
                        panelBackground.color = new Color(0.9f, 0.2f, 0.2f, 0.9f); // Red for error
                        break;
                    case ToastType.Info:
                    default:
                        panelBackground.color = new Color(0f, 0f, 0f, 0.9f); // Black for info/default
                        break;
                }
            }

            // Show the toast panel and fade in
            toastPanel.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(toastCanvasGroup, 0f, 1f, fadeDuration));

            // Wait for display duration or until manually dismissed
            float elapsed = 0f;
            while (elapsed < displayDuration && !dismissedManually)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Fade out and close the toast panel
            yield return StartCoroutine(FadeAndClose());

            // Fire events after toast is fully dismissed
            OnToastDismissed?.Invoke();
            toast.OnDismiss?.Invoke();
        }

        /// <summary>
        /// Handles fade-out and disables the toast panel.
        /// Also clears the toast counter text.
        /// </summary>
        private IEnumerator FadeAndClose()
        {
            yield return StartCoroutine(FadeCanvasGroup(toastCanvasGroup, toastCanvasGroup.alpha, 0f, fadeDuration));
            toastPanel.SetActive(false);
            if (toastCounterText != null)
                toastCounterText.text = "";
        }

        /// <summary>
        /// Handles manual dismissal via the close button.
        /// </summary>
        private void HandleManualDismiss()
        {
            if (currentToast != null)
            {
                dismissedManually = true;

                // Force fade-out and continue queue
                StopCoroutine(currentToast);
                currentToast = StartCoroutine(ToastProcessor());
            }
        }

        /// <summary>
        /// Smoothly fades a CanvasGroup's alpha from one value to another.
        /// </summary>
        /// <param name="group">The CanvasGroup to fade.</param>
        /// <param name="from">The starting alpha value.</param>
        /// <param name="to">The target alpha value.</param>
        /// <param name="duration">The duration of the fade in seconds.</param>
        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            float elapsed = 0f;
            group.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            group.alpha = to;
        }

        /// <summary>
        /// Struct to hold toast data for queueing.
        /// </summary>
        private struct ToastData
        {
            /// <summary>
            /// The message to display in the toast.
            /// </summary>
            public string Message;
            /// <summary>
            /// The type of toast (Info, Success, Error).
            /// </summary>
            public ToastType Type;
            /// <summary>
            /// An optional callback to invoke when the toast is dismissed.
            /// </summary>
            public UnityAction OnDismiss;

            /// <summary>
            /// Creates a new ToastData instance.
            /// </summary>
            /// <param name="msg">The message to display.</param>
            /// <param name="type">The type of toast.</param>
            /// <param name="onDismiss">Optional callback for when the toast is dismissed.</param>
            public ToastData(string msg, ToastType type, UnityAction onDismiss = null)
            {
                Message = msg;
                Type = type;
                OnDismiss = onDismiss;
            }
        }
    }
}
