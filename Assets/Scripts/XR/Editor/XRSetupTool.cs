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
/// En este proyecto el VR se usa SOLO en la Sala 2 (las salas 1 y 3 usan el
/// jugador de teclado). Para eso esta la opcion 4, que lo monta directamente ahi.
///
/// Opciones:
///   - Configurar escena XR: mete el rig VR + el simulador y apaga la camara /
///     el jugador de prueba.
///   - Hacer agarrables las cajas: a las mismas cajas de la sala (tag "Caja")
///     les anade el agarre XR sin quitarles lo de empujar, para que funcionen
///     con ambas mecanicas.
///   - Montar demo completa: hace las dos cosas de golpe en la escena abierta.
///   - Montar demo VR en la Sala 2: abre Sala_2, monta la demo y la guarda.
/// </summary>
public static class XRSetupTool
{
    private const string MenuRoot = "Herramientas XR/";

    private const string NombreRig = "XR Origin (XR Rig)";
    private const string NombreSimulador = "XR Device Simulator";

    // El VR se usa SOLO en la Sala 2. Las salas 1 y 3 usan el jugador de teclado.
    private const string RutaSala2 = "Assets/Scenes/Sala_2.unity";

    // ---------------------------------------------------------------------
    // 1) Configurar la escena: rig + simulador + gestor de toggle (tecla M)
    // ---------------------------------------------------------------------
    [MenuItem(MenuRoot + "1. Configurar escena XR (rig + simulador + toggle M)", priority = 0)]
    public static GameObject ConfigurarEscenaXR()
    {
        GameObject rig = InstanciarPrefabPorNombre(NombreRig);
        if (rig == null) return null;

        GameObject sim = InstanciarPrefabPorNombre(NombreSimulador);

        // Localizar el jugador de teclado (para el toggle, NO lo borramos: la
        // tecla M alternara entre el y el rig VR).
        GameObject jugador = null;
        var spc = Object.FindFirstObjectByType<SimplePlayerController>(FindObjectsInactive.Include);
        if (spc != null) jugador = spc.gameObject;

        // Localizar una camara "normal" suelta (la Main Camera) que no sea del
        // rig ni parte del jugador, para que el toggle tambien la controle.
        GameObject camaraNormal = null;
        foreach (var cam in Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (cam.transform.IsChildOf(rig.transform)) continue;
            if (jugador != null && cam.transform.IsChildOf(jugador.transform)) continue;
            camaraNormal = cam.gameObject;
            break;
        }

        // Crear (o reutilizar) el gestor de toggle y asignarle las referencias.
        var toggle = Object.FindFirstObjectByType<XRToggle>(FindObjectsInactive.Include);
        if (toggle == null)
        {
            var mgr = new GameObject("XR Toggle Manager");
            Undo.RegisterCreatedObjectUndo(mgr, "Crear XR Toggle Manager");
            toggle = Undo.AddComponent<XRToggle>(mgr);
        }
        toggle.ConfigurarReferencias(rig, sim, jugador, camaraNormal);
        EditorUtility.SetDirty(toggle);

        // Estado por defecto: VR APAGADO (se activa con M en Play).
        rig.SetActive(false);
        if (sim != null) sim.SetActive(false);

        MarcarEscenaSucia();
        Selection.activeGameObject = toggle.gameObject;
        Debug.Log("[XRSetupTool] Escena XR lista. En Play, pulsa M para activar/desactivar el VR.", toggle);
        return rig;
    }

    // ---------------------------------------------------------------------
    // 2) Hacer agarrables las cajas que YA existen en la escena (tag "Caja").
    //    Son las mismas cajas de empujar/entregar, asi funcionan con ambas
    //    mecanicas: empujarlas (fisica) y agarrarlas (XR).
    // ---------------------------------------------------------------------
    [MenuItem(MenuRoot + "2. Hacer agarrables las cajas de la escena (tag Caja)", priority = 20)]
    public static int HacerCajasAgarrables()
    {
        var cajas = GameObject.FindGameObjectsWithTag("Caja");
        if (cajas.Length == 0)
        {
            Debug.LogWarning("[XRSetupTool] No se encontraron objetos con el tag 'Caja' en la escena.");
            return 0;
        }

        foreach (var caja in cajas)
            HacerAgarrable(caja);

        MarcarEscenaSucia();
        Debug.Log($"[XRSetupTool] {cajas.Length} caja(s) ahora son agarrables en XR (siguen siendo empujables).");
        return cajas.Length;
    }

    /// <summary>Anade a un objeto los componentes para agarrarlo en XR, sin
    /// tocar los que ya tenga (PushableBox, etc.).</summary>
    public static GameObject HacerAgarrable(GameObject go)
    {
        if (go == null) return null;
        Undo.RegisterFullObjectHierarchyUndo(go, "Hacer agarrable");

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

        return go;
    }

    // ---------------------------------------------------------------------
    // 3) Todo en uno: escena XR + hacer agarrables las cajas existentes
    // ---------------------------------------------------------------------
    [MenuItem(MenuRoot + "3. Montar demo completa (rig + simulador + cajas)", priority = 40)]
    public static void MontarDemoCompleta()
    {
        GameObject rig = ConfigurarEscenaXR();
        if (rig == null) return;

        HacerCajasAgarrables();

        MarcarEscenaSucia();
        Debug.Log("[XRSetupTool] Demo XR lista. En Play pulsa M para activar el VR y agarra las mismas cajas de la sala con el simulador.");
    }

    // ---------------------------------------------------------------------
    // 4) Directo a lo que pide el proyecto: montar la demo VR en la SALA 2
    //    (abre la escena Sala_2, monta rig + simulador + caja y la guarda).
    // ---------------------------------------------------------------------
    [MenuItem(MenuRoot + "4. Montar demo VR en la Sala 2", priority = 60)]
    public static void MontarEnSala2()
    {
        // Guarda cambios pendientes de la escena actual antes de cambiar.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (AssetDatabase.LoadAssetAtPath<Object>(RutaSala2) == null)
        {
            Debug.LogError($"[XRSetupTool] No se encontro la escena '{RutaSala2}'.");
            return;
        }

        var escena = EditorSceneManager.OpenScene(RutaSala2, OpenSceneMode.Single);
        MontarDemoCompleta();
        EditorSceneManager.SaveScene(escena);
        Debug.Log("[XRSetupTool] Demo VR montada y guardada en la Sala 2.");
    }

    // Validaciones: los menus se ven en gris si faltan los samples.
    [MenuItem(MenuRoot + "1. Configurar escena XR (rig + simulador + toggle M)", validate = true)]
    private static bool ValidarSamples() => BuscarPrefabPorNombre(NombreRig) != null;

    [MenuItem(MenuRoot + "4. Montar demo VR en la Sala 2", validate = true)]
    private static bool ValidarSala2() => BuscarPrefabPorNombre(NombreRig) != null;

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
