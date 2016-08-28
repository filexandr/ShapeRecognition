using System.Collections;

using UnityEngine;

/// <summary>
/// Script fo handling user input
/// </summary>
public class InputShapeDrawer : MonoBehaviour
{
    /// <summary>
    /// Visual template for line
    /// </summary>
    public GameObject LineObject;

    /// <summary>
    /// Link to GameController to be called
    /// </summary>
    public GameController GameControllerObject;

    /// <summary>
    /// Mouse trail particle system
    /// </summary>
    public GameObject MouseTrailObject;

    private Vector2 startPos;
    private GameObject currentLine;
    private GameObject inputShapeObject;
    private Shape shape;
    private Rect drawRect;

    /// <summary>
    /// Initialization of the script
    /// </summary>
    private void Start()
    {
        inputShapeObject = this.transform.gameObject;
        drawRect = inputShapeObject.GetComponent<RectTransform>().rect;
    }

    /// <summary>
    /// Mouse BeginDrag handler. Start drawing and trailing.
    /// </summary>
    public void OnBeginDrag()
    {
        MouseTrailObject.SetActive(false);
        moveMouseTrail();
        MouseTrailObject.SetActive(true);

        startPos = Utils.ConvertMousePosToObjectSpace((RectTransform)this.transform);
        shape = new Shape();
        shape.Vertexes.Add(startPos);

        inputShapeObject.DestoyChildren();

        if (currentLine != null)
        {
            Destroy(currentLine);
            currentLine = null;
        }
    }

    /// <summary>
    /// Mouse Drag handler. Draw next line.
    /// </summary>
    public void OnDrag()
    {
        moveMouseTrail();
        var endPos = Utils.ConvertMousePosToObjectSpace((RectTransform)this.transform);
        endPos = Utils.LockPointInsideRect(endPos, drawRect);
        currentLine = Shape.CreateLine(startPos, endPos, LineObject, inputShapeObject);
        shape.Vertexes.Add(endPos);
        startPos = endPos;
    }

    /// <summary>
    /// Mouse EndDarg handler. Finish drawing and trailing, check result.
    /// </summary>
    public void OnEndDrag()
    {
        moveMouseTrail();
        StartCoroutine(switchOffMouseTrail());

        var endPos = Utils.ConvertMousePosToObjectSpace((RectTransform)this.transform);
        endPos = Utils.LockPointInsideRect(endPos, drawRect);
        currentLine = Shape.CreateLine(startPos, endPos, LineObject, inputShapeObject);
        shape.Vertexes.Add(endPos);
        // Add fake vertex at the end for closing test
        shape.Vertexes.Add(endPos);
        StartCoroutine(GameControllerObject.UserInputFinished(shape));
    }

    private IEnumerator switchOffMouseTrail()
    {
        yield return new WaitForSeconds(1);
        MouseTrailObject.SetActive(false);
    }

    private void moveMouseTrail()
    {
        MouseTrailObject.transform.localPosition = Utils.ConvertMousePosToObjectSpace((RectTransform)MouseTrailObject.transform.parent);
    }
}