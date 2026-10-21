using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Los tres tipos de gnomo. Cada uno tiene su propia velocidad y su propio puntaje:
// mientras mas rapido es el gnomo, mas puntos da.
public enum TipoGnomo
{
    Lento,   // grande, camina despacio y aguanta 2 golpes
    Normal,  // velocidad media, 1 golpe
    Rapido   // pequeno y veloz, 1 golpe
}

// Un gnomo que camina hacia la planta para robarla.
// Lo crea EscenaNoche; aqui vive todo lo que le pasa a UN gnomo: caminar, recibir golpes, morir o llegar.
// (Base creada por will: vidaMaxima, puntosAlMorir y RecibirDano; ampliada con movimiento, animaciones y tipos.)
[RequireComponent(typeof(Image))]
public class EnemigoNoche : MonoBehaviour
{
    // ------------------------------------------------------------------ ajustes por tipo (cambialos aqui)

    //                                       velocidad (px/s)   golpes   puntos   tamano
    const float VelocidadLento = 26f;   const int VidaLento = 2;   const int PuntosLento = 100;   const float EscalaLento = 1.35f;
    const float VelocidadNormal = 44f;  const int VidaNormal = 1;  const int PuntosNormal = 150;  const float EscalaNormal = 1f;
    const float VelocidadRapido = 72f;  const int VidaRapido = 1;  const int PuntosRapido = 250;  const float EscalaRapido = 0.85f;

    const float FpsCaminar = 9f;
    const float FpsGolpe = 14f;
    const float FpsMuerte = 16f;

    [Header("Datos del enemigo (se llenan solos segun el tipo)")]
    public TipoGnomo tipo;
    public int vidaMaxima = 1;     // Los golpes que aguanta el enemigo--will
    public int puntosAlMorir = 100; // Los puntos al derrotarlo--will
    public float velocidad = 40f;  // pixeles por segundo
    public float escala = 1f;

    enum Estado { Caminando, Golpeado, Muriendo, Llegando }

    // Cuadros de animacion: se cargan una sola vez y se comparten entre todos los gnomos
    static Sprite[] cuadrosCaminar;
    static Sprite[] cuadrosGolpe;
    static Sprite[] cuadrosMuerte;

    EscenaNoche escena;
    AnimadorSprites animador;
    RectTransform rt;
    Image imagen;

    Vector2 posicion;   // centro del gnomo, en pixeles desde la esquina superior izquierda
    Vector2 objetivo;
    float distanciaLlegada;
    int vidaActual;
    Estado estado = Estado.Caminando;

    public bool EstaVivo { get { return estado == Estado.Caminando || estado == Estado.Golpeado; } }
    public Vector2 Posicion { get { return posicion; } }

    // Cuanto mide la zona donde el matamoscas lo puede golpear
    public float RadioGolpe { get { return 26f * escala; } }

    // ------------------------------------------------------------------ inicio

    public void Iniciar(EscenaNoche laEscena, TipoGnomo elTipo, Vector2 inicio, Vector2 meta, float distancia)
    {
        escena = laEscena;
        tipo = elTipo;
        objetivo = meta;
        distanciaLlegada = distancia;
        posicion = inicio;

        switch (tipo)
        {
            case TipoGnomo.Lento:
                velocidad = VelocidadLento; vidaMaxima = VidaLento; puntosAlMorir = PuntosLento; escala = EscalaLento;
                break;
            case TipoGnomo.Rapido:
                velocidad = VelocidadRapido; vidaMaxima = VidaRapido; puntosAlMorir = PuntosRapido; escala = EscalaRapido;
                break;
            default:
                velocidad = VelocidadNormal; vidaMaxima = VidaNormal; puntosAlMorir = PuntosNormal; escala = EscalaNormal;
                break;
        }
        vidaActual = vidaMaxima; // Inicial con salud maxima el enemigo-will

        if (cuadrosCaminar == null) cuadrosCaminar = Cargador.CargarAnimacion("Noche/Anim/Gnomo_Caminar");
        if (cuadrosGolpe == null) cuadrosGolpe = Cargador.CargarAnimacion("Noche/Anim/Gnomo_Golpe");
        if (cuadrosMuerte == null) cuadrosMuerte = Cargador.CargarAnimacion("Noche/Anim/Gnomo_Muerte");

        imagen = GetComponent<Image>();
        imagen.raycastTarget = false;
        animador = GetComponent<AnimadorSprites>();

        rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = new Vector3(escala, escala, 1f);
        Colocar();

        escena.Registrar(this);
        animador.Reproducir(cuadrosCaminar, FpsCaminar * Mathf.Clamp(velocidad / VelocidadNormal, 0.7f, 1.6f), true);
    }

