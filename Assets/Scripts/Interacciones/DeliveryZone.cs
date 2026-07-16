using UnityEngine;
using UnityEngine.Events;

/// Zona de entrega: plataforma donde el usuario debe DEJAR una caja.
/// Funciona tanto empujando la caja (jugador de prueba) como agarrandola con
/// los mandos VR y soltandola encima (XR).
///
/// Cuando una caja valida entra, la zona se marca como ocupada, avisa al
/// GameManager (contador de entregas) y da feedback visual. Si la caja se
/// retira, la zona vuelve a quedar libre.
///
/// Requiere: un Collider con "Is Trigger" activado.

[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Tag de las cajas que se pueden entregar.")]
    [SerializeField] private string tagCaja = "Caja";

    [Tooltip("Si esta activo, la caja se fija en el centro de la zona al entregarla.")]
    [SerializeField] private bool fijarCajaAlEntregar = true;

    [Header("Feedback visual")]
    [SerializeField] private Renderer indicador;
    [SerializeField] private Color colorLibre = new Color(0.85f, 0.65f, 0.2f);
    [SerializeField] private Color colorOcupado = new Color(0.2f, 0.8f, 0.3f);

    [Header("Eventos")]
    public UnityEvent onCajaEntregada = new UnityEvent();
    public UnityEvent onCajaRetirada = new UnityEvent();

    private bool ocupada = false;
    private Transform cajaActual;

    private void Start()
    {
        ActualizarIndicador();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ocupada) return;
        if (!other.CompareTag(tagCaja)) return;

        ocupada = true;
        cajaActual = other.transform;

        if (fijarCajaAlEntregar)
            FijarCaja(other);

        if (GameManager.Instance != null)
            GameManager.Instance.EntregarCaja();

        onCajaEntregada?.Invoke();
        ActualizarIndicador();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!ocupada) return;
        if (other.transform != cajaActual) return;

        ocupada = false;
        cajaActual = null;

        if (GameManager.Instance != null)
            GameManager.Instance.RetirarCaja();

        onCajaRetirada?.Invoke();
        ActualizarIndicador();
    }

    private void FijarCaja(Collider caja)
    {
        // Centra la caja sobre la plataforma y frena su movimiento, sin
        // volverla cinematica (asi el jugador aun puede empujarla para retirarla).
        var rb = caja.attachedRigidbody;
        Vector3 destino = new Vector3(transform.position.x,
            caja.transform.position.y, transform.position.z);
        caja.transform.position = destino;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void ActualizarIndicador()
    {
        if (indicador != null)
            indicador.material.color = ocupada ? colorOcupado : colorLibre;
    }
}
