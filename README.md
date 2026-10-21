# Juego de plantas (MVP)

Juego en Unity con tamaño de pantalla de celular (320 x 572) pensado para correr en Windows.
De día se cuida la planta (tierra, agua, poda); de noche se defiende de los gnomos de jardín con un matamoscas.
Al final se muestra el puntaje del día completo.

**Integrantes:** _(completar)_ · **Materia:** _(completar)_

## Estado

| Parte | Estado |
|---|---|
| Menú de inicio | Hecho |
| Selección de planta (7 plantas) | Hecho |
| Día 1: la planta pide tierra, agua y poda; la planta crece al terminar | Hecho |
| Noche: 10 gnomos de 3 velocidades, matamoscas, vidas y puntaje | Hecho |
| Menú de pausa (botón o tecla Esc) | Hecho |
| Pantalla de resultados con el puntaje total | Hecho |
| Música y efectos de sonido | Hecho (el menú aún no tiene música) |
| Días siguientes (la planta crece cada día) | Pendiente |

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

**Menú:** **Jugar** → elige una planta → **Elegir**.

**Día.** La planta pide cosas con un globo de pensamiento. Arrastra la herramienta correcta (barra de abajo) hasta la maceta:

| Icono del globo | Herramienta |
|---|---|
| Bolsa de tierra | **Pala** |
| Gota de agua | **Regadera** |
| Tijeras | **Tijera** (poda de hojas secas) |

- Los dos primeros pedidos (tierra y agua) no tienen límite de tiempo: sirven para aprender.
- Después la planta ya brotó y pide **5 cuidados más al azar**. Cada uno tiene una barrita de paciencia sobre el globo: si se vacía, ese cuidado se pierde.
- Si usas la herramienta equivocada, el globo se sacude.
- Al terminar, la planta crece y se pasa a la noche.

**Noche.** Salen **10 gnomos** que caminan hacia la planta (por el hueco de la cerca o por los lados). Se eliminan con **clic** del matamoscas.

| Gnomo | Velocidad | Golpes | Puntos |
|---|---|---|---|
| Lento (grande) | 26 px/s | 2 | 100 |
| Normal | 44 px/s | 1 | 150 |
| Rápido (pequeño) | 72 px/s | 1 | 250 |

- De los 10, 3 son lentos, 4 normales y 3 rápidos, en orden al azar.
- La planta tiene **3 vidas**: cada gnomo que la alcanza le quita una. Con 0 vidas los gnomos se la llevan.
- El **puntaje total** del día completo es el de la noche (el máximo posible es 1650).

**Pausa.** Botón de arriba a la derecha o tecla **Esc**: Reanudar, Reiniciar o Menú principal.

## Cómo está hecho el juego (explicado fácil)

**La idea en una frase:** cada pantalla del juego es una *escena* de Unity, y dentro de cada escena todo lo que se ve (fondo, plantas, botones, globos) son **imágenes colocadas sobre un lienzo llamado Canvas**.

### 1. Escena = una pantalla del juego

Es como una diapositiva. Hay cinco: `Menu`, `SeleccionPlanta`, `Dia1`, `EscenaNoche` y `PantallaResultados`. Para pasar de una a otra basta una línea de código:

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
  ├ EfectoHerramienta   animación de la pala, la regadera o la tijera
  ├ GloboPensamiento    globo con el icono de lo que pide la planta (y su barrita de paciencia)
  ├ TextoDia, TextoProgreso, TextoAyuda
  ├ PanelHerramientas
  ├ Regadera, Pala, Tijera   las que se arrastran
  ├ PanelFin            "Día completado" (empieza oculto)
  ├ BotonPausa          arriba a la derecha
  └ PanelPausa          menú de pausa (empieza oculto)
