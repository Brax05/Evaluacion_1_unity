# Escape Room XR — Evaluación Unity

Prototipo de **escape room en primera persona** hecho en **Unity 6 (6000.5.1f1)** con
**URP** e integración de **XR Interaction Toolkit** para interacciones VR.

El jugador avanza por **3 salas** encadenadas. En cada sala hay que resolver un
reto (recoger paquetes, entregar cajas, accionar palancas, resolver el puzzle de
la puerta) para **abrir la salida** y pasar a la siguiente sala. Al superar la
**Sala 3** aparece el mensaje **"¡GANASTE!"** y el juego **vuelve al menú principal**.

---

## Cómo jugar / probar

- **WASD + ratón**: moverse y mirar (jugador de prueba sin casco).
- **Clic / gatillo XR**: interactuar (palancas, objetos de info).
- **Objetivo**: completar el reto de cada sala → cruzar la puerta → llegar a la Sala 3 → ganar.
- Pulsa **Play** desde la escena `Menu` para empezar desde el principio.

---

## Flujo de escenas (Menú → Salas → Victoria)

Orden en **File ▸ Build Settings** (índices):

| Índice | Escena | Ruta |
|--------|--------|------|
| 0 | Menú principal | [Assets/Scenes/Menu.unity](Assets/Scenes/Menu.unity) |
| 1 | Sala 1 | [Assets/Scenes/Sala_1.unity](Assets/Scenes/Sala_1.unity) |
| 2 | Sala 2 | [Assets/Scenes/Sala_2.unity](Assets/Scenes/Sala_2.unity) |
| 3 | Sala 3 (final) | [Assets/Scenes/Sala_3.unity](Assets/Scenes/Sala_3.unity) |

**Ruta que salta del menú a las salas:**

1. En `Menu`, el botón **JUGAR** llama a `MenuPrincipal.Jugar()`, que carga la
   escena `Sala_1` (campo `primeraSala`).
   → [Assets/Scripts/UI/MenuPrincipal.cs](Assets/Scripts/UI/MenuPrincipal.cs)
2. En cada sala, al cruzar la salida se dispara `CargadorSiguienteSala`, que carga
   la **siguiente escena por índice** de Build Settings (Sala_1 → Sala_2 → Sala_3).
   → [Assets/Scripts/Core/CargadorSiguienteSala.cs](Assets/Scripts/Core/CargadorSiguienteSala.cs)
3. En la **Sala 3** ese mismo componente está marcado como `esSalaFinal` con
   `nombreEscenaVictoria = "Menu"`: muestra el mensaje **"¡GANASTE!"** y, tras un
   breve retardo, **regresa a `Menu`**.

---

## Rutas del código

Todos los scripts están en [Assets/Scripts/](Assets/Scripts/), organizados por carpeta:

### Core — sistema y flujo
- [Scripts/Core/GameManager.cs](Assets/Scripts/Core/GameManager.cs) — Singleton central. Lleva los contadores (paquetes/cajas), objetivos y lanza eventos C# a los que reacciona la UI y los objetos.
- [Scripts/Core/CargadorSiguienteSala.cs](Assets/Scripts/Core/CargadorSiguienteSala.cs) — Zona de salida de cada sala; carga la siguiente escena o, en la sala final, muestra victoria y vuelve al menú.
- [Scripts/Core/GameEventRelay.cs](Assets/Scripts/Core/GameEventRelay.cs) — Puente entre los eventos del GameManager y los UnityEvents del Inspector (reaccionar sin escribir código).
- [Scripts/Core/SimplePlayerController.cs](Assets/Scripts/Core/SimplePlayerController.cs) — Jugador en primera persona (teclado+ratón) para probar sin XR.

### UI — interfaz
- [Scripts/UI/MenuPrincipal.cs](Assets/Scripts/UI/MenuPrincipal.cs) — Botones **Jugar / Salir** del menú.
- [Scripts/UI/UIManager.cs](Assets/Scripts/UI/UIManager.cs) — Conecta el GameManager con el Canvas: contadores, mensajes, barra de progreso y panel final.

