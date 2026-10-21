using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Controla el Dia 1 (cuidar la planta de dia).
//
// Flujo:
//   1. La maceta esta vacia y pide TIERRA (globo con la bolsa de tierra) -> se usa la PALA.
//   2. La semilla pide AGUA (globo con la gota)                          -> se usa la REGADERA.
//   3. Brota la plantita y empieza a pedir cuidados extra al azar: MAS TIERRA (pala), MAS AGUA (regadera)
//      o PODA de hojas (tijera). Cada peticion tiene un tiempo limite (la barrita del globo).
//   4. Al terminar, la planta crece y se muestra el panel "Dia completado". Luego viene la noche.
//
// Si el jugador usa una herramienta equivocada, el globo se sacude. Si se acaba el tiempo, se pierde ese cuidado.
public class DiaPlanta : MonoBehaviour
{
    enum Peticion { Tierra, Agua, Poda }

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
    public Text textoProgreso;
    public RectTransform barraPaciencia;
    public Image rellenoPaciencia;
    public GameObject panelFin;
    public Text textoFin;
    public Image imagenPlantaFin;
    public Button botonReintentar;
    public Button botonMenu; // (will lo usa para pasar a la noche)
    public MenuPausa menuPausa;
    public AudioClip musica;

    [Header("Donde aparece la animacion de cada herramienta (pixeles desde la esquina superior izquierda)")]
    public Vector2 posRegadera = new Vector2(106f, 318f);
    public Vector2 posPala = new Vector2(140f, 328f);
    [Tooltip("Centro de las tijeras cuando podan")]
    public Vector2 posTijera = new Vector2(152f, 378f);

    [Header("Ritmo del dia")]
    [Tooltip("Cuantas peticiones al azar hace la planta despues de brotar")]
    public int peticionesExtra = 5;
    [Tooltip("Segundos que tiene el jugador para atender cada peticion extra")]
    public float tiempoPorPeticion = 14f;
    public float pausaInicial = 1f;
    public float pausaEntreNecesidades = 1.2f;

    static readonly string[] Elogios = { "¡Muy bien!", "¡Qué rico!", "¡Eso es!", "¡Gracias!", "¡Perfecto!" };

    Sprite estadoInicial;
    Sprite estado1;
    Sprite estado2;
    Sprite[] estado2Animado;

    TipoHerramienta herramientaPedida;
    bool esperandoHerramienta;
    bool herramientaCorrectaUsada;

    Vector2 globoBase;
    Vector2 plantaBase;
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
        plantaBase = planta.rectTransform.anchoredPosition;
        globo.gameObject.SetActive(false);
        efecto.gameObject.SetActive(false);
        panelFin.SetActive(false);
        if (barraPaciencia != null) barraPaciencia.gameObject.SetActive(false);

        textoDia.text = "Día " + ConfigJuego.DiaActual;
        botonReintentar.onClick.AddListener(() => {         //Meti un reinicio de puntos al resetear, casi jodo el codigo :v--will
            GameData.ResetearDatos();
            SceneManager.LoadScene("Dia1");
        });
        botonMenu.onClick.AddListener(() => SceneManager.LoadScene("EscenaNoche")); //Voy a enrutar esta boton para que pase al game de noche--will

        GameData.CuidadosAtendidos = 0;
        GameData.CuidadosTotales = 2 + Mathf.Max(0, peticionesExtra);
        ActualizarProgreso();

        AdmAudio.Musica(musica);
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

        // Paso 1: tierra -> pala (la planta no tiene limite de tiempo mientras aprende)
        yield return Atender(Peticion.Tierra, false, () =>
        {
            MostrarPlanta(estado1);
            GameData.TierraCambiada = true;
        });

        yield return new WaitForSeconds(pausaEntreNecesidades);

        // Paso 2: agua -> regadera
        yield return Atender(Peticion.Agua, false, () =>
        {
            MostrarPlanta(estado2);
            animadorPlanta.Reproducir(estado2Animado, 3f, true);
            GameData.PlantaRegada = true;
        });

        // Paso 3: la plantita ya brotó y empieza a pedir cuidados al azar
        List<Peticion> extras = CrearListaDePeticiones(peticionesExtra);
        foreach (Peticion peticion in extras)
        {
            yield return new WaitForSeconds(pausaEntreNecesidades);
            yield return Atender(peticion, true, null);
        }

        // Paso 4: la planta crece
        yield return new WaitForSeconds(1f);
        MostrarCrecimiento();
        yield return Rebote(planta.rectTransform);
        yield return new WaitForSeconds(2f);

