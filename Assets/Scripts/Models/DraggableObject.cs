using System;
using System.Collections;
using System.Linq;
using ARStickyNotes.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityUtils;

namespace ARStickyNotes.Models
{
    /// <summary>
    /// Allows an object to be dragged and dropped using the mouse or touch input.
    /// </summary>
    public class DraggableObject : TouchableObject
    {
        #region References
        [Header("UI References")]

        /// <summary>
        /// Change speed of the different interactions.
        /// </summary>
        [SerializeField] public float ChangeSpeed = 4f;

        /// <summary>
        /// Rate of rescale.
        /// </summary>
        [SerializeField] public float RescaleRate = 0.1f;

        /// <summary>
        /// Rate of rotation.
        /// </summary>
        [SerializeField] public float RotationRate = 1f;
        #endregion
        #region Fields and Data Objects
        /// <summary>
        /// Indicates the type of dragging used.
        /// 0 = Disabled dragging
        /// 1 = single touch dragging
        /// 2 = multi-touch dragging
        /// </summary>
        private int _draggingType = 0;

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
                MaxTouchCount = 2;
                SubscribeToDragEvents();
                base.Start();
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
            TouchAction.AddBinding("<Touchscreen>/touch0/press");
            TouchAction.AddBinding("<Touchscreen>/touch1/press");
            //TouchAction.AddBinding("<Touchscreen>/touch*/press");
            //TouchAction.AddBinding("<Touchscreen>/press");
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
        /// Handles the start of dragging actions on the object.
        /// </summary>
        private void OnDragStart(InputAction.CallbackContext context)
        {
            try
            {

                if (IsTouched())
                {
                    switch (GetTriggeredInputActionBinding(context))
                    {
                        case "<Mouse>/leftButton":
                            _draggingType = 1;
                            break;
                        case "<Touchscreen>/touch0/press":
                            _draggingType = 1;
                            break;
                        case "<Touchscreen>/touch1/press":
                            _draggingType = 2;
                            break;
                    }
                    StartCoroutine(Drag());
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when dragging the object. ", ex);
            }
        }

        /// <summary>
        /// Handles the end of dragging actions on the object.
        /// </summary>
        private void OnDragEnd(InputAction.CallbackContext context)
        {
            try
            {
                switch (GetTriggeredInputActionBinding(context))
                {
                    case "<Mouse>/leftButton":
                        _draggingType = 0;
                        StopCoroutine(Drag());
                        break;
                    case "<Touchscreen>/touch0/press":
                        _draggingType = 0;
                        StopCoroutine(Drag());
                        break;
                    case "<Touchscreen>/touch1/press":
                        _draggingType = 1;
                        break;
                }
                // var name = context.action.name.Replace("TouchAction_", "");
                // if (new ARSpawner().GetGameObject(name, false, true) != null)
                // {
                //     switch (GetTriggeredInputActionBinding(context))
                //     {
                //         case "<Mouse>/leftButton":
                //             _draggingType = 0;
                //             StopCoroutine(Drag());
                //             break;
                //         case "<Touchscreen>/touch0/press":
                //             _draggingType = 0;
                //             StopCoroutine(Drag());
                //             break;
                //         case "<Touchscreen>/touch1/press":
                //             _draggingType = 1;
                //             break;
                //     }
                // }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when dragging ends in the object.", ex);
            }
        }

        /// <summary>
        /// Drag actions to the object.
        /// </summary>
        private IEnumerator Drag()
        {
            float? previousDistance = null;
            Vector3? previousSecondPosition = null;
            float? previousRotationDistance = null;
            while (_draggingType > 0)
            {
                if (_draggingType == 1)
                {
                    transform.position = WorldPositions[0] + _positionOffset;
                    previousDistance = null;
                    previousSecondPosition = null;
                    previousRotationDistance = null;
                }
                else if (_draggingType == 2)
                {
                    var distance = Vector3.Distance(ScreenPositions[0], ScreenPositions[1]);
                    previousDistance ??= distance;
                    if (distance > previousDistance)
                    {
                        Rescale();
                    }
                    else if (distance < previousDistance)
                    {
                        Rescale(-1);
                    }
                    else
                    {
                        if (previousSecondPosition == null)
                        {
                            previousSecondPosition = ScreenPositions[1];
                        }
                        var rotationDistance = Vector3.Distance((Vector3)previousSecondPosition, ScreenPositions[1]);
                        previousRotationDistance ??= rotationDistance;
                        if (rotationDistance > previousRotationDistance)
                        {
                            Rotate();
                        }
                        else if (rotationDistance < previousRotationDistance)
                        {
                            Rotate(-1);
                        }
                        previousSecondPosition = ScreenPositions[1];
                        previousRotationDistance = rotationDistance;
                    }
                    previousDistance = distance;
                }
                yield return null;
            }
        }

        /// <summary>
        /// Rescale the object.
        /// </summary>
        private void Rescale(int direction = 1)
        {
            var amount = RescaleRate * direction;
            var targetScale = transform.localScale;
            var newScale = new Vector3(targetScale.x + amount, targetScale.y + amount, targetScale.z + amount);
            transform.localScale = Vector3.Slerp(targetScale, newScale, Time.deltaTime * ChangeSpeed);
        }

        /// <summary>
        /// Rotate the object.
        /// </summary>
        private void Rotate(int direction = 1)
        {
            var amount = RotationRate * direction;
            var targetRotation = transform.localRotation;
            var newRotation = new Quaternion(targetRotation.x + amount, targetRotation.y + amount, targetRotation.z + amount, targetRotation.w);
            transform.localRotation = Quaternion.Slerp(targetRotation, newRotation, Time.deltaTime * ChangeSpeed);
        }
        #endregion
    }
}
