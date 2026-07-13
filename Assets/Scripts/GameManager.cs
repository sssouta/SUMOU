using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("勝利表示・カウントダウン用のテキストUI")]
    [SerializeField] private TextMeshProUGUI winnerText;

    private bool isGameOver = false;
    public bool IsGameActive { get; private set; } = false;

    // ==========================================
    // 【ここを追加！】BO3（2本先取）用のスコア管理
    // static（静的変数）にすることで、シーンを再読み込みしても数値が消えずに残ります！
    // ==========================================
    private static int player1Wins = 0;
    private static int player2Wins = 0;
    private const int WINS_TO_WIN_MATCH = 2; // 何本先取で勝ちにするか（2 = BO3）

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        // 【演出追加！】現在の取得ラウンド数を、カウントダウンの前に一瞬表示する
        winnerText.text = $"ROUND SCORE\nP1: {player1Wins} - P2: {player2Wins}";
        yield return new WaitForSeconds(1.5f);

        // カウントダウン開始
        winnerText.text = "3";
        yield return new WaitForSeconds(1f);
        winnerText.text = "2";
        yield return new WaitForSeconds(1f);
        winnerText.text = "1";
        yield return new WaitForSeconds(1f);

        winnerText.text = "GO!";
        IsGameActive = true;

        yield return new WaitForSeconds(1f);
        winnerText.gameObject.SetActive(false);
    }

    public void PlayerFell(int loserIndex)
    {
        if (isGameOver) return;
        isGameOver = true;
        IsGameActive = false;

        int winnerIndex = (loserIndex == 1) ? 2 : 1;

        // 【ここを追加！】勝ったプレイヤーのスコアを増やす
        if (winnerIndex == 1) player1Wins++;
        else player2Wins++;

        if (winnerText != null)
        {
            // 2勝したプレイヤーがいた場合（完全決着！）
            if (player1Wins >= WINS_TO_WIN_MATCH || player2Wins >= WINS_TO_WIN_MATCH)
            {
                winnerText.text = $"Player {winnerIndex} MATCH WIN!";

                // 次の試合のために、ここでスコアをリセットする
                player1Wins = 0;
                player2Wins = 0;
            }
            // まだどちらも2勝していない場合（次のラウンドへ）
            else
            {
                winnerText.text = $"Player {winnerIndex} WIN!\n(P1: {player1Wins} - P2: {player2Wins})";
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