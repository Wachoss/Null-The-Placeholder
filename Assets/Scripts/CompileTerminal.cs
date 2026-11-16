using System.Collections;
using UnityEngine;

/// <summary>
/// Terminal de fin de sector (p. ej. &lt;Return 0&gt;). Cuando Null la toca,
/// "compila" a la fase indicada (desbloqueando la habilidad si NullPhase tiene Apply Unlocks).
/// Da feedback visual (destello + pulso) y escribe en la Console qué ha pasado.
/// Collider2D en modo Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CompileTerminal : MonoBehaviour
{
    [Range(1, 3)] public int targetPhase = 1;
    public ParticleSystem activateParticles; // opcional
    public Color usedColor = new Color(0.4f, 1f, 0.4f);
    bool used;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void Start()
    {
        var col = GetComponent<Collider2D>();
        if (!col.isTrigger)
            Debug.LogWarning($"CompileTerminal ({name}): el Collider 2D no tiene 'Is Trigger' marcado; Null chocará con ella en vez de activarla.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used) return;

        // Busca a Null aunque el collider que entra sea de un hijo
        var controller = other.GetComponentInParent<NullController2D>();
        if (controller == null) return;

        var phase = controller.GetComponent<NullPhase>();
        if (phase == null)
        {
            Debug.LogWarning("CompileTerminal: Null no tiene el componente 'Null Phase'. Añádelo (Add Component → Null Phase) para que la terminal cambie de fase.");
            StartCoroutine(Feedback());
            used = true;
            return;
        }

        used = true;
        int before = phase.phase;
        if (phase.phase < targetPhase) phase.SetPhase(targetPhase);
        Debug.Log($"CompileTerminal: Null compilado de Fase {before} a Fase {phase.phase}.");

        if (phase.phase < phase.phaseOverrides.Length && phase.phaseOverrides[phase.phase] == null)
            Debug.LogWarning($"CompileTerminal: la fase {phase.phase} no tiene Animator Override asignado en Null Phase → Phase Overrides, así que el aspecto de Null no cambia.");

        if (activateParticles) activateParticles.Play();
        StartCoroutine(Feedback());
    }

    IEnumerator Feedback()
    {
        var sr = GetComponent<SpriteRenderer>();
        Vector3 s0 = transform.localScale;
        for (float t = 0; t < 0.4f; t += Time.deltaTime)
        {
            float k = t / 0.4f;
            transform.localScale = s0 * (1f + 0.25f * Mathf.Sin(k * Mathf.PI));
            if (sr) sr.color = (Mathf.FloorToInt(t * 20) % 2 == 0) ? Color.white * 2f : usedColor;
            yield return null;
        }
        transform.localScale = s0;
        if (sr) sr.color = usedColor; // terminal ejecutada
    }
}
