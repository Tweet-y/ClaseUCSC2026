using UnityEngine;

public class ControladorCanion : MonoBehaviour
{
    public float fuerzaDisparo = 24f;
    public Transform pivoteCanion;
    public GameObject bala;
    public Transform puntoCreacionBala;
    public AudioSource audioDisparo;

    public void DisparaBala()
    {
        GameObject nuevaBala = Instantiate(
            bala,
            puntoCreacionBala.position,
            puntoCreacionBala.rotation * Quaternion.Euler(90f, 0f, 0f));
        Rigidbody cuerpoBala = nuevaBala.GetComponent<Rigidbody>();
        cuerpoBala.collisionDetectionMode = CollisionDetectionMode.Continuous;

        foreach (Collider colNave in GetComponentsInParent<Collider>())
        {
            foreach (Collider colBala in nuevaBala.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(colBala, colNave);
        }

        cuerpoBala.linearVelocity = pivoteCanion.forward.normalized * fuerzaDisparo;
        if (audioDisparo != null && audioDisparo.clip != null)
        {
            audioDisparo.pitch = Random.Range(0.65f, 2f);
            audioDisparo.Play();
        }
    }
}
