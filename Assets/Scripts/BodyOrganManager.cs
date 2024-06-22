using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BodyOrganManager : MonoBehaviour
{
    [Header("Organs Pannel")]

    [SerializeField]
    private GameObject OrganPanel;

    [Header("Attached Organ Objects")]
    [SerializeField]
    private GameObject[] Organs;

    [Header("Attached Toggle Controller")]
     [SerializeField]
    private GameObject toggelControler;
    [SerializeField]
    private GameObject toggelZoomEnable;
    public GameObject zoomWrapper;

    SaveLoadManager saveLoadManager = new SaveLoadManager();
    
    public bool enableZooming = false;
    private Popups popups;


    public void OnClickOrganPanel(){
        OrganPanel.SetActive(!OrganPanel.activeSelf);
    }


    public void OnToggleOrgan(GameObject organToggler){

        saveLoadManager.SavaBodyOrganName("heart");
        organToggler.SetActive(true);
    }

    public void OnClickRemoveActive(){
        foreach(GameObject organ in Organs){
            if(organ.activeSelf){
                organ.SetActive(false);
            }
        } 
    }
    
    public void OnClickToggleControloer(){
        toggelControler.SetActive(!toggelControler.activeSelf); 
    }


    public void OnClickEnableZoom(){

        popups =  FindAnyObjectByType<Popups>();
        TMP_Text warmIfZoomingText = popups.warmIfZoomingText;

        enableZooming = true;
        toggelZoomEnable.SetActive(false);
        zoomWrapper.SetActive(true);
         warmIfZoomingText.text = "Warn: Zooming In or out will disable the body tracking. Reset it back if you want to track.";
    
    }

    public void ResetRotation()
    {

        popups =  FindAnyObjectByType<Popups>();
        TMP_Text warmIfZoomingText = popups.warmIfZoomingText;


        foreach (GameObject organ in Organs)
        {
            if (organ.activeSelf)
            {
                organ.transform.eulerAngles = Vector3.zero;
            }
        }
        enableZooming = false;
        toggelZoomEnable.SetActive(true);
        zoomWrapper.SetActive(false);
        warmIfZoomingText.text = "";
       
    }

}
