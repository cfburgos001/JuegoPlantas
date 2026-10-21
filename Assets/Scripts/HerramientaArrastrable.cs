using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum TipoHerramienta
{
    Regadera,
    Pala,
    Tijera
}

// Se pone en cada herramienta de la barra inferior. Al arrastrarla aparece una copia que sigue
// al mouse; si se suelta sobre la zona de la planta, avisa a DiaPlanta que se uso esa herramienta.
public class HerramientaArrastrable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public TipoHerramienta tipo;
    public DiaPlanta dia;
    [Tooltip("Contenedor de 320x572 donde vive todo el juego")]
    public RectTransform pantalla;

    Image imagen;
    RectTransform copia;

    void Awake()
    {
        imagen = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData evento)
    {
        if (MenuPausa.EnPausa) return; // con el juego en pausa no se pueden usar herramientas

        GameObject go = new GameObject("Herramienta (arrastre)", typeof(RectTransform), typeof(Image));
        copia = go.GetComponent<RectTransform>();
        copia.SetParent(pantalla, false);
        copia.anchorMin = new Vector2(0.5f, 0.5f);
        copia.anchorMax = new Vector2(0.5f, 0.5f);
        copia.pivot = new Vector2(0.5f, 0.5f);
        copia.sizeDelta = imagen.rectTransform.sizeDelta;

        Image img = go.GetComponent<Image>();
        img.sprite = imagen.sprite;
        img.raycastTarget = false;

        imagen.color = new Color(1f, 1f, 1f, 0.35f); // la original queda "apagada" mientras se arrastra
        Mover(evento);
    }

    public void OnDrag(PointerEventData evento)
    {
        if (copia != null) Mover(evento);
    }

    public void OnEndDrag(PointerEventData evento)
    {
        if (copia == null) return; // el arrastre nunca empezo (estaba en pausa)

        bool sobrePlanta = dia != null && !MenuPausa.EnPausa &&
            RectTransformUtility.RectangleContainsScreenPoint(dia.zonaUso, evento.position, null);

        Destroy(copia.gameObject);
        copia = null;
        imagen.color = Color.white;

        if (sobrePlanta) dia.UsarHerramienta(tipo);
    }

    void Mover(PointerEventData evento)
    {
        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(pantalla, evento.position, null, out local))
        {
            copia.anchoredPosition = local;
        }
    }
}
