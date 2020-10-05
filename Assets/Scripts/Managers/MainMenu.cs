using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // animations
    public Animator transition;
    // audio sources
    public AudioSource selectAudio;

    PlayerMovement player;

    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerMovement>();
        // Diable player control on main menu scene;
        player.DisablePlayerControl();
        // Disable cursor hiding
        Cursor.lockState = CursorLockMode.None;

        // playing music opening
    }

    public void LoadLevel1()
    {
        PlayMenuSelectAudio();
        // Disable menus and give user control of player
        player.EnablePlayerControl();
        this.gameObject.SetActive(false);
    }

    public void LoadLevel2()
    {
        PlayMenuSelectAudio();
        StartCoroutine(LoadScene("Level_2"));
    }

    public void LaodLevel3()
    {
        PlayMenuSelectAudio();
        StartCoroutine(LoadScene("Level_3"));
    }

    public void LaodLevel4()
    {
        PlayMenuSelectAudio();
        StartCoroutine(LoadScene("Level_4"));
    }

    public void LaodLevel5()
    {
        PlayMenuSelectAudio();
        //StartCoroutine(LoadScene("Level_5"));
    }

    public void LaodLevel6()
    {
        PlayMenuSelectAudio();
        //StartCoroutine(LoadScene("Level_6"));
    }

    public void PlayMenuSelectAudio()
    {
        selectAudio.PlayOneShot(selectAudio.clip, 0.7f);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    IEnumerator LoadScene(string levelName)
    {
        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName);
        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;

        // play btn click audio
        //btnClickAudio.Play(0);
        // play scene transtion animation
        transition.SetTrigger("Start");
        
        // wait for aniamtion to finish
        yield return new WaitForSeconds(1.00f);
        // allow scene to change
        asyncOperation.allowSceneActivation = true;
    }
}
