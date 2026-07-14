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

        // 【新スクリプトと連携】WinTransitioner から現在の対戦スコアを取得して表示
        if (WinTransitioner.Instance != null)
        {
            winnerText.text = WinTransitioner.Instance.GetCurrentScoreText();
        }
        else
        {
            winnerText.text = "ROUND START";
        }

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

        // 落ちたプレイヤーの反対側を勝者とする
        int winnerIndex = (loserIndex == 1) ? 2 : 1;

        // 【新スクリプトと連携】勝者の番号を送り、スコア加算とリザルトへの遷移判定を任せる
        if (WinTransitioner.Instance != null)
        {
            WinTransitioner.Instance.RegisterRoundWin(winnerIndex);
        }
        else
        {
            Debug.LogWarning("WinTransitioner がシーン内に見つかりません！アタッチされているか確認してください。");
        }

        // 画面に「Player 〇 WIN!」を表示する
        if (winnerText != null)
        {
            winnerText.text = $"Player {winnerIndex} WIN!";
            winnerText.gameObject.SetActive(true);
        }
    }
}