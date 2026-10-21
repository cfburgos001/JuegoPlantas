using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu de pausa (sirve igual en el Dia y en la Noche).
//
//   - Se abre con el boton de pausa (arriba a la derecha) o con la tecla Esc.
//   - Detiene el juego poniendo Time.timeScale = 0 (todo lo que usa Time.deltaTime se congela).
//   - Baja el volumen de la musica mientras esta abierto.
//   - Botones: Reanudar, Reiniciar y Menu principal.
public class MenuPausa : MonoBehaviour
{
    [Header("Referencias (las pone el constructor de escenas)")]
    public Button botonPausa;
    public GameObject panel;
    public Button botonReanudar;
    public Button botonReiniciar;
    public Button botonMenu;

    [Tooltip("Escena que se carga al presionar Reiniciar (Dia1 o EscenaNoche)")]
    public string escenaReiniciar = "Dia1";

    // true mientras el juego esta en pausa. Otros scripts lo consultan (por ejemplo las herramientas).
    public static bool EnPausa { get; private set; }

    // Se pone en true cuando el nivel ya termino, para que ya no se pueda pausar.
    public bool bloqueado { get; private set; }

    // Se llama cuando el nivel termina: ya no se puede pausar y el boton de pausa desaparece.
    public void Bloquear()
    {
        bloqueado = true;
        if (botonPausa != null) botonPausa.gameObject.SetActive(false);
    }

    void Start()
    {
        EnPausa = false;
        Time.timeScale = 1f;
        panel.SetActive(false);

        botonPausa.onClick.AddListener(Pausar);
        botonReanudar.onClick.AddListener(Reanudar);
        botonReiniciar.onClick.AddListener(Reiniciar);
        botonMenu.onClick.AddListener(IrAlMenu);
    }

    void Update()
    {
        if (EntradaMouse.EscapeEsteFrame) Alternar();
    }

    void OnDestroy()
    {
        // Seguridad: si esta escena se cierra estando en pausa, el juego no debe quedar congelado
        if (EnPausa)
        {
            EnPausa = false;
            Time.timeScale = 1f;
        }
    }

    public void Alternar()
    {
        if (EnPausa) Reanudar();
        else Pausar();
    }

    public void Pausar()
    {
        if (EnPausa || bloqueado) return;
        EnPausa = true;
        Time.timeScale = 0f;
        panel.SetActive(true);
        Sonidos.Reproducir("Poner Pausa v2", 1f, 1.6f);
        AdmAudio.Atenuar(true);
    }

    public void Reanudar()
    {
        if (!EnPausa) return;
        SalirDePausa();
        Sonidos.Reproducir("Reanudar", 1f, 1.4f);
    }

    void Reiniciar()
    {
        SalirDePausa();
        // Reiniciar el dia empieza de cero; reiniciar la noche conserva lo que se hizo en el dia
        if (escenaReiniciar == "Dia1") GameData.ResetearDatos();
        SceneManager.LoadScene(escenaReiniciar);
    }

    void IrAlMenu()
    {
        SalirDePausa();
        GameData.ResetearDatos();
        SceneManager.LoadScene("Menu");
    }

    void SalirDePausa()
    {
        EnPausa = false;
        Time.timeScale = 1f;
        panel.SetActive(false);
        AdmAudio.Atenuar(false);
    }
}
