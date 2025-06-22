using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.AR.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace ARStickyNotes.Utilities
{
    public class ARSpawner
    {
        private const string DefaultResourceName = "PyramidVariant";

        public XRRayInteractor SpawnerRay { get; private set; } = null;

        public bool GetRayHit { get; set; } = false;

        public ARSpawner()
        {
            LoadRay();
        }

        private GameObject GetCamera()
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
        private void LoadRay()
        {
            foreach (var item in UnityEngine.Object.FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None).ToList())
            {
                //item.gameObject.SetActive(false);
                //UnityEngine.Object.Destroy(interactor);
            }
            var touchActions = new XRIDefaultInputActions().asset.FindActionMap("Touchscreen Gestures");
            var obj = new GameObject("ARSpawnerRay", typeof(TouchscreenGestureInputLoader), typeof(XRRayInteractor), typeof(XRInteractionGroup), typeof(TouchscreenHoverFilter), typeof(ScreenSpaceRayPoseDriver));
            obj.transform.SetParent(GetCamera().transform, false);
            var select = new GameObject("ARSelectInput", typeof(ScreenSpaceSelectInput));
            select.transform.SetParent(obj.transform, false);
            var selectInput = select.GetComponent<ScreenSpaceSelectInput>();
            var rotate = new GameObject("ARRotateInput", typeof(ScreenSpaceRotateInput));
            rotate.transform.SetParent(obj.transform, false);
            var scale = new GameObject("ARScaleInput", typeof(ScreenSpacePinchScaleInput));
            scale.transform.SetParent(obj.transform, false);
            //var select = obj.GetComponent<ScreenSpaceSelectInput>();
            //select.tapStartPositionInput = new XRInputValueReader<Vector2>("XRI Default Input Actions/Touchscreen Gestures/Tap Start Position", XRInputValueReader.InputSourceMode.InputActionReference);
            //Assets/Samples/XR Interaction Toolkit/3.1.1/Starter Assets/XRI Default Input Actions.inputactions
            // var rotate = obj.GetComponent<ScreenSpaceRotateInput>();
            // var scale = obj.GetComponent<ScreenSpacePinchScaleInput>();
            SpawnerRay = obj.GetComponent<XRRayInteractor>();
            SpawnerRay.transform.SetParent(GetCamera().transform, false);
            SpawnerRay.enableUIInteraction = true;
            SpawnerRay.blockInteractionsWithScreenSpaceUI = true;
            SpawnerRay.blockUIOnInteractableSelection = true;
            SpawnerRay.useForceGrab = false;
            SpawnerRay.manipulateAttachTransform = true;
            SpawnerRay.translateSpeed = 0;
            SpawnerRay.rotateMode = XRRayInteractor.RotateMode.RotateOverTime;
            SpawnerRay.rotateSpeed = 180f;
            SpawnerRay.scaleMode = UnityEngine.XR.Interaction.Toolkit.Interactors.ScaleMode.ScaleOverTime;


            //SpawnerRay.rotateManipulationInput = ScreenSpaceRotateInput;

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
            SpawnerRay.occludeARHitsWith2DObjects = false;//c348712bda248c246b8c49b3db54643f
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
                    throw new Exception("Hit trackable is not an ARPlane. Cannot spawn object.");
                }
                else if (arPlane.alignment != PlaneAlignment.Vertical)
                {
                    throw new Exception("No vertical plane selected. Cannot spawn object.");
                }
                else
                {
                    lst.Add(hit.pose.position);
                    lst.Add(arPlane.normal);
                }
            }
            // else
            // {
            //     throw new Exception("No valid raycast hit found.");
            // }
            return lst;
        }
        public List<Vector3> SpawnHit()
        {
            var vectors = GetARVectors();
            if (vectors.Count > 0)
            {
                var spawnPoint = vectors[0];//new Vector3(-1.2440004348754883f, 0.6156576871871948f, -0.5772958397865295f);
                var spawnNormal = vectors[1];//new Vector3(1.0000001192092896f, -1.1920928955078126e-7f, 0.0f);
                SpawnObject(spawnPoint, spawnNormal);
                GetRayHit = false; // Reset the flag after spawning
            }
            else
            {
                var spawnPoint = new Vector3(-1.2440004348754883f, 0.6156576871871948f, -0.5772958397865295f);
                var spawnNormal = new Vector3(1.0000001192092896f, -1.1920928955078126e-7f, 0.0f);
                SpawnObject(spawnPoint, spawnNormal);
                GetRayHit = false; // Reset the flag after spawning
            }
            return vectors;
        }
        public void SpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
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
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
        }
    }
}
