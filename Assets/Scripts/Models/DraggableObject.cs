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
        /// Indicates the distance between the touch positions.
        /// </summary>
        private float? _distanceBetweenPositions = null;
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
            TouchActions[0] = new InputAction(type: InputActionType.Button, interactions: "hold(duration=0.1)");
            TouchActions[0].AddBinding("<Mouse>/leftButton");
            TouchActions[0].AddBinding("<Touchscreen>/touch0/press");
            TouchActions[0].performed += (context) =>
            {
                try
                {
                    if (IsTouched())
                    {
                        _draggingType = 1;
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
                    ErrorReporter.Report("Error when starting dragging the object at " + GetTriggeredInputActionBinding(context) + ". ", ex);
                }
            };
            TouchActions[1].canceled += (context) =>
            {
                try
                {
                    _distanceBetweenPositions = null;
                    _draggingType = 1;
                }
                catch (Exception ex)
                {
                    ErrorReporter.Report("Error when dragging ends in the object at " + GetTriggeredInputActionBinding(context) + ".", ex);
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
            while (_draggingType > 0)
            {
                if (_draggingType == 1)
                {
                    transform.position = WorldPositions[0];
                }
                else if (_draggingType == 2)
                {
                    Rescale();
                }
                yield return null;
            }
        }

        /// <summary>
        /// Rescale the object.
        /// </summary>
        private void Rescale()
        {
            var direction = 0;
            var distance = (float)Math.Round(Vector3.Distance(ScreenPositions[0], ScreenPositions[1]), 0);
            _distanceBetweenPositions = _distanceBetweenPositions == null ? distance : _distanceBetweenPositions;
            var distanceDiff = distance - _distanceBetweenPositions;
            if (distanceDiff > 1)
            {
                direction = 1;
            }
            else if (distanceDiff < -1)
            {
                direction = -1;
            }
            _distanceBetweenPositions = distance;
            if (direction != 0)
            {
                var amount = RescaleRate * direction;
                var targetScale = transform.localScale;
                var newScale = new Vector3(targetScale.x + amount, targetScale.y + amount, targetScale.z + amount);
                transform.localScale = Vector3.Slerp(targetScale, newScale, Time.deltaTime * ChangeSpeed);
            }
        }
        #endregion
    }
}
