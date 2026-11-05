# 🎲 SpawnManager.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ควบคุมระบบการสุ่มสร้าง (Spawn) สิ่งกีดขวาง (Low & High Obstacles) และเหรียญตัวอักษร 3D (Collectible Letters) จากจุดปล่อยทางด้านขวาของจอ ทำหน้าที่คำนวณความถี่ในการปล่อยให้สัมพันธ์กับความเร็วของแต่ละด่าน (`worldScrollSpeed`) อัตโนมัติ พร้อมระบบสุ่มตัวอักษรอย่างชาญฉลาด (Smart Letter Generation) อิงตามคำศัพท์สัตว์ประจำด่าน และตัดการทำงานทันทีเมื่อเข้าสู่การพบบอส (Boss Encounter)

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **Speed-Synchronized Spawning (ความถี่สัมพันธ์กับความเร็วด่าน):**
   - ความถี่ในการปล่อยจะปรับตัวตามสูตร `delay = Random.Range(minInterval, maxInterval) * (baseSpeed / currentSpeed)`
   - เมื่อผู้เล่นผ่านด่านและเกมเร็วขึ้น (เช่น 8 -> 10 -> 12) ระบบจะเร่งความถี่ในการปล่อยให้สมดุล ทำให้ระยะห่าง (Distance Gap) ระหว่างสิ่งกีดขวางมีความสม่ำเสมอ ยุติธรรม และกระโดดหลบได้ ไม่กองรวมกัน
2. **Designated Spawn Points (ปล่อยจากจุดที่กำหนด):**
   - `groundSpawnPoint`: จุดระดับพื้นดิน สำหรับสิ่งกีดขวางต่ำ (Low) และตัวอักษรบนพื้น
   - `airSpawnPoint`: จุดลอยสูง สำหรับสิ่งกีดขวางระดับสูง (High) และตัวอักษรระดับกระโดด
3. **Weighted Random System (สุ่มตามอัตราส่วน):**
   - สามารถปรับเปอร์เซ็นต์โอกาสการเกิดของ Low Obstacle, High Obstacle, และตัวอักษรได้อย่างอิสระผ่าน Inspector
4. **Smart Letter Picking (สุ่มตัวอักษรจากคำศัพท์เป้าหมาย):**
   - ตัวอักษรที่สุ่มออกมาจะดึงมาจาก `stageWords` ใน `StageProfile` ของ `GameManager` เป็นหลัก (เช่น โอกาส 80% ที่จะสุ่มตัวอักษรจากคำว่า "LION") เพื่อการันตีว่าผู้เล่นจะได้ตัวอักษรเพียงพอในการสะกดคำเพื่อต่อสู้กับบอส
5. **Phase & Event Lifecycle Control:**
   - เริ่มปล่อยของเมื่อได้รับ Event `OnStageStarted`
   - หยุดปล่อยของอัตโนมัติเมื่อพบบอส (`OnBossEncounterStarted`) หรือเมื่อผู้เล่นเลือดหมด (`OnPlayerDied`)

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `groundSpawnPoint` | `Transform` | - | จุดปล่อยวัตถุระดับพื้นดินทางขวา |
| `airSpawnPoint` | `Transform` | - | จุดปล่อยวัตถุลอยสูงทางขวา |
| `lowObstaclePrefabs` | `List<GameObject>` | - | รายการ Prefab สิ่งกีดขวางต่ำ (ก้อนหิน, หนาม) |
| `highObstaclePrefabs` | `List<GameObject>` | - | รายการ Prefab สิ่งกีดขวางสูง (ท่อนไม้ลอย) |
| `letterBoxPrefab` | `GameObject` | - | Prefab กล่องสุ่มตัวอักษร 3D (`LetterBox`) ระบบหลักแนะนำเพื่อแก้ปัญหา Asset |
| `letterPrefab` | `GameObject` | - | Prefab เหรียญ/ตัวอักษร 3D รายตัวแบบเดิม (Fallback) |
| `minSpawnInterval` | `float` | `1.8` | ช่วงเวลาต่ำสุดในการปล่อยวัตถุ (วินาที) |
| `maxSpawnInterval` | `float` | `3.2` | ช่วงเวลาสูงสุดในการปล่อยวัตถุ (วินาที) |
| `baseReferenceSpeed` | `float` | `8.0` | ความเร็วอ้างอิงฐานสำหรับคำนวณสัดส่วนเวลา |
| `lowObstacleWeight` | `int` | `40` | น้ำหนักโอกาสเกิดสิ่งกีดขวางต่ำ (40%) |
| `highObstacleWeight` | `int` | `30` | น้ำหนักโอกาสเกิดสิ่งกีดขวางสูง (30%) |
| `letterWeight` | `int` | `30` | น้ำหนักโอกาสเกิดเหรียญตัวอักษร (30%) |
| `targetWordLetterChance` | `float` | `0.8` | โอกาสสุ่มได้ตัวอักษรที่ตรงกับคำศัพท์ประจำด่าน |

---

## 💡 แนวทางการติดตั้งใน Unity (Setup Guide)

1. สร้าง Empty GameObject ใน Scene ตั้งชื่อว่า `SpawnManager`
2. สร้างจุดปล่อย 2 จุดทางขวามือของจอ (เช่น `X = 25, Y = 0` และ `X = 25, Y = 1.8`) แล้วลากมาใส่ใน `groundSpawnPoint` และ `airSpawnPoint`
3. ลาก Prefabs สิ่งกีดขวางและตัวอักษรใส่ในช่องรายการ
4. แปะสคริปต์ `SpawnManager.cs` ระบบจะซิงค์การทำงานกับ `GameManager` อัตโนมัติ ไม่ต้องตั้งค่าทีละชิ้นในฉาก
