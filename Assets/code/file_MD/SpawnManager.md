# 🎲 SpawnManager.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมระบบการสุ่มเสก (Spawn) **สิ่งกีดขวางแบ่ง 3 ส่วน: 1.กระโดด 1 ครั้ง, 2.กระโดด 2 ครั้ง, และ 3.สไลด์** พร้อมกับ **กล่องสุ่มตัวอักษร 3D (`LetterBox`)** จากจุดปล่อยทางด้านขวาของจอ ทำหน้าที่คำนวณความถี่ในการปล่อยให้สัมพันธ์กับความเร็วของแต่ละด่าน (`worldScrollSpeed`) อัตโนมัติ พร้อมระบบ **Procedural Prototype Generator** สร้างโมเดลจำลองอัตโนมัติหากยังไม่มี Prefab เพื่อให้ทดสอบเล่นได้ทันที และตัดการทำงานทันทีเมื่อพบบอส

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **ระบบเสกสิ่งกีดขวางแบ่ง 3 ส่วน (3-Part Obstacle Spawning):**
   - **`singleJumpObstaclePrefabs` (1. กระโดด 1 ครั้ง):** สิ่งกีดขวางระดับพื้นเตี้ย (ปล่อยที่ `groundSpawnPoint`) กระโดดครั้งเดียวพ้น
   - **`doubleJumpObstaclePrefabs` (2. กระโดด 2 ครั้ง):** สิ่งกีดขวางระดับพื้นที่มีความสูงหรือกว้างขึ้น ต้องกดกระโดดสองจังหวะ
   - **`slideObstaclePrefabs` (3. สไลด์ลอด):** สิ่งกีดขวางลอยสูงระดับศีรษะ (ปล่อยที่ `airSpawnPoint`) ต้องกดย่อตัวสไลด์ลอดใต้
2. **ระบบกล่องสุ่มตัวอักษร 3D (Generic Letter Box):**
   - เสก `letterBoxPrefab` สลับกับสิ่งกีดขวางตามน้ำหนัก `letterBoxWeight`
   - ทำงานร่วมกับ `LetterDropManager` เพื่อคำนวณ Smart Drop เบื้องหลัง
3. **Procedural Prototype Generator (สร้างของจำลองให้อัตโนมัติ):**
   - หากในลิสต์ Prefab ช่องใดยังว่างเปล่า ระบบจะสร้าง Primitive Mesh จำลองชั่วคราว (พร้อมใส่สีแยกระดับ, ใส่ Collider และผูก `Obstacle` + `ScrollLeft`) ให้อัตโนมัติทันทีที่กด Play
   - ทำให้นักพัฒนาสามารถทดสอบกลไกและเลเวลดีไซน์ได้ทันทีโดยไม่ต้องรอโมเดล 3D จากฝ่ายศิลป์
4. **Speed-Synchronized Spawning (ความถี่สัมพันธ์กับความเร็วด่าน):**
   - คำนวณดีเลย์ตามสูตร: `delay = Random.Range(minInterval, maxInterval) * (baseSpeed / currentSpeed)`
   - รักษาระยะห่างช่องว่างในการหลบหลีก (Distance Gap) ให้ยุติธรรม แม้ด่านจะวิ่งเร็วขึ้น
