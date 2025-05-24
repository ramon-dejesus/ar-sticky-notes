using ARStickyNotes.UI;

namespace ARStickyNotes.Utilities
{
    /// <summary>
    /// Utility for displaying toast messages to the user.
    /// </summary>
    public static class ToastNotifier
    {
        /// <summary>
        /// Shows a toast message to the user.
        /// </summary>
        /// <param name="message">The message to display in the toast.</param>
        public static void Show(string message)
        {
            ToastManager.Instance?.ShowToast(message);
        }
    }
}