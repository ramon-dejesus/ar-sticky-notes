using ARStickyNotes.UI;
using UnityEngine;

namespace ARStickyNotes.Utilities
{
    /// <summary>
    /// Utility for reporting errors to the developer and showing user-friendly toast messages.
    /// </summary>
    public static class ErrorReporter
    {
        public enum ToastSystem
        {
            UGUI,
            UIDocument
        }

        /// <summary>
        /// Reports an error by logging it (in the editor), showing a toast message to the user,
        /// and optionally throwing the provided or a new exception.
        /// </summary>
        /// <param name="userMessage">A user-friendly message to display in the toast. If null, falls back to the exception message.</param>
        /// <param name="ex">The original exception. If null, a new generic exception is created if throwException is true.</param>
        /// <param name="throwException">If true, throws the provided or a new exception after reporting.</param>
        /// <param name="showToast">If true, shows a toast message to the user.</param>
        /// <param name="toastSystem">Which toast system to use.</param>
        public static void Report(
            string userMessage,
            System.Exception ex = null,
            bool throwException = true,
            bool showToast = true,
            ToastSystem toastSystem = ToastSystem.UGUI)
        {
            var errorMessage = "An unknown error occurred.";
            var exception = ex ?? new System.Exception(errorMessage);
            var messageToShow = userMessage ?? exception.Message ?? errorMessage;

#if UNITY_EDITOR
            if (ex != null)
                Debug.LogError(ex);
#endif
            if (showToast)
            {
                switch (toastSystem)
                {
                    case ToastSystem.UGUI:
                        UGUI_ToastNotifier.ShowErrorMessage(messageToShow);
                        break;
                    case ToastSystem.UIDocument:
                        UIDOCUMENT_ToastNotifier.ShowErrorMessage(messageToShow);
                        break;
                }
            }

            if (throwException)
                throw exception;
        }
    }
}