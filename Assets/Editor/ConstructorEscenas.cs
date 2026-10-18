using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Herramienta del editor: arma las 3 escenas del juego (Menu, SeleccionPlanta y Dia1) con un clic.
//
// Uso: menu superior de Unity  ->  Plantas  ->  1. Construir todo (primera vez)
//
// Todo lo que crea queda como objetos normales en la Jerarquia (Hierarchy), asi que despues
// puedes moverlos, cambiarles el tamano, etc. desde el editor como cualquier otro objeto.
//
// Sistema de coordenadas: todo se coloca dentro del objeto "Pantalla" (320 x 572 px, el tamano del arte).
// Las posiciones (x, yArriba) se miden desde la esquina SUPERIOR IZQUIERDA, igual que en un editor de imagenes.
public static class ConstructorEscenas
{
    const string RutaEscenas = "Assets/Scenes";
    const float AnchoPantalla = 320f;
    const float AltoPantalla = 572f;

    static readonly Vector2 ArribaIzq = new Vector2(0f, 1f);
    static readonly Vector2 Centro = new Vector2(0.5f, 0.5f);
    static readonly Vector2 AbajoCentro = new Vector2(0.5f, 0f);

    static readonly Color Crema = new Color(1f, 0.945f, 0.816f, 1f);
    static readonly Color Contorno = new Color(0.23f, 0.12f, 0.05f, 1f);

    // ------------------------------------------------------------------ menus del editor

    [MenuItem("Plantas/1. Construir todo (primera vez)", false, 1)]
    public static void ConstruirTodo()
    {
        if (!EditorUtility.DisplayDialog("Construir juego",
                "Voy a configurar los sprites y crear las escenas Menu, SeleccionPlanta y Dia1.\n\n" +
                "Si ya habías modificado esas escenas a mano, se reemplazarán.",
                "Continuar", "Cancelar"))
        {
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        ConfigurarSprites();
        ConfigurarJugador();
        CrearMenu();
        CrearSeleccion();
        CrearDia1();
        RegistrarEscenasEnBuild();

        EditorSceneManager.OpenScene(RutaEscenas + "/Menu.unity");
        EditorUtility.DisplayDialog("Listo",
            "Escenas creadas en Assets/Scenes.\n\nAbre la escena Menu y presiona Play.", "OK");
    }

    [MenuItem("Plantas/2. Reconstruir solo las escenas", false, 2)]
    public static void ReconstruirEscenas()
    {
        if (!EditorUtility.DisplayDialog("Reconstruir escenas",
                "Se reemplazarán las escenas Menu, SeleccionPlanta y Dia1 (perderás cambios hechos a mano en ellas).",
                "Continuar", "Cancelar"))
        {
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        CrearMenu();
        CrearSeleccion();
        CrearDia1();
        RegistrarEscenasEnBuild();
        EditorSceneManager.OpenScene(RutaEscenas + "/Menu.unity");
    }

    [MenuItem("Plantas/3. Solo configurar sprites", false, 3)]
    public static void SoloSprites()
    {
        ConfigurarSprites();
        EditorUtility.DisplayDialog("Listo", "Sprites configurados (pixel art, sin filtro ni compresión).", "OK");
    }

    // ------------------------------------------------------------------ configuracion general

    // Deja todos los PNG de Assets/Resources/Sprites como sprites de pixel art nitidos.
    static void ConfigurarSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Sprites" });
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
                if (imp == null) continue;

                bool cambio = false;
                if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; cambio = true; }
                if (imp.spriteImportMode != SpriteImportMode.Single) { imp.spriteImportMode = SpriteImportMode.Single; cambio = true; }
                if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; cambio = true; }
                if (imp.textureCompression != TextureImporterCompression.Uncompressed) { imp.textureCompression = TextureImporterCompression.Uncompressed; cambio = true; }
                if (imp.mipmapEnabled) { imp.mipmapEnabled = false; cambio = true; }
                if (imp.wrapMode != TextureWrapMode.Clamp) { imp.wrapMode = TextureWrapMode.Clamp; cambio = true; }
                if (!Mathf.Approximately(imp.spritePixelsPerUnit, 100f)) { imp.spritePixelsPerUnit = 100f; cambio = true; }

