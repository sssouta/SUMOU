using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移動速度")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("プレイヤー番号 (1か2を入れる)")]
    [SerializeField] private int playerIndex = 1;

    private Rigidbody rb;
    private Vector2 inputVector = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 接続されているゲームパッドの数が足りない場合は処理しない
        if (Gamepad.all.Count < playerIndex) return;

        // 自分の番号に応じたゲームパッドを直接指定する (1Pなら0番目、2Pなら1番目)
        Gamepad myGamepad = Gamepad.all[playerIndex - 1];

        // そのパッドの左スティックの値を直接読み取る
        inputVector = myGamepad.leftStick.ReadValue();
    }

    void FixedUpdate()
    {
        // 読み取ったスティックの値で移動させる
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }
}