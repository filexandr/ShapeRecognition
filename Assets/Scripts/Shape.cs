using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Represents in memory shape data and responsible for building visual objects on base of it.
/// Also it contains shape templates.
/// </summary>
public class Shape
{
    /// <summary>
    /// Shape templates for selection during the game play
    /// </summary>
    public static Shape[] Templates =
    {
        // Square
        new Shape() { Vertexes = { new Vector2(0, 0), new Vector2(0, 100), new Vector2(100, 100),
                                   new Vector2(100, 0), new Vector2(0, 0) } },
        // Triangle
        new Shape() { Vertexes = { new Vector2(-100, 0), new Vector2(0, 100), new Vector2(100, 0),
                                   new Vector2(-100, 0) } },
        // Horizontal rectangle
        new Shape() { Vertexes = { new Vector2(-100, 100), new Vector2(-100, 200), new Vector2(-400, 200),
                                   new Vector2(-400, 100), new Vector2(-100, 100) } },
        // Vertical rectangle
        new Shape() { Vertexes = { new Vector2(-500, -500), new Vector2(-400, -500), new Vector2(-400, -1000),
                                   new Vector2(-500, -1000), new Vector2(-500, -500) } },
        // Triangle 2
        new Shape() { Vertexes = { new Vector2(-1000, 0), new Vector2(-1000, 1000), new Vector2(0, 0),
                                   new Vector2(-1000, 0) } },
        // Diamond
        new Shape() { Vertexes = { new Vector2(0, 100), new Vector2(50, 0), new Vector2(0, -100),
                                   new Vector2(-50, 0), new Vector2(0, 100) } }
    };

    /// <summary>
    /// List of shape vertexes
    /// </summary>
    public List<Vector2> Vertexes { get; set; }

    /// <summary>
    /// Initializes instance of the class
    /// </summary>
    public Shape()
    {
        Vertexes = new List<Vector2>();
    }

    /// <summary>
    /// Centrilize and scale the shape inside of a rectangle
    /// </summary>
    /// <param name="width">Rectangle width</param>
    /// <param name="height">Rectangle height</param>
    public void Fit(float width, float height)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (var i = 0; i < Vertexes.Count - 1; i++)
        {
            var x = Vertexes[i].x;
            var y = Vertexes[i].y;

            minX = Mathf.Min(minX, x);
            maxX = Mathf.Max(maxX, x);
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        var shapeExtentX = maxX - minX;
        var shapeExtentY = maxY - minY;
        var scaleX = width / shapeExtentX;
        var scaleY = height / shapeExtentY;
        var scale = Mathf.Min(scaleX, scaleY);
        var scaleVector = new Vector2(scale, scale);

        var transX = -shapeExtentX / 2 - minX;
        var transY = -shapeExtentY / 2 - minY;

        for (var i = 0; i < Vertexes.Count; i++)
        {
            var vertex = Vertexes[i];
            vertex += new Vector2(transX, transY);
            vertex.Scale(scaleVector);
            Vertexes[i] = vertex;
        }
    }

    /// <summary>
    /// Creates visual line objects for whole shape
    /// and assign them to specified parent object
    /// </summary>
    /// <param name="lineObject">Visual line object template</param>
    /// <param name="shapeObject">Parent object to be assigned to</param>
    public void CreateLines(GameObject lineObject, GameObject shapeObject)
    {
        for (var i = 0; i < Vertexes.Count - 1; i++)
        {
            var from = Vertexes[i];
            var to = Vertexes[i + 1];
            var line = CreateLine(from, to, lineObject, shapeObject);
            line.name = string.Format("Line {0}-{1}", i, i + 1);
        }
    }

    /// <summary>
    /// Creates one visual line object and assign it to specified parent object
    /// </summary>
    /// <param name="from">Start position of the line</param>
    /// <param name="to">End position of the line</param>
    /// <param name="lineObject">Visual line object template</param>
    /// <param name="shapeObject">Parent object to be assigned to</param>
    /// <returns></returns>
    public static GameObject CreateLine(Vector2 from, Vector2 to, GameObject lineObject, GameObject shapeObject)
    {
        var angle = Utils.CalculateAngleWithSign(from, to);
        var dist = Vector2.Distance(to, from);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        var line = (GameObject)GameObject.Instantiate(lineObject, Vector3.zero, rotation, shapeObject.transform);
        line.transform.localPosition = new Vector3(from.x, from.y, 0);
        line.transform.localScale += Vector3.right * dist;
        return line;
    }
}
