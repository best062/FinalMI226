# 🏃‍♂️ PlayerController.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมตัวละครหลักสไตล์ **Cookie Run** สำหรับ Unity 3D ในรูปแบบ **วิ่งอยู่กับที่ (Stationary Runner)** โดยให้ฉากและสิ่งกีดขวางเลื่อนเข้าหาตัวละคร รับผิดชอบการกระโดด 2 จังหวะ (Double Jump), การกดย่อตัวสไลด์แบบกดค้างแนบพื้น (Hold-to-Duck), การตรวจจับพื้นโดยตรงจากหน้าสัมผัส (Direct Ground Collision), ระบบพลังชีวิต 3 หัวใจ และระบบป้องกันดาเมจซ้ำชั่วขณะ (I-Frames)

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **Stationary Runner (วิ่งอยู่กับที่บนแกน X):**
   - ล็อคพิกัดแกน X ด้วย `RigidbodyConstraints.FreezePositionX` และกำหนด `rb.velocity = new Vector3(0f, yVel, 0f)` เพื่อให้ตัวละครวิ่งอยู่กับที่อย่างมั่นคง ไม่ไถลหรือโดนสิ่งกีดขวางผลัก
2. **Jump Control (ระบบกระโดด 2 ชั้น):**
   - รองรับปุ่ม `Space`, `W`, หรือ `UpArrow`
   - กระโดดได้สูงสุดตาม `maxJumps` (ค่าเริ่มต้น 2 ครั้ง) โดยจะรีเซ็ตสิทธิ์การกระโดดก็ต่อเมื่อเท้าแตะพื้นจริงเท่านั้น
3. **Hold-to-Duck / Slide (ย่อตัวตามการกดค้าง เท้าแนบพื้น 100%):**
   - รองรับปุ่ม `S`, `DownArrow`, หรือ `LeftControl`
   - เมื่อกดค้าง: คำนวณจุดศูนย์กลาง Collider และโมเดลใหม่ตามสัดส่วนความสูง เพื่อให้เท้าของตัวละครแนบชิดติดพื้นเสมอ ไม่ลอยกลางอากาศ
   - เมื่อปล่อยปุ่ม: คืนค่าขนาด Collider และสัดส่วนโมเดลกลับสู่ความสูงปกติทันที
4. **Direct Ground Collision (ตรวจจับการสัมผัสพื้นโดยตรง):**
   - ใช้ `OnCollisionEnter` และ `OnCollisionStay` ตรวจสอบเวกเตอร์แรงสะท้อน (`contact.normal.y > 0.3f`) ป้องกันปัญหาบั๊ก Raycast พลาดหรือไม่แตะพื้น
5. **Health & Invincibility (ระบบ 3 หัวใจและอมตะชั่วคราว):**
   - มีหัวใจเริ่มต้น 3 ดวง (`maxHearts = 3`)
   - เมื่อชนสิ่งกีดขวาง `TakeDamage(1)` จะลดหัวใจ 1 ดวง พร้อมเปิดสถานะอมตะชั่วขณะ (`invincibilityDuration = 1.5s`) และกระพริบตัวละคร
   - หากหัวใจหมดจะเรียก `Die()` ส่งสัญญาณ Event `OnPlayerDied`
6. **Automatic Visual Hierarchy Setup:**
   - หากไม่มีการกำหนดโมเดลลูกไว้ใน Inspector ระบบจะแยก `MeshFilter` และ `MeshRenderer` ไปไว้ใน GameObject ลูกอัตโนมัติ เพื่อให้ย่อขนาดได้โดยไม่กระทบจุดหมุนและฟิสิกส์ของ Rigidbody

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `runSpeed` | `float` | `8.0` | อ้างอิงความเร็วในการวิ่ง (ตัวละครวิ่งอยู่กับที่) |
| `jumpForce` | `float` | `12.0` | แรงกระโดดขึ้นในแนวแกน Y |
| `maxJumps` | `int` | `2` | จำนวนครั้งที่กระโดดได้สูงสุดก่อนแตะพื้น |
| `normalHeight` | `float` | `2.0` | ความสูงปกติของตัวละครและ Collider |
| `slideHeight` | `float` | `0.9` | ความสูงขณะกดย่อตัวสไลด์ |
| `maxHearts` | `int` | `3` | จำนวนหัวใจสูงสุด |
| `currentHearts` | `int` | `3` | จำนวนหัวใจปัจจุบัน |
| `invincibilityDuration` | `float` | `1.5` | ระยะเวลาอมตะหลังได้รับดาเมจ (วินาที) |
| `peoplePlaceholderModel` | `GameObject` | - | โมเดลตัวละครที่จะทำการย่อขนาด |

---

## 📡 Events & Delegation

- `public event Action<int> OnHealthChanged;` : ส่งค่าหัวใจปัจจุบันออกไปเมื่อมีการเปลี่ยนแปลง เพื่อให้อัปเดต UI หัวใจ
- `public event Action OnPlayerDied;` : แจ้งเตือนเมื่อตัวละครเลือดหมด เพื่อแสดงหน้า Game Over
