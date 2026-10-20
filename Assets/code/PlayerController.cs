using System;
using System.Collections;
using UnityEngine;

/**
 * ==============================================================================
 * PLAYER CONTROLLER (สไตล์ Cookie Run สำหรับ Unity)
 * ทำหน้าที่: ควบคุมการวิ่ง, กระโดด, สไลด์, ปรับ Hitbox และระบบเลือด 3 หัวใจ
 * ==============================================================================
 */
public class PlayerController : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: SETTINGS & REFERENCES (การตั้งค่าและส่วนประกอบตัวละคร)
       ทำหน้าที่: กำหนดค่าความเร็ว, แรงกระโดด, ขนาด Collider ตอนสไลด์
       ========================================================================== */
    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการวิ่งไปข้างหน้า")]
    public float runSpeed = 8f;
    [Tooltip("แรงกระโดด")]
    public float jumpForce = 12f;
    [Tooltip("ระยะเวลาการสไลด์ (วินาที)")]
    public float slideDuration = 0.8f;

    [Header("Collider Hitbox Settings (สำหรับการก้มสไลด์หลบ)")]
    [Tooltip("ความสูงปกติของ Capsule Collider")]
    public float normalHeight = 2.0f;
    [Tooltip("ความสูงขณะสไลด์ (ลดลงเพื่อมุดลอดสิ่งกีดขวาง)")]
    public float slideHeight = 0.9f;
    public Vector3 normalCenter = new Vector3(0, 1.0f, 0);
    public Vector3 slideCenter = new Vector3(0, 0.45f, 0);

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    /* ==========================================================================
       SECTION 2: HEALTH SYSTEM (ระบบเลือด 3 หัวใจ และสถานะอมตะ)
       ทำหน้าที่: จัดการเลือดสูงสุด 3 ดวง และช่วงอมตะชั่วขณะ (I-Frames) ป้องกันดาเมจซ้ำ
       ========================================================================== */
    [Header("Health & Lives")]
    public int maxHearts = 3;
    public int currentHearts = 3;
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    // Events สำหรับส่งข้อมูลให้ UI อัปเดต
    public event Action<int> OnHealthChanged;
    public event Action OnPlayerDied;

    /* ==========================================================================
       SECTION 3: FUTURE MODEL HOOK (จุดเชื่อมต่อโมเดล Slime ในอนาคต)
       ทำหน้าที่: จัดเตรียมตำแหน่งสลับระหว่าง People Placeholder กับโมเดล Slime
       ========================================================================== */
    [Header("Model Placeholders")]
    [Tooltip("โมเดลคนชั่วคราว (Placeholder)")]
    public GameObject peoplePlaceholderModel;
    [Tooltip("โมเดล Slime ในอนาคต (เมื่อพร้อมให้นำ Prefab มาเสียบแทน)")]
    public GameObject slimeModel; // TODO: นำโมเดล Slime 3D มาใส่ที่นี่ในอนาคต

    // Internal Components
    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isGrounded = true;
    private bool isSliding = false;
    private Coroutine slideCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        currentHearts = maxHearts;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHearts);
    }

    void Update()
    {
        CheckGrounded();
        HandleInputs();
    }

    void FixedUpdate()
    {
        // วิ่งไปข้างหน้าอัตโนมัติตามสไตล์ Cookie Run
        rb.velocity = new Vector3(runSpeed, rb.velocity.y, 0);
    }

    /* ==========================================================================
       SECTION 4: INPUT HANDLING (การควบคุม: กระโดด และ สไลด์ เท่านั้น)
       ทำหน้าที่: รับปุ่ม W/ArrowUp/Space (กระโดด) และ S/ArrowDown (สไลด์)
       ========================================================================== */
    private void HandleInputs()
    {
        // 1. กดกระโดด (Jump)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && isGrounded && !isSliding)
        {
            Jump();
        }

        // 2. กดสไลด์ (Slide)
        if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && isGrounded && !isSliding)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlideRoutine());
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, 0);
        // TODO: สั่งเล่น Animator Jump ของ People หรือ Slime
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;

        // ย่อขนาด Collider ลงมาเพื่อลอดสิ่งกีดขวางระดับสูง (High Obstacle)
        col.height = slideHeight;
        col.center = slideCenter;

        // TODO: สั่งเล่น Animator Slide

        yield return new WaitForSeconds(slideDuration);

        // คืนขนาด Collider สู่ระดับปกติ
        col.height = normalHeight;
        col.center = normalCenter;
        isSliding = false;
    }

    private void CheckGrounded()
    {
        if (groundCheckPoint != null)
        {
            isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.25f, groundLayer);
        }
    }

    /* ==========================================================================
       SECTION 5: DAMAGE & INVINCIBILITY (การรับความเสียหายและการตาย)
       ทำหน้าที่: หักลด 1 หัวใจเมื่อชนสิ่งกีดขวาง และเปิดสถานะอมตะชั่วคราว
       ========================================================================== */
    public void TakeDamage(int damage = 1)
    {
        if (isInvincible || currentHearts <= 0) return;

        currentHearts = Mathf.Max(0, currentHearts - damage);
        OnHealthChanged?.Invoke(currentHearts);

        if (currentHearts <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        // เอฟเฟกต์กระพริบตัวละคร
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            foreach (var r in renderers) r.enabled = !r.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        foreach (var r in renderers) r.enabled = true;
        isInvincible = false;
    }

    private void Die()
    {
        runSpeed = 0;
        rb.velocity = Vector3.zero;
        OnPlayerDied?.Invoke();
        Debug.Log("[Player] Game Over: หัวใจหมด 3 ดวงแล้ว!");
    }
}
