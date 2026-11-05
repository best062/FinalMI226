using UnityEngine;

/**
 * ==============================================================================
 * GENERIC LETTER BOX (กล่องสุ่ม / ถุงวัตถุดิบตัวอักษร)
 * ทำหน้าที่:
 * 1. ใช้ Prefab 3D ชิ้นเดียว (เช่น โมเดลกล่องสมบัติ, ถุงผ้า, ผลึกอัญมณี) แทนการเสกตัวอักษร 3D แยกรายตัว
 * 2. เคลื่อนที่จากขวาไปซ้ายตามฉากด้วย ScrollLeft และทำลายตัวเองอัตโนมัติเมื่อพ้นจอ
 * 3. หมุนรอบตัวเองและลอยขึ้นลงเบาๆ (Bobbing Effect) เพื่อให้ดูโดดเด่นและน่าเก็บ
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
    [Tooltip("โมเดลลูกสำหรับหมุน (หากเว้นว่างไว้ จะหมุน Transform หลักของกล่องทั้งหมด)")]
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
    private bool isCollected = false;
    private Collider boxCollider;

    void Start()
    {
        // กำหนด Visual Model
        if (visualModel == null)
        {
            visualModel = transform;
        }

        initialLocalPos = visualModel.localPosition;

        // ตรวจสอบ Collider และตั้งค่าเป็น Trigger อัตโนมัติ
        boxCollider = GetComponent<Collider>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    void Update()
    {
        // 1. หมุนโมเดล 3D รอบแกน Y
        if (visualModel != null)
        {
            visualModel.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

            // 2. เอฟเฟกต์ลอยขึ้นลง (Bobbing)
            if (enableBobbing)
            {
                float newY = initialLocalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                visualModel.localPosition = new Vector3(initialLocalPos.x, newY, initialLocalPos.z);
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

