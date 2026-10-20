using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement;
using UnityEngine.UI; 

public class PantallaResultados : MonoBehaviour
{
    [Header("Referencias de UI (TextMeshPro)")]
    public TextMeshProUGUI textoClicks;
    public TextMeshProUGUI textoEnemigos;
    public TextMeshProUGUI textoPuntajeTotal;

    [Header("Botón para salir")]
    public Button botonVolverAlMenu;

    void Start()
    {
        if (textoClicks != null)
            textoClicks.text = "Clicks en el día: " + GameData.ClicksTotales;

        if (textoEnemigos != null)
            textoEnemigos.text = "Enemigos eliminados: " + GameData.EnemigosEliminados;

        if (textoPuntajeTotal != null)
            textoPuntajeTotal.text = "Puntaje Final: " + GameData.PuntajeTotal + " pts";


        if (botonVolverAlMenu != null)
        {
            botonVolverAlMenu.onClick.AddListener(() => {
                GameData.ResetearDatos();
                SceneManager.LoadScene("Menu"); // Asegúrate de que tu escena de menú se llame "Menu"
            });
        }
    }
}
