using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject[] objectsToZoom;
    public float zoomSpeed = 10f; // Speed of zooming
    public float minZoom = -50f; // Minimum Z position
    public float maxZoom = 50f; // Maximum Z position

    public bool isZoomingIn = false;
    public bool isZoomingOut = false;

   
    void Update()
    {
        if (isZoomingIn)
        {
            ZoomIn();
        }
        else if (isZoomingOut)
        {
            ZoomOut();
        }
    }

    public void ZoomIn()
    {
        foreach (GameObject obj in objectsToZoom)
        {
            if (obj.activeSelf)
            {
                Vector3 newPosition = obj.transform.position + new Vector3(0, 0, zoomSpeed * Time.deltaTime);
                newPosition.z = Mathf.Clamp(newPosition.z, minZoom, maxZoom);
                newPosition.y = 0;
                newPosition.x = 0;
                obj.transform.position = newPosition;
            }
        }
    }

    public void ZoomOut()
    {
        foreach (GameObject obj in objectsToZoom)
        {
            if (obj.activeSelf)
            {
                Vector3 newPosition = obj.transform.position - new Vector3(0, 0, zoomSpeed * Time.deltaTime);
                newPosition.z = Mathf.Clamp(newPosition.z, minZoom, maxZoom);
                newPosition.y = 0;
                newPosition.x = 0;
                obj.transform.position = newPosition;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.name == "ZoomInButton")
        {
            isZoomingIn = true;
        }
        else if (eventData.pointerCurrentRaycast.gameObject.name == "ZoomOutButton")
        {
            isZoomingOut = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isZoomingIn = false;
        isZoomingOut = false;
        
    }
}
