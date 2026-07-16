using System.Collections;
using UnityEngine;

/// Controla un porton / puerta corredera de la bodega.
/// Se abre deslizandose hacia arriba (o el eje que elijas) y se cierra volviendo.
///
/// Llama a estos metodos desde eventos (boton, palanca, placa, GameManager):
///   Abrir()  Cerrar()  Alternar()

public class GateController : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Desplazamiento local al abrirse (ej: 0,3,0 sube 3 metros).")]
    [SerializeField] private Vector3 desplazamientoApertura = new Vector3(0f, 3f, 0f);
    [SerializeField] private float velocidad = 2f;

    [Header("Estado inicial")]
    [SerializeField] private bool empiezaAbierto = false;

    [Header("Sonido (opcional)")]
    [SerializeField] private AudioSource sonidoMovimiento;

    private Vector3 posCerrado;
    private Vector3 posAbierto;
    private bool abierto;
    private Coroutine rutina;

    private void Awake()
    {
        posCerrado = transform.localPosition;
        posAbierto = posCerrado + desplazamientoApertura;
        abierto = empiezaAbierto;
        transform.localPosition = abierto ? posAbierto : posCerrado;
    }

    public void Abrir()
    {
        if (abierto) return;
        abierto = true;
        IniciarMovimiento(posAbierto);
    }

    public void Cerrar()
    {
        if (!abierto) return;
        abierto = false;
        IniciarMovimiento(posCerrado);
    }

    public void Alternar()
    {
        if (abierto) Cerrar();
        else Abrir();
    }

    private void IniciarMovimiento(Vector3 destino)
    {
        if (rutina != null) StopCoroutine(rutina);
        rutina = StartCoroutine(Mover(destino));
    }

    private IEnumerator Mover(Vector3 destino)
    {
        if (sonidoMovimiento != null) sonidoMovimiento.Play();

        while (Vector3.Distance(transform.localPosition, destino) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, destino, velocidad * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = destino;

        if (sonidoMovimiento != null) sonidoMovimiento.Stop();
    }
}
