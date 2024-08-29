using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Why
public class SceneChanger : MonoBehaviour
{
    AppLogger logger;
    SaveLoadManager saveLoadManager = new SaveLoadManager();
    private Stuff stuff;

    public void LoadSceneFromBtnClick(string sceneName){
        logger =  gameObject.AddComponent<AppLogger>();
        stuff = gameObject.AddComponent<Stuff>();

        string currentScene = SceneManager.GetActiveScene().name;

        if(sceneName == "Menu" && currentScene == Store.ARSceneBodyOrgan){
            saveLoadManager.EndTime();
            logger.LogBodyOrganTracker();
        }
        
        SceneManager.LoadScene(sceneName);
    }

}
