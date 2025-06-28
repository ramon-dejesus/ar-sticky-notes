using ARStickyNotes.UI;

namespace ARStickyNotes.Utilities
{
    /// <summary>
    /// Utility for displaying toast messages to the user.
    /// </summary>
    public static class UGUI_ToastNotifier
    {
        /// <summary>
        /// Shows a default toast message to the user.
        /// </summary>
        /// <param name="message">The default message to display in the toast.</param>
        public static void Show(string message)
        {
            ShowInfoMessage(message);
        }

        /// <summary>
        /// Shows a default toast message to the user.
        /// </summary>
        /// <param name="message">The default message to display in the toast.</param>
        public static void ShowInfoMessage(string message)
        {
            UGUI_ToastManager.Instance?.ShowToast(message, ToastType.Info);
        }

        /// <summary>
        /// Shows a success toast message to the user.
        /// </summary>
        /// <param name="message">The success message to display in the toast.</param>
        public static void ShowSuccessMessage(string message)
        {
            UGUI_ToastManager.Instance?.ShowToast(message, ToastType.Success);
        }

        /// <summary>
        /// Shows a error toast message to the user.
        /// </summary>
        /// <param name="message">The error message to display in the toast.</param>
        public static void ShowErrorMessage(string message)
        {
            UGUI_ToastManager.Instance?.ShowToast(message, ToastType.Error);
        }

    }
}