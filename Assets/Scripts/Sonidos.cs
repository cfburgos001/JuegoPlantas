using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Reproductor de efectos de sonido (no es la musica de fondo: esa la maneja AdmAudio).
//
// Uso desde cualquier script:
//     Sonidos.Reproducir("Cuidado exitoso");              // suena completo
//     Sonidos.Reproducir("Cuidado exitoso", 0.8f, 2f);    // al 80% de volumen y se corta (con fade) a los 2 s
//     Sonidos.ReproducirAleatorio("Gnomo Grito 1", "Gnomo Grito 2");
//
// El nombre es el del archivo de audio, sin importar mayusculas, tildes ni espacios
// ("Selección 1", "seleccion 1" y "Seleccion1" son lo mismo).
//
// La lista de clips (Clips) la llena sola la herramienta de Unity: Plantas > 1. Actualizar Dia y Noche.
// Se guarda en el prefab  Assets/Resources/Sonidos.prefab  y se crea solo la primera vez que se necesita.
public class Sonidos : MonoBehaviour
{
    [Tooltip("Todos los efectos de sonido. Los llena el menu Plantas > 1. Actualizar Dia y Noche")]
    public AudioClip[] clips;

    [Range(0f, 1f)]
    public float volumenGeneral = 0.9f;

    const int Voces = 10; // cuantos sonidos pueden sonar a la vez

    static Sonidos instancia;

    AudioSource[] voces;
    Coroutine[] cortes;
    Dictionary<string, AudioClip> porNombre;
    int siguiente;

    // ------------------------------------------------------------------ uso desde otros scripts

    public static void Reproducir(string nombre, float volumen = 1f, float maxSegundos = 0f)
    {
        Sonidos s = Obtener();
        AudioClip clip = s.Buscar(nombre);
        if (clip == null)
        {
            Debug.LogWarning("Sonidos: no encontré el efecto '" + nombre + "'. ¿Ya ejecutaste Plantas > 1. Actualizar Día y Noche?");
            return;
        }
        s.Tocar(clip, volumen, maxSegundos);
    }

    public static void ReproducirAleatorio(params string[] nombres)
    {
        if (nombres == null || nombres.Length == 0) return;
        Reproducir(nombres[Random.Range(0, nombres.Length)]);
    }

    public static void DetenerTodos()
    {
        if (instancia == null) return;
        for (int i = 0; i < instancia.voces.Length; i++)
        {
            if (instancia.cortes[i] != null) instancia.StopCoroutine(instancia.cortes[i]);
            instancia.cortes[i] = null;
            instancia.voces[i].Stop();
        }
    }

    // Deja un nombre en minusculas, sin tildes, sin espacios ni simbolos.
    public static string Normalizar(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        StringBuilder sb = new StringBuilder(texto.Length);
        foreach (char c0 in texto)
        {
            char c = char.ToLowerInvariant(c0);
            switch (c)
            {
                case 'á': case 'à': case 'ä': case 'â': c = 'a'; break;
                case 'é': case 'è': case 'ë': case 'ê': c = 'e'; break;
                case 'í': case 'ì': case 'ï': case 'î': c = 'i'; break;
                case 'ó': case 'ò': case 'ö': case 'ô': c = 'o'; break;
                case 'ú': case 'ù': case 'ü': case 'û': c = 'u'; break;
                case 'ñ': c = 'n'; break;
            }
            if (char.IsLetterOrDigit(c)) sb.Append(c);
        }
        return sb.ToString();
    }

    // ------------------------------------------------------------------ interno

    static Sonidos Obtener()
    {
        if (instancia == null)
        {
            Sonidos prefab = Resources.Load<Sonidos>("Sonidos");
            if (prefab != null) Instantiate(prefab);
            else new GameObject("Sonidos").AddComponent<Sonidos>();
        }
        return instancia;
    }

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        gameObject.name = "Sonidos";
        DontDestroyOnLoad(gameObject);

        voces = new AudioSource[Voces];
        cortes = new Coroutine[Voces];
        for (int i = 0; i < Voces; i++)
        {
            AudioSource fuente = gameObject.AddComponent<AudioSource>();
            fuente.playOnAwake = false;
            fuente.loop = false;
            fuente.spatialBlend = 0f; // sonido 2D
            voces[i] = fuente;
        }

        porNombre = new Dictionary<string, AudioClip>();
        if (clips != null)
        {
            foreach (AudioClip clip in clips)
            {
                if (clip != null) porNombre[Normalizar(clip.name)] = clip;
            }
        }
    }

    AudioClip Buscar(string nombre)
    {
        AudioClip clip;
        return porNombre.TryGetValue(Normalizar(nombre), out clip) ? clip : null;
    }

    void Tocar(AudioClip clip, float volumen, float maxSegundos)
    {
        // Busca una "voz" libre; si todas estan ocupadas, reutiliza la mas antigua
        int i = -1;
        for (int k = 0; k < voces.Length; k++)
        {
            if (!voces[k].isPlaying) { i = k; break; }
        }
        if (i < 0)
        {
            i = siguiente;
            siguiente = (siguiente + 1) % voces.Length;
        }

        if (cortes[i] != null)
        {
            StopCoroutine(cortes[i]);
            cortes[i] = null;
        }

        AudioSource voz = voces[i];
        voz.Stop();
        voz.clip = clip;
        voz.volume = Mathf.Clamp01(volumen * volumenGeneral);
        voz.Play();

        if (maxSegundos > 0f && maxSegundos < clip.length)
        {
            cortes[i] = StartCoroutine(Cortar(voz, i, maxSegundos));
        }
    }

    // Corta el sonido a los "maxSegundos" bajando el volumen poco a poco (varios efectos traen
    // mucho silencio o musica al final y no conviene que suenen completos).
    IEnumerator Cortar(AudioSource voz, int indice, float maxSegundos)
    {
        float fade = Mathf.Min(0.4f, maxSegundos * 0.4f);
        yield return new WaitForSecondsRealtime(maxSegundos - fade);

        float volumenInicial = voz.volume;
        float t = 0f;
        while (t < fade)
        {
            t += Time.unscaledDeltaTime;
            voz.volume = Mathf.Lerp(volumenInicial, 0f, t / fade);
            yield return null;
        }
        voz.Stop();
        cortes[indice] = null;
    }
}
