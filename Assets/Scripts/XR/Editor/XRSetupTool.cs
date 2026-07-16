using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Herramienta de editor para montar la interaccion XR/VR SIN arrastrar prefabs
/// a mano. Aparece en la barra de menus de Unity como "Herramientas XR".
///
/// Requiere que esten importados los samples del XR Interaction Toolkit
/// (Starter Assets y XR Device Simulator), es decir la carpeta
/// Assets/Samples/XR Interaction Toolkit/...
///
/// Opciones:
///   - Configurar escena XR: mete el rig VR + el simulador y apaga la camara /
///     el jugador de prueba.
///   - Hacer objeto agarrable: convierte el objeto seleccionado (o crea una
///     caja) en un agarrable XR con el reporter de eventos.
///   - Montar demo completa: hace las dos cosas de golpe.
/// </summary>
public static class XRSetupTool
{
    private const string MenuRoot = "Herramientas XR/";

    private const string NombreRig = "XR Origin (XR Rig)";
    private const string NombreSimulador = "XR Device Simulator";

    // ---------------------------------------------------------------------
    // 1) Configurar la escena: rig + simulador + apagar camara/jugador viejo
    // ---------------------------------------------------------------------
    [MenuItem(MenuRoot + "1. Configurar escena XR (rig + simulador)", priority = 0)]
    public static GameObject ConfigurarEscenaXR()
    {
        GameObject rig = InstanciarPrefabPorNombre(NombreRig);
        if (rig == null) return null;

        GameObject sim = InstanciarPrefabPorNombre(NombreSimulador);

        // Apagar el jugador de prueba (incluye su camara) para que no compita
        // con el rig XR.
        foreach (var spc in Object.FindObjectsByType<SimplePlayerController>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(spc.gameObject, "Apagar jugador de prueba");
            spc.gameObject.SetActive(false);
        }

        // Apagar cualquier camara suelta que NO forme parte del rig
        // (normalmente la "Main Camera" de la escena).
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
        {
            if (rig != null && cam.transform.IsChildOf(rig.transform)) continue;
            Undo.RecordObject(cam.gameObject, "Apagar camara antigua");
            cam.gameObject.SetActive(false);
        }

        MarcarEscenaSucia();
        Selection.activeGameObject = rig;
        Debug.Log("[XRSetupTool] Escena XR configurada: rig + simulador listos. " +
                  "Pulsa Play y usa el simulador (raton/teclado) para probar sin casco.", rig);
        return rig;
    }

    // ---------------------------------------------------------------------
    // 2) Hacer agarrable el objeto seleccionado (o crear una caja)
    // ---------------------------------------------------------------------
    [MenuItem(MenuRoot + "2. Hacer objeto agarrable (XR)", priority = 20)]
    public static GameObject HacerAgarrable()
    {
        GameObject go = Selection.activeGameObject;

        // Si no hay nada seleccionado, creamos una caja de ejemplo.
        if (go == null)
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Caja Agarrable XR";
            go.transform.localScale = Vector3.one * 0.3f;
            go.transform.position = new Vector3(0f, 1f, 1f);
            Undo.RegisterCreatedObjectUndo(go, "Crear caja agarrable");
        }
        else
        {
            Undo.RegisterFullObjectHierarchyUndo(go, "Hacer agarrable");
        }

        // Collider (necesario para agarrarlo)
        if (go.GetComponent<Collider>() == null)
            Undo.AddComponent<BoxCollider>(go);

        // Rigidbody (el XR Grab Interactable lo mueve con fisica)
        if (go.GetComponent<Rigidbody>() == null)
            Undo.AddComponent<Rigidbody>(go);

        // XR Grab Interactable (lo hace agarrable con los mandos)
        if (go.GetComponent<XRGrabInteractable>() == null)
            Undo.AddComponent<XRGrabInteractable>(go);

        // Nuestro reporter: reenvia agarrar/soltar/gatillo al GameManager
        if (go.GetComponent<XRGrabReporter>() == null)
            Undo.AddComponent<XRGrabReporter>(go);

        MarcarEscenaSucia();
        Selection.activeGameObject = go;
        Debug.Log($"[XRSetupTool] '{go.name}' ahora es agarrable en XR.", go);
        return go;
    }

    // ---------------------------------------------------------------------
    // 3) Todo en uno: escena XR + una caja agarrable de ejemplo
    // ---------------------------------------------------------------------
    [MenuItem(MenuRoot + "3. Montar demo completa (rig + simulador + caja)", priority = 40)]
    public static void MontarDemoCompleta()
    {
        GameObject rig = ConfigurarEscenaXR();
        if (rig == null) return;

        // Colocamos la caja delante del rig para tenerla a mano.
        Selection.activeGameObject = null;
        GameObject caja = HacerAgarrable();
        if (caja != null)
            caja.transform.position = rig.transform.position + rig.transform.forward * 0.6f + Vector3.up * 0.9f;

        MarcarEscenaSucia();
        Debug.Log("[XRSetupTool] Demo XR lista. Pulsa Play y agarra la caja con el simulador.");
    }

    // Validaciones: los menus se ven en gris si faltan los samples.
    [MenuItem(MenuRoot + "1. Configurar escena XR (rig + simulador)", validate = true)]
    private static bool ValidarSamples() => BuscarPrefabPorNombre(NombreRig) != null;

    // ---------------------------------------------------------------------
    // Utilidades internas
    // ---------------------------------------------------------------------
    private static GameObject InstanciarPrefabPorNombre(string nombre)
    {
        GameObject prefab = BuscarPrefabPorNombre(nombre);
        if (prefab == null)
        {
            Debug.LogError($"[XRSetupTool] No se encontro el prefab '{nombre}'. " +
                           "Asegurate de haber importado los samples del XR Interaction Toolkit.");
            return null;
        }

        var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Undo.RegisterCreatedObjectUndo(instancia, "Crear " + nombre);
        return instancia;
    }

    private static GameObject BuscarPrefabPorNombre(string nombre)
    {
        foreach (string guid in AssetDatabase.FindAssets($"\"{nombre}\" t:Prefab"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == nombre)
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        return null;
    }

    private static void MarcarEscenaSucia()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
