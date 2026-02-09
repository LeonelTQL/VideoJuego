using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ASMovement : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // Arrastra el AudioSource del jugador aquí
    [SerializeField] private AudioClip sonidoPaso;    // El clip de "Pasos hero"
    [SerializeField] private AudioClip sonidoMuerte;  // El clip de "Sonido de muerte"
    [SerializeField] private float intervaloPasos = 0.4f; // Tiempo entre cada paso

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movementInput;
    private bool isDead = false;
    private float pasoTimer; // Temporizador para los pasos

    public static ASMovement instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isDead = false;
        Time.timeScale = 1f;
        if (animator != null) { animator.Rebind(); animator.Update(0f); }
        if (rb != null) rb.linearVelocity = Vector2.zero;
        PosicionarJugador();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Si no asignaste el AudioSource en el inspector, lo buscamos
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        PosicionarJugador();
    }

    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(moveX, moveY).normalized;

        if (moveX < 0.0f) transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        else if (moveX > 0.0f) transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        ActualizarAnimaciones(moveX, moveY);
        ManejarSonidoPasos();
    }

    // --- LÓGICA DE PASOS ---
    void ManejarSonidoPasos()
    {
        // Si nos estamos moviendo físicamente
        if (movementInput.magnitude > 0.1f)
        {
            pasoTimer -= Time.deltaTime;
            if (pasoTimer <= 0)
            {
                SonarPaso();
                pasoTimer = intervaloPasos;
            }
        }
        else
        {
            pasoTimer = 0; // Reiniciar para que el primer paso suene al instante al caminar
        }
    }

    // En ASMovement.cs, modifica SonarPaso y Morir
    void SonarPaso()
    {
        // VALIDACIÓN: Solo suena si el SFXManager dice que está activo
        if (SFXManager.Instance != null && !SFXManager.Instance.CanPlaySFX) return;

        if (audioSource != null && sonidoPaso != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(sonidoPaso);
        }
    }

    void ActualizarAnimaciones(float moveX, float moveY)
    {
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
        if (isDead) { rb.linearVelocity = Vector2.zero; return; }
        rb.linearVelocity = movementInput * moveSpeed;
    }


    public void Morir()
    {
        if (isDead) return;
        isDead = true;

        // VALIDACIÓN: Solo suena si está activo
        bool sfxActivo = PlayerPrefs.GetInt("EfectosActivos", 1) == 1;
        if (sfxActivo && audioSource != null && sonidoMuerte != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(sonidoMuerte);
        }

        if (animator != null) animator.SetTrigger("muerte");
        StartCoroutine(ReiniciarEscenaRoutine());
    }

    IEnumerator ReiniciarEscenaRoutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void PosicionarJugador()
    {
        GameObject puntoInicio = GameObject.Find("PuntoInicio");
        if (puntoInicio != null) transform.position = puntoInicio.transform.position;
    }
}