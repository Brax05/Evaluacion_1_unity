using UnityEngine;

/// Caja empujable (colision fisica real).

[RequireComponent(typeof(Rigidbody))]
public class PushableBox : MonoBehaviour
{
    [Header("Sonido de golpe (opcional)")]
    [SerializeField] private AudioClip sonidoGolpe;
    [Tooltip("Fuerza minima de impacto para reproducir el sonido.")]
    [SerializeField] private float impactoMinimo = 2f;
    [SerializeField] private float volumen = 0.6f;

    private void OnCollisionEnter(Collision collision)
    {
        if (sonidoGolpe == null) return;
        if (collision.relativeVelocity.magnitude < impactoMinimo) return;

        AudioSource.PlayClipAtPoint(sonidoGolpe, transform.position, volumen);
    }
}
