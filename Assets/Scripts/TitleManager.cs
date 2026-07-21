using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    [Header("遷移先のステージ選択シーン名")]
    [SerializeField] private string StageSelectSceneName = "StageSelect";

    void Update()
    {
        // キーボードのSpaceキーが押されたか
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        // コントローラーのAボタン（下側のボタン）が押されたか
        bool aButtonPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        // どちらかが押されたらステージ選択画面へ移動
        if (spacePressed || aButtonPressed)
        {
            SceneManager.LoadScene(StageSelectSceneName);
        }
    }
}