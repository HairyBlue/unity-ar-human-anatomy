using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RotationController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // public GameObject[] organs; 
    public float rotationSpeed = 50f;
    private bool isRotating = false; 
    private Vector3 rotationAxis = Vector3.zero;

    private BodyOrganManager bodyOrganManager;

    void Update()
    {
        bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();
        GameObject organ = bodyOrganManager.ActiveObject;

        if (isRotating)
        {
            if ( organ != null)
            {
                if (organ.activeSelf)
                {
                    organ.transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
                    ClampRotation(organ);
                }
            }
        }
        
    }


    private void ClampRotation(GameObject organ)
    {
        Vector3 angles = organ.transform.eulerAngles;
        angles.x = Mathf.Clamp(angles.x, -360f, 360f);
        angles.y = Mathf.Clamp(angles.y, -360f, 360f);
        angles.z = Mathf.Clamp(angles.z, -360f, 360f);
        organ.transform.eulerAngles = angles;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isRotating = true; 
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isRotating = false; 
        rotationAxis = Vector3.zero;
    }

    public void RotateUp()
    {
        rotationAxis = Vector3.right; 
    }

    public void RotateDown()
    {
        rotationAxis = -Vector3.right; 
    }

    public void RotateLeft()
    {
        rotationAxis = Vector3.up; 
    }

    public void RotateRight()
    {
        rotationAxis = -Vector3.up;
    }

}
