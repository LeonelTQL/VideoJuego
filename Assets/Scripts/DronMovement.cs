using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DronMovement : MonoBehaviour
{
    // --- Configuración ---
    [Header("Configuración de Movimiento")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float reachDistance = 1.5f;

    [Header("Configuración Búsqueda (Escaneo)")] // NUEVO
    [SerializeField] private float searchWidth = 1.5f; // Cuánto se mueve a los lados
    [SerializeField] private float searchSpeed = 2f;   // Qué tan rápido se mueve de lado a lado

    [Header("Sensores")]
    [SerializeField] private float visionRange = 8f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask playerMask;

    [Header("Tiempos de Estado")]
    [SerializeField] private float alertDuration = 2f;
    [SerializeField] private float searchDuration = 4f;
    [SerializeField] private float explosionDuration = 0.8f; // NUEVO: Tiempo que dura la anim de explosión

    // --- Referencias ---
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 lastKnownPosition;
    private Vector2 searchCenterPosition; // NUEVO: Para saber dónde hacer el "lado a lado"
    private int currentPatrolIndex = 0;

    // --- Estados ---
    // NUEVO: Agregamos 'Muerte' al final
    private enum State { Patrulla, Persecucion, Alerta, Busqueda, Muerte }
    private State currentState;
    private float stateTimer;
    private bool isDead = false; // Para bloquear lógica si ya explotó

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        currentState = State.Patrulla;
        UpdateAnimation(0);
    }

    void Update()
    {
        if (player == null || isDead) return; // Si murió, no hace nada más

        switch (currentState)
        {
            case State.Patrulla:
                PatrolBehavior();
                CheckForPlayer();
                break;

            case State.Persecucion:
                ChaseBehavior();
                if (!CanSeePlayer())
                {
                    SwitchState(State.Alerta);
                }
                break;

            case State.Alerta:
                AlertBehavior();
                break;

            case State.Busqueda:
                SearchBehavior();
                CheckForPlayer();
                break;

            case State.Muerte:
                // Aquí no hacemos nada, solo esperamos a que termine la animación
                break;
        }

        // DIBUJAR RAYO DE VISIÓN (Debug)
        if (player != null)
        {
            Debug.DrawRay(transform.position, (player.position - transform.position).normalized * visionRange, CanSeePlayer() ? Color.green : Color.red);
        }
    }

    // --- Comportamientos ---

    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;
        Transform target = patrolPoints[currentPatrolIndex];
        MoveTowards(target.position, patrolSpeed);

        if (Vector2.Distance(transform.position, target.position) < reachDistance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void ChaseBehavior()
    {
        MoveTowards(player.position, chaseSpeed);
        lastKnownPosition = player.position;
    }

    void AlertBehavior()
    {
        if (Vector2.Distance(transform.position, lastKnownPosition) > reachDistance)
        {
            MoveTowards(lastKnownPosition, chaseSpeed);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            stateTimer += Time.deltaTime;
            if (stateTimer >= alertDuration)
            {
                SwitchState(State.Busqueda);
            }
        }
    }

    void SearchBehavior()
    {
        // NUEVO: Movimiento de lado a lado (Seno)
        // Usamos Mathf.Sin para crear una oscilación suave izquierda-derecha
        float offset = Mathf.Sin(Time.time * searchSpeed) * searchWidth;

        // Calculamos la nueva posición manteniendo la altura Y original
        Vector2 targetPos = new Vector2(searchCenterPosition.x + offset, searchCenterPosition.y);

        MoveTowards(targetPos, patrolSpeed);

        stateTimer += Time.deltaTime;
        if (stateTimer >= searchDuration)
        {
            SwitchState(State.Patrulla);
        }
    }

    // --- Lógica Auxiliar ---

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;

        if (direction.x > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < -0.1f) transform.localScale = new Vector3(-1, 1, 1);
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < visionRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleMask | playerMask);
            if (hit.collider != null && hit.collider.CompareTag("Player")) return true;
        }
        return false;
    }

    void CheckForPlayer()
    {
        if (CanSeePlayer()) SwitchState(State.Persecucion);
    }

    void SwitchState(State newState)
    {
        currentState = newState;
        stateTimer = 0;

        // NUEVO: Si entramos en búsqueda, guardamos el centro para pivotar ahí
        if (newState == State.Busqueda)
        {
            searchCenterPosition = transform.position;
        }

        switch (newState)
        {
            case State.Patrulla: UpdateAnimation(0); break;
            case State.Persecucion: UpdateAnimation(1); break;
            case State.Alerta: UpdateAnimation(2); break;
            case State.Busqueda: UpdateAnimation(3); break;
            case State.Muerte: UpdateAnimation(4); break; // NUEVO ID 4
        }
    }

    void UpdateAnimation(int stateIndex)
    {
        if (animator != null) animator.SetInteger("State", stateIndex);
    }

    // --- Colisión (Muerte / Explosión) ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("¡BOOM! Contacto con el jugador.");


            ASMovement playerScript = collision.gameObject.GetComponent<ASMovement>();
            if (playerScript != null)
            {
                playerScript.Morir();
            }

            StartCoroutine(ExplodeSequence());
        }
    }


    // NUEVO: Secuencia de explosión
    IEnumerator ExplodeSequence()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero; // Frenar en seco
        rb.bodyType = RigidbodyType2D.Kinematic; // Desactivar física para que no empuje
        GetComponent<Collider2D>().enabled = false; // Desactivar colisión

        SwitchState(State.Muerte); // Activar animación de explosión

        yield return new WaitForSeconds(explosionDuration); // Esperar a que termine la anim

        // Reiniciar nivel (o matar jugador)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}