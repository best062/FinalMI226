using UnityEngine;

/**
 * ==============================================================================
 * SCROLL LEFT (ระบบเลื่อนวัตถุจากขวาไปซ้าย)
 * ทำหน้าที่:
 * 1. เลื่อนวัตถุ (ฉากหลัง, พื้น, สิ่งกีดขวาง, ตัวอักษร, กระสุน) จากขวาไปซ้าย (-X)
 * 2. ทำลายวัตถุอัตโนมัติเมื่อหลุดขอบจอด้านซ้าย (Auto Destroy)
 * 3. รองรับการวนลูปสำหรับแผ่นพื้น / แบคกราวด์ที่ไม่มีที่สิ้นสุด (Infinite Looping)
 * 4. รองรับ Parallax Scrolling (ปรับ Speed Multiplier ให้ฉากหลังขยับช้ากว่าพื้นหน้า)
 * ==============================================================================
 */
public class ScrollLeft : MonoBehaviour
{
    [Header("Speed Settings")]
    [Tooltip("ดึงความเร็วการเลื่อนจาก GameManager อัตโนมัติหรือไม่")]
    public bool useGameManagerSpeed = true;

    [Tooltip("ความเร็วแบบกำหนดเอง (จะถูกใช้หาก useGameManagerSpeed เป็น false หรือไม่มี GameManager ในฉาก)")]
    public float customSpeed = 8f;

    [Tooltip("ตัวคูณความเร็ว (เช่น 1 = ปกติ, 0.5 = ฉากหลังเคลื่อนช้ากว่าแบบ Parallax, 1.5 = วิ่งเร็วขึ้น)")]
    public float speedMultiplier = 1f;

    [Header("Auto Destroy (ทำลายเมื่อหลุดจอ)")]
    [Tooltip("ลบวัตถุทิ้งอัตโนมัติเมื่อเลยขอบจอด้านซ้ายหรือไม่ (เหมาะกับ Obstacle / Letter)")]
    public bool autoDestroy = true;

    [Tooltip("ตำแหน่งแกน X ที่จะถูกทำลายทิ้ง")]
    public float destroyX = -20f;

    [Header("Infinite Looping (วนลูปสำหรับแผ่นพื้น/ฉากหลัง)")]
    [Tooltip("เปิดใช้งานให้วัตถุวาร์ปกลับไปด้านขวาเมื่อหลุดจอ (เหมาะกับชิ้นส่วนพื้นหรือฉาก)")]
    public bool isLooping = false;

    [Tooltip("ตำแหน่งแกน X ทางซ้ายที่แตะแล้วจะให้วาร์ปกลับ")]
    public float loopResetX = -30f;

    [Tooltip("ระยะทางที่จะวาร์ปกลับไปทางขวา (ปกติเท่ากับความกว้างของชิ้นฉากนั้นๆ)")]
    public float loopRespawnWidth = 60f;

    void Update()
    {
        // 1. คำนวณความเร็ว
        float currentSpeed = customSpeed;
        if (useGameManagerSpeed && GameManager.Instance != null)
        {
            currentSpeed = GameManager.Instance.worldScrollSpeed;
        }

        // 2. เลื่อนวัตถุไปทางซ้าย (แกน -X)
        transform.position += Vector3.left * (currentSpeed * speedMultiplier * Time.deltaTime);

        // 3. ตรวจสอบการวนลูป (สำหรับฉากหลัง / พื้น)
        if (isLooping)
        {
            if (transform.position.x <= loopResetX)
            {
                transform.position += new Vector3(loopRespawnWidth, 0f, 0f);
            }
        }
        // 4. ตรวจสอบการทำลายทิ้ง (สำหรับสิ่งกีดขวาง / เหรียญ)
        else if (autoDestroy)
        {
            if (transform.position.x <= destroyX)
            {
                Destroy(gameObject);
            }
        }
    }
}
