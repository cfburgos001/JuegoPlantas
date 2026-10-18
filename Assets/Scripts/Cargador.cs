using System;
using UnityEngine;

// Ayudante para cargar sprites desde  Assets/Resources/Sprites/
// Ejemplo:  Cargador.CargarSprite("Menu/Boton")
public static class Cargador
{
    const string Raiz = "Sprites/";

    public static Sprite CargarSprite(string ruta)
    {
        Sprite sprite = Resources.Load<Sprite>(Raiz + ruta);
        if (sprite == null)
        {
            Debug.LogError("No encontré el sprite '" + ruta + "'. ¿Ya ejecutaste el menú Plantas > Construir todo?");
        }
        return sprite;
    }

    // Carga todos los cuadros de una carpeta de animacion, en orden (_00, _01, _02...)
    public static Sprite[] CargarAnimacion(string carpeta)
    {
        Sprite[] cuadros = Resources.LoadAll<Sprite>(Raiz + carpeta);
        if (cuadros == null || cuadros.Length == 0)
        {
            Debug.LogError("No encontré la animación '" + carpeta + "'.");
            return new Sprite[0];
        }
        Array.Sort(cuadros, (a, b) => string.CompareOrdinal(a.name, b.name));
        return cuadros;
    }
}
