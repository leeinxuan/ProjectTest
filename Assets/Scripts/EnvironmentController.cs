using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class EnvironmentController : MonoBehaviour
{
    [Header("XR 控制組件")]
    public XRLever lever;
    public XRKnob knob;
    
    [Header("移動參數")]
    public float forwardSpeed = 3f;
    public float sideSpeed = 1f;
    
    [Header("XR Car 設置")]
    public Transform xrCarObject;
    public Transform xrOrigin;

    //private bool wasOn;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("EnvironmentController: Start方法開始執行");
        // 自動查找XRCar物件
        if (xrCarObject == null)
        {
            GameObject xrCar = GameObject.Find("XRCar");
            if (xrCar != null)
            {
                xrCarObject = xrCar.transform;
                Debug.Log("EnvironmentController: 自動找到XRCar物件");
            }
        }
        
        // 自動查找XR Origin
        if (xrOrigin == null)
        {
            GameObject xrOriginObj = GameObject.Find("XR Origin (XR Rig)");
            if (xrOriginObj != null)
            {
                xrOrigin = xrOriginObj.transform;
                Debug.Log("EnvironmentController: 自動找到XR Origin");
            }
        }
        
        // 自動查找控制組件
        if (lever == null)
        {
            lever = FindObjectOfType<XRLever>();
            if (lever != null)
                Debug.Log("EnvironmentController: 自動找到XRLever");
        }
        
        if (knob == null)
        {
            knob = FindObjectOfType<XRKnob>();
            if (knob != null)
                Debug.Log("EnvironmentController: 自動找到XRKnob");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lever == null || knob == null)
            return;
            
        // 計算移動速度
        float forwardVelocity = -forwardSpeed * lever.value;
        float sideVelocity = sideSpeed * lever.value * Mathf.Lerp(1,-1,knob.value);

        Vector3 velocity = new Vector3(sideVelocity,0,forwardVelocity);
        transform.position += velocity * Time.deltaTime;
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
