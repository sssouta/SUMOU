using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // 👈 TextMesh Proを使うためにこれを追加！

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("勝利表示用のテキストUI")]
    [SerializeField] private TextMeshProUGUI winnerText; // 👈 ここにさっきのWinnerTextを入れます

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // ゲーム開始時はテキストを確実に隠しておく
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(false);
        }
    }

    public void PlayerFell(int loserIndex)
    {
        if (isGameOver) return;
        isGameOver = true;

        int winnerIndex = (loserIndex == 1) ? 2 : 1;

        // 【ここを追加！】画面に勝者を大きく表示する
        if (winnerText != null)
        {
            winnerText.text = $"Player {winnerIndex} WIN!"; // 文字書き換え
            winnerText.gameObject.SetActive(true);         // 画面に表示！
        }

        Debug.Log($"【試合終了】 Player {winnerIndex} の勝利！");

        StartCoroutine(RestartRound());
    }

    IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}