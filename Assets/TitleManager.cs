using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    void Update()
    {
        // キーボードのSpaceキーが押されたか
        bool spacePressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        // コントローラーのAボタン（下側のボタン）が押されたか
        bool aButtonPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        // どちらかが押されたらゲーム本編へ
        if (spacePressed || aButtonPressed)
        {
            SceneManager.LoadScene(1);
        }
    }
}