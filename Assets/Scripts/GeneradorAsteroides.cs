using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Generador autónomo de asteroides en los bordes del mapa:
/// - Usa los modelos Large solicitados (SM_Env_Asteroid_Large_01 y SM_Env_Asteroid_Large_02 con variaciones).
/// - Escala reducida y configurable para que tengan un tamaño adecuado para el juego.
/// - Generación exclusiva en los 4 bordes del mapa (Superior, Inferior, Izquierdo, Derecho).
/// - Trayectorias cruzadas hacia el área central de juego.
/// </summary>
public class GeneradorAsteroides : MonoBehaviour
{
    [System.Serializable]
    public class TipoAsteroideConfig
    {
        public string nombre = "Tipo";
        public Asteroide.TipoDeAsteroide tipo;
        public GameObject prefabBase;
        [Tooltip("Multiplicador de escala específico para este tipo")]
        public float escalaRelativa = 1f;
        public float velocidadMin = 2.5f;
        public float velocidadMax = 4.5f;
        [Range(1, 10)]
        public int probabilidad = 4;
    }

    [Header("Escala y Velocidad Global")]
    [Tooltip("Ajusta el tamaño general de todos los asteroides")]
    [Range(1f, 30f)]
    public float escalaGeneral = 5.0f;
    [Tooltip("Multiplicador global para acelerar o frenar todos los asteroides")]
    [Range(1f, 20f)]
    public float multiplicadorVelocidadGlobal = 2.0f;

    [Header("3 Tipos de Asteroides (Modelos Large)")]
    public TipoAsteroideConfig tipo1_Large01 = new TipoAsteroideConfig
    {
        nombre = "1. Asteroide Grande 01 (Estándar)",
        tipo = Asteroide.TipoDeAsteroide.RocosoEstandar,
        escalaRelativa = 5.0f,
        velocidadMin = 250.0f,
        velocidadMax = 400.0f,
        probabilidad = 4
    };

    public TipoAsteroideConfig tipo2_Large02 = new TipoAsteroideConfig
    {
        nombre = "2. Asteroide Grande 02 (Angular/Rápido)",
        tipo = Asteroide.TipoDeAsteroide.CrateresRapido,
        escalaRelativa = 4.0f,
        velocidadMin = 350.0f,
        velocidadMax = 550.0f,
        probabilidad = 4
    };

    public TipoAsteroideConfig tipo3_LargeHoles = new TipoAsteroideConfig
    {
        nombre = "3. Asteroide Grande Agujeros/Pesado",
        tipo = Asteroide.TipoDeAsteroide.GigantePesado,
        escalaRelativa = 6.0f,
        velocidadMin = 180.0f,
        velocidadMax = 300.0f,
        probabilidad = 3
    };

    [Header("Configuración de Escenario")]
    public Asteroide.PlanoDeJuego plano = Asteroide.PlanoDeJuego.XZ;
    [Tooltip("Objeto plano del escenario. Si está vacío se detecta automáticamente")]
    public Transform planoEspacio;

    [Header("Límites del Mapa")]
    public float limiteMinX = -20f;
    public float limiteMaxX = 20f;
    public float limiteMinZ_Y = -12f;
    public float limiteMaxZ_Y = 12f;
    public float alturaFija = 0f;
    public float margenBordeSpawn = 30.0f;

    [Header("Frecuencia y Cantidad")]
    [Tooltip("Cantidad de asteroides que entran desde los bordes al iniciar")]
    public int oleadaInicial = 4;
    public bool generacionContinua = true;
    public float intervaloSpawn = 3.0f;
    public int maxAsteroidesSimultaneos = 8;

    private float _temporizador = 0f;
    private Camera _cam;

    void Reset()
    {
        AutoCargarPrefabs();
        AutoDetectarPlano();
    }

    void OnValidate()
    {
        if (tipo1_Large01.prefabBase == null || tipo2_Large02.prefabBase == null || tipo3_LargeHoles.prefabBase == null)
        {
            AutoCargarPrefabs();
        }
        if (planoEspacio == null)
        {
            AutoDetectarPlano();
        }
    }

