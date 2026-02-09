using UnityEngine;
using UnityEngine.SceneManagement; // IMPORTANTE: Necesario para reiniciar la escena
using System.Collections;

public class SpiderAI : MonoBehaviour
{
    // Definimos los estados posibles de nuestra IA
    public enum SpiderState { Patrolling, Chasing, Searching, Attacking }

    [Header("--- Estado Actual (Solo lectura) ---")]
    public SpiderState currentState;

    [Header("--- Configuraci�n de Movimiento ---")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public Transform[] patrolPoints; // Arrastra aqu� puntos vac�os de la escena
    private int currentPatrolIndex = 0;

    [Header("--- Configuraci�n de Sensores (IA) ---")]
    public float visionRange = 5f; // Qu� tan lejos ve
    // Attack range sigue siendo �til para saber cu�ndo *intentar* atacar, 
    // pero el da�o real ahora lo hace el Collider en OnCollisionEnter2D.
    public float attackRange = 0.8f;
    public LayerMask playerLayer;   // Para que solo detecte al jugador
    public LayerMask obstacleLayer; // Para que las paredes bloqueen la visi�n

    [Header("--- Configuraci�n de B�squeda (Nerf) ---")]
    public float searchDuration = 3f; // Tiempo que se queda buscando en el lugar
    private Vector3 lastKnownPlayerPos;
    private float searchTimer;

    [Header("--- Configuraci�n de Muerte/Ataque ---")]
    // Cu�nto dura la animaci�n "spiderAttack" antes de reiniciar el nivel
    public float attackAnimDuration = 1f;
    private bool hasKilled = false; // Candado para que no mate dos veces

    [Header("--- Referencias ---")]
    public Animator animator;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Collider2D myCollider; // Referencia al collider propio
    private bool isFacingRight = true; // Ajusta seg�n tu sprite original

    [Header("Audio")]
    [SerializeField] private AudioSource spiderAudioSource;
    [SerializeField] private AudioClip sonidoPasoSpider; // Clip: Paso araña
    [SerializeField] private float intervaloPasosSpider = 0.3f;
    private float stepTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();

        // Busca al jugador autom�ticamente por su Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // Estado inicial
        SwitchState(SpiderState.Patrolling);
    }

