using UnityEngine;
using TMPro; // ใช้สำหรับแสดง 3D Text บนเหรียญ

/**
 * ==============================================================================
 * COLLECTIBLE LETTER (เหรียญตัวอักษรที่ลอยให้เก็บ)
 * ทำหน้าที่: หมุนตัวอักษร และส่งค่าตัวอักษรเข้า Inventory เมื่อผู้เล่นวิ่งชน
 * ==============================================================================
 */
public class CollectibleLetter : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: LETTER DATA & VISUALS (ข้อมูลตัวอักษรและการแสดงผล)
       ทำหน้าที่: กำหนดตัวอักษร A-Z และอัปเดตข้อความบน 3D Text / TextMeshPro
       ========================================================================== */
    [Header("Letter Settings")]
    public char letter = 'A';
    public TMP_Text textMesh; // สำหรับแสดงตัวอักษรบนวัตถุ
    public float rotateSpeed = 90f;

    void Start()
    {
        UpdateVisuals();
    }

    void Update()
    {
        // แอนิเมชันหมุนเหรียญ
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    public void SetLetter(char newLetter)
    {
        letter = char.ToUpper(newLetter);
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (textMesh != null)
        {
            textMesh.text = letter.ToString();
        }
    }

    /* ==========================================================================
       SECTION 2: TRIGGER DETECTION (ตรวจจับการชนกับผู้เล่น)
       ทำหน้าที่: เพิ่มตัวอักษรลงกระเป๋าและทำลาย Object เหรียญ
       ========================================================================== */
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LetterInventory inv = other.GetComponent<LetterInventory>();
            if (inv != null)
            {
                inv.AddLetter(letter);
            }

            // TODO: เล่นเสียง Sound FX เก็บของ หรือ Particle Effect
            Destroy(gameObject);
        }
    }
}
