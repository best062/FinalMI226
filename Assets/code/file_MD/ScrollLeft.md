# ⬅️ ScrollLeft.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมการเคลื่อนที่ของวัตถุจากขวาไปซ้าย (Right-to-Left Scrolling) สำหรับเกมสไตล์ Endless Runner ที่ตัวละครอยู่กับที่ ได้รับการอัปเกรดให้ **แยกประเภทความเร็วระหว่างสิ่งกีดขวาง/วัตถุในเกม (`GameplayObject`) กับฉากหลัง/แผ่นพื้น (`WorldBackground`) ออกจากกันอย่างอิสระ** ตามแนวคิดเกมต้นแบบอย่าง Cookie Run พร้อมระบบวนลูปแผ่นพื้น (Infinite Looping) และทำลายอัตโนมัติเมื่อหลุดจอ (Auto-Destroy)

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **การแยกความเร็วระหว่างวัตถุกับฉากหลัง (Scroll Type Separation):**
   - **`GameplayObject`:** ใช้สำหรับสิ่งกีดขวาง (Obstacles), กล่องสุ่มตัวอักษร (LetterBox), เหรียญ (Coins) -> ดึงความเร็ว `objectScrollSpeed` จาก `GameManager` เป็นหลัก ทำให้กำหนดความเร็วของเกมเพลย์และจังหวะการหลบได้อย่างเหมาะสม
   - **`WorldBackground`:** ใช้สำหรับฉากหลัง, ท้องฟ้า, แผ่นพื้น และวัตถุ Parallax -> ดึงความเร็ว `worldBackgroundSpeed` จาก `GameManager` ช่วยให้ฉากหลังเคลื่อนที่ช้ากว่าวัตถุหลัก เกิดมิติมุมมองที่สวยงามเหมือนเกมต้นแบบ
   - **`Custom`:** กำหนดความเร็วอิสระเฉพาะชิ้นโดยใช้ `customSpeed`
2. **ตัวคูณความเร็ว Parallax (`speedMultiplier`):**
   - ปรับสัดส่วนความเร็วเพิ่มเติมในแต่ละเลเยอร์ฉากหลัง (เช่น เลเยอร์ภูเขาไกลใช้ 0.3x, เลเยอร์ต้นไม้ใช้ 0.7x)
3. **Auto Destroy (ทำลายทิ้งเมื่อหลุดจอ):**
   - เมื่อสิ่งกีดขวางหรือกล่องสุ่มเลื่อนเลยผู้เล่นไปทางซ้าย (`transform.position.x <= destroyX`) จะทำลายทิ้งทันที
4. **Infinite Looping (วนลูปแผ่นพื้น/ฉากไม่สิ้นสุด):**
   - เมื่อเปิด `isLooping = true` หากชิ้นส่วนพื้นหรือฉากเคลื่อนไปถึง `loopResetX` ทางซ้าย จะวาร์ปกลับไปทางขวาตามระยะ `loopRespawnWidth`

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `scrollType` | `ScrollType` | `GameplayObject` | ประเภทวัตถุ (`GameplayObject`, `WorldBackground`, `Custom`) |
| `useGameManagerSpeed` | `bool` | `true` | ซิงค์ความเร็วจาก GameManager.Instance อัตโนมัติ |
| `customSpeed` | `float` | `8.0` | ความเร็วแบบกำหนดเอง (ใช้เมื่อ scrollType เป็น Custom) |
| `speedMultiplier` | `float` | `1.0` | ตัวคูณความเร็ว (ใช้ทำ Parallax Effect) |
| `autoDestroy` | `bool` | `true` | ทำลายวัตถุทิ้งเมื่อหลุดขอบจอซ้าย |
| `destroyX` | `float` | `-30.0` | พิกัดแกน X ด้านซ้ายที่จะสั่งทำลายวัตถุ |
| `isLooping` | `bool` | `false` | เปิดการวาร์ปวนลูปสำหรับแผ่นพื้น/ฉากหลัง |
| `loopResetX` | `float` | `-30.0` | พิกัดแกน X ทางซ้ายที่แตะแล้วจะวาร์ปกลับไปทางขวา |
| `loopRespawnWidth` | `float` | `60.0` | ระยะทางที่จะวาร์ปไปทางขวา |

---

## 💡 แนวทางการติดตั้งใน Unity (Setup Guide)

1. **สำหรับสิ่งกีดขวางและกล่องสุ่มตัวอักษร:**
   - เลือก `scrollType` เป็น **`GameplayObject`** (ค่าเริ่มต้น) เพื่อให้เคลื่อนที่ตามความเร็วเกมเพลย์หลัก
2. **สำหรับแผ่นพื้นและฉากหลัง (Background / Ground):**
   - เลือก `scrollType` เป็น **`WorldBackground`**
   - สำหรับแผ่นพื้น ให้ติ๊ก `isLooping = true`, ตั้ง `loopResetX = -30` และ `loopRespawnWidth = 60`
