using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla la NOCHE: salen 10 gnomos (lentos, normales y rapidos, en orden al azar) y el jugador
// los elimina con el matamoscas antes de que lleguen a la planta.
//
// Reglas:
//   - La planta tiene 3 vidas. Cada gnomo que la alcanza le quita una (y el gnomo se va).
//   - Cada gnomo eliminado da puntos segun su velocidad (ver EnemigoNoche).
//   - La noche termina cuando ya no quedan gnomos (se salva la planta) o cuando se acaban las vidas.
//   - Al terminar se pasa a PantallaResultados, que muestra el puntaje total.
//
// (Estructura base creada por will: carga de la planta elegida, GameData y paso a PantallaResultados.)
public class EscenaNoche : MonoBehaviour
{
    [Header("Referencias (las pone el constructor de escenas)")]
    public RectTransform pantalla;
    public RectTransform contenedorGnomos;
    public Image imagenPlanta;
    public AnimadorSprites animadorPlanta;
    public MenuPausa menuPausa;
    public Text textoPuntaje;
    public Text textoGnomos;
    public Image[] corazones;
    public GameObject panelBanner;
    public Text textoBanner;
    public AudioClip musica;

    [Header("Ritmo de la noche")]
    public float pausaInicial = 3.5f;
    [Tooltip("Segundos de espera entre un gnomo y el siguiente (minimo y maximo)")]
    public float intervaloMinimo = 1.8f;
    public float intervaloMaximo = 2.8f;

    [Header("La planta (pixeles desde la esquina superior izquierda)")]
    public Vector2 basePlanta = new Vector2(160f, 416f);   // el punto de abajo al centro de la maceta
    public Vector2 centroPlanta = new Vector2(160f, 388f); // hacia aqui caminan los gnomos
    public float distanciaLlegada = 24f;

    [Header("Golpes")]
    [Tooltip("Margen extra alrededor del gnomo donde todavia cuenta el golpe (perdona los clics 'casi')")]
    public float margenGolpe = 12f;

    readonly List<EnemigoNoche> activos = new List<EnemigoNoche>();
    Sprite corazonLleno;
    Sprite corazonVacio;

    public bool Jugando { get; private set; }
    public bool Terminada { get; private set; }

    // ------------------------------------------------------------------ inicio

    void Start()
    {
        GameData.ReiniciarNoche();
        Jugando = false;
        Terminada = false;

        corazonLleno = Cargador.CargarSprite("Noche/Corazon");
        corazonVacio = Cargador.CargarSprite("Noche/CorazonVacio");

        MostrarPlanta();
        ActualizarHud();
        if (panelBanner != null) panelBanner.SetActive(false);
        AdmAudio.Musica(musica);

        StartCoroutine(RutinaNoche());
    }

    // Pone la planta que eligio el jugador, ya crecida segun el dia
    void MostrarPlanta()
    {
        int n = ConfigJuego.PlantaElegida;
        Sprite[] cuadros = Cargador.CargarCrecimiento(n, ConfigJuego.DiaActual);
        float x = basePlanta.x;
        if (cuadros != null)
        {
            x += Cargador.DesfaseXCrecimiento;
        }
        else
        {
            // Esta planta aun no tiene dibujo de crecimiento (plantas 4 y 5): se usa el brote del dia 1
            cuadros = Cargador.CargarAnimacion("Dia/Anim/Planta-Estado2Anima");
        }

        imagenPlanta.rectTransform.anchoredPosition = new Vector2(x, -basePlanta.y);
        animadorPlanta.Reproducir(cuadros, 3f, true);
    }

    // ------------------------------------------------------------------ flujo de la noche

    IEnumerator RutinaNoche()
    {
        MostrarBanner("¡Se acercan los gnomos!");
        Sonidos.Reproducir("Se acercan los Gnomos", 1f, 4.5f);
        yield return new WaitForSeconds(pausaInicial);
        OcultarBanner();

        Jugando = true;
        List<TipoGnomo> plan = CrearPlan(GameData.GnomosPorNoche);
        for (int i = 0; i < plan.Count && !Terminada; i++)
        {
            CrearGnomo(plan[i]);
            yield return new WaitForSeconds(Random.Range(intervaloMinimo, intervaloMaximo));
        }

        // Ya salieron todos: se espera a que el ultimo sea eliminado o llegue a la planta
        while (!Terminada && activos.Count > 0) yield return null;

        if (!Terminada) yield return Terminar(true);
    }

