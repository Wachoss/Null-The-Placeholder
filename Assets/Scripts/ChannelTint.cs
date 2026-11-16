using System.Reflection;
using UnityEngine;

/// <summary>
/// Tiñe este objeto del color del canal activo de Null (cian / magenta / amarillo).
/// Sirve para: Light 2D (halo, foco), Particle System y Sprite Renderer.
/// Ponlo en cada objeto que quieras que cambie de color (hijo de Null).
/// </summary>
public class ChannelTint : MonoBehaviour
{
    public NullController2D controller;
    [Tooltip("Velocidad del cambio de color (más alto = más rápido).")]
    public float blendSpeed = 12f;

    Behaviour light2D;
    PropertyInfo colorProp;
    ParticleSystem ps;
    SpriteRenderer sr;
    Color current;

    void Awake()
    {
        if (controller == null) controller = GetComponentInParent<NullController2D>();
        foreach (var b in GetComponents<Behaviour>())
            if (b.GetType().Name == "Light2D") { light2D = b; colorProp = b.GetType().GetProperty("color"); break; }
        ps = GetComponent<ParticleSystem>();
        sr = GetComponent<SpriteRenderer>();
        if (controller) current = NullController2D.ToColor(controller.currentChannel);
        Apply(current);
    }

    void LateUpdate()
    {
        if (controller == null) return;
        Color target = NullController2D.ToColor(controller.currentChannel);
        current = Color.Lerp(current, target, 1f - Mathf.Exp(-blendSpeed * Time.deltaTime));
        Apply(current);
    }

    void Apply(Color c)
    {
        if (colorProp != null) colorProp.SetValue(light2D, c);
        if (ps)
        {
            var main = ps.main;
            main.startColor = c; // las partículas nuevas salen del color del canal
        }
        if (sr) sr.color = c;
    }
}
