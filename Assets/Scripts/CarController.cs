using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class CarController : MonoBehaviour
{
    [Header("XR 控制組件")]
    public XRLever lever;
    public XRKnob knob;
    
    [Header("移動參數")]
    public float forwardSpeed = 3f;
    public float turnSpeed = 20f;
    
    [Header("XR Car 設置")]
    public Transform xrCarObject;
    public Transform xrOrigin;

    //private bool wasOn;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("CarController: Start方法開始執行");
        // 自動查找XRCar物件
        if (xrCarObject == null)
        {
            GameObject xrCar = GameObject.Find("XRCar");
            if (xrCar != null)
            {
                xrCarObject = xrCar.transform;
                Debug.Log("CarController: 自動找到XRCar物件");
            }
        }
        
        // 自動查找XR Origin
        if (xrOrigin == null)
        {
            GameObject xrOriginObj = GameObject.Find("XR Origin (XR Rig)");
            if (xrOriginObj != null)
            {
                xrOrigin = xrOriginObj.transform;
                Debug.Log("CarController: 自動找到XR Origin");
            }
        }
        
        // 自動查找控制組件
        if (lever == null)
        {
            lever = FindObjectOfType<XRLever>();
            if (lever != null)
                Debug.Log("CarController: 自動找到XRLever");
        }
        
        if (knob == null)
        {
            knob = FindObjectOfType<XRKnob>();
            if (knob != null)
                Debug.Log("CarController: 自動找到XRKnob");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lever == null || knob == null)
            return;
            
        // lever.value: 0.0=後退, 0.5=停止, 1.0=前進
        // 節流量（速度比例 0~1）
        float throttle = Mathf.Abs(lever.value - 0.5f) * 2f;
        // 帶符號的前進速度（世界單位/秒）
        float forwardVelocity = -forwardSpeed * (lever.value - 0.5f) * 2f;
        // 旋鈕輸入映射為 -1（右）到 1（左），決定轉向方向
        float turnInput = Mathf.Lerp(-1f, 1f, knob.value);
        // 本幀旋轉角度（度），隨油門比例縮放
        float yawDelta = turnSpeed * turnInput * throttle * Time.deltaTime;

        // 應用轉向（本地 Y 軸）
        transform.Rotate(0f, yawDelta, 0f, Space.Self);
        // 應用前進（沿物件前方）
        transform.position += transform.forward * forwardVelocity * Time.deltaTime;
        // // 移動XRCar物件
        // if (xrCarObject != null)
        // {
        //     xrCarObject.position += velocity * Time.deltaTime;
            
        //     // 同步XR Origin位置
        //     if (xrOrigin != null)
        //     {
        //         xrOrigin.position = xrCarObject.position;
        //     }
        // }
        // else
        // {
        //     // 如果沒有XRCar物件，移動當前物件
        //     transform.position += velocity * Time.deltaTime;
        // }

        // // 音效控制
        // if(lever.value != wasOn)
        // {
        //     if(lever.value)
        //     {
        //         // AudioManager.instance.Play("Engine");
        //     }
        //     else
        //     {
        //         // AudioManager.instance.Stop("Engine");
        //     }
        // }
        
        // wasOn = lever.value;
    }
}