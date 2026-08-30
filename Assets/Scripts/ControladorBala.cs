using UnityEngine;

public class ControladorBala : MonoBehaviour
{
    public float tiempoVida = 2f;
    public float timer = 0f;
    public bool forzarAltura = true;
    public float alturaFija = 0f;

    void Start()
    {
        Rigidbody cuerpo = GetComponent<Rigidbody>();
        if (cuerpo != null)
        {
            cuerpo.useGravity = false;
            cuerpo.constraints = RigidbodyConstraints.FreezePositionY
                | RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;
        }

        if (forzarAltura)
        {
            Vector3 pos = transform.position;
            pos.y = alturaFija;
            transform.position = pos;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= tiempoVida)
            Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Asteroide>() == null)
            return;

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Asteroide>() == null)
            return;

        Destroy(gameObject);
    }
}

