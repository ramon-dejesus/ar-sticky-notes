using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityUtils;

namespace ARStickyNotes.Utilities
{
    public class ARSpawner
    {
        private const string DefaultResourceName = "CubeVariant";

        public XRRayInteractor SpawnerRay { get; private set; } = null;

        /// <summary>
        /// Indicates whether the AR ray interactor has hit a valid surface.
        /// </summary>
        public bool GetRayHit { get; set; } = false;

        public ARSpawner()
        {

        }

        /// <summary>
        /// Gets the XROrigin's CameraFloorOffsetObject, which is the camera offset
        /// </summary>
        /// <returns></returns>
        private GameObject GetCameraOffset()
        {
            var origin = UnityEngine.Object.FindAnyObjectByType<XROrigin>();
            if (origin == null)
            {
                throw new Exception("No XROrigin found in the scene. Please add one to use ARSpawner.");
            }
            else if (origin.CameraFloorOffsetObject == null)
            {
                throw new Exception("No camera found in the XROrigin. Please ensure it has a camera component.");
            }
            else
            {
                return origin.CameraFloorOffsetObject;
            }
        }

        /// <summary>
        /// Destroys the AR ray interactor and all XRRayInteractors in the scene
        /// to clean up resources.
        /// </summary>
        private void DestroyRay()
        {
            if (SpawnerRay != null)
            {
                UnityEngine.Object.Destroy(SpawnerRay.gameObject);
                SpawnerRay = null;
            }
            foreach (var item in UnityEngine.Object.FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None).ToList())
            {
                //item.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(item);
            }
        }

