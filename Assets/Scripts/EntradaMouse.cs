using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Lee el mouse y el teclado sin importar si el proyecto usa el "Input System" nuevo o el viejo.
// Asi el resto de scripts no tiene que preocuparse por eso:
//     Vector2 donde = EntradaMouse.Posicion;
//     if (EntradaMouse.EscapeEsteFrame) { ... }
public static class EntradaMouse
{
    static Vector2 ultimaPosicion;

    // Posicion del mouse en pixeles de la ventana (0,0 = esquina inferior izquierda)
    public static Vector2 Posicion
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Mouse.current != null) ultimaPosicion = Mouse.current.position.ReadValue();
#else
            ultimaPosicion = Input.mousePosition;
#endif
            return ultimaPosicion;
        }
    }

    // true solo en el frame en que se presiono la tecla Escape
    public static bool EscapeEsteFrame
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
