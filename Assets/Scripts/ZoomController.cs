using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // public GameObject[] objectsToZoom;
    public float zoomSpeed = 10f; // Speed of zooming
    public float minZoom = -50f; // Minimum Z position
    public float maxZoom = 50f; // Maximum Z position

    public bool isZoomingIn = false;
    public bool isZoomingOut = false;
    private BodyOrganManager bodyOrganManager;
   
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
        bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();
        GameObject organ = bodyOrganManager.ActiveObject;

        if (organ != null) {
            if (organ.activeSelf)
            {
                Vector3 newPosition = organ.transform.position + new Vector3(0, 0, zoomSpeed * Time.deltaTime);
                newPosition.z = Mathf.Clamp(newPosition.z, minZoom, maxZoom);
                newPosition.y = 0;
                newPosition.x = 0;
                organ.transform.position = newPosition;
            }
        }
    }

    public void ZoomOut()
    {
        bodyOrganManager = FindAnyObjectByType<BodyOrganManager>();
        GameObject organ = bodyOrganManager.ActiveObject;

        if (organ != null) {
            if (organ.activeSelf)
            {
                Vector3 newPosition = organ.transform.position - new Vector3(0, 0, zoomSpeed * Time.deltaTime);
                newPosition.z = Mathf.Clamp(newPosition.z, minZoom, maxZoom);
                newPosition.y = 0;
                newPosition.x = 0;
                organ.transform.position = newPosition;
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
