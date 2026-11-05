using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * ==============================================================================
 * LETTER DROP MANAGER (ระบบ Smart Drop เบื้องหลัง)
 * ทำหน้าที่:
 * 1. คำนวณสุ่มตัวอักษรหลังบ้านเมื่อผู้เล่นชนกล่อง/ถุงวัตถุดิบ (Generic Letter Box)
 * 2. ป้องกันปัญหา Softlock (ไม่มีตัวอักษรไปตีบอส) ด้วย Weighted Rate:
 *    - Weighted Rate (70%): สุ่มดึงเฉพาะ "ตัวอักษรที่ผู้เล่นยังขาดอยู่" สำหรับสะกดคำตีบอส
 *    - Random Rate (30%): สุ่มตัวอักษรทั่วไปในเกรด A-Z เพิ่มความท้าทาย
 * 3. ส่งตัวอักษรเข้า LetterInventory ของผู้เล่น
 * 4. เสก Pop-up UI Floating Text ("+A") ลอยขึ้นพร้อมสีสันสะดุดตา
 * 5. เล่นเสียงหรือ Particle ตอนเปิดกล่อง
 * ==============================================================================
 */
public class LetterDropManager : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: SINGLETON & RATES (การตั้งค่าน้ำหนักการสุ่ม)
       ========================================================================== */
    public static LetterDropManager Instance { get; private set; }

    [Header("Smart Drop Rates (อัตราส่วนการสุ่มคำนวณเบื้องหลัง)")]
    [Tooltip("โอกาส (เปอร์เซ็นต์) ที่จะสุ่มเฉพาะ 'ตัวอักษรที่ผู้เล่นยังขาดอยู่' จากคำศัพท์บอส")]
    [Range(0f, 100f)]
    public float weightedRate = 70f; // 70% Weighted Rate

    [Tooltip("โอกาส (เปอร์เซ็นต์) ที่จะสุ่มตัวอักษรทั่วไป A-Z")]
    [Range(0f, 100f)]
    public float randomRate = 30f;   // 30% Random Rate

    [Header("Floating Text Settings (Pop-up UI)")]
    [Tooltip("Prefab สคริปต์ FloatingText สำหรับแสดงตัวอักษรที่ได้รับ (เช่น '+A')")]
    public GameObject floatingTextPrefab;

    [Tooltip("ระยะเยื้องความสูงเมื่อเสก Floating Text เหนือกล่อง")]
    public Vector3 floatingTextOffset = new Vector3(0f, 1.2f, 0f);

    [Tooltip("สีข้อความเมื่อสุ่มได้ตัวอักษรเป้าหมายของบอส (Smart Drop)")]
    public Color smartDropTextColor = new Color(1f, 0.84f, 0.0f); // สีทองสดใส

    [Tooltip("สีข้อความเมื่อสุ่มได้ตัวอักษรทั่วไป A-Z (Random Drop)")]
    public Color randomDropTextColor = Color.white;

    [Header("Audio & Visual Feedback")]
    [Tooltip("Particle Effect เมื่อเปิดกล่องสำเร็จ (เช่น ประกายดาวหรือควันระเบิด)")]
    public GameObject pickupParticlePrefab;

    [Tooltip("เสียงเอฟเฟกต์เมื่อเก็บกล่อง")]
    public AudioClip pickupSound;

    // Events สำหรับระบบภายนอกที่ต้องการฟังการดรอป
    public event Action<char, bool> OnLetterDropped; // (letter, isSmartDrop)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /* ==========================================================================
       SECTION 2: SMART DROP ALGORITHM (อัลกอริทึมคำนวณสุ่มตัวอักษรเบื้องหลัง)
       ========================================================================== */
    /**
     * คำนวณสุ่มตัวอักษรตามน้ำหนัก 70% ตัวที่ขาด / 30% สุ่ม A-Z
     * @param inventory กระเป๋าเก็บตัวอักษรของผู้เล่น
     * @param isSmartDrop out ส่งคืนว่าการสุ่มรอบนี้เข้าเงื่อนไข Smart Drop หรือไม่
     */
    public char CalculateDropLetter(LetterInventory inventory, out bool isSmartDrop)
    {
        isSmartDrop = false;

        // 1. ค้นหาคำศัพท์เป้าหมายประจำด่านหรือบอส
        List<string> targetWords = GetCurrentTargetWords();

        // 2. คำนวณหา "ตัวอักษรที่ผู้เล่นยังขาดอยู่" (Missing Letters)
        List<char> missingLetters = FindMissingLetters(targetWords, inventory);

        // 3. ทอยลูกเต๋าตามสัดส่วน Weighted Rate (70%) vs Random Rate (30%)
        float totalWeight = Mathf.Max(0.001f, weightedRate + randomRate);
        float roll = UnityEngine.Random.Range(0f, totalWeight);

        // ตรวจสอบว่าตกในส่วนของ Weighted Rate (70%) หรือไม่
        if (roll < weightedRate)
        {
            // ถ้ายังมีตัวอักษรที่ขาดอยู่ ให้สุ่มเลือก 1 ตัวจากตัวที่ขาด
            if (missingLetters.Count > 0)
            {
                isSmartDrop = true;
                int selectedIndex = UnityEngine.Random.Range(0, missingLetters.Count);
                return missingLetters[selectedIndex];
            }
            else if (targetWords.Count > 0)
            {
                // หากผู้เล่นมีตัวอักษรของคำศัพท์ครบแล้ว ให้สุ่มตัวในคำศัพท์เพื่อสะสมเพิ่ม
                string randomWord = targetWords[UnityEngine.Random.Range(0, targetWords.Count)];
                if (!string.IsNullOrEmpty(randomWord))
                {
                    isSmartDrop = true;
                    return char.ToUpper(randomWord[UnityEngine.Random.Range(0, randomWord.Length)]);
                }
            }
        }

        // 4. หากตกในส่วน Random Rate (30%) หรือไม่มีคำศัพท์เป้าหมาย ให้สุ่ม A-Z ทั่วไป
        return (char)UnityEngine.Random.Range('A', 'Z' + 1);
    }

    /* ==========================================================================
       SECTION 3: MISSING LETTERS CALCULATION (คำนวณรายการตัวอักษรที่ขาด)
       ========================================================================== */
    private List<char> FindMissingLetters(List<string> words, LetterInventory inventory)
    {
        List<char> missing = new List<char>();
        if (words == null || words.Count == 0) return missing;

        // นำคำศัพท์เป้าหมายคำแรก (หรือคำศัพท์ปัจจุบันของบอส) มาคำนวณตัวอักษรที่ต้องใช้
        string targetWord = words[0].ToUpper();
        Dictionary<char, int> requiredCounts = new Dictionary<char, int>();

        foreach (char c in targetWord)
        {
            if (char.IsLetter(c))
            {
                if (requiredCounts.ContainsKey(c)) requiredCounts[c]++;
                else requiredCounts[c] = 1;
            }
        }

        // เปรียบเทียบกับจำนวนที่ผู้เล่นมีในกระเป๋า
        foreach (var kvp in requiredCounts)
        {
            char neededChar = kvp.Key;
            int neededAmount = kvp.Value;
            int currentAmount = (inventory != null) ? inventory.GetLetterCount(neededChar) : 0;

            // ถ้ามีน้อยกว่าที่ต้องการ ให้ใส่ตัวอักษรที่ขาดลงไปตามจำนวนที่ยังขาดอยู่
            int deficit = neededAmount - currentAmount;
            for (int i = 0; i < deficit; i++)
            {
                missing.Add(neededChar);
            }
        }

        return missing;
    }

    private List<string> GetCurrentTargetWords()
    {
        List<string> words = new List<string>();

        // ตรวจสอบจากบอสที่กำลังสู้อยู่ในฉากก่อน (Priority สูงสุด)
        BossController activeBoss = FindObjectOfType<BossController>();
        if (activeBoss != null)
        {
            var bossWord = activeBoss.GetCurrentTargetWord();
            if (!string.IsNullOrEmpty(bossWord.word))
            {
                words.Add(bossWord.word.ToUpper());
                return words;
            }
        }

        // หากยังไม่พบบอส ให้ดึงจากคำศัพท์ประจำด่านของ GameManager
        if (GameManager.Instance != null)
        {
            var profile = GameManager.Instance.GetCurrentStageProfile();
            if (profile.stageWords != null && profile.stageWords.Count > 0)
            {
                foreach (string w in profile.stageWords)
                {
                    if (!string.IsNullOrEmpty(w)) words.Add(w.ToUpper());
                }
            }
        }

        return words;
    }

    /* ==========================================================================
       SECTION 4: WORKFLOW INTERACTION (โฟลว์การทำงานเมื่อผู้เล่นชนกล่อง)
       โฟลว์ตามที่ระบุ:
       1. ส่งตัวอักษรเข้า LetterInventory
       2. เล่น Pop-up UI Floating Text ("+A")
       3. ทำลาย/ปิดการใช้งาน กล่อง
       ========================================================================== */
    public void ProcessLetterBoxPickup(LetterBox letterBox, PlayerController player)
    {
        if (letterBox == null || player == null) return;

        // ดึงคอมโพเนนต์ LetterInventory จากตัวผู้เล่น หรือจาก GameManager
        LetterInventory inventory = player.GetComponent<LetterInventory>();
        if (inventory == null && GameManager.Instance != null)
        {
            inventory = GameManager.Instance.inventory;
        }

        // คำนวณสุ่มตัวอักษรเบื้องหลัง
        bool isSmartDrop;
        char droppedLetter = CalculateDropLetter(inventory, out isSmartDrop);

        // 1. ส่งตัวอักษรเข้า LetterInventory
        if (inventory != null)
        {
            inventory.AddLetter(droppedLetter);
        }
        else
        {
            Debug.LogWarning("[LetterDropManager] ไม่พบคอมโพเนนต์ LetterInventory บนตัว Player!");
        }

        // 2. เสก Pop-up UI Floating Text ("+A")
        SpawnFloatingText(letterBox.transform.position, droppedLetter, isSmartDrop);

        // เล่น Particle และเสียงเอฟเฟกต์ (ถ้ามี)
        PlayFeedbackEffects(letterBox.transform.position);

        // แจ้งเตือน Event
        OnLetterDropped?.Invoke(droppedLetter, isSmartDrop);

        Debug.Log($"[LetterDropManager] ผู้เล่นเปิดกล่องได้รับ: '{droppedLetter}' (Smart Drop: {isSmartDrop})");

        // 3. ทำลายกล่อง
        Destroy(letterBox.gameObject);
    }

    private void SpawnFloatingText(Vector3 boxPosition, char letter, bool isSmartDrop)
    {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPos = boxPosition + floatingTextOffset;
        GameObject textObj = Instantiate(floatingTextPrefab, spawnPos, Quaternion.identity);

        FloatingText floatText = textObj.GetComponent<FloatingText>();
        if (floatText != null)
        {
            string displayMessage = $"+{letter}";
            Color textColor = isSmartDrop ? smartDropTextColor : randomDropTextColor;
            floatText.SetText(displayMessage, textColor);
        }
    }

    private void PlayFeedbackEffects(Vector3 position)
    {
        // สร้าง Particle Effect
        if (pickupParticlePrefab != null)
        {
            Instantiate(pickupParticlePrefab, position, Quaternion.identity);
        }

        // เล่นเสียง
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, position);
        }
    }
}

