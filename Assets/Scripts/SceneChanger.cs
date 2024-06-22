using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : MonoBehaviour
{
    AppLogger logger;
    SaveLoadManager saveLoadManager = new SaveLoadManager();

    public void LoadSceneFromBtnClick(string sceneName){
        logger =  gameObject.AddComponent<AppLogger>();
        string currentScene = SceneManager.GetActiveScene().name;
        if(sceneName == "Menu" && currentScene == Store.ARSceneBodyOrgan){
            saveLoadManager.EndTime();
            logger.LogBodyOrganTracker();
        }
        SceneManager.LoadScene(sceneName);
    }
}
