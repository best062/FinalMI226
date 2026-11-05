# 🎁 LetterDropManager.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ระบบจัดการการสุ่มตัวอักษรเบื้องหลัง (Smart Drop System) ประจำเกม ทำงานร่วมกับ `LetterBox` เพื่อแก้ปัญหาการขาดตัวอักษรสำหรับสะกดคำสู้บอส (Softlock) โดยคำนวณเบื้องหลังว่าผู้เล่นมีตัวอักษรอะไรแล้วบ้าง และยังขาดตัวอักษรใดสำหรับคำศัพท์ของบอสประจำด่าน จากนั้นแบ่งน้ำหนักการสุ่มออกเป็น **Weighted Rate (70%)** สำหรับดึงตัวอักษรที่ยังขาด และ **Random Rate (30%)** สำหรับสุ่ม A–Z ทั่วไป พร้อมทั้งจัดส่งตัวอักษรเข้ากระเป๋าและเรียกเล่นเอฟเฟกต์ Pop-up Floating Text

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **Smart Drop System (อัลกอริทึมแก้ปัญหา Softlock แนบเนียน):**
   - ผู้เล่นมองเห็นเพียงกล่องสุ่มหรือถุงวัตถุดิบลอยมา แต่ระบบเบื้องหลังจะคำนวณการันตีตัวอักษรที่จำเป็น
   - **Weighted Rate (เช่น 70%):** สุ่มดึงเฉพาะ **"ตัวอักษรที่ผู้เล่นยังขาดอยู่"** (`missingLetters`) สำหรับคำศัพท์บอสประจำด่านนั้นๆ
   - **Random Rate (เช่น 30%):** สุ่มตัวอักษรทั่วไป A–Z เพื่อเพิ่มความท้าทาย ไม่ให้เกมง่ายจนเกินไป
2. **Missing Letters Analyzer (ระบบวิเคราะห์ตัวอักษรที่ขาด):**
   - ตรวจสอบคำศัพท์ปัจจุบันของบอสจาก `BossController` (หรือ `stageWords` ใน `GameManager`)
   - เปรียบเทียบตัวอักษรที่ต้องใช้ กับจำนวนตัวอักษรที่มีจริงในกระเป๋า `LetterInventory` ของผู้เล่น
   - คำนวณเป็นรายการตัวอักษรที่ยังขาดจริง (รองรับคำที่มีตัวอักษรซ้ำ เช่น "APPLE" ต้องมี 'P' 2 ตัว)
3. **โฟลว์การทำงานแบบครบวงจร (3-Step Pickup Workflow):**
   - **Step 1:** ส่งตัวอักษรที่คำนวณได้เข้าสู่ `LetterInventory.AddLetter(letter)`
   - **Step 2:** เสก Pop-up UI Floating Text (`"+A"`) ลอยขึ้นเหนือกล่อง พร้อมแยกสี (สีทองสำหรับตัวอักษรคำศัพท์, สีขาวสำหรับตัวอักษรสุ่ม)
   - **Step 3:** เล่น Particle Effect / เสียงประกอบ และทำลายกล่องออกจากฉาก
4. **Singleton Access & Event Notification:**
   - เข้าถึงได้ง่ายผ่าน `LetterDropManager.Instance`
   - มี Event `OnLetterDropped` สำหรับระบบ UI HUD หรือระบบภายนอกนำไปใช้งานต่อ

---

## 🔄 แผนผังการทำงาน (Workflow Diagram)

```
[SpawnManager] ──เสก──> [LetterBox Prefab] (เลื่อนซ้ายตามฉากด้วย ScrollLeft)
                              │
                    (ผู้เล่นวิ่งชน Trigger)
                              │
                              ▼
                [LetterDropManager (Smart Drop)]
                              │
                    ┌─────────┴─────────┐
                    ▼                   ▼
          Weighted Rate (70%)    Random Rate (30%)
          [ดึงตัวอักษรที่ยังขาด]     [สุ่มตัวอักษรทั่วไป A-Z]
                    │                   │
                    └─────────┬─────────┘
                              ▼
         ├── 1. ส่งตัวอักษรเข้า LetterInventory
         ├── 2. เล่น Pop-up UI Floating Text ("+A")
         └── 3. ทำลาย/ปิดการใช้งาน กล่อง
```

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `weightedRate` | `float` | `70.0` | อัตราส่วนน้ำหนักการสุ่มตัวอักษรที่ยังขาดอยู่ (เช่น 70%) |
| `randomRate` | `float` | `30.0` | อัตราส่วนน้ำหนักการสุ่มตัวอักษรทั่วไป A–Z (เช่น 30%) |
| `floatingTextPrefab` | `GameObject` | - | Prefab ข้อความลอย (`FloatingText`) สำหรับแสดงตัวอักษรที่ได้รับ |
| `floatingTextOffset` | `Vector3` | `(0, 1.2, 0)` | ตำแหน่งเยื้องความสูงที่เสกข้อความลอยขึ้นเหนือกล่อง |
| `smartDropTextColor` | `Color` | สีทอง (`#FFD600`) | สีของข้อความเมื่อได้รับตัวอักษรตรงกับคำศัพท์บอส |
| `randomDropTextColor` | `Color` | สีขาว (`#FFFFFF`) | สีของข้อความเมื่อได้รับตัวอักษรทั่วไป A–Z |
| `pickupParticlePrefab` | `GameObject` | - | เอฟเฟกต์อนุภาคตอนเก็บกล่องสำเร็จ (Optional) |
| `pickupSound` | `AudioClip` | - | เสียงเอฟเฟกต์ตอนเก็บกล่องสำเร็จ (Optional) |

---

## 💡 แนวทางการติดตั้งใน Unity (Setup Guide)

1. สร้าง Empty GameObject ใน Scene ตั้งชื่อว่า `LetterDropManager`
2. แปะสคริปต์ `LetterDropManager.cs`
3. ลาก Prefab `FloatingText_Prefab` ใส่ในช่อง `Floating Text Prefab`
4. ปรับค่า `weightedRate` (70) และ `randomRate` (30) ตามความยากง่ายที่ต้องการ
5. (ถ้ามี) ใส่ Particle และ AudioClip เสียงเปิดกล่องในช่อง Feedback

