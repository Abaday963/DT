using System;
using UnityEngine;

public class UIGameplayRootBinder : MonoBehaviour
{
    public event Action GoToMainMenuButtonClicked;
    
    public void HandleGoMainMenuButtonClick()
    {
        GoToMainMenuButtonClicked?.Invoke();
    }
}
