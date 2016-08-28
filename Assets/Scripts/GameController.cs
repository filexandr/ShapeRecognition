using System.Collections;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main script for game logic and navigation
/// </summary>
public class GameController : MonoBehaviour
{
    /// <summary>
    /// Visual template for task shape
    /// </summary>
    public GameObject TaskLineObject;

    /// <summary>
    /// Visual template for input comparision shape
    /// </summary>
    public GameObject InputLineObject;

    /// <summary>
    /// Parent object for task shape
    /// </summary>
    public GameObject TaskShapeObject;

    /// <summary>
    /// Parent object for input comparision shape
    /// </summary>
    public GameObject InputShapeObject;

    /// <summary>
    /// Parent object for user input shape
    /// </summary>
    public GameObject UserInputShapeObject;

    /// <summary>
    /// Main menu UI root object
    /// </summary>
    public GameObject MainMenuObject;

    /// <summary>
    /// Game menu UI root object
    /// </summary>
    public GameObject GameMenuObject;

    /// <summary>
    /// Message box UI root object
    /// </summary>
    public GameObject MessageBoxObject;

    /// <summary>
    /// Game end menu UI root object
    /// </summary>
    public GameObject EndGameMenuObject;

    /// <summary>
    /// UI text for counters
    /// </summary>
    public Text CountersText;

    /// <summary>
    /// UI text for score in game end menu
    /// </summary>
    public Text EndMenuScoreText;

    /// <summary>
    /// Initial time for timer (in milliseconds)
    /// </summary>
    public int StartTime = 10000;

    /// <summary>
    /// Delta time for timer after each level (in milliseconds)
    /// </summary>
    public int LevelDeltaTime = 100;

    /// <summary>
    /// Minimum boundary for timer (in milliseconds)
    /// </summary>
    public int MinTime = 500;

    /// <summary>
    /// Message box wait time (in seconds)
    /// </summary>
    public int MessageTime = 3;

    private int currentTime;
    private int currentScore;
    private bool isResultChecking;
    private bool isGameStarted;
    private int lastShapeIndex = -1;

    /// <summary>
    /// Per frame update. Timer logic.
    /// </summary>
    private void Update()
    {
        if (isGameStarted && !isResultChecking)
        {
            currentTime -= (int)(Time.deltaTime * 1000);
            if (currentTime < 0)
            {
                currentTime = 0;
                EndGame();
            }

            updateCountersText();
        }
    }

    /// <summary>
    /// Initialize and start new game
    /// </summary>
    public void StartGame()
    {
        destroyAllShapes();
        MainMenuObject.SetActive(false);
        EndGameMenuObject.SetActive(false);
        GameMenuObject.SetActive(true);

        selectAndCreateShape();
        isResultChecking = false;
        isGameStarted = true;
        currentTime = StartTime;
        currentScore = 0;
        updateCountersText();
    }

    /// <summary>
    /// Switch game to next shape
    /// </summary>
    /// <returns></returns>
    public IEnumerator NextLevel()
    {
        showMessage("Correct!", Color.green);
        yield return new WaitForSeconds(MessageTime);
        hideMessage();

        destroyAllShapes();

        currentScore++;
        currentTime = StartTime - LevelDeltaTime * currentScore;
        if (currentTime < MinTime)
        {
            currentTime = MinTime;
        }

        selectAndCreateShape();
        isResultChecking = false;
        updateCountersText();
    }

