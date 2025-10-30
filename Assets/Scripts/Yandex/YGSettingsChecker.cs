using UnityEngine;
using YG;

// Повесьте этот скрипт на любой объект в первой сцене
public class YGSettingsChecker : MonoBehaviour
{
    [SerializeField] private bool showDebugInfo = true;

    private void Awake()
    {
        if (showDebugInfo)
        {
            Debug.Log("=== YG SETTINGS CHECK ===");
            Debug.Log($"Unity Editor: {Application.isEditor}");
            Debug.Log($"Platform: {Application.platform}");
        }
    }

    private void Start()
    {
        // Подписываемся на все важные события YG
        YG2.onGetSDKData += OnSDKData;

        if (showDebugInfo)
            Debug.Log("[YGChecker] Подписались на onGetSDKData");

        // Проверяем статус через секунду
        Invoke(nameof(CheckStatus), 1f);
        Invoke(nameof(CheckStatus), 3f);
        Invoke(nameof(CheckStatus), 5f);
    }

    private void OnDestroy()
    {
        YG2.onGetSDKData -= OnSDKData;
    }

    private void OnSDKData()
    {
        Debug.Log("=== 🎯 SDK DATA RECEIVED ===");
        CheckStatus();
    }

    private void CheckStatus()
    {
        Debug.Log("=== YG STATUS ===");
        Debug.Log($"SDK Enabled: {YG2.isSDKEnabled}");
        Debug.Log($"Saves null: {YG2.saves == null}");

        if (YG2.saves != null)
        {
            Debug.Log($"GameProgress: '{YG2.saves.GameProgress}'");
            Debug.Log($"GameProgress length: {YG2.saves.GameProgress?.Length ?? 0}");
            Debug.Log($"Last save time: {YG2.saves.lastSaveTime}");
        }
        else
        {
            Debug.LogWarning("⚠️ YG2.saves все еще NULL!");
        }
    }

    // Кнопка для ручной проверки (в билде можно вызвать из UI)
    public void ManualCheck()
    {
        CheckStatus();
    }

    // Тестовое сохранение
    public void TestSave()
    {
        if (YG2.saves == null)
        {
            Debug.LogError("Cannot save - YG2.saves is null!");
            return;
        }

        YG2.saves.GameProgress = $"{{\"test\":true,\"time\":\"{System.DateTime.Now}\"}}";
        YG2.SaveProgress();

        Debug.Log("✅ Test save executed!");

        Invoke(nameof(CheckStatus), 1f);
    }
}