        MostrarFinDelDia();
    }

    // Muestra el globo, espera la herramienta correcta (o que se acabe el tiempo), reproduce el efecto
    // de la herramienta y hace rebotar la planta.
    IEnumerator Atender(Peticion peticion, bool conTiempo, Action alTerminar)
    {
        herramientaPedida = HerramientaDe(peticion);
        herramientaCorrectaUsada = false;
        iconoGlobo.sprite = IconoDe(peticion);
        Mensaje(MensajeDe(peticion));

        globo.localScale = Vector3.zero;
        globo.gameObject.SetActive(true);
        PonerBarra(1f, conTiempo);
        yield return Escalar(globo, 0f, 1f, 0.25f);
        esperandoHerramienta = true;

        float restante = tiempoPorPeticion;
        bool seAgotoElTiempo = false;
        while (!herramientaCorrectaUsada)
        {
            if (conTiempo)
            {
                restante -= Time.deltaTime;
                PonerBarra(restante / tiempoPorPeticion, true);
                if (restante <= 0f)
                {
                    seAgotoElTiempo = true;
                    break;
                }
            }
            yield return null;
        }
        esperandoHerramienta = false;
        PonerBarra(0f, false);

        if (seAgotoElTiempo)
        {
            // Se perdio este cuidado: el globo se sacude y desaparece
            Sonidos.Reproducir("Cuidado Fallido", 0.9f, 1.4f);
            Mensaje("¡Llegaste tarde!");
            if (!sacudiendo) yield return Sacudir();
            yield return Escalar(globo, 1f, 0f, 0.15f);
            globo.gameObject.SetActive(false);
            yield break;
        }

        yield return Escalar(globo, 1f, 0f, 0.15f);
        globo.gameObject.SetActive(false);

        yield return Efecto(peticion);

        GameData.CuidadosAtendidos++;
        ActualizarProgreso();
        if (alTerminar != null) alTerminar();

        Sonidos.Reproducir("Cuidado exitoso", 0.9f, 2.2f);
        Mensaje(Elogios[UnityEngine.Random.Range(0, Elogios.Length)]);
        yield return Rebote(planta.rectTransform);
    }

    // ---------------------------------------------------------------- efectos de cada herramienta

    IEnumerator Efecto(Peticion peticion)
    {
        switch (peticion)
        {
            case Peticion.Tierra:
                Sonidos.Reproducir("Usando pala", 1f, 2.2f);
                yield return AnimarEfecto(Repetir(Cargador.CargarAnimacion("Dia/Anim/PalaAnima2"), 3), 8f, posPala);
                break;

            case Peticion.Agua:
                Sonidos.Reproducir(UnityEngine.Random.value < 0.5f ? "Regadera 1" : "Regadera 2", 1f, 2.2f);
                yield return AnimarEfecto(Repetir(Cargador.CargarAnimacion("Dia/Anim/AnimaRegadera"), 3), 6f, posRegadera);
                break;

            default:
                yield return EfectoPoda();
                break;
        }
    }

    IEnumerator AnimarEfecto(Sprite[] cuadros, float fps, Vector2 pos)
    {
        RectTransform rt = efecto.rectTransform;
        rt.pivot = new Vector2(0f, 1f); // las posiciones se miden desde la esquina de arriba a la izquierda
        rt.localRotation = Quaternion.identity;
        rt.anchoredPosition = new Vector2(pos.x, -pos.y);
        efecto.gameObject.SetActive(true);

        bool termino = false;
        animadorEfecto.Reproducir(cuadros, fps, false, () => termino = true);
        while (!termino) yield return null;
        efecto.gameObject.SetActive(false);
    }

    // No hay animacion de tijeras dibujada, asi que se mueven por codigo: el sprite de la tijera
    // "corta" balanceandose junto a la planta.
    IEnumerator EfectoPoda()
    {
        Sprite tijera = Cargador.CargarSprite("Dia/Tijera");
        RectTransform rt = efecto.rectTransform;
        animadorEfecto.Detener();
        efecto.sprite = tijera;
        rt.sizeDelta = tijera.rect.size;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(posTijera.x, -posTijera.y);
        efecto.gameObject.SetActive(true);

        Sonidos.Reproducir("Podando planta", 1f, 1.6f);

        const float duracion = 1.4f;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float vaiven = Mathf.Sin(t * 26f);
            rt.localRotation = Quaternion.Euler(0f, 0f, vaiven * 14f);
            rt.anchoredPosition = new Vector2(posTijera.x - Mathf.Abs(vaiven) * 3f, -posTijera.y + Mathf.Sin(t * 13f) * 2f);
            yield return null;
        }

        Sonidos.Reproducir("Cortar hoja");
        rt.localRotation = Quaternion.identity;
        efecto.gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------- final del dia

    // La planta crece: se cambia el brote por el dibujo de la planta elegida ya mas grande
    void MostrarCrecimiento()
    {
        Sprite[] crecida = Cargador.CargarCrecimiento(ConfigJuego.PlantaElegida, ConfigJuego.DiaActual);
        if (crecida == null) return; // las plantas 4 y 5 aun no tienen ese dibujo: se queda el brote

        animadorPlanta.Detener();
        planta.rectTransform.anchoredPosition = plantaBase + new Vector2(Cargador.DesfaseXCrecimiento, 0f);
        animadorPlanta.Reproducir(crecida, 3f, true);
        Sonidos.Reproducir("Cuidado exitoso", 0.9f, 2.2f);
    }

    void MostrarFinDelDia()
    {
        if (menuPausa != null) menuPausa.Bloquear();
        if (globo.gameObject.activeSelf) globo.gameObject.SetActive(false);

        int n = ConfigJuego.PlantaElegida;
        Sprite vista = Cargador.CargarSprite("Menu/Planta" + n);
        imagenPlantaFin.sprite = vista;
        imagenPlantaFin.rectTransform.sizeDelta = vista.rect.size * 1.5f;
        textoFin.text = "Cuidados atendidos: " + GameData.CuidadosAtendidos + "/" + GameData.CuidadosTotales +
                        "\n¡Llega la noche! Protege tu planta.";
        panelFin.SetActive(true);
    }

    // ---------------------------------------------------------------- lo llama HerramientaArrastrable

    public void UsarHerramienta(TipoHerramienta herramienta)
    {
        GameData.ClicksTotales++; //Esto lo agregue para contar los clics--will

        if (!esperandoHerramienta) return;

        if (herramienta == herramientaPedida)
        {
            herramientaCorrectaUsada = true;
        }
        else
        {
            Sonidos.Reproducir("Cuidado Fallido", 0.8f, 0.9f);
            Mensaje("Esa no es la herramienta. ¡Mira el globo!");
            if (!sacudiendo) StartCoroutine(Sacudir());
        }
    }

    // ---------------------------------------------------------------- datos de cada peticion

    static TipoHerramienta HerramientaDe(Peticion p)
    {
        switch (p)
        {
            case Peticion.Tierra: return TipoHerramienta.Pala;
            case Peticion.Agua: return TipoHerramienta.Regadera;
            default: return TipoHerramienta.Tijera;
        }
    }

    static Sprite IconoDe(Peticion p)
    {
        switch (p)
        {
            case Peticion.Tierra: return Cargador.CargarSprite("Dia/BolsaDeTierra");
            case Peticion.Agua: return Cargador.CargarSprite("Dia/GotaAgua");
            default: return Cargador.CargarSprite("Dia/Tijera");
        }
    }

    string MensajeDe(Peticion p)
    {
        bool primera = GameData.CuidadosAtendidos < 2;
        switch (p)
        {
            case Peticion.Tierra:
                return primera ? "Necesito tierra. Arrastra la pala hasta la planta." : "¡Necesito más tierra! Usa la pala.";
            case Peticion.Agua:
                return primera ? "Ahora tengo sed. Arrastra la regadera hasta la planta." : "¡Tengo sed! Usa la regadera.";
            default:
                return "¡Tengo hojas secas! Usa la tijera.";
        }
    }

    // Lista al azar de peticiones: reparte los tres tipos parejo y nunca repite el mismo dos veces seguidas
    static List<Peticion> CrearListaDePeticiones(int cantidad)
    {
        List<Peticion> lista = new List<Peticion>();
        Peticion anterior = Peticion.Agua; // la ultima del tutorial
        while (lista.Count < cantidad)
        {
            List<Peticion> bolsa = new List<Peticion> { Peticion.Tierra, Peticion.Agua, Peticion.Poda };
            for (int i = bolsa.Count - 1; i > 0; i--) // mezclar
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                Peticion guardada = bolsa[i];
                bolsa[i] = bolsa[j];
                bolsa[j] = guardada;
            }
            if (bolsa[0] == anterior)
            {
                Peticion primera = bolsa[0];
                bolsa[0] = bolsa[1];
                bolsa[1] = primera;
            }
            foreach (Peticion p in bolsa)
            {
                if (lista.Count < cantidad)
                {
                    lista.Add(p);
                    anterior = p;
                }
            }
        }
        return lista;
    }

    // ---------------------------------------------------------------- utilidades

    void Mensaje(string texto)
    {
        if (textoAyuda == null) return;
        textoAyuda.text = texto;
        textoAyuda.gameObject.SetActive(true);
    }

    void ActualizarProgreso()
    {
        if (textoProgreso != null)
        {
            textoProgreso.text = "Cuidados: " + GameData.CuidadosAtendidos + "/" + GameData.CuidadosTotales;
        }
    }

    // Barrita de paciencia del globo: verde cuando hay mucho tiempo, roja cuando casi se acaba
    void PonerBarra(float fraccion, bool visible)
    {
        if (barraPaciencia == null || rellenoPaciencia == null) return;
        if (barraPaciencia.gameObject.activeSelf != visible) barraPaciencia.gameObject.SetActive(visible);
        if (!visible) return;

        fraccion = Mathf.Clamp01(fraccion);
        rellenoPaciencia.rectTransform.localScale = new Vector3(fraccion, 1f, 1f);
        rellenoPaciencia.color = Color.Lerp(new Color(0.9f, 0.25f, 0.2f, 1f), new Color(0.35f, 0.85f, 0.3f, 1f), fraccion);
    }

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
