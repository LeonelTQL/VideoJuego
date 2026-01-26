using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CambioEscena : MonoBehaviour
{
    [Header("Configuración")]
    // Aquí escribiremos "clase2" en el Inspector
    public string nombreEscenaDestino;
    public float duracionTransicion = 1f;

    [Header("Referencias Visuales")]
    // Si aún no tienes la animación de humo lista, puedes dejar esto vacío en el inspector
    public Animator transicionAnimator;

    private bool yaActivado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica que sea el Player quien toca la puerta
        if (collision.CompareTag("Player") && !yaActivado)
        {
            yaActivado = true;
            StartCoroutine(SecuenciaCambioEscena());
        }
    }

    IEnumerator SecuenciaCambioEscena()
    {
        Debug.Log("Cargando escena: " + nombreEscenaDestino);

        // 1. Activar animación (solo si existe el animator)
        if (transicionAnimator != null)
        {
            transicionAnimator.SetTrigger("activar");
        }

        // 2. Esperar
        yield return new WaitForSeconds(duracionTransicion);

        // 3. Cargar la nueva escena
        SceneManager.LoadScene(nombreEscenaDestino);
    }
}