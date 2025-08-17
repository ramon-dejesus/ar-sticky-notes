using System;
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
                PreloadNotes();
                try
                {
                    Debug.ClearDeveloperConsole();
                    var gameObjectName = "Noteboard";
                    Debug.Log(gameObjectName);
                    //var item = new ARSpawner().SpawnGameObject("whiteboard");
                    var item = new ARSpawner().SpawnGameObject(gameObjectName);
                    //var item = new ARSpawner().SpawnGameObject("WordSpaceNoteboardUI");
                    //item.GetComponent<NoteboardUIDocument>().LoadNotes(Notes);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("An error occurred while testing note preloading.", ex);
            }
        }

        /// <summary>
        /// Unity Awake method. Initializes local storage.
        /// </summary>
        void Awake()
        {
            try
            {
                Storage = new LocalStorage(Application.persistentDataPath, new UnityEncryption().GetUniquePassword());
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("An error occurred while initializing local storage.", ex);
            }
        }

        /// <summary>
        /// Unity Start method. Attempts to load notes.
        /// </summary>
        void Start()
        {
            try
            {
                GetNotes();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("An error occurred while starting NoteManager.", ex);
            }
        }

        /// <summary>
        /// Unity Update method. Currently unused.
        /// </summary>
        void Update()
        {

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
            // Ensure Notes and Notes.Items are initialized
            if (Notes == null)
                Notes = new NoteList();
            if (Notes.Items == null)
                Notes.Items = new System.Collections.Generic.List<Note>();

            if (Notes.Items.Count == 0)
            {
                for (var x = 0; x < 5; x++)
                {
                    var item = GetNewNote();
                    item.Title = new TextUtility().GetRandomString(10);
                    item.Description = new TextUtility().GetRandomString(100);
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
            return new UnityConverter().ConvertStringToBase64(env.Encrypt(env.GetUniquePassword()));
        }

        /// <summary>
        /// Loads the notes from local storage, or initializes a new list if none found.
        /// </summary>
        /// <returns>The loaded or newly created list of notes.</returns>
        public NoteList GetNotes()
        {
            try
            {
                if (Storage == null)
                    Storage = new LocalStorage(Application.persistentDataPath, new UnityEncryption().GetUniquePassword());

                if (Notes == null)
                {
                    Notes = Storage.GetObject<NoteList>(GetNotesFilename());
                    Notes ??= new NoteList();
                }
                if (Notes.Items == null)
                    Notes.Items = new System.Collections.Generic.List<Note>();
                return Notes;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("An error occurred while getting notes from storage.", ex);
                return null;
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
                // Ensure Notes and Notes.Items are initialized
                if (Notes == null)
                    Notes = new NoteList();
                if (Notes.Items == null)
                    Notes.Items = new System.Collections.Generic.List<Note>();

                return Notes.Items.Find(x => x.Id == id);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report($"An error occurred while getting a note by ID: {id}.", ex);
                return null;
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
                // Ensure Notes and Notes.Items are initialized
                if (Notes == null)
                    Notes = new NoteList();
                if (Notes.Items == null)
                    Notes.Items = new System.Collections.Generic.List<Note>();

                var item = Notes.Items.Find(x => x.Id == id);
                if (item != null)
                {
                    Notes.Items.Remove(item);
                }
                SaveNotes();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report($"An error occurred while deleting a note with ID: {id}.", ex);
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
                // Ensure Notes and Notes.Items are initialized
                if (Notes == null)
                    Notes = new NoteList();
                if (Notes.Items == null)
                    Notes.Items = new System.Collections.Generic.List<Note>();

                Notes.Items.Clear();
                SaveNotes();
                Notes = new NoteList();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("An error occurred while deleting all notes.", ex);
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
                ErrorReporter.Report("An error occurred while creating a new note.", ex);
                return null;
            }
        }

        /// <summary>
        /// Adds a new note to the list, then saves.
        /// </summary>
        /// <param name="item">The note to add.</param>
        /// <exception cref="Exception">Wraps any storage-related exception.</exception>
        public void CreateNote(Note item)
        {
            try
            {
                UpdateNote(item);
            }
            catch (Exception ex)
            {
                ErrorReporter.Report($"An error occurred while new a note with ID: {item?.Id}.", ex);
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
                // Ensure Notes and Notes.Items are initialized
                if (Notes == null)
                    Notes = new NoteList();
                if (Notes.Items == null)
                    Notes.Items = new System.Collections.Generic.List<Note>();

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
                ErrorReporter.Report($"An error occurred while updating a note with ID: {item?.Id}.", ex);
            }
        }
    }
}