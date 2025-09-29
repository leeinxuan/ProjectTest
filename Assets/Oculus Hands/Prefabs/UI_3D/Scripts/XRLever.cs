using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// An interactable lever that snaps into an on or off position by a direct interactor
    /// </summary>
    public class XRLever : XRBaseInteractable
    {
        const float k_LeverDeadZone = 0.1f; // Prevents rapid switching between on and off states when right in the middle

        [SerializeField]
        [Tooltip("The object that is visually grabbed and manipulated")]
        Transform m_Handle = null;

        [SerializeField]
        [Tooltip("The value of the lever (0.0 = off, 1.0 = on)")]
        float m_Value = 0.5f;

        [SerializeField]
        [Tooltip("If enabled, the lever will snap to the value position when released")]
        bool m_LockToValue;

        [SerializeField]
        [Tooltip("Angle of the lever in the 'on' position")]
        [Range(-90.0f, 90.0f)]
        float m_MaxAngle = 90.0f;

        [SerializeField]
        [Tooltip("Angle of the lever in the 'off' position")]
        [Range(-90.0f, 90.0f)]
        float m_MinAngle = -90.0f;

        [SerializeField]
        [Tooltip("Events to trigger when the lever activates")]
        UnityEvent m_OnLeverActivate = new UnityEvent();

        [SerializeField]
        [Tooltip("Events to trigger when the lever deactivates")]
        UnityEvent m_OnLeverDeactivate = new UnityEvent();

        IXRSelectInteractor m_Interactor;

        /// <summary>
        /// The object that is visually grabbed and manipulated
        /// </summary>
        public Transform handle
        {
            get => m_Handle;
            set => m_Handle = value;
        }

        /// <summary>
        /// The value of the lever (0.0 = off, 1.0 = on)
        /// </summary>
        public float value
        {
            get => m_Value;
            set => SetValue(Mathf.Clamp01(value), true);
        }

        /// <summary>
        /// If enabled, the lever will snap to the value position when released
        /// </summary>
        public bool lockToValue { get; set; }

        /// <summary>
        /// Angle of the lever in the 'on' position
        /// </summary>
        public float maxAngle
        {
            get => m_MaxAngle;
            set => m_MaxAngle = value;
        }

        /// <summary>
        /// Angle of the lever in the 'off' position
        /// </summary>
        public float minAngle
        {
            get => m_MinAngle;
            set => m_MinAngle = value;
        }

        /// <summary>
        /// Events to trigger when the lever activates
        /// </summary>
        public UnityEvent onLeverActivate => m_OnLeverActivate;

        /// <summary>
        /// Events to trigger when the lever deactivates
        /// </summary>
        public UnityEvent onLeverDeactivate => m_OnLeverDeactivate;

        void Start()
        {
            SetValue(m_Value, true);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            selectEntered.AddListener(StartGrab);
            selectExited.AddListener(EndGrab);
        }

        protected override void OnDisable()
        {
            selectEntered.RemoveListener(StartGrab);
            selectExited.RemoveListener(EndGrab);
            base.OnDisable();
        }

        void StartGrab(SelectEnterEventArgs args)
        {
            m_Interactor = args.interactorObject;
        }

        void EndGrab(SelectExitEventArgs args)
        {
            // 當釋放桿子時，將桿子吸附到對應的固定角度
            SnapToDiscretePosition();
            m_Interactor = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (isSelected)
                {
                    UpdateValue();
                }
            }
        }

        Vector3 GetLookDirection()
        {
            Vector3 direction = m_Interactor.GetAttachTransform(this).position - m_Handle.position;
            direction = transform.InverseTransformDirection(direction);
            direction.x = 0;

            return direction.normalized;
        }

        void UpdateValue()
        {
            var lookDirection = GetLookDirection();
            var lookAngle = Mathf.Atan2(lookDirection.z, lookDirection.y) * Mathf.Rad2Deg;

            if (m_MinAngle < m_MaxAngle)
                lookAngle = Mathf.Clamp(lookAngle, m_MinAngle, m_MaxAngle);
            else
                lookAngle = Mathf.Clamp(lookAngle, m_MaxAngle, m_MinAngle);

            // 計算角度範圍的三個區域
            float angleRange = Mathf.Abs(m_MaxAngle - m_MinAngle);
            float normalizedAngle;
            
            if (m_MinAngle < m_MaxAngle)
            {
                normalizedAngle = Mathf.InverseLerp(m_MinAngle, m_MaxAngle, lookAngle);
            }
            else
            {
                normalizedAngle = Mathf.InverseLerp(m_MaxAngle, m_MinAngle, lookAngle);
            }

            // 將連續值轉換為三個固定值：0, 0.5, 1
            float discreteValue;
            if (normalizedAngle < 0.33f)
                discreteValue = 0f;      // 後退
            else if (normalizedAngle < 0.67f)
                discreteValue = 0.5f;    // 停止
            else
                discreteValue = 1f;      // 前進

            // 設置桿子到對應的固定角度，而不是跟隨滑鼠位置
            SetValue(discreteValue, true);
        }

        void SetValue(float newValue, bool forceRotation = false)
        {
            newValue = Mathf.Clamp01(newValue);
            
            if (Mathf.Approximately(m_Value, newValue))
            {
                if (forceRotation)
                {
                    SnapToDiscretePosition();
                }
                return;
            }

            float oldValue = m_Value;
            m_Value = newValue;

            // 觸發事件：只有當從非啟用狀態變為啟用狀態時才觸發啟用事件
            // 或者從啟用狀態變為非啟用狀態時觸發停用事件
            bool wasOn = oldValue > 0.5f;
            bool isNowOn = newValue > 0.5f;
            
            if (!wasOn && isNowOn)
            {
                m_OnLeverActivate.Invoke();
            }
            else if (wasOn && !isNowOn)
            {
                m_OnLeverDeactivate.Invoke();
            }

            if (!isSelected && (m_LockToValue || forceRotation))
            {
                SnapToDiscretePosition();
            }
        }

        void SnapToDiscretePosition()
        {
            float targetAngle;
            
            if (m_Value == 0f)
            {
                // 後退位置：最小角度
                targetAngle = m_MinAngle;
            }
            else if (m_Value == 0.5f)
            {
                // 停止位置：中間角度
                targetAngle = (m_MinAngle + m_MaxAngle) / 2f;
            }
            else // m_Value == 1f
            {
                // 前進位置：最大角度
                targetAngle = m_MaxAngle;
            }
            
            SetHandleAngle(targetAngle);
        }

        void SetHandleAngle(float angle)
        {
            if (m_Handle != null)
                m_Handle.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
        }

        void OnDrawGizmosSelected()
        {
            var angleStartPoint = transform.position;

            if (m_Handle != null)
                angleStartPoint = m_Handle.position;

            const float k_AngleLength = 0.25f;

            var angleMaxPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
            var angleMinPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MinAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(angleStartPoint, angleMaxPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(angleStartPoint, angleMinPoint);
        }

        void OnValidate()
        {
            SnapToDiscretePosition();
        }
    }
}
