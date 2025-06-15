using System;
using System.Collections.Generic;
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
        private const string DefaultPrefab = "Assets/Samples/XR Interaction Toolkit/3.1.1/AR Starter Assets/ARDemoSceneAssets/Prefabs/Cube.prefab";
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
            var tmp = UnityEngine.Object.FindAnyObjectByType<XRRayInteractor>();
            if (tmp == null)
            {
                var camera = GetCamera();
                if (camera == null)
                {
                    throw new Exception("No camera found in the scene. Please add one to load XRRayInteractor.");
                }
                else
                {
                    tmp = camera.gameObject.AddComponent<XRRayInteractor>();
                }
            }
            XRRay.hoverEntered.AddListener(new UnityAction<HoverEnterEventArgs>(OnHoverEntered));
            XRRay = tmp;
        }
        public void OnHoverEntered(HoverEnterEventArgs args)
        {
            Debug.Log($"{args.interactorObject} hovered over {args.interactableObject}");
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
                return lst;
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
            var vectors = GetARVectors();
            if (vectors.Count == 0)
            {
                throw new Exception("No valid vectors found for spawning.");
            }
            if (onlySpawnInView && !IsInView(vectors[0]))
            {
                throw new Exception("Spawn point is not in view of the camera.");
            }
            var spawnPoint = vectors[0];
            var spawnNormal = vectors[1];
            var cameraToFace = Camera.main;
            var newObject = UnityEngine.Object.Instantiate(Resources.Load(DefaultPrefab) as GameObject);
            newObject.transform.position = spawnPoint;
            var facePosition = cameraToFace.transform.position;
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
            return true;
        }
    }
}
