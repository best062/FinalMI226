using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * ==============================================================================
 * LETTER INVENTORY SYSTEM (ระบบกระเป๋าเก็บตัวอักษรภาษาอังกฤษ)
 * ทำหน้าที่: จัดเก็บตัวอักษรที่วิ่งเก็บได้ ตรวจสอบคำศัพท์ และตัดตัวอักษรเมื่อใช้โจมตีบอส
 * ==============================================================================
 */
public class LetterInventory : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: STORAGE & DATA (โครงสร้างข้อมูลคลังตัวอักษร)
       ทำหน้าที่: เก็บเป็น Key-Value Pair เช่น 'L' => 2, 'I' => 1, 'O' => 3, 'N' => 1
       ========================================================================== */
    private Dictionary<char, int> lettersBag = new Dictionary<char, int>();

    // Event แจ้งเตือน UI เมื่อจำนวนตัวอักษรเปลี่ยนแปลง
    public event Action<Dictionary<char, int>> OnInventoryChanged;

    /* ==========================================================================
       SECTION 2: COLLECTION LOGIC (การเก็บตัวอักษรเพิ่ม)
       ทำหน้าที่: รับตัวอักษรจากการชนเหรียญตัวอักษร แล้วบันทึกลงกระเป๋า
       ========================================================================== */
    public void AddLetter(char letter)
    {
        char upperChar = char.ToUpper(letter);
        if (lettersBag.ContainsKey(upperChar))
        {
            lettersBag[upperChar]++;
        }
        else
        {
            lettersBag[upperChar] = 1;
        }

        Debug.Log($"[Inventory] เก็บตัวอักษร: {upperChar} (มีทั้งหมด: {lettersBag[upperChar]})");
        OnInventoryChanged?.Invoke(new Dictionary<char, int>(lettersBag));
    }

    /* ==========================================================================
       SECTION 3: SPELLING VALIDATION (ตรวจสอบว่ามีตัวอักษรพอสะกดคำหรือไม่)
       ทำหน้าที่: เช็คว่าในกระเป๋ามีตัวอักษรครบตามที่คำศัพท์เป้าหมายของบอสต้องการหรือไม่
       ========================================================================== */
    public bool CanFormWord(string targetWord)
    {
        if (string.IsNullOrEmpty(targetWord)) return false;

        // นับจำนวนตัวอักษรที่คำศัพท์เป้าหมายต้องการ
        Dictionary<char, int> requiredLetters = new Dictionary<char, int>();
        foreach (char c in targetWord.ToUpper())
        {
            if (requiredLetters.ContainsKey(c)) requiredLetters[c]++;
            else requiredLetters[c] = 1;
        }

        // เปรียบเทียบกับของในกระเป๋า
        foreach (var pair in requiredLetters)
        {
            if (!lettersBag.ContainsKey(pair.Key) || lettersBag[pair.Key] < pair.Value)
            {
                return false; // ตัวอักษรไม่พอ
            }
        }

        return true;
    }

    /* ==========================================================================
       SECTION 4: CONSUME LETTERS (ตัดตัวอักษรออกจากกระเป๋าเพื่อทำดาเมจ)
       ทำหน้าที่: หักลบตัวอักษรที่ใช้ไปจริงเมื่อผู้เล่นพิมพ์สะกดคำสำเร็จ
       ========================================================================== */
    public bool ConsumeWord(string targetWord)
    {
        if (!CanFormWord(targetWord)) return false;

        foreach (char c in targetWord.ToUpper())
        {
            lettersBag[c]--;
            if (lettersBag[c] <= 0)
            {
                lettersBag.Remove(c);
            }
        }

        Debug.Log($"[Inventory] ใช้ตัวอักษรสะกดคำว่า '{targetWord}' เรียบร้อยแล้ว!");
        OnInventoryChanged?.Invoke(new Dictionary<char, int>(lettersBag));
        return true;
    }

    public void ResetInventory()
    {
        lettersBag.Clear();
        OnInventoryChanged?.Invoke(new Dictionary<char, int>(lettersBag));
    }
}
