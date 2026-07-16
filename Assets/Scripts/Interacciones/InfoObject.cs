using UnityEngine;

/// Objeto que muestra informacion cuando el usuario lo toca / selecciona.
/// Ejemplo del enunciado: "un objeto que al tocarse muestre informacion".
///
/// Funciona con clic de raton (prueba en editor) o llamando a MostrarInfo()
/// desde un interactor XR.

[RequireComponent(typeof(Collider))]
public class InfoObject : MonoBehaviour
{
    [TextArea]
    [SerializeField] private string informacion = "Estante de mercancia fragil.";

    private void OnMouseDown()
    {
        MostrarInfo();
    }

    /// <summary>Muestra la informacion en la UI. Llamable desde XR.</summary>
    public void MostrarInfo()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.MostrarMensaje(informacion);
    }
}
