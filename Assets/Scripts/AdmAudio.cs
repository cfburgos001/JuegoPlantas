using UnityEngine;
using UnityEngine.SceneManagement;

public class AdmAudio : MonoBehaviour  //todo esto es de la musica--will
{
    public static AdmAudio Instance { get; private set; }

    [Header("Componentes de Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Clips de Música")]
    [SerializeField] private AudioClip musicaMenu;
    [SerializeField] private AudioClip musicaDia;
    [SerializeField] private AudioClip musicaNoche;

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
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        switch (scene.name)
        {
            case "Menu":
                CambiarMusica(musicaMenu);
                break;
            case "Dia1":
                CambiarMusica(musicaDia);
                break;
            case "EscenaNoche":
                CambiarMusica(musicaNoche);
                break;
        }
    }

    private void CambiarMusica(AudioClip nuevoClip)
    {
        if (audioSource.clip == nuevoClip) return;

        audioSource.Stop();
        audioSource.clip = nuevoClip;
        audioSource.Play();
    }
}
