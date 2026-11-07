using System;
using System.Collections;
using UnityEngine;

/**
 * ==============================================================================
 * PLAYER CONTROLLER (สไตล์ Cookie Run สำหรับ Unity)
 * การควบคุมและการตอบสนอง:
 * 1. วิ่งอยู่กับที่ (X = 0) ฉากและสิ่งกีดขวางเลื่อนเข้าหา
 * 2. ฟิสิกส์การกระโดดสไตล์ Cookie Run (Snappy Jump & Fast Fall):
 *    - น้ำหนักตัวกระชับ ไม่ลอยค้างในอากาศนานเกินไป (Apex Hang & Fast Drop)
 *    - ตกลงมาอย่างรวดเร็ว (Fall Gravity Multiplier) เพื่อให้เท้าแตะพื้นและรีเซ็ตรอบกระโดดได้ทัน
 *    - กระโดด 2 จังหวะ (Double Jump) ที่ตัดแรงเฉื่อยเดิมและส่งตัวขึ้นทันที
 *    - มีระบบ Jump Buffering & Coyote Time ตอบสนองต่อการกดปุ่มทันใจ
 *    - ท่าทิ้งตัวลงพื้นกลางอากาศ (Mid-Air Slide Fast-Fall): กดปุ่มสไลด์กลางอากาศเพื่อพุ่งดิ่งลงพื้นทันที
 * 3. ย่อตัวสไลด์แนบพื้น (Hold to Duck) โดยคำนวณฐานโมเดลและ Collider ให้แนบติดพื้นเสมอ
 * 4. ระบบรองรับการตกเหวในอนาคต (Pit / Fall Death System)
 * ==============================================================================
 */
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: MOVEMENT & JUMP SETTINGS (สไตล์ Cookie Run)
       ========================================================================== */
    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการวิ่ง (ใช้คำนวณหรืออ้างอิง)")]
    public float runSpeed = 8f;

    [Header("Cookie Run Jump Physics (ปรับให้กระชับ ลงแตะพื้นไว)")]
    [Tooltip("แรงส่งกระโดดครั้งแรก")]
    public float jumpForce = 13.5f;

    [Tooltip("แรงส่งกระโดดครั้งที่สอง (Double Jump)")]
    public float doubleJumpForce = 12.5f;

    [Tooltip("ตัวคูณแรงโน้มถ่วงทั่วไปตอนลอยตัวขึ้น (ป้องกันการลอยเอื่อย)")]
    public float upwardGravityMultiplier = 2.8f;

    [Tooltip("ตัวคูณแรงโน้มถ่วงตอนเริ่มตกลงมา (Fast Fall) ช่วยให้เท้าแตะพื้นเร็วขึ้น รีเซ็ตกระโดดได้ทัน")]
    public float fallGravityMultiplier = 4.5f;

    [Tooltip("ตัวคูณแรงโน้มถ่วงเมื่อปล่อยปุ่มกระโดดก่อนถึงจุดสูงสุด (Low Jump)")]
    public float lowJumpGravityMultiplier = 3.5f;

    [Tooltip("ความเร็วในการทิ้งตัวลงพื้นเมื่อกดปุ่มสไลด์กลางอากาศ (Mid-Air Fast Fall แบบ Cookie Run)")]
    public float airSlideFastFallSpeed = 16f;

    [Header("Jump Limits & Responsiveness")]
    [Tooltip("จำนวนครั้งที่กระโดดได้สูงสุดก่อนแตะพื้น (ปกติ 2 ครั้ง)")]
    public int maxJumps = 2;

    [Tooltip("จำนวนครั้งที่กระโดดไปแล้ว")]
    [SerializeField] private int jumpCount = 0;

    [Tooltip("ระยะเวลาล็อคสถานะกลางอากาศหลังกระโดด (วินาที) ป้องกันการรีเซ็ตผิดพลาดขณะเพิ่งลอยตัวขึ้น")]
    public float jumpAirborneLockout = 0.2f;

    [Tooltip("ระยะเวลาเผื่อล่วงหน้าเมื่อกดปุ่มกระโดดก่อนเท้าแตะพื้น (Jump Buffer)")]
    public float jumpBufferTime = 0.15f;

    [Tooltip("ระยะเวลาผ่อนปรนหลังหลุดจากขอบพื้นยังกดโดดได้ (Coyote Time)")]
    public float coyoteTime = 0.12f;

    [Header("Collider & Duck Hitbox Settings")]
    [Tooltip("ความสูงปกติของตัวละคร")]
    public float normalHeight = 2.0f;

    [Tooltip("ความสูงตอนย่อตัว")]
    public float slideHeight = 0.9f;

    /* ==========================================================================
       SECTION 2: HEALTH & PIT DEATH (ระบบเลือดและตกเหว)
       ========================================================================== */
    [Header("Health & Lives")]
    public int maxHearts = 3;
    public int currentHearts = 3;
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;

    [Header("Pit Death Support (รองรับระบบตกเหวในอนาคต)")]
    [Tooltip("เปิดใช้งานระบบตรวจจับการตกเหว")]
    public bool enablePitDeath = true;

    [Tooltip("ระดับความสูงแกน Y ที่ถือว่าตกเหว")]
    public float pitDeathThresholdY = -5.0f;

    // Events
    public event Action<int> OnHealthChanged;
    public event Action OnPlayerDied;
    public event Action OnPlayerFellIntoPit;

    /* ==========================================================================
       SECTION 3: MODEL & COMPONENTS
       ========================================================================== */
    [Header("Model Placeholders")]
    public GameObject peoplePlaceholderModel;
    public GameObject slimeModel;

    [Header("Ground Check Settings")]
    [Tooltip("ระยะเผื่อในการตรวจจับพื้นดินใต้ฝ่าเท้า")]
    public float groundCheckDistance = 0.15f;
    public LayerMask groundLayerMask = ~0; // ตรวจจับทุก Layer ตามค่าเริ่มต้น

    [Header("State (Read Only)")]
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private bool isSliding = false;

    private Rigidbody rb;
    private CapsuleCollider capsuleCol;
    private SphereCollider sphereCol;
    private BoxCollider boxCol;

    private float colliderBottomOffset = -1.0f;
    private float jumpBufferCounter = 0f;
    private float coyoteCounter = 0f;
    private float airborneLockoutTimer = 0f;
    private bool isJumpCut = false;

    void Awake()
    {
        // 1. Rigidbody Setup
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // 2. Collider Setup
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

        // 3. Visual Model Setup
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

                mr.enabled = false;
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
        CheckGroundedStatus();
        HandleInputs();
        CheckPitFall();
    }

    void FixedUpdate()
    {
        ApplySnappyGravity();

        // ขณะย่อตัว ล็อคแกน Y ไม่ให้พุ่งขึ้น และรักษาแกน X=0
        float yVel = isSliding ? Mathf.Min(0f, rb.velocity.y) : rb.velocity.y;
        rb.velocity = new Vector3(0f, yVel, 0f);
    }

    /* ==========================================================================
       SECTION 4: GROUND DETECTION & JUMP RESET (แก้ปัญหาการรีเซ็ตผิดจังหวะ)
       ========================================================================== */
    private void CheckGroundedStatus()
    {
        // 1. ถ้ายังอยู่ในช่วงคูลดาวน์เพิ่งกระโดดขึ้น (Airborne Lockout) ให้ลดเวลาและคงสถานะกลางอากาศไว้
        if (airborneLockoutTimer > 0f)
        {
            airborneLockoutTimer -= Time.deltaTime;
            isGrounded = false;
            return;
        }

        // 2. ถ้าความเร็วแกน Y ยังพุ่งขึ้นอยู่ (velocity.y > 0.1f) แสดงว่ากำลังลอยขึ้น ห้ามตัดสินว่าแตะพื้นเด็ดขาด
        if (rb.velocity.y > 0.1f)
        {
            isGrounded = false;
            return;
        }

        // 3. ยิง Raycast ใต้ฝ่าเท้า (กรองไม่ให้โดน Collider ของตัวเอง และไม่โดน Trigger)
        Vector3 footPosition = transform.position + new Vector3(0f, colliderBottomOffset + 0.05f, 0f);
        RaycastHit[] hits = Physics.RaycastAll(footPosition, Vector3.down, groundCheckDistance + 0.1f, groundLayerMask, QueryTriggerInteraction.Ignore);

        bool hitGround = false;
        foreach (var h in hits)
        {
            if (h.collider != null && !h.collider.transform.IsChildOf(transform) && !h.collider.isTrigger)
            {
                hitGround = true;
                break;
            }
        }

        // 4. สัมผัสพื้นจริงขณะกำลังตกลงมาหรือหยุดนิ่ง -> รีเซ็ตกระโดด
        if (hitGround)
        {
            SetGrounded();
        }
        else
        {
            isGrounded = false;
            coyoteCounter -= Time.deltaTime;
        }
    }

    private void SetGrounded()
    {
        // เงื่อนไขความปลอดภัยสูงสุด: ต้องพ้นช่วงเพิ่งกระโดด และความเร็วแกน Y ต้องไม่พุ่งขึ้น
        if (airborneLockoutTimer > 0f || rb.velocity.y > 0.1f) return;

        isGrounded = true;
        jumpCount = 0;
        isJumpCut = false;
        coyoteCounter = coyoteTime;
    }

    /* ==========================================================================
       SECTION 5: INPUT HANDLING (กระโดด 2 จังหวะ + สไลด์ดิ่งพื้น)
       ========================================================================== */
    private void HandleInputs()
    {
        // 1. รับคำสั่งกระโดด พร้อม Jump Buffer
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
        bool jumpReleased = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow);

        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // ตัดแรงกระโดดเมื่อปล่อยปุ่มเร็ว (Low Jump)
        if (jumpReleased && rb.velocity.y > 0f)
        {
            isJumpCut = true;
        }

        // ตรวจสอบเงื่อนไขกระโดด
        if (jumpBufferCounter > 0f)
        {
            // กระโดดครั้งแรก (อยู่บนพื้น หรืออยู่ในช่วง Coyote Time)
            if ((isGrounded || coyoteCounter > 0f) && jumpCount == 0 && !isSliding)
            {
                ExecuteJump(jumpForce);
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }
            // กระโดดครั้งที่สอง (Double Jump กลางอากาศ)
            else if (jumpCount < maxJumps && !isSliding)
            {
                ExecuteJump(doubleJumpForce);
                jumpBufferCounter = 0f;
            }
        }

        // 2. รับคำสั่งย่อตัว / สไลด์ (Hold to Duck)
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

    private void ExecuteJump(float force)
    {
        jumpCount++;
        isGrounded = false;
        isJumpCut = false;
        coyoteCounter = 0f;
        jumpBufferCounter = 0f;

        // ล็อคสถานะกลางอากาศทันที 0.2 วินาที ป้องกัน Raycast/Collision รีเซ็ตกระโดดขณะเพิ่งพุ่งตัวขึ้น
        airborneLockoutTimer = jumpAirborneLockout;

        // ตัดความเร็วแนวดิ่งเดิมออกทันที แล้วใส่แรงกระโดดใหม่เพื่อให้ Double Jump พุ่งตอบสนองทันใจ
        Vector3 vel = rb.velocity;
        vel.y = force;
        rb.velocity = vel;
    }

    /* ==========================================================================
       SECTION 6: SNAPPY GRAVITY LOGIC (ฟิสิกส์ตกไวสไตล์ Cookie Run)
       ========================================================================== */
    private void ApplySnappyGravity()
    {
        // 1. กำลังตกลงมา (Falling down): เพิ่มแรงโน้มถ่วงสูงมาก เพื่อให้ตัวละครลงถึงพื้นไว
        if (rb.velocity.y < 0f)
        {
            float extraGravity = (fallGravityMultiplier - 1f) * Physics.gravity.y;
            rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);
        }
        // 2. ปล่อยปุ่มกระโดดเร็วก่อนถึงจุดสูงสุด (Low Jump): ตกเร็วขึ้นตามใจผู้เล่น
        else if (rb.velocity.y > 0f && isJumpCut)
        {
            float extraGravity = (lowJumpGravityMultiplier - 1f) * Physics.gravity.y;
            rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);
        }
        // 3. กำลังลอยขึ้นปกติ (Rising): ใช้แรงโน้มถ่วงกระชับ ไม่ลอยเคว้ง
        else if (rb.velocity.y > 0f)
        {
            float extraGravity = (upwardGravityMultiplier - 1f) * Physics.gravity.y;
            rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);
        }
    }

    /* ==========================================================================
       SECTION 7: SLIDE & AIR FAST-FALL (ย่อตัวสไลด์และพุ่งลงพื้น)
       ========================================================================== */
    public void StartSlide()
    {
        isSliding = true;

        // ท่าเฉพาะ Cookie Run: ถ้ากดสไลด์ขณะอยู่กลางอากาศ ให้ทิ้งตัวดิ่งลงพื้นอย่างรวดเร็ว (Fast-Fall)
        if (!isGrounded)
        {
            rb.velocity = new Vector3(0f, -airSlideFastFallSpeed, 0f);
            airborneLockoutTimer = 0f; // เมื่อสั่งดิ่งลงพื้น ให้พร้อมรีเซ็ตสไลด์ทันทีที่แตะพื้น
        }

        float newCenterY = colliderBottomOffset + (slideHeight * 0.5f);

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

        if (peoplePlaceholderModel != null)
        {
            float scaleRatio = Mathf.Clamp(slideHeight / normalHeight, 0.2f, 1f);
            peoplePlaceholderModel.transform.localScale = new Vector3(1f, scaleRatio, 1f);
            peoplePlaceholderModel.transform.localPosition = new Vector3(0f, newCenterY, 0f);
        }
    }

    public void StopSlide()
    {
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

        if (peoplePlaceholderModel != null)
        {
            peoplePlaceholderModel.transform.localScale = Vector3.one;
            peoplePlaceholderModel.transform.localPosition = Vector3.zero;
        }

        isSliding = false;
    }

    /* ==========================================================================
       SECTION 8: COLLISION BACKUP & PIT DEATH
       ========================================================================== */
    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateCollision(Collision collision)
    {
        // ถ้าเพิ่งกระโดด หรือความเร็วดิ่งยังพุ่งขึ้นอยู่ ห้ามรีเซ็ตว่าแตะพื้น
        if (airborneLockoutTimer > 0f || rb.velocity.y > 0.1f) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.4f)
            {
                SetGrounded();
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 footPosition = transform.position + new Vector3(0f, colliderBottomOffset + 0.05f, 0f);
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(footPosition, footPosition + Vector3.down * (groundCheckDistance + 0.1f));
    }

    private void CheckPitFall()
    {
        if (!enablePitDeath) return;

        if (transform.position.y < pitDeathThresholdY && currentHearts > 0)
        {
            Debug.Log("[Player] ตัวละครตกลงสู่เหว!");
            OnPlayerFellIntoPit?.Invoke();
            Die();
        }
    }

    /* ==========================================================================
       SECTION 9: DAMAGE & DEATH
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
