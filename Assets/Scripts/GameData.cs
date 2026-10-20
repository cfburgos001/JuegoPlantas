using UnityEngine;

public static class GameData
{
    // Estas sera las estadisticas del dia 
    public static int ClicksTotales = 0;
    public static bool TierraCambiada = false;
    public static bool PlantaRegada = false;

    // Esta de la noche
    public static int EnemigosEliminados = 0;
    public static int PuntajeNoche = 0;

    // El puntaje
    public static int PuntajeTotal = 0;

    public static void ResetearDatos()
    {
        ClicksTotales = 0;
        TierraCambiada = false;
        PlantaRegada = false;
        EnemigosEliminados = 0;
        PuntajeNoche = 0;
        PuntajeTotal = 0;
    }
}
