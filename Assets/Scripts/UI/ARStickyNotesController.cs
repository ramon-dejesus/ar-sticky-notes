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
        /// Reference to the mainSceneUIDocument containing the mainScene UI Document layout.
        /// </summary>
        [SerializeField] private UIDocument mainSceneUIDocument;

        /// <summary>
        /// Reference to the UXML asset for the note editor panel.
        /// </summary>
        [SerializeField] private VisualTreeAsset noteEditorUXML;

        [Header("Script References")]
        /// <summary>
        /// Reference to the NoteManager responsible for note operations.
        /// </summary>
        [SerializeField] private NoteManager noteManager;

        /// <summary>
        /// The controller for managing the whiteboard.
        /// </summary>
        [SerializeField] private WhiteboardController whiteboardController;
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

        #region Fields and Data Objects   

        /// <summary>
        /// Root VisualElement for the note editor panel.
        /// </summary>  
        private VisualElement noteEditorRoot;

        /// <summary>
        /// Action to invoke when the note editor is closed.
        /// </summary>
        private Action onNoteEditorClosed;

        #endregion

        #region Supporting Functions

        /// <summary>
        /// Initializes and binds UI Toolkit elements from the mainSceneUIDocument.
        /// </summary>
        private void InitiateUIElements()
        {
            try
            {

                var root = mainSceneUIDocument.rootVisualElement;

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
                    row.Add(contentElement);
                    row.Add(deleteButton);
                    container.Add(row);

                    //return row;
                    return container;
                };

                notesListView.bindItem = (element, i) =>
                {
                    var note = noteManager.GetNotes().Items[i];
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

                notesListView.itemsChosen += (IEnumerable<object> selectedItems) =>
                {
                    if (selectedItems == null)
                        return;

                    foreach (var obj in selectedItems)
                    {
                        var note = obj as Note;
                        if (note != null)
                        {
                            // Hide notes menu when editing a note
                            HideNotesMenu();

                            // Hide create note button when editing a note
                            HideCreateNoteButton();

                            // Hide open all notes button when editing a note
                            HideOpenAllNotesButton();

                            // Hide Show Whiteboard button when editing a note
                            HideShowWhiteboardButton();

                            // Hide close all notes button when editing a note
                            HideCloseAllNotesButton();

                            // Open note editor for the selected note
                            ShowNoteEditor(note, NoteActionType.Edit, (result, affectedNote) =>
                            {
                                // Show appropriate toast notification
                                ShowNoteEditorNotification(result, affectedNote);

                                // Show create note button after editing
                                ShowCreateNoteButton();

                                // Show Open All Notes button after editing
                                ShowOpenAllNotesButton();

                                // Show Show Whiteboard button after editing
                                DisplayShowWhiteboardButton();

                                //  Refresh notes list
                                LoadNotes();
                            });
                            break;
                        }
                    }
                };

                notesListView.itemsSource = noteManager.GetNotes().Items;

                var scrollView = notesListView.Q<ScrollView>();
                if (scrollView != null)
                {
                    scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                    scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
                }

                // Initially display Open All Notes button
                ShowOpenAllNotesButton();

                // Initially display Show Whiteboard button
                DisplayShowWhiteboardButton();

                // Initially Show Create Note button
                ShowCreateNoteButton();
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
        /// Displays Show Whiteboard button.
        /// </summary>
        private void DisplayShowWhiteboardButton()
        {
            if (openWhiteboardButton == null)
                return;

            openWhiteboardButton.style.display = DisplayStyle.Flex;
            openWhiteboardButton.SetEnabled(true);
        }

        /// <summary>
        /// Hide Show Whiteboard button.
        /// </summary>
        private void HideShowWhiteboardButton()
        {
            if (openWhiteboardButton == null)
                return;

            openWhiteboardButton.style.display = DisplayStyle.None;
            openWhiteboardButton.SetEnabled(false);
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
            allNotesVisualElement.SetEnabled(true);

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
            allNotesVisualElement.SetEnabled(false);
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
                HideWhiteboard();

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
            // Hide Open All Notes button
            HideOpenAllNotesButton();

            // Hide ShowWhiteboard button
            HideShowWhiteboardButton();

            // Hide Create Note button
            HideCreateNoteButton();

            // Implementation for opening the create note panel can be added here.
            ShowNoteEditor(null, NoteActionType.Insert, (result, affectedNote) =>
            {

                // Show appropriate toast notification
                ShowNoteEditorNotification(result, affectedNote);

                // Show ShowWhiteboard button again
                DisplayShowWhiteboardButton();

                // Show Create Note button again
                ShowCreateNoteButton();

                // Show Open All Notes button again
                ShowOpenAllNotesButton();

                // Reload notes list
                LoadNotes(); // Refresh after save or cancel
            });
        }


        /// <summary>
        /// Shows the whiteboard.
        /// </summary>
        public void ShowWhiteboard()
        {
            try
            {
                // Hide all notes panel visibility
                HideNotesMenu();

                // Update button states
                // Hide create note button, hide close all notes button, and show open all notes button
                HideCreateNoteButton();
                HideCloseAllNotesButton();
                ShowOpenAllNotesButton();
                if (whiteboardController.CurrentNotes == null)
                {
                    whiteboardController.CurrentNotes = noteManager.GetNotes();
                    whiteboardController.NoteClicked += OnWhiteboardNoteClicked;
                }
                whiteboardController.ShowOrHideWhiteboard();
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while showing the whiteboard.", ex);
            }
        }

        /// <summary>
        /// Callback when a note on the whiteboard is clicked.
        /// </summary>
        public void OnWhiteboardNoteClicked(Note note)
        {
            HideWhiteboard();
            ShowNoteEditor(note, NoteActionType.Edit, (result, affectedNote) =>
            {
                ShowNoteEditorNotification(result, affectedNote);
                ShowWhiteboard();
            });
        }

        /// <summary>
        /// Hides the spawned whiteboard, if present.
        /// </summary>
        public void HideWhiteboard()
        {
            try
            {
                whiteboardController.ShowOrHideWhiteboard();
                // Show create note button again
                ShowCreateNoteButton();
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while hiding the whiteboard.", ex);
            }
        }

        /// <summary>
        /// Toggles the whiteboard: shows if hidden, hides if visible.
        /// </summary>
        public void OpenOrHideWhiteboard()
        {
            try
            {
                if (whiteboardController == null)
                {
                    throw new Exception("WhiteboardController reference is missing.");
                }
                if (!whiteboardController.IsVisible)
                {
                    ShowWhiteboard();
                }
                else
                {
                    HideWhiteboard();
                }
            }
            catch (System.Exception ex)
            {
                ErrorReporter.Report("An error occurred while the whiteboard.", ex);
            }
        }

        /// <summary>
        /// Loads notes from the NoteManager and refreshes the ListView.
        /// </summary>
        private void LoadNotes()
        {
            try
            {
                if (notesListView != null)
                {
                    notesListView.itemsSource = noteManager.GetNotes().Items;
                    notesListView.RefreshItems();
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to load notes from storage.", ex);
            }
        }

        /// <summary>
        /// Shows the note editor panel for creating or editing a note.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="action"></param>
        private void ShowNoteEditor(
    Note note = null,
    NoteActionType action = NoteActionType.Insert,
    Action<NoteEditorResult, Note> onComplete = null)
        {
            var root = mainSceneUIDocument.rootVisualElement;
            if (noteEditorRoot != null)
            {
                CloseNoteEditor();
            }
            noteEditorRoot = noteEditorUXML.CloneTree();
            noteEditorRoot.style.position = Position.Absolute;
            noteEditorRoot.style.top = 0;
            noteEditorRoot.style.left = 0;
            noteEditorRoot.style.right = 0;
            noteEditorRoot.style.bottom = 0;
            root.Add(noteEditorRoot);

            var titleField = noteEditorRoot.Q<TextField>("noteTitleField");
            var contentField = noteEditorRoot.Q<TextField>("noteDescriptionField");
            var saveButton = noteEditorRoot.Q<UnityEngine.UIElements.Button>("saveNoteButton");
            var deleteButton = noteEditorRoot.Q<UnityEngine.UIElements.Button>("deleteNoteButton");
            var cancelButton = noteEditorRoot.Q<UnityEngine.UIElements.Button>("cancelNoteButton");

            // Populate fields if editing
            if (action == NoteActionType.Edit && note != null)
            {
                titleField.value = note.Title;
                contentField.value = note.Description;
            }
            else
            {
                titleField.value = "";
                contentField.value = "";
            }

            // Save logic
            saveButton.clicked += () =>
            {
                Note affectedNote = note;
                if (action == NoteActionType.Insert)
                {
                    var newNote = noteManager.GetNewNote();
                    newNote.Title = titleField.value;
                    newNote.Description = contentField.value;
                    newNote.CreatedAt = DateTime.Now;
                    noteManager.CreateNote(newNote);
                    affectedNote = newNote;
                    CloseNoteEditor();
                    onComplete?.Invoke(NoteEditorResult.Created, affectedNote);
                }
                else if (action == NoteActionType.Edit && note != null)
                {
                    note.Title = titleField.value;
                    note.Description = contentField.value;
                    noteManager.UpdateNote(note);
                    CloseNoteEditor();
                    onComplete?.Invoke(NoteEditorResult.Updated, note);
                }
            };

            // Delete logic
            deleteButton.clicked += () =>
            {
                if (action == NoteActionType.Edit && note != null)
                {
                    noteManager.DeleteNote(note.Id);
                    CloseNoteEditor();
                    onComplete?.Invoke(NoteEditorResult.Deleted, note);
                }
            };

            // Cancel logic
            cancelButton.clicked += () =>
            {
                // Simply close the editor without saving            
                CloseNoteEditor();
                onComplete?.Invoke(NoteEditorResult.Cancelled, null);
            };
        }

        /// <summary>
        /// Closes the note editor panel if open.
        /// </summary>
        private void CloseNoteEditor()
        {
            var root = mainSceneUIDocument.rootVisualElement;
            if (noteEditorRoot != null)
            {
                root.Remove(noteEditorRoot);
                noteEditorRoot = null;

                // Invoke any registered callbacks
                onNoteEditorClosed?.Invoke();
                onNoteEditorClosed = null;
            }
        }

        private void ShowNoteEditorNotification(NoteEditorResult result, Note affectedNote)
        {
            switch (result)
            {
                case NoteEditorResult.Created:
                    UIDOCUMENT_ToastNotifier.ShowSuccessMessage("Note created!");
                    break;
                case NoteEditorResult.Updated:
                    UIDOCUMENT_ToastNotifier.ShowSuccessMessage("Note updated!");
                    break;
                case NoteEditorResult.Deleted:
                    UIDOCUMENT_ToastNotifier.ShowInfoMessage("Note deleted.");
                    break;
                case NoteEditorResult.Cancelled:
                default:
                    // No notification for cancel
                    break;
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