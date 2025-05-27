using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // 싱글톤 인스턴스

    [Header("Mixer Settings")]
    public AudioMixer audioMixer; // AudioMixer 연결
    public AudioMixerGroup bgmMixerGroup; // BGM용 AudioMixerGroup
    public AudioMixerGroup sfxMixerGroup; // SFX용 AudioMixerGroup

    [Header("Audio Sources")]
    public AudioSource bgmSource; // BGM 재생용 AudioSource
    public AudioSource sfxSource; // SFX 재생용 AudioSource

    [Header("Volume Exposed Parameters")]
    public string masterVolumeParam = "MasterVolume"; // AudioMixer의 Master 볼륨 이름
    public string bgmVolumeParam = "BGMVolume";       // BGM 볼륨 이름
    public string sfxVolumeParam = "SFXVolume";       // SFX 볼륨 이름

    [Header("BGM Settings")]
    public AudioClip defaultBGM; // 씬 이름이 없을 때 기본으로 재생할 BGM
    public List<BGMEntry> bgmEntries; // 씬별 BGM 리스트

    [Header("SFX Settings")]
    public List<SFXEntry> sfxEntries; // SFX 목록

    private Dictionary<string, AudioClip> sfxDict; // SFX 키로 찾기 위한 Dictionary

    [System.Serializable]
    public class BGMEntry
    {
        public string sceneName;   // 씬 이름
        public AudioClip bgmClip;  // 해당 씬의 BGM
    }

    [System.Serializable]
    public class SFXEntry
    {
        public string key;         // SFX 키값 (예: "ItemPickup")
        public AudioClip clip;     // 재생할 SFX 오디오
    }

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시 파괴되지 않도록
            InitSFXDictionary();           // SFX Dictionary 초기화
        }
        else
        {
            Destroy(gameObject); // 이미 존재하면 중복 제거
        }
    }

    private void OnEnable()
    {
        // 씬 로드 시 이벤트 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 이름에 맞는 BGM 재생
        PlaySceneBGM(scene.name);
    }

    private void InitSFXDictionary()
    {
        // SFX 리스트를 Dictionary로 변환
        sfxDict = sfxEntries.ToDictionary(e => e.key, e => e.clip);
    }

    // ---------------- BGM ----------------

    public void PlaySceneBGM(string sceneName)
    {
        // 씬 이름에 맞는 BGM을 찾아서 재생, 없으면 기본 BGM
        AudioClip clip = bgmEntries.FirstOrDefault(e => e.sceneName == sceneName)?.bgmClip ?? defaultBGM;
        PlayBGM(clip);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // BGM 설정 및 재생
        bgmSource.clip = clip;
        bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // ---------------- SFX ----------------

    public void PlaySFX(string key, bool loop = false)
    {
        // key에 해당하는 SFX를 재생
        if (sfxDict.TryGetValue(key, out AudioClip clip))
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            sfxSource.clip = clip;

            // 반복 여부 설정
            sfxSource.loop = loop;

            if (loop)
            {
                sfxSource.Play(); // 반복 재생
            }
            else
            {
                sfxSource.PlayOneShot(clip); // 한 번만 재생
            }
        }
        else
        {
            Debug.LogWarning($"SFX key not found: {key}");
        }
    }
    
    public void StopLoopingSFX()
    {
        // 현재 sfxSource가 재생 중이고 루프 상태일 때만 중지
        if (sfxSource.isPlaying && sfxSource.loop)
        {
            sfxSource.Stop();
            sfxSource.loop = false; // 다음 재생을 위해 루프 상태 해제
            sfxSource.clip = null;  // 클립 참조 제거 (선택 사항)
        }
    }

    // ---------------- Volume Control ----------------

    public void SetMasterVolume(float volume)
    {
        float dB = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(masterVolumeParam, dB);
    }

    public void SetBGMVolume(float volume)
    {
        float dB = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(bgmVolumeParam, dB);
    }

    public void SetSFXVolume(float volume)
    {
        float dB = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
        audioMixer.SetFloat(sfxVolumeParam, dB);
    }

    public void MuteMaster(bool isMuted)
    {
        // 마스터 음소거 (소리를 거의 꺼지게 만들기 위해 -80dB 사용)
        audioMixer.SetFloat(masterVolumeParam, isMuted ? -80f : 0f);
    }

    public void MuteBGM(bool isMuted)
    {
        // BGM 음소거
        audioMixer.SetFloat(bgmVolumeParam, isMuted ? -80f : 0f);
    }

    public void MuteSFX(bool isMuted)
    {
        // SFX 음소거
        audioMixer.SetFloat(sfxVolumeParam, isMuted ? -80f : 0f);
    }
}