5. **Phase & Event Lifecycle Control:**
   - เริ่มปล่อยของเมื่อเข้าสู่เฟส Runner (`OnStageStarted`)
   - หยุดปล่อยอัตโนมัติเมื่อเจอบอส (`OnBossEncounterStarted`) หรือผู้เล่นตาย (`OnPlayerDied`)
   - มีฟังก์ชัน `SpawnLetterBoxManual()` ให้บอสหรือระบบภายนอกสั่งเสกกล่องสุ่มตัวอักษรเสริมได้

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `groundObstacleSpawnPoint` | `Transform` | - | จุดปล่อยสิ่งกีดขวางระดับพื้นดินทางขวา (สำหรับกระโดด 1 & 2 ครั้ง ที่ระดับ Y=0) |
| `groundLetterBoxSpawnPoint` | `Transform` | - | จุดปล่อยกล่องสุ่มระดับพื้น (ยกสูงขึ้น เช่น Y=0.7m เพื่อไม่ให้จมดินตอนขยับขึ้นลง) |
| `airSpawnPoint` | `Transform` | - | จุดปล่อยวัตถุลอยสูงทางขวา (สำหรับสไลด์ลอด และกล่องสุ่มลอยฟ้า) |
| `groundSpawnPoint` | `Transform` | - | จุดปล่อยสำรองเดิม (Backwards Compatibility) |
| `singleJumpObstaclePrefabs` | `List<GameObject>` | - | 1. รายการ Prefab สิ่งกีดขวางเตี้ย (กระโดด 1 ครั้ง) |
| `doubleJumpObstaclePrefabs` | `List<GameObject>` | - | 2. รายการ Prefab สิ่งกีดขวางสูง (กระโดด 2 ครั้ง) |
| `slideObstaclePrefabs` | `List<GameObject>` | - | 3. รายการ Prefab สิ่งกีดขวางลอยฟ้า (สไลด์ลอด) |
| `letterBoxPrefab` | `GameObject` | - | 4. Prefab กล่องสุ่มตัวอักษร 3D (`LetterBox`) |
| `singleJumpWeight` | `int` | `30` | น้ำหนักโอกาสเสกอุปสรรคกระโดด 1 ครั้ง (30%) |
| `doubleJumpWeight` | `int` | `20` | น้ำหนักโอกาสเสกอุปสรรคกระโดด 2 ครั้ง (20%) |
| `slideWeight` | `int` | `25` | น้ำหนักโอกาสเสกอุปสรรคสไลด์ (25%) |
| `letterBoxWeight` | `int` | `25` | น้ำหนักโอกาสเสกกล่องสุ่มตัวอักษร (25%) |
| `minSpawnInterval` | `float` | `1.8` | ช่วงเวลาต่ำสุดในการปล่อยวัตถุ (วินาที) |
| `maxSpawnInterval` | `float` | `3.2` | ช่วงเวลาสูงสุดในการปล่อยวัตถุ (วินาที) |
| `enableProceduralPrototypes`| `bool` | `true` | สร้างโมเดลจำลองอัตโนมัติหากลิสต์ Prefab ว่าง |

---

## 💡 แนวทางการติดตั้งใน Unity (Setup Guide)

1. สร้าง Empty GameObject ใน Scene ตั้งชื่อว่า `SpawnManager`
2. สร้างจุดปล่อย Transform ทางขวามือ:
   - `groundObstacleSpawnPoint` ที่ `(X = 25, Y = 0, Z = 0)`
   - `groundLetterBoxSpawnPoint` ที่ `(X = 25, Y = 0.7, Z = 0)` (ยกสูงพ้นพื้น เพื่อให้เอฟเฟกต์ลอยขึ้นลงไม่จมดิน)
   - `airSpawnPoint` ที่ `(X = 25, Y = 1.8, Z = 0)`
3. แยก Prefab สิ่งกีดขวางใส่ใน 3 หมวด:
   - ก้อนหินเตี้ย / พุ่มไม้ -> ใส่ใน **`Single Jump Obstacle Prefabs`**
   - เสาหินสูง / ลังซ้อน -> ใส่ใน **`Double Jump Obstacle Prefabs`**
   - ซุงลอย / ผลึกย้อย -> ใส่ใน **`Slide Obstacle Prefabs`**
4. ลาก `LetterBox_Prefab` ใส่ในช่อง `Letter Box Prefab`
5. แปะสคริปต์ `SpawnManager.cs` ระบบจะเริ่มสุ่มปล่อยวัตถุตามสัดส่วนที่ตั้งไว้ทันที
