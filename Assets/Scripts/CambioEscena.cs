using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CambioEscena : MonoBehaviour
{
    [Header("Configuración")]
    public string nombreEscenaDestino;
    public float duracionTransicion = 1f;

    [Header("Música Adaptativa")]
    public int indiceCapaAActivar;

    [Header("Referencias Visuales")]
    public Animator transicionAnimator;

    private bool yaActivado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !yaActivado)
        {
            yaActivado = true;
            StartCoroutine(SecuenciaCambioEscena());
        }
    }

    IEnumerator SecuenciaCambioEscena()
    {
        // LLAMADA CORREGIDA AQUÍ
        if (MusicLayerSystem.Instance != null)
        {
            MusicLayerSystem.Instance.ActivarSiguienteNivel(indiceCapaAActivar);
        }

        if (transicionAnimator != null)
        {
            transicionAnimator.SetTrigger("activar");
        }

        yield return new WaitForSeconds(duracionTransicion);
        SceneManager.LoadScene(nombreEscenaDestino);
    }
}