using UnityEngine;
using UnityEngine.InputSystem;

public class AirplaneVehicle : MonoBehaviour
{
    [SerializeField]
    float Speed = 50;
    [SerializeField]
    float MinY = 100;
    [SerializeField]
    InputActionReference rotateAction = null;
    Vector2 m_v2Rotate = new Vector2();
    [SerializeField]
    float YawScale = 1.0f;
    [SerializeField]
    float PitchScale = 1.0f;
    [SerializeField]
    GameObject SendOrientation = null;
    [SerializeField]
    GameObject RecvOrientation = null;

    private void Awake()
    {
        rotateAction.action.performed += Rotate_started;
        rotateAction.action.canceled += Rotate_canceled;
    }

    private void OnDestroy()
    {
        rotateAction.action.performed -= Rotate_started;
        rotateAction.action.canceled -= Rotate_canceled;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateMinHeight();
    }

    private void Rotate_started(InputAction.CallbackContext obj)
    {
        m_v2Rotate = obj.ReadValue<Vector2>();
    }

    private void Rotate_canceled(InputAction.CallbackContext obj)
    {
        m_v2Rotate = new Vector2();
    }

    // Update is called once per frame
    void Update()
    {
        // pitch
        float fPitch = m_v2Rotate.y * Time.deltaTime * PitchScale;
        Quaternion quatPitch = Quaternion.AngleAxis(fPitch, SendOrientation.transform.right);

        // negative pitch (force)
        Vector3 v3Forward0 = new Vector3(SendOrientation.transform.forward.x, 0.0f, SendOrientation.transform.forward.z);
        v3Forward0.Normalize();
        float fPitchNegativeVelocity = 0.0f;
        if (false == float.IsNaN(v3Forward0.x) && false == float.IsNaN(v3Forward0.y) && false == float.IsNaN(v3Forward0.z))
        {
            float fForce = Vector3.SignedAngle(v3Forward0, SendOrientation.transform.forward, SendOrientation.transform.right);
            fPitchNegativeVelocity = -fForce * 2.5f * Time.deltaTime;
        }
        Quaternion quatNegativePitch = Quaternion.AngleAxis(fPitchNegativeVelocity, SendOrientation.transform.right);

        // yaw
        float fYaw = m_v2Rotate.x * Time.deltaTime * YawScale;
        Quaternion quatYaw = Quaternion.AngleAxis(fYaw, (SendOrientation.transform.up.y > 0.0f) ? (Vector3.up) : (-Vector3.up));

        // roll
        float fRoll = -m_v2Rotate.x * Time.deltaTime * YawScale;
        Quaternion quatRoll = Quaternion.AngleAxis(fRoll, SendOrientation.transform.forward);

        // negative roll (force)
        Vector3 v3Right0 = new Vector3(SendOrientation.transform.right.x, 0.0f, SendOrientation.transform.right.z).normalized;
        float fRollNegativeVelocity = 0.0f;
        if (false == float.IsNaN(v3Right0.x) && false == float.IsNaN(v3Right0.y) && false == float.IsNaN(v3Right0.z))
        {
            float fForce = Vector3.SignedAngle(v3Right0, SendOrientation.transform.right, SendOrientation.transform.forward);
            fRollNegativeVelocity = -fForce * 2.5f * Time.deltaTime;
        }
        Quaternion quatNegativeRoll = Quaternion.AngleAxis(fRollNegativeVelocity, SendOrientation.transform.forward);

        // rotate
        Quaternion localRotate2 = (quatNegativeRoll * quatRoll * quatNegativePitch * quatPitch * quatYaw * SendOrientation.transform.localRotation).normalized;

        // send rotate
        SendOrientation.transform.localRotation = Quaternion.Lerp(SendOrientation.transform.localRotation, localRotate2, 0.333f);
        // recv rotate
        this.transform.localRotation = RecvOrientation.transform.localRotation;

        // translate
        Vector3 v3Velocity = SendOrientation.transform.forward * Speed;
        Vector3 v3StepPosition = v3Velocity * Time.deltaTime;
        this.transform.position += v3StepPosition;
        UpdateMinHeight();
    }

    void UpdateMinHeight()
    {
        Vector3 pos = this.transform.position;
        if (pos.y < MinY)
        {
            pos.y = MinY;
            this.transform.position = pos;
        }
    }
}
