using UnityEngine;

/// <summary>
/// PlayerMoveControls - ควบคุมการเคลื่อนที่, การกระโดด, การหันหน้า และ Animation ของตัวละคร
/// ตามโครงสร้างจาก https://tanapattara.github.io/unity2d/player-animation-controls
///
/// การตั้งค่าใน Inspector:
///   - speed          : ความเร็วเดิน (ค่าแนะนำ: 5)
///   - jumpForce      : แรงกระโดด (ค่าแนะนำ: 10)
///   - rayLength      : ระยะ Raycast ตรวจพื้น (ค่าแนะนำ: 0.1)
///   - groundLayer    : LayerMask ของพื้น → เลือก "Ground"
///   - leftPoint      : ลาก GameObject LeftPoint (ใต้เท้าซ้าย)
///   - rightPoint     : ลาก GameObject RightPoint (ใต้เท้าขวา)
///
/// Animator Parameters ที่ต้องสร้างใน Animator Controller:
///   - Speed   (Float)   → ความเร็วแนวนอน (ใช้ transition idle↔move)
///   - vSpeed  (Float)   → ความเร็วแนวตั้ง  (ใช้ใน BlendTree กระโดด)
///   - Grounded (Bool)  → true=อยู่บนพื้น   (ใช้ transition jump→idle)
/// </summary>
public class PlayerMoveControls : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 10f;

    [Header("Ground Check")]
    public float rayLength = 0.1f;
    public LayerMask groundLayer;
    public Transform leftPoint;
    public Transform rightPoint;

    // ─── private ───────────────────────────────────────────
    private int direction = 1;          // +1 หันขวา, -1 หันซ้าย
    private bool grounded = false;

    private GatherInput gatherInput;
    private Rigidbody2D rigidbody2d;
    private Animator animator;

    // ───────────────────────────────────────────────────────
    void Start()
    {
        gatherInput  = GetComponent<GatherInput>();
        rigidbody2d  = GetComponent<Rigidbody2D>();
        animator     = GetComponent<Animator>();
    }

    void Update()
    {
        // อัปเดต Animator ทุก frame (ไม่ต้องรอ physics)
        SetAnimatorValues();
    }

    void FixedUpdate()
    {
        CheckStatus();   // ตรวจพื้น
        Move();          // เดิน + หันหน้า
        JumpPlayer();    // กระโดด
    }

    // ─── Movement ──────────────────────────────────────────
    private void Move()
    {
        Flip();
        rigidbody2d.velocity = new Vector2(
            gatherInput.valueX * speed,
            rigidbody2d.velocity.y
        );
    }

    private void Flip()
    {
        // เมื่อทิศทางอินพุต ≠ ทิศที่ตัวละครหันอยู่ → สลับด้าน
        if (gatherInput.valueX * direction < 0)
        {
            transform.localScale = new Vector3(
                -transform.localScale.x,
                transform.localScale.y,
                transform.localScale.z
            );
            direction *= -1;
        }
    }

    // ─── Jump ──────────────────────────────────────────────
    private void JumpPlayer()
    {
        if (gatherInput.jumpInput && grounded)
        {
            rigidbody2d.velocity = new Vector2(
                gatherInput.valueX * speed,
                jumpForce
            );
        }
        gatherInput.jumpInput = false;
    }

    // ─── Ground Check ──────────────────────────────────────
    private void CheckStatus()
    {
        bool leftHit  = false;
        bool rightHit = false;

        if (leftPoint != null)
        {
            RaycastHit2D lh = Physics2D.Raycast(
                leftPoint.position, Vector2.down, rayLength, groundLayer);
            leftHit = lh;
        }

        if (rightPoint != null)
        {
            RaycastHit2D rh = Physics2D.Raycast(
                rightPoint.position, Vector2.down, rayLength, groundLayer);
            rightHit = rh;
        }

        grounded = leftHit || rightHit;
    }

    // ─── Animator ──────────────────────────────────────────
    private void SetAnimatorValues()
    {
        if (animator == null) return;

        // Speed: ค่าสัมบูรณ์ของความเร็วแนวนอน → ใช้ switch idle/move
        animator.SetFloat("Speed",   Mathf.Abs(rigidbody2d.velocity.x));

        // vSpeed: ความเร็วแนวตั้ง → ใช้ใน BlendTree ของ Jump
        animator.SetFloat("vSpeed",  rigidbody2d.velocity.y);

        // Grounded: สถานะพื้น → ใช้ switch jump/idle
        animator.SetBool("Grounded", grounded);
    }

    // ─── Debug ─────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = grounded ? Color.green : Color.red;
        if (leftPoint != null)
            Gizmos.DrawLine(leftPoint.position,
                            leftPoint.position + Vector3.down * rayLength);
        if (rightPoint != null)
            Gizmos.DrawLine(rightPoint.position,
                            rightPoint.position + Vector3.down * rayLength);
    }
}
