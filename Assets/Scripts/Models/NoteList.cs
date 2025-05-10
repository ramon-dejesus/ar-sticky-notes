using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARStickyNotes.Models
{
    /// <summary>
    /// Represents a serializable wrapper around a list of <see cref="Note"/> objects.
    /// This is necessary because Unity's <c>JsonUtility</c> does not support direct serialization of lists or arrays.
    /// </summary>
    [Serializable]
    public class NoteList
    {
        /// <summary>
        /// The list of notes contained in this wrapper.
        /// </summary>
        [SerializeField] public List<Note> Items = new List<Note>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NoteList"/> class with an empty note list.
        /// </summary>
        public NoteList() { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NoteList"/> class with a pre-existing list of notes.
        /// </summary>
        /// <param name="lst">A list of <see cref="Note"/> items to initialize the list with.</param>
        public NoteList(List<Note> lst)
        {
            Items = lst;
        }
    }
}