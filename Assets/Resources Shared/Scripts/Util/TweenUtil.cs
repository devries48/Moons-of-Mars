using Game.Astroids;
using UnityEngine;

public class TweenUtil
{
    public static readonly float m_timeMenuOpenClose = 2f;

    /// <summary>
    /// Open the menu window.
    /// </summary>
    /// <param name="window">The gameobject of the window.</param>
    /// <returns>LeanTween ID for optional waiting to complete.</returns>
    public static int MenuWindowOpen(GameObject window)
    {
        return LeanTween.rotate(window, new Vector3(0, -30, 0), m_timeMenuOpenClose).setEase(LeanTweenType.easeOutQuad).id;
    }

    /// <summary>
    /// Close the menu window.
    /// </summary>
    /// <param name="window">The gameobject of the window.</param>
    /// <param name="noTween">No animation will be played.</param>
    /// <returns>LeanTween ID for optional waiting to complete.</returns>
    public static int MenuWindowClose(GameObject window, bool noTween = false)
    {
        var t = noTween ? 0f : m_timeMenuOpenClose;
        return LeanTween.rotate(window, new Vector3(0, -270, 0), t).setEase(LeanTweenType.easeOutQuad).id;
    }

    /// <summary>
    /// Close the application by centering the camera and zooming out.
    /// </summary>
    /// <param name="menuCamera">The virtual menu camera used for the background.</param>
    /// <returns>LeanTween ID for optional waiting to complete.</returns>


    //TODO: simplify (eg: use float for pivot)
    public static int TweenPivot(GameObject gameObj, Vector2 newPivot, object rotateObj,
                 LeanTweenType pivotEase = LeanTweenType.easeInOutSine, float pivotTime = 1f,
                 LeanTweenType rotateEase = LeanTweenType.notUsed, float rotateTime = 0f)
    {
        var rect = gameObj.GetComponent<RectTransform>();
        var id_pivot = 0;
        var id_rotate = 0;

        if (rotateObj is Vector3 rotate)
        {
            if (rotateEase == LeanTweenType.notUsed)
                rect.Rotate(rotate);
            else
                id_rotate = LeanTween.rotate(gameObj, rotate, rotateTime).setEase(rotateEase).id;
        }

        id_pivot = LeanTween.value(gameObj, rect.pivot, newPivot, pivotTime).setEase(pivotEase).setOnUpdateVector2((Vector2 pos) =>
        {
            rect.pivot = pos;
        }).id;

        return pivotTime > rotateTime ? id_pivot : id_rotate;
    }

}
