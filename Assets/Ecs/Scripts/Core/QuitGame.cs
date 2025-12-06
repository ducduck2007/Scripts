using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGame : MonoBehaviour
{
    // Delays quitting for 2 seconds and
    // loads the finalsplash level during that time.

    float showSplashTimeout = 2.0f;
    bool  allowQuitting = false;

    void Awake()
    {
        // This game object needs to survive multiple levels
        Application.DontDestroyOnLoad(this.gameObject);
    }

    void OnApplicationQuit()
    {
        // If we haven't already load up the final splash screen level
        if (Application.loadedLevelName.ToLower() != "finalsplash")
        {
            StartCoroutine("DelayedQuit");
        }

        // Don't allow the user to exit until we got permission in
        if (!allowQuitting)
        {
            Application.CancelQuit();
        }
    }

    IEnumerator DelayedQuit()
    {
        Application.LoadLevel("finalsplash");

        // Wait for showSplashTimeout
        yield return new WaitForSeconds(showSplashTimeout);
        //SendData.OnDisconnectServer();
        // then quit for real
        allowQuitting = true;
        Application.Quit();
    }
    
}
