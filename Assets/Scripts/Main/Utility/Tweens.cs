using Cinemachine;
using UnityEngine;

/// <summary>
/// Contains functions with animations made in LeanTween.
/// </summary>
public static class Tweens
{
    static readonly float _timeMenuOpenClose = 2f;
    static readonly float _timeApplicationClose = _timeMenuOpenClose;

    /// <summary>
    /// Open the menu window.
    /// </summary>
    /// <param name="window">The gameobject of the window.</param>
    /// <returns>LeanTween ID for optional waiting to complete.</returns>

    public static int MenuWindowOpen(GameObject window)
    {
        return LeanTween.rotate(window, new Vector3(0, -30, 0), _timeMenuOpenClose).setEase(LeanTweenType.easeOutQuad).id;
    }

    /// <summary>
    /// Close the menu window.
    /// </summary>
    /// <param name="window">The gameobject of the window.</param>
    /// <returns>LeanTween ID for optional waiting to complete.</returns>
    public static int MenuWindowClose(GameObject window)
    {
        return LeanTween.rotate(window, new Vector3(0, -270, 0), _timeMenuOpenClose).setEase(LeanTweenType.easeOutQuad).id;
    }

    /// <summary>
    /// Close the application by centering the camera and zooming out.
    /// </summary>
    /// <param name="menuCamera">The virtual menu camera used for the background.</param>
    /// <returns>LeanTween ID for optional waiting to complete.</returns>
    public static int ApplicationClose(CinemachineVirtualCamera menuCamera)
    {
        var zoomId = 0;
        CinemachineFramingTransposer transposer = null;

        if (menuCamera.TryGetComponent<CinemachineVirtualCamera>(out var cam))
            transposer = cam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (transposer != null)
        {
            LeanTween.value(transposer.m_ScreenX, 0.5f, _timeApplicationClose / 2).setEase(LeanTweenType.easeOutQuint).setOnUpdate((float val) =>
            {
                transposer.m_ScreenX = val;
            });
            LeanTween.value(transposer.m_ScreenY, 0.5f, _timeApplicationClose / 2).setEase(LeanTweenType.easeOutQuint).setOnUpdate((float val) =>
            {
                transposer.m_ScreenY = val;
            });
            zoomId = LeanTween.value(transposer.m_CameraDistance, _timeApplicationClose * 50f, _timeApplicationClose).setEase(LeanTweenType.easeInOutSine).setOnUpdate((float val) =>
            {
                transposer.m_CameraDistance = val;
            }).id;
        }

        return zoomId;
    }

}
