using System;
using UnityEngine;

namespace ARStickyNotes.Models
{
    /// <summary>
    /// Represents a single sticky note object in the AR environment.
    /// Inherits from <see cref="BaseObject"/> and includes spatial and metadata fields.
    /// </summary>
    [Serializable]
    public class Note : BaseObject
    {
        /// <summary>
        /// The world-space position of the note serialized as a string (e.g., from a Vector3).
        /// </summary>
        [SerializeField] public string Position = "";
        
        /// <summary>
        /// The world-space rotation of the note serialized as a string (e.g., from a Quaternion or Euler angles).
        /// </summary>
        [SerializeField] public string Rotation = "";

        /// <summary>
        /// Indicates whether the note is pinned in space and should not be moved.
        /// </summary>
        [SerializeField] public bool IsPinned = false;

        /// <summary>
        /// The type or category of the note (e.g., text, image, checklist, etc.).
        /// </summary>
        [SerializeField] public string NoteType = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class with default values.
        /// </summary>
        public Note() { }
    }
}