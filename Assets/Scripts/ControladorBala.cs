using UnityEngine;

public class ControladorBala : MonoBehaviour
{
    public float tiempoVida = 2f;
    public float timer = 0f;

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
}
