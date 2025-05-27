using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject settingsPanel;

    // Переключатель: если что-то включено – выключить, иначе включить меню
    public void ToggleMenu()
    {
        if (menuPanel.activeSelf || settingsPanel.activeSelf)
        {
            menuPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }
        else
        {
            menuPanel.SetActive(true);
        }
    }

    // Новая функция — просто выключить всё
    public void CloseAll()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
}
