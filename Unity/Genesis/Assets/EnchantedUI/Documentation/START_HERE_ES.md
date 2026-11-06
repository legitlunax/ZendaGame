# Enchanted UI — kit para Unity

Un tema de fantasía suave en violeta y dorado, basado en la propuesta de tu pantalla de conversación. Contiene piezas de interfaz reales, texto editable y un constructor de prefabs. No es una captura colocada encima del juego.

## Instalación rápida

1. Descomprime este ZIP fuera de tu proyecto.
2. Con Unity abierto, importa `EnchantedUI.unitypackage` mediante **Assets > Import Package > Custom Package**. Como alternativa, copia la carpeta `Assets/EnchantedUI` a la carpeta `Assets` de tu proyecto. Usa solo una de estas dos rutas.
3. Espera a que termine la compilación. Requiere **Unity UI / uGUI** (`com.unity.ugui`). El kit no instala ni cambia paquetes automáticamente.
4. Selecciona **Tools > Enchanted UI > 2 - Build Prefabs and Demo Scene**. Este comando configura los sprites, crea diez prefabs, un tema y una escena de ejemplo en una carpeta nueva dentro de `Assets/EnchantedUI/Generated/Build_001`.
5. Abre **ConversationDemo.unity** por sí sola y pulsa **Play**. Los botones Previous/Continue recorren cuatro textos de ejemplo.

El constructor conserva tus escenas abiertas y no las guarda ni añade objetos a ellas. Cada ejecución crea una carpeta Build nueva para conservar las anteriores. La primera opción del menú permite reconfigurar únicamente los sprites del kit.

## Qué incluye

- 4 PNG ilustrados con transparencia: panel, botón, adorno floral y marco hexagonal.
- 9 PNG funcionales: seis iconos, separador, pista y relleno de progreso.
- Bordes 9-slice en los archivos `.meta` y en el configurador de importación.
- Estados de botón mediante tintes y feedback de pulsación, sin necesitar imágenes distintas para cada estado.
- Constructor de diez prefabs y una escena de conversación.
- 9 scripts C# legibles, editables y separados entre Runtime y Editor.
- Dos fuentes DejaVu incluidas con sus licencias; no dependen de las fuentes instaladas en el dispositivo.
- Lámina `Documentation/CONTACT_SHEET.jpg` con los recursos reales, referencia visual, guía técnica y prompt para tu agente.

## Los diez prefabs generados

| Prefab | Uso |
| --- | --- |
| Panel | Ventanas, diálogos y contenedores escalables |
| Header | Título y contador de recursos |
| PortraitCard | Retrato y franja de nombre |
| DialoguePanel | Hablante, separador y texto desplazable |
| PrimaryButton | Acción principal |
| SecondaryButton | Acción secundaria |
| DisabledButton | Ejemplo de botón deshabilitado |
| HexTab | Acceso de personaje o sección |
| NavigationTray | Tres accesos hexagonales |
| ConversationScreen | Pantalla completa de presentación, sin controlador narrativo de demostración |

## Integrarlo con tu juego

Usa primero la escena de ejemplo para inspeccionar el diseño. Después puedes arrastrar prefabs individuales a tu Canvas existente. `ConversationScreen` tiene su propio Canvas: no lo anides dentro de otro Canvas sin revisar su configuración.

En el componente **EnchantedDialogueView**, asigna tus retratos y fondo con `SetPortraits(...)` y `SetBackground(...)`, y conecta los eventos `continueRequested`, `previousRequested` y `backRequested` a tu controlador narrativo. Los accesos hexagonales son controles genéricos: sus acciones de juego se asignan en sus eventos Button.onClick.

El fondo del mundo y tus personajes no están incluidos como assets separados: usa los originales de tu proyecto. La escena de ejemplo tiene un degradado ligero y espacios para tus retratos. `Documentation/STYLE_REFERENCE.png` es solo una referencia visual y no se importa dentro de Assets.

Si tu juego utiliza UI Toolkit, puedes reutilizar los PNG y sus medidas, pero los prefabs y scripts de este kit son para **uGUI**. Si utiliza TextMeshPro, conserva TMP y adapta las referencias de texto con tu agente; el ejemplo usa `UnityEngine.UI.Text` para no exigir el paquete o los recursos esenciales de TMP.

## Ajustar el aspecto

Los valores iniciales se encuentran en `Runtime/EnchantedTheme.cs`. El constructor genera un ScriptableObject con esos valores y los copia a los prefabs. **El tema es una plantilla de construcción, no un enlace de actualización global en tiempo real.** Edita los componentes/prefabs generados para ajustar una instancia, o cambia los valores iniciales antes de generar una nueva versión.

Los marcos se escalan con Image Type = Sliced y su **Pixels Per Unit Multiplier**. No uses Set Native Size para estas piezas: sus resoluciones de origen son mayores que su tamaño previsto de interfaz. El adorno floral puede desactivarse sin afectar al funcionamiento.

## Plataformas y verificación

El código está escrito para Unity 2021.3 LTS o posterior, con uGUI, y evita shaders personalizados, bibliotecas de escritorio y acceso al sistema de archivos en ejecución. Su intención es servir para Windows, Android y WebGL.

Se verificaron los PNG, transparencia, valores 9-slice, sintaxis C#, integridad del paquete y la apariencia de las piezas escaladas en una composición externa. **No se ejecutó Unity, no se compiló el proyecto con sus assemblies y no se probaron builds en Windows, Android o WebGL.** Importa y prueba en tu versión de Unity antes de integrarlo definitivamente.

Revisa la lista de comprobaciones en `Documentation/UNITY_INTEGRATION.md`. El prompt preparado para tu agente está en `INTEGRATION_PROMPT.txt`.
