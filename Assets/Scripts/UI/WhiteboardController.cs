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
        #endregion

        #region References
        [Header("UI References")]
        /// <summary>
        /// Name of the container element where notes will be placed.
        /// </summary>
        [SerializeField] public string NoteContainerName = "Root";

        /// <summary>
        /// Initial position for the first note on the whiteboard.
        /// </summary>
        [SerializeField] public Vector3 NoteInitialPosition = new(0.012f, 0.01f, -0.006f);

        /// <summary>
        /// Scale to apply to each note on the whiteboard.
        /// </summary>
        [SerializeField] public Vector3 NoteScale = new(0.001f, 0.001f, 0.001f);

        /// <summary>
        /// Size of each note on the whiteboard. If null, it will be calculated from the NotePrefab.
        /// </summary>
        [SerializeField] public Vector3? NoteSize = new(0.004f, 0.01f, -0.004f);

        /// <summary>
        /// Maximum number of rows of notes on the whiteboard.
        /// </summary>
        [SerializeField] public int MaxRowCount = 2;

        /// <summary>
        /// Maximum number of columns of notes on the whiteboard.
        /// </summary>
        [SerializeField] public int MaxColumnCount = 5;
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
        #endregion
        private Vector3 CalculateNotePosition(GameObject note, int index)
        {
            if (NoteSize == null)
            {
                Renderer renderer = note.GetComponent<Renderer>();
                if (renderer == null)
                {
                    if (note.GetComponentInChildren<Renderer>() != null)
                    {
                        renderer = note.GetComponentInChildren<Renderer>();
                        Bounds objectBounds = renderer.bounds; // Or GetComponent<Collider>().bounds;
                        NoteSize = objectBounds.size;
                    }
                    else if (note.GetComponentInChildren<Collider>() != null)
                    {
                        Collider collider = note.GetComponentInChildren<Collider>();
                        Bounds objectBounds = collider.bounds; // Or GetComponent<Collider>().bounds;
                        NoteSize = objectBounds.size;
                    }
                    else if (note.GetComponentInChildren<RectTransform>() != null)
                    {
                        RectTransform rectTransform = note.GetComponentInChildren<RectTransform>();
                        NoteSize = rectTransform.rect.size;
                    }
                    else
                    {
                        throw new System.Exception("Note prefab does not have a Renderer component.");
                    }
                }
                else
                {
                    Bounds objectBounds = renderer.bounds; // Or GetComponent<Collider>().bounds;
                    NoteSize = objectBounds.size;
                }
            }
            if (NoteSize == null)
            {
                throw new System.Exception("Could not determine note size.");
            }
            var row = index / MaxColumnCount;
            var column = index % MaxColumnCount;
            var position = NoteInitialPosition;
            position.x -= column * NoteSize.Value.x;
            position.z -= row * NoteSize.Value.z;
            return position;
        }
        private void LoadNotes()
        {
            if (NotePrefab == null)
            {
                throw new System.Exception("Note prefab reference is missing.");
            }
            _currentNoteIndex++;
            CurrentNotes ??= new NoteList();
            var items = CurrentNotes.Items;
            for (var i = _currentNoteIndex; i < _maxVisibleCount && i < items.Count; i++)
            {
                _currentNoteIndex = i;
                var note = items[_currentNoteIndex];
                var container = new ARSpawner().GetGameObject(NoteContainerName, false, true);
                if (container == null)
                {
                    throw new System.Exception("Could not find container for notes");
                }
                var noteObject = Instantiate(NotePrefab, _spawnedWhiteboard.transform);
                if (noteObject == null)
                {
                    throw new System.Exception("Could not instantiate note prefab");
                }
                noteObject.name = "Note" + note.Id;
                noteObject.transform.SetParent(container.transform, false);
                noteObject.transform.localPosition = CalculateNotePosition(noteObject, _currentNoteIndex);
                noteObject.transform.localScale = NoteScale;
                SetNoteTitle(noteObject, note.Title);
                // var collider = noteObject.GetComponent<Collider>();
                // if (collider != null)
                // {
                //     var whiteboardController = this;
                //     var clickHandler = noteObject.AddComponent<NoteClickHandler>();
                //     clickHandler.OnClick += () =>
                //     {
                //         whiteboardController.NoteClicked?.Invoke(note);
                //     };
                // }
            }
        }
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
        /// Shows or hides the whiteboard.
        /// <param name="show">If true, shows the whiteboard; if false, hides it.</param>
        /// </summary>
        private void ShowWhiteboard(bool show = true)
        {
            if (show)
            {
                _maxVisibleCount = MaxRowCount * MaxColumnCount;
                _currentNoteIndex = -1;
                LoadWhiteboard();
                //NoteClicked.Invoke();
            }
            else if (_spawnedWhiteboard != null)
            {
                new ARSpawner().DestroyGameObject(_spawnedWhiteboard);
                _spawnedWhiteboard = null;
            }
        }

        /// <summary>
        /// Toggles the visibility of the whiteboard.
        /// </summary>
        public void ShowOrHideWhiteboard()
        {
            ShowWhiteboard(!IsVisible);
        }
    }

    /// <summary>
    /// Simple component that invokes an Action when the GameObject is clicked.
    /// Works with 3D objects that have a Collider by using OnMouseDown.
    /// </summary>
    public class NoteClickHandler : MonoBehaviour
    {
        public event Action OnClick;

        private void OnMouseDown()
        {
            OnClick?.Invoke();
        }
    }
}