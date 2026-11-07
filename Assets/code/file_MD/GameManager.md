# 🎯 GameManager.cs

## 📌 บทสรุปสั้น (Summary)
สคริปต์ตัวควบคุมหลัก (Core Game Loop Controller) ของเกม รูปแบบ **Singleton (`GameManager.Instance`)** ทำหน้าที่ควบคุมวัฏจักร 5 เฟสของแต่ละด่าน จัดการความเร็วการเลื่อนฉากและสิ่งกีดขวาง (`worldScrollSpeed`) ให้สัมพันธ์กับแต่ละด่านโดยอัตโนมัติ คำนวณระยะทางวิ่ง สลับเข้าสู่โหมดสู้บอสเมื่อถึงระยะทางที่กำหนด สปอว์นบอสประจำด่าน ตรวจจับเมื่อบอสตาย และเปลี่ยนผ่านผู้เล่นเข้าสู่ด่านถัดไป

---

## 🔄 Core Game Phases (5 เฟสของเกม)

GameManager ควบคุมโฟลว์เกมผ่าน Enum `GamePhase`:

| เฟส (GamePhase) | หน้าที่และการทำงาน |
| :--- | :--- |
| **`Runner`** | ช่วงวิ่งเก็บระยะทาง: คำนวณระยะทางจาก `distanceRun += worldScrollSpeed * Time.deltaTime` หากถึง `distanceToBoss` จะเปลี่ยนสถานะเข้าสู่การพบบอส |
| **`BossEncounter`** | ช่วงเปิดตัวบอส: ส่ง Event `OnBossEncounterStarted` สั่งให้ `SpawnManager` หยุดปล่อยสิ่งกีดขวาง และสปอว์นบอสจาก Prefab ประจำด่านมาที่ `bossSpawnPoint` |
| **`BossBattle`** | ช่วงต่อสู้บอส: ส่งต่อการควบคุมการต่อสู้ให้ `BossController` ดำเนินลูปบอส |
| **`BossDefeated`** | ช่วงบอสพ่ายแพ้: รับ Event `OnBossDefeated` จากบอส และหน่วงเวลารอเปลี่ยนด่าน |
| **`NextStageWait`** | ช่วงเปลี่ยนด่าน: เพิ่มดัชนีด่าน `currentStageIndex++` และเริ่มด่านใหม่ |

---

## ⚙️ คุณสมบัติหลัก (Key Features)

1. **Singleton Pattern (`GameManager.Instance`):**
   - มี Static Property `Instance` ที่เซ็ตใน `Awake()` เพื่อให้ `ScrollLeft` และ `SpawnManager` เข้าถึงค่าความเร็วและสถานะของเกมได้ทันที
2. **Speed Synchronization per Stage (ซิงค์ความเร็วแต่ละด่านอัตโนมัติ):**
   - ใน `StageProfile` สามารถกำหนด `stageSpeed` แยกแต่ละด่านได้ (เช่น ด่าน 1: 8, ด่าน 2: 10, ด่าน 3: 12)
   - เมื่อเริ่มด่านใหม่ ฟังก์ชัน `StartStage()` จะปรับ `worldScrollSpeed` ให้ตรงกับด่านนั้นทันที ทำให้ทุกวัตถุที่ใช้ `ScrollLeft` และ `SpawnManager` วิ่งเร็วขึ้นสอดคล้องกันทั้งฉากโดยไม่ต้องตั้งค่าทีละชิ้น
3. **Stage Vocabulary Data (คำศัพท์สัตว์ประจำด่าน):**
   - มีช่อง `stageWords` ในแต่ละด่าน บันทึกคำศัพท์เป้าหมาย เช่น `["LION", "TIGER"]` เพื่อส่งต่อให้ `SpawnManager` นำไปสุ่มตัวอักษรบนเส้นทางวิ่ง
4. **Stage Configuration (การตั้งค่าด่าน):**
   - โครงสร้างข้อมูล `StageProfile` ประกอบด้วย:
     - `stageNumber`: หมายเลขด่าน
     - `stageName`: ชื่อด่าน (เช่น `"Savanna: Lion"`)
     - `stageSpeed`: ความเร็วประจำด่าน
     - `distanceToBoss`: ระยะทางวิ่งก่อนเจอบอส
     - `bossPrefab`: Prefab ของบอส
     - `stageWords`: คำศัพท์สัตว์ประจำด่าน
5. **Stage Progression (การข้ามด่าน):**
   - เมื่อบอสตาย ฟังก์ชัน `OnBossKilled()` จะเรียก `OnStageCompleted` และส่งต่อไปยัง `ProceedToNextStage()` เพื่อเริ่มด่านใหม่

---

## 📋 ตัวแปรใน Inspector (Inspector Settings)

| ตัวแปร | ชนิดข้อมูล | ค่าเริ่มต้น | คำอธิบาย |
| :--- | :--- | :--- | :--- |
| `stages` | `List<StageProfile>` | - | รายการตั้งค่าด่านทั้งหมดในเกม (กำหนดความเร็ว, ระยะทาง, คำศัพท์, บอส) |
| `currentStageIndex` | `int` | `0` | ดัชนีด่านปัจจุบัน |
| `objectScrollSpeed` | `float` | `10.0` | ความเร็วในการเลื่อนของสิ่งกีดขวาง กล่องสุ่ม และไอเทม (Gameplay Objects) |
| `worldBackgroundSpeed` | `float` | `6.0` | ความเร็วในการเลื่อนของฉากหลัง แผ่นพื้น และสิ่งแวดล้อม (World/Parallax) |
| `worldScrollSpeed` | `float` | `10.0` | อ้างอิงความเร็วรวม (Backwards Compatibility) |
| `currentPhase` | `GamePhase` | `Runner` | สถานะเฟสปัจจุบันของเกม |
| `distanceRun` | `float` | `0` | ระยะทางสะสมที่วิ่งไปแล้วในด่านปัจจุบัน |
| `player` | `PlayerController` | - | อ้างอิงถึงตัวละครผู้เล่น |
| `inventory` | `LetterInventory` | - | อ้างอิงถึงกระเป๋าเก็บตัวอักษร |
| `bossSpawnPoint` | `Transform` | - | จุดพิกัด 3D ที่จะให้บอสสปอว์นลงมา |

---

## 📡 Events & Delegation

- `public event Action<int, StageProfile> OnStageStarted;` : แจ้งเตือนเมื่อเริ่มด่านใหม่และส่งข้อมูลด่าน
- `public event Action OnBossEncounterStarted;` : แจ้งเตือนเมื่อถึงระยะเจอบอส เพื่อให้ Spawner หยุดทำงาน
- `public event Action OnStageCompleted;` : แจ้งเตือนเมื่อผ่านด่านเรียบร้อย
- `public event Action<float, float> OnDistanceUpdated;` : แจ้งเตือนอัปเดตระยะทางแบบ Realtime `(currentDistance, targetDistanceToBoss)` สำหรับหน้าจอ UI (`DistanceUI`)

---

## 🔍 ฟังก์ชันสืบค้นระยะทาง (Distance Query Helpers)

- `GetCurrentDistanceToBoss()` : คืนค่าระยะทางเป้าหมายประจำด่านที่ต้องวิ่งไปให้ถึงก่อนเจอบอส (`distanceToBoss` เช่น 1500)
- `GetDistanceProgressNormalized()` : คืนค่าอัตราส่วนความคืบหน้า (0.0 ถึง 1.0) สำหรับนำไปใช้วาดแถบ Slider Progress Bar ของหน้าจอ UI แสดงระยะทาง