                if (cambio) imp.SaveAndReimport();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
    }

    static void ConfigurarJugador()
    {
        // Ventana de PC con proporcion de celular (320x572 x 1.5)
        PlayerSettings.defaultScreenWidth = 480;
        PlayerSettings.defaultScreenHeight = 858;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
    }

    static void RegistrarEscenasEnBuild()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(RutaEscenas + "/Menu.unity", true),
            new EditorBuildSettingsScene(RutaEscenas + "/SeleccionPlanta.unity", true),
            new EditorBuildSettingsScene(RutaEscenas + "/Dia1.unity", true),
        };
    }

    // ------------------------------------------------------------------ escena: Menu

    static void CrearMenu()
    {
        RectTransform pantalla = NuevaEscena();

        // El fondo mide 320x589; se sube 8 px para que sobre un poco arriba y abajo
        CrearImagen("Fondo", pantalla, "Menu/FondoMenu", 0f, -8f, ArribaIzq);
        CrearTexto("Titulo", pantalla, ConfigJuego.TituloJuego, 24, 160f, 76f, 200f, 80f);

        Button jugar = CrearBoton("BotonJugar", pantalla, "Jugar", 160f, 412f);
        Button salir = CrearBoton("BotonSalir", pantalla, "Salir", 160f, 462f);

        GameObject controlador = new GameObject("MenuPrincipal");
        MenuPrincipal menu = controlador.AddComponent<MenuPrincipal>();
        menu.botonJugar = jugar;
        menu.botonSalir = salir;

        GuardarEscena("Menu");
    }

    // ------------------------------------------------------------------ escena: SeleccionPlanta

    static void CrearSeleccion()
    {
        RectTransform pantalla = NuevaEscena();

        CrearImagen("Fondo", pantalla, "Menu/FondoMenu", 0f, -8f, ArribaIzq);
        CrearTexto("Titulo", pantalla, "Elige tu planta", 24, 160f, 76f, 200f, 80f);

        // Panel oscuro semitransparente para que las plantas se vean sobre el fondo
        CrearPanel("PanelPlantas", pantalla, 160f, 269f, 292f, 258f, new Color(0f, 0f, 0f, 0.4f), false);

        float[] columnasFila1 = { 58f, 126f, 194f, 262f };
        float[] columnasFila2 = { 92f, 160f, 228f };

        Button[] botonesPlantas = new Button[ConfigJuego.TotalPlantas];
        for (int i = 0; i < ConfigJuego.TotalPlantas; i++)
        {
            bool primeraFila = i < 4;
            float x = primeraFila ? columnasFila1[i] : columnasFila2[i - 4];
            float yBase = primeraFila ? 262f : 378f;

            Image img = CrearImagen("Planta" + (i + 1), pantalla, "Menu/Planta" + (i + 1), x, yBase, AbajoCentro, true);
            Button boton = img.gameObject.AddComponent<Button>();
            boton.targetGraphic = img;
            botonesPlantas[i] = boton;
        }

        Text nombre = CrearTexto("TextoNombre", pantalla, "Toca una planta", 16, 160f, 418f, 280f, 24f);
        Button elegir = CrearBoton("BotonElegir", pantalla, "Elegir", 160f, 452f);
        Button volver = CrearBoton("BotonVolver", pantalla, "Volver", 160f, 492f);

        GameObject controlador = new GameObject("SeleccionPlantas");
        SeleccionPlantas seleccion = controlador.AddComponent<SeleccionPlantas>();
        seleccion.botonesPlantas = botonesPlantas;
        seleccion.botonElegir = elegir;
        seleccion.botonVolver = volver;
        seleccion.textoNombre = nombre;

        GuardarEscena("SeleccionPlanta");
    }

    // ------------------------------------------------------------------ escena: Dia1

    static void CrearDia1()
    {
        RectTransform pantalla = NuevaEscena();

        // Fondo y nubes animadas (31 cuadros)
        CrearImagen("Fondo", pantalla, "Dia/EscenarioDia", 0f, 0f, ArribaIzq);
        Image nubes = CrearImagen("Nubes", pantalla, "Dia/Anim/Nubes/Nubes_00", 0f, 40f, ArribaIzq);
        AnimadorSprites animNubes = nubes.gameObject.AddComponent<AnimadorSprites>();
        animNubes.carpetaAlIniciar = "Dia/Anim/Nubes";
        animNubes.fps = 6f;
        animNubes.repetirAlIniciar = true;

        // Zona (invisible) donde se sueltan las herramientas
        RectTransform zonaUso = CrearRect("ZonaUso", pantalla, Centro, 160f, 370f, 120f, 120f);

        // La maceta: el punto de anclaje esta abajo al centro, justo sobre la mancha oscura del pasto
        Image planta = CrearImagen("Planta", pantalla, "Dia/Planta-EstadoInicial", 160f, 416f, AbajoCentro);
        AnimadorSprites animPlanta = planta.gameObject.AddComponent<AnimadorSprites>();

        // Animacion de la herramienta usada (pala / regadera) sobre la planta
        Image efecto = CrearImagen("EfectoHerramienta", pantalla, "Dia/Anim/PalaAnima2/PalaAnima2_00", 140f, 328f, ArribaIzq);
        AnimadorSprites animEfecto = efecto.gameObject.AddComponent<AnimadorSprites>();

        // Globo de pensamiento: la colita (punta inferior izquierda) queda junto a la planta
        Image globo = CrearImagen("GloboPensamiento", pantalla, "Dia/GloboPensamiento1", 178f, 372f,
                                  new Vector2(40f / 128f, 1f - 102f / 128f));
        Image icono = CrearImagen("IconoNecesidad", globo.transform, "Dia/BolsaDeTierra", 79f, 51f, Centro);

        // Textos
        Text textoDia = CrearTexto("TextoDia", pantalla, "Día 1", 20, 160f, 24f, 200f, 30f);
        Text textoAyuda = CrearTexto("TextoAyuda", pantalla, "Arrastra la herramienta correcta hasta la planta", 11, 160f, 462f, 280f, 28f);

        // Barra de herramientas
        CrearPanel("PanelHerramientas", pantalla, 160f, 520f, 300f, 86f, new Color(0f, 0f, 0f, 0.35f), false);
        Image regadera = CrearImagen("Regadera", pantalla, "Dia/Regadera2", 70f, 513f, Centro, true);
        Image pala = CrearImagen("Pala", pantalla, "Dia/Pala", 160f, 513f, Centro, true);
        Image tijera = CrearImagen("Tijera", pantalla, "Dia/Tijera", 250f, 513f, Centro, true);
        CrearTexto("EtiquetaRegadera", pantalla, "Regadera", 11, 70f, 548f, 90f, 16f);
        CrearTexto("EtiquetaPala", pantalla, "Pala", 11, 160f, 548f, 90f, 16f);
        CrearTexto("EtiquetaTijera", pantalla, "Tijera", 11, 250f, 548f, 90f, 16f);

        // Panel de "Dia completado" (empieza oculto)
        RectTransform panelFin = CrearPanel("PanelFin", pantalla, 160f, 286f, AnchoPantalla, AltoPantalla, new Color(0f, 0f, 0f, 0.65f), true);
        CrearTexto("TituloFin", panelFin, "¡Día 1 completado!", 24, 160f, 150f, 280f, 40f);
        Image plantaFin = CrearImagen("PlantaElegida", panelFin, "Menu/Planta1", 160f, 300f, AbajoCentro);
        Text textoFin = CrearTexto("TextoFin", panelFin, "", 14, 160f, 345f, 280f, 60f);
        Button reintentar = CrearBoton("BotonReintentar", panelFin, "Repetir día", 160f, 430f);
        Button menu = CrearBoton("BotonMenu", panelFin, "Menú", 160f, 476f);
        panelFin.gameObject.SetActive(false);

        // Controlador del dia
        GameObject controlador = new GameObject("DiaPlanta");
        DiaPlanta dia = controlador.AddComponent<DiaPlanta>();
        dia.planta = planta;
        dia.animadorPlanta = animPlanta;
        dia.zonaUso = zonaUso;
        dia.globo = globo.rectTransform;
        dia.iconoGlobo = icono;
        dia.efecto = efecto;
        dia.animadorEfecto = animEfecto;
        dia.textoDia = textoDia;
        dia.textoAyuda = textoAyuda;
        dia.panelFin = panelFin.gameObject;
        dia.textoFin = textoFin;
        dia.imagenPlantaFin = plantaFin;
        dia.botonReintentar = reintentar;
        dia.botonMenu = menu;

        // Herramientas arrastrables
        PrepararHerramienta(regadera, TipoHerramienta.Regadera, dia, pantalla);
        PrepararHerramienta(pala, TipoHerramienta.Pala, dia, pantalla);
        PrepararHerramienta(tijera, TipoHerramienta.Tijera, dia, pantalla);

        GuardarEscena("Dia1");
    }

    static void PrepararHerramienta(Image imagen, TipoHerramienta tipo, DiaPlanta dia, RectTransform pantalla)
    {
        HerramientaArrastrable h = imagen.gameObject.AddComponent<HerramientaArrastrable>();
        h.tipo = tipo;
        h.dia = dia;
        h.pantalla = pantalla;
    }

    // ------------------------------------------------------------------ ayudantes para crear objetos

    // Crea una escena vacia con camara, Canvas, EventSystem y el contenedor "Pantalla" de 320x572.
    static RectTransform NuevaEscena()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        Camera cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler escalador = canvasGo.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(AnchoPantalla, AltoPantalla);
        escalador.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand; // siempre cabe completo, con barras negras si hace falta

        GameObject eventos = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        eventos.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        eventos.AddComponent<StandaloneInputModule>();
