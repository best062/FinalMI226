# 📖 Unity Scripts Documentation (สรุปภาพรวมโค้ดทั้งหมด)

เอกสารสรุปสถาปัตยกรรมและการทำงานของสคริปต์ C# ทั้งหมดในโปรเจกต์เกมแนว **Cookie Run Runner + Boss Animal Typing Combat**

---

## 🎮 1. ภาพรวมของระบบเกม (System Overview)

เกมนี้เป็นการผสมผสานระหว่าง **Stationary Endless Runner (สไตล์ Cookie Run ที่ตัวละครวิ่งอยู่กับที่ โดยฉากเลื่อนเข้าหาตัวละคร)** และ **เกมสะกดคำต่อสู้บอสสัตว์ (Animal Vocabulary Typing Combat)** โดยแบ่งเป็น 2 ช่วงหลักในแต่ละด่าน:
1. **ช่วงวิ่ง (Runner Phase):** 
   - ตัวละครผู้เล่นอยู่กับที่บนแกน X ฉาก แผ่นพื้น สิ่งกีดขวาง และเหรียญตัวอักษรจะเลื่อนจากขวาไปซ้ายด้วย `ScrollLeft`
   - **`SpawnManager`** จะสุ่มปล่อยสิ่งกีดขวาง (Low/High) และเหรียญตัวอักษร 3D ออกมาจากจุดที่กำหนดทางขวา โดยปรับความถี่ในการปล่อยให้สัมพันธ์กับความเร็วของด่าน (`worldScrollSpeed`) อัตโนมัติ
   - ผู้เล่นต้องกระโดด 2 จังหวะข้ามสิ่งกีดขวางต่ำ หรือกดย่อตัวสไลด์แนบพื้นลอดสิ่งกีดขวางสูง พร้อมทั้งเก็บ **เหรียญ/โมเดลตัวอักษร 3D (Collectible Letters)** ที่สุ่มมาจากคำศัพท์เป้าหมายเข้ากระเป๋า (Letter Inventory)
2. **ช่วงสู้บอส (Boss Battle Phase):** 
   - เมื่อระยะทางที่ฉากเลื่อนวิ่งถึงที่กำหนด `SpawnManager` จะหยุดปล่อยสิ่งกีดขวาง และบอสประจำด่านจะสปอว์นลงมา
   - ผู้เล่นต้องหลบการโจมตีของบอส รอจังหวะบอสเหนื่อย แล้วใช้ตัวอักษรภาษาอังกฤษที่มีในกระเป๋ามาพิมพ์สะกดคำศัพท์ภาษาอังกฤษผ่านหน้าต่าง UI เพื่อโจมตีบอสจนเลือดหมด แล้วผ่านไปด่านถัดไปที่มีความเร็วสูงขึ้น

---

## 🔄 2. Core Game Loop Diagram

```mermaid
graph TD
    A["1. Runner Phase<br>(ตัวละครอยู่กับที่ + Spawner สุ่มปล่อยของ + ฉากเลื่อนสัมพันธ์ความเร็ว)"] -->|วิ่งถึงระยะ distanceToBoss| B["2. Boss Encounter<br>(Spawner หยุดปล่อยของ + บอส Spawn เข้าสู่ฉาก)"]
    B --> C["3. Boss Battle<br>(บอสตี -> ปล่อยลูกน้อง -> บอสเหนื่อย -> ผู้เล่นสะกดคำตี)"]
    C -->|เลือดบอสลดเหลือ 0| D["4. Boss Defeated<br>(บอสพ่ายแพ้ เล่นเอฟเฟกต์)"]
    D --> E["5. Next Stage<br>(เปลี่ยนด่านถัดไป ปรับ Speed โลกใหม่ รีเซ็ตระยะทาง)"]
    E --> A
```

---

## 📂 3. รายชื่อสคริปต์และสรุปหน้าที่ (Script Index)

