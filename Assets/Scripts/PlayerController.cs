using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ★追加：クールタイムUI用
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("プレイヤー番号（1か2を入れる）")]
    [SerializeField] private int playerIndex = 1;

    [Header("体当たり（ダッシュ）の速度")]
    [SerializeField] private float dashSpeed = 20f;

    [Header("ダッシュの継続時間（秒）")]
    [SerializeField] private float dashDuration = 0.25f;

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

    // =========================================
    // ★追加：クールタイムUI
    // =========================================
    [Header("クールタイムUI")]
    [SerializeField] private Image cooldownFill;

    private Rigidbody rb;
    private Vector2 inputVector = Vector2.zero;

    private float nextDashTime = 0f;
    private float dashStartTime = -999f;
    private float lastHitSoundTime = -999f;

    private bool isDashing = false;
    private Vector3 currentDashDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError($"Player {playerIndex} にRigidbodyが付いていません。");
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogWarning($"Player {playerIndex} にAudioSourceがありません。");
        }

        // ★追加
        // ゲーム開始時は攻撃可能なのでゲージを満タンにする
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 1f;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            inputVector = Vector2.zero;

            // ★追加
            UpdateCooldownUI();

            return;
        }

        if (Gamepad.all.Count < playerIndex)
        {
            inputVector = Vector2.zero;

            // ★追加
            UpdateCooldownUI();

            return;
        }

        Gamepad myGamepad = Gamepad.all[playerIndex - 1];

        inputVector = myGamepad.leftStick.ReadValue();

        // クールタイムが終了している場合だけダッシュ可能
        if (myGamepad.buttonSouth.wasPressedThisFrame && Time.time >= nextDashTime)
        {
            Dash();
        }

        // ★追加：毎フレームゲージを更新
        UpdateCooldownUI();
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            return;
        }

        if (rb == null)
        {
            return;
        }

        // ダッシュ中
        if (isDashing)
        {
            rb.linearVelocity = new Vector3(
                currentDashDirection.x * dashSpeed,
                rb.linearVelocity.y,
                currentDashDirection.z * dashSpeed
            );

            return;
        }

        // 通常移動
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
        if (rb == null || isDashing)
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

        currentDashDirection = dashDirection;

        dashStartTime = Time.time;

        // ★クールタイム開始
        nextDashTime = Time.time + dashCooldown;

        // ★攻撃した瞬間にゲージを空にする
        if (cooldownFill != null)
        {
            cooldownFill.fillAmount = 0f;
        }

        PlayDashSound();

        StartCoroutine(DashRoutine());

        Debug.Log($"Player {playerIndex} が体当たりした！");
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }

    public bool IsAttacking()
    {
        return Time.time <= dashStartTime + attackActiveTime;
    }

    // =========================================
    // ★追加：クールタイムゲージ処理
    // =========================================
    private void UpdateCooldownUI()
    {
        if (cooldownFill == null)
        {
            return;
        }

        // あと何秒クールタイムが残っているか
        float remainingTime = nextDashTime - Time.time;

        // クールタイム終了
        if (remainingTime <= 0f)
        {
            cooldownFill.fillAmount = 1f;
            return;
        }

        // 0 → 1まで徐々に増やす
        float progress = 1f - (remainingTime / dashCooldown);

        cooldownFill.fillAmount = Mathf.Clamp01(progress);
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

        if (!thisPlayerIsAttacking && !targetPlayerIsAttacking)
        {
            return;
        }

        // 2人とも攻撃中
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

        if (!thisPlayerIsAttacking)
        {
            return;
        }

        BlowAwayTarget(collision);
        CreateHitEffect(collision);
        PlayHitSound();

        Debug.Log($"Player {playerIndex} の体当たりがヒット！");
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
            direction * (dashSpeed * 2.5f),
            ForceMode.VelocityChange
        );
    }

    private void CreateHitEffect(Collision collision)
    {
        if (hitEffectPrefab == null || collision.contactCount == 0)
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

    private void PlayDashSound()
    {
        if (audioSource == null || dashSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(dashSound);
    }

    private void PlayHitSound()
    {
        PlaySound(hitSound);
    }

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