    void Awake()
    {
        AutoCargarPrefabs();
        AutoDetectarPlano();
    }

    void Start()
    {
        _cam = Camera.main;
        if (_cam == null) _cam = FindFirstObjectByType<Camera>();

        CalcularLimitesDelMapa();

        // 1. Inicializar asteroides ya colocados en la escena si los hay
        InicializarAsteroidesEnEscena();

        // 2. Generar oleada inicial DESDE LOS BORDES
        for (int i = 0; i < oleadaInicial; i++)
        {
            SpawnAsteroideEnBorde();
        }
    }

    void Update()
    {
        if (!generacionContinua) return;

        _temporizador += Time.deltaTime;
        if (_temporizador >= intervaloSpawn)
        {
            _temporizador = 0f;
            if (ContarAsteroidesActivos() < maxAsteroidesSimultaneos)
            {
                SpawnAsteroideEnBorde();
            }
        }
    }

    public void AutoCargarPrefabs()
    {
#if UNITY_EDITOR
        if (tipo1_Large01.prefabBase == null)
        {
            tipo1_Large01.prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolygonSciFiSpace/Prefabs/Environment/SM_Env_Asteroid_Large_01.prefab");
            if (tipo1_Large01.prefabBase == null)
                tipo1_Large01.prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolygonSciFiSpace/Models/SM_Env_Astroid_Large_01.fbx");
        }

        if (tipo2_Large02.prefabBase == null)
        {
            tipo2_Large02.prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolygonSciFiSpace/Prefabs/Environment/SM_Env_Asteroid_Large_02.prefab");
            if (tipo2_Large02.prefabBase == null)
                tipo2_Large02.prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolygonSciFiSpace/Models/SM_Env_Astroid_Large_02.fbx");
        }

        if (tipo3_LargeHoles.prefabBase == null)
        {
            tipo3_LargeHoles.prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolygonSciFiSpace/Prefabs/Environment/SM_Env_Asteroid_Large_Holes_01.prefab");
            if (tipo3_LargeHoles.prefabBase == null)
                tipo3_LargeHoles.prefabBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PolygonSciFiSpace/Prefabs/Environment/SM_Env_Asteroid_Large_Holes_02.prefab");
            if (tipo3_LargeHoles.prefabBase == null)
                tipo3_LargeHoles.prefabBase = tipo1_Large01.prefabBase;
        }
#endif
    }

