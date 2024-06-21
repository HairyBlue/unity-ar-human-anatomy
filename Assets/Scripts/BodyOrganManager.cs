using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    public void OnClickOrganPanel(){
        OrganPanel.SetActive(!OrganPanel.activeSelf);
    }

    public void OnToggleOrgan(GameObject organToggler){
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


    public void ResetRotation()
    {
        foreach (GameObject organ in Organs)
        {
            if (organ.activeSelf)
            {
                organ.transform.eulerAngles = Vector3.zero;
            }
        }
    }

}
