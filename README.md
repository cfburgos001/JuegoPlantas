# Juego de plantas (MVP)

Unity 6.3 LTS · 2D · pixel art · tamaño de pantalla 320 x 572 (celular), ventana de PC 480 x 858.

## Instalación (primera vez)
1. Unity Hub → **New project** → plantilla **Universal 2D** → nombre `JuegoPlantas`.
2. Copia en la carpeta del proyecto el contenido de este repositorio (`Assets/`, `Arte_Original/`, `Herramientas/`, `.gitignore`, `.gitattributes`). Si pregunta por combinar carpetas, acepta.
3. Vuelve a Unity y espera a que termine de importar.
4. Menú superior: **Plantas → 1. Construir todo (primera vez)**.
5. En la pestaña **Game**: lista de proporciones → **+** → *Fixed Resolution* `320 x 572`.
6. Abre `Assets/Scenes/Menu` y presiona **Play**.

## Qué hay
| Escena | Qué hace |
|---|---|
| `Menu` | Título, botones Jugar / Salir |
| `SeleccionPlanta` | Elegir 1 de 7 plantas |
| `Dia1` | Cuidar la planta: la pala pone tierra, la regadera da agua → brota → fin del día |

Las escenas se generan con `Assets/Editor/ConstructorEscenas.cs`; el código del juego está en `Assets/Scripts/`.

## Cosas fáciles de cambiar
- **Nombre del juego:** `Assets/Scripts/ConfigJuego.cs` → `TituloJuego`, luego **Plantas → 2. Reconstruir solo las escenas**.
- **Bloquear plantas:** `ConfigJuego.cs` → arreglo `disponibles` (`false` = no se puede elegir).
- **Fuente pixel art:** pon un `.ttf` en `Assets/Fonts/Fuente.ttf` y reconstruye las escenas.

## Arte
- `Arte_Original/` guarda los PNG del diseñador con nombres limpios.
- `Herramientas/preparar_sprites.py` corta las hojas de animación en cuadros sueltos dentro de `Assets/Resources/Sprites/` (requiere Python + `pip install pillow`).
- Pendiente del arte: las plantas 4 y 5 no tienen `Estado2` ni `Estado3-5`; la planta 7 no tiene `Estado5`.
