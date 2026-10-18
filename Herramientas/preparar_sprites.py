"""
Prepara los sprites originales del artista para Unity.

Uso:
    python preparar_sprites.py <carpeta_con_PNGs_originales> <carpeta_del_proyecto>

Que hace:
  1. Limpia nombres (sin tildes, sin espacios, corrige "Plata4y5" -> "Planta4y5").
  2. Copia los sprites sueltos a   Assets/Resources/Sprites/<Menu|Dia>/
  3. Corta las hojas de animacion en cuadros sueltos:
        Assets/Resources/Sprites/Dia/Anim/<Nombre>/<Nombre>_00.png, _01.png, ...
  4. Guarda una copia con nombres limpios de los originales en  Arte_Original/

Requiere:  pip install pillow
"""
import os
import re
import shutil
import sys
import unicodedata

from PIL import Image

# Hojas de animacion que NO siguen el patron de las plantas: nombre -> cuadros
ANIMACIONES_FIJAS = {
    "AnimaRegadera": 4,
    "PalaAnima": 3,
    "PalaAnima2": 5,
    "Planta-Estado2Anima": 3,
    "Nubes": 31,
}
# PlantaN-Estado3/4/5, PlantaN-EstadoFinal y PlantaN-Muerta-EstadoFinal = 3 cuadros
PATRON_PLANTA_ANIMADA = re.compile(r"^Planta\d-(Estado[345]|EstadoFinal|Muerta-EstadoFinal)$")

# Las nubes solo ocupan estas filas del lienzo de 320x572 (se recorta para ahorrar memoria).
NUBES_RECORTE_Y = (40, 170)  # (y_inicio, y_fin)

CARPETAS = {"Escenario Dia": "Dia", "Menu Inicio Plantas": "Menu"}


def limpiar(texto):
    # Los nombres del zip vienen como "Men#U00fa": se convierten a su caracter real
    texto = re.sub(r"#U([0-9a-fA-F]{4})", lambda m: chr(int(m.group(1), 16)), texto)
    texto = unicodedata.normalize("NFKD", texto).encode("ascii", "ignore").decode()
    return texto


def nombre_archivo(carpeta, base):
    base = limpiar(base).replace("_", "-")
    base = base.replace("Plata4y5", "Planta4y5")
    base = "".join(p[:1].upper() + p[1:] for p in base.split())  # "Gota agua" -> "GotaAgua"
    if carpeta == "Menu" and base == "Fondo":
        base = "FondoMenu"
    if base == "EscenarioDiaNubes":
        base = "Nubes"
    return base


def cuadros_de(base):
    if base in ANIMACIONES_FIJAS:
        return ANIMACIONES_FIJAS[base]
    if PATRON_PLANTA_ANIMADA.match(base):
        return 3
    return 1


def main(origen, proyecto):
    sprites = os.path.join(proyecto, "Assets", "Resources", "Sprites")
    originales = os.path.join(proyecto, "Arte_Original")
    for carpeta in (sprites, originales):
        if os.path.isdir(carpeta):
            shutil.rmtree(carpeta)

    total_sueltos = total_cuadros = 0
    for raiz, _, archivos in sorted(os.walk(origen)):
        carpeta_limpia = limpiar(os.path.basename(raiz)).strip()
        if carpeta_limpia not in CARPETAS:
            continue
        destino = CARPETAS[carpeta_limpia]
        for archivo in sorted(archivos):
            if not archivo.lower().endswith(".png"):
                continue
            base = nombre_archivo(destino, os.path.splitext(archivo)[0])
            ruta = os.path.join(raiz, archivo)
            img = Image.open(ruta).convert("RGBA")

            # copia limpia del original
            os.makedirs(os.path.join(originales, destino), exist_ok=True)
            img.save(os.path.join(originales, destino, base + ".png"))

            n = cuadros_de(base)
            if n == 1:
                os.makedirs(os.path.join(sprites, destino), exist_ok=True)
                img.save(os.path.join(sprites, destino, base + ".png"))
                total_sueltos += 1
                continue

            ancho, alto = img.size
            assert ancho % n == 0, f"{base}: {ancho}px no se divide en {n} cuadros"
            w = ancho // n
            salida = os.path.join(sprites, destino, "Anim", base)
            os.makedirs(salida, exist_ok=True)
            for i in range(n):
                cuadro = img.crop((i * w, 0, (i + 1) * w, alto))
                if base == "Nubes":
                    cuadro = cuadro.crop((0, NUBES_RECORTE_Y[0], w, NUBES_RECORTE_Y[1]))
                cuadro.save(os.path.join(salida, f"{base}_{i:02d}.png"))
                total_cuadros += 1

    print(f"Sprites sueltos: {total_sueltos} | cuadros de animacion: {total_cuadros}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print(__doc__)
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])
