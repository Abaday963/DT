using UnityEngine;
using YG;

public class AdManager : MonoBehaviour
{
    private float lastAdTime = 0f;
    private float adCooldown = 60f; // 60 секунд между показами

    public void TryShowAd()
    {
        if (Time.time - lastAdTime > adCooldown)
        {
            //YandexGame.FullscreenShow();
            lastAdTime = Time.time;
        }
    }
}