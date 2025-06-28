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
    private Button openAllNotesButton;
    private VisualElement allNotesVisualElement;
    private Button createNoteButton;
    private ListView notesListView;
    #endregion

    #region Fields and Data Objects
    private List<Note> notes = new List<Note>();
    #endregion

    #region Supporting Functions

    /// <summary>
    /// Initializes and binds UI Toolkit elements from the UIDocument.
    /// </summary>
    private void InitiateUIElements()
    {
        try
        {
            var root = uiDocument.rootVisualElement;

            // Ensure names match UXML exactly (case-sensitive)
            openAllNotesButton = root.Q<Button>("OpenAllNotesButton");
            allNotesVisualElement = root.Q<VisualElement>("AllNotesVisualElement");
            createNoteButton = root.Q<Button>("CreateNoteButton");
            notesListView = root.Q<ListView>("NotesListView");

            // Null checks for all UI elements
            if (openAllNotesButton == null)
                throw new Exception("OpenAllNotesButton not found in UXML.");
            if (allNotesVisualElement == null)
                throw new Exception("AllNotesVisualElement not found in UXML.");
            if (createNoteButton == null)
                throw new Exception("CreateNoteButton not found in UXML.");
            if (notesListView == null)
                throw new Exception("NotesListView not found in UXML.");

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

#if UNITY_2022_2_OR_NEWER
                deleteButton.clicked -= (Action)deleteButton.userData;
                deleteButton.userData = null;
#endif
                if (deleteButton.userData is Action prevAction)
                {
                    deleteButton.clicked -= prevAction;
                }

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
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to initialize UI elements.", ex);
        }
    }

    /// <summary>
    /// Subscribes to UI events for toggling the notes panel and creating notes.
    /// </summary>
    private void SubscribeToEvents()
    {
        try
        {
            // Remove previous listeners to avoid stacking
            openAllNotesButton.clicked -= OpenOrHideNotes;
            createNoteButton.clicked -= OpenCreateNotePanel;

            openAllNotesButton.clicked += OpenOrHideNotes;
            createNoteButton.clicked += OpenCreateNotePanel;
        }
        catch (Exception ex)
        {
            ErrorReporter.Report("Failed to subscribe to NoteManager events.", ex);
        }
    }

    /// <summary>
    /// Toggles the visibility of the all notes panel.
    /// </summary>
    private void OpenOrHideNotes()
    {
        if (allNotesVisualElement.style.display == DisplayStyle.Flex)
            allNotesVisualElement.style.display = DisplayStyle.None;
        else
            allNotesVisualElement.style.display = DisplayStyle.Flex;
    }

    /// <summary>
    /// Open Create Note panel and hide all notes.
    /// </summary>
    private void OpenCreateNotePanel()
    {
        // Hide all notes panel
        allNotesVisualElement.style.display = DisplayStyle.None;

        // Show create note panel
        Debug.Log("Opening Create Note Panel");
        //createNotePanel.style.display = DisplayStyle.Flex;
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