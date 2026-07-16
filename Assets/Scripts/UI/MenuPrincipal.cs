using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Logica del menu principal (escena "Menu").
/// Se engancha a los botones de la UI desde sus eventos onClick:
///   Boton JUGAR -> Jugar()   (carga la primera sala)
///   Boton SALIR -> Salir()   (cierra el juego / detiene el Play en el editor)
/// </summary>
public class MenuPrincipal : MonoBehaviour
{
    [Tooltip("Nombre de la escena de la primera sala que se carga al pulsar Jugar.")]
    [SerializeField] private string primeraSala = "Sala_1";

    private void Start()
    {
        // En el menu queremos el cursor visible y libre (las salas lo bloquean).
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Carga la primera sala. Llamable desde el onClick del boton Jugar.</summary>
    public void Jugar()
    {
        SceneManager.LoadScene(primeraSala);
    }

    /// <summary>Cierra el juego (o detiene el Play si estamos en el editor).</summary>
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
