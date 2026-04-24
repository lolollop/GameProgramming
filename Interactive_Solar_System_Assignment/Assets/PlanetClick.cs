using UnityEngine;

public class PlanetClick : MonoBehaviour
{
    public string funFact; // 小知识文本

    void OnMouseDown()
    {
        // 点击时，告诉主摄像机看向我
        Camera.main.GetComponent<CameraControl>().SetTarget(this.transform, funFact);

        // 视觉反馈：点击时闪一下红色
        GetComponent<MeshRenderer>().material.color = Color.red;
        Invoke("ResetColor", 0.2f);
    }

    void ResetColor()
    {
        // 这里可以改回原本颜色，或者简单点就不写
    }
}