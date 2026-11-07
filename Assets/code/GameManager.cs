using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * ==============================================================================
 * GAME MANAGER & CORE GAME LOOP (ตัวควบคุมโฟลว์เกมหลัก)
 * ทำหน้าที่: ดำเนิน Core Game Loop 5 ขั้นตอน:
 *   1. วิ่งหลบ obstacle และเก็บตัวอักษร
 *   2. เจอบอส
 *   3. ตีบอส
 *   4. บอสตาย
 *   5. ไปด่านใหม่
 * ==============================================================================
 */
public class GameManager : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: CORE GAME STATES & STAGE DATA (สถานะของเกมและข้อมูลด่าน)
       ========================================================================== */
    public enum GamePhase
    {
        Runner,         // 1. วิ่งหลบสิ่งกีดขวาง + เก็บตัวอักษร
        BossEncounter,  // 2. เจอบอส
        BossBattle,     // 3. ตีบอส (ลูปพิมพ์สะกดคำ)
        BossDefeated,   // 4. บอสตาย
        NextStageWait   // 5. เตรียมไปด่านใหม่
    }

    [System.Serializable]
    public struct StageProfile
    {
        public int stageNumber;
        public string stageName;       // เช่น "Savanna: Lion", "Forest: Bear"
        [Tooltip("ความเร็วของสิ่งกีดขวาง/ไอเทมประจำด่าน (Gameplay Objects Speed) เช่น 10f")]
        public float stageObjectSpeed;
        [Tooltip("ความเร็วของฉากหลัง/พื้นประจำด่าน (World/Background Speed) เช่น 6f")]
        public float stageWorldSpeed;
        [Tooltip("ความเร็วแบบเดิม (Fallback กรณีไม่ได้แยกช่องด้านบน)")]
        public float stageSpeed;       
        public float distanceToBoss;   // ระยะทางที่ต้องวิ่งก่อนพบบอส (เช่น 1500 เมตร)
        public GameObject bossPrefab;  // Prefab ของบอสประจำด่าน
        [Tooltip("รายการคำศัพท์ประจำด่าน สำหรับให้ Spawner สุ่มโปรยตัวอักษรลงมาบนทางวิ่ง")]
        public List<string> stageWords; 
    }

    [Header("Stage Configuration")]
    public List<StageProfile> stages = new List<StageProfile>();
    public int currentStageIndex = 0;

    [Header("Scroll Speeds (แยกความเร็วฉากหลังกับสิ่งกีดขวาง/ไอเทม)")]
    [Tooltip("ความเร็วในการเลื่อนของสิ่งกีดขวาง กล่องสุ่มตัวอักษร และวัตถุบนทางวิ่ง (Gameplay Objects)")]
    public float objectScrollSpeed = 10f;

    [Tooltip("ความเร็วในการเลื่อนของฉากหลัง แผ่นพื้น และสิ่งแวดล้อม (World Background / Parallax)")]
    public float worldBackgroundSpeed = 6f;

    // Backwards Compatibility: คืนค่า objectScrollSpeed เพื่อให้โค้ดเดิมไม่พัง
    public float worldScrollSpeed
    {
        get => objectScrollSpeed;
        set => objectScrollSpeed = value;
    }

    [Header("Current Progress")]
    public GamePhase currentPhase = GamePhase.Runner;
    public float distanceRun = 0f;

    [Header("References")]
    public PlayerController player;
    public LetterInventory inventory;
    public Transform bossSpawnPoint;

    private GameObject activeBossInstance;
    private BossController activeBossController;

    public static GameManager Instance { get; private set; }

    // Events สำหรับแจ้งเตือนระบบอื่น (เช่น SpawnManager, DistanceUI) ให้เริ่ม/หยุดตามสถานะ
    public event Action<int, StageProfile> OnStageStarted;
    public event Action OnBossEncounterStarted;
    public event Action OnStageCompleted;
    public event Action<float, float> OnDistanceUpdated; // (currentDistance, targetDistanceToBoss)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartStage(currentStageIndex);
    }

    void Update()
    {
        HandleCoreGameLoop();
    }

    /* ==========================================================================
       SECTION 2: CORE GAME LOOP LOGIC (การดำเนินลูปหลักของเกม)
       ทำหน้าที่: ตรวจสอบระยะทางวิ่ง และเปลี่ยนสลับระหว่างช่วงวิ่งกับช่วงสู้บอส
       ========================================================================== */
    private void HandleCoreGameLoop()
    {
        switch (currentPhase)
        {
            case GamePhase.Runner:
                // 1. วิ่งเก็บระยะทางตามความเร็วการเลื่อนฉาก
                distanceRun += worldScrollSpeed * Time.deltaTime;
                float targetDistance = GetCurrentDistanceToBoss();
                OnDistanceUpdated?.Invoke(distanceRun, targetDistance);

                // ตรวจสอบว่าวิ่งถึงระยะเจอบอสหรือยัง
                if (currentStageIndex < stages.Count && distanceRun >= stages[currentStageIndex].distanceToBoss)
                {
                    TransitionToBossEncounter();
                }
                break;

            case GamePhase.BossEncounter:
                // 2. เจอบอส (บอสกำลังกระโดดลงมา / เปิดตัว)
                break;

            case GamePhase.BossBattle:
                // 3. ตีบอส (BossController กำลังทำงาน)
                break;

            case GamePhase.BossDefeated:
                // 4. บอสตาย
                break;

            case GamePhase.NextStageWait:
                // 5. ไปด่านใหม่
                break;
        }
    }

    /* ==========================================================================
       SECTION 3: BOSS ENCOUNTER & TRANSITION (การสลับเข้าสู่โหมดสู้บอส)
       ทำหน้าที่: หยุดการสปอว์นสิ่งกีดขวางทั่วไป และสร้างตัวบอสออกมา
       ========================================================================== */
    private void TransitionToBossEncounter()
    {
        currentPhase = GamePhase.BossEncounter;
        Debug.Log("[GameManager] วิ่งถึงระยะกำหนดแล้ว! พบบอสประจำด่าน!");

        // ส่งสัญญาณให้ Spawner หยุดปล่อยสิ่งกีดขวาง
        OnBossEncounterStarted?.Invoke();

        StageProfile stage = stages[currentStageIndex];
        if (stage.bossPrefab != null && bossSpawnPoint != null)
        {
            activeBossInstance = Instantiate(stage.bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            activeBossController = activeBossInstance.GetComponent<BossController>();

            if (activeBossController != null)
            {
                activeBossController.OnBossDefeated += OnBossKilled;
            }
        }

        currentPhase = GamePhase.BossBattle;
    }

    /* ==========================================================================
       SECTION 4: BOSS DEFEATED & NEXT STAGE (บอสตายและข้ามไปด่านใหม่)
       ทำหน้าที่: เคลียร์บอส, รีเซ็ตระยะทาง และโหลดด่านถัดไป
       ========================================================================== */
    private void OnBossKilled()
    {
        currentPhase = GamePhase.BossDefeated;
        Debug.Log("[GameManager] บอสตายเรียบร้อย! เตรียมเคลื่อนสู่ด่านใหม่...");

        if (activeBossController != null)
        {
            activeBossController.OnBossDefeated -= OnBossKilled;
        }

        OnStageCompleted?.Invoke();
        Invoke(nameof(ProceedToNextStage), 2.5f);
    }

    private void ProceedToNextStage()
    {
        currentStageIndex++;

        if (currentStageIndex < stages.Count)
        {
            // ไปด่านใหม่
            StartStage(currentStageIndex);
        }
        else
        {
            Debug.Log("[GameManager] ยินดีด้วย! คุณผ่านบอสสัตว์ครบทุกด่านแล้ว!");
            // TODO: แสดงหน้าจอ Victory Screen
        }
    }

    private void StartStage(int stageIdx)
    {
        currentStageIndex = stageIdx;
        distanceRun = 0f;
        currentPhase = GamePhase.Runner;

        if (activeBossInstance != null)
        {
            Destroy(activeBossInstance);
        }

        if (stageIdx < stages.Count)
        {
            StageProfile stage = stages[stageIdx];
            // ซิงค์ความเร็วของสิ่งกีดขวางและฉากหลังแยกจากกัน
            if (stage.stageObjectSpeed > 0f)
            {
                objectScrollSpeed = stage.stageObjectSpeed;
            }
            else if (stage.stageSpeed > 0f)
            {
                objectScrollSpeed = stage.stageSpeed;
            }

            if (stage.stageWorldSpeed > 0f)
            {
                worldBackgroundSpeed = stage.stageWorldSpeed;
            }
            else if (stage.stageSpeed > 0f)
            {
                // สไตล์ Cookie Run: ฉากหลังเลื่อนช้ากว่าเลนสิ่งกีดขวางเพื่อมิติ Parallax
                worldBackgroundSpeed = stage.stageSpeed * 0.65f;
            }

            Debug.Log($"[GameManager] ซิงค์ความเร็ว -> วัตถุ/สิ่งกีดขวาง: {objectScrollSpeed}, ฉากหลัง: {worldBackgroundSpeed}");

            Debug.Log($"[GameManager] เริ่มต้นด่านที่ {stageIdx + 1}: {stage.stageName}");
            OnStageStarted?.Invoke(stageIdx, stage);
        }
    }

    public StageProfile GetCurrentStageProfile()
    {
        if (currentStageIndex >= 0 && currentStageIndex < stages.Count)
        {
            return stages[currentStageIndex];
        }
        return default;
    }

    /* ==========================================================================
       SECTION 5: DISTANCE QUERY HELPERS (ฟังก์ชันสืบค้นระยะทางสำหรับ UI)
       ========================================================================== */
    public float GetCurrentDistanceToBoss()
    {
        if (currentStageIndex >= 0 && currentStageIndex < stages.Count)
        {
            return stages[currentStageIndex].distanceToBoss;
        }
        return 1000f; // ค่าสำรองเริ่มต้น
    }

    public float GetDistanceProgressNormalized()
    {
        float target = GetCurrentDistanceToBoss();
        if (target <= 0.001f) return 0f;
        return Mathf.Clamp01(distanceRun / target);
    }
}
