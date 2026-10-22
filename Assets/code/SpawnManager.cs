using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ==============================================================================
 * SPAWN MANAGER (ระบบสุ่มปล่อยสิ่งกีดขวางและตัวอักษร)
 * ทำหน้าที่:
 * 1. สุ่มสร้าง (Spawn) สิ่งกีดขวาง (Low/High) และเหรียญตัวอักษรจากจุดที่กำหนดทางขวา
 * 2. ปรับความถี่ในการปล่อยให้สัมพันธ์กับความเร็วของเกม (worldScrollSpeed) อัตโนมัติ
 * 3. สุ่มตัวอักษรที่สอดคล้องกับคำศัพท์ประจำด่าน เพื่อให้ผู้เล่นเก็บไปสะกดคำตีบอสได้จริง
 * 4. หยุดปล่อยวัตถุอัตโนมัติเมื่อเข้าสู่ช่วงสู้บอส (Boss Encounter) หรือผู้เล่นตาย
 * ==============================================================================
 */
public class SpawnManager : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: PREFABS & SPAWN POINTS (พรีแฟบและจุดปล่อยวัตถุ)
       ========================================================================== */
    [Header("Spawn Points (จุดปล่อยทางด้านขวาของจอ)")]
    [Tooltip("จุดปล่อยวัตถุระดับพื้นดิน (เช่น Low Obstacle และเหรียญบนพื้น)")]
    public Transform groundSpawnPoint;

    [Tooltip("จุดปล่อยวัตถุลอยสูง (เช่น High Obstacle และเหรียญระดับกระโดด)")]
    public Transform airSpawnPoint;

    [Header("Prefabs: Obstacles (สิ่งกีดขวาง)")]
    [Tooltip("รายการ Prefab สิ่งกีดขวางระดับพื้น (กระโดดหลบ)")]
    public List<GameObject> lowObstaclePrefabs = new List<GameObject>();

    [Tooltip("รายการ Prefab สิ่งกีดขวางระดับสูง (กดย่อสไลด์หลบ)")]
    public List<GameObject> highObstaclePrefabs = new List<GameObject>();

    [Header("Prefabs: Letters (ตัวอักษร)")]
    [Tooltip("Prefab เหรียญ/ตัวอักษร 3D ที่มีสคริปต์ CollectibleLetter")]
    public GameObject letterPrefab;

    /* ==========================================================================
       SECTION 2: SPAWN TIMING & SPEED SYNC (การตั้งเวลาและความเร็วที่สัมพันธ์กัน)
       ========================================================================== */
    [Header("Spawn Timing & Frequency")]
    [Tooltip("ช่วงเวลาต่ำสุดในการปล่อยวัตถุ (วินาที) ที่ความเร็วอ้างอิง")]
    public float minSpawnInterval = 1.8f;

    [Tooltip("ช่วงเวลาสูงสุดในการปล่อยวัตถุ (วินาที) ที่ความเร็วอ้างอิง")]
    public float maxSpawnInterval = 3.2f;

    [Tooltip("ความเร็วอ้างอิงฐาน (ใช้คำนวณสัดส่วนความถี่ตามความเร็วของด่าน)")]
    public float baseReferenceSpeed = 8.0f;

    [Header("Spawn Weights (อัตราส่วนการสุ่ม)")]
    [Range(0, 100)] public int lowObstacleWeight = 40;   // 40%
    [Range(0, 100)] public int highObstacleWeight = 30;  // 30%
    [Range(0, 100)] public int letterWeight = 30;        // 30%

    [Header("Smart Letter Settings")]
    [Tooltip("โอกาส (0-1) ที่จะสุ่มได้ตัวอักษรที่อยู่ในคำศัพท์ประจำด่าน เพื่อให้ผู้เล่นเก็บคำไปตีบอสได้")]
    [Range(0f, 1f)] public float targetWordLetterChance = 0.8f;

    [Header("Hierarchy Organization")]
    [Tooltip("Transform สำหรับเก็บวัตถุที่สร้างขึ้น เพื่อไม่ให้รก Hierarchy")]
    public Transform spawnedObjectsParent;

    private Coroutine spawnCoroutine;
    private bool isSpawning = false;
    private List<GameObject> activeSpawnedObjects = new List<GameObject>();

    void Start()
    {
        // ผูก Event ฟัง GameManager และ Player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStageStarted += HandleStageStarted;
            GameManager.Instance.OnBossEncounterStarted += HandleBossEncounter;

            if (GameManager.Instance.player != null)
            {
                GameManager.Instance.player.OnPlayerDied += StopSpawning;
            }

            // ถ้าเริ่มเกมมาอยู่ในเฟส Runner ให้เริ่มปล่อยของทันที
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
       SECTION 3: SPAWN LOOP (ลูปการสุ่มปล่อยวัตถุสัมพันธ์กับความเร็วด่าน)
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
        yield return new WaitForSeconds(1.5f); // รอเปิดฉากเล็กน้อยก่อนปล่อยของชุดแรก

        while (isSpawning)
        {
            // คำนวณความถี่ให้สัมพันธ์กับความเร็วของด่าน (ยิ่งด่านวิ่งเร็ว ระยะห่างเวลาต้องปรับให้เหมาะสม)
            float currentSpeed = (GameManager.Instance != null) ? GameManager.Instance.worldScrollSpeed : baseReferenceSpeed;
            float speedFactor = (currentSpeed > 0f) ? (baseReferenceSpeed / currentSpeed) : 1f;

            // สุ่มเวลาดีเลย์รอบถัดไป
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval) * speedFactor;
            yield return new WaitForSeconds(delay);

            if (!isSpawning) break;

            // สุ่มประเภทของวัตถุที่จะปล่อย
            SpawnRandomObject();
        }
    }

    /* ==========================================================================
       SECTION 4: OBJECT GENERATION (สร้างวัตถุตามน้ำหนักและจุดเกิด)
       ========================================================================== */
    private void SpawnRandomObject()
    {
        int totalWeight = lowObstacleWeight + highObstacleWeight + letterWeight;
        int roll = Random.Range(0, totalWeight);

        if (roll < lowObstacleWeight)
        {
            // ปล่อย Low Obstacle (บนพื้น)
            SpawnLowObstacle();
        }
        else if (roll < lowObstacleWeight + highObstacleWeight)
        {
            // ปล่อย High Obstacle (ลอยสูง)
            SpawnHighObstacle();
        }
        else
        {
            // ปล่อยเหรียญ/ตัวอักษร 3D
            SpawnLetter();
        }
    }

    private void SpawnLowObstacle()
    {
        if (lowObstaclePrefabs.Count == 0 || groundSpawnPoint == null) return;

        GameObject prefab = lowObstaclePrefabs[Random.Range(0, lowObstaclePrefabs.Count)];
        GameObject obj = Instantiate(prefab, groundSpawnPoint.position, groundSpawnPoint.rotation, spawnedObjectsParent);
        RegisterSpawnedObject(obj);
    }

    private void SpawnHighObstacle()
    {
        if (highObstaclePrefabs.Count == 0 || airSpawnPoint == null) return;

        GameObject prefab = highObstaclePrefabs[Random.Range(0, highObstaclePrefabs.Count)];
        GameObject obj = Instantiate(prefab, airSpawnPoint.position, airSpawnPoint.rotation, spawnedObjectsParent);
        RegisterSpawnedObject(obj);
    }

    private void SpawnLetter()
    {
        if (letterPrefab == null) return;

        // สุ่มปล่อยที่พื้นดิน หรือที่ลอยฟ้า (ให้กระโดดเก็บ)
        Transform spawnPoint = (Random.value > 0.5f && airSpawnPoint != null) ? airSpawnPoint : groundSpawnPoint;
        if (spawnPoint == null) return;

        GameObject letterObj = Instantiate(letterPrefab, spawnPoint.position, spawnPoint.rotation, spawnedObjectsParent);
        RegisterSpawnedObject(letterObj);

        // กำหนดตัวอักษร A-Z โดยดึงจากคำศัพท์ประจำด่านเพื่อให้เก็บไปสะกดคำได้
        CollectibleLetter collectible = letterObj.GetComponent<CollectibleLetter>();
        if (collectible != null)
        {
            char letterToGive = PickSmartLetter();
            collectible.SetLetter(letterToGive);
        }
    }

    /* ==========================================================================
       SECTION 5: SMART LETTER PICKING (สุ่มตัวอักษรจากคำศัพท์ประจำด่าน)
       ========================================================================== */
    private char PickSmartLetter()
    {
        // 1. ตรวจสอบว่ามีคำศัพท์ประจำด่านใน GameManager หรือไม่
        if (GameManager.Instance != null)
        {
            var stageProfile = GameManager.Instance.GetCurrentStageProfile();
            if (stageProfile.stageWords != null && stageProfile.stageWords.Count > 0)
            {
                // สุ่มว่าจะดึงจากคำศัพท์เป้าหมาย (targetWordLetterChance) หรือสุ่มอิสระ A-Z
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

        // 2. ถ้าไม่มีคำศัพท์ หรือสุ่มนอกคำ ให้สุ่มอักษรภาษาอังกฤษทั่วไป A-Z
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
       SECTION 6: STAGE & BOSS EVENT HANDLERS
       ========================================================================== */
    private void HandleStageStarted(int stageIndex, GameManager.StageProfile profile)
    {
        // เริ่มปล่อยของใหม่สำหรับด่านนี้
        StartSpawning();
    }

    private void HandleBossEncounter()
    {
        // เมื่อเจอบอส ให้หยุดปล่อยสิ่งกีดขวางทั่วไปทันที เพื่อให้เข้าสู่โหมดสู้บอส
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
