using System.Collections;
using UnityEngine;
using TMPro;

/**
 * ==============================================================================
 * FLOATING TEXT (ระบบข้อความ Pop-up ลอยและจางหาย)
 * ทำหน้าที่:
 * 1. แสดงตัวอักษรที่สุ่มได้จากการเปิดกล่อง เช่น "+A", "+B"
 * 2. ลอยขึ้นด้านบนช้าๆ อย่างนุ่มนวล พร้อมแอนิเมชันขยายขนาดเล็กน้อย (Pop scale)
 * 3. ค่อยๆ จางหายไป (Fade Out) และลบตัวเองออกจากหน่วยความจำอัตโนมัติ
 * ==============================================================================
 */
public class FloatingText : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: SETTINGS & REFERENCES (การตั้งค่าและการอ้างอิง)
       ========================================================================== */
    [Header("Component References")]
    [Tooltip("TextMeshPro สำหรับแสดงข้อความ (รองรับทั้ง 3D TMP และ UI TMP)")]
    public TMP_Text textComponent;

    [Header("Motion & Lifetime")]
    [Tooltip("ความเร็วในการลอยขึ้นตามแกน Y")]
    public float floatSpeed = 1.8f;

    [Tooltip("ระยะเวลาการแสดงผลก่อนที่ข้อความจะหายไปทั้งหมด (วินาที)")]
    public float lifeTime = 1.2f;

    [Tooltip("ช่วงเวลาที่เริ่มค่อยๆ เฟดจางลง (เป็นเปอร์เซ็นต์ของ lifeTime เช่น 0.5 = เริ่มเฟดครึ่งเวลาหลัง)")]
    [Range(0f, 1f)]
    public float fadeStartNormalized = 0.4f;

    [Header("Visual Effects")]
    [Tooltip("ขนาดขยายเริ่มต้นก่อนลดกลับมาขนาดปกติเพื่อความเด้ง (Pop Effect)")]
    public float popScaleMultiplier = 1.3f;

    private Color initialColor = Color.white;
    private Vector3 initialScale;
    private float elapsedTime = 0f;

    void Awake()
    {
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TMP_Text>();
        }

        if (textComponent != null)
        {
            initialColor = textComponent.color;
        }

        initialScale = transform.localScale;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        // 1. เลื่อนตำแหน่งลอยขึ้นตามแกน Y ใน World Space
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // 2. แอนิเมชัน Pop Scale เล็กน้อยตอนเปิดตัว
        float progress = Mathf.Clamp01(elapsedTime / lifeTime);
        if (progress < 0.2f)
        {
            float popFactor = Mathf.Lerp(popScaleMultiplier, 1.0f, progress / 0.2f);
            transform.localScale = initialScale * popFactor;
        }
        else
        {
            transform.localScale = initialScale;
        }

        // 3. จัดการการค่อยๆ จางหาย (Fade Out Alpha)
        if (progress >= fadeStartNormalized && textComponent != null)
        {
            float fadeProgress = (progress - fadeStartNormalized) / (1.0f - fadeStartNormalized);
            float alpha = Mathf.Lerp(initialColor.a, 0f, fadeProgress);
            Color newColor = initialColor;
            newColor.a = alpha;
            textComponent.color = newColor;
        }

        // 4. เมื่อหมดอายุขัย ให้ทำลายวัตถุอัตโนมัติ
        if (elapsedTime >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    /* ==========================================================================
       SECTION 2: PUBLIC INTERACTION (ฟังก์ชันกำหนดข้อความและสี)
       ========================================================================== */
    public void SetText(string message, Color? customColor = null)
    {
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<TMP_Text>();
        }

        if (textComponent != null)
        {
            textComponent.text = message;

            if (customColor.HasValue)
            {
                initialColor = customColor.Value;
                textComponent.color = initialColor;
            }
        }
    }
}

