using System;
using ARStickyNotes.Models;
using ARStickyNotes.Utilities;
using TMPro;
using UnityEngine;

namespace ARStickyNotes.UI
{
    public class WhiteboardController : MonoBehaviour
    {
        #region Prefab References
        [Header("Prefab References")]
        /// <summary>
        /// Reference to the Whiteboard prefab to be instantiated.
        /// </summary>
        [SerializeField] public GameObject WhiteboardPrefab;

        /// <summary>
        /// Reference to the Note prefab to be instantiated.
        /// </summary>
        [SerializeField] public GameObject NotePrefab;

        /// <summary>
        /// Reference to the Previous button prefab.
        /// </summary>
        [SerializeField] public GameObject PreviousPrefab;

        /// <summary>
        /// Reference to the Next button prefab.
        /// </summary>
        [SerializeField] public GameObject NextPrefab;
        #endregion

        #region References
        [Header("UI References")]
        /// <summary>
        /// Name of the container where elements will be placed.
        /// </summary>
        [SerializeField] public string ContainerName = "Root";

        /// <summary>
        /// Initial position for the first note on the whiteboard.
        /// </summary>
        [SerializeField] public Vector3 InitialPosition = new(0.0125f, 0.01f, -0.006f);

        /// <summary>
        /// Scale to apply to each note on the whiteboard.
        /// </summary>
        [SerializeField] public Vector3 NoteScale = new(0.001f, 0.001f, 0.001f);

        /// <summary>
        /// Size of each note on the whiteboard. If null, it will be calculated from the NotePrefab.
        /// </summary>
        [SerializeField] public Vector3? NoteSize = new(0.004f, 0.01f, -0.004f);

        /// <summary>
        /// Size of each pagination button on the whiteboard. If null, it will be calculated from the button prefab.
        /// </summary>
        [SerializeField] public Vector3? PaginationButtonSize = new(0.004f, 0.01f, -0.004f);

        /// <summary>
        /// Maximum number of rows of notes on the whiteboard.
        /// </summary>
        [SerializeField] public int MaxRowCount = 3;

        /// <summary>
        /// Maximum number of columns of notes on the whiteboard.
        /// </summary>
        [SerializeField] public int MaxColumnCount = 7;

        /// <summary>
        /// Scale to apply to each pagination button on the whiteboard.
        /// </summary>
        [SerializeField] public Vector3 PaginationButtonScale = new(0.001f, 0.001f, 0.001f);
        #endregion

        #region Events
        /// <summary>
        /// Event triggered when a note is clicked.
        /// </summary>
        public event Action<Note> NoteClicked;
        #endregion

        #region Fields and Data Objects