DiaPlanta          objeto vacío que solo guarda el script que manda en el día
MenuPausa          objeto vacío con el script de la pausa
```

La escena `EscenaNoche` se arma igual: `Fondo`, `Planta`, `Gnomos` (aquí aparecen los gnomos mientras se juega), la barra de arriba con puntos, gnomos y corazones, `CapaClics` (recibe los clics del matamoscas), `Matamoscas` (el dibujo que sigue al mouse), `BotonPausa` y `PanelPausa`.

### 7. El cerebro: los scripts

El script `DiaPlanta` necesita hablar con la planta, el globo, etc. Para eso tiene **casillas** que se ven en el Inspector del objeto `DiaPlanta` (Planta, Globo, Zona Uso…). En cada casilla se arrastra el objeto correspondiente de la Hierarchy: el script no "busca" nada, ya tiene conectado lo que necesita.

### 8. Qué pasa cuando se juega el Día 1

1. `DiaPlanta` espera un segundo y muestra el **globo** con el icono de lo que pide la planta.
2. El jugador arrastra una herramienta. `HerramientaArrastrable` crea una copia que sigue al mouse.
3. Si la suelta dentro de `ZonaUso`, avisa a `DiaPlanta` qué herramienta se usó.
4. Si es la correcta, suena el efecto, se reproduce la animación y la planta rebota. Si no, el globo se sacude.
5. Se repite con los pedidos extra (al azar) y, al terminar, la planta crece y aparece el `PanelFin`.

### 8b. Qué pasa en la noche

1. `EscenaNoche` muestra «¡Se acercan los gnomos!» y, a los pocos segundos, empieza a crear gnomos uno por uno.
2. Cada gnomo (`EnemigoNoche`) camina en línea recta hacia la planta a su propia velocidad.
3. El matamoscas (`Matamoscas`) sigue al mouse. Al hacer clic, `EscenaNoche.Golpear` busca el gnomo que esté debajo y le quita un golpe.
4. Cuando un gnomo muere suma sus puntos en `GameData`. Si llega a la planta, le quita una vida.
5. La noche termina cuando ya no quedan gnomos o cuando la planta se queda sin vidas, y se pasa a `PantallaResultados`.

### 8c. Los datos que viajan entre escenas

`GameData` es una clase estática (no vive en ninguna escena): guarda clics, cuidados atendidos, gnomos eliminados, vidas y puntaje. Así `PantallaResultados` puede mostrar lo que pasó en las escenas anteriores.

### 8d. El sonido

- **Música de fondo:** la maneja `AdmAudio` (un objeto que vive en la escena `Menu` y no se destruye al cambiar de escena). Cada escena le pide su música.
- **Efectos:** `Sonidos.Reproducir("Nombre del archivo")` desde cualquier script. No importan las mayúsculas ni las tildes. Los efectos se guardan todos en `Assets/Resources/Sonidos.prefab`, que se genera con el menú **Plantas → 1** o **2**.

### 9. Cómo ver todo esto en la ventana Scene

Si **Scene** se ve vacía es normal: el Canvas es un rectángulo enorme comparado con la cámara. Haz clic en `Canvas` en la Hierarchy, pasa el mouse sobre Scene y presiona la tecla **F**. Activa también el botón **2D** de la barra de Scene.

> Las escenas se armaron la primera vez con un script del editor (`Assets/Editor/ConstructorEscenas.cs`) para ahorrar trabajo manual. Hace lo mismo que se haría a mano: crear el Canvas, poner las imágenes y pegar los scripts.

## Estructura del proyecto

```
Assets/
  Scenes/                   Menu, SeleccionPlanta, Dia1, EscenaNoche, PantallaResultados
  Scripts/                  Código del juego
  Editor/                   Herramienta que arma Dia1 y EscenaNoche (ver aviso más abajo)
  Audio/
    Música de fondo/        Música del día y de la noche
    Efectos de Sonido/      Efectos de menú, día, noche y pausa
  Resources/
    Sonidos.prefab          Lista de efectos (lo genera el menú Plantas)
    Sprites/
      Menu/                 Fondo, botón y las 7 plantas del menú
      Dia/                  Escenario, plantas, herramientas, globos
      Dia/Anim/<Nombre>/    Animaciones: un PNG por cuadro (_00, _01, ...)
      Noche/                Fondo de la noche y corazones
      Noche/Anim/<Nombre>/  Gnomo caminando, golpeado y muriendo; matamoscas
      Pausa/Anim/<Nombre>/  Letrero «PAUSA» y botón de pausa
