using System;
using System.Collections;
using System.Linq;
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

        /// <summary>
        /// The offset between the object's position and the cursor's position.
        /// </summary>
        private Vector3 _positionOffset;
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
            TouchAction = new InputAction(name: TouchAction.name, type: InputActionType.Button, interactions: "hold(duration=0.5)");
            TouchAction.AddBinding("<Mouse>/leftButton");
            TouchAction.AddBinding("<Touchscreen>/press");
            TouchAction.performed -= OnTouched;
            TouchAction.performed += OnDragStart;
            TouchAction.canceled += OnDragEnd;
            TouchAction.Enable();
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
                    Debug.Log(GetTriggeredInputActionBinding(context));
                    _isDragging = true;
                    _positionOffset = transform.position - WorldPositions[0];
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
                transform.position = WorldPositions[0] + _positionOffset;
                // var tmp = WorldPosition + PositionOffset;
                // transform.position = new Vector3(tmp.x, tmp.y, transform.position.z);
                yield return null;
            }
        }
        #endregion
    }
}