        /// <summary>
        /// Indicates whether the whiteboard is currently visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _spawnedWhiteboard != null && _spawnedWhiteboard.activeSelf;
            }
        }

        /// <summary>
        /// The list of notes currently displayed on the whiteboard.
        /// </summary>
        public NoteList CurrentNotes { get; set; } = null;

        /// <summary>
        /// Reference to the currently spawned whiteboard instance.
        /// </summary>
        private GameObject _spawnedWhiteboard;

        /// <summary>
        /// Maximum number of visible notes on the whiteboard.
        /// </summary>
        private int _maxVisibleCount = 0;

        /// <summary>
        /// Index of the current note being processed.
        /// </summary>
        private int _currentNoteIndex = -1;

        private readonly string _previousButtonName = "WhiteboardPrevious"; //Guid.NewGuid().ToString("N");
        private readonly string _nextButtonName = "WhiteboardNext"; //Guid.NewGuid().ToString("N");
        #endregion

        #region Supporting Functions
        /// <summary>
        /// Shows the whiteboard.
        /// </summary>
        public void Show()
        {
            _maxVisibleCount = MaxRowCount * MaxColumnCount;
            _currentNoteIndex = -1;
            LoadWhiteboard();
        }

        /// <summary>
        /// Hides the whiteboard.
        /// </summary>
        public void Hide()
        {
            if (_spawnedWhiteboard != null)
            {
                new ARSpawner().DestroyGameObject(_spawnedWhiteboard);
                _spawnedWhiteboard = null;
            }
        }

        /// <summary>
        /// Toggles the visibility of the whiteboard.
        /// </summary>
        public void ShowOrHide()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Loads the whiteboard and its notes.
        /// </summary>
        private void LoadWhiteboard()
        {
            if (WhiteboardPrefab == null)
            {
                throw new System.Exception("Whiteboard prefab reference is missing.");
            }
            _spawnedWhiteboard = new ARSpawner().SpawnGameObject(WhiteboardPrefab);
            LoadNotes();
        }

        /// <summary>
        /// Loads the notes onto the whiteboard.
        /// </summary>
        private void LoadNotes(int skipCount = 0)
        {
            if (NotePrefab == null)
            {
                throw new System.Exception("Note prefab reference is missing.");
            }
            CurrentNotes ??= new NoteList();
            ClearNotes();
            _currentNoteIndex += skipCount;
            _currentNoteIndex = _currentNoteIndex < -1 ? -1 : _currentNoteIndex;
            for (var i = 0; i < _maxVisibleCount && (_currentNoteIndex + 1) < CurrentNotes.Items.Count; i++)
            {
                _currentNoteIndex++;
                var note = CurrentNotes.Items[_currentNoteIndex];
                var container = new ARSpawner().GetGameObject(ContainerName, false, true);
                if (container == null)
                {
                    throw new System.Exception("Could not find container.");
                }
                var noteObject = Instantiate(NotePrefab, _spawnedWhiteboard.transform);
                if (noteObject == null)
                {
                    throw new System.Exception("Could not instantiate the note");
                }
                noteObject.name = "Note" + i.ToString();
                noteObject.transform.SetParent(container.transform, false);
                noteObject.transform.localPosition = CalculatePosition(noteObject, i, NoteSize);
                noteObject.transform.localScale = NoteScale;
                SetNoteTitle(noteObject, note.Title);
                SetNoteClick(noteObject, note);
            }
            LoadPaginationButtons();
        }

        /// <summary>
        /// Clears the notes from the whiteboard.
        /// </summary>
        private void ClearNotes()
        {
            for (var i = 0; i < _maxVisibleCount; i++)
            {
                new ARSpawner().DestroyGameObject("Note" + i.ToString());
            }
        }

        /// <summary>
        /// Calculates the size of a GameObject.
        /// </summary>
        private Vector3? CalculateSize(GameObject item)
        {
            if (!item.TryGetComponent<Renderer>(out var renderer))
            {
                if (item.GetComponentInChildren<Renderer>() != null)
                {
                    renderer = item.GetComponentInChildren<Renderer>();
                    Bounds objectBounds = renderer.bounds;
                    return objectBounds.size;
                }
                else if (item.GetComponentInChildren<Collider>() != null)
                {
                    Collider collider = item.GetComponentInChildren<Collider>();
                    Bounds objectBounds = collider.bounds;
                    return objectBounds.size;
                }
                else if (item.GetComponentInChildren<RectTransform>() != null)
                {
                    RectTransform rectTransform = item.GetComponentInChildren<RectTransform>();
                    return rectTransform.rect.size;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Bounds objectBounds = renderer.bounds;
                return objectBounds.size;
            }
        }

        /// <summary>
        /// Calculates the position for a note based on its index and initial position.
        /// </summary>
        private Vector3 CalculatePosition(GameObject note, int index, Vector3? size = null, int columnCount = 0)
        {
            if (size == null)
            {
                size = CalculateSize(note);
                if (size == null)
                {
                    throw new System.Exception("Could not calculate size.");
                }
            }
            if (size == null)
            {
                throw new System.Exception("Could not determine size.");
            }
            columnCount = columnCount > 0 ? columnCount : MaxColumnCount;
            var row = index / columnCount;
            var column = index % columnCount;
            var position = InitialPosition;
            position.x -= column * size.Value.x;
            position.z -= row * size.Value.z;
            return position;
        }

        /// <summary>
        /// Sets the title text of the note.
        /// </summary>
        private void SetNoteTitle(GameObject noteObject, string title)
        {
            var txt = noteObject.GetComponentInChildren<TextMeshPro>();
            if (txt != null)
            {
                txt.text = title;
            }
            else
            {
                var txt2 = noteObject.GetComponentInChildren<TextMeshProUGUI>();
                if (txt2 != null)
                {
                    txt2.text = title;
                }
            }
        }

        /// <summary>
        /// Sets up the click event for the note.
        /// </summary>
        private void SetNoteClick(GameObject noteObject, Note item)
        {
            if (NoteClicked != null)
            {
                if (noteObject.GetComponent<TouchableObjectController>() == null)
                {
                    noteObject.AddComponent<TouchableObjectController>();
                }
                noteObject.GetComponent<TouchableObjectController>().Clicked += () =>
                {
                    NoteClicked?.Invoke(item);
                };
            }
        }

        /// <summary>
        /// Loads the navigation buttons onto the whiteboard.
        /// </summary>
        private void LoadPaginationButtons()
        {
            if (CurrentNotes.Items.Count > 0 && PreviousPrefab != null && NextPrefab != null)
            {
                LoadPreviousButton();
                LoadNextButton();
            }
            else
            {
                ClearPaginationButtons();
            }
        }

        /// <summary>
        /// Clears the pagination buttons from the whiteboard.
        /// </summary>
        private void ClearPaginationButtons()
        {
            new ARSpawner().DestroyGameObject(_previousButtonName);
            new ARSpawner().DestroyGameObject(_nextButtonName);
        }

        /// <summary>>
        /// Loads the Previous button onto the whiteboard.
        /// </summary>
        private void LoadPreviousButton()
        {
            var container = new ARSpawner().GetGameObject(ContainerName, false, true);
            if (container == null)
            {
                throw new System.Exception("Could not find container.");
            }
            var btn = new ARSpawner().GetGameObject(_previousButtonName, false, true);
            if (btn == null)
            {
                btn = Instantiate(PreviousPrefab, _spawnedWhiteboard.transform);
                if (btn == null)
                {
                    throw new System.Exception("Could not instantiate the previous button.");
                }
                btn.name = _previousButtonName;
                btn.transform.SetParent(container.transform, false);
                btn.transform.localScale = PaginationButtonScale;
                btn.transform.localPosition = CalculatePosition(btn, _maxVisibleCount, PaginationButtonSize);
                if (btn.GetComponent<TouchableObjectController>() == null)
                {
                    btn.AddComponent<TouchableObjectController>();
                }
                btn.GetComponent<TouchableObjectController>().Clicked += () =>
                {
                    LoadNotes(_maxVisibleCount * -2);
                };
            }
            btn.SetActive(_currentNoteIndex + 1 > _maxVisibleCount);
        }

        /// <summary>>
        /// Loads the Next button onto the whiteboard.
        /// </summary>
        private void LoadNextButton()
        {
            var container = new ARSpawner().GetGameObject(ContainerName, false, true);
            var btn = new ARSpawner().GetGameObject(_nextButtonName, false, true);
            if (btn == null)
            {
                btn = Instantiate(NextPrefab, _spawnedWhiteboard.transform);
                if (btn == null)
                {
                    throw new System.Exception("Could not instantiate the next button.");
                }
                btn.name = _nextButtonName;
                btn.transform.SetParent(container.transform, false);
                btn.transform.localScale = PaginationButtonScale;
                btn.transform.localPosition = CalculatePosition(btn, _maxVisibleCount + MaxColumnCount - 1, PaginationButtonSize);
                if (btn.GetComponent<TouchableObjectController>() == null)
                {
                    btn.AddComponent<TouchableObjectController>();
                }
                btn.GetComponent<TouchableObjectController>().Clicked += () =>
                {
                    LoadNotes();
                };
            }
            btn.SetActive(_currentNoteIndex + 1 < CurrentNotes.Items.Count);
        }
        #endregion
    }
}