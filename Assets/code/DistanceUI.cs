using UnityEngine;
using UnityEngine.UI;
using TMPro;

/**
 * ==============================================================================
 * DISTANCE UI CONTROLLER (ระบบแสดงผลระยะทางบนหน้าจอ)
 * ทำหน้าที่:
 * 1. ดึงข้อมูลระยะทาง (distanceRun) และระยะทางสู่บอส (distanceToBoss) จาก GameManager
 * 2. อัปเดตข้อความบน UI Text / TextMeshPro ให้ผู้เล่นทราบระยะทางปัจจุบันแบบ Realtime
 * 3. รองรับรูปแบบการแสดงผล:
 *    - CurrentOnly: "Distance : 0" (ตามภาพ UI ที่ผู้เล่นตั้งไว้)
 *    - CurrentAndTarget: "Distance : 120 / 1500m"
 *    - RemainingToBoss: "Boss in : 1380m"
 * 4. รองรับหลอดเกจแถบความคืบหน้า (Progress Bar Slider) ไปยังบอส
 * ==============================================================================
 */
public class DistanceUI : MonoBehaviour
{
    public enum DisplayMode
    {
        CurrentOnly,       // แสดงเฉพาะระยะทางปัจจุบัน เช่น "Distance : 250"
        CurrentAndTarget,  // แสดงระยะปัจจุบันพร้อมเป้าหมาย เช่น "Distance : 250 / 1500"
        RemainingToBoss    // แสดงระยะทางที่เหลือจนกว่าจะเจอบอส เช่น "Boss in : 1250m"
    }

    /* ==========================================================================
       SECTION 1: UI REFERENCES & SETTINGS
       ========================================================================== */
    [Header("UI Text Components (รองรับทั้ง TextMeshPro และ UI Text เดิม)")]
    [Tooltip("TextMeshPro UGUI สำหรับแสดงข้อความระยะทาง (แนะนำ)")]
    public TMP_Text distanceTMP;

    [Tooltip("Unity UI Text แบบดั้งเดิม (กรณีไม่ได้ใช้ TextMeshPro)")]
    public Text legacyDistanceText;

    [Header("Optional Progress Bar")]
    [Tooltip("Slider แถบความคืบหน้าการวิ่งสู่จุดเจอบอส (0.0 ถึง 1.0)")]
    public Slider distanceProgressBar;

    [Header("Format Settings")]
    [Tooltip("โหมดการแสดงผลข้อความ")]
    public DisplayMode displayMode = DisplayMode.CurrentOnly;

    [Tooltip("คำนำหน้าข้อความ (ค่าเริ่มต้นคือ 'Distance : ')")]
    public string prefix = "Distance : ";

    [Tooltip("คำต่อท้ายหน่วย เช่น 'm' หรือปล่อยว่างไว้")]
    public string suffix = "";

    [Tooltip("ต้องการปัดเศษทศนิยมเป็นจำนวนเต็มหรือไม่")]
    public bool roundToInt = true;

    void Awake()
    {
        // ค้นหา TextMeshPro อัตโนมัติหากยังไม่ได้ลากใส่ใน Inspector
        if (distanceTMP == null)
        {
            distanceTMP = GetComponent<TMP_Text>();
            if (distanceTMP == null)
            {
                distanceTMP = GetComponentInChildren<TMP_Text>();
            }
        }

        // ค้นหา UI Text เดิม
        if (distanceTMP == null && legacyDistanceText == null)
        {
            legacyDistanceText = GetComponent<Text>();
            if (legacyDistanceText == null)
            {
                legacyDistanceText = GetComponentInChildren<Text>();
            }
        }
    }

    void Start()
    {
        // ผูก Event ฟังการอัปเดตระยะทางจาก GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDistanceUpdated += UpdateDistanceDisplay;
        }

        // อัปเดตแสดงผลเริ่มต้น
        RefreshDisplay();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDistanceUpdated -= UpdateDistanceDisplay;
        }
    }

    void Update()
    {
        // Polling fallback เพื่อความชัวร์ในกรณีที่ Event ไม่ถูกทริกเกอร์ หรืออยู่ในสถานะพิเศษ
        if (GameManager.Instance != null && GameManager.Instance.currentPhase == GameManager.GamePhase.Runner)
        {
            RefreshDisplay();
        }
    }

    /* ==========================================================================
       SECTION 2: DISPLAY LOGIC
       ========================================================================== */
    public void RefreshDisplay()
    {
        if (GameManager.Instance == null) return;

        float currentDist = GameManager.Instance.distanceRun;
        float targetDist = GameManager.Instance.GetCurrentDistanceToBoss();
        UpdateDistanceDisplay(currentDist, targetDist);
    }

    private void UpdateDistanceDisplay(float currentDistance, float targetDistance)
    {
        string formattedText = BuildDisplayText(currentDistance, targetDistance);

        // อัปเดต TextMeshPro
        if (distanceTMP != null)
        {
            distanceTMP.text = formattedText;
        }

        // อัปเดต Legacy Text
        if (legacyDistanceText != null)
        {
            legacyDistanceText.text = formattedText;
        }

        // อัปเดตหลอด Progress Slider
        if (distanceProgressBar != null && targetDistance > 0.001f)
        {
            distanceProgressBar.value = Mathf.Clamp01(currentDistance / targetDistance);
        }
    }

    private string BuildDisplayText(float current, float target)
    {
        switch (displayMode)
        {
            case DisplayMode.CurrentOnly:
                return roundToInt 
                    ? $"{prefix}{Mathf.FloorToInt(current)}{suffix}" 
                    : $"{prefix}{current:F1}{suffix}";

            case DisplayMode.CurrentAndTarget:
                return roundToInt
                    ? $"{prefix}{Mathf.FloorToInt(current)} / {Mathf.FloorToInt(target)}{suffix}"
                    : $"{prefix}{current:F1} / {target:F1}{suffix}";

            case DisplayMode.RemainingToBoss:
                float remaining = Mathf.Max(0f, target - current);
                return roundToInt
                    ? $"Boss in : {Mathf.FloorToInt(remaining)}{suffix}"
                    : $"Boss in : {remaining:F1}{suffix}";

            default:
                return $"{prefix}{Mathf.FloorToInt(current)}";
        }
    }
}

