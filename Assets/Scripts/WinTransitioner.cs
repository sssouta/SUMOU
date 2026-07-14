using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinTransitioner : MonoBehaviour
{
    public static WinTransitioner Instance { get; private set; }

    // BO3（2本先取）用のスコア管理（静的変数にしてシーンをまたいでも維持）
    private static int player1Wins = 0;
    private static int player2Wins = 0;
    private const int WINS_TO_WIN_MATCH = 2; // 2本先取

    // リザルト画面で「どっちが勝ったか」を参照するための変数
    public static int FinalWinner { get; private set; } = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // シーンを切り替えてもこのオブジェクトが消えないようにする
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 勝者が決まったときにGameManagerなどから呼び出すメソッド
    public void RegisterRoundWin(int winnerIndex)
    {
        if (winnerIndex == 1) player1Wins++;
        else player2Wins++;

        // 2勝したプレイヤーがいた場合（完全決着！）
        if (player1Wins >= WINS_TO_WIN_MATCH || player2Wins >= WINS_TO_WIN_MATCH)
        {
            FinalWinner = winnerIndex; // 最終勝者を記録

            // スコアを次の試合のためにリセット
            player1Wins = 0;
            player2Wins = 0;

            // 3秒後にリザルトシーンへ遷移
            StartCoroutine(LoadSceneDelayed("ResultScene", 3f));
        }
        // まだ決着がついていない場合（次のラウンドへ）
        else
        {
            // 3秒後に現在のゲームシーンを再読み込み
            StartCoroutine(LoadSceneDelayed(SceneManager.GetActiveScene().name, 3f));
        }
    }

    // 現在のスコアテキストを返す（GameManagerがカウントダウン時に表示するために使う）
    public string GetCurrentScoreText()
    {
        return $"ROUND SCORE\nP1: {player1Wins} - P2: {player2Wins}";
    }

    private IEnumerator LoadSceneDelayed(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}