#endif

        GameObject pantallaGo = new GameObject("Pantalla", typeof(RectTransform), typeof(RectMask2D));
        RectTransform pantalla = pantallaGo.GetComponent<RectTransform>();
        pantalla.SetParent(canvasGo.transform, false);
        pantalla.anchorMin = Centro;
        pantalla.anchorMax = Centro;
        pantalla.pivot = Centro;
        pantalla.sizeDelta = new Vector2(AnchoPantalla, AltoPantalla);
        pantalla.anchoredPosition = Vector2.zero;
        return pantalla;
    }

    static void GuardarEscena(string nombre)
    {
        Directory.CreateDirectory(RutaEscenas);
        UnityEngine.SceneManagement.Scene escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(escena, RutaEscenas + "/" + nombre + ".unity");
    }

    static Sprite Spr(string ruta)
    {
        Sprite sprite = Resources.Load<Sprite>("Sprites/" + ruta);
        if (sprite == null)
        {
            Debug.LogError("No encontré el sprite 'Sprites/" + ruta + "'. Revisa que exista en Assets/Resources/Sprites.");
        }
        return sprite;
    }

    static Font ObtenerFuente()
    {
        // Si pones una fuente pixel art en Assets/Fonts/Fuente.ttf, se usa automaticamente
        Font fuente = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Fuente.ttf");
        if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return fuente;
    }

    static RectTransform CrearRect(string nombre, Transform padre, Vector2 pivote, float x, float yArriba, float ancho, float alto)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(padre, false);
        rt.anchorMin = ArribaIzq;
        rt.anchorMax = ArribaIzq;
        rt.pivot = pivote;
        rt.sizeDelta = new Vector2(ancho, alto);
        rt.anchoredPosition = new Vector2(x, -yArriba);
        return rt;
    }

    static Image CrearImagen(string nombre, Transform padre, string rutaSprite, float x, float yArriba, Vector2 pivote, bool raycast = false)
    {
        Sprite sprite = Spr(rutaSprite);
        Vector2 tamano = sprite != null ? sprite.rect.size : new Vector2(32f, 32f);
        RectTransform rt = CrearRect(nombre, padre, pivote, x, yArriba, tamano.x, tamano.y);
        Image img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = raycast;
        return img;
    }

    static RectTransform CrearPanel(string nombre, Transform padre, float xCentro, float yCentro, float ancho, float alto, Color color, bool bloqueaClics)
    {
        RectTransform rt = CrearRect(nombre, padre, Centro, xCentro, yCentro, ancho, alto);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = bloqueaClics;
        return rt;
    }

    static Text CrearTexto(string nombre, Transform padre, string contenido, int tamano, float xCentro, float yCentro, float ancho, float alto)
    {
        RectTransform rt = CrearRect(nombre, padre, Centro, xCentro, yCentro, ancho, alto);
        Text texto = rt.gameObject.AddComponent<Text>();
        texto.font = ObtenerFuente();
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Crema;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        texto.supportRichText = false;
        texto.raycastTarget = false;

        UnityEngine.UI.Outline contorno = rt.gameObject.AddComponent<UnityEngine.UI.Outline>();
        contorno.effectColor = Contorno;
        contorno.effectDistance = new Vector2(1f, -1f);
        return texto;
    }

    static Button CrearBoton(string nombre, Transform padre, string contenido, float xCentro, float yCentro)
    {
        Image img = CrearImagen(nombre, padre, "Menu/Boton", xCentro, yCentro, Centro, true);
        Button boton = img.gameObject.AddComponent<Button>();
        boton.targetGraphic = img;
        // El texto va centrado dentro del boton (128 x 34): su centro esta en (64, 17) desde la esquina del boton
        CrearTexto("Texto", img.transform, contenido, 16, 64f, 17f, 124f, 30f);
        return boton;
    }
}
