using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ==============================================================================
 * BOSS CONTROLLER & BOSS GAME LOOP (ระบบบอสประจำด่านธีมสัตว์)
 * ทำหน้าที่: ดำเนินลูปบอส 5 ขั้นตอน:
 *   1. เจอบอส (Intro)
 *   2. บอสตี (Boss Attack)
 *   3. บอสปล่อยลูกน้องพร้อมคำศัพท์ (Summon Minions)
 *   4. บอสเหนื่อย (Tired State)
 *   5. เราตี (Player Attack) -> ลูปจนเลือดหมด
 * ==============================================================================
 */
public class BossController : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: BOSS DATA & STATES (ข้อมูลบอสและสถานะลูป)
       ========================================================================== */
    public enum BossState
    {
        Intro,          // 1. เจอบอส
        BossAttack,     // 2. บอสตี
        SummonMinions,  // 3. บอสปล่อยลูกน้องพร้อมคำศัพท์
        BossTired,      // 4. บอสเหนื่อย (เปิดช่องให้พิมพ์ตี)
        Defeated        // บอสตาย
    }

    [System.Serializable]
    public struct AnimalWordData
    {
        public string word;       // เช่น "LION", "BEAR", "TIGER"
        public string meaning;    // คำแปล เช่น "สิงโต"
    }

    [Header("Boss Profile")]
    public string bossName = "LEO THE LION";
    public int maxHealth = 100;
    public int currentHealth = 100;
    public int damagePerWord = 34;

    [Header("Animal Vocabulary")]
    public List<AnimalWordData> targetWords = new List<AnimalWordData>();
    private int currentWordIndex = 0;

    [Header("Attack Settings")]
    public GameObject projectilePrefab; // กระสุนคลื่นพลังที่บอสยิง
    public Transform attackSpawnPoint;
    public float attackInterval = 1.2f;
    public int attackCountPerPhase = 3;

    [Header("State Control")]
    public BossState currentState = BossState.Intro;
    public float tiredDuration = 6.0f; // เวลาที่บอสเหนื่อยให้ผู้เล่นพิมพ์คำศัพท์

    // Events สำหรับส่งสัญญาณไปยัง UI และ GameManager
    public event Action<BossState> OnBossStateChanged;
    public event Action<int, int> OnBossHealthChanged;
    public event Action<AnimalWordData> OnPromptWordToPlayer;
    public event Action OnBossDefeated;

    void Start()
    {
        currentHealth = maxHealth;
        OnBossHealthChanged?.Invoke(currentHealth, maxHealth);
        StartCoroutine(BossLoopCoroutine());
    }

    /* ==========================================================================
       SECTION 2: BOSS GAME LOOP COROUTINE (ลูปการต่อสู้ของบอส)
       ทำหน้าที่: วนลูปบอส 1.เจอบอส -> 2.บอสตี -> 3.ปล่อยลูกน้อง -> 4.เหนื่อย -> 5.เราตี
       ========================================================================== */
    private IEnumerator BossLoopCoroutine()
    {
        // ----------------------------------------------------
        // STEP 1: เจอบอส (INTRO)
        // ----------------------------------------------------
        SetState(BossState.Intro);
        yield return new WaitForSeconds(2.0f);

        while (currentHealth > 0)
        {
            // ----------------------------------------------------
            // STEP 2: บอสตี (BOSS ATTACK)
            // ----------------------------------------------------
            SetState(BossState.BossAttack);
            for (int i = 0; i < attackCountPerPhase; i++)
            {
                PerformAttack();
                yield return new WaitForSeconds(attackInterval);
            }

            // ----------------------------------------------------
            // STEP 3: บอสปล่อยลูกน้องพร้อมคำศัพท์ (SUMMON MINIONS)
            // ----------------------------------------------------
            SetState(BossState.SummonMinions);
            SpawnMinionsAndLetters();
            yield return new WaitForSeconds(2.0f);

            // ----------------------------------------------------
            // STEP 4 & 5: บอสเหนื่อย & เราตี (BOSS TIRED & PLAYER ATTACK)
            // ----------------------------------------------------
            SetState(BossState.BossTired);
            // ส่งคำศัพท์เป้าหมายให้ผู้เล่นพิมพ์
            AnimalWordData currentTarget = GetCurrentTargetWord();
            OnPromptWordToPlayer?.Invoke(currentTarget);

            // รอช่วงเวลาบอสเหนื่อย (หรือจนกว่าผู้เล่นจะตีสำเร็จ)
            float timer = 0f;
            while (timer < tiredDuration && currentState == BossState.BossTired)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // ถ้าพ้นช่วงเหนื่อยแล้วบอสยังไม่ตาย จะกลับไปลูปโจมตีใหม่
        }
    }

    private void SetState(BossState newState)
    {
        currentState = newState;
        OnBossStateChanged?.Invoke(newState);
        Debug.Log($"[Boss] เปลี่ยนสถานะเป็น: {newState}");
    }

    /* ==========================================================================
       SECTION 3: BOSS ATTACK ACTION (การโจมตีของบอส)
       ทำหน้าที่: ยิงคลื่นพลังหรือปล่อยการโจมตีให้ผู้เล่นกระโดดหรือสไลด์หลบ
       ========================================================================== */
    private void PerformAttack()
    {
        if (projectilePrefab != null && attackSpawnPoint != null)
        {
            Instantiate(projectilePrefab, attackSpawnPoint.position, Quaternion.identity);
        }
        Debug.Log("[Boss] กำลังโจมตีใส่ผู้เล่น!");
    }

    /* ==========================================================================
       SECTION 4: MINIONS & LETTER DROPS (ปล่อยลูกน้องและโปรยตัวอักษร)
       ทำหน้าที่: ดรอปตัวอักษรของคำศัพท์เป้าหมายบนทางวิ่งให้ผู้เล่นเก็บทัน
       ========================================================================== */
    private void SpawnMinionsAndLetters()
    {
        AnimalWordData target = GetCurrentTargetWord();
        Debug.Log($"[Boss] ปล่อยลูกน้องพร้อมคำศัพท์เป้าหมาย: {target.word} ({target.meaning})");
        // TODO: เรียก SpawnManager ให้ปล่อยตัวอักษรประจำคำนี้ออกมาบนเลน
    }

    /* ==========================================================================
       SECTION 5: DAMAGE & HEALTH CHECK (การรับดาเมจจากคำศัพท์ที่ผู้เล่นพิมพ์)
       ทำหน้าที่: ลดเลือดบอสเมื่อสะกดคำถูก และตรวจสอบว่าบอสตายหรือไม่
       ========================================================================== */
    public void TakeWordDamage()
    {
        if (currentState != BossState.BossTired) return;

        currentHealth = Mathf.Max(0, currentHealth - damagePerWord);
        currentWordIndex++;
        OnBossHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[Boss] โดนโจมตีด้วยคำศัพท์! เลือดเหลือ: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            SetState(BossState.Defeated);
            OnBossDefeated?.Invoke();
            Debug.Log("[Boss] บอสพ่ายแพ้แล้ว! เตรียมไปด่านใหม่");
        }
        else
        {
            // ถูกตีแล้ว บอสจะหลุดจากสถานะเหนื่อย และกลับไปโจมตีใหม่
            currentState = BossState.BossAttack;
        }
    }

    public AnimalWordData GetCurrentTargetWord()
    {
        if (targetWords.Count == 0) return new AnimalWordData { word = "ANIMAL", meaning = "สัตว์" };
        return targetWords[currentWordIndex % targetWords.Count];
    }
}
