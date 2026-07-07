using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // 外部から簡単にGameManagerを呼べるようにする仕組み（シングルトン）
    public static GameManager Instance { get; private set; }

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // プレイヤーが落ちたときにKillZoneから呼ばれる関数
    public void PlayerFell(int loserIndex)
    {
        if (isGameOver) return;
        isGameOver = true;

        // 勝った方の番号を計算 (1なら2、2なら1)
        int winnerIndex = (loserIndex == 1) ? 2 : 1;
        Debug.Log($"【試合終了】 Player {winnerIndex} の勝利！");

        // 3秒後に自動でステージをリセットするカウントダウンを開始
        StartCoroutine(RestartRound());
    }

    IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(3f);

        // 現在のシーンをもう一度読み直してリセットする（Unity 6対応）
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}