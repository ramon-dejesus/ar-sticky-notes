using ARStickyNotes.Models;
using ARStickyNotes.Utilities;
using UnityEngine;

public class NoteboardController : MonoBehaviour
{
    [SerializeField] private GameObject NotePrefab;

    [SerializeField] private string NoteContainerName = "WhiteboardRoot";

    [SerializeField] private Vector3 NoteInitialPosition = new(0.012f, 0.01f, -0.006f);

    [SerializeField] private Quaternion NoteRotation = new(0, 90, 0, 1);

    [SerializeField] private Vector3 NoteScale = new(0.001f, 0.001f, 0.001f);


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadNotes(NoteList notes)
    {
        foreach (Note note in notes.Items)
        {
            var container = new ARSpawner().GetGameObject(NoteContainerName, false);
            if (container == null)
            {
                throw new System.Exception("Could not find container for notes");
            }
            var go = Instantiate(NotePrefab, transform);
            if (go == null)
            {
                throw new System.Exception("Could not instantiate note prefab");
            }
            go.name = "Note" + note.Id;
            go.transform.SetParent(container.transform, false);
            go.transform.localPosition = NoteInitialPosition;
            go.transform.localScale = NoteScale;
            //go.GetComponent<NoteController>().SetText(note.Text);
            float width;
            GameObject myGameObject = container;
            Renderer renderer = myGameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                width = renderer.bounds.size.x;
                Debug.Log("Width of 3D object: " + width);
            }
            else
            {
                // If no renderer, try getting bounds from a collider
                Collider collider = myGameObject.GetComponent<Collider>();
                if (collider != null)
                {
                    width = collider.bounds.size.x;
                    Debug.Log("Width of 3D object (from collider): " + width);
                }
                else
                {
                    Debug.LogWarning("GameObject has no Renderer or Collider to determine width.");
                }
            }
            break;
        }
    }
}
