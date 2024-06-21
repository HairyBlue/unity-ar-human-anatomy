using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LoadingBar : MonoBehaviour
{    
    [Header("Scene name current and next")]
    [SerializeField]
    private string currentScene;
    [SerializeField]
    private string nextScene;

    [SerializeField]
    private int loadingEnd;

    public Slider slider;
    private Coroutine  stopCourotine;
    private bool switchScene = false;

    SaveLoadManager saveLoadManager = new SaveLoadManager();

    void Start()
    {
        saveLoadManager.RemoveData();

        stopCourotine = StartCoroutine(Loading());
        if(SceneManager.GetActiveScene().name == currentScene){
            saveLoadManager.StartTime();
        }   
    }

    void Update(){
        if(switchScene){
            SceneManager.LoadSceneAsync(nextScene);
        }
    }

    IEnumerator Loading()
    {
        while(slider.value < loadingEnd)
        {
            slider.value++;
            yield return new WaitForSeconds(0.02f);
        }

       yield return new WaitForSeconds(3);
       switchScene = true;
       StopCoroutine(stopCourotine);
    }
}
