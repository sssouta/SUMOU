using UnityEngine;

public class KillZone : MonoBehaviour
{
    // トリガー（センサー）に何かが侵入したときに自動で呼ばれる関数
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            // PlayerControllerからプレイヤーのインスペクター設定値（1か2）を取得する
            // ※昨日作った変数が「private」なら、一時的に名前で判別します
            int playerNum = other.name.Contains("2") ? 2 : 1;

            // GameManagerに「〇Pが落ちたよ！」と伝える
            GameManager.Instance.PlayerFell(playerNum);

            // キャラクターを非表示にする
            other.gameObject.SetActive(false);
        }
    }
}