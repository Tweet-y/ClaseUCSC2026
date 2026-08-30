using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ControladorNave : MonoBehaviour
{
    public float velocidadRotacion = 180f;
    public float fuerzaImpulso = 12f;
    public float velocidadMaxima = 12f;
    public float cooldownDisparo = 0.25f;
    public Asteroide.PlanoDeJuego plano = Asteroide.PlanoDeJuego.XZ;

    Rigidbody cuerpo;
    ControladorCanion canion;
    InputAction accionRotar;
    InputAction accionImpulso;
    InputAction accionDisparo;
    float timerDisparo;

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
        cuerpo.useGravity = false;
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
    }

    void Update()
    {
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
        if (accionImpulso.IsPressed())
            cuerpo.AddForce(transform.forward * fuerzaImpulso, ForceMode.Acceleration);

        if (cuerpo.linearVelocity.magnitude > velocidadMaxima)
            cuerpo.linearVelocity = cuerpo.linearVelocity.normalized * velocidadMaxima;
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
