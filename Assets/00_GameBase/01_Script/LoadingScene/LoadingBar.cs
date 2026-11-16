using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadingBar : MonoBehaviour
{
    public Image loadingBar;
    void Start()
    {
        //cach 1
        loadingBar.fillAmount = 0; //ban dau cua thanh bar
        float fillDuration = 5f; //toc do
        float maxValue = 1f; // 
        
        loadingBar.DOFillAmount(maxValue, fillDuration).OnComplete(delegate
        {
            Debug.Log("Da load xong tien hanh vao game Scene");
            loadingBar.gameObject.SetActive(false);
            gameObject.SetActive(false);
            GoToSceneHome();
        }); // (max, thoigian).
        
       
    }

    private static void GoToSceneHome()
    {
        var scenePath= "00_GameBase/05_Scene/HomeScene";
        // SceneManager.LoadScene(scenePath); cach 1
        SceneManager.LoadSceneAsync(scenePath); //cach 2
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
