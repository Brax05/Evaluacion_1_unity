using UnityEngine;


/// Paquete coleccionable.
/// Cuando el jugador (u objeto con el Tag adecuado) entra en su trigger,
/// avisa al GameManager para aumentar el contador y desaparece.
///
/// Requiere: un Collider con "Is Trigger" activado.

[RequireComponent(typeof(Collider))]
public class Package : MonoBehaviour
{
    [Header("Configuracion")]
    [Tooltip("Tag del objeto que puede recoger el paquete (normalmente 'Player').")]
    [SerializeField] private string tagRecolector = "Player";

    [Tooltip("Efecto opcional al recoger (particula, sonido) que se instancia.")]
    [SerializeField] private GameObject efectoAlRecoger;

    [Tooltip("Sonido opcional al recoger.")]
    [SerializeField] private AudioClip sonidoRecoger;

    private bool recogido = false;

    private void Reset()
    {
        // Al añadir el script, deja el collider como trigger automaticamente.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (recogido) return;
        if (!other.CompareTag(tagRecolector)) return;

        recogido = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RecogerPaquete();

        if (efectoAlRecoger != null)
            Instantiate(efectoAlRecoger, transform.position, Quaternion.identity);

        if (sonidoRecoger != null)
            AudioSource.PlayClipAtPoint(sonidoRecoger, transform.position);

        // Desaparece el paquete (cambio visible en la escena).
        Destroy(gameObject);
    }
}
