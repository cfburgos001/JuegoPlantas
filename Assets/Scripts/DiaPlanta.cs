using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla el Dia 1 (cuidar la planta de dia).
//
// Flujo:
//   1. La maceta esta vacia y pide TIERRA (globo con la bolsa de tierra) -> se usa la PALA.
//   2. La semilla pide AGUA (globo con la gota)                          -> se usa la REGADERA.
//   3. Brota la plantita y se muestra el panel "Dia completado".
//
// Si el jugador usa una herramienta equivocada, el globo se sacude.
public class DiaPlanta : MonoBehaviour
{
    [Header("Referencias (las pone el constructor de escenas)")]
    public Image planta;
    public AnimadorSprites animadorPlanta;
    public RectTransform zonaUso;
    public RectTransform globo;
    public Image iconoGlobo;
    public Image efecto;
    public AnimadorSprites animadorEfecto;
    public Text textoDia;
    public Text textoAyuda;
    public GameObject panelFin;
    public Text textoFin;
    public Image imagenPlantaFin;
    public Button botonReintentar;
    public Button botonMenu;

    [Header("Donde aparece la animacion de cada herramienta (pixeles desde la esquina superior izquierda)")]
    public Vector2 posRegadera = new Vector2(106f, 318f);
    public Vector2 posPala = new Vector2(140f, 328f);

    [Header("Tiempos (segundos)")]
    public float pausaInicial = 1f;
    public float pausaEntreNecesidades = 1.2f;

    Sprite estadoInicial;
    Sprite estado1;
    Sprite estado2;
    Sprite[] estado2Animado;

    TipoHerramienta herramientaPedida;
    bool esperandoHerramienta;
    bool herramientaCorrectaUsada;

    Vector2 globoBase;
    float sacudidaX;
    bool sacudiendo;

    void Start()
    {
        // Sprites de la planta en el dia 1 (iguales para todas las plantas)
        estadoInicial = Cargador.CargarSprite("Dia/Planta-EstadoInicial");
        estado1 = Cargador.CargarSprite("Dia/Planta-Estado1");
        estado2 = Cargador.CargarSprite("Dia/Planta-Estado2");
        estado2Animado = Cargador.CargarAnimacion("Dia/Anim/Planta-Estado2Anima");

        globoBase = globo.anchoredPosition;
        globo.gameObject.SetActive(false);
        efecto.gameObject.SetActive(false);
        panelFin.SetActive(false);

        textoDia.text = "Día " + ConfigJuego.DiaActual;
        botonReintentar.onClick.AddListener(() => SceneManager.LoadScene("Dia1"));
        botonMenu.onClick.AddListener(() => SceneManager.LoadScene("Menu"));

        MostrarPlanta(estadoInicial);
        StartCoroutine(RutinaDia());
    }

    void Update()
    {
        // El globo "flota" suavemente y se sacude cuando se usa la herramienta equivocada
        if (globo.gameObject.activeSelf)
        {
            globo.anchoredPosition = globoBase + new Vector2(sacudidaX, Mathf.Sin(Time.time * 3f) * 2f);
        }
    }

    // ---------------------------------------------------------------- flujo del dia

    IEnumerator RutinaDia()
    {
        yield return new WaitForSeconds(pausaInicial);

        // Paso 1: tierra -> pala
        yield return AtenderNecesidad(
            TipoHerramienta.Pala,
            Cargador.CargarSprite("Dia/BolsaDeTierra"),
            Cargador.CargarAnimacion("Dia/Anim/PalaAnima2"), 8f, posPala,
            () => MostrarPlanta(estado1));

        yield return new WaitForSeconds(pausaEntreNecesidades);

        // Paso 2: agua -> regadera (la animacion se repite dos veces)
        yield return AtenderNecesidad(
            TipoHerramienta.Regadera,
            Cargador.CargarSprite("Dia/GotaAgua"),
            Repetir(Cargador.CargarAnimacion("Dia/Anim/AnimaRegadera"), 2), 6f, posRegadera,
            () =>
            {
                MostrarPlanta(estado2);
                animadorPlanta.Reproducir(estado2Animado, 3f, true);
            });

        yield return new WaitForSeconds(2f);
        MostrarFinDelDia();
    }

