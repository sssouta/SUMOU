using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("勝利表示・カウントダウン用のテキストUI")]
    [SerializeField] private TextMeshProUGUI winnerText;

    [Header("カウントダウンSE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip roundScoreSound;
    [SerializeField] private AudioClip countSound;
    [SerializeField] private AudioClip goSound;

    private bool isGameOver = false;
    public bool IsGameActive { get; private set; } = false;

    private static int player1Wins = 0;
    private static int player2Wins = 0;
    private const int WINS_TO_WIN_MATCH = 2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(true);
            StartCoroutine(StartMatchSequence());
        }
    }

    IEnumerator StartMatchSequence()
    {
        IsGameActive = false;

        // ラウンドスコア表示
        winnerText.text = $"ROUND SCORE\nP1: {player1Wins} - P2: {player2Wins}";

        // ラウンドスコア表示時のSE
        PlaySound(roundScoreSound);

        yield return new WaitForSeconds(1.5f);

        // 3
        winnerText.text = "3";
        PlaySound(countSound);
        yield return new WaitForSeconds(1f);

        // 2
        winnerText.text = "2";
        PlaySound(countSound);
        yield return new WaitForSeconds(1f);

        // 1
        winnerText.text = "1";
        PlaySound(countSound);
        yield return new WaitForSeconds(1f);

        // GO!
        winnerText.text = "GO!";
        PlaySound(goSound);

        IsGameActive = true;

        yield return new WaitForSeconds(1f);
        winnerText.gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayerFell(int loserIndex)
    {
        if (isGameOver) return;

        isGameOver = true;
        IsGameActive = false;

        int winnerIndex = (loserIndex == 1) ? 2 : 1;

        if (winnerIndex == 1)
        {
            player1Wins++;
        }
        else
        {
            player2Wins++;
        }

        if (winnerText != null)
        {
            if (player1Wins >= WINS_TO_WIN_MATCH ||
                player2Wins >= WINS_TO_WIN_MATCH)
            {
                winnerText.text = $"Player {winnerIndex} MATCH WIN!";

                player1Wins = 0;
                player2Wins = 0;
            }
            else
            {
                winnerText.text =
                    $"Player {winnerIndex} WIN!\n" +
                    $"(P1: {player1Wins} - P2: {player2Wins})";
            }

            winnerText.gameObject.SetActive(true);
        }

        StartCoroutine(RestartRound());
    }

    IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}