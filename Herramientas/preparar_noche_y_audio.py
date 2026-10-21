"""
Prepara el arte de la NOCHE y de la PAUSA para Unity (corta las hojas de animacion en cuadros sueltos).

Uso:
    python preparar_noche_y_audio.py <carpeta_Prototipos> <carpeta_del_proyecto>

    <carpeta_Prototipos>  carpeta que contiene Enemy_1.png, Enemy_1_Death.png, Enemy_1_Hit.png,
                          Fondo_Noche.png, matamoscas.png, Pausa_Menu.png (y la subcarpeta PXO's)

Que hace:
  Assets/Resources/Sprites/Noche/FondoNoche.png
  Assets/Resources/Sprites/Noche/Anim/Gnomo_Caminar/    8 cuadros de 64x64
  Assets/Resources/Sprites/Noche/Anim/Gnomo_Golpe/      7 cuadros de 64x64
  Assets/Resources/Sprites/Noche/Anim/Gnomo_Muerte/    10 cuadros de 64x64
  Assets/Resources/Sprites/Noche/Anim/Matamoscas/       6 cuadros de 128x128
  Assets/Resources/Sprites/Pausa/Anim/PausaTitulo/      4 cuadros de 64x44
  Assets/Resources/Sprites/Pausa/Anim/PausaBoton/       6 cuadros de 40x40
  Assets/Resources/Sprites/Noche/Corazon.png            corazon de vida (11x10, dibujado por codigo)
  Assets/Resources/Sprites/Noche/CorazonVacio.png       corazon perdido
  Arte_Original/Noche/                                  copia de los PNG y de los .pxo originales

Requiere:  pip install pillow
"""
import os
import shutil
import sys

from PIL import Image

# nombre del archivo original -> (carpeta de salida, nombre base, numero de cuadros)
HOJAS_SIMPLES = {
    "Enemy_1.png": ("Gnomo_Caminar", 8),
    "Enemy_1_Hit.png": ("Gnomo_Golpe", 7),
    "Enemy_1_Death.png": ("Gnomo_Muerte", 10),
    "matamoscas.png": ("Matamoscas", 6),
}


def guardar(img, ruta):
    os.makedirs(os.path.dirname(ruta), exist_ok=True)
    img.save(ruta)


# Silueta del corazon (9x8). Se le agrega 1 px de contorno, asi que el PNG final mide 11x10.
CORAZON = [
    ".XX...XX.",
    "XXXX.XXXX",
    "XXXXXXXXX",
    "XXXXXXXXX",
    ".XXXXXXX.",
    "..XXXXX..",
    "...XXX...",
    "....X....",
]


def dibujar_corazon(relleno, brillo, contorno):
    alto, ancho = len(CORAZON), len(CORAZON[0])
    img = Image.new("RGBA", (ancho + 2, alto + 2), (0, 0, 0, 0))
    px = img.load()
    nucleo = {(x, y) for y, fila in enumerate(CORAZON) for x, c in enumerate(fila) if c == "X"}
    for (x, y) in nucleo:  # contorno: vecinos (8 direcciones) del nucleo
        for dx in (-1, 0, 1):
            for dy in (-1, 0, 1):
                px[x + 1 + dx, y + 1 + dy] = contorno
    for (x, y) in nucleo:
        px[x + 1, y + 1] = relleno
    if brillo is not None:
        for (x, y) in ((1, 1), (2, 1), (1, 2)):
            px[x + 1, y + 1] = brillo
    return img


def cortar_horizontal(img, n):
    ancho, alto = img.size
    assert ancho % n == 0, f"{ancho}px no se divide en {n} cuadros"
    w = ancho // n
    return [img.crop((i * w, 0, (i + 1) * w, alto)) for i in range(n)]


def main(origen, proyecto):
    sprites = os.path.join(proyecto, "Assets", "Resources", "Sprites")
    for sub in ("Noche", "Pausa"):
        carpeta = os.path.join(sprites, sub)
        if os.path.isdir(carpeta):
            shutil.rmtree(carpeta)

    originales = os.path.join(proyecto, "Arte_Original", "Noche")
    os.makedirs(originales, exist_ok=True)

    total = 0

    # Fondo de la noche
    fondo = Image.open(os.path.join(origen, "Fondo_Noche.png")).convert("RGBA")
    guardar(fondo, os.path.join(sprites, "Noche", "FondoNoche.png"))
    total += 1

    # Gnomo y matamoscas
    for archivo, (nombre, n) in HOJAS_SIMPLES.items():
        hoja = Image.open(os.path.join(origen, archivo)).convert("RGBA")
        for i, cuadro in enumerate(cortar_horizontal(hoja, n)):
            guardar(cuadro, os.path.join(sprites, "Noche", "Anim", nombre, f"{nombre}_{i:02d}.png"))
            total += 1

    # Corazones de vida para el HUD de la noche
    guardar(dibujar_corazon((226, 58, 72, 255), (255, 170, 170, 255), (70, 18, 30, 255)),
            os.path.join(sprites, "Noche", "Corazon.png"))
    guardar(dibujar_corazon((52, 56, 78, 255), None, (22, 24, 36, 255)),
            os.path.join(sprites, "Noche", "CorazonVacio.png"))
    total += 2

    # Pausa_Menu.png: 12 celdas de 64x128. En cada celda hay, de arriba a abajo:
    #   el letrero "PAUSA" (filas 0-44), el boton de pausa (filas 44-84) y un gnomo asomandose (abajo).
    # Solo se usan el letrero (4 cuadros distintos) y el boton (6 cuadros distintos).
    pausa = Image.open(os.path.join(origen, "Pausa_Menu.png")).convert("RGBA")
    for i in range(4):
        celda = pausa.crop((i * 64, 0, (i + 1) * 64, 44))
        guardar(celda, os.path.join(sprites, "Pausa", "Anim", "PausaTitulo", f"PausaTitulo_{i:02d}.png"))
        total += 1
    for i in range(6):
        cx = i * 64 + 32
        boton = pausa.crop((cx - 20, 44, cx + 20, 84))  # 40x40 centrado en el boton
        guardar(boton, os.path.join(sprites, "Pausa", "Anim", "PausaBoton", f"PausaBoton_{i:02d}.png"))
        total += 1

    # Copia de los originales
    for archivo in list(HOJAS_SIMPLES) + ["Fondo_Noche.png", "Pausa_Menu.png"]:
        shutil.copy(os.path.join(origen, archivo), os.path.join(originales, archivo))
    pxo = os.path.join(origen, "PXO's")
    if os.path.isdir(pxo):
        destino = os.path.join(originales, "Fuentes_pxo")
        if os.path.isdir(destino):
            shutil.rmtree(destino)
        shutil.copytree(pxo, destino)

    print(f"Sprites generados: {total}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print(__doc__)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])
