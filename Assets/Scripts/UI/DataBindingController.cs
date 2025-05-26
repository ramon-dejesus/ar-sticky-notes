using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using ARStickyNotes.Models;
using ARStickyNotes.Services;
using ARStickyNotes.Utilities;

/// <summary>
/// Controls data binding between the UI Toolkit elements and the NoteManager,
/// allowing users to view and add notes.
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

    private Button myButton;
    private TextField myTextField;
    private ListView myListView;

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

            myButton = root.Q<Button>("myButton");
            myTextField = root.Q<TextField>("myTextField");
            myListView = root.Q<ListView>("myListView");

            LoadNotes();

            // Set up ListView
            myListView.makeItem = () => new Label();
            myListView.bindItem = (element, i) =>
            {
                ((Label)element).text = notes[i]?.Title ?? "(Untitled)";
            };
            myListView.itemsSource = notes;

            myButton.clicked += OnAddNoteClicked;
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
            if (myListView != null)
            {
                myListView.itemsSource = notes;
                myListView.RefreshItems();
            }
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to load notes from storage.", ex);
        }
    }

    /// <summary>
    /// Handles the button click event to add a new note with the text field's value as the title.
    /// </summary>
    private void OnAddNoteClicked()
    {
        try
        {
            var title = myTextField.value;
            if (!string.IsNullOrWhiteSpace(title))
            {
                var newNote = noteManager.GetNewNote();
                newNote.Title = title;
                noteManager.UpdateNote(newNote);

                LoadNotes();
                myTextField.value = "";
            }
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to add a new note.", ex);
        }
    }
}