    void OnDestroy()
    {
        if (escena != null) escena.Quitar(this);
    }

    // ------------------------------------------------------------------ movimiento

    void Update()
    {
        if (estado != Estado.Caminando) return;

        Vector2 haciaLaPlanta = objetivo - posicion;
        float distancia = haciaLaPlanta.magnitude;
        if (distancia <= distanciaLlegada)
        {
            Llegar();
            return;
        }
        posicion += haciaLaPlanta / distancia * Mathf.Min(velocidad * Time.deltaTime, distancia);
        Colocar();
    }

    void Colocar()
    {
        rt.anchoredPosition = new Vector2(posicion.x, -posicion.y);
    }

    // ------------------------------------------------------------------ golpes

    // la forma en que recibira dano--will
    public void RecibirDano(int cantidadDano)
    {
        if (!EstaVivo) return;

        vidaActual -= cantidadDano;

        // se elimina cuando llega a 0--will
        if (vidaActual <= 0) Morir();
        else StartCoroutine(Aturdirse());
    }

    // Golpe que no mata (solo pasa con el gnomo grande): se queda quieto un momento y sigue
    IEnumerator Aturdirse()
    {
        estado = Estado.Golpeado;
        Sonidos.Reproducir("Gnomo Grande Sorprendido 1");

        bool termino = false;
        animador.Reproducir(cuadrosGolpe, FpsGolpe, false, () => termino = true);
        while (!termino) yield return null;

        estado = Estado.Caminando;
        animador.Reproducir(cuadrosCaminar, FpsCaminar * Mathf.Clamp(velocidad / VelocidadNormal, 0.7f, 1.6f), true);
    }

    void Morir()
    {
        StopAllCoroutines();
        estado = Estado.Muriendo;

        GameData.EnemigosEliminados++;
        GameData.SumarPuntosNoche(puntosAlMorir);
        Debug.Log("Enemigo eliminado (" + tipo + "). Total bajas: " + GameData.EnemigosEliminados + " | Puntos: " + GameData.PuntajeNoche);

        Sonidos.ReproducirAleatorio("Gnomo Grito 1", "Gnomo Grito 2", "Gnomo Grito 3", "Gnomo Grito 4");
        escena.GnomoEliminado(this);

        // El objeto es eliminado de la escena--will (cuando termina su animacion de muerte)
        animador.Reproducir(cuadrosMuerte, FpsMuerte, false, () => Destroy(gameObject));
    }

    // ------------------------------------------------------------------ llegada a la planta

    void Llegar()
    {
        estado = Estado.Llegando;
        animador.Detener();
        escena.GnomoLlegoALaPlanta(this);
        StartCoroutine(Desvanecerse());
    }

    // Despues de pellizcar la planta, el gnomo salta y se esfuma
    IEnumerator Desvanecerse()
    {
        const float duracion = 0.4f;
        float t = 0f;
        Color color = imagen.color;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracion);
            float s = escala * (1f + 0.3f * Mathf.Sin(k * Mathf.PI));
            rt.localScale = new Vector3(s, s, 1f);
            color.a = 1f - k;
            imagen.color = color;
            yield return null;
        }
        Destroy(gameObject);
    }
}
