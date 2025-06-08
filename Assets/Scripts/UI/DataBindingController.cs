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
public class DataBindingController : MonoBehaviour
{
    /// <summary>
    /// Reference to the UIDocument containing the UI Toolkit layout.
    /// </summary>
    [SerializeField] private UIDocument uiDocument;

    /// <summary>
    /// Reference to the NoteManager responsible for note operations.
    /// </summary>
    [SerializeField] private NoteManager noteManager;

    private Button addNoteButton;
    private TextField noteTitleField;
    private TextField noteDescriptionField;
    private ListView notesListView;

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

            addNoteButton = root.Q<Button>("addNoteButton");
            noteTitleField = root.Q<TextField>("noteTitleField");
            noteDescriptionField = root.Q<TextField>("noteDescriptionField");
            notesListView = root.Q<ListView>("notesListView");

            LoadNotes();

            // Set up ListView: Title | Created Date | Delete Button
            notesListView.makeItem = () =>
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

                var titleLabel = new Label
                {
                    name = "titleLabel",
                    style =
                    {
                        flexGrow = 1,
                        maxWidth = 180,
                        unityTextAlign = TextAnchor.MiddleLeft,
                        whiteSpace = WhiteSpace.NoWrap,
                        overflow = Overflow.Hidden,
                        textOverflow = TextOverflow.Ellipsis
                    }
                };

                var dateLabel = new Label
                {
                    name = "dateLabel",
                    style =
                    {
                        flexGrow = 0,
                        maxWidth = 120,
                        unityTextAlign = TextAnchor.MiddleLeft,
                        marginLeft = 10,
                        whiteSpace = WhiteSpace.NoWrap,
                        overflow = Overflow.Hidden,
                        textOverflow = TextOverflow.Ellipsis
                    }
                };

                var deleteButton = new Button
                {
                    name = "deleteButton",
                    text = "Delete",
                    style = { marginLeft = 10, flexGrow = 0 }
                };

                row.Add(titleLabel);
                row.Add(dateLabel);
                row.Add(deleteButton);
                return row;
            };
            notesListView.bindItem = (element, i) =>
            {
                var note = notes[i];
                var titleLabel = element.Q<Label>("titleLabel");
                var dateLabel = element.Q<Label>("dateLabel");
                var deleteButton = element.Q<Button>("deleteButton");

                titleLabel.text = note.Title ?? "(Untitled)";
                dateLabel.text = note.CreatedAt.ToString("yyyy-MM-dd HH:mm");

                // Remove previous click events to avoid stacking
                if (deleteButton.userData is Action prevAction)
                {
                    deleteButton.clicked -= prevAction;
                }
                // Store the callback so it can be removed next time
                Action callback = () =>
                {
                    try
                    {
                        noteManager.DeleteNote(note.Id);
                        LoadNotes();
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.Report("Failed to delete note.", ex);
                    }
                };
                deleteButton.userData = callback;
                deleteButton.clicked += callback;
            };
            notesListView.itemsSource = notes;

            addNoteButton.clicked += OnAddNoteClicked;
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
            if (notesListView != null)
            {
                notesListView.itemsSource = notes;
                notesListView.RefreshItems();
            }
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to load notes from storage.", ex);
        }
    }

    /// <summary>
    /// Handles the button click event to add a new note with the text fields' values.
    /// </summary>
    private void OnAddNoteClicked()
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
}