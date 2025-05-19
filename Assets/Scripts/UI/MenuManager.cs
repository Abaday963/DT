using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menuPanel;

    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }
}
