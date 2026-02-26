using UnityEngine;

public class DeepLinkHandler : MonoBehaviour
{
    public static DeepLinkHandler Instance { get; private set; }

    public System.Action<string> OnDeepLinkReceived;

    private string deeplinkURL;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += OnDeepLinkActivated;

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDeepLinkActivated(string url)
    {
        deeplinkURL = url;
        OnDeepLinkReceived?.Invoke(url);
    }

    public string GetDeepLink()
    {
        return deeplinkURL;
    }
}