// Datos globales del juego. Aqui se guarda, por ejemplo, que planta eligio el jugador
// para que la escena del dia la pueda leer.
public static class ConfigJuego
{
    // Cambia este texto para poner el nombre real del juego.
    // Luego vuelve a ejecutar: Plantas > Reconstruir solo las escenas
    public const string TituloJuego = "Guardianes del Jardín";

    public const int TotalPlantas = 7;

    // true = el jugador puede elegirla. Pon false en las plantas que aun no estén listas.
    static readonly bool[] disponibles = { true, true, true, true, true, true, true };

    // Planta que eligio el jugador (1 a 7)
    public static int PlantaElegida = 1;

    // Dia actual. Por ahora solo existe el dia 1.
    public static int DiaActual = 1;

    public static bool EstaDisponible(int numeroPlanta)
    {
        int i = numeroPlanta - 1;
        return i >= 0 && i < disponibles.Length && disponibles[i];
    }

    public static string NombrePlanta(int numeroPlanta)
    {
        return "Planta " + numeroPlanta;
    }
}
