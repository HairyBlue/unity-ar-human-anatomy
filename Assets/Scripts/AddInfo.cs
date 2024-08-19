using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class AddInfo : MonoBehaviour
{
    [Header("Personal Information")]
    public TMP_InputField AddName;
    public TMP_Text AddPersonalInfoStatus;
    public TMP_Text AddAge;
    public TMP_Dropdown AddGender;

    public GameObject AddPersonalInfoPanel;
    private Coroutine personalInfoChange;


    SaveLoadManager saveLoadManager = new SaveLoadManager();
    private readonly string[] personalInfoTexts = Message.PersonalInfoText;
    private readonly string PlayerDataKey = Store.PlayerDataKey;


    private bool isValidToAdd = false;

    public void SaveAddedPersonalInfo(){

        if (PlayerPrefs.HasKey(PlayerDataKey)){
            PlayerPrefs.DeleteKey(PlayerDataKey);
            PlayerPrefs.Save();
         }

        if(personalInfoChange != null){
            StopCoroutine(personalInfoChange);
        }

        TMP_InputField name = AddName;
        TMP_Text age =  AddAge;
        TMP_Dropdown gender = AddGender;
        TMP_Text personalInfoStatus = AddPersonalInfoStatus;


        // string pattern = @"[0-9]+$";

        if(name != null & age != null){
            string nameStr = name.text;
            int ageInt = int.Parse(age.text);
            string gendetStr =  gender.options[gender.value].text;
            
            if(ageInt >= 10 && ageInt <= 100)
                {
                    personalInfoStatus.text = personalInfoTexts[0];
                    personalInfoStatus.color = Color.green;

                    AppLogger logger = gameObject.AddComponent<AppLogger>();

                    // SaveLoadManager saveLoadManager = gameObject.AddComponent<SaveLoadManager>();
                    Guid uuid = Guid.NewGuid();
                    string uuidString = uuid.ToString();

                    saveLoadManager.SavePlayerData(nameStr, ageInt.ToString(), gendetStr, uuidString);
                    logger.CreateLogFile();
                    isValidToAdd = true;
                    saveLoadManager.EndTime();
                    logger.LogOnBoarding();
                } 
                else
                {
                    personalInfoStatus.text = personalInfoTexts[1];
                    personalInfoStatus.color = Color.red;
                }
        }
        
        personalInfoChange = StartCoroutine(PersonalInfoChange(personalInfoStatus));
    }
    public void AddAgeCounter(string typeCount){
      
        TMP_Text age = AddAge;
    
        if(typeCount != null){

            int ageInt = int.Parse(age.text);
            if(typeCount == "ADD"){
                if(ageInt < 100)
                    ageInt++;
                    age.text = ageInt.ToString();
            }
            if(typeCount == "MINUS"){
                if (ageInt > 0)
                    ageInt--;
                    age.text = ageInt.ToString();
            }
        }
    }


    public void DoneAddPersonalInfo(){
        if(isValidToAdd){
            if(AddPersonalInfoPanel.activeSelf){
              AddPersonalInfoPanel.SetActive(false);
            }
        }
    }

    IEnumerator PersonalInfoChange(TMP_Text personalInfoStatus){

        yield return new WaitForSeconds(10);

        if(personalInfoStatus.text == personalInfoTexts[0]){
            personalInfoStatus.text = "";
        }else{
            personalInfoStatus.text = personalInfoTexts[2];
        }

    }
}
