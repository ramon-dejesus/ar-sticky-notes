using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace ARStickyNotes.Utilities
{
    public class ARSpawner
    {
        private const string DefaultResourceName = "CubeVariant";

        public XRRayInteractor XRRay { get; private set; }

        public ARSpawner()
        {
            LoadXRRayInteractor();
        }

        private Camera GetCamera()
        {
            var origin = UnityEngine.Object.FindAnyObjectByType<XROrigin>();
            if (origin == null)
            {
                throw new Exception("No XROrigin found in the scene. Please add one to use ARSpawner.");
            }
            else if (origin.Camera == null)
            {
                throw new Exception("No camera found in the XROrigin. Please ensure it has a camera component.");
            }
            else
            {
                return origin.Camera;
            }
        }
        private void LoadXRRayInteractor()
        {
            XRRay = UnityEngine.Object.FindAnyObjectByType<XRRayInteractor>();
            if (XRRay == null)
            {
                var camera = GetCamera();
                if (camera == null)
                {
                    throw new Exception("No camera found in the scene. Please add one to load XRRayInteractor.");
                }
                else
                {
                    XRRay = camera.gameObject.AddComponent<XRRayInteractor>();
                }
            }
            // XRRay.hoverEntered.AddListener(new UnityAction<HoverEnterEventArgs>(OnHoverEntered));
            // XRRay.hoverExited.AddListener(new UnityAction<HoverExitEventArgs>(OnHoverExited));
            // XRRay.selectEntered.AddListener(new UnityAction<SelectEnterEventArgs>(OnSelectEntered));
            // XRRay.selectExited.AddListener(new UnityAction<SelectExitEventArgs>(OnSelectExited));            
        }
        public void OnHoverEntered(HoverEnterEventArgs args)
        {
            //Debug.Log($"{args.interactorObject} hovered over {args.interactableObject}");
            Debug.Log("OnHoverEntered called");
        }
        public void OnHoverExited(HoverExitEventArgs args)
        {
            //Debug.Log($"{args.interactorObject} exited hover over {args.interactableObject}");
            Debug.Log("OnHoverExited called");
        }
        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            //Debug.Log($"{args.interactorObject} selected {args.interactableObject}");
            Debug.Log("OnSelectEntered called");
        }
        public void OnSelectExited(SelectExitEventArgs args)
        {
            //Debug.Log($"{args.interactorObject} exited selection of {args.interactableObject}");
            Debug.Log("OnSelectExited called");
        }
        private List<Vector3> GetARVectors()
        {
            var lst = new List<Vector3>();
            if (XRRay == null)
            {
                throw new Exception("XRRayInteractor is not set. Please ensure it is initialized.");
            }
            if (XRRay.TryGetCurrentARRaycastHit(out var hit))
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
            else
            {
                throw new Exception("No valid raycast hit found.");
            }
            return lst;
        }
        private bool IsInView(Vector3 spawnPoint)
        {
            var cameraToFace = Camera.main;
            var inViewMin = 0.15f;
            var inViewMax = 1f - inViewMin;
            var pointInViewportSpace = cameraToFace.WorldToViewportPoint(spawnPoint);
            if (pointInViewportSpace.z < 0f || pointInViewportSpace.x > inViewMax || pointInViewportSpace.x < inViewMin ||
                pointInViewportSpace.y > inViewMax || pointInViewportSpace.y < inViewMin)
            {
                return false;
            }
            return true;
        }
        public bool SpawnObject(bool onlySpawnInView = true)
        {
            // var vectors = GetARVectors();
            // if (vectors.Count == 0)
            // {
            //     throw new Exception("No valid vectors found for spawning.");
            // }
            // if (onlySpawnInView && !IsInView(vectors[0]))
            // {
            //     throw new Exception("Spawn point is not in view of the camera.");
            // }
            // var spawnPoint = vectors[0];
            // var spawnNormal = vectors[1];
            var cameraToFace = Camera.main;
            var facePosition = cameraToFace.transform.position; //new Vector3(0.2199999988079071f, 0.9345943927764893f, -0.20999999344348908f);
            var spawnPoint = new Vector3(-1.2440004348754883f, 0.6156576871871948f, -0.5772958397865295f);
            var spawnNormal = new Vector3(1.0000001192092896f, -1.1920928955078126e-7f, 0.0f);
            var lst = Resources.FindObjectsOfTypeAll<GameObject>().ToList();
            var newResource = lst.FirstOrDefault(x => x.name == DefaultResourceName);
            if (newResource == null)
            {
                throw new Exception("Resource " + DefaultResourceName + " for spawning was not found.");
            }
            var newObject = UnityEngine.Object.Instantiate(newResource);
            newObject.transform.position = spawnPoint;
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
            return true;
        }
    }
}
