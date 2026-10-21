using System;
using UnityEngine;

// Ayudante para cargar sprites desde  Assets/Resources/Sprites/
// Ejemplo:  Cargador.CargarSprite("Menu/Boton")
public static class Cargador
{
    const string Raiz = "Sprites/";

    // Los dibujos de crecimiento (PlantaN-Estado3...) tienen la maceta 1.5 px a la izquierda del centro
    // de su lienzo de 128x128. Se suma este valor a la X para que la maceta quede justo en el pasto.
    public const float DesfaseXCrecimiento = 1.5f;

    public static Sprite CargarSprite(string ruta)
    {
        Sprite sprite = Resources.Load<Sprite>(Raiz + ruta);
        if (sprite == null)
        {
            Debug.LogError("No encontré el sprite '" + ruta + "'. ¿Ya ejecutaste el menú Plantas > Actualizar Día y Noche?");
        }
        return sprite;
    }

    // Carga todos los cuadros de una carpeta de animacion, en orden (_00, _01, _02...)
    public static Sprite[] CargarAnimacion(string carpeta)
    {
        Sprite[] cuadros = IntentarCargarAnimacion(carpeta);
        if (cuadros == null)
        {
            Debug.LogError("No encontré la animación '" + carpeta + "'.");
            return new Sprite[0];
        }
        return cuadros;
    }

    // Igual que CargarAnimacion, pero si la carpeta no existe devuelve null sin mostrar error.
    public static Sprite[] IntentarCargarAnimacion(string carpeta)
    {
        Sprite[] cuadros = Resources.LoadAll<Sprite>(Raiz + carpeta);
        if (cuadros == null || cuadros.Length == 0) return null;
        Array.Sort(cuadros, (a, b) => string.CompareOrdinal(a.name, b.name));
        return cuadros;
    }

    // Animacion de la planta ya crecida segun el dia: dia 1 -> Estado3, dia 2 -> Estado4, dia 3 -> Estado5.
    // Si esa planta no tiene el dibujo (las plantas 4 y 5 solo tienen el EstadoFinal), prueba con el
    // anterior y, si no hay ninguno, devuelve null (quien llama usa el brote generico del dia 1).
    public static Sprite[] CargarCrecimiento(int planta, int dia)
    {
        for (int estado = Mathf.Clamp(2 + dia, 3, 5); estado >= 3; estado--)
        {
            Sprite[] cuadros = IntentarCargarAnimacion("Dia/Anim/Planta" + planta + "-Estado" + estado);
            if (cuadros != null) return cuadros;
        }
        return null;
    }
}
