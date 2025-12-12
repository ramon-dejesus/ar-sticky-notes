using System;
using System.Collections;
using System.Collections.Generic;
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
        /// The offsets between the object's position and the cursor's position.
        /// </summary>
        protected Dictionary<int, Vector3> PositionOffsets = new();
        #endregion
        #region Supporting Functions
        protected new void Start()
        {
            try
            {
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
            //TouchAction = new InputAction(name: TouchAction.name, type: InputActionType.Button, interactions: "hold(duration=0.5)");
            TouchActions[0] = new InputAction(type: InputActionType.Button, interactions: "hold(duration=0.1)");
            TouchActions[0].AddBinding("<Mouse>/leftButton");
            TouchActions[0].AddBinding("<Touchscreen>/touch0/press");
            TouchActions[0].performed += (context) =>
            {
                try
                {
                    if (IsTouched())
                    {
                        if (!PositionOffsets.ContainsKey(0))
                        {
                            var x = transform.position.x - WorldPositions[0].x;
                            var y = transform.position.y - WorldPositions[0].y;
                            var z = transform.position.z - WorldPositions[0].z;
                            PositionOffsets[0] = new Vector3(x, y, z);
                        }
                        _draggingType = 1;
                        StopCoroutine(Drag());
                        StartCoroutine(Drag());
                    }
                }
                catch (Exception ex)
                {
                    ErrorReporter.Report("Error when dragging the object at " + GetTriggeredInputActionBinding(context) + ". ", ex);
                }
            };
            TouchActions[0].canceled += (context) =>
            {
                try
                {
                    _draggingType = 0;
                }
                catch (Exception ex)
                {
                    ErrorReporter.Report("Error when dragging ends in the object at " + GetTriggeredInputActionBinding(context) + ". ", ex);
                }
            };
            TouchActions[1] = new InputAction(type: InputActionType.Button, interactions: "hold(duration=0.1)");
            TouchActions[1].AddBinding("<Touchscreen>/touch1/press");
            TouchActions[1].performed += (context) =>
            {
                try
                {
                    if (IsTouched())
                    {
                        _draggingType = 2;
                    }
                }
                catch (Exception ex)
                {
                    ErrorReporter.Report("Error when dragging the object at " + GetTriggeredInputActionBinding(context) + ". ", ex);
                }
            };
            TouchActions[1].canceled += (context) =>
            {
                try
                {
                    _draggingType = 1;
                }
                catch (Exception ex)
                {
                    ErrorReporter.Report("Error when dragging ends in the object at " + GetTriggeredInputActionBinding(context) + ". ", ex);
                }
            };
            TouchActions[0].Enable();
            TouchActions[1].Enable();
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
                    //transform.position = WorldPositions[0] + PositionOffsets[0];
                    transform.position = WorldPositions[0];
                    previousDistance = null;
                    previousSecondPosition = null;
                    previousRotationDistance = null;
                }
                else if (_draggingType == 2)
                {
                    var distance = (float)Math.Round(Vector3.Distance(ScreenPositions[0], ScreenPositions[1]), 0);
                    previousDistance ??= distance;
                    var distanceDiff = distance - previousDistance;
                    if (distanceDiff > 1)
                    {
                        Rescale();
                    }
                    else if (distanceDiff < -1)
                    {
                        Rescale(-1);
                    }
                    else
                    {
                        previousSecondPosition = previousSecondPosition == null ? ScreenPositions[1] : previousSecondPosition;
                        var rotationDistance = (float)Math.Round(Vector3.Distance((Vector3)previousSecondPosition, ScreenPositions[1]), 0);
                        previousRotationDistance ??= rotationDistance;
                        var rotationDistanceDiff = rotationDistance - previousRotationDistance;
                        if (rotationDistanceDiff > 1)
                        {
                            Rotate();
                        }
                        else if (rotationDistanceDiff < -1)
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
            var newRotation = new Quaternion(targetRotation.x, targetRotation.y, targetRotation.z + amount, targetRotation.w);
            transform.localRotation = Quaternion.Slerp(targetRotation, newRotation, Time.deltaTime * ChangeSpeed);
        }
        #endregion
    }
}
