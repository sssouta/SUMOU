using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("勝利表示・カウントダウン用のテキストUI")]
    [SerializeField] private TextMeshProUGUI winnerText;

    [Header("カウントダウン・ラウンド表示SE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip roundScoreSound;
    [SerializeField] private AudioClip countSound;
    [SerializeField] private AudioClip goSound;

    [Header("遷移先のタイトルシーン名")]
    [SerializeField] private string titleSceneName = "TitleScene"; // ※実際のタイトルシーン名に合わせて変更してください

    private bool isGameOver = false;

    public bool IsGameActive { get; private set; } = false;

    // BO3（2本先取）用のスコア
    private static int player1Wins = 0;
    private static int player2Wins = 0;

    private const int WINS_TO_WIN_MATCH = 2;

    private void Awake()
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

    private void Start()
    {
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(true);
            StartCoroutine(StartMatchSequence());
        }
        else
        {
            Debug.LogWarning("GameManagerのWinner Textが設定されていません。");
        }
    }

    private IEnumerator StartMatchSequence()
    {
        // カウントダウン中はプレイヤーを操作できないようにする
        IsGameActive = false;

        // ラウンドスコアを表示
        winnerText.text =
            $"ROUND SCORE\nP1: {player1Wins} - P2: {player2Wins}";

        // 表示と同時にSE
        PlaySound(roundScoreSound);

        // 合計1.3秒後にカウントダウン開始
        yield return new WaitForSeconds(1.3f);

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

        // GO!が表示された瞬間から操作可能
        IsGameActive = true;

        yield return new WaitForSeconds(1f);

        winnerText.gameObject.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("GameManagerのAudio Sourceが設定されていません。");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("再生するSEが設定されていません。");
            return;
        }

        audioSource.PlayOneShot(clip);
    }

    public void PlayerFell(int loserIndex)
    {
        // 勝敗処理が重複しないようにする
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        IsGameActive = false;

        // 落ちたプレイヤーとは反対側を勝者にする
        int winnerIndex = loserIndex == 1 ? 2 : 1;

        // 勝者の取得ラウンド数を増やす
        if (winnerIndex == 1)
        {
            player1Wins++;
        }
        else
        {
            player2Wins++;
        }

        bool isMatchFinished = false;

        if (winnerText != null)
        {
            // どちらかが2勝した場合（1マッチ終了）
            if (player1Wins >= WINS_TO_WIN_MATCH ||
                player2Wins >= WINS_TO_WIN_MATCH)
            {
                winnerText.text =
                    $"Player {winnerIndex} MATCH WIN!";

                // 次のマッチに備えてスコアをリセット
                player1Wins = 0;
                player2Wins = 0;

                isMatchFinished = true;
            }
            else
            {
                // まだ2勝していない場合（次のラウンドへ）
                winnerText.text =
                    $"Player {winnerIndex} WIN!\n" +
                    $"(P1: {player1Wins} - P2: {player2Wins})";
            }

            winnerText.gameObject.SetActive(true);
        }

        StartCoroutine(RestartRound(isMatchFinished));
    }

    private IEnumerator RestartRound(bool isMatchFinished)
    {
        // 勝敗表示を3秒間見せる
        yield return new WaitForSeconds(3f);

        if (isMatchFinished)
        {
            // マッチ終了時はタイトルシーンへ戻る
            SceneManager.LoadScene(titleSceneName);
        }
        else
        {
            // ラウンド継続時は現在のシーンを再読み込み
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}