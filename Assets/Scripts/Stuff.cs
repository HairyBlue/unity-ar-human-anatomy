using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
public class Stuff : MonoBehaviour
{

    // Another wierd implementation

    static private string directoryPath = Store.FolderLog;

    public GameObject AddInfo;
    public GameObject IntsructionPanel;
    public GameObject instruction1;

    public GameObject AboutPanel;
    public GameObject ExitPanel;

    SaveLoadManager saveLoadManager = new SaveLoadManager();


    void Start() {
        OnboardingExist();
    }

    public void EnableInstruction(){

        if(IntsructionPanel != null && !IntsructionPanel.activeSelf){
            IntsructionPanel.SetActive(true);
            instruction1.SetActive(true);

            saveLoadManager.StartTime();   
        }
    }

    public void ClickAboutPanel(){
        if(AboutPanel != null){
            AboutPanel.SetActive(!AboutPanel.activeSelf);
        }
    }

    public void CloseExitPanel(){
        ExitPanel.SetActive(!ExitPanel.activeSelf);
    }

    public void QuitApp(){
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OnDownloadLogs()
    {
        string logsFolderPath = Path.Combine(Application.persistentDataPath, directoryPath);
        Debug.Log(Application.persistentDataPath);
        if (Directory.Exists(logsFolderPath))
        {
    
            string destinationFolderPath = Path.Combine(Application.persistentDataPath, Store.DownloadFolderLog);

            try
            {
                if (!Directory.Exists(destinationFolderPath))
                {
                    Directory.CreateDirectory(destinationFolderPath);
                }

                string[] files = Directory.GetFiles(logsFolderPath);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string destFilePath = Path.Combine(destinationFolderPath, fileName);
                    // Copy each file to the destination folder
                    File.Copy(file, destFilePath, true);
                }

                Debug.Log("Logs downloaded successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error downloading logs: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Logs folder does not exist.");
        }
    }

    public void OnboardingExist() {
        string logsFolderPath = Path.Combine(Application.persistentDataPath, directoryPath);
        Debug.Log(Application.persistentDataPath);

        PlayerData playerData = saveLoadManager.LoadPlayerData();
        
        if (Directory.Exists(logsFolderPath))
        {
            string file = Path.Combine(logsFolderPath, playerData.playerUUID + ".log" );

            if (File.Exists(file)) {
                string[] lines = File.ReadAllLines(file);

                foreach (string line in lines) {
                    if (string.IsNullOrWhiteSpace(line)) {
                        continue;
                    }
                    // UserInfo userInfo = JsonUtility.FromJson<UserInfo>(line);
                    // if (userInfo.uuid != null)
                    // {
                    //     Debug.Log($"UUID: {userInfo.uuid}, Age: {userInfo.age}, Gender: {userInfo.gender}");
                    //     continue;
                    // }
               
                    TopicInfo topicInfo = JsonUtility.FromJson<TopicInfo>(line);
                    if (topicInfo.topic != null)
                    {
                        Debug.Log($"Topic: {topicInfo.topic}, StartTime: {topicInfo.startTime}, EndTime: {topicInfo.endTime}, EllapsedTime: {topicInfo.ellapsedTime}");
                        if ( IntsructionPanel != null && topicInfo.topic == "onboarding") {                   
                            if (IntsructionPanel.activeSelf){
                                IntsructionPanel.SetActive(false);
                                instruction1.SetActive(false);
                            }
                            if ( AddInfo != null && AddInfo.activeSelf) {
                                AddInfo.SetActive(false);
                            }
                        }
                    }

                }
            }
        }

    }
}
