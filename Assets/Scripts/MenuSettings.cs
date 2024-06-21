using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UI;
public class MenuSettings: MonoBehaviour
{

    [Header("Personal Information")]
    public TMP_InputField Name;
    public TMP_Text PersonalInfoStatus;
    public TMP_Text Age;
    public TMP_Dropdown Gender;

    [Header("Server Information")]
    public TMP_InputField ServerAddress;
     public TMP_InputField ServerPort;
    public TMP_Text ServerStatus;
    public TMP_Text ServerInfo;
    public TMP_Text ErrorMessage;

    [Header("Menu Panel")]
    public GameObject menuPanel;
    SaveLoadManager saveLoadManager = new SaveLoadManager();

    void Update(){
        if (menuPanel != null && menuPanel.activeSelf){
            if (Name.text.Length == 0 && Age.text == "0"){
                PlayerData playerData = saveLoadManager.LoadPlayerData();
                    if (playerData != null){
                        Name.text = playerData.playerName;
                        Age.text = playerData.playerAge;
                        int genderIndex = Gender.options.FindIndex(x => x.text == playerData.playerGender);

                        if (genderIndex != -1)
                        {
                            Gender.SetValueWithoutNotify(genderIndex);
                        }
                        else
                        {
                            Gender.SetValueWithoutNotify(0);
                        }
                }

            } 

            if(ServerAddress.text.Length == 0){
                ServerData serverData = saveLoadManager.LoadServerData();

                if (serverData != null){
                    ServerAddress.text = serverData.serverAddress;
                    ServerPort.text = serverData.serverPort;
                    ServerStatus.text = serverData.serverStatus;
                }


            }
        }
    }
}
