using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Necesario para limpiar la selección
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject pausaPanel;
    public GameObject opcionesPanel;
    public GameObject inicioPanel;

    [Header("Referencias de Botones (Imagen)")]
    public Image imgMusica;
    public Image imgEfectos;

    [Header("Sprites de Estado (Solo para base)")]
    public Sprite spriteActivo;   // La "I" verde
    public Sprite spriteInactivo; // La "O" gris

    private bool estaPausado = false;
    public static MenuController Instance; // Singleton para acceso fácil

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Se sale de cualquier padre para poder viajar
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ UI_Global y MenuController viajando.");
        }
        else
        {
            Debug.Log("⚠️ Destruyendo duplicado de UI.");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ActualizarTodo();
        // Ocultamos paneles al iniciar
        if (pausaPanel) pausaPanel.SetActive(false);
        if (opcionesPanel) opcionesPanel.SetActive(false);
    }

    // Dentro de MenuController.cs en el Update
    void Update()
    {
        if (SceneManager.GetActiveScene().name != "Inicio" && Input.GetKeyDown(KeyCode.Escape))
        {
            // 1. Buscamos si existe alguna nota activa en la escena
            CardItem[] notasEnEscena = FindObjectsOfType<CardItem>();
            bool hayNotaAbierta = false;

            foreach (var nota in notasEnEscena)
            {
                if (nota.IsMenuOpen())
                {
                    hayNotaAbierta = true;
                    break;
                }
            }

            // 2. Si hay una nota, nos detenemos. 
            // El CardItem se encargará de cerrarse, y este script no hará nada este frame.
            if (hayNotaAbierta)
            {
                return;
            }

            // 3. Lógica normal de pausa
            if (estaPausado) Continuar();
            else Pausar();
        }
    }
    // En MenuController.cs
    public void PlayBotonSound(AudioClip clip)
    {
        if (SFXManager.Instance != null && clip != null)
        {
            SFXManager.Instance.PlaySound(clip, 1f);
        }
    }

    // --- Lógica de Botones ---

    public void AlternarMusica()
    {
        int estado = PlayerPrefs.GetInt("MusicaActiva", 1) == 1 ? 0 : 1;
        PlayerPrefs.SetInt("MusicaActiva", estado);
        PlayerPrefs.Save();

        ActualizarTodo();
        LimpiarSeleccionVisual();
    }

    public void AlternarEfectos()
    {
        int estado = PlayerPrefs.GetInt("EfectosActivos", 1) == 1 ? 0 : 1;
        PlayerPrefs.SetInt("EfectosActivos", estado);
        PlayerPrefs.Save();

        ActualizarTodo();
        LimpiarSeleccionVisual();
    }

    private void ActualizarTodo()
    {
        bool mActiva = PlayerPrefs.GetInt("MusicaActiva", 1) == 1;
        bool eActivos = PlayerPrefs.GetInt("EfectosActivos", 1) == 1;

        if (imgMusica != null) imgMusica.sprite = mActiva ? spriteActivo : spriteInactivo;
        if (imgEfectos != null) imgEfectos.sprite = eActivos ? spriteActivo : spriteInactivo;

        // CONTROL DEL SISTEMA DE CAPAS
        if (MusicLayerSystem.Instance != null && MusicLayerSystem.Instance.capas != null)
        {
            foreach (AudioSource capa in MusicLayerSystem.Instance.capas)
            {
                if (capa != null)
                {
                    // Solo silenciamos (mute), NO tocamos el volumen aquí
                    // así respetamos cuáles capas están en 1 y cuáles en 0
                    capa.mute = !mActiva;
                }
            }
        }
    }

    private void LimpiarSeleccionVisual()
    {
        // Esto evita que el botón se quede con el "Selected Sprite" (el bug visual)
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // --- Navegación ---
    public void SalirDelJuego() => Application.Quit();
    public void IniciarJuego() => SceneManager.LoadScene("Clase");
    public void Cerrar()
    {
        // 1. Limpieza total de objetos persistentes (DontDestroyOnLoad)
        LimpiarTodoAntesDeSalir();

        // 2. Cargar la escena de Inicio
        SceneManager.LoadScene("Inicio");
    }

    private void LimpiarTodoAntesDeSalir()
    {
        // Buscamos todos los GameObjects raíz en el juego
        GameObject[] objetos = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in objetos)
        {
            // Destruimos todo para que la escena de "Inicio" cargue limpia
            // El 'gameObject' (el botón/canvas) se destruirá junto con el resto
            Destroy(obj);
        }
    }

    public void AbrirOpciones()
    {
        ActualizarTodo(); // Nos aseguramos de que al abrir estén bien los iconos
        if (pausaPanel) pausaPanel.SetActive(false);
        if (inicioPanel) inicioPanel.SetActive(false);
        opcionesPanel.SetActive(true);
    }

    public void VolverDeOpciones()
    {
        opcionesPanel.SetActive(false);
        if (SceneManager.GetActiveScene().name == "Inicio") inicioPanel.SetActive(true);
        else pausaPanel.SetActive(true);
    }

    public void Pausar() { estaPausado = true; Time.timeScale = 0f; pausaPanel.SetActive(true); }
    public void Continuar() { estaPausado = false; Time.timeScale = 1f; pausaPanel.SetActive(false); }
}