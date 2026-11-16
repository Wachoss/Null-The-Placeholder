using System.Reflection;
using UnityEngine;

/// <summary>
/// Penumbra del Sector 2 (Lumina Layer).
/// - Baja la intensidad de la luz global automáticamente cuando Null está en la Fase 2.
/// - La tecla L (o Select/View en mando) la apaga / enciende a mano para probar.
/// Va en Null (junto a NullPhase). Si no asignas la luz, busca el objeto "Global Light 2D".
/// No depende del paquete URP en el código: funciona con cualquier luz que tenga "intensity".
/// </summary>
public class GlobalDarkness : MonoBehaviour
{
    [Tooltip("El componente Light 2D del objeto 'Global Light 2D'. Se busca solo si lo dejas vacío.")]
    public Behaviour globalLight;
    public float normalIntensity = 1f;
    public float darkIntensity = 0.15f;
    [Tooltip("Oscurecer automáticamente en la Fase 2 (Lumina).")]
    public bool darkInPhase2 = true;
    [Tooltip("Oscurecer también en la Fase 3.")]
    public bool darkInPhase3 = false;
    public float fadeSpeed = 3f;

    NullPhase phase;
    PropertyInfo intensityProp;
    bool manualDark;      // estado de la tecla L
    int lastPhase = -1;

    void Awake()
    {
        phase = GetComponent<NullPhase>();
        if (globalLight == null)
        {
            var go = GameObject.Find("Global Light 2D");
            if (go)
                foreach (var b in go.GetComponents<Behaviour>())
                    if (b.GetType().Name == "Light2D") { globalLight = b; break; }
        }
        if (globalLight) intensityProp = globalLight.GetType().GetProperty("intensity");
        if (intensityProp == null) Debug.LogWarning("GlobalDarkness: no encuentro la Global Light 2D (¿proyecto Universal 2D?).");
        else normalIntensity = (float)intensityProp.GetValue(globalLight);
    }

    void Update()
    {
        if (intensityProp == null) return;

        int p = phase ? phase.phase : 0;
        if (p != lastPhase)   // al cambiar de fase, el modo automático manda
        {
            manualDark = (p == 2 && darkInPhase2) || (p == 3 && darkInPhase3);
            lastPhase = p;
        }
        if (NullInput.ToggleLight()) manualDark = !manualDark;

        float target = manualDark ? darkIntensity : normalIntensity;
        float current = (float)intensityProp.GetValue(globalLight);
        intensityProp.SetValue(globalLight, Mathf.MoveTowards(current, target, fadeSpeed * Time.deltaTime));
    }
}
