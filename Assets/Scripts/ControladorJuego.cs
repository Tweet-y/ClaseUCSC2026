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

        CrearHUDSiFalta();
        ActualizarHUD();
        if (panelFin != null)
            panelFin.SetActive(false);
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

    void ActualizarHUD()
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

    void CrearHUDSiFalta()
    {
        if (textoPuntaje != null && textoVidas != null && panelFin != null)
            return;

        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject evento = new GameObject("EventSystem");
            evento.AddComponent<UnityEngine.EventSystems.EventSystem>();
            evento.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        GameObject canvasObj = new GameObject("CanvasHUD");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        textoNombre = CrearTextoAnclado(canvasObj.transform, "textoNombre", "Jugador", new Vector2(0f, 1f), new Vector2(40f, -20f), TextAlignmentOptions.Left);
        textoPuntaje = CrearTextoAnclado(canvasObj.transform, "textoPuntaje", "Puntaje: 0", new Vector2(0f, 1f), new Vector2(40f, -70f), TextAlignmentOptions.Left);
        textoPuntaje.fontSize = 40f;
        textoVidas = CrearTextoAnclado(canvasObj.transform, "textoVidas", "Vidas: 3", new Vector2(1f, 1f), new Vector2(-40f, -20f), TextAlignmentOptions.Right);

        panelFin = new GameObject("panelFin");
        panelFin.transform.SetParent(canvasObj.transform, false);
        Image fondo = panelFin.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.65f);
        RectTransform rectPanel = panelFin.GetComponent<RectTransform>();
        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.offsetMin = Vector2.zero;
        rectPanel.offsetMax = Vector2.zero;

        CrearTexto(panelFin.transform, "textoFin", "Game Over", new Vector2(0f, 50f), TextAlignmentOptions.Center).fontSize = 56f;
        textoPuntajeFinal = CrearTexto(panelFin.transform, "textoPuntajeFinal", "Puntaje: 0", new Vector2(0f, -10f), TextAlignmentOptions.Center);
        textoPuntajeFinal.fontSize = 36f;

        GameObject botonObj = new GameObject("botonReintentar");
        botonObj.transform.SetParent(panelFin.transform, false);
        Image imagenBoton = botonObj.AddComponent<Image>();
        imagenBoton.color = new Color(0.15f, 0.55f, 0.85f, 1f);
        Button boton = botonObj.AddComponent<Button>();
        boton.onClick.AddListener(Reintentar);
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(220f, 50f);
        rectBoton.anchoredPosition = new Vector2(0f, -80f);

        TMP_Text textoBoton = CrearTexto(botonObj.transform, "textoReintentar", "Reintentar", Vector2.zero, TextAlignmentOptions.Center);
        textoBoton.fontSize = 28f;
    }

    TMP_Text CrearTexto(Transform padre, string nombre, string contenido, Vector2 posicion, TextAlignmentOptions alineacion)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        TMP_Text texto = obj.AddComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = 32f;
        texto.alignment = alineacion;
        texto.color = Color.white;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500f, 60f);
        rect.anchoredPosition = posicion;
        return texto;
    }

    TMP_Text CrearTextoAnclado(Transform padre, string nombre, string contenido, Vector2 ancla, Vector2 posicion, TextAlignmentOptions alineacion)
    {
        TMP_Text texto = CrearTexto(padre, nombre, contenido, posicion, alineacion);
        RectTransform rect = texto.GetComponent<RectTransform>();
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.pivot = ancla;
        rect.anchoredPosition = posicion;
        return texto;
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
