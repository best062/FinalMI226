using UnityEngine;

/**
 * ==============================================================================
 * OBSTACLE CONTROLLER (สิ่งกีดขวาง: กระโดดหลบ / สไลด์หลบ)
 * ทำหน้าที่: ตรวจจับการชนกับผู้เล่น และสร้างความเสียหาย 1 หัวใจ
 * ==============================================================================
 */
[RequireComponent(typeof(ScrollLeft))]
public class Obstacle : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: OBSTACLE TYPES (ประเภทสิ่งกีดขวาง)
       ทำหน้าที่: แยกชนิด:
         - LOW: อยู่บนพื้น ผู้เล่นต้อง 'กระโดด' ข้าม (เช่น หิน, หนาม)
         - HIGH: ลอยอยู่สูงระดับศีรษะ ผู้เล่นต้อง 'สไลด์' ลอด (เช่น ท่อนไม้, กิ่งไม้)
       ========================================================================== */
    public enum ObstacleType { Low, High }

    [Header("Obstacle Settings")]
    public ObstacleType obstacleType = ObstacleType.Low;
    public int damageAmount = 1;

    /* ==========================================================================
       SECTION 2: COLLISION DETECTION (ตรวจจับการชน)
       ทำหน้าที่: เรียกฟังก์ชัน TakeDamage ของ Player เมื่อเดินชน
       ========================================================================== */
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }
        }
    }
}
