using UnityEngine;

public class PlayerCollisionEffects : MonoBehaviour
{
    [Header("激突時に発生させる砂煙プレハブ")]
    [SerializeField] private GameObject sandDustPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        // ぶつかった相手が「Player」タグを持つオブジェクトの場合のみ実行
        if (collision.gameObject.CompareTag("Player"))
        {
            // 二つのプレイヤーが実際にぶつかった位置（接点）の座標を取得する
            if (collision.contacts.Length > 0)
            {
                ContactPoint contact = collision.contacts[0];
                Vector3 spawnPoint = contact.point;

                // 砂煙エフェクトを発生させる
                if (sandDustPrefab != null)
                {
                    // 衝突位置にエフェクトを生成
                    GameObject effectInstance = Instantiate(sandDustPrefab, spawnPoint, Quaternion.identity);

                    // 1.5秒後に自動的にメモリから削除する（ゴミが残らないようにする）
                    Destroy(effectInstance, 1.5f);
                }
            }
        }
    }
}