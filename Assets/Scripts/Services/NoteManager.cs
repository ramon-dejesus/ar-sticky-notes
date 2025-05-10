using System;
using System.Data;
using System.Linq;
using UnityEngine;
using ARStickyNotes.Models;
using ARStickyNotes.Utilities;

namespace ARStickyNotes.Services
{
    /// <summary>
    /// Manages the lifecycle of Notes, including loading, saving, updating,
    /// deleting, and generating new note content.
    /// </summary>
    public class NoteManager : MonoBehaviour
    {
        /// <summary>
        /// Cached list of notes loaded from storage.
        /// </summary>        
        private NoteList Notes { get; set; } = null;

        /// <summary>
        /// Local storage system used to persist notes.
        /// </summary>
        private LocalStorage Storage { get; set; } = null;

        /// <summary>
        /// Test method to log the result of PreloadNotes with and without forced deletion.
        /// Useful for debugging and initialization testing.
        /// </summary>
        public void Test()
        {
            try
            {
                Debug.Log(PreloadNotes());
                Debug.Log(PreloadNotes(true));
            }
            catch (Exception ex)
            {
                GetExceptionTrace(ex);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NoteManager() { }

        /// <summary>
        /// Unity Start method. Initializes local storage and attempts to load notes.
        /// </summary>
        void Start()
        {
            try
            {
                Storage = new LocalStorage(Application.persistentDataPath, new UnityEncryption().GetUniquePassword());
                GetNotes();
            }
            catch (Exception ex)
            {
                GetExceptionTrace(ex);
            }
        }

        /// <summary>
        /// Unity Update method. Currently unused.
        /// </summary>
        void Update()
        {

        }

        /// <summary>
        /// Allan to move this to a utility class.
        /// Generates a random alphanumeric string.
        /// </summary>
        /// <param name="length">The desired length of the string.</param>
        /// <returns>A randomly generated string.</returns>
        private string GetRandomString(int length)
        {
            if (length < 1)
            {
                throw new ArgumentException("Length must be greater than 0", nameof(length));
            }

            var random = new System.Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Loads notes from storage and populates with random data if empty.
        /// </summary>
        /// <param name="forcedDelete">If true, deletes all existing notes first.</param>
        /// <returns>JSON string of the current notes list.</returns>
        private string PreloadNotes(bool forcedDelete = false)
        {
            if (forcedDelete)
            {
                DeleteAllNotes();
            }
            if (Notes.Items.Count == 0)
            {

                for (var x = 0; x < 5; x++)
                {
                    var item = GetNewNote();
                    item.Title = GetRandomString(10);
                    item.Description = GetRandomString(100);
                    UpdateNote(item);
                }
            }
            Notes = null;
            GetNotes();
            return new UnityConverter().ConvertObjectToJson(Notes);
        }

        /// <summary>
        /// Returns the encrypted filename used to store notes.
        /// </summary>
        /// <returns>Base64-encoded encrypted filename string.</returns>
        private string GetNotesFilename()
        {
            var env = new UnityEncryption();
            return new UnityConverter().ConvertStringToBase64(env.Encrypt(env.GetUniquePassword(), "47F8F007168A4DB9834E0746922695A4"));
        }

        /// <summary>
        /// Loads the notes from local storage, or initializes a new list if none found.
        /// </summary>
        /// <returns>The loaded or newly created list of notes.</returns>
        public NoteList GetNotes()
        {
            try
            {
                if (Notes == null)
                {
                    Notes = Storage.GetObject<NoteList>(GetNotesFilename());
                    Notes ??= new NoteList();
                }
                return Notes;
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        /// <summary>
        /// Saves the current list of notes to local storage.
        /// </summary>
        private void SaveNotes()
        {
            Storage.SaveObject(GetNotesFilename(), Notes);
        }

        /// <summary>
        /// Retrieves a note from the list by its ID.
        /// </summary>
        /// <param name="id">The ID of the note to retrieve.</param>
        /// <returns>The matching note, or null if not found.</returns>
        /// <exception cref="Exception">Wraps any storage-related exception.</exception>
        public Note GetNoteById(string id)
        {            
            try
            {
                return Notes.Items.Find(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        /// <summary>
        /// Deletes a note from the list by its ID and saves the updated list.
        /// </summary>
        /// <param name="id">The ID of the note to delete.</param>
        /// <exception cref="Exception">Wraps any storage-related exception.</exception>
        public void DeleteNote(string id)
        {
            try
            {
                var item = Notes.Items.Find(x => x.Id == id);
                if (item != null)
                {
                    Notes.Items.Remove(item);
                }
                SaveNotes();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }
        /// <summary>
        /// Clears all notes from the list and resets the state.
        /// </summary>
        /// <exception cref="Exception">Wraps any storage-related exception.</exception>
        public void DeleteAllNotes()
        {
            try
            {
                Notes.Items.Clear();
                SaveNotes();
                Notes = new NoteList();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }

        /// <summary>
        /// Returns a new instance of a note with default values.
        /// </summary>
        /// <returns>A new <see cref="Note"/> instance.</returns>
        /// <exception cref="Exception">Wraps any instantiation exception.</exception>
        public Note GetNewNote()
        {
            try
            {
                return new Note();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }

        /// <summary>
        /// Updates an existing note in the list or adds it if it doesn't exist, then saves.
        /// </summary>
        /// <param name="item">The note to update or add.</param>
        /// <exception cref="Exception">Wraps any storage-related exception.</exception>
        public void UpdateNote(Note item)
        {
            try
            {
                var tmp = Notes.Items.Find(x => x.Id == item.Id);
                if (tmp != null)
                {
                    Notes.Items.Remove(tmp);
                }
                Notes.Items.Add(item);
                SaveNotes();
            }
            catch (Exception ex)
            {
                throw GetExceptionTrace(ex);
            }
        }

        /// <summary>
        /// Logs an exception and returns a new simplified exception with just the message.
        /// </summary>
        /// <param name="ex">The original exception.</param>
        /// <returns>A new exception with the same message.</returns>
        private Exception GetExceptionTrace(Exception ex)
        {
            Debug.LogError(ex.ToString());
            return new Exception(ex.Message);
        }
    }
}