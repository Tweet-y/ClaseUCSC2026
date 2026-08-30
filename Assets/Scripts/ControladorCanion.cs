using UnityEngine;

public class ControladorCanion : MonoBehaviour
{
    public float fuerzaDisparo = 24f;
    public Transform pivoteCanion;
    public GameObject bala;
    public Transform puntoCreacionBala;
    public AudioSource audioDisparo;

    [Header("Alineación de Proyectiles")]
    [Tooltip("Fuerza la altura Y de la bala para impactar siempre los asteroides sin importar qué tan alta esté la nave")]
    public bool forzarAlturaBala = true;
    public float alturaFijaBala = 0f;

    public void DisparaBala()
    {
        Vector3 posSpawn = puntoCreacionBala != null ? puntoCreacionBala.position : transform.position;
        Quaternion rotSpawn = puntoCreacionBala != null ? puntoCreacionBala.rotation : transform.rotation;

        if (forzarAlturaBala)
            posSpawn.y = alturaFijaBala;

        GameObject nuevaBala = Instantiate(
            bala,
            posSpawn,
            rotSpawn * Quaternion.Euler(90f, 0f, 0f));

        Rigidbody cuerpoBala = nuevaBala.GetComponent<Rigidbody>();
        if (cuerpoBala != null)
        {
            cuerpoBala.collisionDetectionMode = CollisionDetectionMode.Continuous;

            foreach (Collider colNave in GetComponentsInParent<Collider>())
            {
                foreach (Collider colBala in nuevaBala.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(colBala, colNave);
            }

            Vector3 dir = (pivoteCanion != null ? pivoteCanion.forward : transform.forward);
            if (forzarAlturaBala)
                dir.y = 0f;

            cuerpoBala.linearVelocity = dir.normalized * fuerzaDisparo;
        }

        if (audioDisparo != null && audioDisparo.clip != null)
        {
            audioDisparo.pitch = Random.Range(0.65f, 2f);
            audioDisparo.Play();
        }
    }
}

