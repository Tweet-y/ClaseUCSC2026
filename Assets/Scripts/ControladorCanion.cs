using UnityEngine;

public class ControladorCanion : MonoBehaviour
{
    public float fuerzaDisparo = 10f;
    public Transform pivoteCanion;
    public GameObject bala;
    public Transform puntoCreacionBala;
    [Range(0.1f, 5f)]
    public float frecuencia = 1f;
    public float timer = 0f;
    public AudioSource audioDisparo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("EJE Z DEL CANION: " + pivoteCanion.forward);

        if(timer >= frecuencia)
        {
            DisparaBala();
            timer = 0f;
        }
        else
            timer += Time.deltaTime;
            

    }

    public void DisparaBala()
    {
        GameObject nuevaBala = GameObject.Instantiate(bala, puntoCreacionBala.position, puntoCreacionBala.rotation);
        // Dispara una nueva bala en el eje z que apunta el canion
        nuevaBala.GetComponent<Rigidbody>().AddForce(pivoteCanion.forward * fuerzaDisparo);
        audioDisparo.pitch = Random.Range(0.65f,2f);
        audioDisparo.Play();
    }
}
