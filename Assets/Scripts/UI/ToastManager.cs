using UnityEngine;
using TMPro;
using System.Collections;

namespace ARStickyNotes.UI
{
    /// <summary>
    /// Manages displaying toast messages to the user in a non-blocking UI panel.
    /// </summary>
    public class ToastManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance of the ToastManager.
        /// </summary>
        public static ToastManager Instance { get; private set; }

        /// <summary>
        /// The panel GameObject that displays the toast message.
        /// </summary>
        [SerializeField] private GameObject toastPanel;

        /// <summary>
        /// The TextMeshProUGUI component used to show the toast message.
        /// </summary>
        [SerializeField] private TextMeshProUGUI toastText;

        /// <summary>
        /// Duration in seconds for which the toast message is displayed.
        /// </summary>
        [SerializeField] private float displayDuration = 2f;

        private Coroutine currentToast;

        /// <summary>
        /// Initializes the singleton instance and hides the toast panel on startup.
        /// </summary>
        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            toastPanel.SetActive(false);
        }

        /// <summary>
        /// Shows a toast message to the user for a short duration.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void ShowToast(string message)
        {
            if (currentToast != null)
                StopCoroutine(currentToast);

            currentToast = StartCoroutine(ShowToastCoroutine(message));
        }

        /// <summary>
        /// Coroutine to handle the display and hiding of the toast message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator ShowToastCoroutine(string message)
        {
            toastText.text = message;
            toastPanel.SetActive(true);
            yield return new WaitForSeconds(displayDuration);
            toastPanel.SetActive(false);
        }
    }
}