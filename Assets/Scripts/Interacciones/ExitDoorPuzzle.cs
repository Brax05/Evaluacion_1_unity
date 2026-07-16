using UnityEngine;


/// Puzzle de la puerta de salida (puerta roja).
/// Hay dos indicadores (cubos) que empiezan NEGROS y pasan a AMARILLO al
/// completar cada tarea:
///   - Indicador 1: se pone amarillo al acomodar todas las cajas.
///   - Indicador 2: se pone amarillo al accionar la palanca.
/// Cuando los DOS estan amarillos, se abre la puerta roja.
///
/// Cableado (Inspector o generador):
///   Relay.onTodasCajasEntregadas -> ActivarLuzCajas()
///   Palanca.onEncender           -> ActivarLuzPalanca()
///   Palanca.onApagar             -> DesactivarLuzPalanca()

public class ExitDoorPuzzle : MonoBehaviour
{
    [Header("Indicadores (cubos negro -> amarillo)")]
    [SerializeField] private Renderer indicadorCajas;
    [SerializeField] private Renderer indicadorPalanca;
    [SerializeField] private Color colorApagado = Color.black;
    [SerializeField] private Color colorEncendido = Color.yellow;

    [Header("Puerta de salida")]
    [SerializeField] private GateController puertaRoja;

    [Header("Mensajes")]
    [SerializeField] private string mensajeLuzCajas = "Indicador 1 amarillo: cajas acomodadas.";
    [SerializeField] private string mensajeLuzPalanca = "Indicador 2 amarillo: palanca activada.";
    [SerializeField] private string mensajePuertaAbierta =
        "Los dos indicadores en amarillo: la puerta roja se abre!";

    private bool cajasOk;
    private bool palancaOk;

    private void Start()
    {
        // Estado inicial: ambos indicadores negros.
        Pintar(indicadorCajas, false);
        Pintar(indicadorPalanca, false);
    }

    public void ActivarLuzCajas()
    {
        cajasOk = true;
        Pintar(indicadorCajas, true);
        Msg(mensajeLuzCajas);
        Evaluar();
    }

    public void DesactivarLuzCajas()
    {
        cajasOk = false;
        Pintar(indicadorCajas, false);
        Evaluar();
    }

    public void ActivarLuzPalanca()
    {
        palancaOk = true;
        Pintar(indicadorPalanca, true);
        Msg(mensajeLuzPalanca);
        Evaluar();
    }

    public void DesactivarLuzPalanca()
    {
        palancaOk = false;
        Pintar(indicadorPalanca, false);
        Evaluar();
    }

    private void Evaluar()
    {
        if (cajasOk && palancaOk)
        {
            if (puertaRoja != null) puertaRoja.Abrir();
            Msg(mensajePuertaAbierta);
        }
        else if (puertaRoja != null)
        {
            puertaRoja.Cerrar();
        }
    }

    private void Pintar(Renderer r, bool encendido)
    {
        if (r != null)
            r.material.color = encendido ? colorEncendido : colorApagado;
    }

    private void Msg(string texto)
    {
        if (!string.IsNullOrEmpty(texto) && GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje(texto);
    }
}
