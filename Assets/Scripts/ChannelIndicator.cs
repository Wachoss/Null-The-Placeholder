using UnityEngine;

/// <summary>
/// Muestra el canal de color activo de Null (cian / magenta / amarillo).
/// Va en un hijo de Null con un SpriteRenderer (sprite "indicador_canal", en blanco).
/// - El LED se tiñe del color del canal y da un "pulso" al cambiar.
/// - Opcional: durante el Dash, el cuerpo de Null se tiñe de ese color.
/// Funciona igual en las 4 fases porque no depende del sprite del personaje.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ChannelIndicator : MonoBehaviour
{
    public NullController2D controller;
    public bool tintBodyWhileDashing = true;
    [Range(0f, 1f)] public float bodyTintAmount = 0.6f;
    public float pulseScale = 1.6f;
    public float pulseSpeed = 10f;
    public float bobAmplitude = 0.04f;

    SpriteRenderer led;
    ChromaChannel lastChannel;
    Vector3 baseScale, basePos;
    float pulse;

    void Awake()
    {
        led = GetComponent<SpriteRenderer>();
        if (controller == null) controller = GetComponentInParent<NullController2D>();
        baseScale = transform.localScale;
        basePos = transform.localPosition;
        if (controller) lastChannel = controller.currentChannel;
    }

    void LateUpdate()
    {
        if (controller == null) return;
        var ch = controller.currentChannel;
        Color c = NullController2D.ToColor(ch);

        if (ch != lastChannel) { pulse = 1f; lastChannel = ch; }
        pulse = Mathf.MoveTowards(pulse, 0f, pulseSpeed * Time.deltaTime);

        led.color = c;
        led.enabled = controller.spriteRenderer == null || controller.spriteRenderer.enabled; // se oculta al morir
        transform.localScale = baseScale * Mathf.Lerp(1f, pulseScale, pulse);
        transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time * 4f) * bobAmplitude;

        if (tintBodyWhileDashing && controller.spriteRenderer)
            controller.spriteRenderer.color = controller.IsDashing ? Color.Lerp(Color.white, c, bodyTintAmount) : Color.white;
    }
}
