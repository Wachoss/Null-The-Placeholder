using System.Collections;
using UnityEngine;

/// <summary>
/// One-Hit Kill + Instant Respawn (&lt; 150 ms) en el umbral de entrada de la sala.
/// Va en el mismo GameObject que NullController2D.
/// </summary>
[RequireComponent(typeof(NullController2D))]
public class NullRespawn : MonoBehaviour
{
    public float respawnDelay = 0.12f;   // < 150 ms según el Ten-Pager
    public ParticleSystem deathParticles; // opcional: "glitch" al morir
    public Transform startPoint;          // si no hay, usa la posición inicial

    public Vector3 RespawnPoint { get; private set; }
    public bool IsDead { get; private set; }
    public int DeathCount { get; private set; }

    NullController2D controller;
    Rigidbody2D rb;
    SpriteRenderer sr;

    void Awake()
    {
        controller = GetComponent<NullController2D>();
        rb = GetComponent<Rigidbody2D>();
        sr = controller.spriteRenderer != null ? controller.spriteRenderer : GetComponentInChildren<SpriteRenderer>();
        RespawnPoint = startPoint != null ? startPoint.position : transform.position;
    }

    public void SetRespawnPoint(Vector3 p) => RespawnPoint = p;

    /// <summary>Llamado por Hazard, enemigos o la KillZone inferior.</summary>
    public void Kill()
    {
        if (IsDead) return;
        StartCoroutine(DieAndRespawn());
    }

    IEnumerator DieAndRespawn()
    {
        IsDead = true;
        DeathCount++;
        if (deathParticles) Instantiate(deathParticles, transform.position, Quaternion.identity).Play();

        controller.ResetState();
        controller.InputLocked = true;
        rb.simulated = false;
        if (sr) sr.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = RespawnPoint;
        rb.simulated = true;
        if (sr) sr.enabled = true;
        controller.InputLocked = false;
        IsDead = false;
    }
}
