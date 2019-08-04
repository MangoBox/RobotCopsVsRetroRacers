using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    public void PlayGame() {
        if(GameController.gc != null)
            GameController.gc.ResetStaticVariables();
        SceneManager.LoadScene("MainScene");
    }
    
    public void OpenManual() {
        Application.OpenURL("https://docs.google.com/document/d/1QLTqPDLJCCS7BAhdKmldkpMajT-T-8DQiqcKpP3gJV8/edit?usp=sharing");
    }
}
