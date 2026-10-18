using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu de inicio: botones Jugar y Salir.
public class MenuPrincipal : MonoBehaviour
{
    public Button botonJugar;
    public Button botonSalir;

    void Start()
    {
        botonJugar.onClick.AddListener(Jugar);
        botonSalir.onClick.AddListener(Salir);
    }

    void Jugar()
    {
        SceneManager.LoadScene("SeleccionPlanta");
    }

    void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
