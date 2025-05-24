using ARStickyNotes.UI;
using UnityEngine;

namespace ARStickyNotes.Utilities
{
    public static class ErrorReporter
    {
        /// <summary>
        /// Reports an error to the developer and shows a user-friendly toast message.
        /// </summary>
        /// <param name="userMessage">Message to display to the user.</param>
        /// <param name="ex">Optional exception for developer logging.</param>
        public static void Report(string userMessage, System.Exception ex = null)
        {

#if UNITY_EDITOR
            if (ex != null)
                Debug.LogError(ex);
#endif

            ToastManager.Instance?.ShowToast(userMessage);
        }
    }
}