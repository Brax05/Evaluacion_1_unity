using UnityEngine;
using UnityEngine.Events;


/// Palanca de dos estados (encendido / apagado).
/// Al accionarla rota su mango y dispara eventos distintos segun el estado.
/// Se suele usar para encender una maquina o una luz.
///
/// Igual que el boton: funciona con clic de raton (prueba) y con un metodo
/// publico Accionar() que puedes llamar desde XR.

[RequireComponent(typeof(Collider))]
public class LeverInteractable : MonoBehaviour
{
    [Header("Mango que rota")]
    [Tooltip("Transform del mango de la palanca. Si se deja vacio, rota este objeto.")]
    [SerializeField] private Transform mango;
    [SerializeField] private float anguloApagado = -35f;
    [SerializeField] private float anguloEncendido = 35f;
    [SerializeField] private float velocidad = 6f;

    [Header("Estado")]
    [SerializeField] private bool encendida = false;

    [Header("Eventos")]
    public UnityEvent onEncender = new UnityEvent();
    public UnityEvent onApagar = new UnityEvent();

    [Header("Mensajes en pantalla (opcional)")]
    [SerializeField] private string mensajeEncender = "Maquina encendida.";
    [SerializeField] private string mensajeApagar = "Maquina apagada.";

    private void Awake()
    {
        if (mango == null) mango = transform;
        AplicarRotacionInstantanea();
    }

    private void Update()
    {
        // Suaviza la rotacion hacia el angulo objetivo.
        float objetivo = encendida ? anguloEncendido : anguloApagado;
        Quaternion destino = Quaternion.Euler(objetivo, 0f, 0f);
        mango.localRotation = Quaternion.Lerp(
            mango.localRotation, destino, velocidad * Time.deltaTime);
    }

    private void OnMouseDown()
    {
        Accionar();
    }

    /// <summary>Cambia el estado de la palanca. Llamable desde XR.</summary>
    public void Accionar()
    {
        encendida = !encendida;

        if (encendida)
        {
            onEncender?.Invoke();
            MostrarMensaje(mensajeEncender);
        }
        else
        {
            onApagar?.Invoke();
            MostrarMensaje(mensajeApagar);
        }
    }

    private void MostrarMensaje(string txt)
    {
        if (!string.IsNullOrEmpty(txt) && GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje(txt);
    }

    private void AplicarRotacionInstantanea()
    {
        float objetivo = encendida ? anguloEncendido : anguloApagado;
        mango.localRotation = Quaternion.Euler(objetivo, 0f, 0f);
    }
}