### Interacciones — retos y objetos
- [Scripts/Interacciones/Package.cs](Assets/Scripts/Interacciones/Package.cs) — Paquete coleccionable (trigger que suma al contador y desaparece).
- [Scripts/Interacciones/DeliveryZone.cs](Assets/Scripts/Interacciones/DeliveryZone.cs) — Zona de entrega donde se deja una caja (empujada o soltada con mandos VR).
- [Scripts/Interacciones/PushableBox.cs](Assets/Scripts/Interacciones/PushableBox.cs) — Caja empujable con física real.
- [Scripts/Interacciones/LeverInteractable.cs](Assets/Scripts/Interacciones/LeverInteractable.cs) — Palanca de dos estados (encender/apagar) con eventos.
- [Scripts/Interacciones/GateController.cs](Assets/Scripts/Interacciones/GateController.cs) — Puerta/portón corredera que se abre y cierra.
- [Scripts/Interacciones/ExitDoorPuzzle.cs](Assets/Scripts/Interacciones/ExitDoorPuzzle.cs) — Puzzle de la puerta de salida con indicadores que cambian de color.
- [Scripts/Interacciones/InfoObject.cs](Assets/Scripts/Interacciones/InfoObject.cs) — Objeto que muestra información al tocarlo/seleccionarlo.
- [Scripts/Interacciones/MessageZone.cs](Assets/Scripts/Interacciones/MessageZone.cs) — Zona trigger que muestra un mensaje al entrar/salir.
- [Scripts/Interacciones/ShrinkZone.cs](Assets/Scripts/Interacciones/ShrinkZone.cs) — Zona que encoge un objeto mientras el jugador permanece dentro.

### XR — realidad virtual
- [Scripts/XR/XRGrabReporter.cs](Assets/Scripts/XR/XRGrabReporter.cs) — Puente entre los eventos de los mandos XR (agarrar, soltar, gatillo) y el GameManager. Se coloca en un objeto con **XR Grab Interactable**. Funciona con casco real o con el simulador del editor.

---

## Prefabs

Todos en [Assets/Prefabs/](Assets/Prefabs/):

| Prefab | Descripción |
|--------|-------------|
| [Caja_01.prefab](Assets/Prefabs/Caja_01.prefab) | Caja que se empuja / agarra y se entrega. |
| [Coleccionable_01.prefab](Assets/Prefabs/Coleccionable_01.prefab) | Paquete recolectable. |
| [Pad_Entrega_01.prefab](Assets/Prefabs/Pad_Entrega_01.prefab) | Plataforma de zona de entrega. |
| [Puerta.prefab](Assets/Prefabs/Puerta.prefab) | Puerta / portón de salida. |
| [Palanca.prefab](Assets/Prefabs/Palanca.prefab) | Palanca interactiva. |
| [Indicador_A.prefab](Assets/Prefabs/Indicador_A.prefab) | Indicador del puzzle de la puerta. |
| [Indicador_B.prefab](Assets/Prefabs/Indicador_B.prefab) | Indicador del puzzle de la puerta. |

---

## Otras carpetas

- [Assets/Scenes/](Assets/Scenes/) — Escenas (menú y salas).
- [Assets/Materials/](Assets/Materials/) — Materiales del entorno.
- [Assets/UI/](Assets/UI/) — Recursos de interfaz.
- [Assets/XRI/](Assets/XRI/) — Ajustes del XR Interaction Toolkit.
- [Assets/Settings/](Assets/Settings/) — Configuración de URP.

---

## Integración XR/VR (Paso 8) — solo en la Sala 2

El VR se usa **únicamente en la Sala 2**. Las salas 1 y 3 se juegan con el
jugador normal de teclado/ratón (`SimplePlayerController`).

El proyecto incluye el paquete **XR Interaction Toolkit**, los samples
(**Starter Assets** + **XR Device Simulator**) y el script
[XRGrabReporter.cs](Assets/Scripts/XR/XRGrabReporter.cs), que reenvía los eventos
de los mandos (agarrar / soltar / gatillo) al GameManager.

**Activar/desactivar VR con la tecla M:** el script
[XRToggle.cs](Assets/Scripts/XR/XRToggle.cs) alterna en tiempo de ejecución entre
el modo VR (rig + simulador) y el jugador normal de teclado. La escena arranca
con el jugador normal; **pulsa `M` para activar el VR** y `M` otra vez para
volver. Se coloca automáticamente al usar la herramienta.

**Montaje automático:** en la barra de menús de Unity hay una herramienta
**`Herramientas XR`** ([XRSetupTool.cs](Assets/Scripts/XR/Editor/XRSetupTool.cs)):

- **`4. Montar demo VR en la Sala 2`** → abre `Sala_2`, coloca el rig VR
  (`XR Origin`), el simulador y el gestor del toggle `M`, hace agarrables **las
  mismas cajas de la sala** (tag `Caja`) y guarda la escena.
- Las opciones 1-3 permiten montar las piezas por separado en la escena abierta.

**Las cajas funcionan con ambas mecánicas:** son las mismas cajas de empujar /
entregar de la sala; al añadirles `XR Grab Interactable` + `XRGrabReporter` (sin
quitarles `PushableBox`) se pueden **empujar** con el jugador normal **y agarrar**
con los mandos VR.

**Probar sin casco:** con el **XR Device Simulator** se controlan la cámara y los
mandos con teclado/ratón dentro del editor. Pulsa **Play** en `Sala_2`, luego `M`
para entrar en VR, agarra una caja y suéltala sobre una `DeliveryZone`.

