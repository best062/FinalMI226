using System;
using System.Collections;
using UnityEngine;

/**
 * ==============================================================================
 * PLAYER CONTROLLER (สไตล์ Cookie Run สำหรับ Unity)
 * การควบคุม: วิ่งไปข้างหน้าอัตโนมัติ,
 * กระโดดได้มากสุด 2 ครั้ง (เช็คสัมผัสพื้นโดยตรง ถ้าไม่แตะพื้นได้แค่ 2 ครั้ง เมื่อแตะพื้นค่อยรีเซ็ต),
 * ย่อตัวตามการกดค้าง (Hold to Duck) โดยคำนวณฐานของโมเดลและ Collider ให้แนบติดพื้นเสมอ (เท้าไม่ลอย)
 * ==============================================================================
 */
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: SETTINGS & REFERENCES
       ========================================================================== */
    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการวิ่งไปข้างหน้า")]
    public float runSpeed = 8f;
    [Tooltip("แรงกระโดด")]
    public float jumpForce = 12f;

    [Header("Jump Settings (จำกัดการกระโดด)")]
    [Tooltip("จำนวนครั้งที่กระโดดได้สูงสุดก่อนแตะพื้น (ปกติ 2 ครั้ง)")]
    public int maxJumps = 2;
    [Tooltip("จำนวนครั้งที่กระโดดไปแล้ว (จะถูกรีเซ็ตเป็น 0 เมื่อแตะพื้นโดยตรงเท่านั้น)")]
    [SerializeField] private int jumpCount = 0;

    [Header("Collider & Duck Hitbox Settings")]
    [Tooltip("ความสูงปกติของตัวละคร")]
    public float normalHeight = 2.0f;
    [Tooltip("ความสูงตอนย่อตัว")]
    public float slideHeight = 0.9f;

    /* ==========================================================================
       SECTION 2: HEALTH SYSTEM
       ========================================================================== */
    [Header("Health & Lives")]
    public int maxHearts = 3;
    public int currentHearts = 3;
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    public event Action<int> OnHealthChanged;
    public event Action OnPlayerDied;

    /* ==========================================================================
       SECTION 3: MODEL PLACEHOLDERS
       ========================================================================== */
    [Header("Model Placeholders")]
    [Tooltip("โมเดลตัวละครที่จะทำการย่อขนาด (หากเว้นว่างไว้ ระบบจะผูกโมเดลให้อัตโนมัติ)")]
    public GameObject peoplePlaceholderModel;
    public GameObject slimeModel;

    // Internal Components
    private Rigidbody rb;
    private CapsuleCollider capsuleCol;
    private SphereCollider sphereCol;
    private BoxCollider boxCol;

    [Header("State (Read Only)")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isSliding = false;

    // ตำแหน่งระนาบพื้นของเท้า (ขอบล่างของ Collider และโมเดล)
    private float colliderBottomOffset = -1.0f;

    void Awake()
    {
        // 1. Rigidbody setup
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // 2. Collider setup
        capsuleCol = GetComponent<CapsuleCollider>();
        sphereCol = GetComponent<SphereCollider>();
        boxCol = GetComponent<BoxCollider>();

        if (capsuleCol != null)
        {
            capsuleCol.center = Vector3.zero;
            capsuleCol.height = normalHeight;
            colliderBottomOffset = capsuleCol.center.y - (normalHeight * 0.5f);
        }
        else if (boxCol != null)
        {
            boxCol.center = Vector3.zero;
            boxCol.size = new Vector3(boxCol.size.x, normalHeight, boxCol.size.z);
            colliderBottomOffset = boxCol.center.y - (normalHeight * 0.5f);
        }
        else if (sphereCol != null)
        {
            sphereCol.center = Vector3.zero;
            colliderBottomOffset = sphereCol.center.y - sphereCol.radius;
        }

        // 3. จัดการโมเดลแสดงผล (Visual Model Setup)
        // หากไม่มีการกำหนดโมเดลลูกไว้ ให้ย้าย MeshFilter/Renderer ไปเป็นลูก เพื่อให้ย่อขนาดได้โดยไม่กระทบฟิสิกส์ Rigidbody
        if (peoplePlaceholderModel == null)
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mf != null && mr != null)
            {
                GameObject visualObj = new GameObject("VisualModel");
                visualObj.transform.SetParent(transform, false);
                visualObj.transform.localPosition = Vector3.zero;
                visualObj.transform.localRotation = Quaternion.identity;
                visualObj.transform.localScale = Vector3.one;

                MeshFilter childMf = visualObj.AddComponent<MeshFilter>();
                childMf.sharedMesh = mf.sharedMesh;

                MeshRenderer childMr = visualObj.AddComponent<MeshRenderer>();
                childMr.sharedMaterials = mr.sharedMaterials;

                mr.enabled = false; // ปิดตัวแม่ ให้แสดงผ่านตัวลูกแทน
                peoplePlaceholderModel = visualObj;
            }
        }

        currentHearts = maxHearts;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHearts);
    }

    void Update()
    {
        HandleInputs();
    }

    void FixedUpdate()
    {
        // ขณะย่อตัว ล็อคความเร็วแกน Y ไม่ให้พุ่งขึ้น เพื่อให้ตัวละครแนบติดพื้น 100%
        float yVel = isSliding ? Mathf.Min(0f, rb.velocity.y) : rb.velocity.y;
        rb.velocity = new Vector3(runSpeed, yVel, 0f);
    }

    /* ==========================================================================
       SECTION 4: INPUT HANDLING (กระโดด 2 ครั้ง + ย่อตัวแนบพื้น)
       ========================================================================== */
    private void HandleInputs()
    {
        // 1. กระโดด (Jump): โดดได้ไม่เกิน maxJumps (2 ครั้ง) เมื่อครบต้องแตะพื้นก่อนถึงจะโดดใหม่ได้
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        if (jumpPressed && jumpCount < maxJumps && !isSliding)
        {
            Jump();
        }

        // 2. ย่อตัว / สไลด์ (Hold to Duck): กดค้างก็ย่อ ปล่อยก็คืนค่า
        bool duckHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftControl);

        if (duckHeld && !isSliding)
        {
            StartSlide();
        }
        else if (!duckHeld && isSliding)
        {
            StopSlide();
        }
    }

    private void Jump()
    {
        jumpCount++;
        isGrounded = false;

        Vector3 vel = rb.velocity;
        vel.y = jumpForce;
        rb.velocity = vel;
    }

    private void StartSlide()
    {
        isSliding = true;

        // คำนวณจุดศูนย์กลางใหม่เพื่อให้อิงขอบล่าง (เท้า) อยู่ที่ตำแหน่งเดิมเสมอ
        float newCenterY = colliderBottomOffset + (slideHeight * 0.5f);

        // 1. ย่อ Collider
        if (capsuleCol != null)
        {
            capsuleCol.height = slideHeight;
            capsuleCol.center = new Vector3(0f, newCenterY, 0f);
        }
        else if (boxCol != null)
        {
            boxCol.size = new Vector3(boxCol.size.x, slideHeight, boxCol.size.z);
            boxCol.center = new Vector3(0f, newCenterY, 0f);
        }
        else if (sphereCol != null)
        {
            sphereCol.radius = slideHeight * 0.4f;
            sphereCol.center = new Vector3(0f, newCenterY, 0f);
        }

        // 2. ย่อตัวละคร (Visual Ducking) โดยปรับตำแหน่งลงตามสัดส่วน ทำให้เท้าแนบติดพื้นตลอดเวลา ไม่ลอย
        if (peoplePlaceholderModel != null)
        {
            float scaleRatio = Mathf.Clamp(slideHeight / normalHeight, 0.2f, 1f);
            peoplePlaceholderModel.transform.localScale = new Vector3(1f, scaleRatio, 1f);
            peoplePlaceholderModel.transform.localPosition = new Vector3(0f, newCenterY, 0f);
        }
    }

    private void StopSlide()
    {
        // 1. คืนค่า Collider ปกติ (Center ของ Capsule กลับเป็น 0 เสมอ)
        if (capsuleCol != null)
        {
            capsuleCol.height = normalHeight;
            capsuleCol.center = Vector3.zero;
        }
        else if (boxCol != null)
        {
            boxCol.size = new Vector3(boxCol.size.x, normalHeight, boxCol.size.z);
            boxCol.center = Vector3.zero;
        }
        else if (sphereCol != null)
        {
            sphereCol.radius = normalHeight * 0.5f;
            sphereCol.center = Vector3.zero;
        }

        // 2. คืนค่าโมเดลตัวละครกลับสู่ขนาดและตำแหน่งปกติ (เท้าอยู่ที่เดิม)
        if (peoplePlaceholderModel != null)
        {
            peoplePlaceholderModel.transform.localScale = Vector3.one;
            peoplePlaceholderModel.transform.localPosition = Vector3.zero;
        }

        isSliding = false;
    }

    /* ==========================================================================
       SECTION 5: DIRECT GROUND COLLISION (เช็คการสัมผัสพื้นโดยตรง)
       ========================================================================== */
    private void OnCollisionEnter(Collision collision)
    {
        EvaluateGroundCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateGroundCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void EvaluateGroundCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // หน้าสัมผัสต้องอยู่ใต้เท้า (Normal ชี้ขึ้นด้านบน)
            if (contact.normal.y > 0.3f)
            {
                isGrounded = true;

                // รีเซ็ตการกระโดดเมื่อแตะพื้นจริง
                if (rb.velocity.y <= 0.1f)
                {
                    jumpCount = 0;
                }
                return;
            }
        }
    }

    /* ==========================================================================
       SECTION 6: DAMAGE & INVINCIBILITY
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
        Debug.Log("[Player] Game Over!");
    }
}
