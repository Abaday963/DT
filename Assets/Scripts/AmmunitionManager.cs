using UnityEngine;
using UnityEngine.UI;
public class AmmunitionManager : MonoBehaviour
{
    public static AmmunitionManager Instance { get; private set; }

    [SerializeField] private int totalAmmunition = 3;
    [SerializeField] private int remainingAmmunition;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Image[] ammunitionIcons;

    private void Awake()
    {
        // Singleton implementation
        if (Instance == null)
        {
            Instance = this;
            // Удалите DontDestroyOnLoad отсюда
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Переместите это в метод инициализации
        ResetAmmunition();
    }

    // Новый метод для сброса патронов
    public void ResetAmmunition()
    {
        remainingAmmunition = totalAmmunition;

        // Активируем все иконки патронов
        if (ammunitionIcons != null)
        {
            for (int i = 0; i < ammunitionIcons.Length; i++)
            {
                if (ammunitionIcons[i] != null)
                {
                    ammunitionIcons[i].gameObject.SetActive(true);
                }
            }
        }
    }

    public void OnAmmunitionImpact()
    {
        if (remainingAmmunition > 0)
        {
            remainingAmmunition--;
            if (ammunitionIcons != null && remainingAmmunition >= 0 && remainingAmmunition < ammunitionIcons.Length)
            {
                ammunitionIcons[remainingAmmunition].gameObject.SetActive(false);
            }

            if (remainingAmmunition <= 0)
            {
                gameManager.OnAllShardsThrown();
            }
        }
    }

    public int GetRemainingAmmunition()
    {
        return remainingAmmunition;
    }

    public int GetTotalAmmunition()
    {
        return totalAmmunition;
    }
}