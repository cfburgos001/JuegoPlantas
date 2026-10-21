using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla final: muestra el resumen del dia completo (dia + noche) y el puntaje total.
// Creada por will; el puntaje final es el que se gano en la noche (gnomos eliminados).
// Los textos son "Text" normales (igual que el resto del juego); la escena la arma Plantas > 1.
public class PantallaResultados : MonoBehaviour
{
    [Header("Referencias de UI (las pone el constructor de escenas)")]
    public Text textoTitulo;
    public Image imagenPlanta;
    public Text textoClicks;
    public Text textoCuidados;
    public Text textoEnemigos;
    public Text textoPuntajeTotal;

    [Header("Botón para salir")]
    public Button botonVolverAlMenu;

    void Start()
    {
        bool salvada = GameData.NocheSuperada;

        if (textoTitulo != null)
            textoTitulo.text = salvada ? "¡Tu planta se salvó!" : "¡Los gnomos se la llevaron!";

        // La planta solo se muestra si se salvo
        if (imagenPlanta != null)
        {
            imagenPlanta.gameObject.SetActive(salvada);
            if (salvada)
            {
                Sprite vista = Cargador.CargarSprite("Menu/Planta" + ConfigJuego.PlantaElegida);
                imagenPlanta.sprite = vista;
                imagenPlanta.rectTransform.sizeDelta = vista.rect.size * 1.5f;
            }
        }

        if (textoClicks != null)
            textoClicks.text = "Clicks en el día: " + GameData.ClicksTotales;

        if (textoCuidados != null)
            textoCuidados.text = "Cuidados atendidos: " + GameData.CuidadosAtendidos + "/" + GameData.CuidadosTotales;

        if (textoEnemigos != null)
            textoEnemigos.text = "Gnomos eliminados: " + GameData.EnemigosEliminados + "/" + GameData.GnomosPorNoche;

        if (textoPuntajeTotal != null)
            textoPuntajeTotal.text = "Puntaje final: " + GameData.PuntajeTotal + " pts";

        if (botonVolverAlMenu != null)
        {
            botonVolverAlMenu.onClick.AddListener(() => {
                Sonidos.Reproducir("Seleccion 1", 0.9f, 1.0f);
                GameData.ResetearDatos();
                SceneManager.LoadScene("Menu"); // Asegúrate de que tu escena de menú se llame "Menu"
            });
        }
    }
}
