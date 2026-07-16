using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Zona de salida de una sala (escape room). Se coloca DETRAS de la puerta:
/// mientras el puzzle no abra la puerta, el jugador no puede llegar aqui, asi
/// que la puerta fisica ya funciona como bloqueo natural.
///
/// Cuando el jugador entra en el trigger, carga la siguiente escena (la
/// siguiente sala). Tambien se puede llamar por codigo/eventos con Avanzar(),
/// por ejemplo desde el UnityEvent de una palanca o de ExitDoorPuzzle.
///
/// Formas de elegir a que escena ir:
///   - Si "nombreEscenaSiguiente" tiene texto  -> carga esa escena por nombre.
///   - Si esta vacio                           -> carga la siguiente por indice
///                                                (build index actual + 1).
///
/// Si esta sala es la ULTIMA (marca "esSalaFinal"), en vez de cargar otra
/// escena dispara onSalaFinal (para mostrar un panel de victoria) o carga la
/// escena "nombreEscenaVictoria" si la indicas.
///
/// IMPORTANTE: todas las escenas deben estar añadidas en
/// File > Build Settings > Scenes In Build, en el orden de las salas.
///
/// Requiere: un Collider con "Is Trigger" activado.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CargadorSiguienteSala : MonoBehaviour
{
    [Header("Que jugador dispara la salida")]
    [SerializeField] private string tagJugador = "Player";

    [Header("Destino")]
    [Tooltip("Nombre exacto de la escena a cargar. Si se deja vacio, se carga " +
             "la siguiente por indice de Build Settings (actual + 1).")]
    [SerializeField] private string nombreEscenaSiguiente = "";

    [Header("Sala final")]
    [Tooltip("Marca esto solo en la ultima sala. En vez de cargar otra escena, " +
             "dispara onSalaFinal (o carga 'nombreEscenaVictoria' si lo indicas).")]
    [SerializeField] private bool esSalaFinal = false;
    [Tooltip("Escena de victoria opcional para la ultima sala. Si esta vacio, " +
             "solo se dispara el evento onSalaFinal.")]
    [SerializeField] private string nombreEscenaVictoria = "";

    [Header("Comportamiento")]
    [Tooltip("Si esta activo, la salida no funciona hasta llamar a Desbloquear(). " +
             "Util para asegurarse de que solo se cruza tras resolver el puzzle.")]
    [SerializeField] private bool empiezaBloqueada = false;
    [Tooltip("Segundos de espera antes de cambiar de escena (para leer el mensaje).")]
    [SerializeField] private float retardo = 1.2f;
    [TextArea]
    [SerializeField] private string mensajeAlSalir = "Sala superada. Avanzando a la siguiente...";

    [Header("Eventos")]
    [Tooltip("Se dispara al cruzar la salida de la ultima sala (panel de victoria, etc.).")]
    public UnityEvent onSalaFinal = new UnityEvent();

    private bool bloqueada;
    private bool yaDisparada;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        bloqueada = empiezaBloqueada;
    }

    /// <summary>Habilita la salida (por ejemplo desde el evento de puerta abierta).</summary>
    public void Desbloquear() => bloqueada = false;

    /// <summary>Vuelve a bloquear la salida.</summary>
    public void Bloquear() => bloqueada = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagJugador)) return;
        Avanzar();
    }

    /// <summary>
    /// Lanza la transicion de sala. Llamable desde un UnityEvent del Inspector.
    /// </summary>
    public void Avanzar()
    {
        if (yaDisparada || bloqueada) return;
        yaDisparada = true;

        if (GameManager.Instance != null && !string.IsNullOrEmpty(mensajeAlSalir))
            GameManager.Instance.MostrarMensaje(mensajeAlSalir);

        StartCoroutine(CambiarEscena());
    }

    private IEnumerator CambiarEscena()
    {
        if (retardo > 0f)
            yield return new WaitForSeconds(retardo);

        if (esSalaFinal)
        {
            onSalaFinal?.Invoke();
            if (!string.IsNullOrEmpty(nombreEscenaVictoria))
                SceneManager.LoadScene(nombreEscenaVictoria);
            yield break;
        }

        if (!string.IsNullOrEmpty(nombreEscenaSiguiente))
        {
            SceneManager.LoadScene(nombreEscenaSiguiente);
        }
        else
        {
            int siguiente = SceneManager.GetActiveScene().buildIndex + 1;
            if (siguiente < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(siguiente);
            }
            else
            {
                Debug.LogWarning(
                    "[CargadorSiguienteSala] No hay una escena siguiente en Build " +
                    "Settings. Marca 'esSalaFinal' o añade mas escenas.", this);
            }
        }
    }
}
