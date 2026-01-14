using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : ManualSingleton<AudioManager>
{
    [SerializeField] private AudioSource audioBg;
    [SerializeField] private AudioSource audioSound;
    private static bool isMusic = true;
    private static bool isSound = true;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SetSoundConfig();
    }
    
    public void SetSoundConfig()
    {
        SetUpMusicStartGame();
        SetUpSoundStartGame();
    }
    
    private void SetUpMusicStartGame()
    {
        isMusic = AgentUnity.GetInt(KeyLocalSave.PP_AudioBg) != 1;
        if (!isMusic)
        {
            audioBg.Stop();
        }
    }
    private void SetUpSoundStartGame()
    {
        isSound = AgentUnity.GetInt(KeyLocalSave.PP_AudioSound) != 1;
        if (!isSound)
        {
            audioSound.Stop();
        }
    }
    
    public bool GetSoundConfig()
    {
        return isSound;
    }
    
    public bool GetMusicConfig()
    {
        return isMusic;
    }
    
    public void PlayAudioBg()
    {
        if (isMusic)
        {
            audioBg.clip = LoadSound(PathAudio.Background);
            audioBg.Play();
        }
        else
        {
            audioBg.Stop();
        }
    }

    public void SetVolumeBg(float volume = 0.35f)
    {
        audioBg.volume = volume;
    }

    public void StopAudioBg()
    {
        audioBg.Stop();
    }

    public void AudioChat()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.Chat);
            audioSound.Play();
        }
    }
    
    public void AudioClick()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.Click);
            audioSound.Play();
        }
    }
    
    public void AudioHoanThanhNhiemVu()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.HoanThanhNhiemVu);
            audioSound.Play();
        }
    }
    
    public void AudioMuaDoTrongShop()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.MuaDoTrongShop);
            audioSound.Play();
        }
    }
    
    public void AudioOpenItem()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.OpenItem);
            audioSound.Play();
        }
    }
    
    public void AudioTinNhanDen()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.TinNhanDen);
            audioSound.Play();
        }
    }

    
    public void AudioVang()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.Vang);
            audioSound.Play();
        }
    }
    
    
    private static AudioClip LoadSound(string path)
    {
        // path = path.Replace(".mp3", "");
        return Resources.Load<AudioClip>(path);
    }
    

}
