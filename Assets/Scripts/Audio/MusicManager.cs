using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixerGroup musicMixerGroup;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;

    [Header("Audio Source")]
    private AudioSource audioSource;

    // Singleton pattern
    public static MusicManager Instance { get; private set; }

    private string currentScene;
    private bool isInitialized = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMusicManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeMusicManager()
    {
        // Создаем AudioSource если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настраиваем AudioSource
        audioSource.outputAudioMixerGroup = musicMixerGroup;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Подписываемся на события смены сцены
        SceneManager.sceneLoaded += OnSceneLoaded;

        isInitialized = true;

        // Проигрываем музыку для текущей сцены
        PlayMusicForCurrentScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isInitialized) return;

        string sceneName = scene.name;
        // Проверяем, нужно ли менять музыку
        if (currentScene != sceneName)
        {
            currentScene = sceneName;
            PlayMusicForScene(sceneName);
            Debug.Log(sceneName);

        }
    }

    private void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        currentScene = sceneName;
        PlayMusicForScene(sceneName);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clipToPlay = GetMusicClipForScene(sceneName);

        if (clipToPlay != null)
        {
            // Если уже играет нужная музыка, не перезапускаем
            if (audioSource.clip == clipToPlay && audioSource.isPlaying)
                return;

            PlayMusic(clipToPlay);
        }
        else
        {
            StopMusic();
        }
    }

    private AudioClip GetMusicClipForScene(string sceneName)
    {
        switch (sceneName)
        {
            case Scenes.MAIN_MENU:
                return menuMusic;

            case Scenes.LEVEL1:
            case Scenes.LEVEL2:
            case Scenes.LEVEL3:
            case Scenes.LEVEL4:
            case Scenes.LEVEL5:
                return levelMusic;

            default:
                return null;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.UnPause();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Дополнительные методы для плавных переходов (опционально)
    public void FadeOutMusic(float fadeTime = 1f)
    {
        if (audioSource != null)
        {
            StartCoroutine(FadeOutCoroutine(fadeTime));
        }
    }

    public void FadeInMusic(AudioClip clip, float fadeTime = 1f)
    {
        if (audioSource != null && clip != null)
        {
            StartCoroutine(FadeInCoroutine(clip, fadeTime));
        }
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float fadeTime)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeTime);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }

    private System.Collections.IEnumerator FadeInCoroutine(AudioClip clip, float fadeTime)
    {
        audioSource.clip = clip;
        audioSource.volume = 0f;
        audioSource.Play();

        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeTime);
            yield return null;
        }

        audioSource.volume = 1f;
    }
}