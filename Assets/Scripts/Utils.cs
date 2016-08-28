using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Help utilities
/// </summary>
public static class Utils
{
    #region Extensions

    /// <summary>
    /// Extension for GameObject for destroying all its children objects
    /// </summary>
    /// <param name="gameObject">Target GameObject</param>
    public static void DestoyChildren(this GameObject gameObject)
    {
        var children = new List<GameObject>();
        foreach (Transform child in gameObject.transform)
        {
            children.Add(child.gameObject);
        }

        children.ForEach(child =>
        {
            child.transform.parent = null;
            GameObject.Destroy(child);
        });
    }

    #endregion

    /// <summary>
    /// Calculates signed angle biween 2 vectors
    /// </summary>
    /// <param name="from">Start vector</param>
    /// <param name="to">End vector</param>
    /// <returns>Signed angle in degrees</returns>
    public static float CalculateAngleWithSign(Vector2 from, Vector2 to)
    {
        var targetVector = to - from;
        var sign = (to.y < from.y) ? -1.0f : 1.0f;
        var result = Vector2.Angle(Vector2.right, targetVector) * sign;
        return result;
    }

    /// <summary>
    /// Restricts point by rectangle. Calculates new point which doesn't cross boundaries.
    /// </summary>
    /// <param name="point">Original point</param>
    /// <param name="rect">Retstriction rectangle</param>
    /// <returns>New point with garantee to be inside the rectangle</returns>
    public static Vector2 LockPointInsideRect(Vector2 point, Rect rect)
    {
        var result = new Vector2();
        result.x = Mathf.Max(point.x, rect.xMin);
        result.x = Mathf.Min(result.x, rect.xMax);
        result.y = Mathf.Max(point.y, rect.yMin);
        result.y = Mathf.Min(result.y, rect.yMax);
        return result;
    }

    /// <summary>
    /// Converts current mouse position to local position of
    /// object with RectTransform using main camera
    /// </summary>
    /// <param name="transfrom">Rectangle transform</param>
    /// <returns>Local position relative to GameObject space</returns>
    public static Vector2 ConvertMousePosToObjectSpace(RectTransform transfrom)
    {
        Vector2 result;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transfrom,
            Input.mousePosition, Camera.main, out result);
        return result;
    }
}
