# 💬 FloatingText.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมข้อความ Pop-up ลอยบนโลกเกม 3D (World Space Floating Text) ทำหน้าที่แสดงผลตัวอักษรที่ผู้เล่นสุ่มได้รับจากกล่อง เช่น `"+A"`, `"+L"` ให้ผู้เล่นรับรู้ได้อย่างชัดเจนทันที โดยมีอนิเมชันลอยขึ้นช้าๆ (Float Up), เด้งขยายขนาดเล็กน้อย (Pop Scale), และค่อยๆ จางหายไป (Fade Out) ก่อนทำลายตัวเองอัตโนมัติ

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **รองรับ TextMeshPro (TMP_Text):**
   - รองรับทั้ง TextMeshPro 3D (ใน World Space) และ TextMeshProUGUI (บน Screen Canvas)
   - ค้นหาคอมโพเนนต์ `TMP_Text` ให้อัตโนมัติจากตัวเองหรือโหนดลูก
2. **Smooth Float & Pop Animation:**
   - ลอยขึ้นด้านบนตามแกน Y ด้วยความเร็ว `floatSpeed`
   - มีช่วงแอนิเมชัน Pop Scale เล็กน้อยตอนเสก เพื่อให้ความรู้สึกเด้ง นุ่มนวล และมีชีวิตชีวา
3. **Smooth Alpha Fade Out:**
   - ค่อยๆ ลดค่าความโปร่งใส (Alpha) ตามช่วงเวลา `fadeStartNormalized` จนโปร่งใส 100%
   - ไม่ทำให้ตัวหนังสือกระพริบหรือหายไปแบบกะทันหัน
4. **Auto-Cleanup (ทำลายตัวเองอัตโนมัติ):**
   - เมื่อครบกำหนด `lifeTime` (เช่น 1.2 วินาที) สคริปต์จะเรียก `Destroy(gameObject)` ทันที ป้องกันปัญหาขยะสะสมใน Memory

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `textComponent` | `TMP_Text` | - | คอมโพเนนต์ TextMeshPro แสดงข้อความ (จะหาให้อัตโนมัติหากไม่ลากใส่) |
| `floatSpeed` | `float` | `1.8` | ความเร็วในการลอยขึ้นตามแกน Y ในฉาก |
| `lifeTime` | `float` | `1.2` | อายุขัยรวมของข้อความก่อนจะถูกทำลายทิ้ง (วินาที) |
| `fadeStartNormalized` | `float` | `0.4` | จุดเริ่มค่อยๆ จางหาย (0.4 = เริ่มเฟดเมื่อผ่านไป 40% ของเวลา) |
| `popScaleMultiplier` | `float` | `1.3` | ตัวคูณขนาดเริ่มต้นตอนเสกขึ้นมา ก่อนลดลงสู่ขนาดปกติ (Pop Scale) |

---

## 💡 แนวทางการติดตั้งใน Unity (Setup Guide)

1. สร้าง 3D Text - TextMeshPro ใน Scene ตั้งชื่อว่า `FloatingText_Prefab`
2. ปรับขนาดตัวอักษรให้อ่านง่าย ปรับ Alignment เป็นกึ่งกลาง (Center & Middle)
3. แปะสคริปต์ `FloatingText.cs`
4. ลาก GameObject นี้ไปใส่ในโฟลเดอร์ Prefabs แล้วลบชิ้นใน Scene ออก
5. นำ Prefab ที่ได้ไปใส่ในช่อง `floatingTextPrefab` ของ `LetterDropManager`

