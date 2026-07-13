using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // --- 既存の変数（moveSpeed, dashForceなど）はそのまま ---
    [Header("移動速度")][SerializeField] private float moveSpeed = 7f;
    [Header("プレイヤー番号 (1か2を入れる)")][SerializeField] private int playerIndex = 1;
    [Header("体当たり（ダッシュ）の威力")][SerializeField] private float dashForce = 25f;
    [Header("ダッシュのクールタイム（秒）")][SerializeField] private float dashCooldown = 1f;

    // 【ここを追加！】ヒット時に出すエフェクトのプレハブ
    [Header("ヒット時のエフェクトプレハブ")]
    [SerializeField] private GameObject hitEffectPrefab;

    private Rigidbody rb;
    private Vector2 inputVector = Vector2.zero;
    private float nextDashTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {

        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;
        if (Gamepad.all.Count < playerIndex) return;

        Gamepad myGamepad = Gamepad.all[playerIndex - 1];
        inputVector = myGamepad.leftStick.ReadValue();
        if (myGamepad.buttonSouth.wasPressedThisFrame && Time.time >= nextDashTime)
        {
            Dash();
        }
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive) return;

        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        if (inputVector.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
    }

    void Dash()
    {
        Vector3 dashDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);
        nextDashTime = Time.time + dashCooldown;
        Debug.Log($"Player {playerIndex} が体当たりした！");
    }

    // --- ここを丸ごとアップデート！ ---
    private void OnCollisionEnter(Collision collision)
    {
        PlayerController targetPlayer = collision.gameObject.GetComponent<PlayerController>();

        if (targetPlayer != null)
        {
            // ダッシュ直後の攻撃判定時間内かチェック
            if (Time.time < nextDashTime - (dashCooldown - 0.2f))
            {
                // 相手をぶっ飛ばす処理（既存）
                Vector3 direction = (collision.transform.position - transform.position).normalized;
                direction.y = 0f;
                Rigidbody targetRb = collision.gameObject.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    targetRb.linearVelocity = Vector3.zero;
                    targetRb.AddForce(direction * (dashForce * 1.5f), ForceMode.VelocityChange);
                }

                // 【ここを追加！】ヒットエフェクトを発生させる
                if (hitEffectPrefab != null)
                {
                    // ぶつかった接点（ContactPoint）の位置を取得
                    ContactPoint contact = collision.contacts[0];
                    Vector3 pos = contact.point;

                    // その位置にエフェクトを生成する（3秒後に自動で消えるようにする）
                    GameObject effect = Instantiate(hitEffectPrefab, pos, Quaternion.identity);
                    Destroy(effect, 3f);
                }

                Debug.Log($"Player {playerIndex} の体当たりヒット！エフェクト生成！");
            }
        }
    }
}