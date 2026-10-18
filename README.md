# Juego de plantas (MVP)

Juego en Unity con tamaño de pantalla de celular (320 x 572) pensado para correr en Windows.
De día se cuida la planta (tierra, agua); de noche se defiende de los nomos de jardín.

**Integrantes:** _(completar)_ · **Materia:** _(completar)_

## Estado

| Parte | Estado |
|---|---|
| Menú de inicio | Hecho |
| Selección de planta (7 plantas) | Hecho |
| Día 1: tierra con la pala, agua con la regadera, la planta brota | Hecho |
| Días siguientes (la planta crece cada día) | Pendiente |
| Noche (point and click, nomos en oleadas) | Pendiente |

## Requisitos

Esto es lo único que se instala, una sola vez por computadora:

- **Unity Hub** y, desde el Hub, el editor **Unity 6.3 LTS, versión exacta `6000.3.10f1`**. Con otra versión Unity intenta actualizar el proyecto y cambia archivos.
- **Git**, para clonar y subir cambios.
- Visual Studio (opcional, para editar los scripts).

**No hace falta:** crear un proyecto nuevo, copiar archivos a mano, ejecutar el menú **Plantas** ni instalar paquetes. Unity descarga solo los paquetes que necesita la primera vez que abre el proyecto (se necesita internet).

## Cómo abrir el proyecto

1. Clona el repositorio en una carpeta corta, fuera de OneDrive y del Escritorio. Ejemplo:
   ```
   git clone <URL-DEL-REPOSITORIO> C:\Proyectos\JuegoPlantas
   ```
2. Abre **Unity Hub** → **Proyectos** → **Añadir** → **Añadir proyecto desde el disco** y elige la carpeta clonada (la que contiene `Assets`, `Packages` y `ProjectSettings`).
3. Si el Hub avisa que falta el editor, instala `6000.3.10f1` cuando te lo ofrezca.
4. Abre el proyecto. La primera vez tarda varios minutos porque Unity genera la carpeta `Library` (no se sube a Git).
5. En la ventana **Project**, abre `Assets/Scenes/Menu` con doble clic.
6. En la pestaña **Game**, abre la lista de proporciones, pulsa **+** y crea una *Fixed Resolution* de `320 x 572`.
7. Pulsa **Play**.

No hace falta ejecutar nada del menú **Plantas**: las escenas ya vienen en el repositorio.

## Cómo se juega

1. **Jugar** → elige una planta → **Elegir**.
2. En el Día 1 aparece un globo de pensamiento con lo que pide la planta.
3. Arrastra la herramienta correcta (barra de abajo) hasta la maceta: bolsa de tierra → **Pala**, gota → **Regadera**.
4. Si la herramienta es la equivocada, el globo se sacude.

## Cómo está hecho el juego (explicado fácil)

**La idea en una frase:** cada pantalla del juego es una *escena* de Unity, y dentro de cada escena todo lo que se ve (fondo, plantas, botones, globos) son **imágenes colocadas sobre un lienzo llamado Canvas**.

### 1. Escena = una pantalla del juego

Es como una diapositiva. Hay tres: `Menu`, `SeleccionPlanta` y `Dia1`. Para pasar de una a otra basta una línea de código:

```csharp
SceneManager.LoadScene("SeleccionPlanta");
```

### 2. Canvas = el lienzo donde se dibuja todo

Imagina un vidrio transparente pegado a la pantalla de la computadora: lo que pongas en ese vidrio se ve siempre al frente y ocupa toda la ventana. Eso es el **Canvas**.

En este juego **todo está en el Canvas**. No usamos objetos "del mundo 2D", solo imágenes de interfaz. Elegimos esto porque:

- los botones y el arrastrar-y-soltar funcionan casi sin código extra;
- el juego se ajusta solo al tamaño de la ventana.

Dentro del Canvas hay dos piezas importantes:

- **Canvas Scaler:** el arte está hecho para 320 × 572 píxeles. El Scaler lo agranda o lo achica para que quepa completo en la ventana (con barras negras si la ventana tiene otra forma).
- **Pantalla:** un recuadro de 320 × 572 que contiene todo lo demás. Es el "tamaño del arte".

### 3. Los objetos y sus piezas

