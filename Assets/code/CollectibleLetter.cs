using UnityEngine;
using TMPro; // ใช้สำหรับแสดง 3D Text บนเหรียญ

/**
 * ==============================================================================
 * COLLECTIBLE LETTER (เหรียญตัวอักษรที่ลอยให้เก็บ)
 * ทำหน้าที่: หมุนตัวอักษร และส่งค่าตัวอักษรเข้า Inventory เมื่อผู้เล่นวิ่งชน
 * ==============================================================================
 */
[RequireComponent(typeof(ScrollLeft))]
public class CollectibleLetter : MonoBehaviour
{
    /* ==========================================================================
       SECTION 1: LETTER DATA & VISUALS (ข้อมูลตัวอักษรและการแสดงผล)
       ทำหน้าที่: กำหนดตัวอักษร A-Z, รองรับโมเดล 3D และอนิเมชันหมุน/ลอยขึ้นลง
       ========================================================================== */
    [Header("Letter Settings")]
    public char letter = 'A';
    public TMP_Text textMesh; // สำหรับแสดงตัวอักษรบนวัตถุ (TextMeshPro 3D)

    [Header("3D Visuals & Animation")]
    [Tooltip("โมเดลลูกสำหรับหมุน (หากเว้นว่างไว้ จะหมุน Transform หลักทั้งหมด)")]
    public Transform visualModel;
    public float rotateSpeed = 90f;

    [Header("Floating / Bobbing Effect")]
    [Tooltip("เปิดเอฟเฟกต์ลอยขึ้นลงเบาๆ ให้เห็นเด่นชัด")]
    public bool enableBobbing = true;
    public float bobSpeed = 3f;
    public float bobHeight = 0.25f;

    private Vector3 initialLocalPos;
    private float baseY;
    private bool isChildModel = false;

    void Start()
    {
        // ถ้าไม่ได้ผูกโมเดลเฉพาะ ให้ลองหาในโหนดลูกก่อน
        if (visualModel == null)
        {
            if (transform.childCount > 0)
            {
                visualModel = transform.GetChild(0);
            }
            else
            {
                visualModel = transform;
            }
        }

        isChildModel = (visualModel != null && visualModel != transform);

        if (isChildModel)
        {
            initialLocalPos = visualModel.localPosition;
        }
        else
        {
            baseY = transform.position.y;
        }

        // ค้นหา TextMeshPro ในลูกอัตโนมัติหากยังไม่ได้ลากมาใส่
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TMP_Text>();
        }

        UpdateVisuals();
    }

    void Update()
    {
        if (isChildModel && visualModel != null)
        {
            // 1. โมเดลลูก: หมุนและปรับ Local Position โดยไม่รบกวน ScrollLeft
            visualModel.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.Self);

            if (enableBobbing)
            {
                float newY = initialLocalPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                visualModel.localPosition = new Vector3(initialLocalPos.x, newY, initialLocalPos.z);
            }
        }
        else
        {
            // 2. โมเดลแม่: หมุนและปรับเฉพาะแกน Y เท่านั้น เพื่อรักษาแกน X ให้ ScrollLeft เคลื่อนที่
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);

            if (enableBobbing)
            {
                float newY = baseY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                Vector3 curPos = transform.position;
                curPos.y = newY;
                transform.position = curPos;
            }
        }
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
