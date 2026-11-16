using UnityEngine;

/// <summary>
/// Umbral de entrada de una sala de pantalla fija (16:9).
/// Al cruzarlo: guarda el punto de respawn y mueve la cámara a la sala.
/// Collider2D Trigger que cubra la entrada; spawnPoint es un hijo vacío.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomCheckpoint : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform roomCameraAnchor; // centro de la sala; opcional
    public float cameraZ = -10f;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        var respawn = other.GetComponent<NullRespawn>();
        if (!respawn) return;
        respawn.SetRespawnPoint(spawnPoint ? spawnPoint.position : transform.position);

        if (roomCameraAnchor && Camera.main)
        {
            var p = roomCameraAnchor.position;
            Camera.main.transform.position = new Vector3(p.x, p.y, cameraZ); // cambio de sala instantáneo
        }
    }
}
