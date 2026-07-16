using UnityEngine;
using UnityEngine.Events;


/// Zona trigger que muestra un mensaje en pantalla cuando el jugador entra
/// y otro (opcional) cuando sale. Ejemplo de "trigger que activa un evento".
///
/// Requiere: un Collider con "Is Trigger" activado.

[RequireComponent(typeof(Collider))]
public class MessageZone : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private string tagJugador = "Player";

    [TextArea]
    [SerializeField] private string mensajeAlEntrar = "Has entrado en la zona de carga.";
    [TextArea]
    [SerializeField] private string mensajeAlSalir = "";

    [Header("Eventos extra (opcional)")]
    [Tooltip("Se pueden enganchar acciones adicionales desde el Inspector.")]
    public UnityEvent onJugadorEntra = new UnityEvent();
    public UnityEvent onJugadorSale = new UnityEvent();

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagJugador)) return;

        if (GameManager.Instance != null && !string.IsNullOrEmpty(mensajeAlEntrar))
            GameManager.Instance.MostrarMensaje(mensajeAlEntrar);

        onJugadorEntra?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(tagJugador)) return;

        if (GameManager.Instance != null && !string.IsNullOrEmpty(mensajeAlSalir))
            GameManager.Instance.MostrarMensaje(mensajeAlSalir);

        onJugadorSale?.Invoke();
    }
}