    /// <summary>
    /// Game over logic
    /// </summary>
    public void EndGame()
    {
        isGameStarted = false;
        destroyAllShapes();
        GameMenuObject.SetActive(false);
        EndGameMenuObject.SetActive(true);
        EndMenuScoreText.text = string.Format("Score: {0}", currentScore);
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// Compares user input with task shape and navigates according to result.
    /// It scales input shape to task shape boundaries and test colliders for each line.
    /// Rules:
    /// - all input vertexes or lines should be in a task collider
    /// - all task colliders should be visited
    /// - start and end input vertex should be close to each other
    /// </summary>
    /// <param name="inputShape">User input shape</param>
    /// <returns>Result for coroutine</returns>
    public IEnumerator UserInputFinished(Shape inputShape)
    {
        isResultChecking = true;
        var rect = TaskShapeObject.GetComponent<RectTransform>();
        inputShape.Fit(rect.sizeDelta.x, rect.sizeDelta.y);
        inputShape.CreateLines(InputLineObject, InputShapeObject);

        yield return new WaitForFixedUpdate();

        var total = 0;
        var matched = 0;
        var isClosed = false;

        var inputTransforms = InputShapeObject.GetComponentsInChildren<Transform>().ToList();
        inputTransforms.Remove(InputShapeObject.transform);
        var taskColliders = TaskShapeObject.GetComponentsInChildren<BoxCollider2D>();
        var notVisitedColliders = taskColliders.ToList();

        foreach (var inputTransform in inputTransforms)
        {
            total++;
            var isMatched = false;
            var inputCollider = inputTransform.gameObject.GetComponent<BoxCollider2D>();

            foreach (var taskCollider in taskColliders)
            {
                if (taskCollider.OverlapPoint(inputTransform.position) || inputCollider.bounds.Intersects(taskCollider.bounds))
                {
                    notVisitedColliders.Remove(taskCollider);
                    isMatched = true;
                }
            }

            if (isMatched)
            {
                matched++;
                inputTransform.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }
        }

        // 1st point should be close to last one
        var firstTransform = inputTransforms[0];
        var lastTransform = inputTransforms[inputTransforms.Count - 1];
        var diffX = Mathf.Abs(firstTransform.localPosition.x - lastTransform.localPosition.x);
        var diffY = Mathf.Abs(firstTransform.localPosition.y - lastTransform.localPosition.y);
        if (diffX <= 30 && diffY <= 30)
        {
            isClosed = true;
        }
        else
        {
            markLine(firstTransform, Color.blue);
            markLine(lastTransform, Color.blue);
        }

        // Mark lines with notvisited colider(s)
        foreach (var notVisitedCollider in notVisitedColliders)
        {
            markLine(notVisitedCollider.gameObject.transform, Color.red);
        }

        if (total == matched && isClosed && notVisitedColliders.Count == 0)
        {
            StartCoroutine(NextLevel());
        }
        else
        {
            StartCoroutine(nextTry());
        }
    }

    private IEnumerator nextTry()
    {
        showMessage("Wrong.\r\nTry again.", Color.red);
        yield return new WaitForSeconds(MessageTime);
        hideMessage();
        UserInputShapeObject.DestoyChildren();
        InputShapeObject.DestoyChildren();
        restoreTaskShape();
        isResultChecking = false;
    }

    private void markLine(Transform lineTransform, Color color)
    {
        lineTransform.gameObject.GetComponent<SpriteRenderer>().color = color;
        if (lineTransform.localScale.x < 5)
        {
            lineTransform.localScale = new Vector3(5, lineTransform.localScale.y, lineTransform.localScale.z);
        }
    }

    private void restoreTaskShape()
    {
        var originalColor = TaskLineObject.GetComponentInChildren<SpriteRenderer>().color;
        foreach (var sprite in TaskShapeObject.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.color = originalColor;
        }
    }

    private void destroyAllShapes()
    {
        TaskShapeObject.DestoyChildren();
        InputShapeObject.DestoyChildren();
        UserInputShapeObject.DestoyChildren();
    }

    private void selectAndCreateShape()
    {
        int templateIndex;
        if (lastShapeIndex >= 0)
        {
            do
            {
                templateIndex = Random.Range(0, Shape.Templates.Length);
            } while (lastShapeIndex == templateIndex);
        }
        else
        {
            templateIndex = Random.Range(0, Shape.Templates.Length);
        }

        lastShapeIndex = templateIndex;
        var shape = Shape.Templates[templateIndex];
        var rect = TaskShapeObject.GetComponent<RectTransform>();
        shape.Fit(rect.sizeDelta.x, rect.sizeDelta.y);
        shape.CreateLines(TaskLineObject, TaskShapeObject);
    }

    private void updateCountersText()
    {
        CountersText.text = string.Format(" Score: {0} Time: {1} ", currentScore, currentTime);
    }

    private void showMessage(string text, Color textColor)
    {
        var textObject = MessageBoxObject.GetComponentInChildren<Text>();
        textObject.text = text;
        textObject.color = textColor;
        MessageBoxObject.SetActive(true);
    }

    private void hideMessage()
    {
        MessageBoxObject.SetActive(false);
    }
}