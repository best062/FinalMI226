# ⌨️ BossTypingCombat.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมระบบการต่อสู้ด้วยการพิมพ์สะกดคำ (Typing Combat Modal UI) ทำหน้าที่เปิดหน้าต่างรับข้อความที่ผู้เล่นพิมพ์ ตรวจสอบว่าคำตรงกับคำศัพท์สัตว์เป้าหมายหรือไม่ ตรวจสอบว่าในกระเป๋า (`LetterInventory`) มีตัวอักษรภาษาอังกฤษเพียงพอหรือไม่ และสั่งหักตัวอักษรเพื่อสร้างดาเมจใส่บอส (`BossController`)

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **การเชื่อมต่อ Event อัตโนมัติ (Event Subscription):**
   - ใน `OnEnable()` และ `OnDisable()` จะทำการ Subscribe เข้ากับ Event `OnPromptWordToPlayer` ของ `BossController`
   - เมื่อบอสเข้าสู่ช่วงเหนื่อย (Tired State) สคริปต์นี้จะถูกเรียกให้เปิด UI พิมพ์คำศัพท์อัตโนมัติ
2. **การจัดการหน้าต่าง UI (Open & Close Modal):**
   - ฟังก์ชัน `OpenTypingUI(wordData)`:
     - เปิดการแสดงผลหน้าต่าง Pop-up (`typingModalPanel.SetActive(true)`)
     - แสดงคำศัพท์ภาษาอังกฤษบน `targetWordText` เช่น `"LION"`
     - แสดงคำแปลภาษาไทยบน `meaningText` เช่น `"(สิงโต)"`
     - เคลียร์ข้อความเก่าและเรียก `wordInputField.ActivateInputField()` เพื่อให้ผู้เล่นพิมพ์แป้นพิมพ์ได้ทันทีโดยไม่ต้องเอามือไปคลิกเมาส์
3. **ขั้นตอนการตรวจสอบคำศัพท์ (Validation Logic):**
   - ฟังก์ชัน `SubmitWord(string input)` จะถูกเรียกเมื่อผู้เล่นกด `Enter`:
     - **ขั้นที่ 1:** นำข้อความที่พิมพ์มาตัดช่องว่างและแปลงเป็นตัวพิมพ์ใหญ่ (`input.Trim().ToUpper()`) แล้วเทียบกับคำศัพท์เป้าหมาย หากไม่ตรง จะแจ้งเตือน `"❌ คำสะกดไม่ถูกต้อง!"` (สีแดง)
     - **ขั้นที่ 2:** เรียก `inventory.CanFormWord(typed)` เพื่อตรวจสอบว่าในกระเป๋ามีตัวอักษรภาษาอังกฤษที่ต้องใช้ครบหรือไม่ หากไม่พอ จะแจ้งเตือน `"⚠️ ตัวอักษรในกระเป๋าของคุณไม่เพียงพอ!"` (สีเหลือง)
     - **ขั้นที่ 3:** หากผ่านทั้ง 2 ขั้นตอน จะเรียก `inventory.ConsumeWord(typed)` เพื่อตัดตัวอักษรออก เรียก `currentBoss.TakeWordDamage()` เพื่อหักเลือดบอส และปิดหน้าต่าง UI ทันที

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | คำอธิบาย |
| :--- | :--- | :--- |
| `inventory` | `LetterInventory` | อ้างอิงถึงกระเป๋าเก็บตัวอักษรของผู้เล่น |
| `currentBoss` | `BossController` | อ้างอิงถึงตัวบอสที่กำลังต่อสู้อยู่ในฉาก |
| `typingModalPanel` | `GameObject` | หน้าต่าง UI Modal ที่จะเปิด/ปิดตอนพิมพ์คำศัพท์ |
| `targetWordText` | `TMP_Text` | ข้อความ TextMeshPro สำหรับแสดงคำศัพท์เป้าหมาย เช่น LION |
| `meaningText` | `TMP_Text` | ข้อความ TextMeshPro สำหรับแสดงความหมายภาษาไทย |
| `wordInputField` | `TMP_InputField` | ช่อง InputField ให้ผู้เล่นพิมพ์ตัวอักษร |
| `feedbackText` | `TMP_Text` | ข้อความแจ้งผลลัพธ์ (เช่น ถูกต้อง, ตัวอักษรไม่พอ, สะกดผิด) |

---

## 💡 แนวทางการติดตั้งใน Canvas UI (Setup Guide)
1. สร้าง Canvas UI ใน Unity โดยมี Panel สำหรับป๊อปอัพพิมพ์คำศัพท์
2. ใส่คอมโพเนนต์ `TextMeshPro - Input Field` เข้าไปใน Panel
3. ลากคอมโพเนนต์ Text ต่างๆ และ Input Field มาใส่ใน Inspector ของ `BossTypingCombat`
