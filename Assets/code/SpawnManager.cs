using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ==============================================================================
 * SPAWN MANAGER (ระบบเสกสิ่งกีดขวาง 3 ส่วน และกล่องสุ่มตัวอักษร)
 * ทำหน้าที่:
 * 1. สุ่มเสก (Spawn) สิ่งกีดขวางแบ่งเป็น 3 หมวดการควบคุมชัดเจน:
 *    - 1. กระโดด 1 ครั้ง (Single Jump Obstacles): สิ่งกีดขวางระดับพื้นเตี้ย
 *    - 2. กระโดด 2 ครั้ง (Double Jump Obstacles): สิ่งกีดขวางระดับพื้นที่มีความสูงหรือกว้างขึ้น
 *    - 3. สไลด์ (Slide Obstacles): สิ่งกีดขวางลอยฟ้า ต้องย่อตัวสไลด์ลอด
 * 2. เสกกล่องสุ่มตัวอักษร 3D (Generic Letter Box) เพื่อส่งตัวอักษรเข้ากระเป๋าแบบ Smart Drop
 * 3. มีระบบ Procedural Prototype Auto-Generator: หากยังไม่มี Prefab ใส่ในรายการ 
 *    ระบบจะสร้างวัตถุ 3D ชั่วคราว (Primitive) ให้เล่นและทดสอบได้ทันที ไม่ต้องรอนักออกแบบปั้นโมเดล
 * 4. ซิงค์ความถี่ในการปล่อยสัมพันธ์กับความเร็วของด่าน (worldScrollSpeed) อัตโนมัติ
 * 5. หยุดปล่อยวัตถุเมื่อเจอบอส (Boss Encounter) หรือผู้เล่นตาย
 * ==============================================================================
 */
