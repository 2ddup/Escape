using UnityEngine;
using UnityEngine.UI;

public class SoundSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider masterSlider;  // 마스터 볼륨 슬라이더
    public Slider bgmSlider;     // BGM 볼륨 슬라이더
    public Slider sfxSlider;     // SFX 볼륨 슬라이더

    [Header("Toggles")]
    public Toggle masterMuteToggle; // 마스터 음소거
    public Toggle bgmMuteToggle;    // BGM 음소거
    public Toggle sfxMuteToggle;    // SFX 음소거

    private void Start()
    {
        // 슬라이더 초기화 시 볼륨 반영
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // 토글 클릭 시 음소거 적용
        masterMuteToggle.onValueChanged.AddListener(OnMasterMuteToggled);
        bgmMuteToggle.onValueChanged.AddListener(OnBGMMuteToggled);
        sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggled);

       // 현재 AudioMixer 값 기준으로 슬라이더 초기화
        float value;

        if (SoundManager.Instance.audioMixer.GetFloat(SoundManager.Instance.masterVolumeParam, out value))
            masterSlider.value = Mathf.Pow(10, value / 20f);

        if (SoundManager.Instance.audioMixer.GetFloat(SoundManager.Instance.bgmVolumeParam, out value))
            bgmSlider.value = Mathf.Pow(10, value / 20f);

        if (SoundManager.Instance.audioMixer.GetFloat(SoundManager.Instance.sfxVolumeParam, out value))
            sfxSlider.value = Mathf.Pow(10, value / 20f);
    }

    // ---------------- Volume ----------------

    private void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }

    // ---------------- Mute ----------------

    private void OnMasterMuteToggled(bool isOn)
    {
        SoundManager.Instance.MuteMaster(isOn);
    }

    private void OnBGMMuteToggled(bool isOn)
    {
        SoundManager.Instance.MuteBGM(isOn);
    }

    private void OnSFXMuteToggled(bool isOn)
    {
        SoundManager.Instance.MuteSFX(isOn);
    }
}
