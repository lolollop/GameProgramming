using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetOrbit : MonoBehaviour
{

    [Header("旋转速度设置")]
    [Tooltip("数值越大转得越快")]
    public float speed = 20f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Vector3.up 表示绕 Y 轴（上下轴）旋转
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}
