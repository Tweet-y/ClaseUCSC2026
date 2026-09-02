# Clon de Asteroids - Evaluación 1 (UCSC 2026-2)

Proyecto de videojuego desarrollado en **Unity**, correspondiente a la primera evaluación de la asignatura de Desarrollo de Videojuegos. Consiste en un clon funcional del clásico arcade **Asteroids**, compuesto por un menú principal y una escena de juego con mecánicas de vuelo espacial, destrucción de asteroides, recolección de ítems y gestión de estado.

---

## 🚀 Integración de Requerimientos

### 📄 Scripting: Manejo de `Start` y `Update`
- **Ciclo de vida y lógica central:** Implementado en scripts modulares de C# (`ControladorNave`, `Asteroide`, `GeneradorAsteroides`, `ControladorJuego`, `ControladorBala`, etc.).
- **`Start()` / `Awake()`:** Inicialización de referencias a componentes (`Rigidbody`, `AudioSource`), obtención de límites de pantalla (`EnvoltorioEspacio`) y configuración inicial de spawn y UI.
- **`Update()` / `FixedUpdate()`:** Lectura continua de inputs por frame, cálculo de rotaciones/disparos en `Update()`, y aplicación de fuerzas físicas/movimiento inercial sobre rigidbodies en `FixedUpdate()`.

---

### 💥 Colisiones: Manejo de Colisiones Rígidas y Triggers vía Código
- **Colisiones Rígidas (`OnCollisionEnter`):**
  - Manejo de impacto físico entre la nave y asteroides, calculando daño al jugador y rebotes físicos según masas e impulsos.
- **Triggers (`OnTriggerEnter`):**
  - Detección de impacto de proyectiles/balas sobre asteroides para destruirlos o dividirlos.
  - Recolección de monedas/ítems de curación (`Moneda`, `ControladorCuracion`).
  - Detección de zonas de envoltura de pantalla (`EnvoltorioEspacio`) para teletransportar entidades al extremo opuesto.

---

### 🎨 Creación y Asignación de Materiales Básicos
- Configuración y asignación de materiales con shaders estándar/URP y paletas de color definidas para:
  - Nave del jugador.
  - Proyectiles y partículas.
  - Variantes de asteroides y elementos coleccionables en escena.

---

### 🔠 UI Básico: Textos en Pantalla y Botón de Inicio
- **Menú Principal (`ControladorMenu`):** Escena con título, créditos y botón interactivo para iniciar la partida o salir.
- **HUD en Juego (`ControladorCanvas` / `ControladorJuego`):** TextMesh Pro en pantalla para visualizar:
  - Puntuación en tiempo real.
  - Vidas / salud del jugador.
  - Pantalla de Game Over con opción de reinicio.

---

### 🔈 Sonido Básico: Efectos Sonoros y Música de Fondo
- **Música de fondo (BGM):** `AudioSource` en loop para la ambientación de la escena de juego.
- **Efectos de sonido (SFX):** Disparos del cañón, explosiones al fragmentar/destruir asteroides, recolección de monedas y daño recibido.

---

### 🎮 Input Básico: Control de la Experiencia
- **Teclado y Ratón:**
  - Control de empuje y rotación de la nave mediante teclado (WASD / Flechas / Espacio).
  - Disparo mediante clic de ratón / barra espaciadora.
- **Nota sobre Gamepad:** 
  > *Por limitaciones de tiempo en la entrega, los controles mediante Gamepad no alcanzaron a ser implementados ni testeados completamente, quedando la experiencia optimizada y validada únicamente para Teclado + Ratón.*

---

## 🛠️ Estructura del Proyecto

```
Assets/
├── Materiales/            # Materiales básicos y shaders
├── Music/                 # Pistas de audio de fondo
├── Prefabs/               # Prefabs de nave, asteroides, proyectiles y UI
├── Scenes/                # Escena de Menú y Escena de Nivel
├── Scripts/               # Lógica de juego en C#
│   ├── Asteroide.cs
│   ├── ControladorBala.cs
│   ├── ControladorCanion.cs
│   ├── ControladorCanvas.cs
│   ├── ControladorCuracion.cs
│   ├── ControladorJuego.cs
│   ├── ControladorMenu.cs
│   ├── ControladorNave.cs
│   ├── EnvoltorioEspacio.cs
│   ├── GeneradorAsteroides.cs
│   └── Moneda.cs
└── TextMesh Pro/          # Fuentes y recursos de UI
```

---

## 👥 Autores

| Nombre | GitHub |
| :--- | :--- |
| Vicente Alarcón | [@vicente-ai](https://github.com/vicente-ai) |
| Benjamín Bizama | [@Tweet-y](https://github.com/Tweet-y) |
| Nicolás Valdebenito | [@NicoValdebenito](https://github.com/NicoValdebenito) |

