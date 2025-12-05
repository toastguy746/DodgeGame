using UnityEngine;

public class MapBoundsChecker : MonoBehaviour
{
    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("맵 안에 Renderer가 없습니다.");
            return;
        }

        Vector3 min = renderers[0].bounds.min;
        Vector3 max = renderers[0].bounds.max;

        foreach (Renderer rend in renderers)
        {
            min = Vector3.Min(min, rend.bounds.min);
            max = Vector3.Max(max, rend.bounds.max);
        }

        Debug.Log("MinX: " + min.x + ", MaxX: " + max.x);
        Debug.Log("MinZ: " + min.z + ", MaxZ: " + max.z);
    }
}
