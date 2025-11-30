using System;
using System.Collections;
using ARStickyNotes.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARStickyNotes.Models
{
    /// <summary>
    /// Allows an object to be dragged and dropped using the mouse or touch input.
    /// </summary>
    public class DraggableObject : TouchableObject
    {
        #region Fields and Data Objects
        /// <summary>
        /// Indicates whether the object is currently being dragged.
        /// </summary>
        private bool _isDragging = false;
        #endregion
        #region Supporting Functions
        protected new void Start()
        {
            try
            {
                base.Start();
                SubscribeToDragEvents();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to initialize the DraggableObject.", ex);
            }
        }

        /// <summary>
        /// Cleans up input actions on destruction.
        /// </summary>
        protected new void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                UnsubscribeFromDragEvents();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to clean up the DraggableObject.", ex);
            }
        }

        /// <summary>
        /// Subscribes to the drag related events.
        /// </summary>
        private void SubscribeToDragEvents()
        {
            TouchAction.performed -= OnTouched;
            TouchAction.performed += OnDragStart;
            TouchAction.canceled += OnDragEnd;
        }

        /// <summary>
        /// Unsubscribes to the drag related events.
        /// </summary>
        private void UnsubscribeFromDragEvents()
        {
            TouchAction.performed -= OnDragStart;
            TouchAction.canceled -= OnDragEnd;
        }

        /// <summary>
        /// Handles the start of dragging on the object.
        /// </summary>
        private void OnDragStart(InputAction.CallbackContext context)
        {
            try
            {
                if (IsTouched())
                {
                    _isDragging = true;
                    PositionOffset = transform.position - WorldPosition;
                    StartCoroutine(Drag());
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when dragging the object. ", ex);
            }
        }
        /// <summary>
        /// Handles the cancel event when the dragging is released.
        /// </summary>
        private void OnDragEnd(InputAction.CallbackContext context)
        {
            try
            {
                var name = context.action.name.Replace("TouchAction_", "");
                if (new ARSpawner().GetGameObject(name, false, true) != null)
                {
                    _isDragging = false;
                    StopCoroutine(Drag());
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when dragging ends in the object.", ex);
            }
        }
        /// <summary>
        /// Drags the object while the mouse/touch is held down.
        /// </summary>
        private IEnumerator Drag()
        {
            while (_isDragging)
            {
                //transform.position = WorldPosition + PositionOffset;
                var tmp = WorldPosition + PositionOffset;
                transform.position = new Vector3(tmp.x, tmp.y, transform.position.z);
                yield return null;
            }
        }
        #endregion
    }
}
