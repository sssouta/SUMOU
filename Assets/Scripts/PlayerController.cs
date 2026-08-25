using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("プレイヤー番号（1か2を入れる）")]
    [SerializeField] private int playerIndex = 1;

    [Header("体当たり（ダッシュ）の威力")]
    [SerializeField] private float dashForce = 25f;

    [Header("ダッシュのクールタイム（秒）")]
    [SerializeField] private float dashCooldown = 1f;

    [Header("ヒット時のエフェクトプレハブ")]
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("攻撃SE")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("ダッシュした瞬間の音")]
    [SerializeField] private AudioClip dashSound;

    [Tooltip("片方だけがダッシュして当たったときの音")]
    [SerializeField] private AudioClip hitSound;

    [Tooltip("2人ともダッシュ中にぶつかったときの音")]
    [SerializeField] private AudioClip clashSound;

    [Header("ヒット音の連続再生防止時間")]
    [SerializeField] private float hitSoundCooldown = 0.1f;

    [Header("ダッシュの攻撃判定時間")]
    [SerializeField] private float attackActiveTime = 0.2f;

    private Rigidbody rb;
    private Vector2 inputVector = Vector2.zero;

    private float nextDashTime = 0f;
    private float dashStartTime = -999f;
    private float lastHitSoundTime = -999f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError(
                $"Player {playerIndex} にRigidbodyが付いていません。"
            );
        }

        // AudioSourceをInspectorで設定し忘れても自動取得
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogWarning(
                $"Player {playerIndex} にAudioSourceがありません。"
            );
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsGameActive)
        {
            inputVector = Vector2.zero;
            return;
        }

        if (Gamepad.all.Count < playerIndex)
        {
            inputVector = Vector2.zero;
            return;
        }

        Gamepad myGamepad = Gamepad.all[playerIndex - 1];

        inputVector = myGamepad.leftStick.ReadValue();

        if (myGamepad.buttonSouth.wasPressedThisFrame &&
            Time.time >= nextDashTime)
        {
            Dash();
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null &&
            !GameManager.Instance.IsGameActive)
        {
            return;
        }

        if (rb == null)
        {
            return;
        }

        Vector3 moveDirection = new Vector3(
            inputVector.x,
            0f,
            inputVector.y
        );

        if (inputVector.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.linearVelocity.y,
                moveDirection.z * moveSpeed
            );
        }
    }

    private void Dash()
    {
        if (rb == null)
        {
            return;
        }

        Vector3 dashDirection = new Vector3(
            inputVector.x,
            0f,
            inputVector.y
        ).normalized;

        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }

        rb.AddForce(
            dashDirection * dashForce,
            ForceMode.VelocityChange
        );

        dashStartTime = Time.time;
        nextDashTime = Time.time + dashCooldown;

        // ★ 攻撃ボタンを押してダッシュした瞬間にSE再生
        PlayDashSound();

        Debug.Log($"Player {playerIndex} が体当たりした！");
    }

    public bool IsAttacking()
    {
        return Time.time <= dashStartTime + attackActiveTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerController targetPlayer =
            collision.gameObject.GetComponent<PlayerController>();

        if (targetPlayer == null)
        {
            return;
        }

        bool thisPlayerIsAttacking = IsAttacking();
        bool targetPlayerIsAttacking = targetPlayer.IsAttacking();

        // どちらも攻撃中ではない普通の接触なら何もしない
        if (!thisPlayerIsAttacking && !targetPlayerIsAttacking)
        {
            return;
        }

        // 2人ともダッシュ中に衝突
        if (thisPlayerIsAttacking && targetPlayerIsAttacking)
        {
            if (playerIndex == 1)
            {
                CreateHitEffect(collision);
                PlayClashSound();

                Debug.Log("2人の体当たりが同時に衝突！");
            }

            return;
        }

        // 自分がダッシュしていない場合は処理しない
        if (!thisPlayerIsAttacking)
        {
            return;
        }

        BlowAwayTarget(collision);
        CreateHitEffect(collision);
        PlayHitSound();

        Debug.Log(
            $"Player {playerIndex} の体当たりがヒット！"
        );
    }

    private void BlowAwayTarget(Collision collision)
    {
        Vector3 direction =
            collision.transform.position - transform.position;

        direction.y = 0f;
        direction.Normalize();

        Rigidbody targetRb =
            collision.gameObject.GetComponent<Rigidbody>();

        if (targetRb == null)
        {
            return;
        }

        targetRb.linearVelocity = Vector3.zero;

        targetRb.AddForce(
            direction * (dashForce * 1.5f),
            ForceMode.VelocityChange
        );
    }

    private void CreateHitEffect(Collision collision)
    {
        if (hitEffectPrefab == null ||
            collision.contactCount == 0)
        {
            return;
        }

        ContactPoint contact = collision.contacts[0];

        GameObject effect = Instantiate(
            hitEffectPrefab,
            contact.point,
            Quaternion.identity
        );

        Destroy(effect, 3f);
    }

    // ダッシュした瞬間
    private void PlayDashSound()
    {
        if (audioSource == null || dashSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(dashSound);
    }

    // 攻撃が相手にヒット
    private void PlayHitSound()
    {
        PlaySound(hitSound);
    }

    // 2人同時攻撃
    private void PlayClashSound()
    {
        PlaySound(clashSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning(
                $"Player {playerIndex} のAudio Sourceが設定されていません。"
            );
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning(
                $"Player {playerIndex} のAudio Clipが設定されていません。"
            );
            return;
        }

        if (Time.time < lastHitSoundTime + hitSoundCooldown)
        {
            return;
        }

        audioSource.PlayOneShot(clip);
        lastHitSoundTime = Time.time;
    }
}