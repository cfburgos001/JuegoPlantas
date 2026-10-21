using UnityEngine;

// Datos de la partida que se pasan de una escena a otra (Dia -> Noche -> Resultados).
// Creado por will; ampliado con los cuidados del dia, las vidas de la planta y el puntaje de la noche.
public static class GameData
{
    // ---------------------------------------------------------------- DIA

    // Estas seran las estadisticas del dia
    public static int ClicksTotales = 0;
    public static bool TierraCambiada = false;
    public static bool PlantaRegada = false;

    // Cuidados que pidio la planta durante el dia y cuantos se atendieron a tiempo
    public static int CuidadosAtendidos = 0;
    public static int CuidadosTotales = 0;

    // ---------------------------------------------------------------- NOCHE

    public const int GnomosPorNoche = 10;   // cuantos gnomos salen en la noche
    public const int VidasMaximas = 3;      // cuantos gnomos pueden llegar a la planta antes de perderla

    // Esta de la noche
    public static int EnemigosEliminados = 0;
    public static int PuntajeNoche = 0;
    public static int VidasPlanta = VidasMaximas;
    public static bool NocheSuperada = false;

    // ---------------------------------------------------------------- PUNTAJE

    // El puntaje final del dia completo = el puntaje que se ganó en la noche
    public static int PuntajeTotal = 0;

    // Suma los puntos de un gnomo eliminado
    public static void SumarPuntosNoche(int puntos)
    {
        PuntajeNoche += puntos;
        PuntajeTotal = PuntajeNoche;
    }

    // Se llama al empezar la noche: borra solo lo de la noche (lo del dia se conserva)
    public static void ReiniciarNoche()
    {
        EnemigosEliminados = 0;
        PuntajeNoche = 0;
        PuntajeTotal = 0;
        VidasPlanta = VidasMaximas;
        NocheSuperada = false;
    }

    public static void ResetearDatos()
    {
        ClicksTotales = 0;
        TierraCambiada = false;
        PlantaRegada = false;
        CuidadosAtendidos = 0;
        CuidadosTotales = 0;
        ReiniciarNoche();
    }
}
