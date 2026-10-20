using UnityEngine;

public class EnemigoNoche : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    public int vidaMaxima = 2;   // Los golpes que aguanta el enemigo--will
    public int puntosAlMorir = 50; // Los puntos al derrotarlo--will

    private int vidaActual;

    void Start()
    {
        // Inicial con salud maxima el enemigo-will
        vidaActual = vidaMaxima;
    }

    // la forma en que recibira daño--will
    public void RecibirDano(int cantidadDano)
    {
        vidaActual -= cantidadDano;

        // se elimina cuando llega a 0--will
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        GameData.EnemigosEliminados++;
        GameData.PuntajeNoche += puntosAlMorir;
        GameData.PuntajeTotal += puntosAlMorir;

        
        Debug.Log("Enemigo eliminado. Total bajas: " + GameData.EnemigosEliminados + " | Puntos: " + GameData.PuntajeTotal);

        // El objeto es eliminado de la escena--will
        Destroy(gameObject);
    }
}
