using UnityEngine;

public class TweenUtil
{

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

        return (pivotTime > rotateTime) ? id_pivot : id_rotate;
    }

}
