using System;
using UnityEngine;


/// prototipo "Escape Room XR".
/// Lleva la cuenta de paquetes recogidos, controla los objetivos y avisa
/// al resto del juego mediante eventos (relacion clara accion -> respuesta).
///
/// Es un Singleton: cualquier script puede llamar a GameManager.Instance.

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Configuracion de objetivos")]
    [Tooltip("Cuantos paquetes debe recoger el usuario para completar el reto.")]
    [SerializeField] private int paquetesRequeridos = 3;
    [Tooltip("Cuantas cajas debe llevar a las zonas de entrega.")]
    [SerializeField] private int cajasRequeridas = 3;
    [Tooltip("Mensaje/objetivo que se muestra al empezar la sala. Cambialo en " +
             "cada escena para explicar el reto de esa sala concreta.")]
    [TextArea]
    [SerializeField] private string mensajeInicial =
        "Recoge los paquetes y lleva las cajas a la estacion de entrega.";

    [Header("Estado actual (solo lectura)")]
    [SerializeField] private int paquetesRecogidos = 0;
    [SerializeField] private int cajasEntregadas = 0;
    [SerializeField] private bool retoCompletado = false;
    [SerializeField] private bool cajasCompletadas = false;

    // ---- EVENTOS ----
    // La UI y otros objetos se suscriben a estos eventos para reaccionar.
    public event Action<int, int> OnContadorCambia;      // (recogidos, requeridos)
    public event Action<string> OnMensajeCambia;         // texto dinamico en pantalla
    public event Action OnRetoCompletado;                // se recogieron todos los paquetes
    public event Action<float> OnProgresoCambia;         // 0..1 para la barra
    public event Action<int, int> OnEntregasCambia;      // (entregadas, requeridas)
    public event Action OnTodasCajasEntregadas;          // se entregaron todas las cajas

    // Propiedades publicas de consulta
    public int PaquetesRequeridos => paquetesRequeridos;
    public int PaquetesRecogidos => paquetesRecogidos;
    public bool RetoCompletado => retoCompletado;
    public int CajasRequeridas => cajasRequeridas;
    public int CajasEntregadas => cajasEntregadas;

    private void Awake()
    {
        // Patron Singleton: si ya existe otro GameManager, este sobra.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Estado inicial: avisamos a la UI para que muestre 0 / N.
        NotificarContador();
        NotificarEntregas();
        MostrarMensaje(mensajeInicial);
    }

    /// <summary>
    /// Llamado por cada Paquete (Package.cs) cuando el jugador lo recoge.
    /// </summary>
    public void RecogerPaquete()
    {
        if (retoCompletado) return;

        paquetesRecogidos++;
        NotificarContador();
        MostrarMensaje($"Paquete recogido ({paquetesRecogidos}/{paquetesRequeridos})");

        if (paquetesRecogidos >= paquetesRequeridos)
        {
            retoCompletado = true;
            MostrarMensaje("Todos los paquetes recogidos. El porton se abre!");
            OnRetoCompletado?.Invoke();
        }
    }

    /// <summary>
    /// Llamado por una DeliveryZone cuando una caja queda depositada en ella.
    /// </summary>
    public void EntregarCaja()
    {
        cajasEntregadas++;
        NotificarEntregas();
        MostrarMensaje($"Caja entregada ({cajasEntregadas}/{cajasRequeridas})");

        if (!cajasCompletadas && cajasEntregadas >= cajasRequeridas)
        {
            cajasCompletadas = true;
            MostrarMensaje("Todas las cajas entregadas!");
            OnTodasCajasEntregadas?.Invoke();
        }
    }

    /// <summary>
    /// Llamado por una DeliveryZone cuando una caja se retira de ella.
    /// </summary>
    public void RetirarCaja()
    {
        cajasEntregadas = Mathf.Max(0, cajasEntregadas - 1);
        cajasCompletadas = false;
        NotificarEntregas();
    }

    /// <summary>
    /// Cambia el mensaje dinamico que se ve en la interfaz.
    /// Usado por zonas trigger, objetos de informacion, etc.
    /// </summary>
    public void MostrarMensaje(string texto)
    {
        OnMensajeCambia?.Invoke(texto);
    }

    private void NotificarContador()
    {
        OnContadorCambia?.Invoke(paquetesRecogidos, paquetesRequeridos);
        float progreso = paquetesRequeridos > 0
            ? (float)paquetesRecogidos / paquetesRequeridos
            : 0f;
        OnProgresoCambia?.Invoke(Mathf.Clamp01(progreso));
    }

    private void NotificarEntregas()
    {
        OnEntregasCambia?.Invoke(cajasEntregadas, cajasRequeridas);
    }
}
