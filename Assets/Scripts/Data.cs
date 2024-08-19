using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public string playerAge;
    public string playerGender;
    public string playerUUID;
}

[System.Serializable]
public class ServerData {
    public string serverAddress;
    public string serverPort;
    public string serverStatus;
}

[System.Serializable]
public class MenuLog 
{
    public string topic = "menu";
    public string startTime;
    public string endTime;

    public string ellapsedTime;
}

[System.Serializable]
public class InstructionLog 
{
    public string topic = "instruction";
    public string startTime;
    public string endTime;
    public string ellapsedTime;

}


[System.Serializable]
public class BodyOrganLog 
{
    public string topic = "body-organ-ar";
    public string startTime;
    public string endTime;
    public string ellapsedTime;
}

[System.Serializable]
public class BodySystemLog 
{
    public string topic = "body-system-ar";
    public string startTime;
    public string endTime;
    public string ellapsedTime;
}


[System.Serializable]
public class OnboardingLog{
    public string topic = "onboarding";
    public string startTime;
    public string endTime;
    public string ellapsedTime;
}


[System.Serializable]
public class BodyOrganName{
    public string name;
}


[System.Serializable]
public class UserInfo
{
    public string uuid;
    public string age;
    public string gender;
}

[System.Serializable]
public class TopicInfo
{
    public string topic;
    public string startTime;
    public string endTime;
    public string ellapsedTime;
}