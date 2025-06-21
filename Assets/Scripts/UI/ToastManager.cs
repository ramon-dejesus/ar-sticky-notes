using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.XR;

namespace ARStickyNotes.UI
{
    /// <summary>
    /// Represents the type of toast message.
    /// </summary>
    public enum ToastType { Info, Success, Error }

    /// <summary>
    /// Manages displaying toast messages to the user in a non-blocking UI panel.
    /// Supports both screen-space and world-space rendering, with fade-in/out animations.
    /// </summary>
    public class ToastManager : MonoBehaviour
    {
        public static ToastManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private TextMeshProUGUI toastText;
        [SerializeField] private Image panelBackground;
        [SerializeField] private Canvas toastCanvas;
        [SerializeField] private CanvasGroup toastCanvasGroup;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeDuration = 0.3f;

        private Coroutine currentToast;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Destroy(gameObject);
                return;
            }

            // Setup for XR or screen-space use
            if (toastCanvas != null && XRSettings.isDeviceActive)
            {
                toastCanvas.renderMode = RenderMode.WorldSpace;

                // Position the toast in front of the camera
                Transform cam = Camera.main.transform;
                toastCanvas.transform.position = cam.position + cam.forward * 1.5f;
                toastCanvas.transform.rotation = Quaternion.LookRotation(cam.forward);
            }

            toastPanel.SetActive(false);
        }

        /// <summary>
        /// Shows a toast message with default Info type.
        /// </summary>
        public void ShowToast(string message)
        {
            ShowToast(message, ToastType.Info);
        }

        /// <summary>
        /// Shows a toast message with a specified type (Info, Success, Error).
        /// </summary>
        public void ShowToast(string message, ToastType type)
        {
            if (currentToast != null)
                StopCoroutine(currentToast);

            currentToast = StartCoroutine(ShowToastCoroutine(message, type));
        }

        private IEnumerator ShowToastCoroutine(string message, ToastType type)
        {
            toastText.text = message;

            // Change background color based on toast type
            if (panelBackground != null)
            {
                switch (type)
                {
                    case ToastType.Success:
                        panelBackground.color = new Color(0.2f, 0.8f, 0.2f, 0.9f); // green
                        break;
                    case ToastType.Error:
                        panelBackground.color = new Color(0.9f, 0.2f, 0.2f, 0.9f); // red
                        break;
                    case ToastType.Info:
                    default:
                        panelBackground.color = new Color(0f, 0f, 0f, 0.9f); // black
                        break;
                }
            }

            toastPanel.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(toastCanvasGroup, 0f, 1f, fadeDuration)); // Fade in
            yield return new WaitForSeconds(displayDuration);
            yield return StartCoroutine(FadeCanvasGroup(toastCanvasGroup, 1f, 0f, fadeDuration)); // Fade out
            toastPanel.SetActive(false);
        }

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
    }
}
