using UnityEngine;

/**
 * ==============================================================================
 * OBSTACLE CONTROLLER (สิ่งกีดขวางแบ่ง 3 ส่วน: กระโดด 1 ครั้ง / กระโดด 2 ครั้ง / สไลด์)
 * ทำหน้าที่:
 * 1. ตรวจจับการชนกับตัวละครผู้เล่น (Player) และลดพลังชีวิต (TakeDamage)
 * 2. ระบุประเภทของอุปสรรคแบ่งออกเป็น 3 ส่วนชัดเจน:
 *    - 1. SingleJump (กระโดด 1 ครั้ง): อุปสรรคระดับต่ำ วางบนพื้นดิน
 *    - 2. DoubleJump (กระโดด 2 ครั้ง): อุปสรรคระดับกลาง/สูง วางบนพื้น ต้องใช้แรงกระโดดสองจังหวะ
 *    - 3. Slide (สไลด์): อุปสรรคระดับสูงลอยฟ้า ด้านล่างเปิดโล่ง ต้องกดย่อตัวสไลด์มุดลอด
 * 3. เลื่อนเข้าหาตัวละครด้วย ScrollLeft อัตโนมัติ
 * ==============================================================================
 */
[RequireComponent(typeof(ScrollLeft))]
public class Obstacle : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: OBSTACLE TYPES (ประเภทสิ่งกีดขวาง 3 ส่วน)
       ========================================================================== */
    public enum ObstacleType 
    { 
        SingleJump = 0, // 1. กระโดด 1 ครั้ง (ต่ำ: หิน, ตอไม้, หนามเตี้ย)
        DoubleJump = 1, // 2. กระโดด 2 ครั้ง (สูง: กำแพงหิน, เสาสูง, กล่องซ้อน)
        Slide = 2,      // 3. สไลด์ลอด (ลอยสูง: ท่อนไม้ลอย, ผลึกย้อย, กิ่งไม้ขวางระดับหัว)

        // รองรับของเดิมเพื่อไม่ให้พัง (Backwards Compatibility)
        Low = 0,
        High = 2
    }

    [Header("Obstacle Settings")]
    [Tooltip("ประเภทของอุปสรรค (1.กระโดด 1 ครั้ง / 2.กระโดด 2 ครั้ง / 3.สไลด์)")]
    public ObstacleType obstacleType = ObstacleType.SingleJump;

    [Tooltip("จำนวนหัวใจที่จะหักเมื่อผู้เล่นชนสิ่งกีดขวางนี้")]
    public int damageAmount = 1;

    [Header("Optional Feedback")]
    [Tooltip("Particle Effect เมื่อผู้เล่นชน (ถ้ามี)")]
    public GameObject hitParticlePrefab;

    [Tooltip("เสียงเอฟเฟกต์เมื่อผู้เล่นชน (ถ้ามี)")]
    public AudioClip hitSoundClip;

    /* ==========================================================================
       SECTION 2: COLLISION DETECTION (ตรวจจับการชน)
       ========================================================================== */
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
                PlayHitFeedback();
            }
        }
    }

    private void PlayHitFeedback()
    {
        if (hitParticlePrefab != null)
        {
            Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
        }

        if (hitSoundClip != null)
        {
            AudioSource.PlayClipAtPoint(hitSoundClip, transform.position);
        }
    }

    // วาด Gizmos ใน Scene View ช่วยให้นักพัฒนาเห็นประเภทสิ่งกีดขวางได้ชัดเจน
    private void OnDrawGizmos()
    {
        switch (obstacleType)
        {
            case ObstacleType.SingleJump:
                Gizmos.color = Color.green; // สีเขียว = กระโดด 1 ครั้ง
                break;
            case ObstacleType.DoubleJump:
                Gizmos.color = Color.yellow; // สีเหลือง = กระโดด 2 ครั้ง
                break;
            case ObstacleType.Slide:
                Gizmos.color = Color.cyan; // สีฟ้า = สไลด์ลอด
                break;
        }

        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
