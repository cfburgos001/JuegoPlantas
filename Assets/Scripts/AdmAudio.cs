using UnityEngine;
using UnityEngine.SceneManagement;

// Maneja la MUSICA DE FONDO (los efectos de sonido los maneja la clase Sonidos).
// Creado por will; ampliado para: repetir la musica en bucle, bajar el volumen en la pausa
// y permitir que una escena pida su propia musica (AdmAudio.Musica(clip)).
public class AdmAudio : MonoBehaviour  //todo esto es de la musica--will
{
    public static AdmAudio Instance { get; private set; }

    [Header("Componentes de Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Clips de Música")]
    [SerializeField] private AudioClip musicaMenu;
    [SerializeField] private AudioClip musicaDia;
    [SerializeField] private AudioClip musicaNoche;

    [Header("Volumen mientras el juego está en pausa (0 a 1, respecto al normal)")]
    [SerializeField, Range(0f, 1f)] private float volumenEnPausa = 0.25f;

    private float volumenNormal = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Evita que se destruya al cambiar de escena o algo asi
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // la musica se repite
        volumenNormal = audioSource.volume;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Por si esta escena ya estaba cargada cuando este objeto nacio
        if (Instance == this) OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;
        AplicarVolumen(false); // por si se salio de una escena estando en pausa

        switch (scene.name)
        {
            case "Menu":
                if (musicaMenu != null) CambiarMusica(musicaMenu);
                else DetenerMusica(); // sin musica asignada: silencio en el menu
                break;
            case "Dia1":
                CambiarMusica(musicaDia);
                break;
            case "EscenaNoche":
                CambiarMusica(musicaNoche);
                break;
        }
    }

    // ------------------------------------------------------------------ uso desde otros scripts

    // Una escena puede pedir su propia musica:  AdmAudio.Musica(miClip);
    // Si todavia no existe un AdmAudio (por ejemplo, al probar una escena sola), lo crea.
    public static void Musica(AudioClip clip)
    {
        if (clip == null) return;
        if (Instance == null)
        {
            GameObject go = new GameObject("AdmAudio");
            go.AddComponent<AudioSource>();
            go.AddComponent<AdmAudio>();
        }
        Instance.CambiarMusica(clip);
    }

    // Baja o devuelve el volumen de la musica (lo usa el menu de pausa)
    public static void Atenuar(bool atenuar)
    {
        if (Instance != null) Instance.AplicarVolumen(atenuar);
    }

    // ------------------------------------------------------------------ interno

    private void AplicarVolumen(bool atenuar)
    {
        if (audioSource == null) return;
        audioSource.volume = atenuar ? volumenNormal * volumenEnPausa : volumenNormal;
    }

    private void CambiarMusica(AudioClip nuevoClip)
    {
        if (nuevoClip == null) return; // sin clip asignado se deja sonar lo que haya
        if (audioSource.clip == nuevoClip && audioSource.isPlaying) return;

        audioSource.Stop();
        audioSource.clip = nuevoClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void DetenerMusica()
    {
        audioSource.Stop();
        audioSource.clip = null;
    }
}
