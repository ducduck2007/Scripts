using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;

public class VoiceChatManager : MonoBehaviour
{
    public static VoiceChatManager Instance { get; private set; }

    private bool isInitialized = false;
    private bool isLoggedIn = false;
    private string currentChannelName = null;
    private bool isMuted = false;

    public event Action OnVoiceConnected;
    public event Action OnVoiceDisconnected;
    public event Action<bool> OnMuteChanged;
    public event Action<string> OnParticipantJoined;
    public event Action<string> OnParticipantLeft;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeVivox();
    }

    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("VoiceChatManager");
            go.AddComponent<VoiceChatManager>();
        }
    }

    private async Task InitializeVivox()
    {
        try
        {
            if (isInitialized) return;

            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            VivoxService.Instance.LoggedIn += OnLoggedIn;
            VivoxService.Instance.LoggedOut += OnLoggedOut;
            VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
            VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

            isInitialized = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Voice] Init failed: {e.Message}");
        }
    }

    // ========== LOGIN ==========

    public async Task LoginAsync()
    {
        try
        {
            if (!isInitialized)
            {
                await InitializeVivox();
            }

            if (isLoggedIn) return;

            LoginOptions options = new LoginOptions();
            await VivoxService.Instance.LoginAsync(options);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Voice] Login failed: {e.Message}");
        }
    }

    // ========== JOIN CHANNEL ==========

    public async void JoinChannel(string channelName)
    {
        try
        {
            if (!isInitialized || !isLoggedIn)
            {
                await LoginAsync();
            }

            if (currentChannelName == channelName)
            {
                Debug.Log($"[Voice] Already in channel: {channelName}");
                return;
            }

            if (currentChannelName != null)
            {
                await LeaveCurrentChannel();
            }

            RequestMicPermission();

            await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);

            currentChannelName = channelName;

            VivoxService.Instance.UnmuteInputDevice();
            VivoxService.Instance.UnmuteOutputDevice();
            isMuted = false;
            OnVoiceConnected?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Voice] Join channel failed: {e.Message}");
        }
    }

    // ========== LEAVE CHANNEL ==========

    public async void LeaveChannel()
    {
        try
        {
            await LeaveCurrentChannel();
        }
        catch (Exception e)
        {
            Debug.LogError($"[Voice] Leave channel failed: {e.Message}");
        }
    }

    private async Task LeaveCurrentChannel()
    {
        if (currentChannelName == null) return;

        string leaving = currentChannelName;

        try
        {
            await VivoxService.Instance.LeaveChannelAsync(currentChannelName);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Voice] Leave error (ignored): {e.Message}");
        }

        currentChannelName = null;
        OnVoiceDisconnected?.Invoke();
    }

    public void ToggleMute()
    {
        SetMute(!isMuted);
    }

    public void SetMute(bool mute)
    {
        try
        {
            if (mute)
            {
                VivoxService.Instance.MuteInputDevice();
            }
            else
            {
                VivoxService.Instance.UnmuteInputDevice();
            }

            isMuted = mute;
            OnMuteChanged?.Invoke(isMuted);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Voice] Mute failed: {e.Message}");
        }
    }

    public async void Logout()
    {
        try
        {
            if (currentChannelName != null)
            {
                await LeaveCurrentChannel();
            }

            if (isLoggedIn)
            {
                await VivoxService.Instance.LogoutAsync();
                isLoggedIn = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Voice] Logout error: {e.Message}");
        }
    }

    private void OnLoggedIn()
    {
        isLoggedIn = true;
    }

    private void OnLoggedOut()
    {
        isLoggedIn = false;
        currentChannelName = null;
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        if (!participant.IsSelf)
        {
            OnParticipantJoined?.Invoke(participant.DisplayName);
        }
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        if (!participant.IsSelf)
        {
            OnParticipantLeft?.Invoke(participant.DisplayName);
        }
    }

    // ========== MIC PERMISSION ==========

    private void RequestMicPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
        }
#endif
    }

    private async void OnDestroy()
    {
        try
        {
            if (currentChannelName != null)
            {
                await LeaveCurrentChannel();
            }

            if (isLoggedIn)
            {
                await VivoxService.Instance.LogoutAsync();
            }

            if (isInitialized)
            {
                VivoxService.Instance.LoggedIn -= OnLoggedIn;
                VivoxService.Instance.LoggedOut -= OnLoggedOut;
                VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
                VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Voice] Cleanup error: {e.Message}");
        }
    }

    public bool IsInChannel => currentChannelName != null;
    public bool IsMuted => isMuted;
    public bool IsLoggedIn => isLoggedIn;
    public string CurrentChannel => currentChannelName;
}