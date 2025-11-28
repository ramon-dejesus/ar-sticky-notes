using System;
using System.Collections;
using ARStickyNotes.Utilities;
using UnityEngine;
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

        /// <summary>
        /// Indicates whether dragging is enabled for this object.
        /// </summary>
        [SerializeField] public bool DraggingEnabled = false;
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
        /// The offset between the object's position and the cursor's position.
        /// </summary>
        private Vector3 _positionOffset;

        /// <summary>
        /// The world position of the object.
        /// </summary>
        private Vector3 _worldPosition
        {
            get
            {
                float z = _initialCamera.WorldToScreenPoint(transform.position).z;
                return _initialCamera.ScreenToWorldPoint(_screenPosition + new Vector3(0, 0, z));
            }
        }

        /// <summary>
        /// Indicates whether the object is currently being clicked.
        /// </summary>
        private bool _isclicking = false;

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
        #region Supporting Functions

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
        /// Enables dragging functionality for the object, optionally setting a custom LayerMask name.
        /// </summary>
        public void EnableDragging(string customLayerMaskName = null)
        {
            DraggingEnabled = true;
            if (!string.IsNullOrEmpty(customLayerMaskName))
            {
                LayerMaskName = customLayerMaskName;
                LoadCollider();
            }
        }

        /// <summary>
        /// Cleans up input actions on destruction.
        /// </summary>
        void OnDestroy()
        {
            try
            {
                if (_clickAction != null)
                {
                    _clickAction.performed -= OnClicked;
                    _clickAction.canceled -= OnCanceled;
                    _clickAction.Disable();
                }
                if (_positionAction != null)
                {
                    _positionAction.Disable();
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Failed to clean up the TouchableObjectController.", ex);
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
        /// Loads the input action for detecting clicks.
        /// </summary>
        private void LoadClick()
        {
            _clickAction = new InputAction(name: "ClickAction_" + gameObject.name, type: InputActionType.Button, interactions: "Press");
            _clickAction.AddBinding("<Mouse>/leftButton");
            _clickAction.AddBinding("<Touchscreen>/press");
            _clickAction.performed += OnClicked;
            _clickAction.canceled += OnCanceled;
            _clickAction.Enable();
        }

        /// <summary>
        /// Handles the click event on the object.
        /// </summary>
        private void OnClicked(InputAction.CallbackContext context)
        {
            try
            {
                if (IsClicked())
                {
                    if (DraggingEnabled)
                    {
                        _isclicking = true;
                        _positionOffset = transform.position - _worldPosition;
                        StartCoroutine(Drag());
                    }
                    else
                    {
                        Clicked?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when touching the object. ", ex);
            }
        }

        /// <summary>
        /// Handles the cancel event when the click is released.
        /// </summary>
        private void OnCanceled(InputAction.CallbackContext context)
        {
            try
            {
                var name = context.action.name.Replace("ClickAction_", "");
                if (new ARSpawner().GetGameObject(name, false, true) != null)
                {
                    if (DraggingEnabled)
                    {
                        _isclicking = false;
                        StopCoroutine(Drag());
                        Clicked?.Invoke();
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorReporter.Report("Error when releasing the object.", ex);
            }
        }

        /// <summary>
        /// Loads the input action for tracking pointer position.
        /// </summary>
        private void LoadPosition()
        {
            _initialCamera = Camera.main;
            _positionAction = new InputAction(name: "PositionAction_" + gameObject.name, type: InputActionType.Value, expectedControlType: "Vector2");
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
        /// Drags the object while the mouse/touch is held down.
        /// </summary>
        private IEnumerator Drag()
        {
            while (_isclicking)
            {
                transform.position = _worldPosition + _positionOffset;
                yield return null;
            }
        }
        #endregion
    }
}
