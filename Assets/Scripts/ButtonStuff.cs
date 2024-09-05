using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

// Todo: Fix
// fix what?
public class ButtonStuff : MonoBehaviour
{
     // Some Stuff for button a wierd implementaion but it works
    SaveLoadManager saveLoadManager = new SaveLoadManager();
    AppLogger logger;

    private TcpClient client;
    private Coroutine checkValidAddress;
    private Coroutine personalInfoChange;

    private Coroutine removePopupText;
    private readonly string[] personalInfoTexts = Message.PersonalInfoText;
    private readonly string[] serverInfoTexts = Message.ServerInfoText;

    private Instruction menuInstruction;
    private MenuSettings menuSettings;

    // text stuff to be popup onlick
    private Popups popups;


    public void NextInstruction(){
        menuInstruction = FindObjectOfType<Instruction>();
        GameObject current = menuInstruction.currentInstruction;
        GameObject next =  menuInstruction.nextInstruction;

        if(next != null & current.activeSelf){
            next.SetActive(true);
            current.SetActive(false);
        }
        
    }

    public void PrevInstruction(){
        menuInstruction = FindObjectOfType<Instruction>();
        GameObject current = menuInstruction.currentInstruction;
        GameObject prev =  menuInstruction.previousInstruction;

        if(prev != null & current.activeSelf){
            prev.SetActive(true);
            current.SetActive(false);
        }
    }

    public void EndInstruction(){
        menuInstruction = FindObjectOfType<Instruction>();
        GameObject current = menuInstruction.currentInstruction;
        GameObject next =  menuInstruction.nextInstruction;
        GameObject prev =  menuInstruction.previousInstruction;

        if(prev != null & next != null & current.activeSelf){
            current.SetActive(false);
            next.SetActive(false);
            prev.SetActive(false);
            PlayerData playerData = saveLoadManager.LoadPlayerData();
            
            logger =  gameObject.AddComponent<AppLogger>();
            if(playerData != null){
                saveLoadManager.EndTime();
                logger.LogInstruction();
            }
        }
    }

    public void EnableMenu(){
        menuSettings = FindAnyObjectByType<MenuSettings>();
        GameObject menu = menuSettings.menuPanel;
        logger =  gameObject.AddComponent<AppLogger>();

        if(menu != null){
            if(!menu.activeSelf){
                menu.SetActive(true);
                saveLoadManager.StartTime();        
            }else{
                menu.SetActive(false);
                saveLoadManager.EndTime();
                logger.LogMenu();
            }
        }
    }

    public void SavePersonalInfo(){

        PlayerData playerData = saveLoadManager.LoadPlayerData();
        if(personalInfoChange != null){
            StopCoroutine(personalInfoChange);
        }

        menuSettings = FindAnyObjectByType<MenuSettings>();
        TMP_InputField name = menuSettings.Name;
        TMP_Text age = menuSettings.Age;
        TMP_Dropdown gender = menuSettings.Gender;
        TMP_Text personalInfoStatus = menuSettings.PersonalInfoStatus;


        // string pattern = @"[0-9]+$";

        if(name != null & age != null){
            string nameStr = name.text;
            int ageInt = int.Parse(age.text);
            string gendetStr =  gender.options[gender.value].text;
            
            if(ageInt >= 10 && ageInt <= 100)
                {
                    if(
                        playerData != null && 
                        playerData.playerName != nameStr || 
                        playerData.playerGender != gendetStr || 
                        playerData.playerAge != age.text
                       )
                    {
                        // "Successfully Save"
                        personalInfoStatus.text = personalInfoTexts[0];
                        personalInfoStatus.color = Color.green;

                        // ***************************************************************************
                        // if want to create another unique id for the current user, onboarding log will not be capture
                        // Guid uuid = Guid.NewGuid();
                        // string uuidString = uuid.ToString();
                        // config.CreateLogFile();
                        // ***************************************************************************

                        // personal info change but same uuid
                        string currentUUID = playerData.playerUUID;
                        if(currentUUID != null){
                            logger =  gameObject.AddComponent<AppLogger>();
                            saveLoadManager.SavePlayerData(nameStr, ageInt.ToString(), gendetStr, currentUUID);
                            logger.LogPlayerInfoChange();
                        }                       
                    }else{

                        // "Personal Information Already Save"
                        personalInfoStatus.text = personalInfoTexts[3];
                        personalInfoStatus.color = Color.white;
                    }

                } 
                else
                {
                    //"Age must not below 10 or above 100"
                    personalInfoStatus.text = personalInfoTexts[1];
                    personalInfoStatus.color = Color.red;
                }
        }
        
        personalInfoChange = StartCoroutine(PersonalInfoChange(personalInfoStatus));
    }

