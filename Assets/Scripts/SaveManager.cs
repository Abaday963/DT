using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public PlayerSaveData playerData;
    private string localSavePath;
    private bool isYandexGamesAvailable = false;

    private void Awake()
    {
        // Синглтон паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSaveSystem()
    {
        playerData = new PlayerSaveData();
        localSavePath = Path.Combine(Application.persistentDataPath, "playerSave.json");

        // Проверяем, запущена ли игра на платформе Яндекс Игр
#if UNITY_WEBGL && !UNITY_EDITOR
            InitializeYandexGames();
#else
        LoadLocalSave();
#endif
    }

    // Локальное сохранение
    public void SaveLocal()
    {
        string jsonData = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(localSavePath, jsonData);
        Debug.Log("Game saved locally at: " + localSavePath);
    }

    // Локальная загрузка
    public void LoadLocalSave()
    {
        if (File.Exists(localSavePath))
        {
            string jsonData = File.ReadAllText(localSavePath);
            playerData = JsonUtility.FromJson<PlayerSaveData>(jsonData);
            Debug.Log("Game loaded from local save");
        }
        else
        {
            Debug.Log("No save file found, creating new save data");
            playerData = new PlayerSaveData();
            SaveLocal();
        }
    }

    // Методы для взаимодействия с данными
    public void SetPlayerName(string name)
    {
        playerData.playerName = name;
        SaveGame();
    }

    public void CompleteLevel(int level)
    {
        playerData.AddCompletedLevel(level);
        SaveGame();
    }

    // Общий метод сохранения - вызывает и локальное, и облачное сохранение
    public void SaveGame()
    {
        SaveLocal();

        if (isYandexGamesAvailable)
        {
            SaveToYandexGames();
        }
    }

    // Яндекс Игры интеграция - используем JSLib для связи с JS
    private void InitializeYandexGames()
    {
        // Вызываем JavaScript функцию для инициализации SDK Яндекс Игр
        // Это нужно реализовать через плагин для Unity WebGL
        StartCoroutine(InitializeYandexSDK());
    }

    private IEnumerator InitializeYandexSDK()
    {
        // Используем Application.ExternalEval или другой способ вызова JavaScript
#if UNITY_WEBGL && !UNITY_EDITOR
            // Здесь будет код для инициализации YaGames SDK
            // Примерно такой:
            // Application.ExternalEval("InitYandexGames()");
#endif

        yield return new WaitForSeconds(1);

        // Когда SDK инициализирован, пытаемся загрузить данные
        LoadFromYandexGames();
    }

    // Яндекс Игры - сохранение данных
    private void SaveToYandexGames()
    {
        string jsonData = JsonUtility.ToJson(playerData);

#if UNITY_WEBGL && !UNITY_EDITOR
            // Примерно такой код:
            // Application.ExternalEval($"SaveToYandexGames('{jsonData}')");
            Debug.Log("Saving to Yandex Games cloud");
#endif
    }

    // Яндекс Игры - загрузка данных
    private void LoadFromYandexGames()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Примерно такой код:
            // Application.ExternalEval("LoadFromYandexGames()");
            Debug.Log("Loading from Yandex Games cloud");
#endif

        // Вызываем JavaScript функцию, которая вызовет наш метод OnYandexDataLoaded когда данные будут получены
    }

    // Метод, который будет вызван из JavaScript когда данные с Яндекс Игр будут получены
    public void OnYandexDataLoaded(string jsonData)
    {
        if (!string.IsNullOrEmpty(jsonData))
        {
            playerData = JsonUtility.FromJson<PlayerSaveData>(jsonData);
            Debug.Log("Data loaded from Yandex Games");
        }
        else
        {
            // Если облачных данных нет, попробуем загрузить локальные
            LoadLocalSave();
        }

        isYandexGamesAvailable = true;
    }
}