| ชื่อสคริปต์ (Script Name) | เอกสารสรุป (.md) | หน้าที่หลัก (Core Responsibility) |
| :--- | :--- | :--- |
| **`PlayerController.cs`** | [`PlayerController.md`](PlayerController.md) | วิ่งอยู่กับที่ (X=0), กระโดด 2 ชั้น (Double Jump), กดย่อตัวสไลด์แนบพื้น (Hold-to-Duck), ระบบเลือด 3 หัวใจ และอมตะชั่วขณะ (I-Frames) |
| **`ScrollLeft.cs`** | [`ScrollLeft.md`](ScrollLeft.md) | ขับเคลื่อนวัตถุ (ฉาก, พื้น, สิ่งกีดขวาง, ตัวอักษร) จากขวาไปซ้าย, ทำลายอัตโนมัติเมื่อพ้นจอ, และวาร์ปวนลูปแผ่นพื้นไม่สิ้นสุด (Infinite Looping) |
| **`SpawnManager.cs`** | [`SpawnManager.md`](SpawnManager.md) | สุ่มปล่อยสิ่งกีดขวางและตัวอักษร 3D จากจุดปล่อยทางขวา ปรับความถี่สัมพันธ์กับความเร็วด่าน และสุ่มตัวอักษรจากคำศัพท์เป้าหมาย |
| **`Obstacle.cs`** | [`Obstacle.md`](Obstacle.md) | จัดการสิ่งกีดขวาง แบ่งประเภทเป็น Low (กระโดดข้าม) และ High (สไลด์มุดลอด) เลื่อนด้วย `ScrollLeft` และลดเลือดผู้เล่นเมื่อชน |
| **`CollectibleLetter.cs`** | [`CollectibleLetter.md`](CollectibleLetter.md) | จัดการเหรียญและโมเดลตัวอักษร 3D หมุนและลอยขึ้นลง (Bobbing Effect) เลื่อนด้วย `ScrollLeft` และส่งตัวอักษรเข้ากระเป๋าเมื่อชน |
| **`LetterInventory.cs`** | [`LetterInventory.md`](LetterInventory.md) | คลังจัดเก็บตัวอักษร (Dictionary) ตรวจสอบว่ามีตัวอักษรพอสะกดคำศัพท์เป้าหมายหรือไม่ และหักตัวอักษรออกเมื่อโจมตีบอส |
| **`BossController.cs`** | [`BossController.md`](BossController.md) | จัดการลูปบอส 5 ขั้นตอน (เจอบอส -> บอสตี -> ปล่อยลูกน้อง -> บอสเหนื่อย -> เราตี), หลอดเลือดบอส และคำศัพท์ประจำด่าน |
| **`BossTypingCombat.cs`** | [`BossTypingCombat.md`](BossTypingCombat.md) | ควบคุมหน้าต่าง UI สำหรับพิมพ์คำศัพท์ (TextMeshPro InputField), ตรวจสอบความถูกต้อง และส่งคำสั่งโจมตีบอส |
| **`GameManager.cs`** | [`GameManager.md`](GameManager.md) | ควบคุมเกมรวมแบบ Singleton, จัดการความเร็วของแต่ละด่าน (`stageSpeed`), คำนวณระยะทาง, สปอว์นบอส และโหลดด่านถัดไป |

---

## 🔗 4. ความสัมพันธ์ระหว่างสคริปต์ (Component Relationships)

```mermaid
classDiagram
    class GameManager {
        +static GameManager Instance
        +float worldScrollSpeed
        +GamePhase currentPhase
        +float distanceRun
        +StartStage()
        +TransitionToBossEncounter()
    }
    class SpawnManager {
        +Transform groundSpawnPoint
        +Transform airSpawnPoint
        +StartSpawning()
        +StopSpawning()
        +SpawnRandomObject()
    }
    class PlayerController {
        +int currentHearts
        +Jump()
        +StartSlide()
        +StopSlide()
        +TakeDamage()
    }
    class ScrollLeft {
        +float customSpeed
        +bool isLooping
        +bool autoDestroy
    }
    class LetterInventory {
        +AddLetter(char)
        +CanFormWord(string)
        +ConsumeWord(string)
    }
    class Obstacle {
        +ObstacleType obstacleType
        +OnTriggerEnter()
    }
    class CollectibleLetter {
        +char letter
        +Transform visualModel
        +OnTriggerEnter()
    }
    class BossController {
        +BossState currentState
        +TakeWordDamage()
        +BossLoopCoroutine()
    }
    class BossTypingCombat {
        +OpenTypingUI()
        +SubmitWord()
    }

    GameManager --> PlayerController : ตรวจสอบสถานะผู้เล่น
    GameManager --> BossController : Spawns & ฟัง Event บอสตาย
    GameManager --> SpawnManager : ส่ง Event OnStageStarted / OnBossEncounter
    ScrollLeft ..> GameManager : ดึงค่า worldScrollSpeed
    SpawnManager ..> GameManager : ดึงความเร็วและคำศัพท์ประจำด่าน
    SpawnManager ..> Obstacle : สร้าง Prefab สิ่งกีดขวาง
    SpawnManager ..> CollectibleLetter : สร้าง Prefab ตัวอักษร
    Obstacle --|> ScrollLeft : ผูกคอมโพเนนต์เลื่อนซ้ายอัตโนมัติ
    CollectibleLetter --|> ScrollLeft : ผูกคอมโพเนนต์เลื่อนซ้ายอัตโนมัติ
    Obstacle --> PlayerController : TakeDamage(1)
    CollectibleLetter --> LetterInventory : AddLetter(letter)
    PlayerController --> LetterInventory : ผู้เล่นถือคอมโพเนนต์ Inventory
    BossTypingCombat --> LetterInventory : ตรวจสอบและตัดตัวอักษร
    BossTypingCombat --> BossController : สั่ง TakeWordDamage()
    BossController --> BossTypingCombat : ส่งสัญญาณให้เปิด UI พิมพ์
```
