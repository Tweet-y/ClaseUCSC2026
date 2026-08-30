using UnityEngine;

/// <summary>
/// Controla el comportamiento de un asteroide al estilo Atari Asteroids:
/// Movimiento constante, rotación espacial 3D, división jerárquica y 3 tipos diferenciados.
/// </summary>
public class Asteroide : MonoBehaviour
{
    public enum PlanoDeJuego
    {
        XZ, // Plano horizontal 3D (X horizontal, Z vertical/profundidad)
        XY  // Plano vertical 2D / 2.5D (X horizontal, Y vertical)
    }

    public enum TipoDeAsteroide
    {
        RocosoEstandar,  // Velocidad media, comportamiento balanceado
        CrateresRapido,  // Más rápido y genera 3 fragmentos ágiles
        GigantePesado    // Lento, imponente y resistente
    }

    [Header("Tipo y Jerarquía")]
    public TipoDeAsteroide tipo = TipoDeAsteroide.RocosoEstandar;
    [Tooltip("Nivel: 3 = Grande, 2 = Mediano, 1 = Pequeño (en 1 se destruye definitivamente)")]
    [Range(1, 3)]
    public int nivelTamanio = 3;

    [Header("Físicas y Movimiento")]
    public PlanoDeJuego plano = PlanoDeJuego.XZ;
    public float velocidadMin = 35f;
    public float velocidadMax = 65f;
    public float velocidadRotacionMin = 150f;
    public float velocidadRotacionMax = 500f;

    [Header("División al ser destruido")]
    public GameObject[] prefabsFragmentos;
    public int cantidadFragmentos = 2;
    public float multiplicadorVelocidadHijo = 1.45f;
    public float anguloDispersion = 45f;

    [Header("Efectos Opcionales")]
    public GameObject prefabEfectoExplosion;
    public AudioClip sonidoExplosion;

