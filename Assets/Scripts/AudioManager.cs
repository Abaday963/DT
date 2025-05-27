using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("UI Elements")]
    public Button muteButton;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;

    [Header("Mute Button Sprites")]
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private bool isMuted = false;
    private float[] savedVolumes = new float[2]; // Master, Music

    private void Start()
    {
        // Загружаем сохраненные настройки
        LoadAudioSettings();

        // Настраиваем UI
        if (muteButton != null)
            muteButton.onClick.AddListener(ToggleMute);

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Сохраняем текущие значения и выключаем звук
            audioMixer.GetFloat("Master", out savedVolumes[0]);
            audioMixer.GetFloat("MusicVolume", out savedVolumes[1]);

            audioMixer.SetFloat("Master", -80f);
            audioMixer.SetFloat("MusicVolume", -80f);

            if (muteButton != null)
                muteButton.GetComponent<Image>().sprite = soundOffSprite;
        }
        else
        {
            // Восстанавливаем сохраненные значения
            audioMixer.SetFloat("Master", savedVolumes[0]);
            audioMixer.SetFloat("MusicVolume", savedVolumes[1]);

            if (muteButton != null)
                muteButton.GetComponent<Image>().sprite = soundOnSprite;
        }

        SaveAudioSettings();
    }

    public void SetMasterVolume(float volume)
    {
        if (!isMuted)
        {
            float dbValue = Mathf.Log10(volume) * 20;
            if (volume == 0) dbValue = -80f;

            audioMixer.SetFloat("Master", dbValue);
            SaveAudioSettings();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (!isMuted)
        {
            float dbValue = Mathf.Log10(volume) * 20;
            if (volume == 0) dbValue = -80f;

            audioMixer.SetFloat("MusicVolume", dbValue);
            SaveAudioSettings();
        }
    }

    private void SaveAudioSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);

        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);

        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        // Загружаем значения слайдеров
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.8f);
            SetMasterVolume(masterVolumeSlider.value);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            SetMusicVolume(musicVolumeSlider.value);
        }

        // Загружаем состояние mute
        isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        if (muteButton != null)
            muteButton.GetComponent<Image>().sprite = isMuted ? soundOffSprite : soundOnSprite;
    }
}