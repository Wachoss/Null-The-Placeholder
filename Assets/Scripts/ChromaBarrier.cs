using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Barrera de color sólida (Sector 1 — Chroma Bus).
/// Bloquea a Null salvo cuando hace Chromatic Dash con el mismo canal.
/// Collider2D NO trigger, en una capa que NO esté en groundMask para que no sirva de pared para wall jump
/// (o sí, si queréis que se pueda trepar: es decisión de diseño).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ChromaBarrier : MonoBehaviour
{
    public ChromaChannel channel = ChromaChannel.Magenta;

    static readonly List<ChromaBarrier> all = new List<ChromaBarrier>();
    Collider2D col;
    SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = NullController2D.ToColor(channel);
    }

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    /// <summary>El jugador llama a esto al empezar/terminar el dash.</summary>
    public static void SetPassThrough(Collider2D player, ChromaChannel playerChannel, bool pass)
    {
        foreach (var b in all)
        {
            bool match = b.channel == playerChannel;
            Physics2D.IgnoreCollision(player, b.col, pass && match);
            if (b.sr)
            {
                var c = b.sr.color;
                c.a = (pass && match) ? 0.35f : 1f; // feedback visual: se vuelve translúcida
                b.sr.color = c;
            }
        }
    }
}