    // Lista de tipos de gnomo en orden al azar: 30% lentos, 40% normales, 30% rapidos
    static List<TipoGnomo> CrearPlan(int total)
    {
        int lentos = Mathf.RoundToInt(total * 0.3f);
        int rapidos = Mathf.RoundToInt(total * 0.3f);
        int normales = total - lentos - rapidos;

        List<TipoGnomo> plan = new List<TipoGnomo>();
        for (int i = 0; i < lentos; i++) plan.Add(TipoGnomo.Lento);
        for (int i = 0; i < normales; i++) plan.Add(TipoGnomo.Normal);
        for (int i = 0; i < rapidos; i++) plan.Add(TipoGnomo.Rapido);

        // Mezclar (Fisher-Yates)
        for (int i = plan.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            TipoGnomo guardado = plan[i];
            plan[i] = plan[j];
            plan[j] = guardado;
        }

        // El primer gnomo nunca es rapido, para que el jugador entre en calor
        if (plan.Count > 1 && plan[0] == TipoGnomo.Rapido)
        {
            for (int i = 1; i < plan.Count; i++)
            {
                if (plan[i] != TipoGnomo.Rapido)
                {
                    plan[0] = plan[i];
                    plan[i] = TipoGnomo.Rapido;
                    break;
                }
            }
        }
        return plan;
    }

    void CrearGnomo(TipoGnomo tipo)
    {
        GameObject go = new GameObject("Gnomo " + tipo,
            typeof(RectTransform), typeof(Image), typeof(AnimadorSprites), typeof(EnemigoNoche));
        go.transform.SetParent(contenedorGnomos, false);
        go.GetComponent<EnemigoNoche>().Iniciar(this, tipo, PuntoDeEntrada(), centroPlanta, distanciaLlegada);

        if (tipo == TipoGnomo.Lento) Sonidos.ReproducirAleatorio("Gnomo Grande Curioso 1", "Gnomo Grande Curioso 2");
        else Sonidos.ReproducirAleatorio("Gnomo Curioso 1", "Gnomo Curioso 2", "Gnomo Curioso 3");
    }

    // Por donde entra el gnomo: por el hueco de la cerca (40%) o por la izquierda / derecha del jardin
    static Vector2 PuntoDeEntrada()
    {
        float r = Random.value;
        if (r < 0.4f) return new Vector2(Random.Range(138f, 182f), 282f);

        float y = Random.Range(352f, 470f);
        return r < 0.7f ? new Vector2(-40f, y) : new Vector2(360f, y);
    }

    // ------------------------------------------------------------------ lo llaman los gnomos y el matamoscas

    public void Registrar(EnemigoNoche gnomo)
    {
        if (!activos.Contains(gnomo)) activos.Add(gnomo);
    }

    public void Quitar(EnemigoNoche gnomo)
    {
        activos.Remove(gnomo);
    }

    // Convierte la posicion del mouse (pixeles de la ventana) a pixeles del juego,
    // medidos desde la esquina superior izquierda de la pantalla de 320x572.
    public Vector2 PantallaAPuntoDeJuego(Vector2 posicionEnPantalla)
    {
        Vector2 local;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(pantalla, posicionEnPantalla, null, out local);
        return new Vector2(local.x + pantalla.rect.width * 0.5f, pantalla.rect.height * 0.5f - local.y);
    }

    // El matamoscas cayo en "punto". Golpea al gnomo mas cercano que este debajo.
    public void Golpear(Vector2 punto)
    {
        EnemigoNoche mejor = null;
        float mejorDistancia = float.MaxValue;
        foreach (EnemigoNoche gnomo in activos)
        {
            if (!gnomo.EstaVivo) continue;
            float distancia = (gnomo.Posicion - punto).magnitude;
            if (distancia <= gnomo.RadioGolpe + margenGolpe && distancia < mejorDistancia)
            {
                mejor = gnomo;
                mejorDistancia = distancia;
            }
        }
        if (mejor != null) mejor.RecibirDano(1);
    }

    public void GnomoEliminado(EnemigoNoche gnomo)
    {
        MostrarPuntos(gnomo.Posicion, gnomo.puntosAlMorir);
        ActualizarHud();
    }

    public void GnomoLlegoALaPlanta(EnemigoNoche gnomo)
    {
        if (Terminada) return;

        GameData.VidasPlanta = Mathf.Max(0, GameData.VidasPlanta - 1);
        Sonidos.Reproducir("Cuidado Fallido", 0.9f, 1.3f);
        StartCoroutine(SacudirPlanta());
        ActualizarHud();

        if (GameData.VidasPlanta <= 0) StartCoroutine(Terminar(false));
    }

    // ------------------------------------------------------------------ final de la noche

