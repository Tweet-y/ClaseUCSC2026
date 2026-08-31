using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    [Tooltip("Ajusta el tamaño general de todos los asteroides (aumentado para visibilidad desde la cámara)")]
    [Range(1f, 50f)]
    public float escalaGeneral = 10.0f;
    [Tooltip("Multiplicador global para acelerar o frenar todos los asteroides (ajustable en vivo)")]
    [Range(0.5f, 10f)]
    public float multiplicadorVelocidadGlobal = 2.0f;

    [Header("Variedad de Texturas y Materiales")]
    [Tooltip("Colección de materiales que se asignarán de forma aleatoria a los asteroides generados")]
    public Material[] materialesVariados;

    [Header("Sonido y VFX de Explosión de Asteroides")]
    public AudioClip clipExplosion;
    public GameObject prefabEfectoExplosion;
    [Range(0f, 1f)]
    public float volumenExplosion = 0.9f;
    public float pitchMinExplosion = 0.7f;
    public float pitchMaxExplosion = 1.4f;
    public bool modularPitchPorTamanio = true;

    [Header("3 Tipos de Asteroides (Modelos Large)")]
    public TipoAsteroideConfig tipo1_Large01 = new TipoAsteroideConfig
    {
        nombre = "1. Asteroide Grande 01 (Estándar)",
        tipo = Asteroide.TipoDeAsteroide.RocosoEstandar,
        escalaRelativa = 7.0f,
        velocidadMin = 35.0f,
        velocidadMax = 55.0f,
        probabilidad = 4
    };

    public TipoAsteroideConfig tipo2_Large02 = new TipoAsteroideConfig
    {
        nombre = "2. Asteroide Grande 02 (Angular/Rápido)",
        tipo = Asteroide.TipoDeAsteroide.CrateresRapido,
        escalaRelativa = 6.0f,
        velocidadMin = 50.0f,
        velocidadMax = 75.0f,
        probabilidad = 4
    };

    public TipoAsteroideConfig tipo3_LargeHoles = new TipoAsteroideConfig
    {
        nombre = "3. Asteroide Grande Agujeros/Pesado",
        tipo = Asteroide.TipoDeAsteroide.GigantePesado,
        escalaRelativa = 8.0f,
        velocidadMin = 25.0f,
        velocidadMax = 40.0f,
        probabilidad = 3
    };

    [Header("Configuración de Escenario")]
    public Asteroide.PlanoDeJuego plano = Asteroide.PlanoDeJuego.XZ;
    [Tooltip("Objeto plano del escenario. Si está vacío se detecta automáticamente")]
    public Transform planoEspacio;

    [Header("Límites del Mapa (Calculados desde la Cámara)")]
    public float limiteMinX = -80f;
    public float limiteMaxX = 80f;
    public float limiteMinZ_Y = -45f;
    public float limiteMaxZ_Y = 45f;
    public float alturaFija = 0f;
    [Tooltip("Margen justo fuera del marco de la pantalla donde nacen los asteroides")]
    public float margenBordeSpawn = 8.0f;

    [Header("Frecuencia y Cantidad")]
    [Tooltip("Cantidad de asteroides que entran desde los bordes al iniciar")]
    public int oleadaInicial = 3;
    public bool generacionContinua = true;
    public float intervaloSpawn = 4.0f;
    [Tooltip("Límite absoluto de asteroides en pantalla contando grandes, medianos y pequeños")]
    public int maxAsteroidesTotales = 8;
    [Tooltip("Límite de asteroides grandes que pueden coexistir")]
    public int maxAsteroidesGrandes = 3;

    private float _temporizador = 0f;
    private Camera _cam;

    void Reset()
    {
        AutoCargarPrefabs();
        AutoDetectarPlano();
        AutoCargarMaterialesPlanetas();
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
        if (materialesVariados == null || materialesVariados.Length == 0)
        {
            AutoCargarMaterialesPlanetas();
        }
    }

    void Awake()
    {
        AutoCargarPrefabs();
        AutoDetectarPlano();
        if (materialesVariados == null || materialesVariados.Length == 0)
        {
            AutoCargarMaterialesPlanetas();
        }
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
            int total = ContarAsteroidesTotales();
            int grandes = ContarAsteroidesGrandes();

            // Solo genera nuevos si no se ha alcanzado el límite total ni el de grandes
            if (total < maxAsteroidesTotales && grandes < maxAsteroidesGrandes)
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

        // Cargar materiales de planetas automáticamente si la lista está vacía
        if (materialesVariados == null || materialesVariados.Length == 0)
        {
            AutoCargarMaterialesPlanetas();
        }

        if (clipExplosion == null)
        {
            clipExplosion = AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/Ultimate Sound FX Bundle/Sci FI Sounds Pro/Sci Fi Grenade/Sci Fi Grenade 1.wav");
        }

        if (prefabEfectoExplosion == null)
        {
            prefabEfectoExplosion = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/PolygonSciFiSpace/Prefabs/FX/FX_Explosion.prefab");
        }
#endif
    }

    /// <summary>
    /// Busca y carga todos los materiales coloridos de la carpeta Planet_Materials
    /// </summary>
    [ContextMenu("Cargar Materiales de Planet_Materials")]
    public void AutoCargarMaterialesPlanetas()
    {
#if UNITY_EDITOR
        List<Material> listaMats = new List<Material>();
        string[] carpetasBusqueda = new[] { "Assets/PolygonSciFiSpace/Materials/Planet_Materials" };
        string[] guids = AssetDatabase.FindAssets("t:Material", carpetasBusqueda);

        foreach (var guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            string rutaMin = ruta.ToLower();

            // Omitir anillos transparentes o máscaras
            if (rutaMin.Contains("ring") || rutaMin.Contains("cloud") || rutaMin.Contains("mask"))
                continue;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (mat != null && !listaMats.Contains(mat))
            {
                listaMats.Add(mat);
            }
        }

        if (listaMats.Count > 0)
        {
            materialesVariados = listaMats.ToArray();
            EditorUtility.SetDirty(this);
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
        if (_cam == null)
        {
            _cam = Camera.main;
            if (_cam == null) _cam = FindFirstObjectByType<Camera>();
        }

        // 1. Proyectar la vista de la cámara sobre el plano de juego
        if (_cam != null)
        {
            if (plano == Asteroide.PlanoDeJuego.XZ)
            {
                Plane suelo = new Plane(Vector3.up, new Vector3(0f, alturaFija, 0f));
                Ray r00 = _cam.ViewportPointToRay(new Vector3(0f, 0f, 0f));
                Ray r10 = _cam.ViewportPointToRay(new Vector3(1f, 0f, 0f));
                Ray r01 = _cam.ViewportPointToRay(new Vector3(0f, 1f, 0f));
                Ray r11 = _cam.ViewportPointToRay(new Vector3(1f, 1f, 0f));

                float d00, d10, d01, d11;
                if (suelo.Raycast(r00, out d00) && suelo.Raycast(r10, out d10) &&
                    suelo.Raycast(r01, out d01) && suelo.Raycast(r11, out d11))
                {
                    Vector3 p00 = r00.GetPoint(d00);
                    Vector3 p10 = r10.GetPoint(d10);
                    Vector3 p01 = r01.GetPoint(d01);
                    Vector3 p11 = r11.GetPoint(d11);

                    limiteMinX = Mathf.Min(p00.x, p10.x, p01.x, p11.x);
                    limiteMaxX = Mathf.Max(p00.x, p10.x, p01.x, p11.x);
                    limiteMinZ_Y = Mathf.Min(p00.z, p10.z, p01.z, p11.z);
                    limiteMaxZ_Y = Mathf.Max(p00.z, p10.z, p01.z, p11.z);
                    return;
                }
            }
            else // XY
            {
                Plane planoXY = new Plane(Vector3.forward, new Vector3(0f, 0f, alturaFija));
                Ray r00 = _cam.ViewportPointToRay(new Vector3(0f, 0f, 0f));
                Ray r11 = _cam.ViewportPointToRay(new Vector3(1f, 1f, 0f));

                float d00, d11;
                if (planoXY.Raycast(r00, out d00) && planoXY.Raycast(r11, out d11))
                {
                    Vector3 p00 = r00.GetPoint(d00);
                    Vector3 p11 = r11.GetPoint(d11);

                    limiteMinX = Mathf.Min(p00.x, p11.x);
                    limiteMaxX = Mathf.Max(p00.x, p11.x);
                    limiteMinZ_Y = Mathf.Min(p00.y, p11.y);
                    limiteMaxZ_Y = Mathf.Max(p00.y, p11.y);
                    return;
                }
            }
        }

        // 2. Si no hay cámara disponible, usar límites del plano
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
            }
        }
    }

    /// <summary>
    /// Genera un asteroide exactamente en uno de los 4 bordes exteriores del marco de la cámara
    /// y lo envía con trayectoria hacia el área central de juego visible.
    /// </summary>
    public void SpawnAsteroideEnBorde()
    {
        CalcularLimitesDelMapa();

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

        // Punto objetivo dentro de la zona central visible de la pantalla (40% central)
        float centroX = (limiteMinX + limiteMaxX) * 0.5f;
        float centroZY = (limiteMinZ_Y + limiteMaxZ_Y) * 0.5f;
        float radioX = (limiteMaxX - limiteMinX) * 0.35f;
        float radioZY = (limiteMaxZ_Y - limiteMinZ_Y) * 0.35f;

        Vector3 puntoObjetivo;
        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            puntoObjetivo = new Vector3(
                Random.Range(centroX - radioX, centroX + radioX),
                alturaFija,
                Random.Range(centroZY - radioZY, centroZY + radioZY)
            );
        }
        else
        {
            puntoObjetivo = new Vector3(
                Random.Range(centroX - radioX, centroX + radioX),
                Random.Range(centroZY - radioZY, centroZY + radioZY),
                alturaFija
            );
        }

        Vector3 direccion = (puntoObjetivo - posicionSpawn).normalized;

        // Instanciar
        GameObject nuevoAsteroide = Instantiate(config.prefabBase, posicionSpawn, Random.rotation);

        // Aplicar escala adecuada
        float factorEscala = Mathf.Max(1f, escalaGeneral) * Mathf.Max(1f, config.escalaRelativa);
        nuevoAsteroide.transform.localScale = Vector3.one * factorEscala;

        // Configurar componente Asteroide
        Asteroide ast = nuevoAsteroide.GetComponent<Asteroide>();
        if (ast == null)
        {
            ast = nuevoAsteroide.AddComponent<Asteroide>();
        }

        nuevoAsteroide.tag = "Asteroide";
        ast.tipo = config.tipo;
        ast.plano = plano;
        ast.nivelTamanio = 3; // Grande inicial
        ast.velocidadMin = config.velocidadMin;
        ast.velocidadMax = config.velocidadMax;
        ast.sonidoExplosion = clipExplosion;
        ast.prefabEfectoExplosion = prefabEfectoExplosion;
        ast.volumenExplosion = volumenExplosion;
        ast.pitchMin = pitchMinExplosion;
        ast.pitchMax = pitchMaxExplosion;
        ast.modularPitchPorTamanio = modularPitchPorTamanio;
        ast.AplicarPropiedadesSegunTipo();

        float vel = Random.Range(config.velocidadMin, config.velocidadMax) * multiplicadorVelocidadGlobal;
        ast.InicializarMovimiento(direccion, vel);

        // Asignar textura/material variado aleatorio
        if (materialesVariados != null && materialesVariados.Length > 0)
        {
            Renderer rend = nuevoAsteroide.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                Material matElegido = materialesVariados[Random.Range(0, materialesVariados.Length)];
                if (matElegido != null)
                {
                    rend.sharedMaterial = matElegido;
                }
            }
        }

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

            // Asignar material de planeta variado
            if (materialesVariados != null && materialesVariados.Length > 0)
            {
                Renderer rend = ast.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    rend.sharedMaterial = materialesVariados[Random.Range(0, materialesVariados.Length)];
                }
            }

            ast.plano = plano;
            ast.sonidoExplosion = clipExplosion;
            ast.prefabEfectoExplosion = prefabEfectoExplosion;
            ast.volumenExplosion = volumenExplosion;
            ast.pitchMin = pitchMinExplosion;
            ast.pitchMax = pitchMaxExplosion;
            ast.modularPitchPorTamanio = modularPitchPorTamanio;

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

    public int ContarAsteroidesTotales()
    {
        Asteroide[] todos = FindObjectsByType<Asteroide>(FindObjectsSortMode.None);
        int total = 0;
        foreach (var a in todos)
        {
            if (a != null && a.gameObject.activeInHierarchy) total++;
        }
        return total;
    }

    public int ContarAsteroidesGrandes()
    {
        Asteroide[] todos = FindObjectsByType<Asteroide>(FindObjectsSortMode.None);
        int grandes = 0;
        foreach (var a in todos)
        {
            if (a != null && a.gameObject.activeInHierarchy && a.nivelTamanio >= 3) grandes++;
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
