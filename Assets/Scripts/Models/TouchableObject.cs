using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ARStickyNotes.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ARStickyNotes.Models
{
    public class TouchableObject : MonoBehaviour
    {
        #region References
        [Header("UI References")]

        /// <summary>
        /// Name of the LayerMask for detecting touched objects.
        /// </summary>
        [SerializeField] public string LayerMaskName;

        /// <summary>
        /// Precendence in the situation when objects with diferent layer mask collide. Defaults to: _defaultLayerMaskName, LayerMaskName;
        /// </summary>
        [SerializeField] public List<string> LayerMaskPrecedence = new();

        /// <summary>
        /// The maximum amount of touch input to be detected. Maximum value is 10.
        /// </summary>
        [SerializeField, Range(1, 10)] public int MaxTouchCount = 1;

        /// <summary>
        /// Input actions for detecting touch.
        /// </summary>
        [SerializeField] public InputAction TouchAction = new(type: InputActionType.Button, interactions: "Press");

        /// <summary>
        /// Input actions for tracking touch position.
        /// </summary>
        [SerializeField] public InputAction PositionAction = new(type: InputActionType.Value, expectedControlType: "Vector2");
        #endregion
        #region Fields and Data Objects       

        /// <summary>
        /// Initial camera reference for raycasting.
        /// </summary>
        private Camera _initialCamera;

        /// <summary>
        /// Initial position of the object.
        /// </summary>
        private Vector3 _initialPosition = Vector3.zero;

        /// <summary>
        /// The world positions of touch actions. The default behavior is that it will store 10 positions.
        /// </summary>
        protected List<Vector3> WorldPositions = new();

        /// <summary>
        /// The screen positions of touch actions. The default behavior is that it will store 10 positions.
        /// </summary>
        protected List<Vector3> ScreenPositions = new();

        /// <summary>
        /// Reference to the mesh object for collision detection.
        /// </summary>
        private GameObject _meshObject;

        /// <summary>
        /// The default layer mask name.
        /// </summary>
        private string _defaultLayerMaskName = "TouchableObject";
        #endregion
        #region Events
        /// <summary>
        /// Event triggered when the object is touched.
        /// </summary>
        public event Action Touched;
        #endregion
        #region Supporting Functions
        /// <summary>
        /// Change the layer mask.
        /// </summary>
        public void ChangeLayerMask(string customLayerMaskName)
        {
            LayerMaskName = customLayerMaskName;
            LoadCollider();
        }

        /// <summary>
        /// Initializes the components.
        /// </summary>
        protected void Start()
        {
            try
            {
                LayerMaskName = string.IsNullOrEmpty(LayerMaskName) ? _defaultLayerMaskName : LayerMaskName;
                if (LayerMaskPrecedence.Count == 0)
                {
                    if (LayerMaskName != _defaultLayerMaskName)
                    {
                        LayerMaskPrecedence.Add(_defaultLayerMaskName);
                    }
                    LayerMaskPrecedence.Add(LayerMaskName);
                }
                LoadCollider();
                LoadRigidbody();
                LoadInput();
                _initialPosition = _initialCamera.WorldToScreenPoint(transform.position); //transform.position;
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to initialize the TouchableObject.", ex);
            }
        }

        /// <summary>
        /// Cleans up input actions on destruction.
        /// </summary>
        protected void OnDestroy()
        {
            try
            {
                if (TouchAction != null)
                {
                    TouchAction.performed -= OnTouched;
                    TouchAction.Disable();
                }
                if (PositionAction != null)
                {
                    PositionAction.performed -= OnPosition;
                    PositionAction.Disable();
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to clean up the TouchableObject.", ex);
            }
        }

        /// <summary>
        /// Loads the MeshCollider component for detecting touch actions.
        /// </summary>
        protected void LoadCollider()
        {
            var meshCollider = gameObject.GetComponentInChildren<MeshCollider>();
            if (meshCollider == null)
            {
                _meshObject = gameObject.GetComponentInChildren<MeshFilter>().gameObject;
                if (_meshObject == null)
                {
                    _meshObject = gameObject;
                }
                _meshObject.name = Guid.NewGuid().ToString("N");
                meshCollider = _meshObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                meshCollider.sharedMesh = _meshObject.GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                _meshObject = meshCollider.gameObject;
            }
            if (LayerMaskName == null || LayerMaskName == "" || LayerMask.NameToLayer(LayerMaskName) == -1)
            {
                LayerMaskName = LayerMask.LayerToName(_meshObject.layer);
            }
            else
            {
                _meshObject.layer = LayerMask.NameToLayer(LayerMaskName);
            }
        }

        /// <summary>
        /// Loads the Rigidbody component for physics interactions. 
        /// </summary>
        private void LoadRigidbody()
        {
            var rigidbody = gameObject.GetComponentInChildren<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = _meshObject.AddComponent<Rigidbody>();
                rigidbody.mass = 1;
                rigidbody.linearDamping = 0;
                rigidbody.angularDamping = 0.05f;
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                rigidbody.automaticCenterOfMass = true;
                rigidbody.automaticInertiaTensor = true;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }

        /// <summary>
        /// Loads the input action for detecting touch.
        /// </summary>
        private void LoadTouch()
        {
            if (TouchAction == null || TouchAction.bindings.Count == 0)
            {
                TouchAction.AddBinding("<Mouse>/leftButton");
                TouchAction.AddBinding("<Touchscreen>/touch0/tap");
                TouchAction.performed += OnTouched;
                TouchAction.Enable();
            }
        }

        /// <summary>
        /// Handles the touched event on the object.
        /// </summary>
        private void OnTouched(InputAction.CallbackContext context)
        {
            try
            {
                if (IsTouched())
                {
                    Touched?.Invoke();
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when touching the object. ", ex);
            }
        }

        /// <summary>
        /// Returns the Input Action binding full name/path that was triggered.
        /// </summary>
        protected string GetTriggeredInputActionBinding(InputAction.CallbackContext context)
        {
            var bindingIndex = context.action.GetBindingIndexForControl(context.control);
            var binding = context.action.bindings[bindingIndex];
            return binding.effectivePath;
        }

        /// <summary>
        /// Loads the input action for tracking pointer position.
        /// </summary>
        private void LoadPosition()
        {
            _initialCamera = Camera.main;
            if (PositionAction.bindings.Count == 0)
            {

                PositionAction.AddBinding("<Mouse>/position");
                WorldPositions.Clear();
                for (var x = 0; x < MaxTouchCount; x++)
                {
                    PositionAction.AddBinding("<Touchscreen>/touch" + x.ToString() + "/position");
                    WorldPositions.Add(Vector3.zero);
                    ScreenPositions.Add(Vector3.zero);
                }
                PositionAction.performed += OnPosition;
                PositionAction.Enable();
            }
        }

        /// <summary>
        /// Handles the position event on the object.
        /// </summary>
        private void OnPosition(InputAction.CallbackContext context)
        {
            try
            {
                var index = 0;
                switch (GetTriggeredInputActionBinding(context))
                {
                    case "<Touchscreen>/touch0/position":
                        index = 0;
                        break;
                    case "<Touchscreen>/touch1/position":
                        index = 1;
                        break;
                    case "<Touchscreen>/touch2/position":
                        index = 2;
                        break;
                    case "<Touchscreen>/touch3/position":
                        index = 3;
                        break;
                    case "<Touchscreen>/touch4/position":
                        index = 4;
                        break;
                    case "<Touchscreen>/touch5/position":
                        index = 5;
                        break;
                    case "<Touchscreen>/touch6/position":
                        index = 6;
                        break;
                    case "<Touchscreen>/touch7/position":
                        index = 7;
                        break;
                    case "<Touchscreen>/touch8/position":
                        index = 8;
                        break;
                    case "<Touchscreen>/touch9/position":
                        index = 9;
                        break;
                }
                ScreenPositions[index] = (Vector3)context.ReadValue<Vector2>();
                WorldPositions[index] = _initialCamera.ScreenToWorldPoint(ScreenPositions[index] + new Vector3(0, 0, _initialPosition.z));
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when getting position of the object. ", ex);
            }
        }

        /// <summary>
        /// Loads all input actions.
        /// </summary>
        private void LoadInput()
        {
            LoadTouch();
            LoadPosition();
        }

        /// <summary>
        /// Checks if the object has been touched based on the current pointer position.
        /// </summary>
        protected bool IsTouched()
        {
            if (_meshObject == null || LayerMaskPrecedence.Count == 0)
            {
                return false;
            }
            var ray = _initialCamera.ScreenPointToRay(ScreenPositions[0]);
            foreach (var name in LayerMaskPrecedence)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(name)))
                {
                    return _meshObject.name == hit.collider.gameObject.name;
                }
            }
            return false;
        }
        #endregion
    }
}