    IEnumerator Terminar(bool gano)
    {
        if (Terminada) yield break;
        Terminada = true;
        Jugando = false;

        GameData.NocheSuperada = gano;
        GameData.PuntajeTotal = GameData.PuntajeNoche; // el puntaje total del dia completo es el de la noche
        if (menuPausa != null) menuPausa.Bloquear();

        string titulo;
        if (gano)
        {
            titulo = "¡Tu planta está a salvo!";
            Sonidos.Reproducir("Victoria", 1f, 5.5f);
            StartCoroutine(Rebote(imagenPlanta.rectTransform));
        }
        else
        {
            // Los gnomos que quedaban se van corriendo con la planta
            foreach (EnemigoNoche gnomo in new List<EnemigoNoche>(activos))
            {
                if (gnomo != null) Destroy(gnomo.gameObject);
            }
            StartCoroutine(RobarPlanta());

            if (GameData.EnemigosEliminados < 3)
            {
                titulo = "¡Los gnomos se la llevaron!";
                Sonidos.Reproducir("Humillación", 1f, 5.5f);
            }
            else
            {
                titulo = "¡Los gnomos ganaron esta noche!";
                Sonidos.Reproducir("Derrota", 1f, 5.5f);
            }
        }

        MostrarBanner(titulo + "\nGnomos: " + GameData.EnemigosEliminados + "/" + GameData.GnomosPorNoche +
                      "   Puntos: " + GameData.PuntajeNoche);
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("PantallaResultados");
    }

    // ------------------------------------------------------------------ pantalla (HUD y efectos)

    void ActualizarHud()
    {
        if (textoPuntaje != null) textoPuntaje.text = "Puntos: " + GameData.PuntajeNoche;
        if (textoGnomos != null) textoGnomos.text = "Gnomos: " + GameData.EnemigosEliminados + "/" + GameData.GnomosPorNoche;
        if (corazones != null)
        {
            for (int i = 0; i < corazones.Length; i++)
            {
                if (corazones[i] != null) corazones[i].sprite = i < GameData.VidasPlanta ? corazonLleno : corazonVacio;
            }
        }
    }

    void MostrarBanner(string texto)
    {
        if (panelBanner == null) return;
        textoBanner.text = texto;
        panelBanner.SetActive(true);
    }

    void OcultarBanner()
    {
        if (panelBanner != null) panelBanner.SetActive(false);
    }

    // "+150" que sube y se desvanece donde estaba el gnomo
    void MostrarPuntos(Vector2 donde, int puntos)
    {
        GameObject go = new GameObject("+" + puntos, typeof(RectTransform), typeof(Text), typeof(Outline));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(contenedorGnomos, false);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(80f, 24f);
        rt.anchoredPosition = new Vector2(donde.x, -donde.y);

        Text texto = go.GetComponent<Text>();
        texto.font = textoPuntaje.font;
        texto.text = "+" + puntos;
        texto.fontSize = 16;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = new Color(1f, 0.9f, 0.3f, 1f);
        texto.raycastTarget = false;

        Outline contorno = go.GetComponent<Outline>();
        contorno.effectColor = new Color(0.23f, 0.12f, 0.05f, 1f);
        contorno.effectDistance = new Vector2(1f, -1f);

        StartCoroutine(Flotar(rt, texto));
    }

    IEnumerator Flotar(RectTransform rt, Text texto)
    {
        const float duracion = 0.9f;
        Vector2 inicio = rt.anchoredPosition;
        float t = 0f;
        while (t < duracion && rt != null)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracion);
            rt.anchoredPosition = inicio + new Vector2(0f, 30f * k);
            Color c = texto.color;
            c.a = 1f - k * k;
            texto.color = c;
            yield return null;
        }
        if (rt != null) Destroy(rt.gameObject);
    }

    IEnumerator SacudirPlanta()
    {
        const float duracion = 0.4f;
        RectTransform rt = imagenPlanta.rectTransform;
        float xBase = rt.anchoredPosition.x;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = 1f - t / duracion;
            rt.anchoredPosition = new Vector2(xBase + Mathf.Sin(t * 60f) * 4f * k, rt.anchoredPosition.y);
            imagenPlanta.color = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.4f, 1f), k);
            yield return null;
        }
        rt.anchoredPosition = new Vector2(xBase, rt.anchoredPosition.y);
        imagenPlanta.color = Color.white;
    }

    // La planta se encoge hasta desaparecer: se la robaron
    IEnumerator RobarPlanta()
    {
        yield return new WaitForSeconds(0.5f);
        RectTransform rt = imagenPlanta.rectTransform; // su punto de anclaje esta abajo al centro: se encoge hacia la maceta
        const float duracion = 0.6f;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float s = 1f - Mathf.Clamp01(t / duracion);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        imagenPlanta.enabled = false;
    }

    IEnumerator Rebote(RectTransform rt)
    {
        const float duracion = 0.4f;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float s = 1f + 0.2f * Mathf.Sin(Mathf.Clamp01(t / duracion) * Mathf.PI);
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }
}
