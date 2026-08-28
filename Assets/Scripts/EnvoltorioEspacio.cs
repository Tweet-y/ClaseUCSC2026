using UnityEngine;

/// <summary>
/// Permite que un objeto espacial (asteroides, naves) que salga por un borde de la pantalla
/// reaparezca instantáneamente por el lado opuesto (comportamiento de pantalla envolvente / torus de Atari Asteroids).
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

    [Header("Límites Actuales de Pantalla")]
    public float limiteMinX = -80f;
    public float limiteMaxX = 80f;
    public float limiteMinZ_Y = -45f;
    public float limiteMaxZ_Y = 45f;

    [Header("Referencia al Plano (Opcional)")]
    [Tooltip("Asigna aquí el GameObject del plano si prefieres límites basados en él en lugar de la cámara")]
    public Transform planoEspacio;

    [Header("Margen de Salida")]
    [Tooltip("Distancia fuera de la pantalla antes de teletransportarse para que la transición sea natural")]
    public float margen = 8.0f;

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
                return;
            }
        }

        if (modo == ModoLimites.CamaraPrincipal && _cam != null)
        {
            if (plano == Asteroide.PlanoDeJuego.XZ)
            {
                Plane suelo = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));
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
                }
            }
            else // XY
            {
                Plane planoXY = new Plane(Vector3.forward, new Vector3(0f, 0f, transform.position.z));
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
                }
            }
        }
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        bool teletransportado = false;
        float anchoTotal = (limiteMaxX - limiteMinX) + (margen * 2f);
        float altoTotal = (limiteMaxZ_Y - limiteMinZ_Y) + (margen * 2f);

        // Borde horizontal (Eje X)
        if (pos.x > limiteMaxX + margen)
        {
            pos.x -= anchoTotal;
            teletransportado = true;
        }
        else if (pos.x < limiteMinX - margen)
        {
            pos.x += anchoTotal;
            teletransportado = true;
        }

        // Borde vertical (Eje Z en XZ, o Eje Y en XY)
        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            if (pos.z > limiteMaxZ_Y + margen)
            {
                pos.z -= altoTotal;
                teletransportado = true;
            }
            else if (pos.z < limiteMinZ_Y - margen)
            {
                pos.z += altoTotal;
                teletransportado = true;
            }
        }
        else
        {
            if (pos.y > limiteMaxZ_Y + margen)
            {
                pos.y -= altoTotal;
                teletransportado = true;
            }
            else if (pos.y < limiteMinZ_Y - margen)
            {
                pos.y += altoTotal;
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
