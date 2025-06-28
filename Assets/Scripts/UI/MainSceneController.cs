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
public class MainSceneController : MonoBehaviour
{
    #region References
    [Header("UI References")]
    /// <summary>
    /// Reference to the UIDocument containing the UI Toolkit layout.
    /// </summary>
    [SerializeField] private UIDocument uiDocument;

    [Header("Script References")]
    /// <summary>
    /// Reference to the NoteManager responsible for note operations.
    /// </summary>
    [SerializeField] private NoteManager noteManager;
    #endregion

    #region UI Elements
    private Button OpenAllNotesButton;
    private VisualElement AllNotesVisualElement;
    private Button CreateNoteButton;
    private ListView NotesListView;

    #endregion

    #region Fields and Data Objects

    private List<Note> notes = new List<Note>();

    #endregion

    #region Supporting Functions
    private void InitiateUIElements()
    {
        try
        {
            var root = uiDocument.rootVisualElement;

            OpenAllNotesButton = root.Q<Button>("openAllNotesButton");
            AllNotesVisualElement = root.Q<VisualElement>("allNotesVisualElement");
            CreateNoteButton = root.Q<Button>("createNoteButton");
            NotesListView = root.Q<ListView>("NotesListView");

            // Set up ListView: Title | Created Date | Delete Button
            NotesListView.makeItem = () =>
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

            NotesListView.bindItem = (element, i) =>
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

            NotesListView.itemsSource = notes;
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to initialize UI elements.", ex);
        }
    }

    private void SubscribeToEvents()
    {
        try
        {
            OpenAllNotesButton.clicked += () => AllNotesVisualElement.style.display = DisplayStyle.Flex;
            CreateNoteButton.clicked += () => AllNotesVisualElement.style.display = DisplayStyle.None;
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to subscribe to NoteManager events.", ex);
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
            if (NotesListView != null)
            {
                NotesListView.itemsSource = notes;
                NotesListView.RefreshItems();
            }
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to load notes from storage.", ex);
        }
    }
    #endregion
    
    /// <summary>
    /// Unity OnEnable method. Binds UI elements and loads notes.
    /// </summary>
    private void OnEnable()
    {
        try
        {
            InitiateUIElements();
            LoadNotes();
            SubscribeToEvents();
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to initialize the data binding UI.", ex);
        }
    }  
}