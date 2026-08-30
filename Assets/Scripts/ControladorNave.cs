using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ControladorNave : MonoBehaviour
{
    public float velocidadRotacion = 180f;
    public float fuerzaImpulso = 12f;
    public float velocidadMaxima = 12f;
    public float cooldownDisparo = 0.25f;
    public float alturaFija = 0f;
    public Asteroide.PlanoDeJuego plano = Asteroide.PlanoDeJuego.XZ;

    public bool estaInvulnerable;
    public float tiempoInvulnerable;
    public bool estaActiva = true;

    [Header("Iluminación de la Nave")]
    public bool emitirLuz = true;
    public Color colorLuz = new Color(0.2f, 0.85f, 1f, 1f);
    public float intensidadLuz = 3.5f;
    public float rangoLuz = 16f;
    public Light luzNave;

    Rigidbody cuerpo;
    ControladorCanion canion;
    InputAction accionRotar;
    InputAction accionImpulso;
    InputAction accionDisparo;
    float timerDisparo;
    Vector3 posicionInicial;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody>();
        canion = GetComponent<ControladorCanion>();
        if (canion == null)
            canion = GetComponentInChildren<ControladorCanion>();
        CrearAcciones();
    }

    void OnEnable()
    {
        accionRotar.Enable();
        accionImpulso.Enable();
        accionDisparo.Enable();
    }

    void OnDisable()
    {
        accionRotar.Disable();
        accionImpulso.Disable();
        accionDisparo.Disable();
    }

    void Start()
    {
        gameObject.tag = "Jugador";
        cuerpo.useGravity = false;

        Vector3 posicion = transform.position;
        posicion.y = alturaFija;
        transform.position = posicion;
        posicionInicial = transform.position;

        if (plano == Asteroide.PlanoDeJuego.XZ)
        {
            cuerpo.constraints = RigidbodyConstraints.FreezePositionY
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;
        }
        else
        {
            cuerpo.constraints = RigidbodyConstraints.FreezePositionZ
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationY;
        }

        if (GetComponent<EnvoltorioEspacio>() == null)
        {
            EnvoltorioEspacio wrap = gameObject.AddComponent<EnvoltorioEspacio>();
            wrap.plano = plano;
        }

        ConfigurarLuz();
    }

    void ConfigurarLuz()
    {
        if (!emitirLuz)
            return;

        if (luzNave == null)
            luzNave = GetComponentInChildren<Light>();

        if (luzNave == null)
        {
            GameObject objLuz = new GameObject("LuzNave");
            objLuz.transform.SetParent(transform, false);
            Vector3 offset = (plano == Asteroide.PlanoDeJuego.XZ) ? new Vector3(0f, 1.8f, 0f) : new Vector3(0f, 0f, -1.8f);
            objLuz.transform.localPosition = offset;
            luzNave = objLuz.AddComponent<Light>();
        }

        luzNave.type = LightType.Point;
        luzNave.color = colorLuz;
        luzNave.intensity = intensidadLuz;
        luzNave.range = rangoLuz;
    }

    void Update()
    {
        if (tiempoInvulnerable > 0f)
        {
            tiempoInvulnerable -= Time.deltaTime;
            if (tiempoInvulnerable <= 0f)
                estaInvulnerable = false;
        }

        if (!estaActiva)
            return;

        float rotacion = accionRotar.ReadValue<float>();
        transform.Rotate(0f, rotacion * velocidadRotacion * Time.deltaTime, 0f);

        timerDisparo += Time.deltaTime;
        if (accionDisparo.IsPressed() && timerDisparo >= cooldownDisparo)
        {
            Disparar();
            timerDisparo = 0f;
        }
    }

    void FixedUpdate()
    {
        if (!estaActiva)
            return;

        if (accionImpulso.IsPressed())
            cuerpo.AddForce(transform.forward * fuerzaImpulso, ForceMode.Acceleration);

        if (cuerpo.linearVelocity.magnitude > velocidadMaxima)
            cuerpo.linearVelocity = cuerpo.linearVelocity.normalized * velocidadMaxima;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!estaActiva || estaInvulnerable)
            return;

        Asteroide asteroide = collision.gameObject.GetComponent<Asteroide>();
        if (asteroide == null)
            return;

        asteroide.Dividir();
        if (ControladorJuego.instancia != null)
            ControladorJuego.instancia.PerderVida();
    }

    public void ActivarInvulnerabilidad(float segundos)
    {
        estaInvulnerable = true;
        tiempoInvulnerable = segundos;
    }

    public void ColocarEnCentro()
    {
        cuerpo.linearVelocity = Vector3.zero;
        cuerpo.angularVelocity = Vector3.zero;
        transform.position = posicionInicial;
        transform.rotation = Quaternion.identity;
    }

    public void ActivarNave()
    {
        estaActiva = true;
        SetNaveVisible(true);
    }

    public void DesactivarNave()
    {
        estaActiva = false;
        cuerpo.linearVelocity = Vector3.zero;
        cuerpo.angularVelocity = Vector3.zero;
        SetNaveVisible(false);
    }

    void SetNaveVisible(bool visible)
    {
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
            rend.enabled = visible;

        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = visible;

        foreach (Light l in GetComponentsInChildren<Light>())
            l.enabled = visible;
    }

    void CrearAcciones()
    {
        accionRotar = new InputAction("Rotar", InputActionType.Value);
        accionRotar.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");
        accionRotar.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        accionRotar.AddBinding("<Gamepad>/leftStick/x");

        accionImpulso = new InputAction("Impulso", InputActionType.Button);
        accionImpulso.AddBinding("<Keyboard>/upArrow");
        accionImpulso.AddBinding("<Keyboard>/w");
        accionImpulso.AddBinding("<Gamepad>/rightTrigger");

        accionDisparo = new InputAction("Disparo", InputActionType.Button);
        accionDisparo.AddBinding("<Keyboard>/space");
        accionDisparo.AddBinding("<Mouse>/leftButton");
        accionDisparo.AddBinding("<Gamepad>/buttonSouth");
    }

    void Disparar()
    {
        if (canion != null)
            canion.DisparaBala();
    }
}
