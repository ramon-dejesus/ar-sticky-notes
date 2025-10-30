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
        [SerializeField] private GameObject NotePrefab;

        [SerializeField] private string NoteContainerName = "Root";

        [SerializeField] private Vector3 NoteInitialPosition = new(0.012f, 0.01f, -0.006f);

        //[SerializeField] private Quaternion NoteRotation = new(0, 90, 0, 1);

        [SerializeField] private Vector3 NoteScale = new(0.001f, 0.001f, 0.001f);

        [SerializeField] private Vector3? NoteSize = new(0.004f, 0.01f, -0.004f);

        [SerializeField] private int MaxRowCount = 2;

        [SerializeField] private int MaxColumnCount = 5;

        private int MaxVisibleCount = 0;

        private int CurrentNoteIndex = -1;
        private List<Note> CurrentNotes = new();

        public static event Action EnableEvent;

        private bool IsEnabled = false;

        private void Update()
        {
            if (IsEnabled && EnableEvent != null)
            {
                IsEnabled = false;
                EnableEvent.Invoke();
            }
        }

        private void OnEnable()
        {
            IsEnabled = true;
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
            CurrentNoteIndex++;
            for (var i = CurrentNoteIndex; i < MaxVisibleCount && i < CurrentNotes.Count; i++)
            {
                CurrentNoteIndex = i;
                var note = CurrentNotes[CurrentNoteIndex];
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
                noteObject.transform.localPosition = CalculateNotePosition(noteObject, CurrentNoteIndex);
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
            MaxVisibleCount = MaxRowCount * MaxColumnCount;
            CurrentNotes = notes.Items;
            CurrentNoteIndex = -1;
            SetNotes();
        }
    }
}
