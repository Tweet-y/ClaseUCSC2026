using UnityEngine;

/// <summary>
/// Permite que un objeto espacial (asteroides, naves) que salga por un borde de la pantalla o plano
/// reaparezca instantáneamente por el lado opuesto (comportamiento de pantalla envolvente / torus de Atari).
/// </summary>
public class EnvoltorioEspacio : MonoBehaviour
{
    public enum ModoLimites
    {
        CamaraPrincipal,
        LimitesManuales,
        PlanoReferencia
    }

    [Header("Configuración de Límites")]
    public Asteroide.PlanoDeJuego plano = Asteroide.PlanoDeJuego.XZ;
    public ModoLimites modo = ModoLimites.CamaraPrincipal;

    [Header("Límites Manuales")]
    public float limiteMinX = -25f;
    public float limiteMaxX = 25f;
    public float limiteMinZ_Y = -15f;
    public float limiteMaxZ_Y = 15f;

    [Header("Referencia al Plano (Opcional)")]
    [Tooltip("Asigna aquí el GameObject del plano de tu escenario para que use sus dimensiones exactas")]
    public Transform planoEspacio;

    [Header("Margen de Salida")]
    [Tooltip("Distancia fuera de la pantalla antes de teletransportarse para que no aparezca de golpe")]
    public float margen = 30.0f;

    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
        if (_cam == null) _cam = FindFirstObjectByType<Camera>();

        ActualizarLimites();
    }

    public void ActualizarLimites()
    {
        if (modo == ModoLimites.PlanoReferencia && planoEspacio != null)
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
                }
                else
                {
                    limiteMinZ_Y = rend.bounds.min.y;
                    limiteMaxZ_Y = rend.bounds.max.y;
                }
            }
        }
        else if (modo == ModoLimites.CamaraPrincipal && _cam != null)
        {
            float distanciaCam = (plano == Asteroide.PlanoDeJuego.XZ) 
                ? Mathf.Abs(_cam.transform.position.y - transform.position.y)
                : Mathf.Abs(_cam.transform.position.z - transform.position.z);

            if (distanciaCam <= 0.1f) distanciaCam = 15f;

            if (_cam.orthographic)
            {
                float alto = _cam.orthographicSize;
                float ancho = alto * _cam.aspect;
                limiteMinX = _cam.transform.position.x - ancho;
                limiteMaxX = _cam.transform.position.x + ancho;

                if (plano == Asteroide.PlanoDeJuego.XZ)
                {
                    limiteMinZ_Y = _cam.transform.position.z - alto;
                    limiteMaxZ_Y = _cam.transform.position.z + alto;
                }
                else
                {
                    limiteMinZ_Y = _cam.transform.position.y - alto;
                    limiteMaxZ_Y = _cam.transform.position.y + alto;
                }
            }
            else
            {
                float alto = Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * distanciaCam;
                float ancho = alto * _cam.aspect;
                limiteMinX = _cam.transform.position.x - ancho;
                limiteMaxX = _cam.transform.position.x + ancho;

                if (plano == Asteroide.PlanoDeJuego.XZ)
                {
                    limiteMinZ_Y = _cam.transform.position.z - alto;
                    limiteMaxZ_Y = _cam.transform.position.z + alto;
                }
                else
                {
                    limiteMinZ_Y = _cam.transform.position.y - alto;
                    limiteMaxZ_Y = _cam.transform.position.y + alto;
                }
            }
        }
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        bool teletransportado = false;

        // Borde horizontal (Eje X)
        if (pos.x > limiteMaxX + margen)
        {
            pos.x = limiteMinX - margen + 0.1f;
            teletransportado = true;
        }
        else if (pos.x < limiteMinX - margen)
        {
            pos.x = limiteMaxX + margen - 0.1f;
            teletransportado = true;
        }

        // Borde vertical (Z en XZ, Y en XY)
        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            if (pos.z > limiteMaxZ_Y + margen)
            {
                pos.z = limiteMinZ_Y - margen + 0.1f;
                teletransportado = true;
            }
            else if (pos.z < limiteMinZ_Y - margen)
            {
                pos.z = limiteMaxZ_Y + margen - 0.1f;
                teletransportado = true;
            }
        }
        else
        {
            if (pos.y > limiteMaxZ_Y + margen)
            {
                pos.y = limiteMinZ_Y - margen + 0.1f;
                teletransportado = true;
            }
            else if (pos.y < limiteMinZ_Y - margen)
            {
                pos.y = limiteMaxZ_Y + margen - 0.1f;
                teletransportado = true;
            }
        }

        if (teletransportado)
        {
            transform.position = pos;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            Vector3 centro = new Vector3((limiteMinX + limiteMaxX) * 0.5f, transform.position.y, (limiteMinZ_Y + limiteMaxZ_Y) * 0.5f);
            Vector3 tam = new Vector3(limiteMaxX - limiteMinX + margen * 2, 0.2f, limiteMaxZ_Y - limiteMinZ_Y + margen * 2);
            Gizmos.DrawWireCube(centro, tam);
        }
        else
        {
            Vector3 centro = new Vector3((limiteMinX + limiteMaxX) * 0.5f, (limiteMinZ_Y + limiteMaxZ_Y) * 0.5f, transform.position.z);
            Vector3 tam = new Vector3(limiteMaxX - limiteMinX + margen * 2, limiteMaxZ_Y - limiteMinZ_Y + margen * 2, 0.2f);
            Gizmos.DrawWireCube(centro, tam);
        }
    }
}
