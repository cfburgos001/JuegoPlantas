using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro;              

public class EscenaNoche : MonoBehaviour
{
    [Header("Configuración del Nivel Nocturno")]
    public float duracionDeLaNoche = 60f; // Este es el tiempo que le eh puesto de game nocturno, ha criterio de cuanto se deciden--will

    [Header("UI (Opcional)")]
    public TextMeshProUGUI textoTemporizador; // Se veera el tiempo en pantalla, aun hay que ver como se mira-will

    [Header("Referencias de la Planta")]
    public UnityEngine.UI.Image imagenPlanta;

    private float tiempoRestante;
    private bool nocheTerminada = false;

    void Start()
    {
        tiempoRestante = duracionDeLaNoche;
        nocheTerminada = false;
        // Carga la planta que seleccionó el jugador al inicio--will
        int plantaElegida = ConfigJuego.PlantaElegida;
        if (imagenPlanta != null)
        { //Tengo un problema aqui y es que no entiend si es por cuestiones de directorios o que pero no me enruta bien para que aparezca la planta 
          //que se ha elegido, si puedes modificarla si ya sepas cual es la ruta, o si cloud te da alguna mejor solucion ocupala
          //por que si no logro modificar correctamente, lo demas creo que lo deje solo para que modifiques la parte del fondo de noche, mañana que suban las cosas
          //creeria y ya esta la tabla de puntaje pero faltaria probar si se suma con las ordas cuando las coloques
            string rutaArchivo = "Planta" + plantaElegida + "-Estado3/Planta" + plantaElegida + "-Estado3_00";
            imagenPlanta.sprite = Cargador.CargarSprite("Dia/" + rutaArchivo);

            imagenPlanta.color = Color.white;

            imagenPlanta.SetNativeSize();
            imagenPlanta.rectTransform.localScale = new Vector3(3.5f, 3.5f, 1f);
        }//hasta aqui llega el error que hace que no aparezca la planta que seleccionas en el dia. cualquier cosa mañana le damos secso
    }   
    void Update()
    {
        if (nocheTerminada) return;

        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;

            if (textoTemporizador != null)
            {
                textoTemporizador.text = "Tiempo restante: " + Mathf.CeilToInt(tiempoRestante) + "s";
            }
        }
        else
        {
            TerminarNocheYVerResultados();
        }
    }

    void TerminarNocheYVerResultados()
    {
        nocheTerminada = true;
        SceneManager.LoadScene("PantallaResultados");
    }
}