        /// <summary>
        /// Loads the AR ray interactor and its associated inputs.
        /// </summary>
        public void LoadRay()
        {
            DestroyRay();
            var touchActions = new XRIDefaultInputActions().asset.FindActionMap("Touchscreen Gestures");
            var obj = new GameObject("ARSpawnerRay", typeof(TouchscreenGestureInputLoader), typeof(XRRayInteractor), typeof(XRInteractionGroup), typeof(TouchscreenHoverFilter), typeof(ScreenSpaceRayPoseDriver));
            obj.transform.SetParent(GetCameraOffset().transform, false);
            var select = new GameObject("ARSelectInput", typeof(ScreenSpaceSelectInput));
            select.transform.SetParent(obj.transform, false);
            var rotate = new GameObject("ARRotateInput", typeof(ScreenSpaceRotateInput));
            rotate.transform.SetParent(obj.transform, false);
            var scale = new GameObject("ARScaleInput", typeof(ScreenSpacePinchScaleInput));
            scale.transform.SetParent(obj.transform, false);
            var selectInput = select.GetComponent<ScreenSpaceSelectInput>();
            selectInput.tapStartPositionInput = new XRInputValueReader<Vector2>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Tap Start Position")
            };
            selectInput.dragCurrentPositionInput = new XRInputValueReader<Vector2>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Drag Current Position")
            };
            selectInput.pinchGapDeltaInput = new XRInputValueReader<float>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Pinch Gap Delta")
            };
            selectInput.twistDeltaRotationInput = new XRInputValueReader<float>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Twist Delta Rotation")
            };
            var rotateInput = rotate.GetComponent<ScreenSpaceRotateInput>();
            rotateInput.twistDeltaRotationInput = new XRInputValueReader<float>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Twist Delta Rotation")
            };
            rotateInput.dragDeltaInput = new XRInputValueReader<Vector2>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Drag Delta")
            };
            rotateInput.screenTouchCountInput = new XRInputValueReader<int>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Screen Touch Count")
            };
            var scaleInput = scale.GetComponent<ScreenSpacePinchScaleInput>();
            scaleInput.useRotationThreshold = true;
            scaleInput.rotationThreshold = 0.02f;
            scaleInput.pinchGapDeltaInput = new XRInputValueReader<float>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Pinch Gap Delta")
            };
            scaleInput.twistDeltaRotationInput = new XRInputValueReader<float>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Twist Delta Rotation")
            };
            SpawnerRay = obj.GetComponent<XRRayInteractor>();
            SpawnerRay.rotateManipulationInput = new XRInputValueReader<Vector2>(null, XRInputValueReader.InputSourceMode.ObjectReference);
            SpawnerRay.rotateManipulationInput.SetObjectReference(rotateInput);
            SpawnerRay.scaleDistanceDeltaInput = new XRInputValueReader<float>(null, XRInputValueReader.InputSourceMode.ObjectReference);
            SpawnerRay.scaleDistanceDeltaInput.SetObjectReference(scaleInput);
            SpawnerRay.selectInput = new XRInputButtonReader(null, null, false, XRInputButtonReader.InputSourceMode.ObjectReference);
            SpawnerRay.selectInput.SetObjectReference(selectInput);
            SpawnerRay.enableUIInteraction = true;
            SpawnerRay.blockInteractionsWithScreenSpaceUI = true;
            SpawnerRay.blockUIOnInteractableSelection = true;
            SpawnerRay.useForceGrab = false;
            SpawnerRay.manipulateAttachTransform = true;
            SpawnerRay.translateSpeed = 0;
            SpawnerRay.rotateMode = XRRayInteractor.RotateMode.RotateOverTime;
            SpawnerRay.rotateSpeed = 180f;
            SpawnerRay.scaleMode = UnityEngine.XR.Interaction.Toolkit.Interactors.ScaleMode.ScaleOverTime;
            SpawnerRay.disableVisualsWhenBlockedInGroup = true;
            SpawnerRay.lineType = XRRayInteractor.LineType.StraightLine;
            SpawnerRay.maxRaycastDistance = 30f;
            SpawnerRay.raycastTriggerInteraction = QueryTriggerInteraction.Ignore;
            SpawnerRay.raycastSnapVolumeInteraction = XRRayInteractor.QuerySnapVolumeInteraction.Collide;
            SpawnerRay.hitDetectionType = XRRayInteractor.HitDetectionType.Raycast;
            SpawnerRay.hitClosestOnly = false;
            SpawnerRay.blendVisualLinePoints = true;
            SpawnerRay.selectActionTrigger = XRBaseInputInteractor.InputTriggerType.StateChange;
            SpawnerRay.keepSelectedTargetValid = true;
            SpawnerRay.allowHoveredActivate = false;
            SpawnerRay.hoverToSelect = false;
            SpawnerRay.targetPriorityMode = TargetPriorityMode.None;
            SpawnerRay.enableARRaycasting = true;
            SpawnerRay.occludeARHitsWith3DObjects = true;
            SpawnerRay.occludeARHitsWith2DObjects = false;
            var group = obj.GetComponent<XRInteractionGroup>();
            group.startingGroupMembers = new List<UnityEngine.Object> { SpawnerRay };
            var hover = obj.GetComponent<TouchscreenHoverFilter>();
            hover.screenTouchCountInput = new XRInputValueReader<int>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Screen Touch Count")
            };
            var driver = obj.GetComponent<ScreenSpaceRayPoseDriver>();
            driver.tapStartPositionInput = new XRInputValueReader<Vector2>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Tap Start Position")
            };
            driver.dragStartPositionInput = new XRInputValueReader<Vector2>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Drag Start Position")
            };
            driver.dragCurrentPositionInput = new XRInputValueReader<Vector2>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Drag Current Position")
            };
            driver.screenTouchCountInput = new XRInputValueReader<int>(null, XRInputValueReader.InputSourceMode.InputAction)
            {
                inputAction = touchActions.FindAction("Screen Touch Count")
            };
            SpawnerRay.gameObject.SetActive(true);
        }

        /// <summary>
        /// Gets the AR raycast hit point and normal vector from the SpawnerRay.
        /// </summary>
        /// <returns></returns>
        private List<Vector3> GetARVectors()
        {
            var lst = new List<Vector3>();
            if (SpawnerRay == null)
            {
                throw new Exception("XRRayInteractor is not set. Please ensure it is initialized.");
            }
            if (SpawnerRay.TryGetCurrentARRaycastHit(out var hit))
            {
                if (!(hit.trackable is ARPlane arPlane))
                {
                    //ToastNotifier.Show("Please select a valid surface to spawn the object.");
                }
                else if (arPlane.alignment != PlaneAlignment.Vertical)
                {
                    //ToastNotifier.Show("Please select a vertical plane/wall to spawn the object.");
                }
                else
                {
                    lst.Add(hit.pose.position);
                    lst.Add(arPlane.normal);
                }
            }
            return lst;
        }

        /// <summary>
        /// Spawns an object at the hit point of the AR raycast if GetRayHit is true.
        /// </summary>
        /// <returns></returns>
        public List<Vector3> SpawnHit()
        {
            if (GetRayHit)
            {
                var vectors = GetARVectors();
                if (vectors.Count > 0)
                {
                    var spawnPoint = vectors[0];//new Vector3(-1.2440004348754883f, 0.6156576871871948f, -0.5772958397865295f);
                    var spawnNormal = vectors[1];//new Vector3(1.0000001192092896f, -1.1920928955078126e-7f, 0.0f);
                    SpawnObject(spawnPoint, spawnNormal);
                    GetRayHit = false; // Reset the flag after spawning
                    Debug.Log("Spawning hit...");
                    new XRIDefaultInputActions().asset.FindActionMap("Touchscreen Gestures").Dispose();
                }
                return vectors;
            }
            return new List<Vector3>();
        }
        public GameObject SpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
        {
            var lst = Resources.FindObjectsOfTypeAll<GameObject>().ToList();
            var newResource = lst.FirstOrDefault(x => x.name == DefaultResourceName);
            if (newResource == null)
            {
                throw new Exception("Resource " + DefaultResourceName + " for spawning was not found.");
            }
            var newObject = UnityEngine.Object.Instantiate(newResource);
            newObject.transform.position = spawnPoint;
            var cameraToFace = Camera.main;
            var facePosition = cameraToFace.transform.position; //new Vector3(0.2199999988079071f, 0.9345943927764893f, -0.20999999344348908f);
            facePosition = new Vector3(0.2199999988079071f, 0.9345943927764893f, -0.20999999344348908f);
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
            return newObject;
        }

        /// <summary>
        /// Finds a GameObject by name in the scene and activates it.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private GameObject GetGameObject(string name)
        {
            foreach (var item in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(x => x.name.Trim().ToLower().Contains(name.ToLower())).ToList())
            {
                item.SetActive(true);
                return item;
            }
            return null;
        }

        /// <summary>
        /// Spawns a GameObject with the specified name at a position in front of the main camera.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public GameObject SpawnGameObject(string name)
        {
            var newObject = GetGameObject(name);
            if (newObject == null)
            {
                throw new Exception(name + " was not found.");
            }
            var cameraToFace = Camera.main;
            var distanceInFront = 2f;
            var spawnPoint = cameraToFace.transform.position + cameraToFace.transform.forward * distanceInFront;
            newObject.transform.position = spawnPoint;
            //newObject.transform.localScale = new Vector3(1, 1, 1);
            newObject.transform.rotation = cameraToFace.transform.rotation;
            return newObject;
        }
    }
}
