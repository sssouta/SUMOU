using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;

public class StageSelectManager : MonoBehaviour
{
    [Header("SE")]
    [SerializeField] private AudioSource audioSource;

    [Header("選択を動かした時のSE")]
    [SerializeField] private AudioClip scrollSE;

    [Header("マップを決定した時のSE")]
    [SerializeField] private AudioClip clickSE;

    [Header("SEを鳴らしてから画面遷移するまでの時間")]
    [SerializeField] private float waitTime = 0.3f;

    private GameObject lastSelected;
    private bool isTransitioning = false;

    void Start()
    {
        // 最初に選択されているボタンを記録
        if (EventSystem.current != null)
        {
            lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }

    void Update()
    {
        if (EventSystem.current == null)
            return;

        GameObject currentSelected =
            EventSystem.current.currentSelectedGameObject;

        // 選択しているボタンが変わったらスクロールSE
        if (currentSelected != null &&
            currentSelected != lastSelected)
        {
            // 最初の選択時には鳴らさない
            if (lastSelected != null)
            {
                PlayScrollSE();
            }

            lastSelected = currentSelected;
        }
    }

    // スクロールSE
    private void PlayScrollSE()
    {
        if (audioSource != null && scrollSE != null)
        {
            audioSource.PlayOneShot(scrollSE);
        }
    }

    // ノーマルマップ
    public void LoadNormalStage()
    {
        StartStage("Stage_Normal");
    }

    // 氷マップ
    public void LoadIceStage()
    {
        StartStage("Stage_Ice");
    }

    // 穴あきマップ
    public void LoadHoleStage()
    {
        StartStage("Stage_Hole");
    }

    // 決定SEを鳴らしてからステージへ移動
    private void StartStage(string sceneName)
    {
        if (isTransitioning)
            return;

        isTransitioning = true;

        if (audioSource != null && clickSE != null)
        {
            audioSource.PlayOneShot(clickSE);
        }

        StartCoroutine(LoadStageAfterSE(sceneName));
    }

    private IEnumerator LoadStageAfterSE(string sceneName)
    {
        yield return new WaitForSeconds(waitTime);

        SceneManager.LoadScene(sceneName);
    }
}