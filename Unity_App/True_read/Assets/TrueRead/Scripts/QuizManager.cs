using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct QuizQuestion
{
    [Header("The 2D Image for this Question")]
    public Sprite questionImage;

    [Header("Drag your UI Buttons here")]
    public Button button1;
    public Button button2;
    public Button button3;

    [Header("Which button is correct? (1, 2, or 3)")]
    [Range(1, 3)]
    public int correctButtonNumber;
}

public class QuizManager : MonoBehaviour
{
    [Header("Your Custom Quiz List")]
    public List<QuizQuestion> questionList;

    [Header("UI — Question")]
    public TextMeshProUGUI promptTMP;
    public Image questionImageDisplay;

    [Header("UI — Controls & Stats")]
    public Button nextQButton;
    public Button backButton;
    public TextMeshProUGUI streakTMP;
    public TextMeshProUGUI scoreTMP;

    [Header("Audio Feedback")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    private AudioSource _audioSource;

    private int _currentQuestionIndex = 0;
    private int _currentStreak = 0;
    private int _currentScore = 0;
    private int _questionsAsked = 0;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (backButton != null) backButton.onClick.AddListener(OnBackPressed);
        if (nextQButton != null) nextQButton.onClick.AddListener(OnNextPressed);

        _currentQuestionIndex = 0;
        SetupQuestion();
    }

    void SetupQuestion()
    {
        if (questionList == null || questionList.Count == 0) return;
        if (nextQButton != null) nextQButton.gameObject.SetActive(false);

        QuizQuestion currentQ = questionList[_currentQuestionIndex];

        if (questionImageDisplay != null && currentQ.questionImage != null)
        {
            questionImageDisplay.sprite = currentQ.questionImage;
            questionImageDisplay.preserveAspect = true;
        }

        if (promptTMP != null) promptTMP.text = "What is this?";

        // ════════════════════════════════════════════════════════════════════
        // THE FIX: FORCE-WIRING THE BUTTONS
        // This ignores the Inspector and guarantees Button 1 sends '1', etc.
        // ════════════════════════════════════════════════════════════════════
        if (currentQ.button1 != null)
        {
            currentQ.button1.gameObject.SetActive(true);
            currentQ.button1.interactable = true;
            currentQ.button1.onClick.RemoveAllListeners();
            currentQ.button1.onClick.AddListener(() => ProcessAnswer(1));
        }
        if (currentQ.button2 != null)
        {
            currentQ.button2.gameObject.SetActive(true);
            currentQ.button2.interactable = true;
            currentQ.button2.onClick.RemoveAllListeners();
            currentQ.button2.onClick.AddListener(() => ProcessAnswer(2));
        }
        if (currentQ.button3 != null)
        {
            currentQ.button3.gameObject.SetActive(true);
            currentQ.button3.interactable = true;
            currentQ.button3.onClick.RemoveAllListeners();
            currentQ.button3.onClick.AddListener(() => ProcessAnswer(3));
        }

        if (streakTMP != null) streakTMP.text = "Streak: " + _currentStreak;
        if (scoreTMP != null) scoreTMP.text = "Score: " + _currentScore;
    }

    private void ProcessAnswer(int selectedOptionNumber)
    {
        _questionsAsked++;
        QuizQuestion currentQ = questionList[_currentQuestionIndex];

        // Mathematically check if the button they clicked matches your correct number
        bool isCorrect = (selectedOptionNumber == currentQ.correctButtonNumber);

        // Lock buttons
        if (currentQ.button1 != null) currentQ.button1.interactable = false;
        if (currentQ.button2 != null) currentQ.button2.interactable = false;
        if (currentQ.button3 != null) currentQ.button3.interactable = false;

        if (isCorrect)
        {
            _currentScore++;
            _currentStreak++;
            if (_audioSource != null && correctSound != null) _audioSource.PlayOneShot(correctSound);
        }
        else
        {
            _currentStreak = 0;
            if (_audioSource != null && wrongSound != null) _audioSource.PlayOneShot(wrongSound);
        }

        if (streakTMP != null) streakTMP.text = "Streak: " + _currentStreak;
        if (scoreTMP != null) scoreTMP.text = "Score: " + _currentScore;

        if (nextQButton != null) nextQButton.gameObject.SetActive(true);
    }

    void OnNextPressed()
    {
        QuizQuestion currentQ = questionList[_currentQuestionIndex];
        if (currentQ.button1 != null) currentQ.button1.gameObject.SetActive(false);
        if (currentQ.button2 != null) currentQ.button2.gameObject.SetActive(false);
        if (currentQ.button3 != null) currentQ.button3.gameObject.SetActive(false);

        _currentQuestionIndex++;

        if (_currentQuestionIndex >= questionList.Count)
        {
            if (StatsManager.Instance != null)
                StatsManager.Instance.SaveSession(_currentScore, _questionsAsked, _currentStreak);

            UnityEngine.SceneManagement.SceneManager.LoadScene("StatsScene");
        }
        else
        {
            SetupQuestion();
        }
    }

    void OnBackPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}