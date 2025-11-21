using System;
using System.Collections.Generic;
using System.Linq;
using ARStickyNotes.Models;
using ARStickyNotes.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityUtils;
using ARStickyNotes.UI;
using UnityEngine.InputSystem;

namespace ARStickyNotes.UI
{
    public class TouchableObjectController : MonoBehaviour
    {
        #region References
        [Header("UI References")]

        /// <summary>
        /// Name of the LayerMask for detecting clicks.
        /// </summary>
        [SerializeField] public string LayerMaskName = "TouchableObject";
        #endregion
        #region Fields and Data Objects

        /// <summary>
        /// Input action for detecting clicks.
        /// </summary>
        private InputAction _clickAction = new InputAction(type: InputActionType.Button, interactions: "Press");

        /// <summary>
        /// Input action for tracking pointer position.
        /// </summary>
        private InputAction _positionAction = new InputAction(type: InputActionType.Value, expectedControlType: "Vector2");

        /// <summary>
        /// Initial camera reference for raycasting.
        /// </summary>
        private Camera _initialCamera;

        /// <summary>
        /// Current screen position of the pointer.
        /// </summary>
        private Vector3 _screenPosition;

        /// <summary>
        /// Reference to the mesh object for collision detection.
        /// </summary>
        private GameObject _meshObject;
        #endregion
        #region Events
        /// <summary>
        /// Event triggered when the object is clicked.
        /// </summary>
        public event Action Clicked;
        #endregion

        /// <summary>
        /// Initializes the TouchableObjectController by setting up collider, rigidbody, and input actions.
        /// </summary>
        void Start()
        {
            try
            {
                LoadCollider();
                LoadRigidbody();
                LoadInput();
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to initialize the TouchableObjectController.", ex);
            }
        }

        /// <summary>
        /// Loads the MeshCollider component for detecting clicks.
        /// </summary>
        private void LoadCollider()
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
                if (LayerMaskName == null || LayerMaskName == "" || LayerMask.NameToLayer(LayerMaskName) == -1)
                {
                    LayerMaskName = LayerMask.LayerToName(_meshObject.layer);
                }
                else
                {
                    _meshObject.layer = LayerMask.NameToLayer(LayerMaskName);
                }
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
        /// Checks if the object has been clicked based on the current pointer position.
        /// </summary>
        private bool IsClicked()
        {
            if (_meshObject == null)
            {
                return false;
            }
            var ray = _initialCamera.ScreenPointToRay(_screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(LayerMaskName)))
            {
                return _meshObject.name == hit.collider.gameObject.name;
            }
            return false;
        }

        /// <summary>
        /// Loads the input action for detecting clicks.
        /// </summary>
        private void LoadClick()
        {
            _clickAction.AddBinding("<Mouse>/leftButton");
            _clickAction.AddBinding("<Touchscreen>/press");
            _clickAction.performed += context =>
            {
                if (IsClicked())
                {
                    Clicked?.Invoke();
                }
            };
            _clickAction.Enable();
        }

        /// <summary>
        /// Loads the input action for tracking pointer position.
        /// </summary>
        private void LoadPosition()
        {
            _initialCamera = Camera.main;
            _positionAction.AddBinding("<Mouse>/position");
            _positionAction.AddBinding("<Touchscreen>/position");
            _positionAction.performed += context =>
            {
                _screenPosition = context.ReadValue<Vector2>();
            };
            _positionAction.Enable();
        }

        /// <summary>
        /// Loads all input actions.
        /// </summary>
        private void LoadInput()
        {
            LoadClick();
            LoadPosition();
        }
    }
}
