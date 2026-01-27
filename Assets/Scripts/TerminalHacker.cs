using UnityEngine;
using UnityEngine.UI;
using TMPro; // Necesario para TextMeshPro. Si usas texto legacy, quita esto.

public class TerminalHacker : MonoBehaviour
{
    [Header("Configuración UI")]
    [SerializeField] private GameObject panelIngreso; // Arrastra aquí tu Panel
    [SerializeField] private TMP_InputField inputPassword; // Arrastra tu InputField
    [SerializeField] private Button botonDestruir; // Arrastra el botón de confirmar

    [Header("Objetivos a Destruir")]
    [SerializeField] private GameObject caraDelFondo; // Arrastra el objeto de la cara gigante (EDNA)
    [SerializeField] private string passwordCorrecta = "374101A5110";

    private bool playerEnRango = false;

    void Start()
    {
        // Asegurarnos que el panel empiece cerrado
        if (panelIngreso != null) panelIngreso.SetActive(false);

        // Configurar el botón para que llame a la función al hacer click
        if (botonDestruir != null) botonDestruir.onClick.AddListener(VerificarPassword);
    }

    private void Update()
    {
        // Opcional: Cerrar con Escape
        if (panelIngreso.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CerrarPanel();
        }
    }

    // --- Detección del Jugador ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AbrirPanel();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CerrarPanel();
        }
    }

    // --- Lógica de la UI ---
    void AbrirPanel()
    {
        panelIngreso.SetActive(true);
        // Time.timeScale = 0f;  <--- PONLE ESTAS DOS BARRAS PARA ANULAR LA PAUSA

        inputPassword.text = "";
        StartCoroutine(FocoEnInput());
    }

    System.Collections.IEnumerator FocoEnInput()
    {
        // Esperamos un frame real (ignora el timeScale)
        yield return new WaitForSecondsRealtime(0.1f);

        // Forzamos el foco en el input
        inputPassword.Select();
        inputPassword.ActivateInputField();
    }

    public void CerrarPanel()
    {
        panelIngreso.SetActive(false);
        Time.timeScale = 1f; // Reanudar juego
    }

    // --- Lógica de Destrucción ---
    void VerificarPassword()
    {
        if (inputPassword.text == passwordCorrecta)
        {
            Debug.Log("¡Acceso Concedido! Iniciando protocolo de destrucción...");
            EjecutarDestruccionMasiva();
            CerrarPanel();
        }
        else
        {
            Debug.Log("Contraseña Incorrecta");
            inputPassword.text = ""; // Borrar para intentar de nuevo
            // Aquí podrías poner un sonido de error
        }
    }

    void EjecutarDestruccionMasiva()
    {
        // 1. Destruir la Cara del Fondo (EDNA) para liberar el paso
        if (caraDelFondo != null)
        {
            Destroy(caraDelFondo);
            Debug.Log("Cara del fondo destruida. Paso libre.");
        }

        // 2. Buscar y Destruir todos los Drones
        // Usamos FindObjectsByType que es más eficiente en Unity 6
        DronMovement[] drones = Object.FindObjectsByType<DronMovement>(FindObjectsSortMode.None);
        foreach (DronMovement dron in drones)
        {
            // Opcional: Instanciar una explosión aquí antes de destruir
            Destroy(dron.gameObject);
        }

        // 3. Buscar y Destruir todas las Arañas
        SpiderAI[] aranas = Object.FindObjectsByType<SpiderAI>(FindObjectsSortMode.None);
        foreach (SpiderAI arana in aranas)
        {
            Destroy(arana.gameObject);
        }

        Debug.Log($"Se eliminaron {drones.Length} drones y {aranas.Length} arañas.");
    }
}