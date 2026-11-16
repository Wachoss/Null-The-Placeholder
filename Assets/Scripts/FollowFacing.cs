using UnityEngine;

/// <summary>
/// Hace que un hijo de Null (p. ej. el foco Spot Light 2D de la Fase 2) mire
/// hacia donde mira Null. Ponlo en el objeto del foco.
/// </summary>
public class FollowFacing : MonoBehaviour
{
    public NullController2D controller;
    [Tooltip("Rotación Z cuando Null mira a la derecha (un Spot Light 2D apunta hacia arriba por defecto, así que -90 lo pone mirando a la derecha).")]
    public float rightAngleZ = -90f;

    void Awake()
    {
        if (controller == null) controller = GetComponentInParent<NullController2D>();
    }

    void LateUpdate()
    {
        if (controller == null) return;
        float z = controller.FacingDir > 0 ? rightAngleZ : -rightAngleZ;
        transform.localRotation = Quaternion.Euler(0f, 0f, z);
    }
}
