using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena
using System.Collections;

public class ASMovement : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 5f;

    // Componentes
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movementInput;

    // Estado
    private bool isDead = false;

    public static ASMovement instance;

    // --- AGREGA ESTE BLOQUE AWAKE ---
    void Awake()
    {
        if (instance == null)
        {
            // Si no hay jugador registrado, YO soy el original.
            instance = this;
            DontDestroyOnLoad(gameObject); // Me hago inmortal
        }
        else
        {
            // Si YA existe un 'instance', entonces YO soy una copia sobrante de la nueva escena.
            // ¡Me autodestruyo para no molestar!
            Destroy(gameObject);
        }
    }

    // --- 1. DETECCIÓN DE CAMBIO DE ESCENA (El arreglo del Respawn) ---
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cargar una escena nueva, "revivimos" al jugador
        isDead = false;
        Time.timeScale = 1f; // Aseguramos que el juego no esté pausado

        // Reiniciamos el animator para quitar la animación de muerte
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // Reseteamos físicas
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Nos movemos al punto de inicio
        PosicionarJugador();
        Debug.Log("✅ Jugador revivido y posicionado en la nueva escena.");
    }
    // ---------------------------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Posicionamos también al iniciar el juego por primera vez
        PosicionarJugador();
    }

    void Update()
    {
        // Si está muerto, no dejamos que se mueva ni haga nada
        if (isDead) return;

        // Input de movimiento
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(moveX, moveY).normalized;

        // Girar sprite izquierda/derecha
        if (moveX < 0.0f) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (moveX > 0.0f) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        // Animaciones
        if (animator != null)
        {
            bool isMovingUp = moveY > 0.01f;
            bool isMovingDown = moveY < -0.01f;
            bool isMovingSide = Mathf.Abs(moveX) > 0.01f;

            animator.SetBool("walkUp", isMovingUp);
            animator.SetBool("walkDown", isMovingDown);
            animator.SetBool("walk", isMovingSide && !isMovingUp && !isMovingDown);
        }
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = Vector2.zero; // Frenado total si muere
            return;
        }

        // Movimiento físico
        rb.linearVelocity = movementInput * moveSpeed;
    }

    // --- ESTA ES LA FUNCIÓN QUE TE FALTABA (La que piden la Araña y el Dron) ---
    public void Morir()
    {
        if (isDead) return; // Si ya murió, no muere dos veces

        isDead = true;
        Debug.Log("💀 El jugador ha muerto.");

        if (animator != null) animator.SetTrigger("muerte");

        // Iniciamos la rutina para reiniciar el juego
        StartCoroutine(ReiniciarEscenaRoutine());
    }

    IEnumerator ReiniciarEscenaRoutine()
    {
        yield return new WaitForSeconds(2f); // Espera 2 segundos de animación
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recarga la escena
    }

    void PosicionarJugador()
    {
        GameObject puntoInicio = GameObject.Find("PuntoInicio");
        if (puntoInicio != null)
        {
            transform.position = puntoInicio.transform.position;
        }
    }
}