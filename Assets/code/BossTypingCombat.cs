using UnityEngine;
using TMPro; // รองรับ TextMeshPro UI

/**
 * ==============================================================================
 * BOSS TYPING COMBAT (ระบบสะกดคำและโจมตีบอส)
 * ทำหน้าที่: รับข้อความที่ผู้เล่นพิมพ์ ตรวจสอบกับกระเป๋าตัวอักษร และสั่งโจมตีบอส
 * ==============================================================================
 */
public class BossTypingCombat : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: UI REFERENCES & SETUP (การเชื่อมต่อกับ UI Unity)
       ========================================================================== */
    [Header("References")]
    public LetterInventory inventory;
    public BossController currentBoss;

    [Header("UI Elements (TextMeshPro)")]
    public GameObject typingModalPanel;     // หน้าต่างพิมพ์คำศัพท์
    public TMP_Text targetWordText;          // แสดงคำศัพท์เป้าหมาย เช่น "LION"
    public TMP_Text meaningText;             // แสดงความหมาย เช่น "(สิงโต)"
    public TMP_InputField wordInputField;   // ช่องให้ผู้เล่นพิมพ์คำ
    public TMP_Text feedbackText;            // ข้อความแจ้งเตือนผลลัพธ์

    private string currentTargetWord = "";

    void Awake()
    {
        if (wordInputField != null)
        {
            wordInputField.onSubmit.AddListener(SubmitWord);
        }
    }

    void OnEnable()
    {
        if (currentBoss != null)
        {
            currentBoss.OnPromptWordToPlayer += OpenTypingUI;
        }
    }

    void OnDisable()
    {
        if (currentBoss != null)
        {
            currentBoss.OnPromptWordToPlayer -= OpenTypingUI;
        }
    }

    /* ==========================================================================
       SECTION 2: OPEN & CLOSE UI (เปิด/ปิดหน้าต่างพิมพ์คำศัพท์)
       ทำหน้าที่: แสดงหน้าต่างตอนบอสเข้าสู่สถานะเหนื่อย (Tired)
       ========================================================================== */
    public void OpenTypingUI(BossController.AnimalWordData wordData)
    {
        currentTargetWord = wordData.word.ToUpper();

        if (typingModalPanel != null) typingModalPanel.SetActive(true);
        if (targetWordText != null) targetWordText.text = currentTargetWord;
        if (meaningText != null) meaningText.text = $"({wordData.meaning})";
        if (feedbackText != null) feedbackText.text = "ใช้ตัวอักษรจากกระเป๋าของคุณสะกดคำ!";

        if (wordInputField != null)
        {
            wordInputField.text = "";
            wordInputField.ActivateInputField(); // โฟกัสช่องพิมพ์ทันที
        }
    }

    public void CloseTypingUI()
    {
        if (typingModalPanel != null) typingModalPanel.SetActive(false);
    }

    /* ==========================================================================
       SECTION 3: VALIDATION & ATTACK (ตรวจสอบตัวอักษรและการสร้างดาเมจ)
       ทำหน้าที่: ตรวจสอบคำตรงเป๊ะหรือไม่ และในกระเป๋ามีตัวอักษรพอหรือไม่
       ========================================================================== */
    public void SubmitWord(string input)
    {
        string typed = input.Trim().ToUpper();

        // 1. ตรวจสอบว่าคำที่พิมพ์ตรงกับคำศัพท์เป้าหมายหรือไม่
        if (typed != currentTargetWord)
        {
            ShowFeedback("❌ คำสะกดไม่ถูกต้อง!", Color.red);
            return;
        }

        // 2. ตรวจสอบว่าใน Inventory มีตัวอักษรเหล่านี้ครบหรือไม่
        if (!inventory.CanFormWord(typed))
        {
            ShowFeedback("⚠️ ตัวอักษรในกระเป๋าของคุณไม่เพียงพอ!", Color.yellow);
            return;
        }

        // 3. หักตัวอักษรออกจากกระเป๋า และทำดาเมจใส่บอส!
        inventory.ConsumeWord(typed);
        ShowFeedback("💥 โจมตีสำเร็จ!", Color.green);

        if (currentBoss != null)
        {
            currentBoss.TakeWordDamage();
        }

        CloseTypingUI();
    }

    private void ShowFeedback(string msg, Color col)
    {
        if (feedbackText != null)
        {
            feedbackText.text = msg;
            feedbackText.color = col;
        }
    }
}
