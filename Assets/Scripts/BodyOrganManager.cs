using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Linq;

public class BodyOrganManager : MonoBehaviour
{
    [Header("Organs Pannel and Instruction")]

    [SerializeField]
    private GameObject OrganPanel;
    [SerializeField]
    private GameObject bodyOrganInstruction;

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

    public void OnClickViewInstruction() {
        bodyOrganInstruction.SetActive(!bodyOrganInstruction.activeSelf);
    }

    public void CheckObjectOrganNames(string organNameLC) {
         string[] supportedOrgans = new string[7]{"brain",  "heart", "lungs", "kidney", "liver", "stomach", "intestine"};
         int idx = Array.IndexOf(supportedOrgans, organNameLC);
         if (idx == -1) {
            string joinStr =  String.Join(", ", supportedOrgans);
            Debug.LogError("Either wrong type or wrong spelling object name of organ under AR Camera, => " + organNameLC + " | Supported Organ Names ff: " + joinStr);
    
         }

    }
    public void OnToggleOrgan(GameObject organToggler){
        string organNameLC = organToggler.name.ToLower();

        OnClickRemoveActive();
        CheckObjectOrganNames(organNameLC);
        saveLoadManager.SavaBodyOrganName(organNameLC);
        // Debug.Log(organToggler.name.ToLower());
        organToggler.SetActive(true);
    }

    public void OnClickRemoveActive(){
        foreach(GameObject organ in Organs){
            if (organ.activeSelf) {
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
