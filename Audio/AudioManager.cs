using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            PlayAudioBg();
        }
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

    /// <summary>
    /// Phát voice của hero (Voices folder).
    /// heroFolder = tên folder trong Resources/AudioTuong/, ví dụ "Kayn", "Leona", "Torch"...
    /// keyword: "attack", "spellcast", "dying", "effort", "taunt", "laugh", "move"
    /// </summary>
    public void PlayHeroSound(string heroFolder, HeroSoundType type)
    {
        if (!isSound) return;
        if (string.IsNullOrEmpty(heroFolder)) return;

        string keyword = type switch
        {
            HeroSoundType.NormalAttack => "attack",
            HeroSoundType.Skill => "spellcast",
            HeroSoundType.Dying => "dying",
            HeroSoundType.Effort => "effort",
            HeroSoundType.Taunt => "taunt",
            HeroSoundType.Laugh => "laugh",
            HeroSoundType.Move => "move",
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

    /// <summary>
    /// Phát sound hiệu ứng skill của hero (Skills folder).
    /// heroFolder = tên folder trong Resources/AudioTuong/, ví dụ "Kayn", "Leona", "Torch"...
    /// Tìm file có tên chứa keyword "skill" (đặt tên file theo pattern: skill_1.mp3, skill_cast.mp3,...).
    /// Nếu không có file nào khớp, phát random bất kỳ clip trong folder.
    /// </summary>
    public void PlaySkillSound(string heroFolder)
    {
        if (!isSound) return;
        if (string.IsNullOrEmpty(heroFolder)) return;

        AudioClip[] clips = Resources.LoadAll<AudioClip>($"AudioTuong/{heroFolder}/Skills");
        if (clips == null || clips.Length == 0) return;

        // Ưu tiên file tên chứa "skill"
        List<AudioClip> matched = new List<AudioClip>();
        foreach (var c in clips)
        {
            if (c.name.ToLower().Contains("skill"))
                matched.Add(c);
        }

        // Nếu không có file nào khớp keyword, fallback phát random toàn bộ folder
        if (matched.Count == 0)
        {
            audioSound.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            return;
        }

        audioSound.PlayOneShot(matched[Random.Range(0, matched.Count)]);
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
        return Resources.Load<AudioClip>(path);
    }

    public void PlayOneShotByPath(string resourcesPath)
    {
        if (!isSound) return;
        AudioClip clip = Resources.Load<AudioClip>(resourcesPath);
        if (clip != null) audioSound.PlayOneShot(clip);
    }

    /// <summary>
    /// Phát âm thanh skill theo hero và slot.
    /// skillSlot = 0: đánh thường, 1/2/3: skill tương ứng
    /// </summary>
    public void PlayHeroSkillSound(string heroFolder, int skillSlot)
    {
        if (!isSound) return;
        if (string.IsNullOrEmpty(heroFolder)) return;

        string fileName = skillSlot switch
        {
            0 => "audio_normal_attack",
            1 => "audio_sk1",
            2 => "audio_sk2",
            3 => "audio_sk3",
            _ => ""
        };

        if (string.IsNullOrEmpty(fileName)) return;

        AudioClip clip = Resources.Load<AudioClip>($"AudioTuong/{heroFolder}/Skills/{fileName}");
        if (clip != null)
            audioSound.PlayOneShot(clip);
    }
}