Cada cosa que ves es un objeto de la **Hierarchy** al que se le pegan piezas (componentes):

| Pieza | Para qué sirve |
|---|---|
| **Image** | Muestra un sprite: fondo, planta, herramienta, globo |
| **Button** | Hace que se pueda hacer clic |
| **Text** | Muestra texto |
| **Script** | Le da comportamiento (nuestro código) |

### 4. Dónde está cada cosa en pantalla

Selecciona un objeto y mira su **Rect Transform** en el Inspector:

- **Pos X / Pos Y:** posición medida desde la **esquina superior izquierda** de `Pantalla`. Un Pos Y negativo significa "hacia abajo".
- **Width / Height:** tamaño en píxeles del arte.
- **Pivot:** el punto del objeto que se coloca en esa posición. La maceta usa "abajo al centro" para que su base quede justo sobre la mancha oscura del pasto.

### 5. Orden de las capas

Lo que está **más arriba** en la Hierarchy se dibuja primero (queda atrás). Lo que está **más abajo** se dibuja encima. Por eso `Fondo` es el primero de la lista y `PanelFin` es el último.

### 6. Así se ve la escena `Dia1` por dentro

```
Main Camera        fondo negro; solo existe para que Unity dibuje algo
EventSystem        el que detecta los clics y los arrastres
Canvas             el lienzo
└ Pantalla         recuadro de 320 x 572 con todo adentro
  ├ Fondo               imagen del jardín
  ├ Nubes               imagen que cambia de cuadro sola
  ├ ZonaUso             área invisible donde se sueltan las herramientas
  ├ Planta              la maceta (cambia de sprite al crecer)
  ├ EfectoHerramienta   animación de la pala o de la regadera
  ├ GloboPensamiento    globo con el icono de lo que pide la planta
  ├ TextoDia, TextoAyuda
  ├ PanelHerramientas
  ├ Regadera, Pala, Tijera   las que se arrastran
  └ PanelFin            "Día completado" (empieza oculto)
DiaPlanta          objeto vacío que solo guarda el script que manda en el día
```

### 7. El cerebro: los scripts

El script `DiaPlanta` necesita hablar con la planta, el globo, etc. Para eso tiene **casillas** que se ven en el Inspector del objeto `DiaPlanta` (Planta, Globo, Zona Uso…). En cada casilla se arrastra el objeto correspondiente de la Hierarchy: el script no "busca" nada, ya tiene conectado lo que necesita.

### 8. Qué pasa cuando se juega el Día 1

1. `DiaPlanta` espera un segundo y muestra el **globo** con el icono de lo que pide la planta.
2. El jugador arrastra una herramienta. `HerramientaArrastrable` crea una copia que sigue al mouse.
3. Si la suelta dentro de `ZonaUso`, avisa a `DiaPlanta` qué herramienta se usó.
4. Si es la correcta, se reproduce su animación y la planta cambia de sprite. Si no, el globo se sacude.
5. Se repite con el segundo pedido. Al terminar aparece el `PanelFin`.

### 9. Cómo ver todo esto en la ventana Scene

Si **Scene** se ve vacía es normal: el Canvas es un rectángulo enorme comparado con la cámara. Haz clic en `Canvas` en la Hierarchy, pasa el mouse sobre Scene y presiona la tecla **F**. Activa también el botón **2D** de la barra de Scene.

> Las escenas se armaron la primera vez con un script del editor (`Assets/Editor/ConstructorEscenas.cs`) para ahorrar trabajo manual. Hace lo mismo que se haría a mano: crear el Canvas, poner las imágenes y pegar los scripts.

## Estructura del proyecto

```
Assets/
  Scenes/                   Menu, SeleccionPlanta, Dia1
  Scripts/                  Código del juego
  Editor/                   Herramienta que generó las escenas (ver aviso más abajo)
  Resources/Sprites/
    Menu/                   Fondo, botón y las 7 plantas del menú
    Dia/                    Escenario, plantas, herramientas, globos
    Dia/Anim/<Nombre>/      Animaciones: un PNG por cuadro (_00, _01, ...)
Arte_Original/              PNG originales del diseñador, con nombres limpios
Herramientas/               Script de Python para cortar hojas de animación
```

### Scripts

