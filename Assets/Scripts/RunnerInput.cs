using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-200)]
public sealed class RunnerInput : MonoBehaviour
{
    InputAction jump;
    InputAction dash;
    InputAction pause;
    InputAction capture;
    public bool JumpPressed { get { return jump != null && jump.WasPressedThisFrame(); } }
    public bool DashPressed { get { return dash != null && dash.WasPressedThisFrame(); } }
    public bool PausePressed { get { return pause != null && pause.WasPressedThisFrame(); } }
    public bool CapturePressed { get { return capture != null && capture.WasPressedThisFrame(); } }

    void Awake()
    {
        jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        dash = new InputAction("Dash", InputActionType.Button, "<Keyboard>/leftShift");
        dash.AddBinding("<Keyboard>/rightShift");
        pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        capture = new InputAction("Capture", InputActionType.Button, "<Keyboard>/f12");
    }

    void OnEnable()
    {
        jump?.Enable();
        dash?.Enable();
        pause?.Enable();
        capture?.Enable();
    }

    void OnDisable()
    {
        jump?.Disable();
        dash?.Disable();
        pause?.Disable();
        capture?.Disable();
    }

    void OnDestroy()
    {
        jump?.Dispose();
        dash?.Dispose();
        pause?.Dispose();
        capture?.Dispose();
    }
}
