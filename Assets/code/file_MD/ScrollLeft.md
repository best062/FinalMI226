# ⬅️ ScrollLeft.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมการเคลื่อนที่ของวัตถุจากขวาไปซ้าย (Right-to-Left Scrolling) สำหรับเกมสไตล์ Endless Runner ที่ตัวละครอยู่กับที่ ทำหน้าที่ขับเคลื่อนฉากหลัง แผ่นพื้น สิ่งกีดขวาง เหรียญตัวอักษร และกระสุนเข้าหาตัวผู้เล่น พร้อมระบบทำลายอัตโนมัติเมื่อหลุดจอ (Auto-Destroy) และระบบวาร์ปวนลูปแผ่นพื้นไม่สิ้นสุด (Infinite Looping)

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **Right-to-Left Movement (เคลื่อนที่จากขวาไปซ้าย):**
   - เลื่อน Transform ไปทางแกน -X ในฟังก์ชัน `Update()`:
     `transform.position += Vector3.left * (currentSpeed * speedMultiplier * Time.deltaTime);`
2. **GameManager Synchronization (ซิงค์ความเร็วกับระบบเกม):**
   - มีตัวเลือก `useGameManagerSpeed`: ดึงค่า `worldScrollSpeed` จาก `GameManager.Instance` โดยอัตโนมัติ ทำให้เมื่อเกมปรับความเร็ว ทุกชิ้นส่วนจะเคลื่อนที่สอดคล้องกันทันที
   - หากไม่มี GameManager ในฉาก หรือปิดการซิงค์ จะใช้ค่า `customSpeed` แทน
3. **Parallax Scrolling (การแบ่งชั้นความเร็วฉาก):**
   - มีตัวแปร `speedMultiplier` สำหรับปรับสัดส่วนความเร็ว (เช่น ฉากหลังไกลใช้ 0.3x เพื่อให้เคลื่อนช้า, วัตถุด้านหน้าใช้ 1.0x)
4. **Auto Destroy (ทำลายทิ้งเมื่อหลุดจอ):**
   - สำหรับสิ่งกีดขวาง (Obstacle) และเหรียญตัวอักษร (CollectibleLetter) เมื่อลอยเลยตัวผู้เล่นและหลุดขอบจอด้านซ้าย (`transform.position.x <= destroyX`) จะเรียก `Destroy(gameObject)` ทันทีเพื่อประหยัดหน่วยความจำ
5. **Infinite Looping (วนลูปแผ่นพื้น/ฉากไม่สิ้นสุด):**
   - สำหรับแผ่นพื้นหรือฉากหลัง เมื่อเปิด `isLooping = true` หากชิ้นส่วนเคลื่อนไปถึง `loopResetX` ทางซ้าย จะวาร์ปบวกตำแหน่งแกน X ไปข้างหน้าเท่ากับ `loopRespawnWidth` ทันที ทำให้ต่อฉากได้อย่างต่อเนื่องไร้รอยต่อ

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `useGameManagerSpeed` | `bool` | `true` | ดึงค่าความเร็วจาก GameManager.Instance อัตโนมัติ |
| `customSpeed` | `float` | `8.0` | ความเร็วแบบกำหนดเอง (ใช้เมื่อไม่ซิงค์กับ GameManager) |
| `speedMultiplier` | `float` | `1.0` | ตัวคูณความเร็ว (ใช้ทำ Parallax Effect สำหรับฉากหลัง) |
| `autoDestroy` | `bool` | `true` | ทำลายวัตถุทิ้งเมื่อหลุดขอบจอซ้าย |
| `destroyX` | `float` | `-20.0` | พิกัดแกน X ด้านซ้ายที่จะสั่งทำลายวัตถุ |
| `isLooping` | `bool` | `false` | เปิดการวาร์ปวนลูป (เหมาะกับชิ้นส่วนแผ่นพื้น/ฉาก) |
| `loopResetX` | `float` | `-30.0` | พิกัดแกน X ทางซ้ายที่แตะแล้วจะวาร์ปกลับไปทางขวา |
| `loopRespawnWidth` | `float` | `60.0` | ระยะทางที่จะวาร์ปไปทางขวา (ปกติเท่ากับความยาวรวมของชิ้นพื้น) |

---

## 💡 แนวทางการติดตั้งใน Unity (Setup Guide)

1. **สำหรับสิ่งกีดขวางและเหรียญ:**
   - สคริปต์ `Obstacle.cs` และ `CollectibleLetter.cs` มีคำสั่ง `[RequireComponent(typeof(ScrollLeft))]` อยู่แล้ว เมื่อแปะสคริปต์เหล่านั้น Unity จะเพิ่ม `ScrollLeft` ให้อัตโนมัติ โดยตั้งค่า `autoDestroy = true`
2. **สำหรับแผ่นพื้นไม่สิ้นสุด (Infinite Ground):**
   - นำแผ่นพื้น 2 ชิ้นวางต่อกัน (เช่น ชิ้น A อยู่ที่ X=0, ชิ้น B อยู่ที่ X=30 ยาวชิ้นละ 30 เมตร)
   - แปะสคริปต์ `ScrollLeft` ให้แผ่นพื้นทั้งคู่
   - ติ๊กถูก `isLooping = true`, ตั้ง `loopResetX = -30` และ `loopRespawnWidth = 60`
