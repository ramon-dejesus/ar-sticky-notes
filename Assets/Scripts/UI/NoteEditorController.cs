using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ARStickyNotes.Models;
using ARStickyNotes.Services;
using ARStickyNotes.Utilities;

/// <summary>
/// Controls data binding between the UI Toolkit elements and the NoteManager,
/// allowing users to view, add, and delete notes.
/// </summary>
public class NoteEditorController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UIDocument containing the UI Toolkit layout.
    /// </summary>
    [SerializeField] private UIDocument uiDocument;

    /// <summary>
    /// Reference to the NoteManager responsible for note operations.
    /// </summary>
    [SerializeField] private NoteManager noteManager;

    private Button saveNoteButton;
    private Button deleteNoteButton;
    private TextField noteTitleField;
    private TextField noteDescriptionField;

    /// <summary>
    /// The current list of notes displayed in the UI.
    /// </summary>
    private List<Note> notes = new List<Note>();

    /// <summary>
    /// Unity OnEnable method. Binds UI elements and loads notes.
    /// </summary>
    private void OnEnable()
    {
        try
        {
            var root = uiDocument.rootVisualElement;

            saveNoteButton = root.Q<Button>("saveNoteButton");
            deleteNoteButton = root.Q<Button>("deleteNoteButton");
            noteTitleField = root.Q<TextField>("noteTitleField");
            noteDescriptionField = root.Q<TextField>("noteDescriptionField");

            LoadNotes();

            saveNoteButton.clicked += OnSaveNoteClicked;
            deleteNoteButton.clicked += OnDeleteNoteClicked;
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to initialize the data binding UI.", ex);
        }
    }

    /// <summary>
    /// Loads notes from the NoteManager and refreshes the ListView.
    /// </summary>
    private void LoadNotes()
    {
        try
        {
            var noteList = noteManager.GetNotes();
            notes = noteList?.Items ?? new List<Note>();
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to load notes from storage.", ex);
        }
    }

    /// <summary>
    /// Handles the button click event to save a new note with the text fields' values.
    /// </summary>
    private void OnSaveNoteClicked()
    {
        try
        {
            var title = noteTitleField.value;
            var description = noteDescriptionField.value;
            if (!string.IsNullOrWhiteSpace(title))
            {
                var newNote = noteManager.GetNewNote();
                newNote.Title = title;
                newNote.Description = description;
                noteManager.UpdateNote(newNote);

                LoadNotes();
                noteTitleField.value = "";
                noteDescriptionField.value = "";
            }
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to add a new note.", ex);
        }
    }

    /// <summary>
    /// Handles the button click event to delete a note with the text fields' values.
    /// </summary>
    private void OnDeleteNoteClicked()
    {
        try
        {
            var title = noteTitleField.value;
            var description = noteDescriptionField.value;
            /// missing note deletion logic for existing notes
            if (!string.IsNullOrWhiteSpace(title))
            {
                LoadNotes();
            }
            noteTitleField.value = "";
            noteDescriptionField.value = "";
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to delete note.", ex);
        }
    }
}
