using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorMenu : MonoBehaviour
{
    public const string claveNombre = "nombreJugador";
    public string nombreEscenaJuego = "Asteroids_escenario";
    public AudioClip clipMusica;
    public float volumenMusica = 0.2f;
    public Material materialPanelMenu;
    TMP_InputField campoNombre;

    void Awake()
    {
        IniciarMusica();
    }

    void Start()
    {
        CrearMenu();
    }

    public void Jugar()
    {
        string nombre = "Jugador";
        if (campoNombre != null && !string.IsNullOrWhiteSpace(campoNombre.text))
            nombre = campoNombre.text.Trim();

        PlayerPrefs.SetString(claveNombre, nombre);
        PlayerPrefs.Save();
        SceneManager.LoadScene(nombreEscenaJuego);
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

    void CrearMenu()
    {
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject evento = new GameObject("EventSystem");
            evento.AddComponent<UnityEngine.EventSystems.EventSystem>();
            evento.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        GameObject canvasObj = new GameObject("CanvasMenu");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("panelMenu");
        panel.transform.SetParent(canvasObj.transform, false);
        Image fondo = panel.AddComponent<Image>();
        if (materialPanelMenu != null)
            fondo.material = materialPanelMenu;
        fondo.color = new Color(0.05f, 0.08f, 0.14f, 0.92f);
        RectTransform rectPanel = panel.GetComponent<RectTransform>();
        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.offsetMin = Vector2.zero;
        rectPanel.offsetMax = Vector2.zero;

        TMP_Text titulo = CrearTexto(panel.transform, "titulo", "ASTEROIDS", new Vector2(0f, 140f));
        titulo.fontSize = 64f;

        CrearTexto(panel.transform, "etiquetaNombre", "Escribe tu nombre", new Vector2(0f, 40f)).fontSize = 26f;
        campoNombre = CrearCampoNombre(panel.transform, new Vector2(0f, -10f));
        if (PlayerPrefs.HasKey(claveNombre))
            campoNombre.text = PlayerPrefs.GetString(claveNombre);

        GameObject botonObj = new GameObject("botonJugar");
        botonObj.transform.SetParent(panel.transform, false);
        Image imagenBoton = botonObj.AddComponent<Image>();
        imagenBoton.color = new Color(0.15f, 0.55f, 0.85f, 1f);
        Button boton = botonObj.AddComponent<Button>();
        boton.onClick.AddListener(Jugar);
        RectTransform rectBoton = botonObj.GetComponent<RectTransform>();
        rectBoton.sizeDelta = new Vector2(220f, 56f);
        rectBoton.anchoredPosition = new Vector2(0f, -90f);

        CrearTexto(botonObj.transform, "textoJugar", "Jugar", Vector2.zero).fontSize = 32f;
    }

    TMP_Text CrearTexto(Transform padre, string nombre, string contenido, Vector2 posicion)
    {
        GameObject obj = new GameObject(nombre);
        obj.transform.SetParent(padre, false);
        TMP_Text texto = obj.AddComponent<TextMeshProUGUI>();
        texto.text = contenido;
        texto.fontSize = 32f;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = Color.white;
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(500f, 80f);
        rect.anchoredPosition = posicion;
        return texto;
    }

    TMP_InputField CrearCampoNombre(Transform padre, Vector2 posicion)
    {
        GameObject campo = new GameObject("campoNombre");
        campo.transform.SetParent(padre, false);
        Image fondoCampo = campo.AddComponent<Image>();
        fondoCampo.color = Color.white;
        RectTransform rectCampo = campo.GetComponent<RectTransform>();
        rectCampo.sizeDelta = new Vector2(360f, 48f);
        rectCampo.anchoredPosition = posicion;

        GameObject area = new GameObject("areaTexto");
        area.transform.SetParent(campo.transform, false);
        RectTransform rectArea = area.AddComponent<RectTransform>();
        rectArea.anchorMin = Vector2.zero;
        rectArea.anchorMax = Vector2.one;
        rectArea.offsetMin = new Vector2(10f, 4f);
        rectArea.offsetMax = new Vector2(-10f, -4f);

        GameObject placeholderObj = new GameObject("placeholder");
        placeholderObj.transform.SetParent(area.transform, false);
        TMP_Text placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholder.text = "Nombre";
        placeholder.fontSize = 24f;
        placeholder.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform rectPlaceholder = placeholderObj.GetComponent<RectTransform>();
        rectPlaceholder.anchorMin = Vector2.zero;
        rectPlaceholder.anchorMax = Vector2.one;
        rectPlaceholder.offsetMin = Vector2.zero;
        rectPlaceholder.offsetMax = Vector2.zero;

        GameObject textoObj = new GameObject("texto");
        textoObj.transform.SetParent(area.transform, false);
        TMP_Text texto = textoObj.AddComponent<TextMeshProUGUI>();
        texto.fontSize = 24f;
        texto.color = Color.black;
        texto.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform rectTexto = textoObj.GetComponent<RectTransform>();
        rectTexto.anchorMin = Vector2.zero;
        rectTexto.anchorMax = Vector2.one;
        rectTexto.offsetMin = Vector2.zero;
        rectTexto.offsetMax = Vector2.zero;

        TMP_InputField input = campo.AddComponent<TMP_InputField>();
        input.textViewport = rectArea;
        input.textComponent = texto;
        input.placeholder = placeholder;
        input.characterLimit = 16;
        return input;
    }
}
