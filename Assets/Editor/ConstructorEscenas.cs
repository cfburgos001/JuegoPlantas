using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Herramienta del editor: arma las escenas del juego con un clic.
//
// Uso: menu superior de Unity  ->  Plantas  ->  1. Actualizar Dia y crear Noche
//
// Crea (o vuelve a crear) las escenas Dia1 y EscenaNoche, con su menu de pausa, y deja listos los sprites
// y el audio. NO toca Menu, SeleccionPlanta ni PantallaResultados.
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

    // Avisos que se muestran al final (por ejemplo, un audio que no se encontro)
    static readonly List<string> avisos = new List<string>();

    // ------------------------------------------------------------------ menus del editor

    [MenuItem("Plantas/1. Actualizar Día y crear Noche", false, 1)]
    public static void ActualizarDiaYNoche()
    {
        if (!EditorUtility.DisplayDialog("Actualizar Día y crear Noche",
                "Voy a:\n" +
                "  • configurar los sprites y el audio,\n" +
                "  • crear el prefab de sonidos,\n" +
                "  • reconstruir las escenas Dia1, EscenaNoche y PantallaResultados.\n\n" +
                "Si cambiaste algo a mano en esas tres escenas, se reemplazará.\n" +
                "Menu y SeleccionPlanta NO se tocan.",
                "Continuar", "Cancelar"))
        {
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        avisos.Clear();
        ConfigurarSprites();
        ConfigurarAudio();
        CrearPrefabSonidos();
        ConfigurarJugador();
        CrearDia1();
        CrearNoche();
        CrearResultados();
        RegistrarEscenasEnBuild();

        // Se deja abierta la escena Menu: el juego siempre empieza ahi
        if (File.Exists(RutaEscenas + "/Menu.unity")) EditorSceneManager.OpenScene(RutaEscenas + "/Menu.unity");
        InicioDesdeMenu.Aplicar();

        string mensaje = "Escenas Dia1, EscenaNoche y PantallaResultados creadas.\n\n" +
                         "Presiona Play para probar el juego completo (empieza en el Menú).";
        if (avisos.Count > 0) mensaje += "\n\nAvisos:\n  • " + string.Join("\n  • ", avisos.ToArray());
        EditorUtility.DisplayDialog("Listo", mensaje, "OK");
    }

    [MenuItem("Plantas/2. Solo configurar sprites y audio", false, 2)]
    public static void SoloSpritesYAudio()
    {
        avisos.Clear();
        ConfigurarSprites();
        ConfigurarAudio();
        CrearPrefabSonidos();

        string mensaje = "Sprites (pixel art, sin filtro ni compresión) y audio configurados.";
        if (avisos.Count > 0) mensaje += "\n\nAvisos:\n  • " + string.Join("\n  • ", avisos.ToArray());
        EditorUtility.DisplayDialog("Listo", mensaje, "OK");
    }

    // Solo para emergencias: vuelve a crear el menu y la seleccion de planta desde cero.
    [MenuItem("Plantas/Avanzado/Reconstruir Menú y SeleccionPlanta (reemplaza esas escenas)", false, 30)]
    public static void ReconstruirMenuYSeleccion()
    {
        if (!EditorUtility.DisplayDialog("Reconstruir Menú y SeleccionPlanta",
                "Se reemplazarán las escenas Menu y SeleccionPlanta por versiones nuevas.\n\n" +
                "OJO: la escena Menu tiene el objeto AdmAudio (la música). Al reconstruirla se pierde y " +
                "habría que volver a ponerlo. Úsalo solo si esas escenas se dañaron.",
                "Reemplazar", "Cancelar"))
        {
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        ConfigurarSprites();
        CrearMenu();
        CrearSeleccion();
        RegistrarEscenasEnBuild();
        EditorSceneManager.OpenScene(RutaEscenas + "/Menu.unity");
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

    // Deja en Build Settings las escenas en el orden del juego (solo las que existen)
    static void RegistrarEscenasEnBuild()
    {
        string[] orden = { "Menu", "SeleccionPlanta", "Dia1", "EscenaNoche", "PantallaResultados" };
        List<EditorBuildSettingsScene> lista = new List<EditorBuildSettingsScene>();
        foreach (string nombre in orden)
        {
            string ruta = RutaEscenas + "/" + nombre + ".unity";
            if (File.Exists(ruta)) lista.Add(new EditorBuildSettingsScene(ruta, true));
            else avisos.Add("No existe la escena " + nombre + " (no se agregó al Build Settings).");
        }
        EditorBuildSettings.scenes = lista.ToArray();
    }

    // ------------------------------------------------------------------ audio

    // Musica = streaming (no ocupa memoria); efectos = se descomprimen al cargar (suenan sin retraso).
    static void ConfigurarAudio()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Audio"))
        {
            avisos.Add("No encontré la carpeta Assets/Audio. El juego funcionará sin sonido.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string guid in guids)
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter imp = AssetImporter.GetAtPath(ruta) as AudioImporter;
                if (imp == null) continue;

                bool esMusica = Sonidos.Normalizar(ruta).Contains("musicadefondo");
                AudioClipLoadType tipo = esMusica ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;

                AudioImporterSampleSettings ajustes = imp.defaultSampleSettings;
                if (ajustes.loadType != tipo)
                {
                    ajustes.loadType = tipo;
                    imp.defaultSampleSettings = ajustes;
                    imp.SaveAndReimport();
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
    }

    // Guarda todos los efectos de sonido en el prefab Assets/Resources/Sonidos.prefab
    // (la clase Sonidos lo carga sola cuando algun script pide un efecto).
    static void CrearPrefabSonidos()
    {
        const string carpeta = "Assets/Audio/Efectos de Sonido";
        List<AudioClip> clips = new List<AudioClip>();
        if (AssetDatabase.IsValidFolder(carpeta))
        {
            List<string> rutas = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { carpeta }))
            {
                rutas.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            rutas.Sort(string.CompareOrdinal);
            foreach (string ruta in rutas)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
                if (clip != null) clips.Add(clip);
            }
        }

        if (clips.Count == 0)
        {
            avisos.Add("No encontré efectos de sonido en '" + carpeta + "'. El juego funcionará sin efectos.");
            return;
        }

        GameObject go = new GameObject("Sonidos");
        Sonidos sonidos = go.AddComponent<Sonidos>();
        sonidos.clips = clips.ToArray();

        Directory.CreateDirectory("Assets/Resources");
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Resources/Sonidos.prefab");
        Object.DestroyImmediate(go);
        AssetDatabase.SaveAssets();

        // Avisa si falta algun efecto que el juego usa
        string[] necesarios =
        {
            "Seleccion 1", "Seleccion 2", "Iniciar nivel", "Poner Pausa v2", "Reanudar",
            "Usando pala", "Regadera 1", "Regadera 2", "Podando planta", "Cortar hoja", "Cuidado exitoso", "Cuidado Fallido",
            "Matamoscas", "Se acercan los Gnomos", "Gnomo Curioso 1", "Gnomo Grande Curioso 1", "Gnomo Grande Sorprendido 1",
            "Gnomo Grito 1", "Victoria", "Derrota", "Humillación"
        };
        HashSet<string> existentes = new HashSet<string>();
        foreach (AudioClip clip in clips) existentes.Add(Sonidos.Normalizar(clip.name));
        foreach (string nombre in necesarios)
        {
            if (!existentes.Contains(Sonidos.Normalizar(nombre))) avisos.Add("Falta el efecto de sonido '" + nombre + "'.");
        }
    }

    // Busca un audio por el nombre del archivo (sin extension) dentro de Assets/Audio
    static AudioClip BuscarClip(string nombre)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Audio")) return null;
        foreach (string guid in AssetDatabase.FindAssets(nombre + " t:AudioClip", new[] { "Assets/Audio" }))
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(ruta) == nombre) return AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
        }
        return null;
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

        // Animacion de la herramienta usada (pala / regadera / tijera) sobre la planta
        Image efecto = CrearImagen("EfectoHerramienta", pantalla, "Dia/Anim/PalaAnima2/PalaAnima2_00", 140f, 328f, ArribaIzq);
        AnimadorSprites animEfecto = efecto.gameObject.AddComponent<AnimadorSprites>();

        // Globo de pensamiento: la colita (punta inferior izquierda) queda junto a la planta
        Image globo = CrearImagen("GloboPensamiento", pantalla, "Dia/GloboPensamiento1", 178f, 372f,
                                  new Vector2(40f / 128f, 1f - 102f / 128f));
        Image icono = CrearImagen("IconoNecesidad", globo.transform, "Dia/BolsaDeTierra", 79f, 51f, Centro);

        // Barrita de paciencia arriba del globo (se vacia mientras la planta espera)
        RectTransform barra = CrearPanel("BarraPaciencia", globo.transform, 79f, 14f, 44f, 6f, new Color(0f, 0f, 0f, 0.65f), false);
        RectTransform relleno = CrearRect("Relleno", barra, new Vector2(0f, 0.5f), 1f, 3f, 42f, 4f);
        Image imagenRelleno = relleno.gameObject.AddComponent<Image>();
        imagenRelleno.color = new Color(0.35f, 0.85f, 0.3f, 1f);
        imagenRelleno.raycastTarget = false;
        barra.gameObject.SetActive(false);

        // Textos
        Text textoDia = CrearTexto("TextoDia", pantalla, "Día 1", 20, 160f, 24f, 200f, 30f);
        Text textoProgreso = CrearTexto("TextoProgreso", pantalla, "Cuidados: 0/7", 11, 160f, 46f, 200f, 16f);
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
        Button irANoche = CrearBoton("BotonMenu", panelFin, "Ir a la noche", 160f, 476f); // (se llama BotonMenu porque will lo enruto a la noche)
        panelFin.gameObject.SetActive(false);

        // Menu de pausa (arriba del todo para que tape lo demas)
        MenuPausa pausa = CrearPausa(pantalla, "Dia1");

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
        dia.textoProgreso = textoProgreso;
        dia.barraPaciencia = barra;
        dia.rellenoPaciencia = imagenRelleno;
        dia.panelFin = panelFin.gameObject;
        dia.textoFin = textoFin;
        dia.imagenPlantaFin = plantaFin;
        dia.botonReintentar = reintentar;
        dia.botonMenu = irANoche;
        dia.menuPausa = pausa;
        dia.musica = BuscarClip("Dia_Soundtrack_2");
        if (dia.musica == null) avisos.Add("No encontré la música del día (Dia_Soundtrack_2). El día no tendrá música.");

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

    // ------------------------------------------------------------------ escena: EscenaNoche

    static void CrearNoche()
    {
        RectTransform pantalla = NuevaEscena();

        CrearImagen("Fondo", pantalla, "Noche/FondoNoche", 0f, 0f, ArribaIzq);

        // La planta en el mismo lugar que de dia (la maceta sobre la mancha clara del pasto)
        Image planta = CrearImagen("Planta", pantalla, "Dia/Anim/Planta-Estado2Anima/Planta-Estado2Anima_00", 160f, 416f, AbajoCentro);
        AnimadorSprites animPlanta = planta.gameObject.AddComponent<AnimadorSprites>();

        // Aqui aparecen todos los gnomos (lo crea EscenaNoche mientras juegas)
        RectTransform gnomos = CrearRect("Gnomos", pantalla, ArribaIzq, 0f, 0f, AnchoPantalla, AltoPantalla);

        // Barra superior: puntos, gnomos eliminados y vidas de la planta
        CrearPanel("PanelHud", pantalla, 160f, 24f, AnchoPantalla, 48f, new Color(0f, 0f, 0f, 0.55f), false);
        Text textoPuntaje = CrearTexto("TextoPuntaje", pantalla, "Puntos: 0", 14, 72f, 16f, 130f, 20f);
        Text textoGnomos = CrearTexto("TextoGnomos", pantalla, "Gnomos: 0/" + GameData.GnomosPorNoche, 11, 72f, 36f, 130f, 16f);
        float[] xCorazones = { 186f, 212f, 238f };
        Image[] corazones = new Image[xCorazones.Length];
        for (int i = 0; i < corazones.Length; i++)
        {
            corazones[i] = CrearImagen("Corazon" + (i + 1), pantalla, "Noche/Corazon", xCorazones[i], 25f, Centro);
            corazones[i].rectTransform.sizeDelta *= 2f; // el dibujo mide 11x10: se muestra al doble
        }

        // Cartel del centro ("¡Se acercan los gnomos!", resultado de la noche...)
        RectTransform banner = CrearPanel("PanelBanner", pantalla, 160f, 250f, AnchoPantalla, 84f, new Color(0f, 0f, 0f, 0.55f), false);
        Text textoBanner = CrearTexto("TextoBanner", banner, "", 20, 160f, 42f, 296f, 76f);
        banner.gameObject.SetActive(false);

        // Capa invisible que recibe los clics del matamoscas (queda debajo de los botones)
        RectTransform capa = CrearPanel("CapaClics", pantalla, 160f, 286f, AnchoPantalla, AltoPantalla, new Color(0f, 0f, 0f, 0f), true);

        // El matamoscas: su punto de anclaje esta en el centro de la parte de red (la que golpea)
        Image cursor = CrearImagen("Matamoscas", pantalla, "Noche/Anim/Matamoscas/Matamoscas_00", 160f, 300f,
                                   new Vector2(0.5f, 1f - 47f / 128f));
        AnimadorSprites animCursor = cursor.gameObject.AddComponent<AnimadorSprites>();

        // Menu de pausa
        MenuPausa pausa = CrearPausa(pantalla, "EscenaNoche");

        // Controlador de la noche
        GameObject controlador = new GameObject("EscenaNoche");
        EscenaNoche noche = controlador.AddComponent<EscenaNoche>();
        noche.pantalla = pantalla;
        noche.contenedorGnomos = gnomos;
        noche.imagenPlanta = planta;
        noche.animadorPlanta = animPlanta;
        noche.menuPausa = pausa;
        noche.textoPuntaje = textoPuntaje;
        noche.textoGnomos = textoGnomos;
        noche.corazones = corazones;
        noche.panelBanner = banner.gameObject;
        noche.textoBanner = textoBanner;
        noche.musica = BuscarClip("Noche_Soundtrack_Bajo");
        if (noche.musica == null) noche.musica = BuscarClip("Noche_Soundtrack");
        if (noche.musica == null) avisos.Add("No encontré la música de la noche (Noche_Soundtrack_Bajo). Sonará solo la que tenga AdmAudio.");

        Matamoscas matamoscas = capa.gameObject.AddComponent<Matamoscas>();
        matamoscas.pantalla = pantalla;
        matamoscas.imagenCursor = cursor.rectTransform;
        matamoscas.animador = animCursor;
        matamoscas.escena = noche;

        GuardarEscena("EscenaNoche");
    }

    // ------------------------------------------------------------------ escena: PantallaResultados

    static void CrearResultados()
    {
        RectTransform pantalla = NuevaEscena();

        CrearImagen("Fondo", pantalla, "Noche/FondoNoche", 0f, 0f, ArribaIzq);
        CrearPanel("Velo", pantalla, 160f, 286f, AnchoPantalla, AltoPantalla, new Color(0f, 0f, 0f, 0.5f), false);

        Text titulo = CrearTexto("TextoTitulo", pantalla, "¡Fin del día!", 22, 160f, 50f, 300f, 44f);

        // La planta elegida (el script la cambia segun la que se eligio; solo se ve si se salvo)
        Image planta = CrearImagen("Planta", pantalla, "Menu/Planta1", 160f, 205f, AbajoCentro);

        // Datos, cada uno en su propia linea
        CrearPanel("PanelDatos", pantalla, 160f, 330f, 280f, 150f, new Color(0f, 0f, 0f, 0.55f), false);
        Text clicks = CrearTexto("TextoClicks", pantalla, "Clicks en el día: 0", 14, 160f, 282f, 264f, 22f);
        Text cuidados = CrearTexto("TextoCuidados", pantalla, "Cuidados atendidos: 0/0", 14, 160f, 308f, 264f, 22f);
        Text enemigos = CrearTexto("TextoEnemigos", pantalla, "Gnomos eliminados: 0/10", 14, 160f, 334f, 264f, 22f);
        Text total = CrearTexto("TextoPuntajeTotal", pantalla, "Puntaje final: 0 pts", 20, 160f, 378f, 264f, 34f);

        Button volver = CrearBoton("BotonVolverAlMenu", pantalla, "Volver al menú", 160f, 480f);

        GameObject controlador = new GameObject("PantallaResultados");
        PantallaResultados resultados = controlador.AddComponent<PantallaResultados>();
        resultados.textoTitulo = titulo;
        resultados.imagenPlanta = planta;
        resultados.textoClicks = clicks;
        resultados.textoCuidados = cuidados;
        resultados.textoEnemigos = enemigos;
        resultados.textoPuntajeTotal = total;
        resultados.botonVolverAlMenu = volver;

        GuardarEscena("PantallaResultados");
    }

    // ------------------------------------------------------------------ menu de pausa (Dia y Noche)

    static MenuPausa CrearPausa(RectTransform pantalla, string escenaReiniciar)
    {
        // Boton de pausa (arriba a la derecha); su dibujo "late" con una animacion de 6 cuadros
        Image imgBoton = CrearImagen("BotonPausa", pantalla, "Pausa/Anim/PausaBoton/PausaBoton_00", 296f, 25f, Centro, true);
        Button botonPausa = imgBoton.gameObject.AddComponent<Button>();
        botonPausa.targetGraphic = imgBoton;
        AnimadorSprites animBoton = imgBoton.gameObject.AddComponent<AnimadorSprites>();
        animBoton.carpetaAlIniciar = "Pausa/Anim/PausaBoton";
        animBoton.fps = 6f;
        animBoton.repetirAlIniciar = true;

        // Panel que tapa todo el juego mientras esta en pausa (empieza oculto)
        RectTransform panel = CrearPanel("PanelPausa", pantalla, 160f, 286f, AnchoPantalla, AltoPantalla, new Color(0f, 0f, 0f, 0.8f), true);

        Image titulo = CrearImagen("TituloPausa", panel, "Pausa/Anim/PausaTitulo/PausaTitulo_00", 160f, 150f, Centro);
        titulo.rectTransform.localScale = new Vector3(2.4f, 2.4f, 1f); // el letrero mide 64x44: se muestra mas grande
        AnimadorSprites animTitulo = titulo.gameObject.AddComponent<AnimadorSprites>();
        animTitulo.carpetaAlIniciar = "Pausa/Anim/PausaTitulo";
        animTitulo.fps = 4f;
        animTitulo.repetirAlIniciar = true;
        animTitulo.usarTiempoReal = true; // sigue animado aunque el juego este detenido

        Button reanudar = CrearBoton("BotonReanudar", panel, "Reanudar", 160f, 250f);
        Button reiniciar = CrearBoton("BotonReiniciar", panel, "Reiniciar", 160f, 296f);
        Button menu = CrearBoton("BotonMenuPrincipal", panel, "Menú principal", 160f, 342f);
        CrearTexto("TextoEsc", panel, "Esc = continuar", 11, 160f, 392f, 200f, 16f);
        panel.gameObject.SetActive(false);

        GameObject controlador = new GameObject("MenuPausa");
        MenuPausa pausa = controlador.AddComponent<MenuPausa>();
        pausa.botonPausa = botonPausa;
        pausa.panel = panel.gameObject;
        pausa.botonReanudar = reanudar;
        pausa.botonReiniciar = reiniciar;
        pausa.botonMenu = menu;
        pausa.escenaReiniciar = escenaReiniciar;
        return pausa;
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
