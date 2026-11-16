using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Lectura de controles compatible con el Input System nuevo (Unity 6 por defecto)
/// y con el Input Manager clásico. Teclado + mando.
/// </summary>
public static class NullInput
{
#if ENABLE_INPUT_SYSTEM
    static Keyboard K => Keyboard.current;
    static Gamepad P => Gamepad.current;

    public static float Horizontal()
    {
        float x = 0f;
        if (K != null)
        {
            if (K.aKey.isPressed || K.leftArrowKey.isPressed) x -= 1f;
            if (K.dKey.isPressed || K.rightArrowKey.isPressed) x += 1f;
        }
        if (P != null)
        {
            float stick = P.leftStick.x.ReadValue();
            if (Mathf.Abs(stick) > 0.3f) x = Mathf.Sign(stick);
            if (P.dpad.left.isPressed) x = -1f;
            if (P.dpad.right.isPressed) x = 1f;
        }
        return Mathf.Clamp(x, -1f, 1f);
    }

    public static bool JumpDown() =>
        (K != null && (K.spaceKey.wasPressedThisFrame || K.wKey.wasPressedThisFrame || K.upArrowKey.wasPressedThisFrame)) ||
        (P != null && P.buttonSouth.wasPressedThisFrame);

    public static bool JumpUp() =>
        (K != null && (K.spaceKey.wasReleasedThisFrame || K.wKey.wasReleasedThisFrame || K.upArrowKey.wasReleasedThisFrame)) ||
        (P != null && P.buttonSouth.wasReleasedThisFrame);

    public static bool DashDown() =>
        (K != null && (K.leftShiftKey.wasPressedThisFrame || K.kKey.wasPressedThisFrame)) ||
        (P != null && (P.buttonWest.wasPressedThisFrame || P.rightShoulder.wasPressedThisFrame));

    public static bool Channel1() => (K != null && K.digit1Key.wasPressedThisFrame) || (P != null && P.dpad.up.wasPressedThisFrame);
    public static bool Channel2() => (K != null && K.digit2Key.wasPressedThisFrame) || (P != null && P.buttonNorth.wasPressedThisFrame);
    public static bool Channel3() => (K != null && K.digit3Key.wasPressedThisFrame) || (P != null && P.buttonEast.wasPressedThisFrame);

    public static bool ChannelPrev() => (K != null && K.qKey.wasPressedThisFrame);
    public static bool ChannelNext() =>
        (K != null && K.eKey.wasPressedThisFrame) || (P != null && P.leftShoulder.wasPressedThisFrame);

    public static bool ToggleLight() =>
        (K != null && K.lKey.wasPressedThisFrame) || (P != null && P.selectButton.wasPressedThisFrame);

    public static bool Phase(int i)
    {
        if (K == null) return false;
        switch (i)
        {
            case 0: return K.f1Key.wasPressedThisFrame;
            case 1: return K.f2Key.wasPressedThisFrame;
            case 2: return K.f3Key.wasPressedThisFrame;
            default: return K.f4Key.wasPressedThisFrame;
        }
    }
#else
    public static float Horizontal() => Input.GetAxisRaw("Horizontal");
    public static bool JumpDown() => Input.GetButtonDown("Jump");
    public static bool JumpUp() => Input.GetButtonUp("Jump");
    public static bool DashDown() => Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K);
    public static bool Channel1() => Input.GetKeyDown(KeyCode.Alpha1);
    public static bool Channel2() => Input.GetKeyDown(KeyCode.Alpha2);
    public static bool Channel3() => Input.GetKeyDown(KeyCode.Alpha3);
    public static bool ChannelPrev() => Input.GetKeyDown(KeyCode.Q);
    public static bool ChannelNext() => Input.GetKeyDown(KeyCode.E);
    public static bool ToggleLight() => Input.GetKeyDown(KeyCode.L);
    public static bool Phase(int i) => Input.GetKeyDown(KeyCode.F1 + i);
#endif
}
