using UnityEngine;

public class ObjetoInteractuable : MonoBehaviour
{
    [Header("Configuración de UI")]
    public GameObject cartelInteractuar; // Arrastra aquí tu Prefab de Canvas

    private bool jugadorCerca = false;

    void Start()
    {
        if (cartelInteractuar != null)
            cartelInteractuar.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Jugador detectado cerca de: " + gameObject.name); // Esto saldrá en la consola
            jugadorCerca = true;
            if (cartelInteractuar != null)
                cartelInteractuar.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (cartelInteractuar != null) cartelInteractuar.SetActive(false);
        }
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            Interactuar();
        }
    }

    void Interactuar()
    {
        Debug.Log("Interactuando con: " + gameObject.name);
        // Aquí puedes disparar eventos, como abrir una nota o activar el panel.
    }
}