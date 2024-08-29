using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Store
{
    // Directory Logs
    public static readonly string FolderLog = "UserLogs";
    public static readonly string DownloadFolderLog = "DownloadedLogs";


    // Persisten Keys
    public static readonly string PlayerDataKey = "PlayerDataKey";
    public static readonly string ServerDataKey = "ServerDataKey";
    public static readonly string StartTimeKey = "StartTimeKey";
    public static readonly string EndTimeKey = "EndTimeKey";
    public static readonly string BodyOrganNameKey = "BodyOrganNameKey";

    // Server
    public static readonly string ServerOnline = "Online";
    public static readonly string ServerOffline = "Offline";

    // AR Scene
    public static readonly string ARSceneBodyOrgan = "BodyOrganTracker";
    public static readonly string ARSceneSystemOrgan = "BodySystemTracker";
}

[System.Serializable]
public class Message{

    public static readonly string[] PersonalInfoText = 
    {
        "Successfully Save", 
        "Age must not below 10 or above 100",
        "Personal Information Was Not Save",
        "Information Already Save"
    };

    public static readonly string[] ServerInfoText = 
    {
        "Invalid Server Address and Port",
        "Invalid Server Address",
        "Invalid Server Port",
        "Valid Server Address and Port",
        "You are not connected to the server. Please Go to the MENU to connect to the server"
    };

}