    private Vector3 _direccionMovimiento;
    private float _velocidadActual;
    private Vector3 _ejeRotacion;
    private float _velocidadRotacion;
    private Rigidbody _rb;
    private bool _haSidoDestruido = false;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        // Asegurar colisionador convexo para mallas 3D
        MeshCollider meshCol = GetComponent<MeshCollider>();
        if (meshCol != null && !meshCol.convex)
        {
            meshCol.convex = true;
        }
        else if (GetComponent<Collider>() == null)
        {
            SphereCollider sc = gameObject.AddComponent<SphereCollider>();
            sc.radius = 1.2f;
        }
    }

    void Start()
    {
        AplicarPropiedadesSegunTipo();

        if (_direccionMovimiento == Vector3.zero)
        {
            InicializarMovimientoAleatorio();
        }

        _ejeRotacion = Random.onUnitSphere;
        _velocidadRotacion = Random.Range(velocidadRotacionMin, velocidadRotacionMax);

        if (_rb != null)
        {
            _rb.useGravity = false;
            if (plano == PlanoDeJuego.XZ)
            {
                _rb.constraints = RigidbodyConstraints.FreezePositionY;
            }
            else
            {
                _rb.constraints = RigidbodyConstraints.FreezePositionZ;
            }
        }

        // Asegurar que tenga componente de envoltorio de pantalla
        if (GetComponent<EnvoltorioEspacio>() == null)
        {
            EnvoltorioEspacio wrap = gameObject.AddComponent<EnvoltorioEspacio>();
            wrap.plano = plano;
        }
    }

    /// <summary>
    /// Configura las características especiales de los 3 tipos de asteroides
    /// </summary>
    public void AplicarPropiedadesSegunTipo()
    {
        switch (tipo)
        {
            case TipoDeAsteroide.RocosoEstandar:
                cantidadFragmentos = 2;
                multiplicadorVelocidadHijo = 1.3f;
                break;

            case TipoDeAsteroide.CrateresRapido:
                velocidadMin *= 1.4f;
                velocidadMax *= 1.5f;
                cantidadFragmentos = 3; // El rápido genera más pedazos pequeños
                multiplicadorVelocidadHijo = 1.45f;
                break;

            case TipoDeAsteroide.GigantePesado:
                velocidadMin *= 0.75f;
                velocidadMax *= 0.8f;
                cantidadFragmentos = 2;
                multiplicadorVelocidadHijo = 1.2f;
                break;
        }
    }

    public void InicializarMovimiento(Vector3 direccion, float velocidad)
    {
        if (plano == PlanoDeJuego.XZ)
        {
            direccion.y = 0f;
        }
        else
        {
            direccion.z = 0f;
        }

        _direccionMovimiento = direccion.normalized;
        _velocidadActual = velocidad;
        _ejeRotacion = Random.onUnitSphere;
        _velocidadRotacion = Random.Range(velocidadRotacionMin, velocidadRotacionMax);
    }

    public void InicializarMovimientoAleatorio()
    {
        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        if (plano == PlanoDeJuego.XZ)
        {
            _direccionMovimiento = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo)).normalized;
        }
        else
        {
            _direccionMovimiento = new Vector3(Mathf.Cos(angulo), Mathf.Sin(angulo), 0f).normalized;
        }

        _velocidadActual = Random.Range(velocidadMin, velocidadMax);
    }

    void Update()
    {
        transform.position += _direccionMovimiento * (_velocidadActual * Time.deltaTime);
        transform.Rotate(_ejeRotacion, _velocidadRotacion * Time.deltaTime, Space.Self);
    }

    void OnCollisionEnter(Collision collision)
    {
        ProcesarImpacto(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        ProcesarImpacto(other.gameObject);
    }

    private void ProcesarImpacto(GameObject objetoImpacto)
    {
        if (_haSidoDestruido) return;

        bool esBala = objetoImpacto.CompareTag("Bala") || 
                      objetoImpacto.GetComponent<ControladorBala>() != null ||
                      objetoImpacto.name.ToLower().Contains("bala");

        if (esBala)
        {
            Destroy(objetoImpacto);
            Dividir();
        }
    }

    /// <summary>
    /// Divide el asteroide en fragmentos menores o lo destruye completamente si es nivel 1.
    /// </summary>
    public void Dividir()
    {
        if (_haSidoDestruido) return;
        _haSidoDestruido = true;

        if (sonidoExplosion != null)
        {
            AudioSource.PlayClipAtPoint(sonidoExplosion, transform.position);
        }

        if (prefabEfectoExplosion != null)
        {
            Instantiate(prefabEfectoExplosion, transform.position, Quaternion.identity);
        }

        if (nivelTamanio > 1)
        {
            GenerarFragmentos();
        }

        Destroy(gameObject);
    }

    private void GenerarFragmentos()
    {
        for (int i = 0; i < cantidadFragmentos; i++)
        {
            GameObject nuevoAsteroideObj = null;

            if (prefabsFragmentos != null && prefabsFragmentos.Length > 0)
            {
                GameObject prefabElegido = prefabsFragmentos[Random.Range(0, prefabsFragmentos.Length)];
                if (prefabElegido != null)
                {
                    nuevoAsteroideObj = Instantiate(prefabElegido, transform.position, Random.rotation);
                    nuevoAsteroideObj.transform.localScale = transform.localScale * 0.55f;
                }
            }

            // Si no hay prefabs asignados, se clona y reduce automáticamente al 55% del tamaño del padre
            if (nuevoAsteroideObj == null)
            {
                nuevoAsteroideObj = Instantiate(gameObject, transform.position, Random.rotation);
                nuevoAsteroideObj.transform.localScale = transform.localScale * 0.55f;
            }

            Asteroide hijo = nuevoAsteroideObj.GetComponent<Asteroide>();
            if (hijo == null)
            {
                hijo = nuevoAsteroideObj.AddComponent<Asteroide>();
            }

            hijo.tipo = tipo;
            hijo.nivelTamanio = nivelTamanio - 1;
            hijo.plano = plano;
            hijo.prefabsFragmentos = prefabsFragmentos;
            hijo.prefabEfectoExplosion = prefabEfectoExplosion;
            hijo.sonidoExplosion = sonidoExplosion;

            // Conservar la textura / material que tenía el asteroide padre
            Renderer rendPadre = GetComponentInChildren<Renderer>();
            Renderer rendHijo = nuevoAsteroideObj.GetComponentInChildren<Renderer>();
            if (rendPadre != null && rendHijo != null && rendPadre.sharedMaterials != null && rendPadre.sharedMaterials.Length > 0)
            {
                rendHijo.sharedMaterials = rendPadre.sharedMaterials;
            }

            // Calcular dirección dispersa en abanico
            float signo = (i % 2 == 0) ? 1f : -1f;
            float variacion = (anguloDispersion * signo * ((i + 1) * 0.7f)) + Random.Range(-12f, 12f);
            Vector3 direccionHijo;

            if (plano == PlanoDeJuego.XZ)
            {
                Quaternion rot = Quaternion.Euler(0f, variacion, 0f);
                direccionHijo = (rot * _direccionMovimiento).normalized;
            }
            else
            {
                Quaternion rot = Quaternion.Euler(0f, 0f, variacion);
                direccionHijo = (rot * _direccionMovimiento).normalized;
            }

            float nuevaVelocidad = _velocidadActual * multiplicadorVelocidadHijo;
            hijo.InicializarMovimiento(direccionHijo, nuevaVelocidad);
        }
    }
}