| Script | Para qué sirve |
|---|---|
| `ConfigJuego.cs` | Título, planta elegida, día actual y qué plantas se pueden elegir |
| `Cargador.cs` | Carga sprites y animaciones desde `Resources/Sprites` |
| `AnimadorSprites.cs` | Reproduce una animación cambiando el sprite cuadro por cuadro |
| `MenuPrincipal.cs` | Botones Jugar y Salir |
| `SeleccionPlantas.cs` | Pantalla para elegir la planta |
| `DiaPlanta.cs` | Lógica del día: globo, necesidades, animaciones y fin del día |
| `HerramientaArrastrable.cs` | Arrastrar herramientas hasta la planta |

## Cómo seguir agregando cosas

**Agregar un sprite nuevo**
1. Ponlo en `Assets/Resources/Sprites/...` (nombres sin espacios ni tildes).
2. Menú **Plantas → 3. Solo configurar sprites**. Esta opción es segura, se puede ejecutar cuando quieras: deja la imagen como pixel art nítido.
3. Cárgalo desde código con `Cargador.CargarSprite("Dia/NombreDelSprite")`.

**Agregar una animación nueva**
1. Si el diseñador entrega una hoja con varios cuadros, ponla en `Arte_Original/` y agrega su nombre y número de cuadros en `Herramientas/preparar_sprites.py`. Ese script requiere Python y `pip install pillow`.
2. Si ya son cuadros sueltos, déjalos en una carpeta `Assets/Resources/Sprites/Dia/Anim/<Nombre>/` con nombres `Nombre_00.png`, `Nombre_01.png`, etc.
3. Reprodúcela con `Cargador.CargarAnimacion("Dia/Anim/<Nombre>")` y un `AnimadorSprites`.

**Agregar una escena nueva (por ejemplo la noche)**
1. **File → New Scene**, guárdala en `Assets/Scenes/`.
2. Regístrala: **File → Build Profiles → Scene List → Add Open Scenes**.
3. Entra a ella con `SceneManager.LoadScene("NombreDeLaEscena")` (el nombre debe estar escrito igual).
4. Para armar la interfaz, copia la estructura de `Dia1`: `Canvas` → `Pantalla` (320 x 572) → objetos. Todo se mide desde la esquina superior izquierda de `Pantalla`.

**Agregar un script nuevo**
Clic derecho en `Assets/Scripts` → **Create → MonoBehaviour Script**. Arrástralo al objeto que lo va a usar.

**Para el día 2 en adelante:** los sprites de crecimiento de cada planta (`Planta<N>-Estado3`, `Estado4`, `Estado5`, `EstadoFinal` y `Muerta-EstadoFinal`) ya están cortados en `Assets/Resources/Sprites/Dia/Anim/`.

### Aviso: no vuelvan a ejecutar el menú «Plantas → 1» ni «2»

`Assets/Editor/ConstructorEscenas.cs` generó las escenas la primera vez. Sus opciones **1** y **2** **vuelven a crear las escenas desde cero** y borran cualquier cambio hecho a mano en el editor. Desde que alguien modifique las escenas, esas dos opciones ya no se usan. La opción **3** sí es segura.

## Trabajo en equipo con Git

- Antes de empezar: `git pull`.
- Al terminar una parte: `git add .` → `git commit -m "mensaje corto de lo que se hizo"` → `git push`.
- **Guarden en Unity antes del commit** (`Ctrl + S` y **File → Save Project**).
- **Las escenas (`.unity`) no se mezclan bien.** Solo una persona edita una escena a la vez; avisen en el grupo cuál están tocando.
- Para trabajar sin pisarse, cada quien puede usar su rama: `git checkout -b nombre-de-la-rama` y luego un Pull Request en GitHub.
- **Los archivos `.meta` sí se suben** (Unity los necesita). La carpeta `Library/` no.
- Mover, renombrar o borrar archivos siempre **desde la ventana Project de Unity**, no desde el Explorador, para que el `.meta` viaje con el archivo.
- Ignoren la barra "Unity VCS" de arriba: el control de versiones es Git.

## Pendiente del arte

- Las plantas 4 y 5 no tienen `Estado2` hasta `Estado5`, solo `Estado1` y el estado final. La planta 7 no tiene `Estado5`.
- No hay sprites de nomos, ni de la noche con la planta en el centro.
