using UnityEngine;
using UnityEngine.Events;


/// Puente entre los eventos C# del GameManager y los UnityEvents del Inspector.
/// Sirve para reaccionar a "reto completado" sin escribir codigo: por ejemplo,
/// arrastrar aqui el Porton y llamar a GateController.Abrir() cuando el usuario
/// recoge todos los paquetes.


public class GameEventRelay : MonoBehaviour
{
    [Tooltip("Se dispara cuando se recogen TODOS los paquetes.")]
    public UnityEvent onRetoCompletado = new UnityEvent();

    [Tooltip("Se dispara cuando se entregan TODAS las cajas.")]
    public UnityEvent onTodasCajasEntregadas = new UnityEvent();

    private GameManager gm;
    private bool suscrito = false;

    private void OnEnable()
    {
        Suscribir();
    }

    private void Start()
    {
        // Por si el GameManager aun no existia en OnEnable.
        Suscribir();
    }

    private void OnDisable()
    {
        if (gm != null)
        {
            gm.OnRetoCompletado -= ReenviarPaquetes;
            gm.OnTodasCajasEntregadas -= ReenviarCajas;
        }
        suscrito = false;
    }

    private void Suscribir()
    {
        if (suscrito) return;
        gm = GameManager.Instance;
        if (gm == null) return;
        gm.OnRetoCompletado += ReenviarPaquetes;
        gm.OnTodasCajasEntregadas += ReenviarCajas;
        suscrito = true;
    }

    private void ReenviarPaquetes() => onRetoCompletado?.Invoke();
    private void ReenviarCajas() => onTodasCajasEntregadas?.Invoke();
}
