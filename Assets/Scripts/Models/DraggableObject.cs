using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARStickyNotes.Models
{
    /// <summary>
    /// Allows an object to be dragged and dropped using the mouse or touch input.
    /// </summary>
    public class DraggableObject : MonoBehaviour
    {
        /// <summary>
        /// The input action for pressing.
        /// </summary>
        [SerializeField] private InputAction press;

        /// <summary>
        /// The input action for the cursor's position on the screen.
        /// </summary>
        [SerializeField] private InputAction screenPos;

        /// <summary>
        /// The current position of the cursor in screen space.
        /// </summary>
        private Vector3 CursorScreenPosition;

        /// <summary>
        /// Indicates whether the object is currently being dragged.
        /// </summary>
        private bool IsDragging = false;

        /// <summary>
        /// The offset between the object's position and the cursor's position.
        /// </summary>
        private Vector3 PositionOffset;

        /// <summary>
        /// The world position of the object.
        /// </summary>
        private Vector3 WorldPosition
        {
            get
            {
                float z = Camera.main.WorldToScreenPoint(transform.position).z;
                return Camera.main.ScreenToWorldPoint(CursorScreenPosition + new Vector3(0, 0, z));
            }
        }

        /// <summary>
        /// Indicates whether the object is currently being clicked.
        /// </summary>
        private bool IsClickedOn
        {
            get
            {
                Ray ray = Camera.main.ScreenPointToRay(CursorScreenPosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    return hit.transform == transform;
                }
                return false;
            }
        }

        private void Awake()
        {
            LoadEvents();
        }

        /// <summary>
        /// Loads the input events for dragging.
        /// </summary>
        private void LoadEvents()
        {
            screenPos.Enable();
            press.Enable();
            screenPos.performed += context =>
            {
                CursorScreenPosition = context.ReadValue<Vector2>();
            };
            press.performed += _ =>
            {
                if (IsClickedOn)
                {
                    IsDragging = true;
                    PositionOffset = transform.position - WorldPosition;
                    StartCoroutine(Drag());
                }
            };
            press.canceled += _ =>
            {
                IsDragging = false;
            };
        }

        /// <summary>
        /// Drags the object while the mouse/touch is held down.
        /// </summary>
        private IEnumerator Drag()
        {
            while (IsDragging)
            {
                transform.position = WorldPosition + PositionOffset;
                yield return null;
            }
        }
    }
}
