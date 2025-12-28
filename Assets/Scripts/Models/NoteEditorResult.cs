// Assets/Scripts/Models/NoteEditorResult.cs
namespace ARStickyNotes.Models
{
    /// <summary>
    /// Result of the note editor action.
    /// </summary>
    public enum NoteEditorResult
    {
        Created, // Note was created
        Updated, // Note was updated
        Deleted, // Note was deleted
        Cancelled // Note creation/editing was cancelled
    }

}