public class SpawnManager : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: SPAWN POINTS (จุดปล่อยทางขวา แยกพื้นสำหรับอุปสรรคและกล่องสุ่ม)
       ========================================================================== */
    [Header("Spawn Points (จุดปล่อยทางด้านขวาของจอ)")]
    [Tooltip("จุดปล่อยสิ่งกีดขวางระดับพื้นดิน (สำหรับ: 1.กระโดด 1 ครั้ง, 2.กระโดด 2 ครั้ง เช่น Y = 0)")]
    public Transform groundObstacleSpawnPoint;

    [Tooltip("จุดปล่อยกล่องสุ่มระดับพื้นดิน (ยกสูงขึ้น เช่น Y = 0.6 ถึง 0.8m เพื่อให้ขยับขึ้นลง (Bobbing) โดยไม่จมดิน)")]
    public Transform groundLetterBoxSpawnPoint;

    [Tooltip("จุดปล่อยวัตถุลอยสูง (สำหรับ: 3.สไลด์มุดลอด, และกล่องสุ่มลอยฟ้าที่ต้องกระโดดเก็บ)")]
    public Transform airSpawnPoint;

    [Tooltip("จุดปล่อยพื้นเดิม (Backwards Compatibility กรณีไม่ได้ลากแยกช่องใหม่)")]
    public Transform groundSpawnPoint;

    /* ==========================================================================
       SECTION 2: 3-PART OBSTACLE PREFABS (รายการ Prefab สิ่งกีดขวางแบ่ง 3 ส่วน)
       ========================================================================== */
    [Header("1. สิ่งกีดขวาง: กระโดด 1 ครั้ง (Single Jump)")]
    [Tooltip("Prefab สิ่งกีดขวางต่ำที่กระโดดครั้งเดียวพ้น (เช่น ก้อนหินเล็ก, ขอนไม้เตี้ย, หนามบนพื้น)")]
    public List<GameObject> singleJumpObstaclePrefabs = new List<GameObject>();

    [Header("2. สิ่งกีดขวาง: กระโดด 2 ครั้ง (Double Jump)")]
    [Tooltip("Prefab สิ่งกีดขวางสูงที่ต้องกดกระโดด 2 ครั้ง (เช่น กำแพงหินสูง, เสาหิน, กล่องซ้อนสองชั้น)")]
    public List<GameObject> doubleJumpObstaclePrefabs = new List<GameObject>();

    [Header("3. สิ่งกีดขวาง: สไลด์มุดลอด (Slide / Duck)")]
    [Tooltip("Prefab สิ่งกีดขวางลอยสูงระดับศีรษะที่ต้องกดย่อตัวสไลด์ลอด (เช่น ท่อนไม้ลอย, ผลึกหินย้อย)")]
    public List<GameObject> slideObstaclePrefabs = new List<GameObject>();

    [Header("Backwards Compatibility (ของเดิม)")]
    [Tooltip("รายการ Prefab เดิม (ถ้าช่องด้านบนว่าง ระบบจะใช้ช่องนี้เป็นตัวสำรองอัตโนมัติ)")]
    public List<GameObject> lowObstaclePrefabs = new List<GameObject>();
    public List<GameObject> highObstaclePrefabs = new List<GameObject>();

    /* ==========================================================================
       SECTION 3: LETTER BOX PREFABS (กล่องสุ่มตัวอักษร)
       ========================================================================== */
    [Header("Prefabs: Generic Letter Box / Letters (กล่องสุ่มตัวอักษร)")]
    [Tooltip("Prefab กล่องสุ่ม/ถุงวัตถุดิบ (Generic Letter Box) ที่มีสคริปต์ LetterBox แนะนำให้ใช้เป็นหลัก")]
    public GameObject letterBoxPrefab;

    [Tooltip("Prefab เหรียญ/ตัวอักษร 3D รายตัวแบบเดิม (Fallback)")]
    public GameObject letterPrefab;

    /* ==========================================================================
       SECTION 4: SPAWN WEIGHTS & TIMING (อัตราส่วนการสุ่มและเวลา)
       ========================================================================== */
    [Header("Spawn Weights (อัตราส่วนการสุ่ม 4 หมวด)")]
    [Range(0, 100)] public int singleJumpWeight = 30; // 30% กระโดด 1 ครั้ง
    [Range(0, 100)] public int doubleJumpWeight = 20; // 20% กระโดด 2 ครั้ง
    [Range(0, 100)] public int slideWeight = 25;      // 25% สไลด์
    [Range(0, 100)] public int letterBoxWeight = 25;  // 25% กล่องสุ่มตัวอักษร

    [Header("Spawn Timing & Frequency")]
    [Tooltip("ช่วงเวลาต่ำสุดในการปล่อยวัตถุ (วินาที)")]
    public float minSpawnInterval = 1.8f;

    [Tooltip("ช่วงเวลาสูงสุดในการปล่อยวัตถุ (วินาที)")]
    public float maxSpawnInterval = 3.2f;

    [Tooltip("ความเร็วอ้างอิงฐานสำหรับคำนวณสัดส่วนเวลา")]
    public float baseReferenceSpeed = 8.0f;

    [Header("Smart Letter Settings")]
    [Tooltip("โอกาส (0-1) ที่จะสุ่มได้ตัวอักษรที่ตรงกับคำศัพท์ประจำด่าน")]
    [Range(0f, 1f)] public float targetWordLetterChance = 0.8f;

    [Header("Hierarchy & Fallback Prototype Generator")]
    [Tooltip("Transform สำหรับเก็บวัตถุที่สร้างขึ้นเพื่อไม่ให้รก Hierarchy")]
    public Transform spawnedObjectsParent;

    [Tooltip("เปิดใช้งานระบบสร้างโมเดลตัวอย่างชั่วคราวอัตโนมัติหากยังไม่มี Prefab ในลิสต์")]
    public bool enableProceduralPrototypes = true;

    private Coroutine spawnCoroutine;
    private bool isSpawning = false;
    private List<GameObject> activeSpawnedObjects = new List<GameObject>();

    void Start()
    {
        // สร้างจุดปล่อยอัตโนมัติหากยังไม่ได้ผูกใน Inspector
        EnsureSpawnPointsExist();

        // ผูก Event ฟัง GameManager และ Player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStageStarted += HandleStageStarted;
            GameManager.Instance.OnBossEncounterStarted += HandleBossEncounter;

            if (GameManager.Instance.player != null)
            {
                GameManager.Instance.player.OnPlayerDied += StopSpawning;
            }

            if (GameManager.Instance.currentPhase == GameManager.GamePhase.Runner)
            {
                StartSpawning();
            }
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStageStarted -= HandleStageStarted;
            GameManager.Instance.OnBossEncounterStarted -= HandleBossEncounter;

            if (GameManager.Instance.player != null)
            {
                GameManager.Instance.player.OnPlayerDied -= StopSpawning;
            }
        }
    }

    /* ==========================================================================
       SECTION 5: SPAWN LOOP (ลูปควบคุมการปล่อยวัตถุ)
       ========================================================================== */
    public void StartSpawning()
    {
        if (isSpawning) return;
        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnLoopRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLoopRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        while (isSpawning)
        {
            float currentSpeed = (GameManager.Instance != null) ? GameManager.Instance.worldScrollSpeed : baseReferenceSpeed;
            float speedFactor = (currentSpeed > 0f) ? (baseReferenceSpeed / currentSpeed) : 1f;

            float delay = Random.Range(minSpawnInterval, maxSpawnInterval) * speedFactor;
            yield return new WaitForSeconds(delay);

            if (!isSpawning) break;

            SpawnRandomObject();
        }
    }

    /* ==========================================================================
       SECTION 6: OBJECT GENERATION (แบ่ง 3 ส่วนอุปสรรค + 1 กล่องสุ่ม)
       ========================================================================== */
    private void SpawnRandomObject()
    {
        int totalWeight = singleJumpWeight + doubleJumpWeight + slideWeight + letterBoxWeight;
        if (totalWeight <= 0) totalWeight = 100;

        int roll = Random.Range(0, totalWeight);

        if (roll < singleJumpWeight)
        {
            // 1. กระโดด 1 ครั้ง
            SpawnSingleJumpObstacle();
        }
        else if (roll < singleJumpWeight + doubleJumpWeight)
        {
            // 2. กระโดด 2 ครั้ง
            SpawnDoubleJumpObstacle();
        }
        else if (roll < singleJumpWeight + doubleJumpWeight + slideWeight)
        {
            // 3. สไลด์ลอด
            SpawnSlideObstacle();
        }
        else
        {
            // 4. กล่องสุ่มตัวอักษร
            SpawnLetterBox();
        }
    }

    /* --------------------------------------------------------------------------
       1. เสกอุปสรรค: กระโดด 1 ครั้ง (Single Jump)
       -------------------------------------------------------------------------- */
    public GameObject SpawnSingleJumpObstacle()
    {
        EnsureSpawnPointsExist();
        Transform targetSpawn = groundObstacleSpawnPoint != null ? groundObstacleSpawnPoint : groundSpawnPoint;
        Vector3 spawnPos = targetSpawn.position;

        // ดึงจาก singleJumpObstaclePrefabs หรือ fallback ไป lowObstaclePrefabs
        GameObject prefab = GetRandomPrefab(singleJumpObstaclePrefabs);
        if (prefab == null) prefab = GetRandomPrefab(lowObstaclePrefabs);

        GameObject obj = null;
        if (prefab != null)
        {
            obj = Instantiate(prefab, spawnPos, targetSpawn.rotation, spawnedObjectsParent);
        }
        else if (enableProceduralPrototypes)
        {
            // สร้างโมเดลจำลองอัตโนมัติ (เตี้ย ความสูงประมาณ 0.8m)
            obj = CreateProceduralObstacle("Proto_SingleJump", spawnPos + new Vector3(0f, 0.4f, 0f), new Vector3(1f, 0.8f, 1f), Color.red, Obstacle.ObstacleType.SingleJump);
        }

        RegisterSpawnedObject(obj);
        return obj;
    }

    /* --------------------------------------------------------------------------
       2. เสกอุปสรรค: กระโดด 2 ครั้ง (Double Jump)
       -------------------------------------------------------------------------- */
    public GameObject SpawnDoubleJumpObstacle()
    {
        EnsureSpawnPointsExist();
        Transform targetSpawn = groundObstacleSpawnPoint != null ? groundObstacleSpawnPoint : groundSpawnPoint;
        Vector3 spawnPos = targetSpawn.position;

        // ดึงจาก doubleJumpObstaclePrefabs
        GameObject prefab = GetRandomPrefab(doubleJumpObstaclePrefabs);

        GameObject obj = null;
        if (prefab != null)
        {
            obj = Instantiate(prefab, spawnPos, targetSpawn.rotation, spawnedObjectsParent);
        }
        else if (enableProceduralPrototypes)
        {
            // สร้างโมเดลจำลองอัตโนมัติ (สูงประมาณ 1.8m - 2.0m ต้องกระโดด 2 จังหวะ)
            obj = CreateProceduralObstacle("Proto_DoubleJump", spawnPos + new Vector3(0f, 1.0f, 0f), new Vector3(1.2f, 2.0f, 1f), new Color(1f, 0.5f, 0f), Obstacle.ObstacleType.DoubleJump);
        }

        RegisterSpawnedObject(obj);
        return obj;
    }

    /* --------------------------------------------------------------------------
       3. เสกอุปสรรค: สไลด์ลอด (Slide)
       -------------------------------------------------------------------------- */
    public GameObject SpawnSlideObstacle()
    {
        EnsureSpawnPointsExist();
        Vector3 spawnPos = (airSpawnPoint != null) 
            ? airSpawnPoint.position 
            : (groundObstacleSpawnPoint != null ? groundObstacleSpawnPoint.position : groundSpawnPoint.position) + new Vector3(0f, 1.6f, 0f);

        // ดึงจาก slideObstaclePrefabs หรือ fallback ไป highObstaclePrefabs
        GameObject prefab = GetRandomPrefab(slideObstaclePrefabs);
        if (prefab == null) prefab = GetRandomPrefab(highObstaclePrefabs);

        GameObject obj = null;
        if (prefab != null)
        {
            obj = Instantiate(prefab, spawnPos, airSpawnPoint.rotation, spawnedObjectsParent);
        }
        else if (enableProceduralPrototypes)
        {
            // สร้างโมเดลจำลองอัตโนมัติ (ลอยสูง 1.6m ใต้ท้องเปิดโล่งให้สไลด์ลอด)
            obj = CreateProceduralObstacle("Proto_SlideHigh", spawnPos, new Vector3(1.6f, 0.8f, 1f), Color.blue, Obstacle.ObstacleType.Slide);
        }

        RegisterSpawnedObject(obj);
        return obj;
    }

    /* --------------------------------------------------------------------------
       4. เสกกล่องสุ่มตัวอักษร (Generic Letter Box)
       -------------------------------------------------------------------------- */
    public GameObject SpawnLetterBox()
    {
        EnsureSpawnPointsExist();

        // แยกจุดปล่อย: สุ่มว่าจะเป็นลอยฟ้า (ให้กระโดดเก็บ) หรือระดับพื้น (ใช้ groundLetterBoxSpawnPoint ที่ยกสูงพ้นพื้น ไม่จมดิน)
        Transform groundTarget = groundLetterBoxSpawnPoint != null ? groundLetterBoxSpawnPoint : groundObstacleSpawnPoint;
        Transform spawnPoint = (Random.value > 0.5f && airSpawnPoint != null) ? airSpawnPoint : groundTarget;
        if (spawnPoint == null) spawnPoint = groundSpawnPoint;
        if (spawnPoint == null) return null;

        GameObject obj = null;

        // 1. ตรวจสอบว่ามี Prefab กล่องสุ่มหรือไม่
        if (letterBoxPrefab != null)
        {
            obj = Instantiate(letterBoxPrefab, spawnPoint.position, spawnPoint.rotation, spawnedObjectsParent);
            if (obj != null && obj.GetComponent<ScrollLeft>() == null)
            {
                obj.AddComponent<ScrollLeft>();
            }
        }
        else if (letterPrefab != null)
        {
            // Fallback ไปตัวอักษรรายตัวแบบเดิม
            obj = Instantiate(letterPrefab, spawnPoint.position, spawnPoint.rotation, spawnedObjectsParent);
            CollectibleLetter collectible = obj.GetComponent<CollectibleLetter>();
            if (collectible != null)
            {
                collectible.SetLetter(PickSmartLetter());
            }
        }
        else if (enableProceduralPrototypes)
        {
            // สร้างโมเดลกล่องจำลองชั่วคราว (สีทอง) พร้อมสคริปต์ LetterBox และ ScrollLeft
            obj = CreateProceduralLetterBox("Proto_LetterBox", spawnPoint.position);
        }

        RegisterSpawnedObject(obj);
        return obj;
    }

    // ฟังก์ชันเรียกเสกกล่องสุ่มตัวอักษรภายนอก (เช่น บอสเรียกเสก)
    public void SpawnLetterBoxManual()
    {
        SpawnLetterBox();
    }

    /* ==========================================================================
       SECTION 7: PROCEDURAL PROTOTYPE GENERATION (สร้างวัตถุจำลองอัตโนมัติ)
       ========================================================================== */
    private GameObject CreateProceduralObstacle(string name, Vector3 pos, Vector3 size, Color color, Obstacle.ObstacleType type)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = pos;
        cube.transform.localScale = size;
        if (spawnedObjectsParent != null) cube.transform.SetParent(spawnedObjectsParent);

        // ใส่ Material สี
        Renderer rend = cube.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = color;
        }

        // ตั้งค่า Collider เป็น Trigger
        Collider col = cube.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // แปะสคริปต์ Obstacle และ ScrollLeft
        Obstacle obs = cube.AddComponent<Obstacle>();
        obs.obstacleType = type;

        // ScrollLeft จะถูก Require อัตโนมัติจาก Obstacle
        return cube;
    }

    private GameObject CreateProceduralLetterBox(string name, Vector3 pos)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.position = pos;
        box.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        if (spawnedObjectsParent != null) box.transform.SetParent(spawnedObjectsParent);

        Renderer rend = box.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = new Color(1f, 0.85f, 0.1f); // สีทอง
        }

        Collider col = box.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        box.AddComponent<ScrollLeft>();
        box.AddComponent<LetterBox>();
        return box;
    }

    private GameObject GetRandomPrefab(List<GameObject> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    private void EnsureSpawnPointsExist()
    {
        // 1. จุดปล่อยสิ่งกีดขวางระดับพื้น (Y = 0)
        if (groundObstacleSpawnPoint == null)
        {
            if (groundSpawnPoint != null)
            {
                groundObstacleSpawnPoint = groundSpawnPoint;
            }
            else
            {
                GameObject g = new GameObject("Auto_GroundObstacleSpawnPoint");
                g.transform.position = new Vector3(25f, 0f, 0f);
                groundObstacleSpawnPoint = g.transform;
            }
        }

        // 2. จุดปล่อยกล่องสุ่มระดับพื้น (ยกสูง Y = 0.7m เพื่อไม่ให้จมดินตอนขยับขึ้นลง)
        if (groundLetterBoxSpawnPoint == null)
        {
            GameObject lb = new GameObject("Auto_GroundLetterBoxSpawnPoint");
            Vector3 basePos = groundObstacleSpawnPoint.position;
            lb.transform.position = new Vector3(basePos.x, basePos.y + 0.7f, basePos.z);
            groundLetterBoxSpawnPoint = lb.transform;
        }

        // 3. จุดปล่อยวัตถุลอยสูง (Y = 1.8m)
        if (airSpawnPoint == null)
        {
            GameObject a = new GameObject("Auto_AirSpawnPoint");
            a.transform.position = new Vector3(25f, 1.8f, 0f);
            airSpawnPoint = a.transform;
        }

        // เชื่อมโยงตัวแปรสำรอง
        if (groundSpawnPoint == null) groundSpawnPoint = groundObstacleSpawnPoint;
    }

    private char PickSmartLetter()
    {
        if (GameManager.Instance != null)
        {
            var stageProfile = GameManager.Instance.GetCurrentStageProfile();
            if (stageProfile.stageWords != null && stageProfile.stageWords.Count > 0)
            {
                if (Random.value <= targetWordLetterChance)
                {
                    string randomWord = stageProfile.stageWords[Random.Range(0, stageProfile.stageWords.Count)].ToUpper();
                    if (!string.IsNullOrEmpty(randomWord))
                    {
                        return randomWord[Random.Range(0, randomWord.Length)];
                    }
                }
            }
        }
        return (char)Random.Range('A', 'Z' + 1);
    }

    private void RegisterSpawnedObject(GameObject obj)
    {
        if (obj != null)
        {
            activeSpawnedObjects.Add(obj);
        }
    }

    /* ==========================================================================
       SECTION 8: STAGE & BOSS EVENTS
       ========================================================================== */
    private void HandleStageStarted(int stageIndex, GameManager.StageProfile profile)
    {
        StartSpawning();
    }

    private void HandleBossEncounter()
    {
        StopSpawning();
    }

    public void ClearActiveObjects()
    {
        foreach (var obj in activeSpawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        activeSpawnedObjects.Clear();
    }
}