    public void AutoDetectarPlano()
    {
        if (planoEspacio != null) return;

        GameObject[] todos = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in todos)
        {
            string n = obj.name.ToLower();
            if (n.Contains("plane") || n.Contains("plano") || n.Contains("espacio") || n.Contains("suelo") || n.Contains("floor"))
            {
                planoEspacio = obj.transform;
                break;
            }
        }
    }

    private void CalcularLimitesDelMapa()
    {
        if (planoEspacio != null)
        {
            Renderer rend = planoEspacio.GetComponent<Renderer>();
            if (rend != null)
            {
                limiteMinX = rend.bounds.min.x;
                limiteMaxX = rend.bounds.max.x;
                if (plano == Asteroide.PlanoDeJuego.XZ)
                {
                    limiteMinZ_Y = rend.bounds.min.z;
                    limiteMaxZ_Y = rend.bounds.max.z;
                    alturaFija = rend.bounds.center.y;
                }
                else
                {
                    limiteMinZ_Y = rend.bounds.min.y;
                    limiteMaxZ_Y = rend.bounds.max.y;
                    alturaFija = rend.bounds.center.z;
                }
                return;
            }
        }

        // Si no hay plano asignado, calcular a través de la cámara
        if (_cam != null)
        {
            float dist = Mathf.Abs(_cam.transform.position.y);
            if (dist < 5f) dist = 20f;
            float alto = Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist;
            float ancho = alto * _cam.aspect;

            limiteMinX = _cam.transform.position.x - ancho;
            limiteMaxX = _cam.transform.position.x + ancho;

            if (plano == Asteroide.PlanoDeJuego.XZ)
            {
                limiteMinZ_Y = _cam.transform.position.z - alto;
                limiteMaxZ_Y = _cam.transform.position.z + alto;
                alturaFija = 0f;
            }
            else
            {
                limiteMinZ_Y = _cam.transform.position.y - alto;
                limiteMaxZ_Y = _cam.transform.position.y + alto;
                alturaFija = 0f;
            }
        }
    }

    /// <summary>
    /// Genera un asteroide exactamente en uno de los 4 bordes exteriores del mapa
    /// y lo envía con trayectoria hacia el área de juego.
    /// </summary>
    public void SpawnAsteroideEnBorde()
    {
        TipoAsteroideConfig config = SeleccionarTipoAleatorio();
        if (config.prefabBase == null) return;

        // Elegir aleatoriamente uno de los 4 bordes: 0=Arriba, 1=Abajo, 2=Izquierda, 3=Derecha
        int borde = Random.Range(0, 4);
        Vector3 posicionSpawn = Vector3.zero;

        float posX = 0f;
        float posZY = 0f;

        switch (borde)
        {
            case 0: // Borde Superior
                posX = Random.Range(limiteMinX, limiteMaxX);
                posZY = limiteMaxZ_Y + margenBordeSpawn;
                break;
            case 1: // Borde Inferior
                posX = Random.Range(limiteMinX, limiteMaxX);
                posZY = limiteMinZ_Y - margenBordeSpawn;
                break;
            case 2: // Borde Izquierdo
                posX = limiteMinX - margenBordeSpawn;
                posZY = Random.Range(limiteMinZ_Y, limiteMaxZ_Y);
                break;
            case 3: // Borde Derecho
                posX = limiteMaxX + margenBordeSpawn;
                posZY = Random.Range(limiteMinZ_Y, limiteMaxZ_Y);
                break;
        }

        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            posicionSpawn = new Vector3(posX, alturaFija, posZY);
        }
        else
        {
            posicionSpawn = new Vector3(posX, posZY, alturaFija);
        }

        // Punto objetivo dentro del área central del mapa
        float margenInterior = 0.5f;
        Vector3 puntoObjetivo;
        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            puntoObjetivo = new Vector3(
                Random.Range(limiteMinX * margenInterior, limiteMaxX * margenInterior),
                alturaFija,
                Random.Range(limiteMinZ_Y * margenInterior, limiteMaxZ_Y * margenInterior)
            );
        }
        else
        {
            puntoObjetivo = new Vector3(
                Random.Range(limiteMinX * margenInterior, limiteMaxX * margenInterior),
                Random.Range(limiteMinZ_Y * margenInterior, limiteMaxZ_Y * margenInterior),
                alturaFija
            );
        }

        Vector3 direccion = (puntoObjetivo - posicionSpawn).normalized;

        // Instanciar
        GameObject nuevoAsteroide = Instantiate(config.prefabBase, posicionSpawn, Random.rotation);

        // Aplicar escala adecuada (no gigantes)
        Vector3 escalaCalculada = Vector3.one * (escalaGeneral * config.escalaRelativa);
        nuevoAsteroide.transform.localScale = escalaCalculada;

        // Configurar componente Asteroide
        Asteroide ast = nuevoAsteroide.GetComponent<Asteroide>();
        if (ast == null)
        {
            ast = nuevoAsteroide.AddComponent<Asteroide>();
        }

        ast.tipo = config.tipo;
        ast.plano = plano;
        ast.nivelTamanio = 3; // Grande inicial
        ast.velocidadMin = config.velocidadMin;
        ast.velocidadMax = config.velocidadMax;
        ast.AplicarPropiedadesSegunTipo();

        float vel = Random.Range(config.velocidadMin, config.velocidadMax) * multiplicadorVelocidadGlobal;
        ast.InicializarMovimiento(direccion, vel);

        // Configurar componente de envoltorio con los límites exactos
        EnvoltorioEspacio wrap = nuevoAsteroide.GetComponent<EnvoltorioEspacio>();
        if (wrap == null)
        {
            wrap = nuevoAsteroide.AddComponent<EnvoltorioEspacio>();
        }
        wrap.plano = plano;
        wrap.modo = EnvoltorioEspacio.ModoLimites.LimitesManuales;
        wrap.limiteMinX = limiteMinX;
        wrap.limiteMaxX = limiteMaxX;
        wrap.limiteMinZ_Y = limiteMinZ_Y;
        wrap.limiteMaxZ_Y = limiteMaxZ_Y;
        wrap.margen = margenBordeSpawn;
    }

    private TipoAsteroideConfig SeleccionarTipoAleatorio()
    {
        int totalPeso = tipo1_Large01.probabilidad + tipo2_Large02.probabilidad + tipo3_LargeHoles.probabilidad;
        int tiro = Random.Range(0, totalPeso);

        if (tiro < tipo1_Large01.probabilidad)
            return tipo1_Large01;
        else if (tiro < tipo1_Large01.probabilidad + tipo2_Large02.probabilidad)
            return tipo2_Large02;
        else
            return tipo3_LargeHoles;
    }

    private void InicializarAsteroidesEnEscena()
    {
        Asteroide[] existentes = FindObjectsByType<Asteroide>(FindObjectsSortMode.None);
        foreach (var ast in existentes)
        {
            // Ajustar escala si es un asteroide original gigante puesto a mano
            if (ast.transform.localScale.x > 0.5f)
            {
                ast.transform.localScale = Vector3.one * escalaGeneral;
            }

            ast.plano = plano;
            EnvoltorioEspacio wrap = ast.GetComponent<EnvoltorioEspacio>();
            if (wrap == null)
            {
                wrap = ast.gameObject.AddComponent<EnvoltorioEspacio>();
            }
            wrap.plano = plano;
            wrap.modo = EnvoltorioEspacio.ModoLimites.LimitesManuales;
            wrap.limiteMinX = limiteMinX;
            wrap.limiteMaxX = limiteMaxX;
            wrap.limiteMinZ_Y = limiteMinZ_Y;
            wrap.limiteMaxZ_Y = limiteMaxZ_Y;
            wrap.margen = margenBordeSpawn;
        }
    }

    private int ContarAsteroidesActivos()
    {
        Asteroide[] todos = FindObjectsByType<Asteroide>(FindObjectsSortMode.None);
        int grandes = 0;
        foreach (var a in todos)
        {
            if (a.nivelTamanio == 3) grandes++;
        }
        return grandes;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            Vector3 centro = new Vector3((limiteMinX + limiteMaxX) * 0.5f, alturaFija, (limiteMinZ_Y + limiteMaxZ_Y) * 0.5f);
            Vector3 tam = new Vector3(limiteMaxX - limiteMinX, 0.2f, limiteMaxZ_Y - limiteMinZ_Y);
            Gizmos.DrawWireCube(centro, tam);

            Gizmos.color = Color.yellow;
            Vector3 tamSpawn = new Vector3(limiteMaxX - limiteMinX + margenBordeSpawn * 2, 0.2f, limiteMaxZ_Y - limiteMinZ_Y + margenBordeSpawn * 2);
            Gizmos.DrawWireCube(centro, tamSpawn);
        }
        else
        {
            Vector3 centro = new Vector3((limiteMinX + limiteMaxX) * 0.5f, (limiteMinZ_Y + limiteMaxZ_Y) * 0.5f, alturaFija);
            Vector3 tam = new Vector3(limiteMaxX - limiteMinX, limiteMaxZ_Y - limiteMinZ_Y, 0.2f);
            Gizmos.DrawWireCube(centro, tam);

            Gizmos.color = Color.yellow;
            Vector3 tamSpawn = new Vector3(limiteMaxX - limiteMinX + margenBordeSpawn * 2, limiteMaxZ_Y - limiteMinZ_Y + margenBordeSpawn * 2, 0.2f);
            Gizmos.DrawWireCube(centro, tamSpawn);
        }
    }
}
