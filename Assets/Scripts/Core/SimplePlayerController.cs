using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de jugador en primera persona muy simple, para PROBAR el
/// prototipo sin XR (teclado + raton). Cuando montes el rig XR, desactiva
/// este objeto y usa el XR Origin en su lugar.
///
/// Usa el nuevo Input System (Keyboard.current / Mouse.current), que es el
/// que este proyecto tiene activo.
///
/// Caracteristicas:
///   - Moverse con WASD / flechas.
///   - Mirar con el raton.
///   - Empujar objetos con Rigidbody (las cajas) al chocar con ellos.
///   - Interactuar: apunta a un objeto y pulsa E o clic izquierdo para
///     llamar a sus metodos Pulsar()/Accionar()/MostrarInfo().
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 4f;
    [SerializeField] private float gravedad = -9.81f;
    [SerializeField] private float sensibilidadRaton = 0.1f;

    [Header("Empuje de objetos fisicos")]
    [Tooltip("Fuerza con la que el jugador empuja Rigidbodies (cajas).")]
    [SerializeField] private float fuerzaEmpuje = 2.5f;

    [Header("Interaccion")]
    [Tooltip("Distancia maxima a la que se puede interactuar apuntando.")]
    [SerializeField] private float alcanceInteraccion = 3f;

    private CharacterController controller;
    private Camera camara;
    private float velocidadVertical;
    private float pitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        camara = GetComponentInChildren<Camera>();
        if (camara == null)
        {
            var camGO = new GameObject("Camara_Jugador");
            camGO.transform.SetParent(transform);
            camGO.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            camara = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        var teclado = Keyboard.current;
        var raton = Mouse.current;
        if (teclado == null) return; // sin teclado no hay nada que hacer

        Mirar(raton);
        Mover(teclado);

        bool interactuar =
            teclado.eKey.wasPressedThisFrame ||
            (raton != null && raton.leftButton.wasPressedThisFrame);
        if (interactuar) IntentarInteractuar();

        // Liberar el raton con Escape (util al probar en el editor).
        if (teclado.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Mirar(Mouse raton)
    {
        if (raton == null || Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = raton.delta.ReadValue() * sensibilidadRaton;

        transform.Rotate(Vector3.up * delta.x);

        pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
        camara.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Mover(Keyboard teclado)
    {
        float h = 0f, v = 0f;
        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) h -= 1f;
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) h += 1f;
        if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) v -= 1f;
        if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) v += 1f;

        Vector3 dir = (transform.right * h + transform.forward * v).normalized;

        if (controller.isGrounded && velocidadVertical < 0f)
            velocidadVertical = -1f;
        velocidadVertical += gravedad * Time.deltaTime;

        Vector3 movimiento = dir * velocidad + Vector3.up * velocidadVertical;
        controller.Move(movimiento * Time.deltaTime);
    }

    private void IntentarInteractuar()
    {
        Ray ray = new Ray(camara.transform.position, camara.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, alcanceInteraccion)) return;

        var go = hit.collider.gameObject;

        var palanca = go.GetComponentInParent<LeverInteractable>();
        if (palanca != null) { palanca.Accionar(); return; }

        var info = go.GetComponentInParent<InfoObject>();
        if (info != null) { info.MostrarInfo(); return; }
    }

    // Empuja los Rigidbodies (cajas) con los que choca el CharacterController.
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic) return;

        Vector3 empuje = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        rb.AddForce(empuje * fuerzaEmpuje, ForceMode.Impulse);
    }
}
