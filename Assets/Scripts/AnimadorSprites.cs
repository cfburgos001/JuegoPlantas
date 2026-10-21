using System;
using UnityEngine;
using UnityEngine.UI;

// Reproduce una animacion cambiando el sprite de un Image cuadro por cuadro.
// Se puede usar desde otro script:  animador.Reproducir(cuadros, 8f, false, alTerminar);
// o dejarlo automatico escribiendo una carpeta en "carpetaAlIniciar" (ej: Dia/Anim/Nubes).
[RequireComponent(typeof(Image))]
public class AnimadorSprites : MonoBehaviour
{
    [Tooltip("Carpeta dentro de Resources/Sprites que se reproduce sola al iniciar (opcional)")]
    public string carpetaAlIniciar = "";
    public float fps = 8f;
    public bool repetirAlIniciar = true;
    [Tooltip("Marcar si la animacion debe seguir moviendose aunque el juego este en pausa")]
    public bool usarTiempoReal = false;

    Image imagen;
    Sprite[] cuadros;
    bool repetir;
    Action alTerminar;
    float tiempo;
    int indice;
    bool activo;

    void Awake()
    {
        imagen = GetComponent<Image>();
    }

    void Start()
    {
        if (!string.IsNullOrEmpty(carpetaAlIniciar))
        {
            Reproducir(Cargador.CargarAnimacion(carpetaAlIniciar), fps, repetirAlIniciar, null);
        }
    }

    public void Reproducir(Sprite[] nuevosCuadros, float velocidad, bool repetirAnimacion, Action alFinalizar = null)
    {
        if (imagen == null) imagen = GetComponent<Image>();
        cuadros = nuevosCuadros;
        fps = velocidad;
        repetir = repetirAnimacion;
        alTerminar = alFinalizar;
        indice = 0;
        tiempo = 0f;
        activo = cuadros != null && cuadros.Length > 0;
        if (activo) Mostrar(0);
        else if (alFinalizar != null) alFinalizar();
    }

    public void Detener()
    {
        activo = false;
        alTerminar = null;
    }

    void Update()
    {
        if (!activo) return;

        tiempo += usarTiempoReal ? Time.unscaledDeltaTime : Time.deltaTime;
        float duracionCuadro = 1f / Mathf.Max(fps, 0.01f);

        while (tiempo >= duracionCuadro)
        {
            tiempo -= duracionCuadro;
            indice++;

            if (indice >= cuadros.Length)
            {
                if (repetir)
                {
                    indice = 0;
                }
                else
                {
                    activo = false;
                    Action callback = alTerminar;
                    alTerminar = null;
                    if (callback != null) callback();
                    return;
                }
            }
            Mostrar(indice);
        }
    }

    void Mostrar(int n)
    {
        imagen.sprite = cuadros[n];
        imagen.rectTransform.sizeDelta = cuadros[n].rect.size;
    }
}
