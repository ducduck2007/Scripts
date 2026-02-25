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

    public void AudioNormalAttack()
    {
        if (isSound)
        {
            audioSound.clip = LoadSound(PathAudio.NormalAttack);
            audioSound.PlayOneShot(audioSound.clip);
        }
    }

    // Audio cho giọng nhân vật
    public void PlayHeroSound(string heroFolder, HeroSoundType type)
    {
    if (!isSound) return;

    string keyword = type switch
    {
        HeroSoundType.NormalAttack => "attack",
        HeroSoundType.Skill        => "spellcast",
        HeroSoundType.Dying        => "dying",
        HeroSoundType.Effort       => "effort",
        HeroSoundType.Taunt        => "taunt",
        HeroSoundType.Laugh        => "laugh",
        HeroSoundType.Move         => "move",
        _ => ""
    };

    if (string.IsNullOrEmpty(keyword)) return;

    AudioClip[] clips = Resources.LoadAll<AudioClip>($"AudioTuong/{heroFolder}/Voices");
    if (clips == null || clips.Length == 0) return;

    List<AudioClip> list = new List<AudioClip>();
    foreach (var c in clips)
    {
        if (c.name.ToLower().Contains(keyword))
            list.Add(c);
    }

    if (list.Count == 0) return;

    audioSound.PlayOneShot(list[Random.Range(0, list.Count)]);
    }

    // Audio cho kỹ năng
    public void PlaySkillSound(string heroFolder)
    {
        if(!isSound) return;

        AudioClip[] clips = Resources.LoadAll<AudioClip>($"AudioTuong/{heroFolder}/Skills");
        if (clips == null || clips.Length == 0) return;
        
        List<AudioClip> list = new List<AudioClip>();
        foreach (var c in clips)
        {
            if (c.name.ToLower().Contains("Katana_Swing_Cut".ToLower()))
                list.Add(c);
        }
        if (list.Count == 0) return;
        audioSound.PlayOneShot(list[Random.Range(0, list.Count)]);
    }

    public enum HeroSoundType
    {
    NormalAttack,
    Skill,
    Dying,
    Effort,
    Taunt,
    Laugh,
    Move
    }

    private static AudioClip LoadSound(string path)
    {
        // path = path.Replace(".mp3", "");
        return Resources.Load<AudioClip>(path);
    }
    

}
