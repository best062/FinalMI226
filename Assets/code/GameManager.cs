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
        public float distanceToBoss;   // ระยะทางที่ต้องวิ่งก่อนพบบอส (เช่น 500 เมตร)
        public GameObject bossPrefab;  // Prefab ของบอสประจำด่าน
    }

    [Header("Stage Configuration")]
    public List<StageProfile> stages = new List<StageProfile>();
    public int currentStageIndex = 0;

    [Header("Current Progress")]
    public GamePhase currentPhase = GamePhase.Runner;
    public float distanceRun = 0f;

    [Header("References")]
    public PlayerController player;
    public LetterInventory inventory;
    public Transform bossSpawnPoint;

    private GameObject activeBossInstance;
    private BossController activeBossController;

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
                // 1. วิ่งเก็บระยะทาง
                distanceRun += player.runSpeed * Time.deltaTime;

                // ตรวจสอบว่าวิ่งถึงระยะเจอบอสหรือยัง
                if (distanceRun >= stages[currentStageIndex].distanceToBoss)
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

        Debug.Log($"[GameManager] เริ่มต้นด่านที่ {stageIdx + 1}: {stages[stageIdx].stageName}");
    }
}
