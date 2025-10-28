using UnityEngine;

public static class UIPanelSettingsHelper
{
    // 메시지 위치
    public static Vector2 GetCenterPosition() => Vector2.zero;
    public static Vector2 GetUpperPosition(float offsetY = 100f) => new Vector2(0, offsetY);

    // Fade 시간 구조체
    public struct FadeSettings
    {
        public float fadeInTime;
        public float displayTime;
        public float fadeOutTime;

        public FadeSettings(float fadeIn, float display, float fadeOut)
        {
            fadeInTime = fadeIn;
            displayTime = display;
            fadeOutTime = fadeOut;
        }
    }

    public static FadeSettings GetDefaultFadeSettings() => new FadeSettings(0.8f, 2f, 0.8f);
}
