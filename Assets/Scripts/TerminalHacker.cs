using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerminalHacker : MonoBehaviour
{
    [Header("Configuración UI")]
    [SerializeField] private GameObject panelIngreso;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private Button botonDestruir;
    [SerializeField] private GameObject textoAyuda; // Opcional: Un texto que diga "Presiona E"

    [Header("Objetivos a Destruir")]
    [SerializeField] private GameObject caraDelFondo;
    [SerializeField] private string passwordCorrecta = "374101A5110";

    // Bandera para saber si podemos interactuar
    private bool playerEnRango = false;

    void Start()
    {
        if (panelIngreso != null) panelIngreso.SetActive(false);
        if (textoAyuda != null) textoAyuda.SetActive(false); // Ocultar mensaje al inicio

        if (botonDestruir != null) botonDestruir.onClick.AddListener(VerificarPassword);
    }

    private void Update()
    {
        // 1. Detectar tecla E para abrir panel (solo si está en rango y el panel está cerrado)
        if (playerEnRango && Input.GetKeyDown(KeyCode.E) && !panelIngreso.activeSelf)
        {
            AbrirPanel();
        }

        // 2. Cerrar con Escape
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
            playerEnRango = true;
            if (textoAyuda != null) textoAyuda.SetActive(true); // Mostrar "Presiona E"
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerEnRango = false;
            if (textoAyuda != null) textoAyuda.SetActive(false); // Ocultar mensaje
            CerrarPanel(); // Cerrar si se aleja demasiado
        }
    }

    // --- Lógica de la UI ---
    void AbrirPanel()
    {
        panelIngreso.SetActive(true);
        if (textoAyuda != null) textoAyuda.SetActive(false); // Ocultar la ayuda mientras escribe

        // Opcional: Si quieres pausar el juego mientras escribe, descomenta esto:
        // Time.timeScale = 0f; 

        inputPassword.text = "";
        StartCoroutine(FocoEnInput());
    }

    System.Collections.IEnumerator FocoEnInput()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        inputPassword.Select();
        inputPassword.ActivateInputField();
    }

    public void CerrarPanel()
    {
        panelIngreso.SetActive(false);
        // Time.timeScale = 1f; // Si pausaste, reanuda aquí

        // Si sigue en rango, mostrar la ayuda de nuevo
        if (playerEnRango && textoAyuda != null) textoAyuda.SetActive(true);
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
            inputPassword.text = "";
        }
    }

    void EjecutarDestruccionMasiva()
    {
        // 1. Destruir la Cara del Fondo
        if (caraDelFondo != null)
        {
            Destroy(caraDelFondo);
            Debug.Log("Cara del fondo destruida.");
        }

        // 2. Drones (Usando parámetro entero "State" = 4)
        DronMovement[] drones = Object.FindObjectsByType<DronMovement>(FindObjectsSortMode.None);
        foreach (DronMovement dron in drones)
        {
            dron.enabled = false;

            // Congelar físicas
            Rigidbody2D rb = dron.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            Collider2D col = dron.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Animator anim = dron.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                // ESTA ES LA CLAVE: Usamos el parámetro entero "State" y lo ponemos en 4.
                // Esto activará la transición a "explosion" como se ve en tu Animator.
                anim.SetInteger("State", 4);
                Debug.Log("Activando estado de explosión en Dron (State=4)");
            }

            Destroy(dron.gameObject, 0.8f); // Ajusta el tiempo si la animación es más larga
        }

        // 3. Arañas (Usando trigger "trigDie")
        SpiderAI[] aranas = Object.FindObjectsByType<SpiderAI>(FindObjectsSortMode.None);
        foreach (SpiderAI arana in aranas)
        {
            arana.enabled = false;

            Rigidbody2D rb = arana.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
            Collider2D col = arana.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Animator anim = arana.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                // Usamos el Trigger que aparece en la lista de parámetros de la araña.
                anim.SetTrigger("trigDie");
                Debug.Log("Activando trigger 'trigDie' en Araña");
            }

            Destroy(arana.gameObject, 0.8f); // Ajusta el tiempo si es necesario
        }
    }
}