Arte_Original/             PNG originales del diseñador (y los .pxo de la noche)
Herramientas/              Scripts de Python para cortar hojas de animación
```

### Scripts

| Script | Para qué sirve |
|---|---|
| `ConfigJuego.cs` | Título, planta elegida, día actual y qué plantas se pueden elegir |
| `GameData.cs` | Datos que pasan de una escena a otra: clics, cuidados, gnomos, vidas y puntaje |
| `Cargador.cs` | Carga sprites y animaciones desde `Resources/Sprites` |
| `AnimadorSprites.cs` | Reproduce una animación cambiando el sprite cuadro por cuadro |
| `EntradaMouse.cs` | Lee el mouse y la tecla Esc con el Input System nuevo o el viejo |
| `AdmAudio.cs` | Música de fondo (repite en bucle y baja el volumen en la pausa) |
| `Sonidos.cs` | Efectos de sonido: `Sonidos.Reproducir("nombre")` |
| `MenuPrincipal.cs` | Botones Jugar y Salir |
| `SeleccionPlantas.cs` | Pantalla para elegir la planta |
| `DiaPlanta.cs` | Lógica del día: globo, peticiones al azar, tiempo límite, animaciones y fin del día |
| `HerramientaArrastrable.cs` | Arrastrar herramientas hasta la planta |
| `EscenaNoche.cs` | Lógica de la noche: crea los 10 gnomos, vidas, banner y final |
| `EnemigoNoche.cs` | Un gnomo: velocidad, puntos, golpes, animaciones |
| `Matamoscas.cs` | El matamoscas que sigue al mouse y golpea al hacer clic |
| `MenuPausa.cs` | Pausa (Time.timeScale = 0), Reanudar, Reiniciar y Menú principal |
| `PantallaResultados.cs` | Resumen final y puntaje total (la escena la arma el menú Plantas) |

## Cómo seguir agregando cosas

**Agregar un sprite nuevo**
1. Ponlo en `Assets/Resources/Sprites/...` (nombres sin espacios ni tildes).
2. Menú **Plantas → 2. Solo configurar sprites y audio**. Esta opción es segura, se puede ejecutar cuando quieras: deja la imagen como pixel art nítido y regenera la lista de efectos de sonido.
3. Cárgalo desde código con `Cargador.CargarSprite("Dia/NombreDelSprite")`.

**Agregar una animación nueva**
1. Si el diseñador entrega una hoja con varios cuadros, ponla en `Arte_Original/` y agrega su nombre y número de cuadros en `Herramientas/preparar_sprites.py` (día y menú) o `Herramientas/preparar_noche_y_audio.py` (noche y pausa). Esos scripts requieren Python y `pip install pillow`.
2. Si ya son cuadros sueltos, déjalos en una carpeta `Assets/Resources/Sprites/Dia/Anim/<Nombre>/` con nombres `Nombre_00.png`, `Nombre_01.png`, etc.
3. Reprodúcela con `Cargador.CargarAnimacion("Dia/Anim/<Nombre>")` y un `AnimadorSprites`.

**Agregar un efecto de sonido nuevo**
1. Copia el MP3 dentro de `Assets/Audio/Efectos de Sonido/` (desde la ventana Project de Unity).
2. Menú **Plantas → 2. Solo configurar sprites y audio** (vuelve a generar `Sonidos.prefab`).
3. Úsalo con `Sonidos.Reproducir("Nombre del archivo")`. Si el efecto trae mucho silencio al final, puedes cortarlo: `Sonidos.Reproducir("Nombre", 1f, 2f)` (volumen 1, se corta a los 2 segundos).

**Cambiar la dificultad de la noche**
- Velocidad, golpes, puntos y tamaño de cada tipo de gnomo: constantes al inicio de `EnemigoNoche.cs`.
- Cuántos gnomos salen: `GnomosPorNoche` en `GameData.cs`.
- Vidas de la planta: `VidasMaximas` en `GameData.cs`.
- Tiempo entre un gnomo y otro: `Intervalo Minimo` / `Intervalo Maximo` en el Inspector del objeto `EscenaNoche`.

**Cambiar la dificultad del día**
En el Inspector del objeto `DiaPlanta`: `Peticiones Extra` (cuántos cuidados al azar) y `Tiempo Por Peticion` (segundos de paciencia).

**Agregar una escena nueva**
1. **File → New Scene**, guárdala en `Assets/Scenes/`.
2. Regístrala: **File → Build Profiles → Scene List → Add Open Scenes**.
3. Entra a ella con `SceneManager.LoadScene("NombreDeLaEscena")` (el nombre debe estar escrito igual).
4. Para armar la interfaz, copia la estructura de `Dia1`: `Canvas` → `Pantalla` (320 x 572) → objetos. Todo se mide desde la esquina superior izquierda de `Pantalla`.

**Agregar un script nuevo**
Clic derecho en `Assets/Scripts` → **Create → MonoBehaviour Script**. Arrástralo al objeto que lo va a usar.

**Para el día 2 en adelante:** los sprites de crecimiento de cada planta (`Planta<N>-Estado3`, `Estado4`, `Estado5`, `EstadoFinal` y `Muerta-EstadoFinal`) ya están cortados en `Assets/Resources/Sprites/Dia/Anim/`. El `Estado3` ya se usa al final del día 1 y en la noche (`Cargador.CargarCrecimiento`); el resto queda para los días siguientes.

### Aviso: el menú «Plantas → 1» reconstruye Dia1, EscenaNoche y PantallaResultados

`Assets/Editor/ConstructorEscenas.cs` generó esas tres escenas. La opción **1. Actualizar Día y crear Noche** **vuelve a crearlas desde cero** y borra los cambios hechos a mano en ellas. Úsenla solo para regenerarlas (por ejemplo, si cambian el arte o los scripts) y avisen antes al equipo. La opción **2** (sprites y audio) sí es segura. `Menu` y `SeleccionPlanta` nunca se tocan, salvo con **Avanzado → Reconstruir Menú y SeleccionPlanta**, que reemplaza `Menu` y se lleva consigo el objeto `AdmAudio`.

### Al dar Play siempre empieza en el Menú

`Assets/Editor/InicioDesdeMenu.cs` hace que **Play** arranque siempre en la escena `Menu`, sin importar cuál esté abierta (así el orden es Menú → Selección → Día → Noche → Resultados). Para probar una sola escena, apaguen la opción en el menú **Plantas → Al dar Play, empezar siempre desde el Menú**.

## Trabajo en equipo con Git

- Antes de empezar: `git pull`.
- Al terminar una parte: `git add .` → `git commit -m "mensaje corto de lo que se hizo"` → `git push`.
- **Guarden en Unity antes del commit** (`Ctrl + S` y **File → Save Project**).
- **Las escenas (`.unity`) no se mezclan bien.** Solo una persona edita una escena a la vez; avisen en el grupo cuál están tocando.
- Para trabajar sin pisarse, cada quien puede usar su rama: `git checkout -b nombre-de-la-rama` y luego un Pull Request en GitHub.
- **Los archivos `.meta` sí se suben** (Unity los necesita). La carpeta `Library/` no.
- Mover, renombrar o borrar archivos siempre **desde la ventana Project de Unity**, no desde el Explorador, para que el `.meta` viaje con el archivo.
- Ignoren la barra "Unity VCS" de arriba: el control de versiones es Git.

## Pendiente

- Las plantas 4 y 5 solo tienen `Estado1` y el estado final: en la noche se muestra el brote del día 1 en vez de la planta crecida. La planta 7 no tiene `Estado5`.
- La hoja `Pausa_Menu.png` trae una tercera fila (un gnomo que se asoma) que todavía no se usa.
- El menú de inicio y la selección de planta aún no tienen música: en el objeto `AdmAudio` de la escena `Menu`, la casilla **Musica Menu** debe quedar vacía o con una canción real (no un efecto de sonido).
- Los dibujos de `PlantaN-EstadoFinal` y `PlantaN-Muerta-EstadoFinal` todavía no se usan (servirán para los días siguientes).
- Días 2 en adelante: más gnomos por noche y nuevos tipos (la hoja `Enemy_1` es solo el primer enemigo).
