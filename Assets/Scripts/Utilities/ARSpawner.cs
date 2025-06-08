using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace ARStickyNotes.Utilities
{
    public class ARSpawner
    {
        private const string DefaultPrefab = "Assets/Samples/XR Interaction Toolkit/3.1.1/AR Starter Assets/ARDemoSceneAssets/Prefabs/Cube.prefab";

        public ARSpawner(){}

        private Camera GetCamera()
        {
            var origin = Object.FindAnyObjectByType<XROrigin>();
            if (origin == null)
            {
                ErrorReporter.Report("No XROrigin found in the scene. Please add one to use ARSpawner.");
            }
            else if (origin.Camera == null)
            {
                ErrorReporter.Report("No camera found in the XROrigin. Please ensure it has a camera component.");
            }
            else
            {
                return origin.Camera;
            }
            return null;
        }
        public XRRayInteractor GetXRRayInteractor()
        {
            var tmp = Object.FindAnyObjectByType<XRRayInteractor>();
            if (tmp == null)
            {
                var camera = GetCamera();
                if (camera == null)
                {
                    ErrorReporter.Report("No camera found in the scene. Please add one to load XRRayInteractor.");
                }
                else
                {
                    return camera.gameObject.AddComponent<XRRayInteractor>();
                }
            }
            return tmp;
        }
        private List<Vector3> GetARVectors()
        {
            var lst = new List<Vector3>();
            var ray = GetXRRayInteractor();
            if (ray == null)
            {
                ErrorReporter.Report("XRRayInteractor is not set. Please ensure it is initialized.");
                return lst;
            }
            if (ray.TryGetCurrentARRaycastHit(out var hit))
            {
                return lst;
                if (!(hit.trackable is ARPlane arPlane))
                {
                    ErrorReporter.Report("Hit trackable is not an ARPlane. Cannot spawn object.");
                }
                else if (arPlane.alignment != PlaneAlignment.Vertical)
                {
                    ErrorReporter.Report("No vertical plane selected. Cannot spawn object.");
                }
                else
                {
                    lst.Add(hit.pose.position);
                    lst.Add(arPlane.normal);
                }
            }
            else
            {
                ErrorReporter.Report("No valid raycast hit found.");
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
            var vectors = GetARVectors();
            if (vectors.Count == 0)
            {
                ErrorReporter.Report("No valid vectors found for spawning.");
                return false;
            }
            if (onlySpawnInView && !IsInView(vectors[0]))
            {
                ErrorReporter.Report("Spawn point is not in view of the camera.");
                return false;
            }
            var spawnPoint = vectors[0];
            var spawnNormal = vectors[1];
            var cameraToFace = Camera.main;
            var newObject = Object.Instantiate(Resources.Load(DefaultPrefab) as GameObject);
            newObject.transform.position = spawnPoint;
            var facePosition = cameraToFace.transform.position;
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
            return true;
        }
    }
}
