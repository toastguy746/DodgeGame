using UnityEngine;

public class HealthBarBillboard : MonoBehaviour
{
    public float size = 0.1f;  // 화면에서 보이는 고정 크기 비율

    void LateUpdate()
    {
        // 1. 카메라 바라보기
        transform.forward = Camera.main.transform.forward;

        // 2. 화면 크기 일정하게 조절
        float dist = Vector3.Distance(Camera.main.transform.position, transform.position);
        float scale = dist * size;
        transform.localScale = Vector3.one * scale;
    }
}
