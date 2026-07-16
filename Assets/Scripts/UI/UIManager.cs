using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Conecta el GameManager con la interfaz (Canvas).
/// Escucha los eventos del GameManager y actualiza los textos, el contador,
/// la barra de progreso y el panel final.
///
public class UIManager : MonoBehaviour
{
    [Header("Textos principales")]
    [Tooltip("Nombre del prototipo (texto fijo).")]
    [SerializeField] private TMP_Text tituloTexto;
    [Tooltip("Instrucciones basicas (texto fijo).")]
    [SerializeField] private TMP_Text instruccionesTexto;

    [Header("Elementos dinamicos")]
    [Tooltip("Contador de paquetes, ej: 'Paquetes: 1/3'.")]
    [SerializeField] private TMP_Text contadorTexto;
    [Tooltip("Contador de cajas entregadas, ej: 'Cajas: 1/3'.")]
    [SerializeField] private TMP_Text cajasTexto;
    [Tooltip("Mensaje contextual que cambia segun la accion del usuario.")]
    [SerializeField] private TMP_Text mensajeTexto;
    [Tooltip("Barra de progreso (Slider o Image tipo Filled).")]
    [SerializeField] private Slider barraProgreso;

    [Header("Paneles opcionales")]
    [Tooltip("Panel que aparece al completar el reto.")]
    [SerializeField] private GameObject panelFinal;

    [Header("Textos fijos")]
    [SerializeField] private string nombrePrototipo = "BODEGA XR";
    [TextArea]
    [SerializeField] private string instrucciones =
        "WASD / mando: moverse\nClic o gatillo: interactuar\nRecoge los paquetes y abre el porton.";

    private void OnEnable()
    {
        // Nos suscribimos a los eventos del GameManager.
        if (GameManager.Instance != null)
            Suscribir(GameManager.Instance);
    }

    private void Start()
    {
        // Por si el GameManager se creo despues (orden de ejecucion).
        if (GameManager.Instance != null)
            Suscribir(GameManager.Instance);

        // Textos fijos
        if (tituloTexto != null) tituloTexto.text = nombrePrototipo;
        if (instruccionesTexto != null) instruccionesTexto.text = instrucciones;
        if (panelFinal != null) panelFinal.SetActive(false);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            Desuscribir(GameManager.Instance);
    }

    private bool suscrito = false;

    private void Suscribir(GameManager gm)
    {
        if (suscrito) return;
        gm.OnContadorCambia += ActualizarContador;
        gm.OnEntregasCambia += ActualizarCajas;
        gm.OnMensajeCambia += ActualizarMensaje;
        gm.OnProgresoCambia += ActualizarProgreso;
        gm.OnRetoCompletado += MostrarPanelFinal;
        suscrito = true;
    }

    private void Desuscribir(GameManager gm)
    {
        if (!suscrito) return;
        gm.OnContadorCambia -= ActualizarContador;
        gm.OnEntregasCambia -= ActualizarCajas;
        gm.OnMensajeCambia -= ActualizarMensaje;
        gm.OnProgresoCambia -= ActualizarProgreso;
        gm.OnRetoCompletado -= MostrarPanelFinal;
        suscrito = false;
    }

    private void ActualizarContador(int recogidos, int requeridos)
    {
        if (contadorTexto != null)
            contadorTexto.text = $"Paquetes: {recogidos}/{requeridos}";
    }

    private void ActualizarCajas(int entregadas, int requeridas)
    {
        if (cajasTexto != null)
            cajasTexto.text = $"Cajas: {entregadas}/{requeridas}";
    }

    private void ActualizarMensaje(string texto)
    {
        if (mensajeTexto != null)
            mensajeTexto.text = texto;
    }

    private void ActualizarProgreso(float valor01)
    {
        if (barraProgreso != null)
            barraProgreso.value = valor01;
    }

    private void MostrarPanelFinal()
    {
        if (panelFinal != null)
            panelFinal.SetActive(true);
    }
}
