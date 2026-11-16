using UnityEngine;

/// <summary>
/// Fases de compilación de Null (0 Placeholder, 1 Chroma, 2 Lumina, 3 Full).
/// Cambia el aspecto con un AnimatorOverrideController por fase y activa los
/// objetos extra de cada fase (luces 2D, partículas...). Opcionalmente aplica
/// los desbloqueos del Ten-Pager. F1..F4 cambia de fase para probar / grabar el vídeo.
/// </summary>
[RequireComponent(typeof(NullController2D))]
public class NullPhase : MonoBehaviour
{
    [Range(0, 3)] public int phase = 0;

    [Tooltip("Un override por fase (0..3). El 0 puede quedar vacío y usa el controller base.")]
    public AnimatorOverrideController[] phaseOverrides = new AnimatorOverrideController[4];

    [Tooltip("Objetos que se activan desde esa fase en adelante: [2] halo y foco, [3] bloom/estela...")]
    public GameObject[] phaseExtras = new GameObject[4];

    [Tooltip("Si está activo: Wall Jump desde Fase 1 y Dash desde Fase 2, como en el Ten-Pager.")]
    public bool applyUnlocks = false;
    public bool debugKeys = true;

    NullController2D controller;
    RuntimeAnimatorController baseController;

    void Awake()
    {
        controller = GetComponent<NullController2D>();
        if (controller.animator) baseController = controller.animator.runtimeAnimatorController;
    }

    void Start() => SetPhase(phase);

    void Update()
    {
        if (!debugKeys) return;
        if (NullInput.Phase(0)) SetPhase(0);
        if (NullInput.Phase(1)) SetPhase(1);
        if (NullInput.Phase(2)) SetPhase(2);
        if (NullInput.Phase(3)) SetPhase(3);
    }

    /// <summary>Llamar desde la terminal de fin de sector (p.ej. &lt;Return 0&gt;).</summary>
    public void SetPhase(int newPhase)
    {
        phase = Mathf.Clamp(newPhase, 0, 3);

        var anim = controller.animator;
        if (anim)
        {
            var ov = phase < phaseOverrides.Length ? phaseOverrides[phase] : null;
            anim.runtimeAnimatorController = ov != null ? ov : baseController;
        }

        for (int i = 0; i < phaseExtras.Length; i++)
            if (phaseExtras[i]) phaseExtras[i].SetActive(phase >= i);

        if (applyUnlocks)
        {
            controller.wallJumpUnlocked = phase >= 1;
            controller.dashUnlocked = phase >= 2;
        }
    }
}
