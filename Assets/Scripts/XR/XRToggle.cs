using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Alterna entre el modo VR (rig XR + simulador) y el jugador normal de teclado
/// pulsando una tecla (por defecto la M). La MISMA tecla activa y desactiva.
///
/// - VR activado  -> se enciende el XR Origin y el simulador, se apaga el
///                   jugador de teclado.
/// - VR desactivado -> al reves.
///
/// Si no asignas las referencias en el Inspector, el script las busca solo en
/// la escena (por nombre / por componente).
///
/// Usa el nuevo Input System (Keyboard.current), que es el que tiene activo
/// el proyecto.
/// </summary>
public class XRToggle : MonoBehaviour
{
    private static XRToggle instance;

    [Header("Referencias (opcionales: se autocompletan si se dejan vacias)")]
    [Tooltip("El rig VR de la escena (XR Origin (XR Rig)).")]
    [SerializeField] private GameObject xrRig;
    [Tooltip("El XR Device Simulator de la escena.")]
    [SerializeField] private GameObject xrSimulador;
    [Tooltip("El objeto del jugador de teclado (con SimplePlayerController).")]
    [SerializeField] private GameObject jugadorTeclado;
    [Tooltip("La camara normal de la escena (Main Camera), si esta separada del jugador.")]
    [SerializeField] private GameObject camaraNormal;

    [Header("Configuracion")]
    [Tooltip("Tecla que activa/desactiva el VR.")]
    [SerializeField] private Key teclaToggle = Key.M;
    [Tooltip("Si esta activo, la escena arranca ya en modo VR.")]
    [SerializeField] private bool empezarEnVR = false;

    private bool vrActivo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InicializarAutomaticamente()
    {
        // Si ya hay un XRToggle en la escena, no creamos duplicados
        if (FindFirstObjectByType<XRToggle>(FindObjectsInactive.Include) == null)
        {
            GameObject go = new GameObject("XR Toggle Manager (Auto)");
            go.AddComponent<XRToggle>();
        }
    }

    private void Awake()
    {
        // Patrón Singleton persistente
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Suscribirse al evento de cambio de escena para volver a buscar referencias
        SceneManager.sceneLoaded += OnSceneLoaded;

        BuscarReferencias();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuscarReferencias();
        
        // Al cargar una escena, aplicar el estado inicial deseado.
        AplicarEstado(empezarEnVR);
    }

    private void Start()
    {
        // Si es el primer arranque, aplicamos el estado inicial.
        AplicarEstado(empezarEnVR);
    }

    private void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado[teclaToggle].wasPressedThisFrame)
        {
            // Rebuscar referencias por si se destruyó o cambió algo dinámicamente
            BuscarReferencias();
            AplicarEstado(!vrActivo);
        }
    }

    private void BuscarReferencias()
    {
        // Buscar el Rig XR
        if (xrRig == null) xrRig = BuscarPorNombre("XR Origin (XR Rig)");
        
        // Buscar el Simulador
        if (xrSimulador == null) xrSimulador = BuscarPorNombre("XR Device Simulator");
        
        // Buscar el jugador de teclado
        if (jugadorTeclado == null)
        {
            var spc = FindFirstObjectByType<SimplePlayerController>(FindObjectsInactive.Include);
            if (spc != null) jugadorTeclado = spc.gameObject;
        }

        // Buscar cámara normal si está separada del jugador
        if (camaraNormal == null)
        {
            var cam = GameObject.FindWithTag("MainCamera");
            if (cam != null && (jugadorTeclado == null || !cam.transform.IsChildOf(jugadorTeclado.transform)))
            {
                camaraNormal = cam;
            }
        }
    }

    /// <summary>Enciende/apaga el VR. Tambien es llamable desde un UnityEvent.</summary>
    public void AplicarEstado(bool vr)
    {
        vrActivo = vr;

        // Asegurarse de tener las referencias actualizadas antes de aplicar estado
        BuscarReferencias();

        if (xrRig != null) xrRig.SetActive(vr);
        if (xrSimulador != null) xrSimulador.SetActive(vr);
        if (jugadorTeclado != null) jugadorTeclado.SetActive(!vr);
        if (camaraNormal != null) camaraNormal.SetActive(!vr);

        // Controlar el estado del cursor del ratón según el modo
        if (vr)
        {
            // En modo VR, desbloquear el cursor para interactuar con el simulador
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // En modo teclado, bloquear el cursor para el controlador de primera persona
            // Solo si el jugador de teclado está activo en esta escena
            if (jugadorTeclado != null && jugadorTeclado.activeSelf)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        string txt = vr
            ? "Modo VR ACTIVADO (pulsa M para volver al jugador normal)."
            : "Modo VR desactivado (pulsa M para activar el VR).";

        if (GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje(txt);
        else
            Debug.Log("[XRToggle] " + txt);
    }

    /// <summary>
    /// Asigna las referencias desde fuera (lo usa la herramienta de editor
    /// XRSetupTool para dejar el toggle listo sin tener que arrastrar nada).
    /// </summary>
    public void ConfigurarReferencias(GameObject rig, GameObject simulador,
                                      GameObject jugador, GameObject camara)
    {
        xrRig = rig;
        xrSimulador = simulador;
        jugadorTeclado = jugador;
        camaraNormal = camara;
    }

    private static GameObject BuscarPorNombre(string nombre)
    {
        // Incluye objetos inactivos por si el rig arranca apagado.
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name == nombre && t.gameObject.scene.IsValid())
                return t.gameObject;
        }
        return null;
    }
}
