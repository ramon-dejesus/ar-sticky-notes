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
        /// Button to open all notes.
        /// </summary>  
        private UnityEngine.UIElements.Button openAllNotesButton;

        /// <summary>
        /// Button to close all notes.
        /// </summary>
        private UnityEngine.UIElements.Button closeAllNotesButton;

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

        /// <summary>
        /// Button to launch whiteboard view of notes.
        /// </summary>
        private UnityEngine.UIElements.Button openWhiteboardButton;

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

        /// <summary>
        /// Reference to the currently spawned whiteboard instance.
        /// </summary>
        private GameObject spawnedWhiteboard;
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
                closeAllNotesButton = root.Q<UnityEngine.UIElements.Button>("CloseAllNotesButton");
                allNotesVisualElement = root.Q<VisualElement>("AllNotesVisualElement");
                createNoteButton = root.Q<UnityEngine.UIElements.Button>("CreateNoteButton");
                notesListView = root.Q<ListView>("NotesListView");
                openWhiteboardButton = root.Q<UnityEngine.UIElements.Button>("OpenWhiteboardButton");

                // Null checks for all UI elements
                if (openAllNotesButton == null)
                    throw new Exception("OpenAllNotesButton not found in UXML.");
                if (closeAllNotesButton == null)
                    throw new Exception("CloseAllNotesButton not found in UXML.");
                if (allNotesVisualElement == null)
                    throw new Exception("AllNotesVisualElement not found in UXML.");
                if (createNoteButton == null)
                    throw new Exception("CreateNoteButton not found in UXML.");
                if (notesListView == null)
                    throw new Exception("NotesListView not found in UXML.");
                if (openWhiteboardButton == null)
                    throw new Exception("OpenWhiteboardButton not found in UXML.");

                // Set up ListView: Title | Created Date | Delete Button
                notesListView.makeItem = () =>
                {
                    var container = new VisualElement { name = "RowContainerElement" };
                    // Adding styling to container as needed to apply the uss from our style sheet.
                    container.style.paddingBottom = new StyleLength(new Length(1, LengthUnit.Pixel));
                    container.style.paddingTop = new StyleLength(new Length(1, LengthUnit.Pixel));
                    container.style.marginBottom = new StyleLength(new Length(5, LengthUnit.Percent));
                    container.style.marginTop = new StyleLength(new Length(5, LengthUnit.Percent));
                    container.style.marginLeft = new StyleLength(new Length(5, LengthUnit.Percent));
                    container.style.marginRight = new StyleLength(new Length(5, LengthUnit.Percent));

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
                    contentElement.Add(titleLabel);
                    contentElement.Add(dateLabel);
                    contentElement.Add(deleteButton);
                    row.Add(contentElement);
                    container.Add(row);

                    //return row;
                    return container;
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
                openAllNotesButton.clicked -= OpenOrHideNotesMenu;
                closeAllNotesButton.clicked -= OpenOrHideNotesMenu;
                createNoteButton.clicked -= OpenCreateNotePanel;
                openWhiteboardButton.clicked -= OpenOrHideWhiteboard;

                openAllNotesButton.clicked += OpenOrHideNotesMenu;
                closeAllNotesButton.clicked += OpenOrHideNotesMenu;
                createNoteButton.clicked += OpenCreateNotePanel;
                openWhiteboardButton.clicked += OpenOrHideWhiteboard;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to subscribe to NoteManager events.", ex);
            }
        }

        /// <summary>
        /// Show Open All Notes button.
        /// </summary>
        private void ShowOpenAllNotesButton()
        {
            if (openAllNotesButton == null)
                return;
            openAllNotesButton.style.display = DisplayStyle.Flex;
            openAllNotesButton.SetEnabled(true);
        }

        /// <summary>
        /// Hide Open All Notes button.
        /// </summary>
        private void HideOpenAllNotesButton()
        {
            if (openAllNotesButton == null)
                return;

            openAllNotesButton.style.display = DisplayStyle.None;
            openAllNotesButton.SetEnabled(false);
        }

        /// <summary>
        /// Show Close All Notes button.
        /// </summary>
        private void ShowCloseAllNotesButton()
        {
            if (closeAllNotesButton == null)
                return;

            closeAllNotesButton.style.display = DisplayStyle.Flex;
            closeAllNotesButton.SetEnabled(true);
        }

        /// <summary>
        /// Hides Close All Notes button.
        /// </summary>
        private void HideCloseAllNotesButton()
        {
            if (closeAllNotesButton == null)
                return;

            closeAllNotesButton.style.display = DisplayStyle.None;
            closeAllNotesButton.SetEnabled(false);
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

            // Hide create note button when notes panel is open
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
        /// Toggles the notes menu: shows if hidden, hides if visible.
        /// </summary>
        public void OpenOrHideNotesMenu()
        {
            if (allNotesVisualElement == null)
                return;
            // Toggle visibility
            if (allNotesVisualElement.style.display == DisplayStyle.Flex)
            {
                // Currently visible, so hide it
                HideNotesMenu();
                ShowCreateNoteButton();

                // Update button states
                // Disable close button, enable open button
                ShowOpenAllNotesButton();
                HideCloseAllNotesButton();
            }
            else
            {
                // If whiteboard is open, hide/destroy it
                HideOrDestroyWhiteboard(false);

                // Currently hidden, so show it
                ShowNotesMenu();

                // Update button states
                // Disable open button, enable close button
                HideOpenAllNotesButton();
                ShowCloseAllNotesButton();
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
            createNoteButton.SetEnabled(true);
        }

        /// <summary>
        /// Hides the create note button in the main UI.
        /// </summary>
        private void HideCreateNoteButton()
        {
            if (createNoteButton == null)
                return;

            createNoteButton.style.display = DisplayStyle.None;
            createNoteButton.SetEnabled(false);
        }

        /// <summary>
        /// Opens the create/edit note panel.
        /// </summary>
        private void OpenCreateNotePanel()
        {
            // Implementation for opening the create note panel can be added here.
            UIDOCUMENT_ToastNotifier.ShowInfoMessage("Create Note panel to be opened.");
        }

        /// <summary>
        /// Hides the create/edit note panel.
        /// </summary>
        private void HideCreateNotePanel()
        {
            // Implementation for hiding the create/edit note panel can be added here.
            UIDOCUMENT_ToastNotifier.ShowInfoMessage("Create Note panel to be hidden.");
        }


        /// <summary>
        /// Shows the whiteboard: spawns if needed, or just makes visible if hidden.
        /// </summary>
        public void ShowOrSpawnWhiteboard()
        {
            // Hide all notes panel visibility
            HideNotesMenu();

            // Update button states
            // Hide create note button, hide close all notes button, and show open all notes button
            HideCreateNoteButton();
            HideCloseAllNotesButton();
            ShowOpenAllNotesButton();

            if (spawnedWhiteboard == null)
            {
                try
                {
                    if (Whiteboard == null)
                    {
                        throw new System.Exception("Whiteboard reference is missing.");
                    }
                    WhiteboardController.EnableEvent += OnWhiteboardActivated;
                    spawnedWhiteboard = new ARSpawner().SpawnGameObject(Whiteboard);
                    //UIDOCUMENT_ToastNotifier.ShowInfoMessage("Whiteboard spawned. Tap on it to add notes.");
                }
                catch (System.Exception ex)
                {
                    ErrorReporter.Report("An error occurred while spawning the whiteboard.", ex);
                }
            }
            else if (!spawnedWhiteboard.activeSelf)
            {
                spawnedWhiteboard.SetActive(true);
                //UIDOCUMENT_ToastNotifier.ShowInfoMessage("Whiteboard shown.");
            }
            else
            {
                UIDOCUMENT_ToastNotifier.ShowInfoMessage("Whiteboard is already visible.");
            }
        }
        public void OnWhiteboardActivated()
        {
            try
            {
                if (spawnedWhiteboard != null)
                {
                    spawnedWhiteboard.GetComponent<WhiteboardController>().LoadNotes(noteManager.GetNotes());
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to load notes onto whiteboard upon activation.", ex);
            }
        }
        /// <summary>
        /// Hides or destroys the spawned whiteboard, if present.
        /// </summary>
        /// <param name="destroy">If true, destroys the whiteboard. Otherwise, just hides it.</param>
        public void HideOrDestroyWhiteboard(bool destroy = false)
        {
            if (spawnedWhiteboard != null)
            {
                if (destroy)
                {
                    Destroy(spawnedWhiteboard);
                    spawnedWhiteboard = null;
                    UIDOCUMENT_ToastNotifier.ShowInfoMessage("Whiteboard destroyed.");
                }
                else if (spawnedWhiteboard.activeSelf)
                {
                    spawnedWhiteboard.SetActive(false);
                }
                else
                {
                    // Whiteboard is already hidden
                }
            }
            else
            {
                // UIDOCUMENT_ToastNotifier.ShowInfoMessage("No whiteboard to hide or destroy.");
            }

            // Show create note button again
            ShowCreateNoteButton();
        }

        /// <summary>
        /// Toggles the whiteboard: shows/spawns if hidden, hides if visible.
        /// </summary>
        public void OpenOrHideWhiteboard()
        {
            if (spawnedWhiteboard == null || !spawnedWhiteboard.activeSelf)
            {
                ShowOrSpawnWhiteboard();
            }
            else
            {
                HideOrDestroyWhiteboard(false); // Only hide, do not destroy
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
                // UIDOCUMENT_ToastNotifier.ShowSuccessMessage("Welcome to the AR Sticky Notes!");
                // UIDOCUMENT_ToastNotifier.ShowErrorMessage("This is an error message example.");
                // UIDOCUMENT_ToastNotifier.ShowInfoMessage("This is an info message example.");
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to initialize the data binding UI.", ex);
            }
        }
    }
}
