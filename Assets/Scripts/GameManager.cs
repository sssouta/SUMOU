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

    [Tooltip("ラウンド開始時のスコア表示SE")]
    [SerializeField] private AudioClip roundScoreSound;

    [Tooltip("3・2・1のカウントSE")]
    [SerializeField] private AudioClip countSound;

    [Tooltip("GO!のSE")]
    [SerializeField] private AudioClip goSound;

    // ★追加：ラウンド勝利SE
    [Header("勝利SE")]

    [Tooltip("1ラウンド取得した時のSE")]
    [SerializeField] private AudioClip roundWinSound;

    [Tooltip("2ラウンド取得して試合に勝利した時のSE")]
    [SerializeField] private AudioClip matchWinSound;

    [Header("対戦BGM")]
    [SerializeField] private AudioSource battleBgmSource;
    [SerializeField] private AudioClip battleBgm;

    [Header("遷移先のタイトルシーン名")]
    [SerializeField] private string titleSceneName = "TitleScene";

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
        // カウントダウン中はプレイヤーを操作できない
        IsGameActive = false;

        // 念のためBGMを止めておく
        if (battleBgmSource != null)
        {
            battleBgmSource.Stop();
        }

        // ラウンドスコア表示
        winnerText.text =
            $"ROUND SCORE\nP1: {player1Wins} - P2: {player2Wins}";

        PlaySound(roundScoreSound);

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

        // GO!と同時に対戦BGM開始
        PlayBattleBGM();

        // GO!から操作可能
        IsGameActive = true;

        yield return new WaitForSeconds(1f);

        winnerText.gameObject.SetActive(false);
    }

    // SE再生
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

    // 対戦BGM再生
    private void PlayBattleBGM()
    {
        if (battleBgmSource == null)
        {
            Debug.LogWarning("対戦BGM用のAudio Sourceが設定されていません。");
            return;
        }

        if (battleBgm == null)
        {
            Debug.LogWarning("対戦BGMが設定されていません。");
            return;
        }

        battleBgmSource.clip = battleBgm;
        battleBgmSource.loop = true;
        battleBgmSource.Play();
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

        // 勝敗が決まったらBGM停止
        if (battleBgmSource != null)
        {
            battleBgmSource.Stop();
        }

        // 落ちたプレイヤーとは反対側を勝者にする
        int winnerIndex = loserIndex == 1 ? 2 : 1;

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
            // どちらかが2勝した場合
            if (player1Wins >= WINS_TO_WIN_MATCH ||
                player2Wins >= WINS_TO_WIN_MATCH)
            {
                winnerText.text =
                    $"Player {winnerIndex} MATCH WIN!";

                // ★追加：最終勝利SE
                PlaySound(matchWinSound);

                player1Wins = 0;
                player2Wins = 0;

                isMatchFinished = true;
            }
            else
            {
                winnerText.text =
                    $"Player {winnerIndex} WIN!\n" +
                    $"(P1: {player1Wins} - P2: {player2Wins})";

                // ★追加：1ラウンド取得SE
                PlaySound(roundWinSound);
            }

            winnerText.gameObject.SetActive(true);
        }

        StartCoroutine(RestartRound(isMatchFinished));
    }

    private IEnumerator RestartRound(bool isMatchFinished)
    {
        yield return new WaitForSeconds(3f);

        if (isMatchFinished)
        {
            SceneManager.LoadScene(titleSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}