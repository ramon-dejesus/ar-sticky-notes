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
        /// Input actions for detecting touch.
        /// </summary>
        [SerializeField] public Dictionary<int, InputAction> TouchActions = new();

        /// <summary>
        /// Input actions for tracking touch position.
        /// </summary>
        [SerializeField] public Dictionary<int, InputAction> PositionActions = new();
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
        /// The world positions of touch actions.
        /// </summary>
        protected Dictionary<int, Vector3> WorldPositions = new();

        /// <summary>
        /// The screen positions of touch actions.
        /// </summary>
        protected Dictionary<int, Vector3> ScreenPositions = new();

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
        /// Events triggered when the object is touched.
        /// </summary>
        public Dictionary<int, Action> TouchEvents = new();
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
                foreach (var item in TouchActions)
                {
                    item.Value.Dispose();
                }
                foreach (var item in PositionActions)
                {
                    item.Value.Dispose();
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
            if (TouchActions.Count == 0)
            {
                TouchActions[0] = new(type: InputActionType.Button, interactions: "Press");
                TouchActions[0].AddBinding("<Mouse>/leftButton");
                TouchActions[0].AddBinding("<Touchscreen>/touch0/tap");
                TouchActions[0].performed += (context) =>
                {
                    try
                    {
                        if (IsTouched() && TouchEvents.ContainsKey(0))
                        {
                            TouchEvents[0]?.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.Report("Error when touching the object at index 0. ", ex);
                    }
                };
                TouchActions[0].Enable();
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
            if (PositionActions.Count == 0)
            {
                foreach (var item in TouchActions)
                {
                    PositionActions[item.Key] = new(type: InputActionType.Value, expectedControlType: "Vector2");
                    if (item.Key == 0)
                    {
                        PositionActions[item.Key].AddBinding("<Mouse>/position");
                    }
                    PositionActions[item.Key].AddBinding("<Touchscreen>/touch" + item.Key.ToString() + "/position");
                    PositionActions[item.Key].performed += (context) =>
                    {
                        try
                        {
                            ScreenPositions[item.Key] = (Vector3)context.ReadValue<Vector2>();
                            WorldPositions[item.Key] = _initialCamera.ScreenToWorldPoint(ScreenPositions[item.Key] + new Vector3(0, 0, _initialPosition.z));
                        }
                        catch (Exception ex)
                        {
                            ErrorReporter.Report("Error when getting touch position " + item.Key.ToString() + " of the object. ", ex);
                        }
                    };
                    PositionActions[item.Key].Enable();
                }
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
        protected bool IsTouched(int touchIndex = 0)
        {
            if (_meshObject == null || LayerMaskPrecedence.Count == 0 || !ScreenPositions.ContainsKey(touchIndex))
            {
                return false;
            }
            var ray = _initialCamera.ScreenPointToRay(ScreenPositions[touchIndex]);
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
