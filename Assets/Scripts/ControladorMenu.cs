using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControladorMenu : MonoBehaviour
{
    public const string claveNombre = "nombreJugador";
    public AudioClip clipMusica;
    public float volumenMusica = 0.2f;
    public Material materialPanelMenu;
    public GameObject panelMenu;
    public TMP_InputField campoNombre;
    public Button botonJugar;

    void Awake()
    {
        IniciarMusica();
    }

    void Start()
    {
        if (panelMenu == null || campoNombre == null)
            return;

        if (PlayerPrefs.HasKey(claveNombre))
            campoNombre.text = PlayerPrefs.GetString(claveNombre);

        if (botonJugar != null)
        {
            botonJugar.onClick.RemoveListener(Jugar);
            botonJugar.onClick.AddListener(Jugar);
        }
    }

    public void Jugar()
    {
        string nombre = "Jugador";
        if (campoNombre != null && !string.IsNullOrWhiteSpace(campoNombre.text))
            nombre = campoNombre.text.Trim();

        PlayerPrefs.SetString(claveNombre, nombre);
        PlayerPrefs.Save();
        if (panelMenu != null)
            panelMenu.SetActive(false);

        if (ControladorJuego.instancia != null)
        {
            ControladorJuego.instancia.nombreJugador = nombre;
            ControladorJuego.instancia.ActualizarHUD();
        }
    }

    void IniciarMusica()
    {
        AudioSource musica = GetComponent<AudioSource>();
        if (musica == null)
            musica = gameObject.AddComponent<AudioSource>();

        if (clipMusica == null)
        {
#if UNITY_EDITOR
            clipMusica = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(
                "Assets/Ultimate Sound FX Bundle/Sci FI Sounds Pro/Sci Fi Beam/Sci Fi Beam 1.wav");
#endif
        }
        if (materialPanelMenu == null)
        {
#if UNITY_EDITOR
            materialPanelMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Materials/MaterialPanelMenu.mat");
#endif
        }
        if (clipMusica != null)
            musica.clip = clipMusica;

        musica.loop = true;
        musica.playOnAwake = false;
        musica.volume = volumenMusica;
        if (musica.clip != null && !musica.isPlaying)
            musica.Play();
    }
}
