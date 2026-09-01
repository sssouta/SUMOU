using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    [Header("遷移先のステージ選択シーン名")]
    [SerializeField] private string StageSelectSceneName = "StageSelect";

    [Header("決定SE")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip decideSE;

    [Header("SEを鳴らしてから画面遷移するまでの時間")]
    [SerializeField] private float waitTime = 0.5f;

    private bool isTransitioning = false;

    void Update()
    {
        // すでに画面遷移処理中なら入力を受け付けない
        if (isTransitioning)
        {
            return;
        }

        // キーボードのSpaceキーが押されたか
        bool spacePressed =
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;

        // コントローラーのAボタンが押されたか
        bool aButtonPressed =
            Gamepad.current != null &&
            Gamepad.current.buttonSouth.wasPressedThisFrame;

        // どちらかが押されたらSEを鳴らして画面遷移
        if (spacePressed || aButtonPressed)
        {
            isTransitioning = true;

            // 決定SEを再生
            if (audioSource != null && decideSE != null)
            {
                audioSource.PlayOneShot(decideSE);
            }

            StartCoroutine(ChangeScene());
        }
    }

    private IEnumerator ChangeScene()
    {
        // SEを少し聞かせるため待つ
        yield return new WaitForSeconds(waitTime);

        // ステージ選択画面へ移動
        SceneManager.LoadScene(StageSelectSceneName);
    }
}