    void Update()
    {
        // Si no hay jugador, o si la ara�a ya mat� a alguien, dejamos de ejecutar la IA
        if (playerTransform == null || hasKilled) return;

        // --- EL CEREBRO DE LA IA ---
        // Decide qu� estado tomar basado en la situaci�n
        DecideNextState();

        // --- EJECUTAR COMPORTAMIENTO ---
        // Realiza la acci�n del estado actual
        ExecuteCurrentState();

        if (rb.linearVelocity.magnitude > 0.1f && (currentState == SpiderState.Patrolling || currentState == SpiderState.Chasing))
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0)
            {
                PlaySpiderStep();
                stepTimer = intervaloPasosSpider;
            }
        }
    }


    void PlaySpiderStep()
    {
        // VALIDACIÓN
        if (SFXManager.Instance != null && !SFXManager.Instance.CanPlaySFX) return;

        if (spiderAudioSource != null && sonidoPasoSpider != null)
        {
            spiderAudioSource.pitch = Random.Range(0.7f, 1.3f);
            spiderAudioSource.PlayOneShot(sonidoPasoSpider);
        }
    }
    // ========================================================
    //              L�GICA DE DECISI�N (EL CEREBRO)
    // ========================================================
    void DecideNextState()
    {
        // Si ya estamos atacando, no decidimos nada m�s, dejamos que termine la animaci�n
        if (currentState == SpiderState.Attacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool canSeePlayer = CanSeePlayer(distanceToPlayer);

        switch (currentState)
        {
            case SpiderState.Patrolling:
                if (canSeePlayer) SwitchState(SpiderState.Chasing);
                break;

            case SpiderState.Chasing:
                if (!canSeePlayer)
                {
                    // NERF: Si te pierde de vista, guarda la posici�n y va a buscar
                    lastKnownPlayerPos = playerTransform.position;
                    SwitchState(SpiderState.Searching);
                }
                // NOTA: Quitamos la l�gica de ataque por distancia aqu�, 
                // ahora el ataque real ocurre por contacto f�sico en OnCollisionEnter2D
                break;

            case SpiderState.Searching:
                // Si mientras busca te vuelve a ver, vuelve a perseguir
                if (canSeePlayer) SwitchState(SpiderState.Chasing);
                break;
        }
    }

    // ========================================================
    //              EJECUCI�N DE ESTADOS
    // ========================================================
    void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case SpiderState.Patrolling:
                PatrolBehavior();
                break;
            case SpiderState.Chasing:
                ChaseBehavior();
                break;
            case SpiderState.Searching:
                SearchBehavior();
                break;
                // El caso Attacking ya no necesita hacer nada en el Update,
                // se maneja en la colisi�n.
        }
    }

    // --- Comportamiento: Patrullar ---
    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        MoveTowards(targetPoint.position, patrolSpeed);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.2f)
        {
            // Pasa al siguiente punto
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    // --- Comportamiento: Perseguir ---
    void ChaseBehavior()
    {
        MoveTowards(playerTransform.position, chaseSpeed);
    }

    // --- Comportamiento: Buscar (El "Algoritmo nerfeado") ---
    void SearchBehavior()
    {
        // 1. Ir a la �ltima posici�n conocida
        if (Vector2.Distance(transform.position, lastKnownPlayerPos) > 0.5f)
        {
            animator.SetTrigger("trigWalk"); // Asegurar que camine hacia el punto
            MoveTowards(lastKnownPlayerPos, chaseSpeed);
        }
        else
        {
            // 2. Una vez ah�, detenerse y reproducir animaci�n de b�squeda
            rb.linearVelocity = Vector2.zero;
            // Solo disparar la animaci�n una vez al llegar
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("spiderWalk"))
            {
                animator.SetTrigger("trigSearch");
            }

            // 3. Esperar un tiempo
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0)
            {
                // Si no encontr� nada, volver a patrullar
                SwitchState(SpiderState.Patrolling);
            }
        }
    }

    // --- Funci�n para cambiar de estado y manejar animaciones ---
    void SwitchState(SpiderState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case SpiderState.Patrolling:
            case SpiderState.Chasing:
                animator.ResetTrigger("trigSearch");
                animator.ResetTrigger("trigAttack");
                animator.SetTrigger("trigWalk");
                break;
            case SpiderState.Searching:
                searchTimer = searchDuration; // Reiniciar timer
                break;
            case SpiderState.Attacking:
                // Este estado ahora solo se llama desde OnCollisionEnter2D
                rb.linearVelocity = Vector2.zero; // Frenar para atacar
                animator.SetTrigger("trigAttack");
                break;
        }
    }

    // ========================================================
    //          SECCI�N DE MUERTE (Basado en el Dron)
    // ========================================================

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si ya mat�, ignoramos colisiones
        if (hasKilled) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("�La ara�a atrap� al jugador!");

            // 1. Intentar obtener el script del jugador y matarlo
            // IMPORTANTE: Aseg�rate que tu jugador tiene un script llamado 'ASMovement' con un m�todo p�blico 'Morir()'
            ASMovement playerScript = collision.gameObject.GetComponent<ASMovement>();
            if (playerScript != null)
            {
                playerScript.Morir();
            }
            else
            {
                Debug.LogWarning("No se encontr� el script 'ASMovement' en el jugador.");
            }

            // 2. Iniciar la secuencia de fin de juego de la ara�a
            StartCoroutine(KillSequence());
        }
    }

    // Secuencia de ataque final y reinicio
    IEnumerator KillSequence()
    {
        hasKilled = true; // Bloqueamos la IA en el Update

        // Cambiamos visualmente al estado de ataque
        currentState = SpiderState.Attacking;
        animator.SetTrigger("trigAttack");

        // Frenamos en seco y desactivamos f�sicas para evitar empujones raros
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;
        if (myCollider != null) myCollider.enabled = false;

        // Esperamos a que termine la animaci�n de ataque
        yield return new WaitForSeconds(attackAnimDuration);

        // Reiniciar nivel
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ========================================================
    //              HERRAMIENTAS AUXILIARES
    // ========================================================

    // Moverse y girar el sprite
    void MoveTowards(Vector3 target, float speed)
    {
        Vector2 direction = (target - transform.position).normalized;
        rb.linearVelocity = direction * speed;

        // Girar sprite (asumiendo que el sprite mira a la derecha por defecto)
        if (direction.x > 0 && !isFacingRight) Flip();
        else if (direction.x < 0 && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Sistema de visi�n (Raycast)
    bool CanSeePlayer(float distance)
    {
        if (distance > visionRange) return false;

        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

        // Lanzamos un rayo hacia el jugador. Si choca con una pared antes, no lo ve.
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, visionRange, playerLayer | obstacleLayer);

        if (hit.collider != null)
        {
            // Si lo primero que toca es al jugador, entonces s� lo ve
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    // Dibujar gizmos en el editor para ver los rangos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange); // Rango de visi�n

        if (currentState == SpiderState.Searching)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(lastKnownPlayerPos, 0.5f); // D�nde est� buscando
        }
    }
}