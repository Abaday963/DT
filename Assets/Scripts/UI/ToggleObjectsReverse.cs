using UnityEngine;

public class ToggleObjectsReverse : MonoBehaviour
{
    public GameObject objectToHide;
    public GameObject objectToShow;

    // Вызывается при нажатии на вторую кнопку
    public void ToggleBack()
    {
        if (objectToHide != null) objectToHide.SetActive(false);
        if (objectToShow != null) objectToShow.SetActive(true);
    }
}
