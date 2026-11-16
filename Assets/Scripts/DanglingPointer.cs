using UnityEngine;

/// <summary>
/// Enemigo del Sector 0: centinela lineal en bucle entre dos puntos (FSM mínima: Patrol / Wait).
/// Toca = muerte (One-Hit Kill). Si Null le cae encima mientras cae, lo "libera" (rebote tipo stomp),
/// demostrando la interacción del salto con los enemigos.
/// Necesita un Collider2D trigger. Rigidbody2D Kinematic recomendado.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DanglingPointer : MonoBehaviour
{
    enum State { Patrol, Wait }

    public Transform pointA, pointB;
    public float speed = 3f;
    public float waitTime = 0.4f;
    public bool canBeStomped = true;
    public float stompBounce = 12f;

    State state = State.Patrol;
    Vector3 target;
    float waitUntil;

    void Start() { target = pointB ? pointB.position : transform.position; }

    void Update()
    {
        if (!pointA || !pointB) return;
        switch (state)
        {
            case State.Patrol:
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                if ((transform.position - target).sqrMagnitude < 0.0004f)
                {
                    state = State.Wait;
                    waitUntil = Time.time + waitTime;
                }
                break;
            case State.Wait:
                if (Time.time >= waitUntil)
                {
                    target = target == pointA.position ? pointB.position : pointA.position;
                    state = State.Patrol;
                }
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var respawn = other.GetComponent<NullRespawn>();
        if (!respawn) return;

        var rb = other.attachedRigidbody;
        bool fromAbove = rb && rb.linearVelocity.y < -0.1f && other.bounds.min.y > transform.position.y;
        if (canBeStomped && fromAbove)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stompBounce);
            gameObject.SetActive(false); // puntero "liberado"
            return;
        }
        respawn.Kill();
    }

    void OnDrawGizmos()
    {
        if (!pointA || !pointB) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pointA.position, pointB.position);
    }
}
