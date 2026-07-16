using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;


/// Puente entre los eventos de los mandos XR/VR y la logica del juego.
///
/// Se coloca en un objeto AGARRABLE (que ya tenga un XR Grab Interactable).
/// Escucha cuando el mando lo agarra, lo suelta o pulsa el gatillo mientras
/// lo sostiene, y reenvia esos eventos al GameManager (mensajes en pantalla)
/// y a UnityEvents que puedes conectar desde el Inspector.
///
/// Funciona igual con un casco real o con el XR Device Simulator / XR
/// Interaction Simulator (probar en el editor sin casco).
///
/// Requiere: un componente XR Grab Interactable en el mismo objeto.

[RequireComponent(typeof(XRGrabInteractable))]
public class XRGrabReporter : MonoBehaviour
{
    [Header("Mensajes en pantalla (opcional)")]
    [SerializeField] private string mensajeAlAgarrar = "Has agarrado la caja con el mando VR.";
    [SerializeField] private string mensajeAlSoltar = "Has soltado la caja.";
    [SerializeField] private string mensajeAlGatillo = "Gatillo XR pulsado.";

    [Header("Eventos (conectables desde el Inspector)")]
    [Tooltip("Se dispara cuando un mando XR agarra el objeto.")]
    public UnityEvent onAgarrada = new UnityEvent();

    [Tooltip("Se dispara cuando el mando XR suelta el objeto.")]
    public UnityEvent onSoltada = new UnityEvent();

    [Tooltip("Se dispara al pulsar el gatillo/boton de accion mientras se sostiene.")]
    public UnityEvent onGatillo = new UnityEvent();

    private XRGrabInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        // Nos suscribimos a los eventos que emiten los controladores XR.
        interactable.selectEntered.AddListener(AlAgarrar);
        interactable.selectExited.AddListener(AlSoltar);
        interactable.activated.AddListener(AlActivar);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(AlAgarrar);
        interactable.selectExited.RemoveListener(AlSoltar);
        interactable.activated.RemoveListener(AlActivar);
    }

    private void AlAgarrar(SelectEnterEventArgs args)
    {
        MostrarMensaje(mensajeAlAgarrar);
        onAgarrada?.Invoke();
    }

    private void AlSoltar(SelectExitEventArgs args)
    {
        MostrarMensaje(mensajeAlSoltar);
        onSoltada?.Invoke();
    }

    private void AlActivar(ActivateEventArgs args)
    {
        // "activated" = gatillo (o boton de accion) del mando mientras se sostiene.
        MostrarMensaje(mensajeAlGatillo);
        onGatillo?.Invoke();
    }

    private void MostrarMensaje(string txt)
    {
        if (!string.IsNullOrEmpty(txt) && GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje(txt);
    }
}
