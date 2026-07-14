using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
// ↓新しいInput Systemを使うためにこの一行を追加
using UnityEngine.InputSystem;

public class ResultManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;

    void Start()
    {
        // データの反映
       // scoreText.text = "SCORE: " + GameManager.finalScore.ToString();
       // timeText.text = "TIME: " + GameManager.clearTime.ToString("F1") + "s";
    }

    void Update()
    {
        // 新しいInput Systemでの「キーボードのSpaceキーが、このフレームで押されたか」の書き方
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ReturnToTitle();
        }
    }

    public void OnTitleButton()
    {
        ReturnToTitle();
    }

    private void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}