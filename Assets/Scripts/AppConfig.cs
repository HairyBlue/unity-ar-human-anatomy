using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class AppConfig: MonoBehaviour 
{
  SaveLoadManager saveLoadManager = new SaveLoadManager();
  private readonly string startTimeKey = Store.StartTimeKey;
  private readonly string endTimeKey = Store.EndTimeKey;
  private readonly string directoryLogs = Store.FolderLog;  
 

  private string fileName;

  private void RegisterFilePath(){
    PlayerData playerData = saveLoadManager.LoadPlayerData();
    string logsFolderPath  = Path.Combine(Application.persistentDataPath, directoryLogs);   


    if(!Directory.Exists(logsFolderPath))
    {
      Directory.CreateDirectory(logsFolderPath);
    }

    if(playerData != null && playerData.playerUUID != null){
      fileName = Path.Combine(logsFolderPath, playerData.playerUUID + ".log");
    }
  }

  private void logToFile(string text){
    RegisterFilePath();
    using (StreamWriter writer = new StreamWriter(fileName, true))
    {
      writer.WriteLineAsync(text);
    }
    
  }

  public string getStartTime(){
    if(PlayerPrefs.HasKey(startTimeKey)){
      return PlayerPrefs.GetString(startTimeKey);
    }


    PlayerPrefs.SetString(startTimeKey, "");
    PlayerPrefs.Save();
    return "";
  }
  public string getEndTime(){
    if(PlayerPrefs.HasKey(endTimeKey)){
      return PlayerPrefs.GetString(endTimeKey);
    }

    PlayerPrefs.SetString(endTimeKey, "");
    PlayerPrefs.Save();
    return "";
  }

  private void ResetTime(){
    PlayerPrefs.SetString(startTimeKey, "");
    PlayerPrefs.Save();

    PlayerPrefs.SetString(endTimeKey, "");
    PlayerPrefs.Save();
  }

  private  string GetElapsedTimeSeconds()
    {

      string startTime = getStartTime();
      string endTime = getEndTime();

      if(startTime.Length != 0 && endTime.Length != 0){
        
        long startTimeLong = long.Parse(startTime);
        long endTimeLong =  long.Parse(endTime);
        long elapsedMillis = endTimeLong - startTimeLong;

        return (elapsedMillis / 1000.0).ToString();
      }

      return "";
    }

  public void CreateLogFile(){
    PlayerData playerData = saveLoadManager.LoadPlayerData();
    string logsFolderPath  = Path.Combine(Application.persistentDataPath, directoryLogs); 
    if(!Directory.Exists(logsFolderPath))
    {
      Directory.CreateDirectory(logsFolderPath);
    }

    if (playerData != null)
    { 
      string logMsg = $"{{\"uuid\":\"{playerData.playerUUID}\",\"age\":\"{playerData.playerAge}\",\"gender\":\"{playerData.playerGender}\"}}";
      Debug.Log(logMsg);
      logToFile(logMsg);
    }
  }


  public void LogMenu(){
     string logsFolderPath  = Path.Combine(Application.persistentDataPath, directoryLogs); 
    if (Directory.Exists(logsFolderPath) )  
      {
        MenuLog log = new MenuLog
        {
            startTime = getStartTime(),
            endTime = getEndTime(),
            ellapsedTime = GetElapsedTimeSeconds()
        };

        string toJson = JsonUtility.ToJson(log);
        Debug.Log(toJson);
        logToFile(toJson);
        ResetTime();
      }
    }


 public void LogInstruction(){
    string logsFolderPath  = Path.Combine(Application.persistentDataPath, directoryLogs); 
      if (Directory.Exists(logsFolderPath) )  
        {
          InstructionLog log = new InstructionLog
          {
              startTime = getStartTime(),
              endTime = getEndTime(),
              ellapsedTime = GetElapsedTimeSeconds()
          };

          string toJson = JsonUtility.ToJson(log);
          Debug.Log(toJson);
          logToFile(toJson);
          ResetTime();
        }
    }

 public void LogOnBoarding(){
    string logsFolderPath  = Path.Combine(Application.persistentDataPath, directoryLogs); 
      if (Directory.Exists(logsFolderPath) )  
        {
          OnboardingLog log = new OnboardingLog
          {
              startTime = getStartTime(),
              endTime = getEndTime(),
              ellapsedTime = GetElapsedTimeSeconds()
          };

          string toJson = JsonUtility.ToJson(log);
          Debug.Log(toJson);
          logToFile(toJson);
          ResetTime();
        }
    }

}
