using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseApp App { get; private set; }
    public bool IsReady { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                App = FirebaseApp.DefaultInstance;
                Auth = FirebaseAuth.DefaultInstance;
                IsReady = true;
            }
            else
            {
                IsReady = false;
            }
        });
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}