using System;
using System.Collections;
using System.Collections.Generic;
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
        #endregion
        #region Fields and Data Objects

        /// <summary>
        /// Input action for detecting touch.
        /// </summary>
        protected InputAction TouchAction = new(type: InputActionType.Button, interactions: "Press");

        /// <summary>
        /// Input action for tracking pointer position.
        /// </summary>
        protected InputAction PositionAction = new(type: InputActionType.Value, expectedControlType: "Vector2");

        /// <summary>
        /// Initial camera reference for raycasting.
        /// </summary>
        private Camera _initialCamera;

        /// <summary>
        /// Current screen position of the pointer.
        /// </summary>
        private Vector3 _screenPosition = Vector3.zero;

        /// <summary>
        /// Initial position of the object.
        /// </summary>
        private Vector3 _initialPosition = Vector3.zero;

        /// <summary>
        /// The offset between the object's position and the cursor's position.
        /// </summary>
        protected Vector3 PositionOffset;

        /// <summary>
        /// The world position of the object.
        /// </summary>
        protected Vector3 WorldPosition
        {
            get
            {
                return _initialCamera.ScreenToWorldPoint(_screenPosition + new Vector3(0, 0, _initialPosition.z));
            }
        }

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
            TouchAction = new InputAction(name: "TouchAction_" + gameObject.name, type: InputActionType.Button, interactions: "Press");
            TouchAction.AddBinding("<Mouse>/leftButton");
            TouchAction.AddBinding("<Touchscreen>/press");
            TouchAction.performed += OnTouched;
            TouchAction.Enable();
        }

        /// <summary>
        /// Handles the touched event on the object.
        /// </summary>
        protected void OnTouched(InputAction.CallbackContext context)
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
        /// Loads the input action for tracking pointer position.
        /// </summary>
        private void LoadPosition()
        {
            _initialCamera = Camera.main;
            PositionAction = new InputAction(name: "PositionAction_" + gameObject.name, type: InputActionType.Value, expectedControlType: "Vector2");
            PositionAction.AddBinding("<Mouse>/position");
            PositionAction.AddBinding("<Touchscreen>/position");
            PositionAction.performed += context =>
            {
                _screenPosition = context.ReadValue<Vector2>();
            };
            PositionAction.Enable();
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
            var ray = _initialCamera.ScreenPointToRay(_screenPosition);
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
