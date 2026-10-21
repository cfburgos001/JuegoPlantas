using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Pantalla de seleccion de planta: el jugador toca una planta y luego "Elegir".
public class SeleccionPlantas : MonoBehaviour
{
    [Tooltip("Los 7 botones, en orden Planta1 a Planta7")]
    public Button[] botonesPlantas;
    public Button botonElegir;
    public Button botonVolver;
    public Text textoNombre;

    const float EscalaSeleccionada = 1.3f;
    int seleccionada = 0; // 0 = ninguna

    void Start()
    {
        for (int i = 0; i < botonesPlantas.Length; i++)
        {
            int numero = i + 1; // se copia para que cada boton recuerde su numero
            botonesPlantas[i].interactable = ConfigJuego.EstaDisponible(numero);
            botonesPlantas[i].onClick.AddListener(() => Seleccionar(numero));
        }
        botonElegir.onClick.AddListener(Elegir);
        botonVolver.onClick.AddListener(Volver);
        Refrescar();
    }

    void Seleccionar(int numero)
    {
        seleccionada = numero;
        Sonidos.Reproducir("Seleccion 2", 0.8f, 1.0f);
        Refrescar();
    }

    void Refrescar()
    {
        for (int i = 0; i < botonesPlantas.Length; i++)
        {
            float escala = (i + 1 == seleccionada) ? EscalaSeleccionada : 1f;
            botonesPlantas[i].transform.localScale = new Vector3(escala, escala, 1f);
        }
        botonElegir.interactable = seleccionada > 0;
        textoNombre.text = seleccionada > 0 ? ConfigJuego.NombrePlanta(seleccionada) : "Toca una planta";
    }

    void Elegir()
    {
        ConfigJuego.PlantaElegida = seleccionada;
        ConfigJuego.DiaActual = 1;
        GameData.ResetearDatos(); // partida nueva: puntajes y cuidados en cero
        Sonidos.Reproducir("Iniciar nivel", 1f, 1.6f);
        SceneManager.LoadScene("Dia1");
    }

    void Volver()
    {
        Sonidos.Reproducir("Seleccion 1", 0.9f, 1.0f);
        SceneManager.LoadScene("Menu");
    }
}
