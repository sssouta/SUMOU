using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    // 各ボタンのOnClick()から呼び出すメソッド
    public void LoadNormalStage()
    {
        SceneManager.LoadScene("Stage_Normal");
    }

    public void LoadIceStage()
    {
        SceneManager.LoadScene("Stage_Ice");
    }

    public void LoadHoleStage()
    {
        SceneManager.LoadScene("Stage_Hole");
    }
}