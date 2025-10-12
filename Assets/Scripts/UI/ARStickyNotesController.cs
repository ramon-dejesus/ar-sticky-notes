using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using ARStickyNotes.Models;
using ARStickyNotes.Services;
using ARStickyNotes.Utilities;


namespace ARStickyNotes.UI
{
    public class ARStickyNotesController : MonoBehaviour
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
        [Header("UI Elements")]
        /// <summary>
        /// UI Toolkit elements bound from the UIDocument.
        /// </summary>  
        private UnityEngine.UIElements.Button openAllNotesButton;
        /// <summary>
        /// VisualElement that contains all notes and can be toggled.
        /// </summary>
        private VisualElement allNotesVisualElement;
        /// <summary>
        /// Button to create a new note.
        /// </summary>
        private UnityEngine.UIElements.Button createNoteButton;
        /// <summary>
        /// ListView to display existing notes.
        /// </summary>
        private ListView notesListView;
        #endregion

        #region Prefab References
        [Header("Prefab References")]
        /// <summary>
        /// Reference to the Whiteboard prefab to be instantiated.
        /// </summary>
        [SerializeField]
        private GameObject Whiteboard;
        #endregion

        #region Fields and Data Objects
        /// <summary>
        /// List of notes loaded from the NoteManager.
        /// </summary>
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
                openAllNotesButton = root.Q<UnityEngine.UIElements.Button>("OpenAllNotesButton");
                allNotesVisualElement = root.Q<VisualElement>("AllNotesVisualElement");
                createNoteButton = root.Q<UnityEngine.UIElements.Button>("CreateNoteButton");
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

                    var row = new VisualElement { name = "RowElement" };
                    row.AddToClassList("row");
                    var contentElement = new VisualElement { name = "ContentElement" };
                    contentElement.AddToClassList("content");
                    var titleLabel = new Label { name = "titleLabel" };
                    titleLabel.AddToClassList("title-label");
                    var dateLabel = new Label { name = "dateLabel" };
                    dateLabel.AddToClassList("date-label");
                    var deleteButton = new UnityEngine.UIElements.Button { name = "deleteButton" };
                    deleteButton.AddToClassList("delete-list-item-btn");
                    /* 
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
                    }; */

                    /* var dateLabel = new Label
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
                    */
                    contentElement.Add(titleLabel);
                    contentElement.Add(dateLabel);
                    contentElement.Add(deleteButton);
                    row.Add(contentElement);

                    return row;
                };

                notesListView.bindItem = (element, i) =>
                {
                    var note = notes[i];
                    var titleLabel = element.Q<Label>("titleLabel");
                    var dateLabel = element.Q<Label>("dateLabel");
                    var deleteButton = element.Q<UnityEngine.UIElements.Button>("deleteButton");

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
                createNoteButton.clicked -= SpawnWhiteboard;

                openAllNotesButton.clicked += OpenOrHideNotes;
                createNoteButton.clicked += SpawnWhiteboard;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to subscribe to NoteManager events.", ex);
            }
        }

        

        /// <summary>
        /// Shows Notes Menu.
        /// </summary>
        private void ShowNotesMenu()
        {
            if (allNotesVisualElement == null)
                return;

            if (allNotesVisualElement.style.display == DisplayStyle.Flex)
                return;

            allNotesVisualElement.style.display = DisplayStyle.Flex;
            HideCreateNoteButton();
        }

        /// <summary>
        /// Hides Notes Menu.
        /// </summary>
        private void HideNotesMenu()
        {
            if (allNotesVisualElement == null)
                return;

            if (allNotesVisualElement.style.display == DisplayStyle.None)
                return;

            allNotesVisualElement.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Toggles the visibility of the all notes panel.
        /// </summary>
        private void OpenOrHideNotes()
        {
            if (allNotesVisualElement.style.display == DisplayStyle.Flex)
            {
                HideNotesMenu();
            }
            else
            {
                ShowNotesMenu();
            }
        }

        /// <summary>
        /// Show create note button in main UI
        /// </summary>
        private void ShowCreateNoteButton()
        {
            if (createNoteButton == null)
                return;

            createNoteButton.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Hides the create note button in the main UI.
        /// </summary>
        private void HideCreateNoteButton()
        {
            if (createNoteButton == null)
                return;

            createNoteButton.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Show or Hide the create note button based on display flex.
        /// </summary>
        private void ToggleCreateNoteButton()
        {
            if (createNoteButton == null)
                return;

            if (createNoteButton.style.display == DisplayStyle.Flex)
            {
                HideCreateNoteButton();
            }
            else
            {
                ShowCreateNoteButton();
            }
        }

        /// <summary>
        /// Displays the whiteboard and loads notes onto it.
        /// </summary>
        void ShowWhiteboard()
        {
            try
            {
                if (Whiteboard == null)
                {
                    throw new System.Exception("Whiteboard reference is missing.");
                }
                var item = Instantiate(Whiteboard, transform);
                item = new ARSpawner().SpawnGameObject(item);

                UIDOCUMENT_ToastNotifier.ShowInfoMessage("Whiteboard spawned. Tap on it to add notes.");

            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while spawning the whiteboard.", ex);
            }
        }

        /// <summary>
        /// Spawn the whiteboard and prepare for note creation.
        /// </summary>
        private void SpawnWhiteboard()
        {
            // Hide all notes panel visibility
            HideNotesMenu();
            HideCreateNoteButton();

            // Spawn white board if not already present
            ShowWhiteboard();            
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
        /// Unity Start method. Binds UI elements and loads notes.
        /// </summary>
        private void Start()
        {
            try
            {
                InitiateUIElements();
                LoadNotes();
                SubscribeToEvents();

                // Show an info toast
                UIDOCUMENT_ToastNotifier.ShowSuccessMessage("Welcome to the AR Sticky Notes!");
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to initialize the data binding UI.", ex);
            }
        }        
    }
}
