using UnityEngine;

/// <summary>
/// Trampa de un golpe (pinchos, láser, KillZone bajo la sala...).
/// Si tiene canal de color, Null la atraviesa haciendo Chromatic Dash del mismo color.
/// Collider2D en modo Trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    public ChromaChannel channel = ChromaChannel.None;
    public bool tintSprite = true;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (tintSprite && sr && channel != ChromaChannel.None) sr.color = NullController2D.ToColor(channel);
    }

    void OnTriggerEnter2D(Collider2D other) => TryKill(other);
    void OnTriggerStay2D(Collider2D other) => TryKill(other); // por si termina el dash dentro

    void TryKill(Collider2D other)
    {
        var controller = other.GetComponent<NullController2D>();
        if (controller == null || controller.IsImmuneTo(channel)) return;
        var respawn = other.GetComponent<NullRespawn>();
        if (respawn) respawn.Kill();
    }
}
