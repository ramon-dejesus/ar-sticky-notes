using System;
using System.Collections.Generic;
using ARStickyNotes.Models;
using ARStickyNotes.Utilities;
using TMPro;
using UnityEngine;

namespace ARStickyNotes.UI
{
    public class WhiteboardController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Whiteboard prefab to be instantiated.
        /// </summary>
        [SerializeField]
        public GameObject WhiteboardPrefab;

        /// <summary>
        /// Reference to the Note prefab to be instantiated.
        /// </summary>
        [SerializeField]
        public GameObject NotePrefab;

        /// <summary>
        /// Reference to the currently spawned whiteboard instance.
        /// </summary>
        private GameObject _spawnedWhiteboard;

        /// <summary>
        /// Name of the container element where notes will be placed.
        /// </summary>
        [SerializeField]
        public string NoteContainerName = "Root";

        /// <summary>
        /// Initial position for the first note on the whiteboard.
        /// </summary>
        [SerializeField]
        public Vector3 NoteInitialPosition = new(0.012f, 0.01f, -0.006f);

        /// <summary>
        /// Scale to apply to each note on the whiteboard.
        /// </summary>
        [SerializeField]
        public Vector3 NoteScale = new(0.001f, 0.001f, 0.001f);

        /// <summary>
        /// Size of each note on the whiteboard. If null, it will be calculated from the NotePrefab.
        /// </summary>
        [SerializeField]
        public Vector3? NoteSize = new(0.004f, 0.01f, -0.004f);

        /// <summary>
        /// Maximum number of rows of notes on the whiteboard.
        /// </summary>
        [SerializeField]
        public int MaxRowCount = 2;

        /// <summary>
        /// Maximum number of columns of notes on the whiteboard.
        /// </summary>
        [SerializeField]
        public int MaxColumnCount = 5;

        /// <summary>
        /// Event triggered when the whiteboard is enabled.
        /// </summary>
        public static event Action EnableEvent;

        /// <summary>
        /// Maximum number of visible notes on the whiteboard.
        /// </summary>
        private int _maxVisibleCount = 0;

        /// <summary>
        /// Index of the current note being processed.
        /// </summary>
        private int _currentNoteIndex = -1;
        private List<Note> _currentNotes = new();
        private bool _isEnabled = false;

        private void Update()
        {
            if (_isEnabled && EnableEvent != null)
            {
                _isEnabled = false;
                EnableEvent.Invoke();
            }
        }

        private void OnEnable()
        {
            _isEnabled = true;
        }
        private Vector3 CalculateNotePosition(GameObject note, int index)
        {
            if (NoteSize == null)
            {
                Renderer renderer = note.GetComponent<Renderer>();
                if (renderer == null)
                {
                    if (note.GetComponentInChildren<Renderer>() != null)
                    {
                        Debug.Log("Calculating note size from child renderer.");
                        renderer = note.GetComponentInChildren<Renderer>();
                        Bounds objectBounds = renderer.bounds; // Or GetComponent<Collider>().bounds;
                        NoteSize = objectBounds.size;
                    }
                    else if (note.GetComponentInChildren<Collider>() != null)
                    {
                        Debug.Log("Calculating note size from collider.");
                        Collider collider = note.GetComponentInChildren<Collider>();
                        Bounds objectBounds = collider.bounds; // Or GetComponent<Collider>().bounds;
                        NoteSize = objectBounds.size;
                    }
                    else if (note.GetComponentInChildren<RectTransform>() != null)
                    {
                        Debug.Log("Calculating note size from RectTransform.");
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
                    Debug.Log("Calculating note size from renderer.");
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
        private void SetNotes()
        {
            _currentNoteIndex++;
            for (var i = _currentNoteIndex; i < _maxVisibleCount && i < _currentNotes.Count; i++)
            {
                _currentNoteIndex = i;
                var note = _currentNotes[_currentNoteIndex];
                var container = new ARSpawner().GetGameObject(NoteContainerName, false, true);
                if (container == null)
                {
                    throw new System.Exception("Could not find container for notes");
                }
                var noteObject = Instantiate(NotePrefab, transform);
                if (noteObject == null)
                {
                    throw new System.Exception("Could not instantiate note prefab");
                }
                noteObject.name = "Note" + note.Id;
                noteObject.transform.SetParent(container.transform, false);
                noteObject.transform.localPosition = CalculateNotePosition(noteObject, _currentNoteIndex);
                noteObject.transform.localScale = NoteScale;
                SetNoteTitle(noteObject, note.Title);
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
        public void LoadNotes(NoteList notes)
        {
            _maxVisibleCount = MaxRowCount * MaxColumnCount;
            _currentNotes = notes.Items;
            _currentNoteIndex = -1;
            SetNotes();
        }
    }
}
