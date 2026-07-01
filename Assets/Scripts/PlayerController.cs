
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("プレイヤー番号 (1か2を入れる)")]
    [SerializeField] private int playerIndex = 1;

    [Header("体当たり（ダッシュ）の威力")]
    [SerializeField] private float dashForce = 25f;

    [Header("ダッシュのクールタイム（秒）")]
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody rb;
    private Vector2 inputVector = Vector2.zero;
    private float nextDashTime = 0f; // 次にダッシュできる時刻

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Gamepad.all.Count < playerIndex) return;

        Gamepad myGamepad = Gamepad.all[playerIndex - 1];

        // スティックの入力を取得
        inputVector = myGamepad.leftStick.ReadValue();

        // ダッシュボタン（Aボタン / ×ボタン）が「今押されたか」を判定
        if (myGamepad.buttonSouth.wasPressedThisFrame)
        {
            // クールタイムが終わっているか確認
            if (Time.time >= nextDashTime)
            {
                Dash();
            }
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        // 通常移動（ダッシュ中でない時のベース移動）
        // ※完全に静止した状態からのダッシュを綺麗に決めるため、微修正しています
        if (inputVector.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
    }

    // 体当たり（ダッシュ）の処理
    void Dash()
    {
        // スティックを入れている方向を取得。何も入れていなければ前方にダッシュ
        Vector3 dashDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        if (dashDirection == Vector3.zero)
        {
            // スティックがニュートラルなら、とりあえず初期の前方方向（Z軸プラス）へ
            dashDirection = transform.forward;
        }

        // 瞬間的に速度を上書きしてぶっ飛ばす（力の一瞬の追加：VelocityChange）
        rb.AddForce(dashDirection * dashForce, ForceMode.VelocityChange);

        // クールタイムを設定（現在の時刻 + 設定秒数）
        nextDashTime = Time.time + dashCooldown;

        Debug.Log($"Player {playerIndex} が体当たりした！");
    }
    private void OnCollisionEnter(Collision collision)
    {
        PlayerController targetPlayer = collision.gameObject.GetComponent<PlayerController>();

        if (targetPlayer != null)
        {
            // ダッシュ直後の攻撃判定時間内かチェック
            if (Time.time < nextDashTime - (dashCooldown - 0.2f))
            {
                // 相手をぶっ飛ばす方向を計算（真横に固定）
                Vector3 direction = (collision.transform.position - transform.position).normalized;
                direction.y = 0f;

                Rigidbody targetRb = collision.gameObject.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    // 相手の速度を一度ゼロにしてから、ダッシュ威力の1.5倍で弾き飛ばす
                    targetRb.linearVelocity = Vector3.zero;
                    targetRb.AddForce(direction * (dashForce * 1.5f), ForceMode.VelocityChange);

                    Debug.Log($"Player {playerIndex} の体当たりヒット！");
                }
            }
        }
    }
}