    // Muestra el globo, espera la herramienta correcta, reproduce su animacion y hace crecer la planta.
    IEnumerator AtenderNecesidad(TipoHerramienta herramienta, Sprite icono, Sprite[] cuadrosEfecto,
                                 float fps, Vector2 posEfecto, Action alTerminar)
    {
        herramientaPedida = herramienta;
        herramientaCorrectaUsada = false;
        iconoGlobo.sprite = icono;

        globo.localScale = Vector3.zero;
        globo.gameObject.SetActive(true);
        yield return Escalar(globo, 0f, 1f, 0.25f);
        esperandoHerramienta = true;

        while (!herramientaCorrectaUsada) yield return null;
        esperandoHerramienta = false;
        if (textoAyuda != null) textoAyuda.gameObject.SetActive(false);

        yield return Escalar(globo, 1f, 0f, 0.15f);
        globo.gameObject.SetActive(false);

        // Animacion de la herramienta sobre la planta
        efecto.rectTransform.anchoredPosition = new Vector2(posEfecto.x, -posEfecto.y);
        efecto.gameObject.SetActive(true);
        bool termino = false;
        animadorEfecto.Reproducir(cuadrosEfecto, fps, false, () => termino = true);
        while (!termino) yield return null;
        efecto.gameObject.SetActive(false);

        if (alTerminar != null) alTerminar();
        yield return Rebote(planta.rectTransform);
    }

    void MostrarFinDelDia()
    {
        int n = ConfigJuego.PlantaElegida;
        Sprite vista = Cargador.CargarSprite("Menu/Planta" + n);
        imagenPlantaFin.sprite = vista;
        imagenPlantaFin.rectTransform.sizeDelta = vista.rect.size * 1.5f;
        textoFin.text = "Tu " + ConfigJuego.NombrePlanta(n).ToLower() + " ya brotó.\n¡Mañana seguirá creciendo!";
        panelFin.SetActive(true);
    }

    // ---------------------------------------------------------------- lo llama HerramientaArrastrable

    public void UsarHerramienta(TipoHerramienta herramienta)
    {
        if (!esperandoHerramienta) return;

        if (herramienta == herramientaPedida)
        {
            herramientaCorrectaUsada = true;
        }
        else if (!sacudiendo)
        {
            StartCoroutine(Sacudir());
        }
    }

    // ---------------------------------------------------------------- utilidades

    void MostrarPlanta(Sprite sprite)
    {
        animadorPlanta.Detener();
        planta.sprite = sprite;
        planta.rectTransform.sizeDelta = sprite.rect.size;
    }

    static Sprite[] Repetir(Sprite[] cuadros, int veces)
    {
        Sprite[] resultado = new Sprite[cuadros.Length * veces];
        for (int i = 0; i < resultado.Length; i++) resultado[i] = cuadros[i % cuadros.Length];
        return resultado;
    }

    IEnumerator Escalar(RectTransform rt, float desde, float hasta, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(desde, hasta, Mathf.Clamp01(t / duracion));
            rt.localScale = new Vector3(k, k, 1f);
            yield return null;
        }
        rt.localScale = new Vector3(hasta, hasta, 1f);
    }

    IEnumerator Rebote(RectTransform rt)
    {
        const float duracion = 0.3f;
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

    IEnumerator Sacudir()
    {
        sacudiendo = true;
        const float duracion = 0.35f;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            sacudidaX = Mathf.Sin(t * 50f) * 5f * (1f - t / duracion);
            yield return null;
        }
        sacudidaX = 0f;
        sacudiendo = false;
    }
}
