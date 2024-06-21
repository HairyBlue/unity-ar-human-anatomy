using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class SaveLoadManager
{
    private readonly string PlayerDataKey = Store.PlayerDataKey;
    private readonly string ServerDataKey = Store.ServerDataKey;
    private readonly string startTimeKey = Store.StartTimeKey;
    private readonly string endTimeKey = Store.EndTimeKey;
    private readonly string BodyOrganNameKey = Store.BodyOrganNameKey;

    public void SavePlayerData(string name, string age, string gender, string uuid){

        PlayerData playerData = new()
        {
            playerName = name,
            playerAge = age,
            playerGender = gender,
            playerUUID = uuid
        };

        string jsonData = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(PlayerDataKey, jsonData);
        PlayerPrefs.Save();
    }

    public PlayerData LoadPlayerData(){

        if (PlayerPrefs.HasKey(PlayerDataKey))
        {
            string jsonData = PlayerPrefs.GetString(PlayerDataKey);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);

            return playerData;
        }else{
            return null;
        }
    }

    public void SaveServerData(string serverAddr, string serverPort, string serverStatus){
        ServerData serverData = new()
        {
            serverAddress = serverAddr,
            serverPort = serverPort,
            serverStatus = serverStatus
        };

        string jsonData = JsonUtility.ToJson(serverData);
        PlayerPrefs.SetString(ServerDataKey, jsonData);
        PlayerPrefs.Save();
    }

    public ServerData LoadServerData(){
        if (PlayerPrefs.HasKey(PlayerDataKey))
        {
            string jsonData = PlayerPrefs.GetString(ServerDataKey);
            ServerData serverData = JsonUtility.FromJson<ServerData>(jsonData);

            return serverData;
        }else{
            return null;
        }
    }

    public void RemoveData(){
        bool hasRemove = false;

         if (PlayerPrefs.HasKey(PlayerDataKey)){
            PlayerPrefs.DeleteKey(PlayerDataKey);
            hasRemove = true;
         }
         if(PlayerPrefs.HasKey(ServerDataKey)){
            PlayerPrefs.DeleteKey(ServerDataKey);
            hasRemove = true;
         }
         if(PlayerPrefs.HasKey(startTimeKey)){
            PlayerPrefs.DeleteKey(startTimeKey);
            hasRemove = true;
         }
         if(PlayerPrefs.HasKey(endTimeKey)){
            PlayerPrefs.DeleteKey(endTimeKey);
            hasRemove = true;
         }

         if(hasRemove){
            PlayerPrefs.Save();
         }
    }


    public void StartTime(){
        string dateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        PlayerPrefs.SetString(startTimeKey, dateTime);
        PlayerPrefs.Save();
    }

    public void EndTime(){
        string dateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        PlayerPrefs.SetString(endTimeKey, dateTime);
        PlayerPrefs.Save();
    }

    public void SavaBodyOrganName(string name){
        BodyOrganName bodyOrganName = new()
        {
            name = name,
        };

        string jsonData = JsonUtility.ToJson(bodyOrganName);
        PlayerPrefs.SetString(BodyOrganNameKey, jsonData);
        PlayerPrefs.Save();
    }
}
