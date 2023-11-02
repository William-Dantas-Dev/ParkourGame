using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Player Position")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 framingOffSet;
    [SerializeField] private float distance = 5f;


    [Header("RotateCamera")]
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 45;
    [SerializeField] private float rotationSpeed = 200f;
    private float rotationY;
    private float rotationX;

    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY;
    private float invertXVal;
    private float invertYVal;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        invertXVal = (invertX) ? -1 : 1;
        invertYVal = (invertY) ? -1 : 1;


        rotationY += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime * invertYVal;

        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        rotationX += Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime * invertXVal;

        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        var focusPosition = followTarget.position + framingOffSet;
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, distance);
        transform.rotation = targetRotation;
    }
    public Quaternion PlanarRotation => Quaternion.Euler(0f, rotationY, 0f);
}
