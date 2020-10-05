using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region singleton
    public static GameManager instance;

    void Awake()
    {
        MakeSingleton();
    }

    void MakeSingleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    public Animator transition;
    public AudioSource gameMusic;

    PlayerMovement player;
    ProjectileManager projectileManager;


    void Start()
    {
        // Subscribe to player's input broadcast
        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerMovement>();
        player.OnButtonPress += CheckPlayerInput;
        // Subscribe to player's next stage broadcast
        player.OnNextStage += MoveToNextScene;

        projectileManager = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<ProjectileManager>();
        projectileManager.OnStartShadowPlayer += StopGameMusic;
    }

    void StopGameMusic()
    {
        gameMusic.Stop();
    }

    void CheckPlayerInput(KeyCode keyCode)
    {
        if (keyCode == KeyCode.Escape || keyCode == KeyCode.Tab)
        {
            // Unsubscribe from player
            player.OnButtonPress -= CheckPlayerInput;
            // Change scene to main menu
            StartCoroutine(LoadScene("Level_1"));
        }
    }

    void MoveToNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadSceneByIndex(nextSceneIndex));
        }
        else
        {
            // If no more next scenes return to Main Menu Scene
            StartCoroutine(LoadScene("Level_1"));
        }
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

    IEnumerator LoadSceneByIndex(int index)
    {
        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(index);
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
