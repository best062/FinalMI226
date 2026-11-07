using UnityEngine;

/**
 * ==============================================================================
 * SCROLL LEFT (ระบบเลื่อนวัตถุจากขวาไปซ้าย แยกความเร็ววัตถุกับฉากหลัง)
 * ทำหน้าที่:
 * 1. เลื่อนวัตถุ (ฉากหลัง, พื้น, สิ่งกีดขวาง, กล่องสุ่ม, เหรียญ) จากขวาไปซ้าย (-X)
 * 2. แยกประเภทความเร็วชัดเจน:
 *    - GameplayObject: อิง objectScrollSpeed จาก GameManager (ความเร็วของอุปสรรคและไอเทม)
 *    - WorldBackground: อิง worldBackgroundSpeed จาก GameManager (ความเร็วของฉากหลัง, พื้น, Parallax)
 *    - Custom: ใช้ความเร็ว customSpeed ที่กำหนดเองเฉพาะชิ้น
 * 3. ทำลายวัตถุอัตโนมัติเมื่อหลุดขอบจอด้านซ้าย (Auto Destroy)
 * 4. รองรับการวนลูปสำหรับแผ่นพื้น / แบคกราวด์ที่ไม่มีที่สิ้นสุด (Infinite Looping)
 * ==============================================================================
 */
public class ScrollLeft : MonoBehaviour
{
    public enum ScrollType
    {
        [Tooltip("สิ่งกีดขวาง กล่องสุ่ม หรือไอเทมบนทางวิ่ง (ดึง objectScrollSpeed จาก GameManager)")]
        GameplayObject,

        [Tooltip("ฉากหลัง แผ่นพื้น หรือสิ่งแวดล้อม (ดึง worldBackgroundSpeed จาก GameManager)")]
        WorldBackground,

        [Tooltip("กำหนดความเร็วอิสระเอง (ใช้ customSpeed)")]
        Custom
    }

    /* ==========================================================================
       SECTION 1: SPEED SETTINGS (การตั้งค่าความเร็วแยกประเภท)
       ========================================================================== */
    [Header("Speed Settings (แยกความเร็วฉากกับวัตถุ)")]
    [Tooltip("เลือกประเภทของวัตถุ เพื่อดึงความเร็วจาก GameManager ให้ถูกต้อง")]
    public ScrollType scrollType = ScrollType.GameplayObject;

    [Tooltip("ดึงความเร็วการเลื่อนจาก GameManager อัตโนมัติหรือไม่")]
    public bool useGameManagerSpeed = true;

    [Tooltip("ความเร็วแบบกำหนดเอง (จะถูกใช้หาก useGameManagerSpeed เป็น false หรือ scrollType เป็น Custom)")]
    public float customSpeed = 8f;

    [Tooltip("ตัวคูณความเร็ว (เช่น 1 = ปกติ, 0.6 = ฉากหลังเคลื่อนช้ากว่าแบบ Parallax, 1.2 = วิ่งเร็วขึ้น)")]
    public float speedMultiplier = 1f;

    /* ==========================================================================
       SECTION 2: BOUNDARIES & DESTROY
       ========================================================================== */
    [Header("Auto Destroy (ทำลายเมื่อหลุดจอ)")]
    [Tooltip("ลบวัตถุทิ้งอัตโนมัติเมื่อเลยขอบจอด้านซ้ายหรือไม่ (เหมาะกับ Obstacle / LetterBox)")]
    public bool autoDestroy = true;

    [Tooltip("ตำแหน่งแกน X ที่จะถูกทำลายทิ้ง")]
    public float destroyX = -30f;

    [Header("Infinite Looping (วนลูปสำหรับแผ่นพื้น/ฉากหลัง)")]
    [Tooltip("เปิดใช้งานให้วัตถุวาร์ปกลับไปด้านขวาเมื่อหลุดจอ (เหมาะกับชิ้นส่วนพื้นหรือฉาก)")]
    public bool isLooping = false;

    [Tooltip("ตำแหน่งแกน X ทางซ้ายที่แตะแล้วจะให้วาร์ปกลับ")]
    public float loopResetX = -30f;

    [Tooltip("ระยะทางที่จะวาร์ปกลับไปทางขวา (ปกติเท่ากับความกว้างของชิ้นฉากนั้นๆ)")]
    public float loopRespawnWidth = 60f;

    void Update()
    {
        // 1. คำนวณความเร็วตามประเภทวัตถุ
        float currentSpeed = customSpeed;

        if (useGameManagerSpeed && GameManager.Instance != null && scrollType != ScrollType.Custom)
        {
            if (scrollType == ScrollType.GameplayObject)
            {
                currentSpeed = GameManager.Instance.objectScrollSpeed;
            }
            else if (scrollType == ScrollType.WorldBackground)
            {
                currentSpeed = GameManager.Instance.worldBackgroundSpeed;
            }
        }

        // ตัวกรองความเร็ว Fallback ป้องกันการหยุดนิ่ง
        if (currentSpeed <= 0f)
        {
            currentSpeed = customSpeed > 0f ? customSpeed : 8f;
        }

        // 2. เลื่อนวัตถุไปทางซ้าย (แกน -X ใน World Space)
        transform.position += Vector3.left * (currentSpeed * speedMultiplier * Time.deltaTime);

        // 3. ตรวจสอบการวนลูป (สำหรับฉากหลัง / พื้น)
        if (isLooping)
        {
            if (transform.position.x <= loopResetX)
            {
                transform.position += new Vector3(loopRespawnWidth, 0f, 0f);
            }
        }
        // 4. ตรวจสอบการทำลายทิ้ง (สำหรับสิ่งกีดขวาง / เหรียญ / กล่องสุ่ม)
        else if (autoDestroy)
        {
            if (transform.position.x <= destroyX)
            {
                Destroy(gameObject);
            }
        }
    }
}
