# ZendaGenesis — Paquete de iconos v1

Identidad visual: Z dorada sobre cristal amatista, marco de fantasía y hojas que evocan crecimiento. Paleta inspirada en la captura del juego suministrada. Preparado el 27 de octubre de 2026.

## Archivos principales

| Uso | Archivo |
| --- | --- |
| Icono general, fondo opaco | `PNG/ZendaGenesis_AppIcon_1024.png` |
| Logo para interfaz, fondo transparente | `PNG/ZendaGenesis_Logo_Transparent_1024.png` |
| Windows, contorno transparente | `Windows/ZendaGenesis.ico` |
| Windows, fondo cuadrado violeta | `Windows/ZendaGenesis_Square.ico` |
| Android en Unity, icono Legacy | `Android/Unity/ZendaGenesis_Legacy_1024.png` |
| Android en Unity, capa Foreground | `Android/Unity/ZendaGenesis_Adaptive_Foreground_1024.png` |
| Android en Unity, capa Background | `Android/Unity/ZendaGenesis_Adaptive_Background_1024.png` |
| Imagen cuadrada para ficha de la aplicación | `Android/ZendaGenesis_Store_512.png` |
| Web, favicon e iconos | Carpeta `Web/` |

Se incluyen originales de 1254 × 1254 px en `Master/`. Las demás resoluciones son exportaciones reducidas. El emblema es raster; no se incluye un SVG vectorizado.

## Integración en Unity

1. Copia los PNG que necesites a una carpeta de tu proyecto, por ejemplo `Assets/Branding/ZendaGenesis/`.
2. Abre `Edit > Project Settings > Player` y asigna `ZendaGenesis_AppIcon_1024.png` a **Default Icon**.
3. Para Windows, si utilizas la sección de iconos específicos de la plataforma, asigna el PNG transparente de 1024 px o los PNG de `Windows/PNG/` a los tamaños correspondientes. Unity utiliza texturas PNG para estos campos; el ICO también sirve para herramientas externas que acepten ese formato.
4. En la plataforma Android, abre **Icon**, habilita la personalización de iconos si tu versión lo requiere y asigna el PNG Legacy a los campos Legacy. En los campos Adaptive, asigna la capa `Adaptive_Foreground` a **Foreground** y `Adaptive_Background` a **Background**. Repite las mismas texturas en los tamaños solicitados; Unity las puede reducir al tamaño de destino.
5. Conserva el alfa de las imágenes transparentes. Para usarlas también dentro de la interfaz puedes importarlas como `Sprite (2D and UI)`; usa compresión desactivada para conservar la nitidez del logo.
6. Vuelve a compilar el juego para incluir los iconos. No se modifica un EXE o APK ya generado. Si el escritorio conserva el icono anterior, comprueba el archivo recién compilado y actualiza o recrea su acceso directo.

Los nombres y la organización de los campos pueden variar según la versión de Unity. Para Android en Unity utiliza las imágenes de `Android/Unity/`; los recursos nativos de la siguiente sección son una alternativa para proyectos Android exportados o nativos.

## Android nativo o proyecto exportado

`Android/Native/res/` contiene iconos Legacy por densidad (48, 72, 96, 144 y 192 px), capas Foreground (108, 162, 216, 324 y 432 px), un recurso de color y la definición Adaptive para API 26 o superior. El emblema del Foreground se ha reducido y centrado para respetar el área segura del icono adaptable.

Integra los recursos en el módulo Android correspondiente sin sustituir archivos propios sin revisar. Si ya existen recursos llamados `ic_launcher`, combina o renombra los recursos y sus referencias. El elemento `application` de tu manifiesto debe referirse al icono con `android:icon="@mipmap/ic_launcher"`. Si ya hay un atributo `android:roundIcon`, comprueba que apunte al recurso que quieres utilizar. No añadas un segundo elemento `application`.

Este paquete incluye las capas Adaptive en color. No incluye una capa Monochrome específica para iconos temáticos.

## Windows y web

Los ICO contienen imágenes de 16, 24, 32, 48, 64, 128 y 256 px dentro de un mismo archivo. La versión transparente deja visible la silueta del emblema; la versión Square conserva el fondo violeta. En WebGL, configura el favicon en la plantilla web de tu juego utilizando los archivos de `Web/`.

## Verificación y alcance

Se han comprobado dimensiones, canales alfa, estructura de los ICO y referencias de los recursos Android. Las imágenes se han revisado visualmente. El paquete no se ha compilado dentro de tu proyecto Unity ni se ha probado en un dispositivo; revisa el resultado del build en tus plataformas de destino.

## Referencias de integración

- [Unity: Player settings](https://docs.unity3d.com/Manual/class-PlayerSettings.html)
- [Unity: Android Player settings](https://docs.unity3d.com/Manual/class-PlayerSettingsAndroid.html)
- [Android: Adaptive icons](https://developer.android.com/develop/ui/compose/system/icon_design_adaptive)

Las especificaciones de las dos generaciones visuales se incluyen en `DESIGN_PROMPTS.txt`. El diseño se creó con la herramienta integrada de generación de imágenes; los formatos y tamaños del paquete se exportaron después.
