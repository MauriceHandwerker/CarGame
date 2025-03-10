using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Game state variables
    public static int score;
    private static int highScore;
    private static int lastScore;

    public GameObject Car;
    public Spawner Spawner;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI lastScoreText;
    public float countdownTime = 3f;
    public GameObject countdownPanel;
    public GameObject loosePanel;
    public GameObject StartGamePanel;
    public CarController carController;

    private Vector3 startPosition = new Vector3(10f, 0f, 10f);
    private Quaternion startRotation = Quaternion.Euler(0f, 0f, 0f);
    private bool isGameActive = false;
    private bool isCountingDown = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        lastScore = PlayerPrefs.GetInt("LastScore", 0);
    }

    public void startGame()
    {
        if (StartGamePanel != null)
            StartGamePanel.SetActive(false);
        InitializeGameState();
        
    }

    private void InitializeGameState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        score = 0;
        UpdateScoreText();
        if (loosePanel != null) loosePanel.SetActive(false);
        if (lastScoreText != null) lastScoreText.gameObject.SetActive(false);
        
        UpdateHighScoreText();
        UpdateLastScoreText();
        
        ResetGame();
        StartCountdown();
    }

    private void ResetGame()
    {
        if (Car != null)
        {
            Car.transform.position = startPosition;
            Car.transform.rotation = startRotation;
            
            Rigidbody rb = Car.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        if (Spawner != null)
        {
            Spawner.ClearAllSpawnedObjects();
            Spawner.ResetSpawner();
        }

        isGameActive = false;
    }

    public void StartCountdown()
    {
        if (isCountingDown) return;
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        if (countdownPanel == null) yield break;

        isCountingDown = true;
        countdownPanel.SetActive(true);
        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            if (countdownText != null)
                countdownText.text = Mathf.Ceil(timeLeft).ToString();
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }
        if (countdownText != null)
            countdownText.text = "Go!";
        yield return new WaitForSeconds(1f);
        countdownPanel.SetActive(false);
        InitialiseGame();
        isCountingDown = false;
    }

    public void InitialiseGame()
    {
        if (carController != null)
            carController.CanMove = true;
            
        if (Spawner != null && !isGameActive)
        {
            Spawner.Spawn();
            isGameActive = true;
        }
        else if (Spawner == null)
        {
            Debug.LogError("Spawner reference is null, cannot spawn drops!");
        }
    }

    public static void AddScore()
    {
        score++;
        Instance.UpdateScoreText();
        Instance.UpdateHighScore();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        else
            Debug.LogWarning("ScoreText is not assigned in the Inspector!");
    }

    private void UpdateHighScoreText()
    {
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
        else
            Debug.LogWarning("HighScoreText is not assigned in the Inspector!");
    }

    private void UpdateLastScoreText()
    {
        if (lastScoreText != null)
        {
            lastScoreText.text = "Last Score: " + lastScore;
            Debug.Log($"Last Score updated to: {lastScore}");
        }
        else
            Debug.LogWarning("LastScoreText is not assigned in the Inspector!");
    }

    private void UpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            UpdateHighScoreText();
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
    }

    public void Lose()
    {
        if (carController != null)
            carController.CanMove = false;
        else
            Debug.LogWarning("CarController is not assigned!");

        // Update last score
        lastScore = score;
        PlayerPrefs.SetInt("LastScore", lastScore);
        PlayerPrefs.Save();
        UpdateLastScoreText();

        // Update high score
        UpdateHighScore();

        // Reset current score
        score = 0;
        UpdateScoreText();

        // Show lose panel and last score
        if (loosePanel != null)
        {
            loosePanel.SetActive(true);
            Debug.Log("Lose panel activated");
        }
        else
            Debug.LogWarning("LoosePanel is not assigned in the Inspector!");

        if (lastScoreText != null)
        {
            lastScoreText.gameObject.SetActive(true);
            lastScoreText.text = "Last Score: " + lastScore; // Double-check text update
            Debug.Log($"LastScoreText activated and set to: {lastScoreText.text}");
        }
        else
            Debug.LogWarning("LastScoreText is not assigned in the Inspector!");

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Replay()
    {
        if (loosePanel != null)
            loosePanel.SetActive(false);
        if (lastScoreText != null)
            lastScoreText.gameObject.SetActive(false);

        ResetGame();
        StartCountdown();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ResetScores()
    {
        highScore = 0;
        lastScore = 0;
        PlayerPrefs.SetInt("HighScore", 0);
        PlayerPrefs.SetInt("LastScore", 0);
        PlayerPrefs.Save();
        UpdateHighScoreText();
        UpdateLastScoreText();
    }

    public void Update()
    {
        if (Car == null) return;

        if (Car.transform.position.y < -5)
        {
            Lose();
        }

        if (Car.transform.position.y > 0.5f)
        {
            Car.transform.position += Vector3.down * 9.8f * Time.deltaTime;
        }

        if (score >= 50)
        {
            Debug.Log("You Win!");
        }
    }
}