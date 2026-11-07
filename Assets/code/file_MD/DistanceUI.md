# 📏 DistanceUI.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมการแสดงผล **ระยะทางของผู้เล่น (Distance Display UI)** บนหน้าจอ HUD เชื่อมต่อกับ `GameManager` แบบ Realtime เพื่อให้ผู้เล่นทราบว่าวิ่งมาไกลแค่ไหนแล้ว และเหลือระยะทางอีกเท่าใดจะถึงจุดปะทะบอสประจำด่าน (`distanceToBoss`) รองรับทั้ง TextMeshPro และ UI Text เดิม พร้อมรองรับหลอดความคืบหน้า (Progress Slider)

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **เชื่อมโยงกับ GameManager โดยตรง (Event & Polling Sync):**
   - ดักฟัง Event `OnDistanceUpdated(distanceRun, distanceToBoss)` จาก `GameManager.Instance`
   - มีระบบ Polling Fallback ใน `Update()` ช่วยการันตีว่าค่าจะอัปเดตต่อเนื่อง นุ่มนวล ไม่มีอาการหลุดซิงค์
2. **โหมดการแสดงผล 3 รูปแบบ (Display Modes):**
   - `CurrentOnly`: แสดงเฉพาะระยะทางปัจจุบัน เช่น `Distance : 0` หรือ `Distance : 350` (ตรงตามหน้าจอ UI ที่ผู้ใช้ตั้งไว้)
   - `CurrentAndTarget`: แสดงระยะทางปัจจุบันเทียบกับระยะทางเจอบอส เช่น `Distance : 350 / 1500m`
   - `RemainingToBoss`: แสดงระยะทางนับถอยหลังที่เหลือก่อนถึงบอส เช่น `Boss in : 1150m`
3. **รองรับ UI หลากหลายรูปแบบ:**
   - ใช้งานได้ทั้ง **TextMeshProUGUI (`TMP_Text`)** และ **Legacy UI Text (`Text`)**
   - รองรับ **Slider Progress Bar** แสดงแถบเปอร์เซ็นต์ความคืบหน้า (0.0 ถึง 1.0) สู่จุดเจอบอส
4. **Customizable Formatting:**
   - ปรับเปลี่ยนคำนำหน้า (`prefix`) เช่น `"Distance : "` และคำต่อท้ายหน่วย (`suffix`) เช่น `"m"` ได้อย่างอิสระผ่าน Inspector
   - มีตัวเลือกปัดเศษทศนิยมเป็นจำนวนเต็ม (`roundToInt = true`)

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `distanceTMP` | `TMP_Text` | - | คอมโพเนนต์ TextMeshPro แสดงข้อความระยะทาง (จะค้นหาอัตโนมัติ) |
| `legacyDistanceText` | `Text` | - | Unity UI Text เดิม (กรณีไม่ได้ใช้ TextMeshPro) |
| `distanceProgressBar` | `Slider` | - | หลอดแถบเกจระยะทางวิ่งมุ่งสู่บอส (Optional) |
| `displayMode` | `DisplayMode` | `CurrentOnly` | โหมดแสดงผล (`CurrentOnly`, `CurrentAndTarget`, `RemainingToBoss`) |
| `prefix` | `string` | `"Distance : "` | คำนำหน้าข้อความ |
| `suffix` | `string` | `""` | คำต่อท้ายหน่วย เช่น `"m"` |
| `roundToInt` | `bool` | `true` | ปัดเศษทศนิยมเป็นจำนวนเต็มหรือไม่ |

---

## 💡 แนวทางการติดตั้งใน Unity (Setup Guide)

1. เลือก Text บน Canvas ที่เขียนว่า `Distance : 0` (หรือสร้าง Text - TextMeshPro ขึ้นมาใหม่)
2. แปะสคริปต์ `DistanceUI.cs` เข้าไปที่ GameObject นั้น
3. ตัวสคริปต์จะค้นหาคอมโพเนนต์ Text ให้อัตโนมัติ และจะดึงค่า `distanceRun` จาก `GameManager` มาแสดงผลทันทีเมื่อกด Play
4. (ตัวเลือกเสริม) หากมีแถบ Slider แสดงความคืบหน้า สามารถลากมาใส่ในช่อง `Distance Progress Bar` ได้

