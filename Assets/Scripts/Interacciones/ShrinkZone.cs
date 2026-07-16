using UnityEngine;

/// <summary>
/// Zona (gatillo) que ENCOGE un objetivo mientras el jugador esta encima.
/// Cuanto mas tiempo permanezca el jugador dentro del trigger, mas pequeño se
/// vuelve el objetivo (p. ej. el cubo pesado) hasta desaparecer.
///
/// Si el jugador sale del trigger, el encogido se pausa; al volver, continua.
///
/// Requiere: un Collider con "Is Trigger" activado.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ShrinkZone : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private string tagJugador = "Player";

    [Tooltip("Objeto que se va a encoger (el cubo pesado).")]
    [SerializeField] private Transform objetivo;

    [Tooltip("Velocidad de encogido (unidades de escala por segundo).")]
    [SerializeField] private float velocidadEncogido = 1.5f;

    [Tooltip("Al desaparecer, desactiva el objeto.")]
    [SerializeField] private bool desactivarAlDesaparecer = true;

    [Tooltip("Si esta activo, en cuanto el jugador pisa la placa el objetivo " +
             "sigue encogiendose hasta desaparecer aunque el jugador se aleje.")]
    [SerializeField] private bool seguirAlSalir = true;

    [Header("Mensajes")]
    [SerializeField] private string mensajeInicio = "Reduciendo el cubo pesado...";
    [SerializeField] private string mensajeFin = "El cubo ha desaparecido. Ya puedes pasar!";

    private bool jugadorDentro;
    private bool activado;   // el jugador ya piso la placa al menos una vez
    private bool anunciado;
    private bool terminado;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorDentro = true;
            activado = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagJugador))
            jugadorDentro = false;
    }

    private void Update()
    {
        if (terminado || objetivo == null) return;

        // Encoge mientras el jugador esta encima o, si se activo seguirAlSalir,
        // desde que lo piso por primera vez.
        bool debeEncoger = jugadorDentro || (seguirAlSalir && activado);
        if (!debeEncoger) return;

        if (!anunciado)
        {
            Msg(mensajeInicio);
            anunciado = true;
        }

        // Encoge proporcionalmente hacia cero (MoveTowards mantiene la forma).
        objetivo.localScale = Vector3.MoveTowards(
            objetivo.localScale, Vector3.zero, velocidadEncogido * Time.deltaTime);

        if (objetivo.localScale.sqrMagnitude <= 0.01f)
        {
            terminado = true;
            Msg(mensajeFin);
            if (desactivarAlDesaparecer)
                objetivo.gameObject.SetActive(false);
        }
    }

    private void Msg(string texto)
    {
        if (!string.IsNullOrEmpty(texto) && GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje(texto);
    }
}
