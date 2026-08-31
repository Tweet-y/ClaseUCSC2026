using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia;
    public int vidas = 3;
    public int puntaje = 0;
    public string nombreJugador = "Jugador";
    public TMP_Text textoNombre;
    public TMP_Text textoPuntaje;
    public TMP_Text textoVidas;
    public TMP_Text textoPuntajeFinal;
    public GameObject panelFin;
    public Button botonReintentar;
    public ControladorNave nave;
    public AudioClip clipMusica;
    public float volumenMusica = 0.2f;
    public AudioClip clipReaparicion;
    public float volumenReaparicion = 0.8f;
    public AudioClip clipPerderVida;
    public float volumenPerderVida = 0.8f;
    AudioSource audioEfectos;

    void Awake()
    {
        instancia = this;
        IniciarMusica();
        IniciarAudioEfectos();
    }

    void Start()
    {
        if (nave == null)
            nave = FindFirstObjectByType<ControladorNave>();

        nombreJugador = PlayerPrefs.GetString(ControladorMenu.claveNombre, "Jugador");
        if (string.IsNullOrWhiteSpace(nombreJugador))
            nombreJugador = "Jugador";

        ActualizarHUD();
        if (panelFin != null)
            panelFin.SetActive(false);
        if (botonReintentar != null)
        {
            botonReintentar.onClick.RemoveListener(Reintentar);
            botonReintentar.onClick.AddListener(Reintentar);
        }
    }

    public void SumarPuntos(int cantidad)
    {
        puntaje += cantidad;
        ActualizarHUD();
    }

    public void PerderVida()
    {
        if (vidas <= 0)
            return;

        vidas--;
        ActualizarHUD();
        ReproducirSonidoPerderVida();

        if (vidas <= 0)
        {
            FinDelJuego();
            return;
        }

        if (nave != null)
            nave.DesactivarNave();
        Invoke(nameof(Reaparecer), 1f);
    }

    public void ReproducirSonidoPerderVida()
    {
        if (clipPerderVida != null)
        {
            if (audioEfectos == null)
                IniciarAudioEfectos();
            audioEfectos.PlayOneShot(clipPerderVida, volumenPerderVida);
        }
    }

    public void Reaparecer()
    {
        if (nave == null)
            return;

        nave.ColocarEnCentro();
        nave.ActivarNave();
        nave.ActivarInvulnerabilidad(2f);
        ReproducirSonidoReaparicion();
    }

    public void ReproducirSonidoReaparicion()
    {
        if (clipReaparicion != null)
        {
            if (audioEfectos == null)
                IniciarAudioEfectos();
            audioEfectos.PlayOneShot(clipReaparicion, volumenReaparicion);
        }
    }

    public void FinDelJuego()
    {
        if (nave != null)
            nave.DesactivarNave();
        if (textoPuntajeFinal != null)
            textoPuntajeFinal.text = nombreJugador + "  -  Puntaje: " + puntaje;
        if (panelFin != null)
            panelFin.SetActive(true);
    }

    public void Reintentar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ActualizarHUD()
    {
        if (textoNombre != null)
            textoNombre.text = nombreJugador;
        if (textoPuntaje != null)
            textoPuntaje.text = "Puntaje: " + puntaje;
        if (textoVidas != null)
            textoVidas.text = "Vidas: " + vidas;
    }

    void IniciarAudioEfectos()
    {
        audioEfectos = gameObject.AddComponent<AudioSource>();
        audioEfectos.playOnAwake = false;
        audioEfectos.loop = false;
    }

    void IniciarMusica()
    {
        AudioSource musica = GetComponent<AudioSource>();
        if (musica == null)
            musica = gameObject.AddComponent<AudioSource>();

        if (clipMusica == null)
            clipMusica = CargarClip("Assets/Ultimate Sound FX Bundle/Sci FI Sounds Pro/Sci Fi Beam/Sci Fi Beam 1.wav");
        if (clipMusica != null)
            musica.clip = clipMusica;

        musica.loop = true;
        musica.playOnAwake = false;
        musica.volume = volumenMusica;
        if (musica.clip != null && !musica.isPlaying)
            musica.Play();
    }

    AudioClip CargarClip(string ruta)
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(ruta);
#else
        return null;
#endif
    }
}
