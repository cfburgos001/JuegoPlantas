using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// El matamoscas de la noche: sigue al mouse y golpea al hacer clic.
//
// Este script va en "CapaClics", una imagen invisible del tamano de toda la pantalla que esta por
// debajo de los botones. Asi un clic sobre el boton de pausa NO cuenta como golpe.
// La imagen del matamoscas ("Cursor") es otro objeto aparte que solo se mueve con el mouse.
public class Matamoscas : MonoBehaviour, IPointerDownHandler
{
    [Header("Referencias (las pone el constructor de escenas)")]
    public RectTransform pantalla;
    public RectTransform imagenCursor;
    public AnimadorSprites animador;
    public EscenaNoche escena;

    [Header("Ajustes")]
    [Tooltip("Tamano del matamoscas en pantalla (1 = tamano del dibujo)")]
    public float escala = 0.85f;
    [Tooltip("Segundos minimos entre un golpe y el siguiente")]
    public float enfriamiento = 0.22f;
    public float cuadrosPorSegundo = 22f;

    Image imagenDelCursor;
    Sprite[] cuadros;
    float proximoGolpe;

    void Start()
    {
        cuadros = Cargador.CargarAnimacion("Noche/Anim/Matamoscas");
        imagenDelCursor = imagenCursor.GetComponent<Image>();
        imagenCursor.localScale = new Vector3(escala, escala, 1f);
        VolverAReposo();
    }

    void OnDisable()
    {
        Cursor.visible = true; // por si se sale de la escena
    }

    void Update()
    {
        // El matamoscas reemplaza al cursor normal, menos en la pausa y cuando la noche ya termino
        bool visible = !MenuPausa.EnPausa && !escena.Terminada;
        if (imagenCursor.gameObject.activeSelf != visible) imagenCursor.gameObject.SetActive(visible);
        Cursor.visible = !visible;
        if (!visible) return;

        Vector2 punto = escena.PantallaAPuntoDeJuego(EntradaMouse.Posicion);
        imagenCursor.anchoredPosition = new Vector2(punto.x, -punto.y);
    }

    public void OnPointerDown(PointerEventData evento)
    {
        if (evento.button != PointerEventData.InputButton.Left) return;
        if (MenuPausa.EnPausa || escena.Terminada) return;
        if (Time.time < proximoGolpe) return;
        proximoGolpe = Time.time + enfriamiento;

        // Animacion y sonido del golpe (se ven aunque todavia no haya gnomos)
        animador.Reproducir(cuadros, cuadrosPorSegundo, false, VolverAReposo);
        Sonidos.Reproducir("Matamoscas");

        // El golpe cae justo donde esta el centro del matamoscas
        if (escena.Jugando) escena.Golpear(escena.PantallaAPuntoDeJuego(evento.position));
    }

    void VolverAReposo()
    {
        if (cuadros == null || cuadros.Length == 0) return;
        imagenDelCursor.sprite = cuadros[0];
        imagenCursor.sizeDelta = cuadros[0].rect.size;
    }
}
