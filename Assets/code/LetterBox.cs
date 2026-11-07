using UnityEngine;

/**
 * ==============================================================================
 * GENERIC LETTER BOX (กล่องสุ่ม / ถุงวัตถุดิบตัวอักษร)
 * ทำหน้าที่:
 * 1. ใช้ Prefab 3D ชิ้นเดียว (เช่น โมเดลกล่องสมบัติ, ถุงผ้า, ผลึกอัญมณี) แทนการเสกตัวอักษร 3D แยกรายตัว
 * 2. เคลื่อนที่จากขวาไปซ้ายตามฉากด้วย ScrollLeft และทำลายตัวเองอัตโนมัติเมื่อพ้นจอ
 * 3. หมุนรอบตัวเองและลอยขึ้นลงเบาๆ (Bobbing Effect) โดยไม่รบกวนแกน X ของ ScrollLeft
 * 4. เมื่อผู้เล่นวิ่งชน จะส่งต่อไปยัง LetterDropManager เพื่อคำนวณสุ่มตัวอักษรเบื้องหลัง
 * ==============================================================================
 */
[RequireComponent(typeof(ScrollLeft))]
[RequireComponent(typeof(Collider))]
public class LetterBox : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: VISUALS & ANIMATIONS (การแสดงผลโมเดลและแอนิเมชัน)
       ========================================================================== */
    [Header("3D Visuals & Rotation")]
    [Tooltip("โมเดลลูกสำหรับหมุน (หากเว้นว่างไว้ ระบบจะหาในโหนดลูกให้อัตโนมัติ หรือหมุนตัวมันเอง)")]
    public Transform visualModel;

    [Tooltip("ความเร็วในการหมุนรอบแกน Y (องศาต่อวินาที)")]
    public float rotateSpeed = 75f;

    [Header("Floating / Bobbing Effect")]
    [Tooltip("เปิดเอฟเฟกต์ลอยขึ้นลงเบาๆ ให้เห็นเด่นชัด")]
    public bool enableBobbing = true;

    [Tooltip("ความถี่ในการลอยขึ้นลง")]
    public float bobSpeed = 3.5f;

    [Tooltip("ระยะความสูงในการลอยขึ้นลง")]
    public float bobHeight = 0.2f;

    private Vector3 initialLocalPos;
    private float baseY;
    private bool isChildModel = false;
    private bool isCollected = false;
    private Collider boxCollider;

    void Start()
    {
        // 1. ตรวจสอบ Visual Model: หากมีโหนดลูกให้ใช้โหนดลูกเพื่อแยกการเคลื่อนที่อย่างอิสระ
        if (visualModel == null)
        {
            if (transform.childCount > 0)
            {
                visualModel = transform.GetChild(0);
            }
            else
            {
                visualModel = transform;
            }
        }

        isChildModel = (visualModel != null && visualModel != transform);

        if (isChildModel)
        {
            initialLocalPos = visualModel.localPosition;
        }
        else
        {
            baseY = transform.position.y;
        }

        // 2. ตรวจสอบ Collider และตั้งค่าเป็น Trigger อัตโนมัติ
        boxCollider = GetComponent<Collider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }

        // 3. ป้องกันปัญหา Rigidbody ขัดขวางการเคลื่อนที่ของ ScrollLeft
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 4. ตรวจสอบความพร้อมของ ScrollLeft
        ScrollLeft scroll = GetComponent<ScrollLeft>();
        if (scroll != null && !scroll.enabled)
        {
            scroll.enabled = true;
        }
    }

    void Update()
    {
        // แอนิเมชันหมุนและลอยขึ้นลง
        if (isChildModel && visualModel != null)
        {
            // กรณีเป็นโมเดลลูก: หมุนและปรับ Local Position ของลูก โดยไม่กระทบ Parent ที่กำลังเลื่อนซ้าย
            visualModel.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.Self);

            if (enableBobbing)
            {
                float newY = initialLocalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                visualModel.localPosition = new Vector3(initialLocalPos.x, newY, initialLocalPos.z);
            }
        }
        else
        {
            // กรณีเป็นโมเดลตัวแม่เอง: หมุนตัวแม่ และปรับเฉพาะแกน Y เท่านั้น (ห้ามแตะแกน X เพื่อให้ ScrollLeft เลื่อนได้ตามปกติ)
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

            if (enableBobbing)
            {
                float newY = baseY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                Vector3 curPos = transform.position;
                curPos.y = newY;
                transform.position = curPos; // รักษาค่า X ของ ScrollLeft ไว้ 100%
            }
        }
    }

    /* ==========================================================================
       SECTION 2: TRIGGER DETECTION (ตรวจจับการชนกับผู้เล่น)
       ========================================================================== */
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;

            // ปิด Collider ทันทีเพื่อป้องกันการตรวจจับซ้ำซ้อนในเฟรมเดียวกัน
            if (boxCollider != null)
            {
                boxCollider.enabled = false;
            }

            PlayerController player = other.GetComponent<PlayerController>();

            // ส่งต่อไปยังระบบ Smart Drop คำนวณเบื้องหลัง
            if (LetterDropManager.Instance != null)
            {
                LetterDropManager.Instance.ProcessLetterBoxPickup(this, player);
            }
            else
            {
                // Fallback กรณีที่ไม่ได้วาง LetterDropManager ไว้ใน Scene
                HandleFallbackPickup(other.gameObject);
            }
        }
    }

    private void HandleFallbackPickup(GameObject playerObj)
    {
        LetterInventory inventory = playerObj.GetComponent<LetterInventory>();
        if (inventory != null)
        {
            char fallbackChar = (char)Random.Range('A', 'Z' + 1);
            inventory.AddLetter(fallbackChar);
            Debug.Log($"[LetterBox] (Fallback) ไม่พบ LetterDropManager สุ่มมอบตัวอักษร: '{fallbackChar}'");
        }
        Destroy(gameObject);
    }
}