    public void AgeCounter(string typeCount){
        menuSettings = FindAnyObjectByType<MenuSettings>();
        TMP_Text age = menuSettings.Age;
    
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

    public void ConnectServer(){

        menuSettings = FindAnyObjectByType<MenuSettings>();
        string domainPattern = @"^(?!\-)([A-Za-z0-9\-]{1,63}(?<!\-)\.)+[A-Za-z]{2,}$";
        string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
        string portPattern = @"^\d{2,}$";
        
        TMP_InputField serverAddr = menuSettings.ServerAddress;
        TMP_InputField serverPort = menuSettings.ServerPort;

        TMP_Text serverStatus = menuSettings.ServerStatus;
        TMP_Text serverInfo = menuSettings.ServerInfo;
        TMP_Text serverErrorMsg = menuSettings.ErrorMessage;
        
        if(checkValidAddress != null){
            StopCoroutine(checkValidAddress);
        }
    
        if(serverAddr != null && serverPort != null){            
            string serverStr = serverAddr.text.Trim();
            string portStr = serverPort.text.Trim();

            bool isValidAddress = Regex.IsMatch(serverStr, ipPattern) || Regex.IsMatch(serverStr, domainPattern);
            bool isValidPort = Regex.IsMatch(portStr, portPattern);

            if (isValidAddress && isValidPort) {
                serverInfo.text = serverInfoTexts[3];
                serverInfo.color = Color.green;
                StartCoroutine(CheckServer(serverStr, portStr, serverStatus, serverErrorMsg));
            } else {
                if (!isValidAddress && !isValidPort) { 
                    serverInfo.text = serverInfoTexts[0];
                } else if (!isValidAddress) {
                    serverInfo.text = serverInfoTexts[1];
                } else if (!isValidPort) {
                    serverInfo.text = serverInfoTexts[2];
                }
                serverInfo.color = Color.red;
            }
        
             checkValidAddress = StartCoroutine(CheckValidAddress(serverInfo));
        }

    }


    public void OnArBodyOrganClick(){
        popups = FindAnyObjectByType<Popups>();

        TMP_Text toARScenceButtonErrorText = popups.ToARScenceButtonErrorText;
        ServerData serverData = saveLoadManager.LoadServerData();
        
        if(serverData == null){
            toARScenceButtonErrorText.text = Message.ServerInfoText[4];
            StartCoroutine(RemovePopupText(toARScenceButtonErrorText));
        }
        else{
            if(serverData.serverStatus == Store.ServerOffline){
                toARScenceButtonErrorText.text = Message.ServerInfoText[4];
                StartCoroutine(RemovePopupText(toARScenceButtonErrorText));
            }
            else
            {
                saveLoadManager.StartTime();
                SceneManager.LoadScene(Store.ARSceneBodyOrgan);
            }
        }

    }

    IEnumerator CheckValidAddress(TMP_Text serverInfo){

        yield return new WaitForSeconds(6);
        serverInfo.text = "";


        // serverStatus.text = "Trying to Connect...";
        // serverStatus.color = Color.gray;

        // yield return new WaitForSeconds(5);

        // serverStatus.text = "Server not Found";
        // serverStatus.color = Color.red;

        // yield return new WaitForSeconds(3);

        // serverStatus.text = "Not Connected";
        // serverStatus.color = Color.black;
    
    }

    IEnumerator CheckServer(string serverStr, string portStr, TMP_Text serverStatus, TMP_Text serverErrorMsg){
        serverErrorMsg.text = ""; 
        serverStatus.text = "Trying to Connect...";
        serverStatus.color = Color.gray;
        yield return new WaitForSeconds(5);

        // SaveLoadManager saveLoadManager = gameObject.AddComponent<SaveLoadManager>();
        try {
            client = new TcpClient(serverStr, int.Parse(portStr));
            if(client.Connected){
                serverStatus.text = Store.ServerOnline;
                serverStatus.color = Color.green;
                saveLoadManager.SaveServerData(serverStr, portStr, serverStatus.text);
            }
        }catch (Exception e){
            serverErrorMsg.text = e.Message;
            serverStatus.text = Store.ServerOffline;
            serverStatus.color = Color.black;
            saveLoadManager.SaveServerData(serverStr, portStr, serverStatus.text);

        }

        yield return new WaitForSeconds(10);
        serverErrorMsg.text = ""; 
    }


    IEnumerator PersonalInfoChange(TMP_Text personalInfoStatus){

        yield return new WaitForSeconds(10);

        if(personalInfoStatus.text == personalInfoTexts[0]){
            personalInfoStatus.text = "";
        }else if(personalInfoStatus.text == personalInfoTexts[1]) {
            personalInfoStatus.text = personalInfoTexts[2];
            personalInfoStatus.color = Color.black;
        }

        yield return new WaitForSeconds(5);
        personalInfoStatus.text = "";
    }

    IEnumerator RemovePopupText(TMP_Text clickARButtonText){
       yield return new WaitForSeconds(5);
       clickARButtonText.text = "";
    }

}
