using UnityEngine;

namespace ToggleElementsApp
{
    public class SettingsButtion : MonoBehaviour
    {
        public GameObject objectToHide;
        public GameObject objectToShow;

        // Вызывается при нажатии на кнопку
        public void Toggle()
        {
            if (objectToHide != null) objectToHide.SetActive(false);
            if (objectToShow != null) objectToShow.SetActive(true);
        }
    }
}
