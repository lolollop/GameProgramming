using UnityEngine;
using TMPro; // 如果你用了 TextMeshPro

public class CameraControl : MonoBehaviour
{
    private Transform target;
    public Vector3 originalPos = new Vector3(0, 10, -20);

    void Start() { transform.position = originalPos; }

    public void SetTarget(Transform newTarget, string fact)
    {
        target = newTarget;
        Debug.Log(fact); // 在控制台显示小知识
    }

    void Update()
    {
        if (target != null)
        {
            // 平滑跟踪目标
            Vector3 desiredPos = target.position + new Vector3(0, 2, -5);
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 5f);
            transform.LookAt(target);
        }
    }

    // 给 UI 按钮调用的方法，返回主视图
    public void ResetView()
    {
        target = null;
        transform.position = originalPos;
        transform.rotation = Quaternion.Euler(20, 0, 0); // 初始角度
    }
}