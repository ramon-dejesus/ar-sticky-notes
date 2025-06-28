using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // The camera that this object will look at
    // If not set, it will default to the main camera
    [SerializeField] Camera targetCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetCamera != null)
        {
            transform.LookAt(targetCamera.transform);
            transform.Rotate(0, 180, 0); // Adjust rotation to face the camera correctly
        }
    }
}